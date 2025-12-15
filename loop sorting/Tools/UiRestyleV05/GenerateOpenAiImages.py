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
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, Iterable, List, Optional, Tuple

from PIL import Image, ImageChops, ImageFilter


@dataclass(frozen=True)
class PromptItem:
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
    headers = list(re.finditer(r"^## UI_Sprites/([^ ]+)", text, flags=re.M))
    if not headers:
        raise RuntimeError(f"No sections found in prompt sheet: {path}")

    items: List[PromptItem] = []
    for i, m in enumerate(headers):
        start = m.start()
        end = headers[i + 1].start() if i + 1 < len(headers) else len(text)
        section = text[start:end]
        filename = m.group(1).strip()

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


def http_json_post(url: str, payload: dict, api_key: str, timeout: int = 120) -> dict:
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(url, data=data, method="POST")
    req.add_header("Content-Type", "application/json")
    req.add_header("Authorization", f"Bearer {api_key}")
    with urllib.request.urlopen(req, timeout=timeout) as resp:
        body = resp.read().decode("utf-8")
        return json.loads(body)


def openai_generate_b64(
    *,
    api_key: str,
    model: str,
    prompt: str,
    size: str,
    quality: Optional[str],
    style: Optional[str],
    timeout: int,
    max_retries: int,
    base_backoff: float,
) -> Tuple[bytes, Optional[str]]:
    url = "https://api.openai.com/v1/images/generations"

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

    last_err: Optional[Exception] = None
    for attempt in range(1, max_retries + 1):
        try:
            rsp = http_json_post(url, payload, api_key=api_key, timeout=timeout)
            data = rsp.get("data") or []
            if not data:
                raise RuntimeError(f"Empty response data: {rsp}")

            first = data[0]
            b64 = first.get("b64_json")
            if not b64:
                img_url = first.get("url")
                if not img_url:
                    raise RuntimeError(f"Missing b64_json/url in response: {rsp}")
                with urllib.request.urlopen(img_url, timeout=timeout) as img_resp:
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

            # Rate limit/backoff.
            retryable = e.code in (408, 429, 500, 502, 503, 504)
            if not retryable or attempt == max_retries:
                raise RuntimeError(f"OpenAI HTTPError {e.code}: {body}") from e

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
    canvas = Image.new("RGBA", (target_w, target_h), (0, 0, 0, 0))

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

    scale = min(ref_bw / gen_bw, ref_bh / gen_bh) * 0.985
    scale = max(0.15, min(3.0, scale))
    new_w = max(1, int(round(img.size[0] * scale)))
    new_h = max(1, int(round(img.size[1] * scale)))
    img = img.resize((new_w, new_h), resample=Image.Resampling.LANCZOS)

    gen_bbox2 = alpha_bbox(img) or (0, 0, img.size[0], img.size[1])
    gen_cx = (gen_bbox2[0] + gen_bbox2[2]) / 2.0
    gen_cy = (gen_bbox2[1] + gen_bbox2[3]) / 2.0

    paste_x = int(round(ref_cx - gen_cx))
    paste_y = int(round(ref_cy - gen_cy))

    # Clamp to keep bbox inside canvas as much as possible.
    bx0 = paste_x + gen_bbox2[0]
    by0 = paste_y + gen_bbox2[1]
    bx1 = paste_x + gen_bbox2[2]
    by1 = paste_y + gen_bbox2[3]

    if bx0 < 0:
        paste_x += -bx0
    if by0 < 0:
        paste_y += -by0
    if bx1 > target_w:
        paste_x -= bx1 - target_w
    if by1 > target_h:
        paste_y -= by1 - target_h

    canvas.alpha_composite(img, dest=(paste_x, paste_y))

    if not wants_transparent:
        # Fill remaining with opaque background sampled from edges.
        bg = sample_edge_bg_rgb(gen)
        opaque = Image.new("RGBA", (target_w, target_h), (bg[0], bg[1], bg[2], 255))
        opaque.alpha_composite(canvas)
        canvas = opaque

    return canvas


def main() -> int:
    ap = argparse.ArgumentParser(description="Batch-generate UI PNGs via OpenAI Images API from the prompt sheet.")
    ap.add_argument("--model", default="gpt-image-1", help="e.g. gpt-image-1 or dall-e-3")
    ap.add_argument("--quality", default="", help="dall-e-3 only: standard|hd")
    ap.add_argument("--style", default="", help="dall-e-3 only: vivid|natural")
    ap.add_argument("--api-key-env", default="OPENAI_API_KEY")
    ap.add_argument(
        "--api-key-file",
        default="",
        help="Optional path to a file that contains the API key (recommended to avoid pasting into commands).",
    )

    ap.add_argument("--prompt-sheet", default="Tools/UiRestyleV05/_prompt_sheet_hud_v05.md")
    ap.add_argument("--sizes-json", default="Tools/UiRestyleV05/_sizes_ui_sprites.json")
    ap.add_argument("--kit-root", default="Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti")
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
    out_root = Path(args.out_dir)

    items = parse_prompt_sheet(prompt_sheet)
    sizes = load_json(sizes_json)
    size_map: Dict[str, Tuple[int, int]] = {}
    for it in sizes:
        if it.get("dir") == "UI_Sprites":
            size_map[it["name"]] = (int(it["w"]), int(it["h"]))

    def want_file(name: str) -> bool:
        if not args.only:
            return True
        return any(fnmatch.fnmatch(name, pat) for pat in args.only)

    ui_out = out_root / "UI_Sprites"
    ui_out.mkdir(parents=True, exist_ok=True)

    count = 0
    for item in items:
        name = item.filename
        if not want_file(name):
            continue

        target_size = size_map.get(name)
        if not target_size:
            print(f"[skip] missing size entry: {name}")
            continue
        target_w, target_h = target_size

        ref_path = kit_root / "UI_Sprites" / name
        if not ref_path.exists():
            print(f"[skip] missing reference PNG: {ref_path}")
            continue

        out_path = ui_out / name
        if not args.overwrite and out_path.exists():
            print(f"[skip] exists: {out_path}")
            continue

        gen_w, gen_h = choose_gen_size(target_w, target_h)
        gen_size = f"{gen_w}x{gen_h}"

        prompt = item.positive.strip()
        neg = item.negative.strip()
        if neg:
            prompt = f"{prompt}\n\nAvoid: {neg}"

        if args.dry_run:
            print(f"[dry-run] {name} target={target_w}x{target_h} gen={gen_size} model={args.model}")
            count += 1
        else:
            print(f"[gen] {name} target={target_w}x{target_h} gen={gen_size} model={args.model}")

            img_bytes, revised = openai_generate_b64(
                api_key=api_key,
                model=args.model,
                prompt=prompt,
                size=gen_size,
                quality=args.quality or None,
                style=args.style or None,
                timeout=args.timeout,
                max_retries=args.max_retries,
                base_backoff=args.base_backoff,
            )
            img = Image.open(io.BytesIO(img_bytes)).convert("RGBA")

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
