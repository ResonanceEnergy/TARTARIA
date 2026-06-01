#!/usr/bin/env python3
"""Apply local-LLM outputs to destinations. Strips ANSI, fixes mojibake,
joins split string literals, then writes to path declared in each ticket's
'## Output destination' header.

Usage: python tools/local-llm/apply_outputs.py [--dry-run] [--no-overwrite]
"""
import sys
import re
from pathlib import Path

REPO = Path(__file__).resolve().parent.parent.parent
TASKS_DIR = REPO / "tools" / "local-llm" / "LOCAL_TASKS"
OUTPUTS_DIR = REPO / "tools" / "local-llm" / "LOCAL_OUTPUTS"

DRY_RUN = "--dry-run" in sys.argv
NO_OVERWRITE = "--no-overwrite" in sys.argv

MOJIBAKE_MAP = {
    "ΓåÆ": "->",
    "ΓÇö": "-",
    "ΓÇô": "-",
    "ΓÇ£": '"',
    "ΓÇ¥": '"',
    "ΓÇÿ": "'",
    "ΓÇÖ": "'",
}


def strip_ansi(text):
    """Replay terminal cursor sequences left by Ollama streaming output."""
    text = re.sub(r"\x1b\[", "[", text)
    out_lines = []
    cur_line = []
    i = 0
    n = len(text)
    while i < n:
        ch = text[i]
        if ch == "\n":
            out_lines.append("".join(cur_line))
            cur_line = []
            i += 1
            continue
        m = re.match(r"\[(\d+)D", text[i:])
        if m:
            back = int(m.group(1))
            if back <= len(cur_line):
                del cur_line[-back:]
            else:
                cur_line.clear()
            i += m.end()
            continue
        if text[i:i+2] == "[K":
            i += 2
            continue
        m = re.match(r"\[\d*[ABCDEFGHJSTfm]", text[i:])
        if m:
            i += m.end()
            continue
        m = re.match(r"\[\?\d+[hl]", text[i:])
        if m:
            i += m.end()
            continue
        cur_line.append(ch)
        i += 1
    if cur_line:
        out_lines.append("".join(cur_line))
    return "\n".join(out_lines)


def fix_mojibake(text):
    for bad, good in MOJIBAKE_MAP.items():
        text = text.replace(bad, good)
    return text


def join_split_strings(text):
    """Join consecutive lines where a regular double-quoted string was split
    by the model. C# regular strings cannot span lines."""
    lines = text.split("\n")
    out = []
    i = 0
    while i < len(lines):
        line = lines[i]
        quotes = len(re.findall(r'(?<!\\)"', line))
        if quotes % 2 == 1 and i + 1 < len(lines):
            joined = line
            j = i + 1
            while j < len(lines):
                continuation = lines[j].lstrip()
                joined += " " + continuation
                if len(re.findall(r'(?<!\\)"', joined)) % 2 == 0:
                    break
                j += 1
                if j - i > 5:
                    break
            out.append(joined)
            i = j + 1
        else:
            out.append(line)
            i += 1
    return "\n".join(out)


def find_destination(ticket_md):
    text = ticket_md.read_text(encoding="utf-8", errors="replace")
    m = re.search(r"^##\s+Output destination\s*\n+(.+?)(?:\n\n|\n##)",
                  text, re.MULTILINE | re.DOTALL)
    if not m:
        return None
    for line in m.group(1).strip().splitlines():
        line = line.strip().strip("`").strip()
        if not line or line.startswith("**") or line.startswith("("):
            continue
        if "/" in line or line.endswith((".cs", ".ps1", ".py", ".yaml")):
            return line.replace("\\", "/")
    return None


def extract_largest_block(response_md):
    raw = response_md.read_text(encoding="utf-8", errors="replace")
    cleaned = strip_ansi(raw)
    cleaned = fix_mojibake(cleaned)
    blocks = []
    for m in re.finditer(r"```(\w+)?\n(.*?)```", cleaned, re.DOTALL):
        lang = (m.group(1) or "").lower()
        content = m.group(2)
        content = join_split_strings(content)
        blocks.append((lang, content))
    if not blocks:
        return None
    blocks.sort(key=lambda b: len(b[1]), reverse=True)
    return blocks[0]


def main():
    if not OUTPUTS_DIR.exists():
        print("No outputs dir")
        return 1
    results = []
    for ticket_dir in sorted(OUTPUTS_DIR.iterdir()):
        if not ticket_dir.is_dir() or ticket_dir.name.startswith("_"):
            continue
        ticket_md = TASKS_DIR / (ticket_dir.name + ".md")
        response_md = ticket_dir / "response.md"
        if not ticket_md.exists():
            results.append((ticket_dir.name, "SKIP", "no ticket"))
            continue
        if not response_md.exists() or response_md.stat().st_size < 200:
            results.append((ticket_dir.name, "PENDING", "no response"))
            continue
        dest = find_destination(ticket_md)
        if dest is None:
            results.append((ticket_dir.name, "FAIL", "no destination"))
            continue
        block = extract_largest_block(response_md)
        if block is None:
            results.append((ticket_dir.name, "FAIL", "no code block"))
            continue
        lang, content = block
        dest_path = REPO / dest
        dest_path.parent.mkdir(parents=True, exist_ok=True)
        if dest_path.exists() and NO_OVERWRITE:
            results.append((ticket_dir.name, "SKIP", dest + " exists"))
            continue
        if DRY_RUN:
            results.append((ticket_dir.name, "DRY", "-> " + dest + " (" + str(len(content)) + ")"))
        else:
            dest_path.write_text(content, encoding="utf-8")
            results.append((ticket_dir.name, "OK", "-> " + dest + " (" + str(len(content)) + ")"))
    width = max((len(r[0]) for r in results), default=20)
    print()
    print("TICKET".ljust(width) + "  STATUS    DETAIL")
    print("-" * (width + 30))
    for name, status, detail in results:
        print(name.ljust(width) + "  " + status.ljust(8) + "  " + detail)
    print()
    fails = sum(1 for r in results if r[1] == "FAIL")
    oks = sum(1 for r in results if r[1] == "OK")
    pending = sum(1 for r in results if r[1] == "PENDING")
    print("OK: " + str(oks) + "    PENDING: " + str(pending) + "    FAIL: " + str(fails))
    return 0 if fails == 0 else 1


if __name__ == "__main__":
    sys.exit(main())
