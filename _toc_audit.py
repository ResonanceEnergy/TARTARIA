"""
TARTARIA WORLD OF WONDER — Comprehensive Documentation Validator
================================================================
Runs four audit passes across all 54+ markdown documents:
  1. TOC parity   — Table of Contents entries match ## headings
  2. Link check   — All inter-doc markdown links resolve to real files
  3. Status footer — Every doc has a '**Document Status:** FINAL' footer
  4. Format check  — Title separator, typo scan

Usage:
    python _toc_audit.py            # Full report
    python _toc_audit.py --toc      # TOC audit only (original behaviour)
    python _toc_audit.py --json     # Machine-readable JSON output
"""
import os, re, glob, sys, json

# ── Config ──────────────────────────────────────────────────────────────────

DOCS_DIR = "docs"
KNOWN_TYPOS = re.compile(r"\b(mirrorss|consumingthe|teh |hte )\b", re.IGNORECASE)

# ── Discover targets ────────────────────────────────────────────────────────

targets = []
for f in sorted(glob.glob(os.path.join(DOCS_DIR, "*.md"))):
    targets.append(f)
for f in sorted(glob.glob(os.path.join(DOCS_DIR, "appendices", "*.md"))):
    if os.path.basename(f)[0] in "ABCDEFGHIJ":
        targets.append(f)
for f in sorted(glob.glob(os.path.join(DOCS_DIR, "dlc", "*.md"))):
    targets.append(f)

# ── Slugify (GitHub-style anchor) ──────────────────────────────────────────

def slugify(text):
    text = text.lower().strip()
    text = re.sub(r"[^\w\s-]", "", text)
    text = re.sub(r"[\s]+", "-", text)
    text = re.sub(r"-+", "-", text)
    return text.strip("-")

# ── TOC Audit ──────────────────────────────────────────────────────────────

def audit_toc(filepath, lines):
    """Returns (status, issues) where status is 'pass'|'fail'|'no_toc'."""
    all_h2 = []
    for i, line in enumerate(lines):
        m = re.match(r"^## (.+)$", line.strip())
        if m:
            all_h2.append((i + 1, m.group(1).strip()))

    toc_line = None
    for i, line in enumerate(lines):
        if re.match(r"^## Table of Contents\s*$", line.strip()):
            toc_line = i
            break

    body_headers = []
    for ln, h in all_h2:
        if h == "Table of Contents":
            continue
        if toc_line is not None and ln <= toc_line + 1:
            continue
        body_headers.append((ln, h))

    if toc_line is None:
        meaningful2 = []
        for ln, h in all_h2:
            if h == "Table of Contents":
                continue
            if ln <= 5 and not re.match(r"^\d", h):
                continue
            meaningful2.append(h)
        if meaningful2:
            return ("no_toc", [f"{len(meaningful2)} sections but no TOC"])
        return ("pass", [])

    toc_entries = []
    for i in range(toc_line + 1, len(lines)):
        line = lines[i].strip()
        if re.match(r"^##\s", line):
            break
        if line == "" or line.startswith("---"):
            continue
        link_match = re.search(r"\[([^\]]+)\]\(#([^\)]+)\)", line)
        if link_match:
            toc_entries.append((link_match.group(1).strip(), link_match.group(2).strip()))
        elif re.match(r"^\d+\.", line) or re.match(r"^[-*]", line):
            toc_entries.append((line, None))

    anchor_counts = {}
    body_anchor_map = []
    for ln, h in body_headers:
        anchor = slugify(h)
        if anchor in anchor_counts:
            anchor_counts[anchor] += 1
            anchor = anchor + "-" + str(anchor_counts[anchor])
        else:
            anchor_counts[anchor] = 0
        body_anchor_map.append((ln, h, anchor))

    issues = []
    matched_headers = set()

    for entry_text, entry_anchor in toc_entries:
        if entry_anchor is None:
            issues.append(f'TOC entry without anchor link: "{entry_text}"')
            continue
        found = False
        for ln, h, body_anc in body_anchor_map:
            if entry_anchor == body_anc:
                found = True
                matched_headers.add((ln, h))
                break
        if not found:
            for ln, h, body_anc in body_anchor_map:
                if entry_text.rstrip(".") == h.rstrip(".") or entry_text == h:
                    found = True
                    matched_headers.add((ln, h))
                    if entry_anchor != body_anc:
                        issues.append(
                            f'Anchor mismatch: TOC "#{entry_anchor}" vs expected "#{body_anc}" for "{h}"'
                        )
                    break
        if not found:
            issues.append(f'TOC entry orphaned: "{entry_text}" (#{entry_anchor})')

    for ln, h, body_anc in body_anchor_map:
        if (ln, h) not in matched_headers:
            text_match = any(et == h or et.rstrip(".") == h.rstrip(".") for et, ea in toc_entries)
            anchor_match = any(ea == body_anc for et, ea in toc_entries)
            if not text_match and not anchor_match:
                issues.append(f'Section missing from TOC: "{h}" (line {ln})')

    return ("fail" if issues else "pass", issues)

