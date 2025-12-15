import argparse
import base64
import fnmatch
import io
import json
import os
import random
import re
import time
import urllib.error
import urllib.request
import urllib.parse
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, Iterable, List, Optional, Tuple

from PIL import Image, ImageChops, ImageFilter


@dataclass(frozen=True)
class PromptItem:
    dir: str
    filename: str
    positive: str
    negative: str
    wants_transparent: bool


def load_json(path: Path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def parse_prompt_sheet(path: Path) -> List[PromptItem]:
    text = path.read_text(encoding="utf-8-sig")

    # Split by section headers.
    # Example: ## UI_Sprites/mint_square_normal.png (552x566)
    # Example: ## BoosterPurchase/btn_close.png (110x108)
    headers = list(re.finditer(r"^## ([^/]+)/([^ ]+)", text, flags=re.M))
    if not headers:
        raise RuntimeError(f"No sections found in prompt sheet: {path}")

    items: List[PromptItem] = []
    for i, m in enumerate(headers):
        start = m.start()
        end = headers[i + 1].start() if i + 1 < len(headers) else len(text)
        section = text[start:end]
        dir_name = m.group(1).strip()
        filename = m.group(2).strip()

        def extract_block(title: str) -> str:
            # Matches:
            # **Positive prompt**
            # ~~~
            # ...
            # ~~~
            pat = re.compile(
                rf"^\*\*{re.escape(title)}\*\*\s*\n+^~~~\s*\n(.*?)\n^~~~\s*$",
                flags=re.S | re.M,
            )
            mm = pat.search(section)
            if not mm:
                raise RuntimeError(f"Missing '{title}' block for {filename} in {path}")
            return mm.group(1).strip()

        positive = extract_block("Positive prompt")
        negative = extract_block("Negative prompt")
        wants_transparent = "transparent background" in positive.lower()

        items.append(
            PromptItem(
                dir=dir_name,
                filename=filename,
                positive=positive,
                negative=negative,
                wants_transparent=wants_transparent,
            )
        )

    return items


def choose_gen_size(target_w: int, target_h: int) -> Tuple[int, int]:
    # Keep it conservative to reduce cost and post-process artifacts:
    # - default to square
    # - only switch to wide/tall for extreme aspect ratios
    r = target_w / max(1, target_h)
    if r >= 1.8:
        return (1792, 1024)
    if r <= 0.55:
        return (1024, 1792)
    return (1024, 1024)


def normalize_size_arg(s: str) -> str:
    s = (s or "").strip().lower().replace("×", "x")
    if not s:
        return ""
    if not re.fullmatch(r"\d+x\d+", s):
        raise ValueError(f"Invalid size: {s} (expected like 512x512)")
    return s


def http_json_post(url: str, payload: dict, api_key: str, timeout: int = 120) -> dict:
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(url, data=data, method="POST")
    req.add_header("Content-Type", "application/json")
    req.add_header("Authorization", f"Bearer {api_key}")
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        body = resp.read().decode("utf-8")
        return json.loads(body)


def redact_secrets(text: str) -> str:
    if not text:
        return text
    # Common OpenAI-style key pattern; keeps logs safe if upstream echoes it.
    return re.sub(r"\bsk-[A-Za-z0-9]{8,}\b", "sk-***", text)


def openai_generate_b64(
    *,
    images_url: str,
    api_base: str,
    api_key: str,
    model: str,
    prompt: str,
    size: str,
    quality: Optional[str],
    style: Optional[str],
    response_format: Optional[str],
    timeout: int,
    max_retries: int,
    base_backoff: float,
) -> Tuple[bytes, Optional[str]]:
    payload: Dict[str, object] = {
        "model": model,
        "prompt": prompt,
        "size": size,
        "n": 1,
    }
    # Only include these when explicitly provided; different models may reject unknown fields.
    if quality:
        payload["quality"] = quality
    if style:
        payload["style"] = style
    if response_format:
        payload["response_format"] = response_format

    last_err: Optional[Exception] = None
    for attempt in range(1, max_retries + 1):
        try:
            rsp = http_json_post(images_url, payload, api_key=api_key, timeout=timeout)
            data = rsp.get("data") or []
            if not data:
                raise RuntimeError(f"Empty response data: {rsp}")

            first = data[0]
            b64 = first.get("b64_json")
            if not b64:
                img_url = first.get("url")
                if not img_url:
                    raise RuntimeError(f"Missing b64_json/url in response: {rsp}")
                if isinstance(img_url, str) and not img_url.lower().startswith(("http://", "https://")):
                    img_url = urllib.parse.urljoin(api_base.rstrip("/") + "/", img_url.lstrip("/"))
                with urllib.request.urlopen(str(img_url), timeout=timeout) as img_resp:
                    return img_resp.read(), first.get("revised_prompt")

            revised = first.get("revised_prompt")
            return base64.b64decode(b64), revised
        except urllib.error.HTTPError as e:
            last_err = e
            # Try to parse error body for helpful message.
            try:
                body = e.read().decode("utf-8")
            except Exception:
                body = ""
            body = redact_secrets(body)

            # Some OpenAI-compatible proxies don't support response_format; auto-fallback to default.
            if (
                e.code == 400
                and "response_format" in payload
                and ("Unknown parameter" in body or '"param":"response_format"' in body)
            ):
                print("[warn] API rejected response_format; falling back to default response format")
                payload.pop("response_format", None)
                if attempt < max_retries:
                    continue

            # Rate limit/backoff.
            retryable = e.code in (408, 429, 500, 502, 503, 504)
            if not retryable or attempt == max_retries:
                raise RuntimeError(f"Images API HTTPError {e.code}: {body}") from e

            sleep_s = base_backoff * (2 ** (attempt - 1)) + random.random() * 0.25
            print(f"[retry] HTTP {e.code} attempt {attempt}/{max_retries} sleep {sleep_s:.2f}s")
            time.sleep(sleep_s)
        except Exception as e:
            last_err = e
            if attempt == max_retries:
                raise
            sleep_s = base_backoff * (2 ** (attempt - 1)) + random.random() * 0.25
            print(f"[retry] error attempt {attempt}/{max_retries} sleep {sleep_s:.2f}s: {e}")
            time.sleep(sleep_s)

    raise RuntimeError(f"Failed after retries: {last_err}")


def sample_edge_bg_rgb(img: Image.Image, step: int = 8) -> Tuple[int, int, int]:
    rgb = img.convert("RGB")
    w, h = rgb.size
    pts: List[Tuple[int, int]] = []
    for x in range(0, w, step):
        pts.append((x, 0))
        pts.append((x, h - 1))
    for y in range(0, h, step):
        pts.append((0, y))
        pts.append((w - 1, y))

    colors = [rgb.getpixel(p) for p in pts]
    # Median per channel for robustness.
    rs = sorted(c[0] for c in colors)
    gs = sorted(c[1] for c in colors)
    bs = sorted(c[2] for c in colors)
    mid = len(colors) // 2
    return (rs[mid], gs[mid], bs[mid])


def floodfill_bg_mask(
    img: Image.Image,
    bg_rgb: Tuple[int, int, int],
    tol: int,
    max_side: int = 512,
) -> Image.Image:
    # Returns L mask: 255 for background (edge-connected), 0 elsewhere.
    rgb = img.convert("RGB")
    w, h = rgb.size
    scale = max(1, max(w, h) // max_side)
    sw, sh = max(1, w // scale), max(1, h // scale)
    small = rgb.resize((sw, sh), resample=Image.Resampling.BILINEAR)

    tol2 = int(tol) ** 2 * 3
    pix = list(small.getdata())
    cand = [False] * (sw * sh)
    for i, (r, g, b) in enumerate(pix):
        dr = r - bg_rgb[0]
        dg = g - bg_rgb[1]
        db = b - bg_rgb[2]
        cand[i] = (dr * dr + dg * dg + db * db) <= tol2

    from collections import deque

    visited = [False] * (sw * sh)
    q = deque()

    def push(x: int, y: int):
        idx = y * sw + x
        if not cand[idx] or visited[idx]:
            return
        visited[idx] = True
        q.append((x, y))

    for x in range(sw):
        push(x, 0)
        push(x, sh - 1)
    for y in range(sh):
        push(0, y)
        push(sw - 1, y)

    while q:
        x, y = q.popleft()
        if x > 0:
            push(x - 1, y)
        if x + 1 < sw:
            push(x + 1, y)
        if y > 0:
            push(x, y - 1)
        if y + 1 < sh:
            push(x, y + 1)

    mask_small = Image.new("L", (sw, sh), 0)
    mask_small.putdata([255 if v else 0 for v in visited])
    return mask_small.resize((w, h), resample=Image.Resampling.NEAREST)


def ensure_transparent(img: Image.Image, *, tol: int) -> Image.Image:
    rgba = img.convert("RGBA")
    a = rgba.getchannel("A")
    if a.getextrema()[0] < 250:
        return rgba

    bg = sample_edge_bg_rgb(rgba)
    bg_mask = floodfill_bg_mask(rgba, bg, tol=tol)
    alpha = ImageChops.invert(bg_mask)
    alpha = alpha.filter(ImageFilter.GaussianBlur(radius=1))
    rgba.putalpha(alpha)
    return rgba


def alpha_bbox(img: Image.Image, alpha_threshold: int = 8) -> Optional[Tuple[int, int, int, int]]:
    a = img.convert("RGBA").getchannel("A")
    bbox = a.point([0 if i < alpha_threshold else 255 for i in range(256)]).getbbox()
    return bbox


def fit_to_reference(
    gen: Image.Image,
    *,
    ref_path: Path,
    target_size: Tuple[int, int],
    wants_transparent: bool,
) -> Image.Image:
    target_w, target_h = target_size
    ref = Image.open(ref_path).convert("RGBA")
    ref_bbox = alpha_bbox(ref) or (0, 0, target_w, target_h)
    ref_cx = (ref_bbox[0] + ref_bbox[2]) / 2.0
    ref_cy = (ref_bbox[1] + ref_bbox[3]) / 2.0
    ref_bw = max(1, ref_bbox[2] - ref_bbox[0])
    ref_bh = max(1, ref_bbox[3] - ref_bbox[1])

    img = gen.convert("RGBA")
    gen_bbox = alpha_bbox(img) or (0, 0, img.size[0], img.size[1])
    gen_bw = max(1, gen_bbox[2] - gen_bbox[0])
    gen_bh = max(1, gen_bbox[3] - gen_bbox[1])

    base_scale = min(ref_bw / gen_bw, ref_bh / gen_bh) * 0.985
    base_scale = max(0.15, min(3.0, base_scale))

    # Avoid tiny "shadow clipping" by verifying with a more sensitive alpha threshold and shrinking if needed.
    last_canvas: Optional[Image.Image] = None
    for shrink_i in range(5):
        scale = base_scale * (0.97**shrink_i)
        new_w = max(1, int(round(img.size[0] * scale)))
        new_h = max(1, int(round(img.size[1] * scale)))
        resized = img.resize((new_w, new_h), resample=Image.Resampling.LANCZOS)

        gen_bbox2 = alpha_bbox(resized, alpha_threshold=8) or (0, 0, resized.size[0], resized.size[1])
        gen_bbox2_outer = alpha_bbox(resized, alpha_threshold=1) or gen_bbox2
        gen_cx = (gen_bbox2[0] + gen_bbox2[2]) / 2.0
        gen_cy = (gen_bbox2[1] + gen_bbox2[3]) / 2.0

        paste_x = int(round(ref_cx - gen_cx))
        paste_y = int(round(ref_cy - gen_cy))

        # Clamp using outer bbox so faint pixels don't get clipped.
        bx0 = paste_x + gen_bbox2_outer[0]
        by0 = paste_y + gen_bbox2_outer[1]
        bx1 = paste_x + gen_bbox2_outer[2]
        by1 = paste_y + gen_bbox2_outer[3]

        if bx0 < 0:
            paste_x += -bx0
        if by0 < 0:
            paste_y += -by0
        if bx1 > target_w:
            paste_x -= bx1 - target_w
        if by1 > target_h:
            paste_y -= by1 - target_h

        bx0 = paste_x + gen_bbox2_outer[0]
        by0 = paste_y + gen_bbox2_outer[1]
        bx1 = paste_x + gen_bbox2_outer[2]
        by1 = paste_y + gen_bbox2_outer[3]
        touches = bx0 < 1 or by0 < 1 or bx1 > (target_w - 1) or by1 > (target_h - 1)

        canvas = Image.new("RGBA", (target_w, target_h), (0, 0, 0, 0))
        canvas.alpha_composite(resized, dest=(paste_x, paste_y))
        last_canvas = canvas
        if not touches:
            break

    if not wants_transparent:
        # Fill remaining with opaque background sampled from edges.
        bg = sample_edge_bg_rgb(gen)
        opaque = Image.new("RGBA", (target_w, target_h), (bg[0], bg[1], bg[2], 255))
        opaque.alpha_composite(last_canvas or Image.new("RGBA", (target_w, target_h), (0, 0, 0, 0)))
        canvas = opaque
    else:
        canvas = last_canvas or Image.new("RGBA", (target_w, target_h), (0, 0, 0, 0))

    return canvas


def main() -> int:
    ap = argparse.ArgumentParser(description="Batch-generate UI PNGs via OpenAI Images API from the prompt sheet.")
    ap.add_argument(
        "--api-base",
        default="https://api.openai.com/v1",
        help="API base URL, e.g. https://api.openai.com/v1 or a proxy like https://api.apiyi.com/v1",
    )
    ap.add_argument("--model", default="gpt-image-1", help="e.g. gpt-image-1 or dall-e-3")
    ap.add_argument("--quality", default="", help="Optional: depends on model/proxy (e.g. low|standard|hd).")
    ap.add_argument("--style", default="", help="dall-e-3 only: vivid|natural")
    ap.add_argument(
        "--response-format",
        default="",
        help="Optional: url|b64_json. Using url can reduce proxy 'completion tokens' by avoiding base64 in the JSON response.",
    )
    ap.add_argument(
        "--gen-size",
        default="",
        help="Override generation size, e.g. 256x256, 512x512, 1024x1024 (proxy/model dependent).",
    )
    ap.add_argument("--api-key-env", default="OPENAI_API_KEY")
    ap.add_argument(
        "--api-key-file",
        default="",
        help="Optional path to a file that contains the API key (recommended to avoid pasting into commands).",
    )

    ap.add_argument("--prompt-sheet", default="Tools/UiRestyleV05/_prompt_sheet_hud_v05.md")
    ap.add_argument("--sizes-json", default="Tools/UiRestyleV05/_sizes_ui_sprites.json")
    ap.add_argument("--kit-root", default="Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti")
    ap.add_argument("--resources-root", default="Assets/Resources", help="Project Resources root (for BoosterPurchase, setting_page_assets, etc).")
    ap.add_argument("--out-dir", default="Tools/UiRestyleV05/_openai_output")

    ap.add_argument("--only", action="append", default=[], help="glob filter(s), e.g. 'mint_square_*' (repeatable)")
    ap.add_argument("--limit", type=int, default=0, help="limit number of generated files (0 = no limit)")
    ap.add_argument("--overwrite", action="store_true", default=False, help="overwrite existing output PNGs (default: skip)")
    ap.add_argument("--dry-run", action="store_true", default=False)

    ap.add_argument("--bg-tolerance", type=int, default=18, help="background remove tolerance (when transparency missing)")
    ap.add_argument("--timeout", type=int, default=180)
    ap.add_argument("--max-retries", type=int, default=5)
    ap.add_argument("--base-backoff", type=float, default=1.2)
    ap.add_argument("--sleep", type=float, default=0.2, help="sleep between requests")

    args = ap.parse_args()
    rejected_gen_sizes: set[str] = set()

    def load_api_key() -> str:
        key = os.environ.get(args.api_key_env, "").strip()
        if key:
            return key

        key_file = args.api_key_file.strip()
        if not key_file:
            key_file = str((Path(__file__).resolve().parent / "_secrets" / "openai_api_key.txt"))

        p = Path(key_file)
        if not p.exists():
            return ""
        return p.read_text(encoding="utf-8").strip()

    api_key = load_api_key()
    if not api_key and not args.dry_run:
        raise SystemExit(
            f"Missing API key: set env var {args.api_key_env} or create key file at "
            f"Tools/UiRestyleV05/_secrets/openai_api_key.txt (not committed)."
        )

    prompt_sheet = Path(args.prompt_sheet)
    sizes_json = Path(args.sizes_json)
    kit_root = Path(args.kit_root)
    resources_root = Path(args.resources_root)
    out_root = Path(args.out_dir)

    items = parse_prompt_sheet(prompt_sheet)
    sizes = load_json(sizes_json) if sizes_json.exists() else []
    size_map: Dict[str, Tuple[int, int]] = {}
    for it in sizes:
        if it.get("dir") == "UI_Sprites":
            size_map[it["name"]] = (int(it["w"]), int(it["h"]))

    def want_file(dir_name: str, name: str) -> bool:
        if not args.only:
            return True
        rel = f"{dir_name}/{name}"
        return any(fnmatch.fnmatch(name, pat) or fnmatch.fnmatch(rel, pat) for pat in args.only)

    def resolve_reference_path(dir_name: str, name: str) -> Path:
        if dir_name in ("UI_Sprites", "World_Sprites"):
            return kit_root / dir_name / name
        if dir_name == "conveyor_belt_texture_v02_candy":
            return kit_root / dir_name / name
        if dir_name == "ResourcesRoot":
            return resources_root / name
        return resources_root / dir_name / name

    count = 0
    for item in items:
        name = item.filename
        if not want_file(item.dir, name):
            continue

        ref_path = resolve_reference_path(item.dir, name)
        if not ref_path.exists():
            print(f"[skip] missing reference PNG: {ref_path}")
            continue

        with Image.open(ref_path) as ref_img:
            target_w, target_h = ref_img.size

        # Optional sanity: warn if size map disagrees.
        if item.dir == "UI_Sprites":
            mapped = size_map.get(name)
            if mapped and mapped != (target_w, target_h):
                print(f"[warn] size json mismatch for {item.dir}/{name}: json={mapped[0]}x{mapped[1]} ref={target_w}x{target_h}")

        out_path = out_root / item.dir / name
        if not args.overwrite and out_path.exists():
            print(f"[skip] exists: {out_path}")
            continue

        gen_w, gen_h = choose_gen_size(target_w, target_h)
        gen_size = normalize_size_arg(args.gen_size) or f"{gen_w}x{gen_h}"

        prompt = item.positive.strip()
        neg = item.negative.strip()
        if neg:
            prompt = f"{prompt}\n\nAvoid: {neg}"

        if args.dry_run:
            print(f"[dry-run] {item.dir}/{name} target={target_w}x{target_h} gen={gen_size} model={args.model}")
            count += 1
        else:
            def size_candidates() -> List[str]:
                if not args.gen_size:
                    return [gen_size]
                # When overriding to a smaller size, auto-fallback to larger common sizes if the API rejects it.
                common = ["256x256", "512x512", "1024x1024"]
                first = gen_size
                return [first] + [s for s in common if s != first]

            candidates = [s for s in size_candidates() if s not in rejected_gen_sizes]
            if not candidates:
                candidates = ["1024x1024"]
            last_error: Optional[Exception] = None
            for attempt_size in candidates:
                if attempt_size != gen_size:
                    print(f"[warn] retry with gen size {attempt_size} (previous rejected)")
                print(f"[gen] {item.dir}/{name} target={target_w}x{target_h} gen={attempt_size} model={args.model}")

                api_base = str(args.api_base).strip().rstrip("/")
                images_url = f"{api_base}/images/generations"
                try:
                    img_bytes, revised = openai_generate_b64(
                        images_url=images_url,
                        api_base=api_base,
                        api_key=api_key,
                        model=args.model,
                        prompt=prompt,
                        size=attempt_size,
                        quality=args.quality or None,
                        style=args.style or None,
                        response_format=args.response_format or None,
                        timeout=args.timeout,
                        max_retries=args.max_retries,
                        base_backoff=args.base_backoff,
                    )
                    img = Image.open(io.BytesIO(img_bytes)).convert("RGBA")
                    break
                except RuntimeError as e:
                    last_error = e
                    msg = str(e)
                    invalid_size = ('"param":"size"' in msg) or ("size" in msg.lower() and "invalid" in msg.lower())
                    if invalid_size and attempt_size != candidates[-1]:
                        rejected_gen_sizes.add(attempt_size)
                        continue
                    raise
            else:
                raise last_error or RuntimeError("Image generation failed")

            if item.wants_transparent:
                img = ensure_transparent(img, tol=args.bg_tolerance)

            final_img = fit_to_reference(
                img,
                ref_path=ref_path,
                target_size=(target_w, target_h),
                wants_transparent=item.wants_transparent,
            )

            out_path.parent.mkdir(parents=True, exist_ok=True)
            final_img.save(out_path, format="PNG")
            if revised:
                (out_path.parent / f"{name}.revised_prompt.txt").write_text(revised, encoding="utf-8")

            time.sleep(max(0.0, float(args.sleep)))
            count += 1

        if args.limit and count >= args.limit:
            break

    print(f"Done. Processed: {count}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
