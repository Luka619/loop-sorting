#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import argparse
from pathlib import Path

# Reuse the same API plumbing as the asset generator.
from GenerateOpenAiImages import openai_generate_b64  # type: ignore


def load_api_key(api_key_env: str, api_key_file: str) -> str:
    import os

    key = os.environ.get(api_key_env, "").strip()
    if key:
        return key

    key_file = (api_key_file or "").strip()
    if not key_file:
        key_file = str((Path(__file__).resolve().parent / "_secrets" / "openai_api_key.txt"))
    p = Path(key_file)
    if not p.exists():
        return ""
    return p.read_text(encoding="utf-8").strip()


def build_gameplayhud_concept_prompt(style: str) -> str:
    # Note: For concepts, the model can't literally paste existing PNGs.
    # We enforce "component faithfulness" by describing the exact shapes/roles and forbidding new UI widgets.
    return f"""
Design a mobile puzzle game's **gameplay HUD screen** concept (full-screen UI mockup), NOT individual assets.

CANVAS: 1024x1536 portrait, PNG, **opaque background**.
STYLE: {style}

IMPORTANT: This project has a fixed UI component system. Compose the screen using ONLY these component types and their roles:
- Background: `bg_main` (full-screen)
- Square button backgrounds: `mint_square_*` (Shop/Settings), `purple_square_*` (Speed)
- Pills: `hud_pill_dark_small` (Coins/Lives counters), `hud_level_label_bg` (Level label)
- Icons (glyph-only, centered, no frame): `icon_shop`, `icon_gear`, `icon_coin`, `icon_heart`, `icon_plus`, `icon_sort_noframe`, `icon_shuffle_noframe`
- Digits: `digit_0`..`digit_9` (use for counters; no handwritten numbers)
- Tag: `tag_fast_info_bg` (shows "FAST x5" text on top)
- Badge: `badge_red_bg` (small red circle with a digit on top)

LAYOUT (must follow):
1) Top-left: mint square button + `icon_shop`.
2) Top-right: mint square button + `icon_gear`.
3) Top center: level label pill (`hud_level_label_bg`) with TMP-like text "LEVEL 2".
4) Under the level label (still within top HUD area), show two pills:
   - Left: coins pill (`hud_pill_dark_small`) + `icon_coin` + digits like "33810" + a small `icon_plus` button beside it (coins add).
   - Right: lives pill (`hud_pill_dark_small`) + `icon_heart` + digits like "5" + a small `icon_plus` button beside it (lives add).
5) Conveyor speed control button: a **purple square** button with TMP-like text "1x" placed in the **top HUD area** (top-right cluster, near coins/lives/settings). It must be clearly visible.
6) Bottom HUD area: two booster buttons side-by-side:
   - Left: a square button with `icon_sort_noframe` (no text label on the button).
   - Right: a square button with `icon_shuffle_noframe` (no text label on the button).
7) Add a `tag_fast_info_bg` tag with TMP-like text "FAST x5" and a `badge_red_bg` badge showing a digit.
   - The FAST tag must be a small overlay near the top HUD area or attached to the playfield corner.
   - It must NOT be placed between the two booster buttons.

GAMEPLAY WORLD AREA (middle):
- Show a simplified conveyor/loop-sorting playfield (belts + colored items + containers).
- Do NOT draw a match-3 grid, tiles, or a Sudoku-like board.
- Keep the world scene subtle so the HUD remains the focus.

TEXT RULES:
- All text looks like separate UI text layers (TMP-like). Do NOT bake text into button textures.
- No random scribbles/handwriting. No fake symbols.

HARD DO-NOTs:
- No new UI widgets beyond the listed component types.
- No extra panels/frames/cards.
- No watermark/logo.
- No perspective/isometric skew.
""".strip()


def main() -> int:
    ap = argparse.ArgumentParser(description="Generate a full-screen UI concept mockup (v0.5).")
    ap.add_argument("--api-base", default="https://api.openai.com/v1")
    ap.add_argument("--model", default="gpt-image-1-mini")
    ap.add_argument("--quality", default="low")
    ap.add_argument("--size", default="1024x1536", help="API size enum, e.g. 1024x1536 or auto (proxy/model dependent).")
    ap.add_argument("--background", default="opaque", help="transparent|opaque (concepts should use opaque).")
    ap.add_argument("--style", default="creamy plastic, warm candy, soft 3D UI, rounded corners, thick outline, subtle specular highlight from top-left, soft ambient occlusion, gentle inner shadow, clean silhouette, mobile game UI, orthographic front view")
    ap.add_argument("--out", default="Tools/UiRestyleV05/_concepts/Screen_GameplayHUD_concept_v05.png")
    ap.add_argument("--api-key-env", default="OPENAI_API_KEY")
    ap.add_argument("--api-key-file", default="")
    args = ap.parse_args()

    api_key = load_api_key(args.api_key_env, args.api_key_file)
    if not api_key:
        raise SystemExit(
            f"Missing API key: set env var {args.api_key_env} or create key file at Tools/UiRestyleV05/_secrets/openai_api_key.txt"
        )

    api_base = str(args.api_base).strip().rstrip("/")
    images_url = f"{api_base}/images/generations"

    prompt = build_gameplayhud_concept_prompt(args.style)
    img_bytes, revised = openai_generate_b64(
        images_url=images_url,
        api_base=api_base,
        api_key=api_key,
        model=args.model,
        prompt=prompt,
        size=str(args.size).strip(),
        quality=str(args.quality).strip() or None,
        style=None,
        background=str(args.background).strip() or None,
        response_format=None,
        timeout=180,
        max_retries=5,
        base_backoff=1.2,
    )

    out_path = Path(args.out)
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_bytes(img_bytes)
    if revised:
        out_path.with_suffix(out_path.suffix + ".revised_prompt.txt").write_text(revised, encoding="utf-8")
    print(f"Wrote: {out_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
