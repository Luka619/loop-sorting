#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import argparse
import json
import os
import re
from collections import defaultdict
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Dict, Iterable, List, Optional, Set, Tuple


def load_json(path: Path) -> Any:
    # Some repo json files may be written with UTF-8 BOM.
    # Using utf-8-sig transparently strips BOM if present.
    try:
        return json.loads(path.read_text(encoding="utf-8-sig"))
    except UnicodeDecodeError:
        return json.loads(path.read_text(encoding="utf-8"))


def dump_md_lines(lines: Iterable[str], out_path: Path) -> None:
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_text("\n".join(lines).rstrip() + "\n", encoding="utf-8")


def uniq_sorted(items: Iterable[str]) -> List[str]:
    return sorted({x for x in items if x})


def categorize_sprite(name: str) -> str:
    base = os.path.basename(name).lower()
    if base.startswith("bg_"):
        return "Background"
    if base.startswith("overlay_"):
        return "Overlay"
    if base.startswith("panel_"):
        return "Panel"
    if base.startswith("hud_"):
        return "HUD"
    if base.startswith("btn_") or base.startswith("button_"):
        return "Button"
    if base.startswith("icon_"):
        return "Icon"
    if base.startswith("digit_"):
        return "Digit"
    if base.startswith("shop_"):
        return "Shop"
    if base.startswith("toggle_"):
        return "Toggle"
    if base.startswith("tag_") or base.startswith("badge_"):
        return "Tag/Badge"
    if base.startswith("lock_"):
        return "Lock"
    return "Other"


@dataclass
class ScreenUsage:
    key: str
    prefabs: List[str]
    sprites: List[str]
    tmp_texts: List[str]


def _collect_from_node(
    node: Any,
    *,
    blueprint_root: Dict[str, Any],
    sprites: Set[str],
    prefabs: Set[str],
    tmp_texts: List[str],
    prefab_stack: List[str],
    max_prefab_depth: int,
) -> None:
    if node is None:
        return

    if isinstance(node, list):
        for x in node:
            _collect_from_node(
                x,
                blueprint_root=blueprint_root,
                sprites=sprites,
                prefabs=prefabs,
                tmp_texts=tmp_texts,
                prefab_stack=prefab_stack,
                max_prefab_depth=max_prefab_depth,
            )
        return

    if not isinstance(node, dict):
        return

    sprite = node.get("sprite")
    if isinstance(sprite, str) and sprite.strip():
        sprites.add(sprite.strip())

    sprite_states = node.get("spriteStates")
    if isinstance(sprite_states, dict):
        for _, v in sprite_states.items():
            if isinstance(v, str) and v.strip():
                sprites.add(v.strip())

    if node.get("type") == "TMP_Text":
        t = node.get("text")
        if isinstance(t, str) and t.strip():
            tmp_texts.append(t.strip())

    if node.get("type") == "PrefabInstance":
        prefab_name = node.get("prefab")
        if isinstance(prefab_name, str) and prefab_name.strip():
            prefab_name = prefab_name.strip()
            if prefab_name not in prefabs:
                prefabs.add(prefab_name)
            if len(prefab_stack) < max_prefab_depth and prefab_name not in prefab_stack:
                prefab_def = blueprint_root.get(prefab_name)
                if isinstance(prefab_def, dict):
                    _collect_from_node(
                        prefab_def,
                        blueprint_root=blueprint_root,
                        sprites=sprites,
                        prefabs=prefabs,
                        tmp_texts=tmp_texts,
                        prefab_stack=prefab_stack + [prefab_name],
                        max_prefab_depth=max_prefab_depth,
                    )

    children = node.get("children")
    if isinstance(children, list):
        _collect_from_node(
            children,
            blueprint_root=blueprint_root,
            sprites=sprites,
            prefabs=prefabs,
            tmp_texts=tmp_texts,
            prefab_stack=prefab_stack,
            max_prefab_depth=max_prefab_depth,
        )


