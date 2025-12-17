import argparse
import json
import os
import re
from dataclasses import dataclass

from PIL import Image


@dataclass(frozen=True)
class Border:
    left: int
    right: int
    top: int
    bottom: int

    def to_config_list(self):
        # Config order: [left, right, top, bottom]
        return [int(self.left), int(self.right), int(self.top), int(self.bottom)]


def _alpha_bbox(im: Image.Image, alpha_threshold: int):
    a = im.getchannel("A")
    mask = a.point(lambda v: 255 if v > alpha_threshold else 0)
    b = mask.getbbox()
    if not b:
        return None
    left, upper, right_excl, lower_excl = b
    return left, upper, right_excl - 1, lower_excl - 1


def _clamp_border_for_sprite(border: Border, w: int, h: int) -> Border:
    # Unity requires center region sizes >= 1px for both axes to avoid collapsing to 3-slice.
    left = max(0, min(border.left, w - 2))
    right = max(0, min(border.right, w - 2 - left))
    top = max(0, min(border.top, h - 2))
    bottom = max(0, min(border.bottom, h - 2 - top))
    return Border(left=left, right=right, top=top, bottom=bottom)


def _compute_border_for_sprite(path: str, alpha_threshold: int) -> Border | None:
    im = Image.open(path).convert("RGBA")
    w, h = im.size
    bbox = _alpha_bbox(im, alpha_threshold=alpha_threshold)
    if bbox is None:
        return None

    min_x, min_y, max_x, max_y = bbox
    pad_left = min_x
    pad_top = min_y
    pad_right = (w - 1) - max_x
    pad_bottom = (h - 1) - max_y

    visible_w = max(1, w - pad_left - pad_right)
    visible_h = max(1, h - pad_top - pad_bottom)
    aspect = visible_w / float(visible_h)

    file_name = os.path.basename(path).lower()

    # Prefer style-guided "corner sizes" and only compensate for transparent padding.
    # This avoids over-estimating borders when the art contains strong bevels/gradients.
    def _starts(prefix: str):
        return file_name.startswith(prefix)

    # Base "corner sizes" (no padding) in pixels, roughly matching the kit's intended 9-slice lines.
    # For long/pill shapes, horizontal caps are derived from the visible height (true capsule ends),
    # and vertical borders use a safe fraction of the visible height to preserve a 3-row grid.
    base = None
    if _starts("mint_square_") or _starts("purple_square_") or _starts("orange_square_") or _starts("pink_square_"):
        base = Border(170, 170, 170, 170)
    elif _starts("btn_small_"):
        base = Border(80, 80, 55, 55)
    elif _starts("btn_price_green_"):
        base = Border(60, 60, 40, 40)
    elif _starts("tag_fast_"):
        base = Border(60, 60, 40, 40)
    elif _starts("tag_small_"):
        base = Border(50, 50, 30, 30)
    elif file_name == "card_setting_row.png":
        base = Border(70, 70, 50, 50)
    elif file_name in ("lock_chip_plate.png", "lock_overlay.png"):
        base = Border(60, 60, 40, 40)
    elif file_name.startswith("shop_row_") or file_name.startswith("shop_card_") or file_name.startswith("shop_group_"):
        base = Border(90, 90, 60, 60)
    elif file_name.startswith("panel_thick_"):
        base = Border(140, 140, 140, 140)
    elif file_name.startswith("panel_"):
        base = Border(120, 120, 120, 120)

    is_long = ("_long_" in file_name) or ("pill" in file_name) or (file_name.startswith("hud_pill_")) or (aspect >= 1.8)
    if is_long:
        cap_x = int(round(visible_h * 0.5))
        cap_y = int(round(visible_h * 0.25))
        cap_y = max(8, min(cap_y, int(visible_h * 0.45)))
        left = pad_left + cap_x
        right = pad_right + cap_x
        # If we have style defaults, use them as a floor (so we don't go too thin on tall assets).
        if base is not None:
            top = pad_top + max(base.top, cap_y)
            bottom = pad_bottom + max(base.bottom, cap_y)
        else:
            top = pad_top + cap_y
            bottom = pad_bottom + cap_y
        return _clamp_border_for_sprite(Border(left, right, top, bottom), w=w, h=h)

    if base is not None:
        left = pad_left + base.left
        right = pad_right + base.right
        top = pad_top + base.top
        bottom = pad_bottom + base.bottom
        return _clamp_border_for_sprite(Border(left, right, top, bottom), w=w, h=h)

    # Generic rounded-rect corner detection: scan top-left corner until the left edge "fills in".
    pixels = im.load()
    radius_y = 0
    for y in range(min_y, max_y + 1):
        first = None
        for x in range(min_x, max_x + 1):
            if pixels[x, y][3] > alpha_threshold:
                first = x
                break
        if first is not None and first == min_x:
            radius_y = y - min_y
            break

    radius_x = 0
    for x in range(min_x, max_x + 1):
        first = None
        for y in range(min_y, max_y + 1):
            if pixels[x, y][3] > alpha_threshold:
                first = y
                break
        if first is not None and first == min_y:
            radius_x = x - min_x
            break

    radius = max(radius_x, radius_y)
    radius = max(0, min(radius, int(min(visible_w, visible_h) * 0.45)))

    left = pad_left + radius
    right = pad_right + radius
    top = pad_top + radius
    bottom = pad_bottom + radius
    return _clamp_border_for_sprite(Border(left, right, top, bottom), w=w, h=h)


