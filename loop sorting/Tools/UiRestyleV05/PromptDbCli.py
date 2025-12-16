import argparse
import json
from pathlib import Path

from PromptDbLib import PromptDb


def cmd_get(args: argparse.Namespace) -> int:
    db = PromptDb.load(Path(args.db))
    item = db.get(args.key)
    if not item:
        raise SystemExit(f"Key not found: {args.key}")
    print(json.dumps(item, ensure_ascii=False, indent=2))
    return 0


def cmd_list(args: argparse.Namespace) -> int:
    db = PromptDb.load(Path(args.db))
    keys = sorted(db.items.keys())
    if args.contains:
        keys = [k for k in keys if args.contains in k]
    for k in keys:
        print(k)
    return 0


def cmd_set(args: argparse.Namespace) -> int:
    db_path = Path(args.db)
    db = PromptDb.load(db_path)
    item = db.get(args.key)
    if not item:
        raise SystemExit(f"Key not found: {args.key}")

    changed = False
    if args.positive is not None:
        item["positive"] = args.positive
        changed = True
    if args.negative is not None:
        item["negative"] = args.negative
        changed = True
    if args.background is not None:
        item["background"] = args.background
        changed = True
    if args.template is not None:
        item["template"] = args.template
        changed = True
    if args.tags is not None:
        item["tags"] = [t for t in (args.tags.split(",") if args.tags else []) if t]
        changed = True

    if not changed:
        print("No changes requested.")
        return 0

    db.items[args.key] = item
    db.save(db_path)
    print(f"Updated: {args.key}")
    return 0


def cmd_export_md(args: argparse.Namespace) -> int:
    db = PromptDb.load(Path(args.db))
    out_path = Path(args.out)
    out_path.write_text(db.to_markdown(), encoding="utf-8")
    print(f"Wrote: {out_path}")
    return 0


def main() -> int:
    ap = argparse.ArgumentParser(description="Prompt DB helper (get/set/export).")
    ap.add_argument("--db", default="Tools/UiRestyleV05/_prompt_db_all_v05.json")
    sub = ap.add_subparsers(dest="cmd", required=True)

    ap_list = sub.add_parser("list", help="List keys")
    ap_list.add_argument("--contains", default="")
    ap_list.set_defaults(func=cmd_list)

    ap_get = sub.add_parser("get", help="Print a single entry as JSON")
    ap_get.add_argument("key")
    ap_get.set_defaults(func=cmd_get)

    ap_set = sub.add_parser("set", help="Update a single entry")
    ap_set.add_argument("key")
    ap_set.add_argument("--positive", default=None)
    ap_set.add_argument("--negative", default=None)
    ap_set.add_argument("--background", choices=["transparent", "opaque"], default=None)
    ap_set.add_argument("--template", default=None)
    ap_set.add_argument("--tags", default=None, help="Comma-separated tags")
    ap_set.set_defaults(func=cmd_set)

    ap_export = sub.add_parser("export-md", help="Export DB back to a single markdown sheet")
    ap_export.add_argument("--out", default="Tools/UiRestyleV05/_prompt_sheet_all_v05.md")
    ap_export.set_defaults(func=cmd_export_md)

    args = ap.parse_args()
    return int(args.func(args))


if __name__ == "__main__":
    raise SystemExit(main())

