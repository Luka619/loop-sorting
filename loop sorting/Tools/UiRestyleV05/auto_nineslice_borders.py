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


def _nearest_pow2(n: int) -> int:
    n = max(1, int(n))
    p = 1
    while p < n:
        p <<= 1
    # p is now >= n, (p>>1) < n
    lower = p >> 1
    if lower < 1:
        return p
    return lower if (n - lower) <= (p - n) else p


def _next_pow2(n: int) -> int:
    n = max(1, int(n))
    p = 1
    while p < n:
        p <<= 1
    return p


def _prev_pow2(n: int) -> int:
    n = max(1, int(n))
    p = 1
    while (p << 1) <= n:
        p <<= 1
    return p


def _parse_texture_meta(meta_path: str, platform: str):
    # Parse only the bits we need from Unity .meta YAML.
    # NPOTScale enum: 0=None, 1=ToNearest, 2=ToLarger, 3=ToSmaller
    n_pot_scale = 0
    default_max_size = None
    platform_max_size = None
    platform_overridden = False

    if not os.path.exists(meta_path):
        return n_pot_scale, default_max_size, platform_max_size, platform_overridden

    in_platform_block = False
    current_target = None
    current_overridden = False
    current_max_size = None

    with open(meta_path, "r", encoding="utf-8", errors="ignore") as f:
        for raw in f:
            line = raw.strip()
            m = re.match(r"nPOTScale:\s*(\d+)", line)
            if m:
                n_pot_scale = int(m.group(1))
                continue

            m = re.match(r"maxTextureSize:\s*(\d+)", line)
            if m and default_max_size is None:
                default_max_size = int(m.group(1))
                continue

            # platformSettings is a list; entries start with "- serializedVersion:"
            if line.startswith("platformSettings:"):
                in_platform_block = True
                current_target = None
                current_overridden = False
                current_max_size = None
                continue

            if not in_platform_block:
                continue

            if line.startswith("- "):
                # commit previous entry
                if current_target == platform and current_overridden and current_max_size:
                    platform_max_size = current_max_size
                    platform_overridden = True
                current_target = None
                current_overridden = False
                current_max_size = None
                continue

            m = re.match(r"buildTarget:\s*(.+)", line)
            if m:
                current_target = m.group(1).strip()
                continue

            m = re.match(r"overridden:\s*(\d+)", line)
            if m:
                current_overridden = m.group(1).strip() == "1"
                continue

            m = re.match(r"maxTextureSize:\s*(\d+)", line)
            if m:
                current_max_size = int(m.group(1))
                continue

    # commit last entry
    if current_target == platform and current_overridden and current_max_size:
        platform_max_size = current_max_size
        platform_overridden = True

    return n_pot_scale, default_max_size, platform_max_size, platform_overridden


def _simulate_import_size(orig_w: int, orig_h: int, max_size: int | None, n_pot_scale: int) -> tuple[int, int]:
    w = int(orig_w)
    h = int(orig_h)

    if max_size and max(orig_w, orig_h) > max_size:
        scale = max_size / float(max(orig_w, orig_h))
        w = max(1, int(round(orig_w * scale)))
        h = max(1, int(round(orig_h * scale)))

    if n_pot_scale == 0:
        return w, h

    if n_pot_scale == 1:  # ToNearest
        w2 = _nearest_pow2(w)
        h2 = _nearest_pow2(h)
    elif n_pot_scale == 2:  # ToLarger
        w2 = _next_pow2(w)
        h2 = _next_pow2(h)
    elif n_pot_scale == 3:  # ToSmaller
        w2 = _prev_pow2(w)
        h2 = _prev_pow2(h)
    else:
        w2, h2 = w, h

    # Keep under max texture size if specified (best-effort).
    if max_size and max(w2, h2) > max_size:
        scale = max_size / float(max(w2, h2))
        w2 = max(1, int(round(w2 * scale)))
        h2 = max(1, int(round(h2 * scale)))
        if n_pot_scale != 0:
            w2 = _nearest_pow2(w2) if n_pot_scale == 1 else (_next_pow2(w2) if n_pot_scale == 2 else _prev_pow2(w2))
            h2 = _nearest_pow2(h2) if n_pot_scale == 1 else (_next_pow2(h2) if n_pot_scale == 2 else _prev_pow2(h2))

    return int(w2), int(h2)


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

    # Convert to the same pixel space Unity uses at runtime by simulating TextureImporter scaling.
    meta_path = path + ".meta"
    n_pot_scale, default_max, platform_max, overridden = _parse_texture_meta(
        meta_path, platform=_compute_border_for_sprite.platform
    )
    max_size = platform_max if overridden and platform_max else default_max
    imp_w, imp_h = _simulate_import_size(w, h, max_size=max_size, n_pot_scale=n_pot_scale)

    sx = imp_w / float(w)
    sy = imp_h / float(h)

    pad_left_i = int(round(pad_left * sx))
    pad_right_i = int(round(pad_right * sx))
    pad_top_i = int(round(pad_top * sy))
    pad_bottom_i = int(round(pad_bottom * sy))

    visible_w_i = max(1, imp_w - pad_left_i - pad_right_i)
    visible_h_i = max(1, imp_h - pad_top_i - pad_bottom_i)

    # User rule: after excluding transparent padding, keep only the middle `center_stretch_fraction`
    # (width & height) as the stretchable area. Borders are the remaining area split equally on both sides.
    # side_fraction = (1 - center) / 2
    center = _compute_border_for_sprite.center_stretch_fraction
    side_fraction = (1.0 - center) * 0.5

    vis_border_x = max(1, int(round(visible_w_i * side_fraction)))
    vis_border_y = max(1, int(round(visible_h_i * side_fraction)))

    left = pad_left_i + vis_border_x
    right = pad_right_i + vis_border_x
    top = pad_top_i + vis_border_y
    bottom = pad_bottom_i + vis_border_y
    return _clamp_border_for_sprite(Border(left, right, top, bottom), w=imp_w, h=imp_h)


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
        "--center",
        type=float,
        default=(1.0 / 3.0),
        help="Center stretch fraction after trimming padding (e.g. 0.5 keeps middle half; 0.333 keeps middle third).",
    )
    ap.add_argument(
        "--platform",
        default="WebGL",
        help="Unity buildTarget name to apply importer overrides from .meta (e.g. WebGL, Android).",
    )
    ap.add_argument(
        "--dry-run",
        action="store_true",
        help="Print summary only; do not write config.",
    )
    args = ap.parse_args()
    _compute_border_for_sprite.center_stretch_fraction = max(0.1, min(0.9, float(args.center)))
    _compute_border_for_sprite.platform = str(args.platform).strip()

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
