import argparse
import fnmatch
import json
import os
import re
from dataclasses import dataclass
from pathlib import Path
from typing import Dict, List, Optional, Tuple

from PIL import Image, ImageChops, ImageFilter


@dataclass(frozen=True)
class PromptItem:
    filename: str
    wants_transparent: bool


def load_json(path: Path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def parse_prompt_sheet(path: Path) -> List[PromptItem]:
    text = path.read_text(encoding="utf-8-sig")
    files = re.findall(r"^## UI_Sprites/([^ ]+)", text, flags=re.M)
    if not files:
        raise RuntimeError(f"No sections found in prompt sheet: {path}")

    items: List[PromptItem] = []
    for name in files:
        # Heuristic: prompt sheet includes "transparent background" for most assets,
        # except bg_main/overlay_dim. We follow that convention.
        wants_transparent = name not in ("bg_main.png", "overlay_dim.png")
        items.append(PromptItem(filename=name, wants_transparent=wants_transparent))
    return items


def alpha_bbox(img: Image.Image, alpha_threshold: int = 8) -> Optional[Tuple[int, int, int, int]]:
    a = img.convert("RGBA").getchannel("A")
    bbox = a.point([0 if i < alpha_threshold else 255 for i in range(256)]).getbbox()
    return bbox


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


def fit_to_reference(
    gen: Image.Image,
    *,
    ref_path: Path,
    wants_transparent: bool,
) -> Image.Image:
    ref = Image.open(ref_path).convert("RGBA")
    target_w, target_h = ref.size

    canvas = Image.new("RGBA", (target_w, target_h), (0, 0, 0, 0))
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
    scale = max(0.15, min(6.0, scale))
    new_w = max(1, int(round(img.size[0] * scale)))
    new_h = max(1, int(round(img.size[1] * scale)))
    img = img.resize((new_w, new_h), resample=Image.Resampling.LANCZOS)

    gen_bbox2 = alpha_bbox(img) or (0, 0, img.size[0], img.size[1])
    gen_cx = (gen_bbox2[0] + gen_bbox2[2]) / 2.0
    gen_cy = (gen_bbox2[1] + gen_bbox2[3]) / 2.0

    paste_x = int(round(ref_cx - gen_cx))
    paste_y = int(round(ref_cy - gen_cy))

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
        bg = sample_edge_bg_rgb(gen)
        opaque = Image.new("RGBA", (target_w, target_h), (bg[0], bg[1], bg[2], 255))
        opaque.alpha_composite(canvas)
        canvas = opaque

    return canvas


def main() -> int:
    ap = argparse.ArgumentParser(description="Normalize Web ChatGPT-generated images into exact Unity UI sprite sizes.")
    ap.add_argument("--in-dir", required=True, help="Directory containing UI_Sprites/*.png (names should match target filenames).")
    ap.add_argument("--out-dir", default="Tools/UiRestyleV05/_web_output", help="Output directory root (will create UI_Sprites/).")
    ap.add_argument("--kit-root", default="Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti")
    ap.add_argument("--prompt-sheet", default="Tools/UiRestyleV05/_prompt_sheet_hud_v05.md")

    ap.add_argument("--only", action="append", default=[], help="glob filter(s), e.g. 'mint_square_*' (repeatable)")
    ap.add_argument("--overwrite", action="store_true", default=False)
    ap.add_argument("--allow-partial", action="store_true", default=False)
    ap.add_argument("--bg-tolerance", type=int, default=18)
    args = ap.parse_args()

    in_root = Path(args.in_dir)
    kit_root = Path(args.kit_root)
    prompt_sheet = Path(args.prompt_sheet)
    out_root = Path(args.out_dir)

    items = parse_prompt_sheet(prompt_sheet)

    def want_file(name: str) -> bool:
        if not args.only:
            return True
        return any(fnmatch.fnmatch(name, pat) for pat in args.only)

    in_ui = in_root / "UI_Sprites"
    if not in_ui.exists():
        # allow passing UI_Sprites directly
        if in_root.name.lower() == "ui_sprites":
            in_ui = in_root
        else:
            raise SystemExit(f"UI_Sprites folder not found in: {in_root}")

    out_ui = out_root / "UI_Sprites"
    out_ui.mkdir(parents=True, exist_ok=True)

    processed = 0
    missing: List[str] = []

    for item in items:
        name = item.filename
        if not want_file(name):
            continue

        src = in_ui / name
        if not src.exists():
            missing.append(name)
            continue

        ref = kit_root / "UI_Sprites" / name
        if not ref.exists():
            print(f"[skip] missing reference: {ref}")
            continue

        dst = out_ui / name
        if dst.exists() and not args.overwrite:
            print(f"[skip] exists: {dst}")
            continue

        img = Image.open(src).convert("RGBA")
        if item.wants_transparent:
            img = ensure_transparent(img, tol=args.bg_tolerance)

        final_img = fit_to_reference(img, ref_path=ref, wants_transparent=item.wants_transparent)
        final_img.save(dst, format="PNG")
        processed += 1
        print(f"[ok] {name} -> {dst}")

    if missing and not args.allow_partial:
        print("")
        print("Missing files (rename your downloads to these exact names and put them under UI_Sprites/):")
        for m in missing:
            print(f"- {m}")
        raise SystemExit(f"Missing {len(missing)} files. Re-run with --allow-partial to ignore.")

    print("")
    print(f"Done. Processed: {processed}. Missing: {len(missing)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

