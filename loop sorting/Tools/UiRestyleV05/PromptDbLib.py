import json
import re
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Dict, List, Optional, Tuple


@dataclass
class PromptDb:
    version: int
    style_core: str
    negative_core: str
    export_core: str
    items: Dict[str, Dict[str, Any]]  # key => item dict

    @staticmethod
    def load(path: Path) -> "PromptDb":
        data = json.loads(path.read_text(encoding="utf-8-sig"))
        if int(data.get("version", 0)) != 1:
            raise RuntimeError(f"Unsupported PromptDb version in {path}: {data.get('version')}")
        return PromptDb(
            version=1,
            style_core=data.get("style_core", ""),
            negative_core=data.get("negative_core", ""),
            export_core=data.get("export_core", ""),
            items=dict(data.get("items", {})),
        )

    def save(self, path: Path) -> None:
        data = {
            "version": 1,
            "style_core": self.style_core,
            "negative_core": self.negative_core,
            "export_core": self.export_core,
            "items": self.items,
        }
        path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    def get(self, key: str) -> Optional[Dict[str, Any]]:
        return self.items.get(key)

    def to_markdown(self) -> str:
        # Keep the existing markdown format so existing tooling and manual workflows still work.
        lines: List[str] = []
        lines.append("# UI Prompt Sheet (v0.5 / Creamy Plastic) - All")
        lines.append("")
        lines.append(
            "Usage: copy each item prompt to generate a PNG with the exact same filename and pixel size, then run `Tools/UiRestyleV05/ReplacePngs.ps1` to overwrite Unity assets (only `.png`, keep `.meta`)."
        )
        lines.append("")
        lines.append("Global constants (recommended):")
        lines.append("")
        lines.append("**STYLE_CORE**")
        lines.append("~~~")
        lines.append(self.style_core.strip())
        lines.append("~~~")
        lines.append("")
        lines.append("**NEGATIVE_CORE**")
        lines.append("~~~")
        lines.append(self.negative_core.strip())
        lines.append("~~~")
        lines.append("")
        lines.append("**EXPORT_CORE**")
        lines.append("~~~")
        lines.append(self.export_core.strip())
        lines.append("~~~")
        lines.append("")
        lines.append("---")
        lines.append("")

        def tag_of(key: str) -> str:
            return key.split("/", 1)[0]

        keys = sorted(self.items.keys(), key=lambda k: (tag_of(k).lower(), k.lower()))
        tags: List[str] = []
        for k in keys:
            t = tag_of(k)
            if not tags or tags[-1] != t:
                tags.append(t)

        for t in tags:
            lines.append(f"### TAG: {t}")
            lines.append("")
            for k in [kk for kk in keys if tag_of(kk) == t]:
                item = self.items[k]
                size = item.get("size", "")
                header = f"## {k}"
                if size:
                    header += f" ({size})"
                lines.append(header)
                if item.get("template"):
                    lines.append(f"- template: {item['template']}")
                if item.get("background"):
                    lines.append(f"- background: {item['background']}")
                if item.get("tags"):
                    lines.append(f"- tags: {', '.join(item['tags'])}")
                lines.append("")
                lines.append("**Positive prompt**")
                lines.append("~~~")
                lines.append((item.get("positive") or "").strip())
                lines.append("~~~")
                lines.append("")
                lines.append("**Negative prompt**")
                lines.append("~~~")
                lines.append((item.get("negative") or "").strip())
                lines.append("~~~")
                lines.append("")

        return "\n".join(lines).rstrip() + "\n"


HEADER_RE = re.compile(r"^## ([^/]+)/([^ ]+)(?: \(([^)]+)\))?", re.M)


def parse_prompt_sheet_md(path: Path) -> Tuple[Dict[str, str], Dict[Tuple[str, str], Dict[str, Any]]]:
    text = path.read_text(encoding="utf-8-sig")
    # Normalize line endings to avoid regex edge-cases on Windows (\r\n).
    text = text.replace("\r\n", "\n")
    # Extract global constants blocks if present.
    def extract_block(title: str) -> str:
        pat = re.compile(rf"^\*\*{re.escape(title)}\*\*\s*\n+^~~~\s*\n(.*?)\n^~~~\s*$", re.S | re.M)
        m = pat.search(text)
        return (m.group(1).strip() if m else "")

    globals_ = {
        "STYLE_CORE": extract_block("STYLE_CORE"),
        "NEGATIVE_CORE": extract_block("NEGATIVE_CORE"),
        "EXPORT_CORE": extract_block("EXPORT_CORE"),
    }

    headers = list(HEADER_RE.finditer(text))
    if not headers:
        raise RuntimeError(f"No sections found in prompt sheet: {path}")

    items: Dict[Tuple[str, str], Dict[str, Any]] = {}
    for i, m in enumerate(headers):
        start = m.start()
        end = headers[i + 1].start() if i + 1 < len(headers) else len(text)
        section = text[start:end]
        dir_name = m.group(1).strip()
        filename = m.group(2).strip()
        size = (m.group(3) or "").strip()

        def extract_meta_value(key: str) -> Optional[str]:
            mm = re.search(rf"^\s*-\s*{re.escape(key)}\s*:\s*(.*?)\s*$", section, flags=re.M)
            return (mm.group(1).strip() if mm else None)

        def extract_prompt_block(title: str) -> str:
            pat = re.compile(rf"^\*\*{re.escape(title)}\*\*\s*\n+^~~~\s*\n(.*?)\n^~~~\s*$", re.S | re.M)
            mm = pat.search(section)
            if not mm:
                raise RuntimeError(f"Missing '{title}' block for {dir_name}/{filename} in {path}")
            return mm.group(1).strip()

        positive = extract_prompt_block("Positive prompt")
        negative = extract_prompt_block("Negative prompt")

        bg = (extract_meta_value("background") or "").strip().lower()
        template = extract_meta_value("template")
        tags_raw = extract_meta_value("tags") or ""
        tags = [t.strip() for t in tags_raw.split(",") if t.strip()]

        item: Dict[str, Any] = {
            "dir": dir_name,
            "filename": filename,
            "size": size,
            "background": (bg if bg in ("transparent", "opaque") else ""),
            "template": (template or ""),
            "tags": tags,
            "positive": positive,
            "negative": negative,
        }
        items[(dir_name, filename)] = item

    return globals_, items


def build_prompt_db_from_md(prompt_sheet_md: Path, out_json: Path) -> PromptDb:
    globals_, items = parse_prompt_sheet_md(prompt_sheet_md)
    db_items: Dict[str, Dict[str, Any]] = {}
    for (d, f), item in items.items():
        key = f"{d}/{f}"
        db_items[key] = {
            "dir": d,
            "filename": f,
            "size": item.get("size", ""),
            "background": item.get("background", ""),
            "template": item.get("template", ""),
            "tags": item.get("tags", []),
            "positive": item.get("positive", ""),
            "negative": item.get("negative", ""),
        }

    db = PromptDb(
        version=1,
        style_core=globals_.get("STYLE_CORE", ""),
        negative_core=globals_.get("NEGATIVE_CORE", ""),
        export_core=globals_.get("EXPORT_CORE", ""),
        items=db_items,
    )
    db.save(out_json)
    return db