def _replace_nineslice_rules_in_file(config_path: str, new_rules_list: list[dict]):
    raw = open(config_path, "r", encoding="utf-8").read()

    key_idx = raw.find('"nineSliceRules"')
    if key_idx < 0:
        raise RuntimeError('Could not find "nineSliceRules" in config.')

    array_start = raw.find("[", key_idx)
    if array_start < 0:
        raise RuntimeError('Could not find "[" after "nineSliceRules".')

    i = array_start
    depth = 0
    in_str = False
    esc = False
    while i < len(raw):
        ch = raw[i]
        if in_str:
            if esc:
                esc = False
            elif ch == "\\":
                esc = True
            elif ch == '"':
                in_str = False
        else:
            if ch == '"':
                in_str = True
            elif ch == "[":
                depth += 1
            elif ch == "]":
                depth -= 1
                if depth == 0:
                    array_end = i
                    break
        i += 1
    else:
        raise RuntimeError("Could not find end of nineSliceRules array.")

    indent_match = re.search(r'\n(\s*)"nineSliceRules"\s*:\s*\[', raw)
    indent = indent_match.group(1) if indent_match else "  "

    new_rules_json = json.dumps(new_rules_list, ensure_ascii=False, indent=2)
    # Re-indent to match file style (2 spaces inside array, plus file indent).
    new_rules_json = "\n".join(indent + line if line.strip() else line for line in new_rules_json.splitlines())

    replaced = raw[:array_start] + new_rules_json[new_rules_json.find("[") :] + raw[array_end + 1 :]
    open(config_path, "w", encoding="utf-8").write(replaced)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument(
        "--config",
        default=os.path.join("Assets", "Resources", "LoopSortingUIKitConfig.json"),
        help="Path to LoopSortingUIKitConfig.json",
    )
    ap.add_argument(
        "--alpha",
        type=int,
        default=32,
        help="Alpha threshold (0-255) for detecting visible pixels; higher ignores softer glows/shadows.",
    )
    ap.add_argument(
        "--dry-run",
        action="store_true",
        help="Print summary only; do not write config.",
    )
    args = ap.parse_args()

    cfg = json.loads(open(args.config, "r", encoding="utf-8").read())
    resources_root = cfg.get("resourcesRoot")
    if not resources_root:
        raise RuntimeError("Missing resourcesRoot in config.")

    sprites = cfg.get("sprites") or []
    nine_slice_files = set()
    for s in sprites:
        if not isinstance(s, dict):
            continue
        if not s.get("applyNineSlice"):
            continue
        p = s.get("path")
        if not p:
            continue
        nine_slice_files.add(os.path.basename(p))

    if not nine_slice_files:
        print("No sprites with applyNineSlice=true found; nothing to do.")
        return 0

    ui_sprites_dir = os.path.join("Assets", "Resources", resources_root, "UI_Sprites")

    computed = {}
    missing = []
    for file_name in sorted(nine_slice_files, key=lambda x: x.lower()):
        path = os.path.join(ui_sprites_dir, file_name)
        if not os.path.exists(path):
            missing.append(file_name)
            continue
        b = _compute_border_for_sprite(path, alpha_threshold=args.alpha)
        if b is None:
            continue
        computed[file_name] = b

    print(f"applyNineSlice sprites: {len(nine_slice_files)}")
    print(f"computed borders: {len(computed)}")
    if missing:
        print(f"missing files: {len(missing)} (first 10: {missing[:10]})")

    # Preserve existing non-exact or non-target rules; replace exact-per-file rules for targets.
    existing_rules = cfg.get("nineSliceRules") or []
    preserved = []
    removed = 0
    for r in existing_rules:
        if not isinstance(r, dict):
            continue
        pat = (r.get("pattern") or "").strip()
        if not pat:
            continue
        is_exact = ("*" not in pat)
        if is_exact and pat in computed:
            removed += 1
            continue
        preserved.append(r)

    auto_rules = []
    for file_name, border in computed.items():
        auto_rules.append({"pattern": file_name, "border": border.to_config_list()})

    new_rules = auto_rules + preserved

    if args.dry_run:
        for i, (file_name, border) in enumerate(list(computed.items())[:10]):
            print(f"  {file_name}: {border.to_config_list()}")
        print(f"preserved rules: {len(preserved)} (removed exact overrides: {removed})")
        return 0

    _replace_nineslice_rules_in_file(args.config, new_rules)
    # Validate we wrote valid JSON.
    json.loads(open(args.config, "r", encoding="utf-8").read())
    print(f"Updated nineSliceRules in {args.config}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
