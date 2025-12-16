import argparse
from pathlib import Path

from PromptDbLib import build_prompt_db_from_md


def main() -> int:
    ap = argparse.ArgumentParser(description="Convert _prompt_sheet_all_v05.md to PromptDb JSON.")
    ap.add_argument("--in-md", default="Tools/UiRestyleV05/_prompt_sheet_all_v05.md")
    ap.add_argument("--out-json", default="Tools/UiRestyleV05/_prompt_db_all_v05.json")
    args = ap.parse_args()

    in_md = Path(args.in_md)
    out_json = Path(args.out_json)
    if not in_md.exists():
        raise SystemExit(f"Missing input: {in_md}")

    build_prompt_db_from_md(in_md, out_json)
    print(f"Wrote: {out_json}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

