import argparse
import base64
import json
import os
import re
import sys
import uuid
import urllib.request
from pathlib import Path


def _join_images_endpoint(base_url: str) -> str:
    base_url = (base_url or "").strip().rstrip("/")
    if not base_url:
        raise ValueError("Missing base URL (set APIYI_BASE_URL or pass --base-url).")
    if base_url.endswith("/v1"):
        return base_url + "/images/generations"
    return base_url + "/v1/images/generations"


def _http_json(url: str, payload: dict, headers: dict, timeout_s: int) -> dict:
    data = json.dumps(payload).encode("utf-8")
    req = urllib.request.Request(url, data=data, method="POST")
    for k, v in headers.items():
        req.add_header(k, v)
    req.add_header("Content-Type", "application/json")
    with urllib.request.urlopen(req, timeout=timeout_s) as resp:
        raw = resp.read()
    return json.loads(raw.decode("utf-8"))


def _download(url: str, timeout_s: int) -> bytes:
    with urllib.request.urlopen(url, timeout=timeout_s) as resp:
        return resp.read()


def _ensure_parent(path: Path) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)


def _write_png(path: Path, png_bytes: bytes) -> None:
    if not png_bytes.startswith(b"\x89PNG\r\n\x1a\n"):
        raise ValueError("Output is not a PNG (missing PNG signature).")
    _ensure_parent(path)
    path.write_bytes(png_bytes)


def _copy_meta(meta_template: Path, dst_png: Path) -> Path:
    if not meta_template.exists():
        raise FileNotFoundError(f"meta template not found: {meta_template}")
    dst_meta = dst_png.with_suffix(dst_png.suffix + ".meta")
    text = meta_template.read_text(encoding="utf-8")
    new_guid = uuid.uuid4().hex
    out_lines = []
    for line in text.splitlines():
        if line.startswith("guid: "):
            out_lines.append("guid: " + new_guid)
        else:
            out_lines.append(line)
    dst_meta.write_text("\n".join(out_lines) + "\n", encoding="utf-8")
    return dst_meta


def _apply_mapping(dst_png: Path) -> None:
    cfg = Path("Assets/Resources/LoopSortingUIKitConfig.json")
    cs = Path("Assets/Scripts/LoopSortingUIKit.cs")
    if not cfg.exists() or not cs.exists():
        raise FileNotFoundError("Expected project files missing (Assets/Resources/LoopSortingUIKitConfig.json, Assets/Scripts/LoopSortingUIKit.cs).")

    # Expect output under: Assets/Resources/<pack>/World_Sprites/<file>.png
    parts = dst_png.as_posix().split("/")
    try:
        world_idx = parts.index("World_Sprites")
    except ValueError as e:
        raise ValueError("Output must be under a 'World_Sprites' folder to apply mapping automatically.") from e
    rel_path = "/".join(parts[world_idx:])  # World_Sprites/<file>.png
    rel_path = rel_path.replace("World_Sprites/", "World_Sprites/")  # no-op, keep clarity
    rel_path = "/".join(rel_path.split("/")[1:])  # <file>.png
    rel_path = f"World_Sprites/{rel_path}"

    cfg_text = cfg.read_text(encoding="utf-8")
    cfg_new, n1 = re.subn(
        r'(\{\s*"key"\s*:\s*"world\.conveyor_slot"\s*,\s*"path"\s*:\s*")[^"]+(")',
        r"\g<1>" + rel_path + r"\2",
        cfg_text,
        count=1,
    )
    if n1 != 1:
        raise ValueError("Failed to update LoopSortingUIKitConfig.json mapping for world.conveyor_slot.")
    cfg.write_text(cfg_new, encoding="utf-8")

    cs_text = cs.read_text(encoding="utf-8")
    cs_new, n2 = re.subn(
        r'(new TextureEntry\s*\{\s*key\s*=\s*"world\.conveyor_slot"\s*,\s*path\s*=\s*")[^"]+(")',
        r"\g<1>" + rel_path + r"\2",
        cs_text,
        count=1,
    )
    if n2 != 1:
        raise ValueError("Failed to update LoopSortingUIKit.cs DefaultConfig mapping for world.conveyor_slot.")
    cs.write_text(cs_new, encoding="utf-8")


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--base-url", default=os.environ.get("APIYI_BASE_URL") or os.environ.get("OPENAI_BASE_URL") or "")
    ap.add_argument("--api-base", default="", help="Alias for --base-url (matches UiRestyleV05 scripts).")
    ap.add_argument("--api-key", default=os.environ.get("APIYI_API_KEY") or os.environ.get("OPENAI_API_KEY") or "")
    ap.add_argument("--api-key-file", default="", help="Optional path to a file that contains the API key.")
    ap.add_argument("--endpoint", default="")
    ap.add_argument("--model", default="gpt-image-1.5")
    ap.add_argument("--prompt", required=True)
    ap.add_argument("--size", default="512x512")
    ap.add_argument(
        "--quality",
        default="",
        help="Optional quality hint (e.g. low/medium/high). Omitted by default for compatibility.",
    )
    ap.add_argument(
        "--background",
        default="",
        help="Optional background hint (e.g. transparent). Omitted by default for compatibility.",
    )
    ap.add_argument("--out", required=True)
    ap.add_argument("--timeout", type=int, default=120)
    ap.add_argument("--auth-header", default="Authorization")
    ap.add_argument("--auth-prefix", default="Bearer ")
    ap.add_argument("--response-format", default="b64_json", choices=["b64_json", "url"])
    ap.add_argument("--meta-template", default="")
    ap.add_argument("--apply-mapping", action="store_true")
    args = ap.parse_args()

    base_url = (args.api_base or args.base_url).strip()
    api_key = args.api_key.strip()
    if not api_key and args.api_key_file:
        api_key = Path(args.api_key_file).read_text(encoding="utf-8-sig").strip()

    if not api_key:
        print("Missing API key (set APIYI_API_KEY/OPENAI_API_KEY or pass --api-key/--api-key-file).", file=sys.stderr)
        return 2

    endpoint = args.endpoint.strip() or _join_images_endpoint(base_url)
    headers = {args.auth_header: args.auth_prefix + api_key}

    payload = {
        "model": args.model,
        "prompt": args.prompt,
        "size": args.size,
        "n": 1,
        "response_format": args.response_format,
    }
    if args.quality:
        payload["quality"] = args.quality
    if args.background:
        payload["background"] = args.background

    resp = _http_json(endpoint, payload, headers, timeout_s=args.timeout)
    data = resp.get("data") or []
    if not data:
        raise ValueError("No image returned (missing response.data[0]).")
    item = data[0] or {}

    png_bytes = None
    if "b64_json" in item and item["b64_json"]:
        png_bytes = base64.b64decode(item["b64_json"])
    elif "url" in item and item["url"]:
        png_bytes = _download(item["url"], timeout_s=args.timeout)
    else:
        raise ValueError("Unsupported response format (expected b64_json or url in response.data[0]).")

    out_path = Path(args.out)
    _write_png(out_path, png_bytes)
    print(f"Wrote: {out_path}")

    if args.meta_template:
        meta_path = _copy_meta(Path(args.meta_template), out_path)
        print(f"Wrote: {meta_path}")

    if args.apply_mapping:
        _apply_mapping(out_path)
        print("Updated mapping: world.conveyor_slot")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