# ── Link Audit ──────────────────────────────────────────────────────────────

def audit_links(filepath, content):
    """Check all [text](path.md) links resolve to existing files."""
    issues = []
    dir_of = os.path.dirname(filepath)
    for m in re.finditer(r"\[([^\]]+)\]\(([^)#]+\.md)(?:#[^)]*)?\)", content):
        link_text, link_path = m.group(1), m.group(2)
        resolved = os.path.normpath(os.path.join(dir_of, link_path))
        if not os.path.isfile(resolved):
            issues.append(f'Broken link: [{link_text}]({link_path})')
    return issues

# ── Status Footer Audit ────────────────────────────────────────────────────

def audit_footer(content):
    """Check for Document Status: FINAL footer."""
    if re.search(r"\*\*Document Status:\*\*\s*FINAL", content):
        return []
    return ["Missing '**Document Status:** FINAL' footer"]

# ── Format Audit ────────────────────────────────────────────────────────────

def audit_format(filepath, lines, content):
    """Check title separator consistency and known typos."""
    issues = []
    if lines and lines[0].startswith("# TARTARIA") and ": " in lines[0] and "\u2014" not in lines[0]:
        issues.append(f'Title uses colon instead of em-dash: "{lines[0].strip()}"')
    for i, line in enumerate(lines):
        for m in KNOWN_TYPOS.finditer(line):
            issues.append(f'Possible typo line {i+1}: "{m.group()}"')
    return issues

# ── Word Count ──────────────────────────────────────────────────────────────

def word_count(content):
    text = re.sub(r"[|#*`\[\]\(\)>_~]", " ", content)
    return len(text.split())

# ── Main ────────────────────────────────────────────────────────────────────

