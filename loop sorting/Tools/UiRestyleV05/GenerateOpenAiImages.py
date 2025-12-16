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
import threading
from concurrent.futures import ThreadPoolExecutor, as_completed
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, Iterable, List, Optional, Tuple

from PIL import Image

try:
    from PromptDbLib import PromptDb
except Exception:
    PromptDb = None  # type: ignore


@dataclass(frozen=True)
class PromptItem:
    dir: str
    filename: str
    positive: str
    negative: str
    wants_transparent: bool
    declared_size: Optional[Tuple[int, int]] = None


def load_json(path: Path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def parse_prompt_sheet(path: Path) -> List[PromptItem]:
    # New: Prompt DB JSON (faster to target-edit via CLI; markdown remains supported).
    if path.suffix.lower() == ".json":
        if PromptDb is None:
            raise RuntimeError("PromptDbLib.py not available; cannot read .json prompt db.")
        db = PromptDb.load(path)
        items: List[PromptItem] = []

        def parse_declared_size(s: str) -> Optional[Tuple[int, int]]:
            s = (s or "").strip().lower().replace("×", "x")
            if not s:
                return None
            m = re.fullmatch(r"(\d+)\s*x\s*(\d+)", s)
            if not m:
                return None
            return (int(m.group(1)), int(m.group(2)))

        for key, item in db.items.items():
            dir_name = str(item.get("dir") or "").strip() or key.split("/", 1)[0]
            filename = str(item.get("filename") or "").strip() or key.split("/", 1)[1]
            positive = str(item.get("positive") or "").strip()
            negative = str(item.get("negative") or "").strip()
            bg_meta = str(item.get("background") or "").strip().lower()
            wants_transparent = bg_meta == "transparent" if bg_meta in ("transparent", "opaque") else ("transparent background" in positive.lower())
            declared_size = parse_declared_size(str(item.get("size") or ""))
            items.append(
                PromptItem(
                    dir=dir_name,
                    filename=filename,
                    positive=positive,
                    negative=negative,
                    wants_transparent=wants_transparent,
                    declared_size=declared_size,
                )
            )
        return items

    text = path.read_text(encoding="utf-8-sig")

    # Split by section headers.
    # Example: ## UI_Sprites/mint_square_normal.png (552x566)
    # Example: ## BoosterPurchase/btn_close.png (110x108)
    headers = list(re.finditer(r"^## ([^/]+)/([^ ]+)(?: \((\d+)\s*x\s*(\d+)\))?", text, flags=re.M))
    if not headers:
        raise RuntimeError(f"No sections found in prompt sheet: {path}")

    items: List[PromptItem] = []
    for i, m in enumerate(headers):
        start = m.start()
        end = headers[i + 1].start() if i + 1 < len(headers) else len(text)
        section = text[start:end]
        dir_name = m.group(1).strip()
        filename = m.group(2).strip()
        declared_size = None
        if m.group(3) and m.group(4):
            declared_size = (int(m.group(3)), int(m.group(4)))

        def extract_meta_value(key: str) -> Optional[str]:
            # Matches: - key: value (within the section)
            mm = re.search(rf"^\s*-\s*{re.escape(key)}\s*:\s*(.*?)\s*$", section, flags=re.M)
            return (mm.group(1).strip() if mm else None)

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

        # Prefer structured metadata (recommended): "- background: transparent|opaque"
        # Fallback to legacy heuristic: look for "transparent background" in the positive prompt.
        bg_meta = (extract_meta_value("background") or "").strip().lower()
        if bg_meta in ("transparent", "opaque"):
            wants_transparent = (bg_meta == "transparent")
        else:
            wants_transparent = "transparent background" in positive.lower()

        items.append(
            PromptItem(
                dir=dir_name,
                filename=filename,
                positive=positive,
                negative=negative,
                wants_transparent=wants_transparent,
                declared_size=declared_size,
            )
        )

    return items


def choose_gen_size(target_w: int, target_h: int) -> Tuple[int, int]:
    # Keep it conservative to reduce cost and post-process artifacts:
    # - default to square
    # - only switch to wide/tall for extreme aspect ratios
    r = target_w / max(1, target_h)
    if r >= 1.8:
        # api.apiyi.com gpt-image-1-mini supports 1536x1024 (not 1792x1024)
        return (1536, 1024)
    if r <= 0.55:
        # api.apiyi.com gpt-image-1-mini supports 1024x1536 (not 1024x1792)
        return (1024, 1536)
    return (1024, 1024)


def normalize_size_arg(s: str) -> str:
    s = (s or "").strip().lower().replace("×", "x")
    if not s:
        return ""
    if s == "auto":
        return s
    if not re.fullmatch(r"\d+x\d+", s):
        raise ValueError(f"Invalid size: {s} (expected like 512x512 or auto)")
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
    background: Optional[str],
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
    if background:
        payload["background"] = background
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
            if (
                e.code == 400
                and "background" in payload
                and ("Unknown parameter" in body or '"param":"background"' in body)
            ):
                print("[warn] API rejected background; falling back to prompt-only background instruction")
                payload.pop("background", None)
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


#
# We intentionally do not do any bbox-based fitting / cropping / size normalization here.
# Transparent UI assets are easy to clip when post-processed. Generate with padding instead.


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
        "--background",
        default="",
        help="Optional: transparent|opaque (model/proxy dependent). When set, the prompt is also adjusted to match.",
    )
    ap.add_argument(
        "--gen-size",
        default="",
        help="Override generation size, e.g. 256x256, 512x512, 1024x1024, auto (proxy/model dependent).",
    )
    ap.add_argument(
        "--strict-gen-size",
        action="store_true",
        default=False,
        help="When --gen-size is set, do not fallback to other sizes if the API rejects it.",
    )
    ap.add_argument("--api-key-env", default="OPENAI_API_KEY")
    ap.add_argument(
        "--api-key-file",
        default="",
        help="Optional path to a file that contains the API key (recommended to avoid pasting into commands).",
    )

    ap.add_argument("--prompt-sheet", default="Tools/UiRestyleV05/_prompt_db_all_v05.json")
    ap.add_argument("--sizes-json", default="Tools/UiRestyleV05/_sizes_ui_sprites.json")
    ap.add_argument("--kit-root", default="Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti")
    ap.add_argument("--resources-root", default="Assets/Resources", help="Project Resources root (for BoosterPurchase, setting_page_assets, etc).")
    ap.add_argument("--out-dir", default="Tools/UiRestyleV05/_openai_output")

    ap.add_argument("--only", action="append", default=[], help="glob filter(s), e.g. 'mint_square_*' (repeatable)")
    ap.add_argument("--limit", type=int, default=0, help="limit number of generated files (0 = no limit)")
    ap.add_argument("--overwrite", action="store_true", default=False, help="overwrite existing output PNGs (default: skip)")
    ap.add_argument(
        "--parallel",
        type=int,
        default=1,
        help="Number of concurrent generations (default 1). Use with care to avoid rate limits.",
    )
    ap.add_argument("--dry-run", action="store_true", default=False)

    ap.add_argument("--timeout", type=int, default=180)
    ap.add_argument("--max-retries", type=int, default=5)
    ap.add_argument("--base-backoff", type=float, default=1.2)
    ap.add_argument("--sleep", type=float, default=0.2, help="sleep between requests")

    args = ap.parse_args()
    rejected_gen_sizes: set[str] = set()
    rejected_lock = threading.Lock()

    background = (args.background or "").strip().lower()
    if background and background not in ("transparent", "opaque"):
        raise SystemExit(f"Invalid --background: {args.background} (expected transparent|opaque)")

    parallel = int(args.parallel or 1)
    if parallel < 1:
        raise SystemExit(f"Invalid --parallel: {args.parallel} (expected >= 1)")

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

    def apply_background_override(prompt: str) -> str:
        if not background:
            return prompt
        if background == "opaque":
            p = prompt
            p = re.sub(r"(?i)\btransparent background\b", "opaque background", p)
            p = re.sub(r"(?i)\btransparent padding\b", "padding", p)
            p = re.sub(r"(?i)\bleave generous transparent padding\b", "leave generous padding", p)
            p = re.sub(r"(?i)\bno background\b", "plain solid background (no background scene)", p)
            if "background:" not in p.lower():
                p = f"{p}\n\nBackground: opaque solid color, no scene."
            return p
        if background == "transparent":
            p = prompt
            p = re.sub(r"(?i)\bopaque background\b", "transparent background", p)
            if "transparent background" not in p.lower() and "background:" not in p.lower():
                p = f"{p}\n\nBackground: transparent."
            return p
        return prompt

    def resolve_output_path(item: PromptItem) -> Path:
        return out_root / item.dir / item.filename

    def resolve_target_size(item: PromptItem, ref_path: Path) -> Optional[Tuple[int, int]]:
        ref_size: Optional[Tuple[int, int]] = None
        if ref_path.exists():
            with Image.open(ref_path) as ref_img:
                ref_size = (int(ref_img.size[0]), int(ref_img.size[1]))

        declared = item.declared_size
        target = ref_size
        if declared and (not target or declared != target):
            if target:
                print(
                    f"[warn] declared size overrides reference for {item.dir}/{item.filename}: "
                    f"declared={declared[0]}x{declared[1]} ref={target[0]}x{target[1]}"
                )
            target = declared

        if item.dir == "UI_Sprites":
            mapped = size_map.get(item.filename)
            if mapped:
                if target and mapped != target:
                    print(
                        f"[warn] size json mismatch for {item.dir}/{item.filename}: "
                        f"json={mapped[0]}x{mapped[1]} ref/declared={target[0]}x{target[1]} (use json)"
                    )
                target = mapped

        return target

    def size_candidates(*, gen_size: str) -> List[str]:
        if not args.gen_size:
            return [gen_size]
        if args.strict_gen_size:
            return [gen_size]
        # When overriding to a smaller size, auto-fallback to larger common sizes if the API rejects it.
        common = ["256x256", "512x512", "1024x1024", "1536x1024", "1024x1536", "auto"]
        first = gen_size
        return [first] + [s for s in common if s != first]

    def process_item(item: PromptItem) -> bool:
        name = item.filename
        ref_path = resolve_reference_path(item.dir, name)
        target = resolve_target_size(item, ref_path)
        if not target:
            print(f"[skip] missing reference PNG and no declared size: {ref_path}")
            return False
        target_w, target_h = target

        out_path = out_root / item.dir / name
        if not args.overwrite and out_path.exists():
            print(f"[skip] exists: {out_path}")
            return False

        gen_w, gen_h = choose_gen_size(target_w, target_h)
        gen_size = normalize_size_arg(args.gen_size) or f"{gen_w}x{gen_h}"

        wants_transparent = item.wants_transparent
        if background == "opaque":
            wants_transparent = False
        elif background == "transparent":
            wants_transparent = True

        prompt = apply_background_override(item.positive.strip())
        neg = item.negative.strip()
        if neg:
            prompt = f"{prompt}\n\nAvoid: {neg}"

        def candidates_for_item() -> List[str]:
            if args.gen_size and args.strict_gen_size:
                return size_candidates(gen_size=gen_size)
            with rejected_lock:
                candidates = [s for s in size_candidates(gen_size=gen_size) if s not in rejected_gen_sizes]
            return candidates or ["1024x1024"]

        candidates = candidates_for_item()
        last_error: Optional[Exception] = None
        img_bytes: Optional[bytes] = None
        revised: Optional[str] = None
        img: Optional[Image.Image] = None
        for attempt_size in candidates:
            if attempt_size != gen_size:
                print(f"[warn] retry with gen size {attempt_size} (previous rejected)")
            bg_info = f" bg={background}" if background else ""
            print(f"[gen] {item.dir}/{name} target={target_w}x{target_h} gen={attempt_size} model={args.model}{bg_info}")

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
                    background=background or None,
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
                    with rejected_lock:
                        rejected_gen_sizes.add(attempt_size)
                    continue
                raise
        else:
            raise last_error or RuntimeError("Image generation failed")

        if not img_bytes:
            raise RuntimeError("Image generation returned empty bytes")

        out_path.parent.mkdir(parents=True, exist_ok=True)
        if img is None:
            raise RuntimeError("Image decode failed")
        img.save(out_path, format="PNG")
        if revised:
            (out_path.parent / f"{name}.revised_prompt.txt").write_text(revised, encoding="utf-8")

        time.sleep(max(0.0, float(args.sleep)))
        return True

    # Build work list.
    work: List[PromptItem] = []
    for item in items:
        name = item.filename
        if not want_file(item.dir, name):
            continue
        work.append(item)
        if args.limit and len(work) >= args.limit:
            break

    if args.dry_run:
        count = 0
        for item in work:
            name = item.filename
            ref_path = resolve_reference_path(item.dir, name)
            target = resolve_target_size(item, ref_path)
            if not target:
                print(f"[skip] missing reference PNG and no declared size: {ref_path}")
                continue
            target_w, target_h = target

            out_path = resolve_output_path(item)
            if not args.overwrite and out_path.exists():
                print(f"[skip] exists: {out_path}")
                continue

            gen_w, gen_h = choose_gen_size(target_w, target_h)
            gen_size = normalize_size_arg(args.gen_size) or f"{gen_w}x{gen_h}"
            bg_info = f" bg={background}" if background else ""
            print(f"[dry-run] {item.dir}/{name} target={target_w}x{target_h} gen={gen_size} model={args.model}{bg_info}")
            count += 1
        print(f"Done. Processed: {count}")
        return 0

    # Execute work list.
    count = 0
    if parallel == 1:
        for item in work:
            if process_item(item):
                count += 1
    else:
        with ThreadPoolExecutor(max_workers=parallel) as ex:
            futures = [ex.submit(process_item, item) for item in work]
            for fut in as_completed(futures):
                if fut.result():
                    count += 1

    print(f"Done. Processed: {count}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
