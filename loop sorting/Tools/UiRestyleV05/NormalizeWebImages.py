import argparse
import fnmatch
from dataclasses import dataclass
from pathlib import Path
from typing import List, Optional, Tuple

from PIL import Image

try:
    from PromptDbLib import PromptDb
except Exception:
    PromptDb = None  # type: ignore


@dataclass(frozen=True)
class PromptItem:
    dir: str
    filename: str
    declared_size: Optional[Tuple[int, int]] = None


def parse_prompt_sheet(path: Path) -> List[PromptItem]:
    if path.suffix.lower() != ".json":
        raise RuntimeError("NormalizeWebImages.py only supports the Prompt DB JSON in this workflow.")
    if PromptDb is None:
        raise RuntimeError("PromptDbLib.py not available; cannot read .json prompt db.")

    db = PromptDb.load(path)
    items: List[PromptItem] = []
    for key, item in db.items.items():
        dir_name = str(item.get("dir") or "").strip() or key.split("/", 1)[0]
        filename = str(item.get("filename") or "").strip() or key.split("/", 1)[1]
        items.append(PromptItem(dir=dir_name, filename=filename))
    return items


def main() -> int:
    ap = argparse.ArgumentParser(
        description=(
            "Copy images into the project asset folder structure (no bbox fitting / no cropping / no size normalization). "
            "This exists for cases where assets come from a browser download or other sources."
        )
    )
    ap.add_argument("--in-dir", required=True, help="Directory containing subfolders like UI_Sprites/, World_Sprites/, etc.")
    ap.add_argument("--out-dir", default="Tools/UiRestyleV05/_web_output", help="Output directory root.")
    ap.add_argument("--prompt-sheet", default="Tools/UiRestyleV05/_prompt_db_all_v05.json")
    ap.add_argument("--only", action="append", default=[], help="glob filter(s), e.g. 'mint_square_*' or 'UI_Sprites/*' (repeatable)")
    ap.add_argument("--overwrite", action="store_true", default=False)
    ap.add_argument("--allow-partial", action="store_true", default=False)

    args = ap.parse_args()

    in_root = Path(args.in_dir)
    out_root = Path(args.out_dir)
    prompt_sheet = Path(args.prompt_sheet)

    items = parse_prompt_sheet(prompt_sheet)

    def want_file(dir_name: str, name: str) -> bool:
        if not args.only:
            return True
        rel = f"{dir_name}/{name}"
        return any(fnmatch.fnmatch(name, pat) or fnmatch.fnmatch(rel, pat) for pat in args.only)

    count = 0
    missing = 0
    for item in items:
        if not want_file(item.dir, item.filename):
            continue

        src = in_root / item.dir / item.filename
        dst = out_root / item.dir / item.filename

        if not src.exists():
            missing += 1
            if args.allow_partial:
                print(f"[skip] missing input: {src}")
                continue
            raise SystemExit(f"Missing input image: {src} (use --allow-partial to skip)")

        if dst.exists() and not args.overwrite:
            print(f"[skip] exists: {dst}")
            continue

        dst.parent.mkdir(parents=True, exist_ok=True)
        img = Image.open(src).convert("RGBA")
        img.save(dst, format="PNG")
        count += 1

    print(f"Done. Copied: {count}. Missing: {missing}.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