def collect_screen_usage(
    blueprint: Dict[str, Any],
    screen_key: str,
    *,
    include_prefabs: bool = True,
    max_prefab_depth: int = 3,
) -> ScreenUsage:
    root = blueprint.get(screen_key)
    sprites: Set[str] = set()
    prefabs: Set[str] = set()
    tmp_texts: List[str] = []
    _collect_from_node(
        root,
        blueprint_root=blueprint if include_prefabs else {},
        sprites=sprites,
        prefabs=prefabs,
        tmp_texts=tmp_texts,
        prefab_stack=[],
        max_prefab_depth=max_prefab_depth if include_prefabs else 0,
    )
    return ScreenUsage(
        key=screen_key,
        prefabs=uniq_sorted(prefabs),
        sprites=uniq_sorted(sprites),
        tmp_texts=uniq_sorted(tmp_texts),
    )


def load_size_map(sizes_json: Optional[Path]) -> Dict[str, Tuple[int, int]]:
    if not sizes_json or not sizes_json.exists():
        return {}
    data = load_json(sizes_json)
    out: Dict[str, Tuple[int, int]] = {}
    if isinstance(data, list):
        for it in data:
            if not isinstance(it, dict):
                continue
            if it.get("dir") != "UI_Sprites":
                continue
            name = it.get("name")
            if not isinstance(name, str):
                continue
            try:
                out[name] = (int(it.get("w")), int(it.get("h")))
            except Exception:
                continue
    return out


def main() -> int:
    ap = argparse.ArgumentParser(description="Report per-screen UI sprite usage from ui_blueprint.json.")
    ap.add_argument(
        "--blueprint",
        default="Assets/Resources/loop_sorting_ui_components_v04_4_meta_pack_firework_confetti/Layout/ui_blueprint.json",
        help="Path to ui_blueprint.json",
    )
    ap.add_argument(
        "--sizes-json",
        default="Tools/UiRestyleV05/_sizes_ui_sprites.json",
        help="Optional sizes json for UI_Sprites (for quick size reference).",
    )
    ap.add_argument(
        "--out",
        default="Tools/UiRestyleV05/_ui_screen_usage_report.md",
        help="Output markdown path.",
    )
    ap.add_argument("--include-prefabs", action="store_true", default=True)
    ap.add_argument("--max-prefab-depth", type=int, default=3)
    args = ap.parse_args()

    blueprint_path = Path(args.blueprint)
    sizes_json = Path(args.sizes_json) if args.sizes_json else None
    out_path = Path(args.out)

    blueprint = load_json(blueprint_path)
    if not isinstance(blueprint, dict):
        raise SystemExit(f"Invalid blueprint json: root is {type(blueprint).__name__}, expected object.")
    # ui_blueprint.json stores screens/modals under the "prefabs" object.
    blueprint_root = blueprint.get("prefabs")
    if isinstance(blueprint_root, dict):
        blueprint = blueprint_root

    size_map = load_size_map(sizes_json)

    screen_keys = [k for k in blueprint.keys() if isinstance(k, str) and (k.startswith("Screen_") or k.startswith("Modal_"))]
    screen_keys = sorted(screen_keys, key=lambda s: (0 if s.startswith("Screen_") else 1, s))

    usages: List[ScreenUsage] = []
    for key in screen_keys:
        usages.append(
            collect_screen_usage(
                blueprint,
                key,
                include_prefabs=bool(args.include_prefabs),
                max_prefab_depth=max(0, int(args.max_prefab_depth)),
            )
        )

    lines: List[str] = []
    lines.append("# UI Screen → Sprite Usage Report")
    lines.append("")
    lines.append(f"- Blueprint: `{blueprint_path.as_posix()}`")
    if sizes_json and sizes_json.exists():
        lines.append(f"- Sizes: `{sizes_json.as_posix()}`")
    lines.append("")
    lines.append("## Screens")
    lines.append("")
    for u in usages:
        lines.append(f"### {u.key}")
        lines.append(f"- Prefabs: {', '.join(u.prefabs) if u.prefabs else '(none)'}")
        if u.tmp_texts:
            lines.append(f"- TMP texts: {', '.join(u.tmp_texts)}")
        lines.append("")

        by_cat: Dict[str, List[str]] = defaultdict(list)
        for s in u.sprites:
            by_cat[categorize_sprite(s)].append(s)

        for cat in sorted(by_cat.keys()):
            items = sorted(by_cat[cat])
            lines.append(f"**{cat}** ({len(items)})")
            for s in items:
                base = os.path.basename(s)
                size = size_map.get(base)
                size_str = f" ({size[0]}x{size[1]})" if size else ""
                lines.append(f"- `{base}`{size_str}")
            lines.append("")

    dump_md_lines(lines, out_path)
    print(f"Wrote: {out_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