def main():
    toc_only = "--toc" in sys.argv
    json_mode = "--json" in sys.argv

    toc_pass, toc_fail, toc_notoc = [], [], []
    link_issues_all = {}
    footer_issues_all = {}
    format_issues_all = {}
    word_counts = {}

    for filepath in targets:
        relpath = filepath.replace(os.sep, "/")
        with open(filepath, "r", encoding="utf-8") as f:
            content = f.read()
        lines = content.split("\n")

        status, issues = audit_toc(filepath, lines)
        if status == "pass":
            toc_pass.append(relpath)
        elif status == "fail":
            toc_fail.append((relpath, issues))
        else:
            toc_notoc.append((relpath, issues))

        if not toc_only:
            li = audit_links(filepath, content)
            if li:
                link_issues_all[relpath] = li
            fi = audit_footer(content)
            if fi:
                footer_issues_all[relpath] = fi
            fmi = audit_format(filepath, lines, content)
            if fmi:
                format_issues_all[relpath] = fmi
            word_counts[relpath] = word_count(content)

    # ── JSON output ─────────────────────────────────────────────────────
    if json_mode:
        result = {
            "toc": {"pass": len(toc_pass), "fail": len(toc_fail), "no_toc": len(toc_notoc)},
            "links": {"broken": sum(len(v) for v in link_issues_all.values()), "files": len(link_issues_all)},
            "footers": {"missing": len(footer_issues_all)},
            "format": {"issues": sum(len(v) for v in format_issues_all.values())},
            "words": {"total": sum(word_counts.values()), "docs": len(word_counts)},
        }
        print(json.dumps(result, indent=2))
        return

    # ── Text output ─────────────────────────────────────────────────────
    bar = "=" * 80
    print(bar)
    print("TARTARIA DOCS \u2014 COMPREHENSIVE VALIDATION REPORT")
    print(bar)

    print(f"\n{'\u2500'*40}")
    print("1. TOC PARITY AUDIT")
    print(f"{'\u2500'*40}")
    print(f"\n  PASS ({len(toc_pass)} docs) \u2014 TOC perfectly matches sections")
    for p in toc_pass:
        print(f"    [OK] {p}")
    if toc_fail:
        print(f"\n  FAIL ({len(toc_fail)} docs) \u2014 Mismatches found")
        for p, iss in toc_fail:
            print(f"\n    [FAIL] {p}")
            for i in iss:
                print(f"      - {i}")
    if toc_notoc:
        print(f"\n  NO_TOC ({len(toc_notoc)} docs) \u2014 Has sections but no TOC")
        for p, iss in toc_notoc:
            print(f"    [NO_TOC] {p}")
    total = len(toc_pass) + len(toc_fail) + len(toc_notoc)
    print(f"\n  Result: {len(toc_pass)} PASS | {len(toc_fail)} FAIL | {len(toc_notoc)} NO_TOC | {total} audited")

    if toc_only:
        print(f"\n{bar}")
        print(f"TOTALS: {len(toc_pass)} PASS | {len(toc_fail)} FAIL | {len(toc_notoc)} NO_TOC | {total} audited")
        print(bar)
        return

    # 2. Links
    print(f"\n{'\u2500'*40}")
    print("2. INTER-DOCUMENT LINK CHECK")
    print(f"{'\u2500'*40}")
    if link_issues_all:
        for fp, iss in link_issues_all.items():
            print(f"\n  [BROKEN] {fp}")
            for i in iss:
                print(f"    - {i}")
        broken = sum(len(v) for v in link_issues_all.values())
        print(f"\n  Result: {broken} broken links in {len(link_issues_all)} files")
    else:
        print("\n  [OK] All inter-document links resolve correctly")

    # 3. Status Footers
    print(f"\n{'\u2500'*40}")
    print("3. STATUS FOOTER CHECK")
    print(f"{'\u2500'*40}")
    if footer_issues_all:
        for fp in footer_issues_all:
            print(f"  [MISSING] {fp}")
        print(f"\n  Result: {len(footer_issues_all)} docs missing FINAL footer")
    else:
        print(f"\n  [OK] All {len(targets)} docs have FINAL status footer")

    # 4. Format
    print(f"\n{'\u2500'*40}")
    print("4. FORMAT & TYPO CHECK")
    print(f"{'\u2500'*40}")
    if format_issues_all:
        for fp, iss in format_issues_all.items():
            print(f"\n  [FMT] {fp}")
            for i in iss:
                print(f"    - {i}")
        fcount = sum(len(v) for v in format_issues_all.values())
        print(f"\n  Result: {fcount} format issues in {len(format_issues_all)} files")
    else:
        print("\n  [OK] No formatting issues detected")

    # 5. Word Count Summary
    print(f"\n{'\u2500'*40}")
    print("5. WORD COUNT SUMMARY")
    print(f"{'\u2500'*40}")
    total_words = sum(word_counts.values())
    print(f"\n  Total: {total_words:,} words across {len(word_counts)} documents")
    top5 = sorted(word_counts.items(), key=lambda x: -x[1])[:5]
    print("  Top 5:")
    for fp, wc in top5:
        print(f"    {wc:>6,}  {fp}")

    # ── Summary ─────────────────────────────────────────────────────────
    print(f"\n{bar}")
    all_ok = (
        len(toc_fail) == 0
        and len(toc_notoc) == 0
        and len(link_issues_all) == 0
        and len(footer_issues_all) == 0
        and len(format_issues_all) == 0
    )
    if all_ok:
        print(f"ALL CHECKS PASSED \u2014 {total} docs, {total_words:,} words, 0 issues")
    else:
        issues_total = (
            len(toc_fail)
            + len(toc_notoc)
            + sum(len(v) for v in link_issues_all.values())
            + len(footer_issues_all)
            + sum(len(v) for v in format_issues_all.values())
        )
        print(f"ISSUES FOUND: {issues_total} across {total} docs")
    print(bar)


if __name__ == "__main__":
    main()
