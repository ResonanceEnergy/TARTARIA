"""TOC parity audit for TARTARIA docs. Temp file — safe to delete."""
import os, re, glob

docs_dir = r"docs"
targets = []

for f in sorted(glob.glob(os.path.join(docs_dir, "*.md"))):
    targets.append(f)

for f in sorted(glob.glob(os.path.join(docs_dir, "appendices", "*.md"))):
    basename = os.path.basename(f)
    if basename[0] in "ABCDEFGHIJ":
        targets.append(f)

for f in sorted(glob.glob(os.path.join(docs_dir, "dlc", "*.md"))):
    targets.append(f)


def slugify(text):
    text = text.lower().strip()
    # Remove special chars but keep ampersand → convert to empty, parens, colons etc
    text = re.sub(r"[^\w\s-]", "", text)
    text = re.sub(r"[\s]+", "-", text)
    text = re.sub(r"-+", "-", text)
    text = text.strip("-")
    return text


results_pass = []
results_fail = []
results_notoc = []

for filepath in targets:
    relpath = filepath.replace(os.sep, "/")

    with open(filepath, "r", encoding="utf-8") as f:
        content = f.read()
    lines = content.split("\n")

    # Find all ## headers
    all_h2 = []
    for i, line in enumerate(lines):
        m = re.match(r"^## (.+)$", line.strip())
        if m:
            all_h2.append((i + 1, m.group(1).strip()))

    # Find TOC
    toc_line = None
    for i, line in enumerate(lines):
        if re.match(r"^## Table of Contents\s*$", line.strip()):
            toc_line = i
            break

    # Get body headers — skip TOC itself and subtitle before TOC
    body_headers = []
    for ln, h in all_h2:
        if h == "Table of Contents":
            continue
        # Skip headers that appear before or at the TOC line
        if toc_line is not None and ln <= toc_line + 1:
            continue
        body_headers.append((ln, h))

    # Also filter subtitle-style headers at the very top (before line 6, non-numbered)
    filtered_body = []
    for ln, h in body_headers:
        # Keep all — the TOC filter above should handle it
        filtered_body.append((ln, h))
    body_headers = filtered_body

    if toc_line is None:
        if len(body_headers) > 0:
            # Count only meaningful sections (skip subtitle-style headers at top)
            meaningful = [h for ln, h in all_h2 if h != "Table of Contents"]
            # Filter out subtitle at line 2-3
            meaningful2 = []
            for ln, h in all_h2:
                if h == "Table of Contents":
                    continue
                if ln <= 5 and not re.match(r"^\d", h):
                    continue
                meaningful2.append(h)
            if len(meaningful2) > 0:
                results_notoc.append((relpath, len(meaningful2)))
        continue

    # Extract TOC entries
    toc_entries = []
    for i in range(toc_line + 1, len(lines)):
        line = lines[i].strip()
        if re.match(r"^##\s", line):
            break
        if line == "" or line.startswith("---"):
            continue
        link_match = re.search(r"\[([^\]]+)\]\(#([^\)]+)\)", line)
        if link_match:
            entry_text = link_match.group(1).strip()
            entry_anchor = link_match.group(2).strip()
            toc_entries.append((entry_text, entry_anchor))
        elif re.match(r"^\d+\.", line) or re.match(r"^[-*]", line):
            toc_entries.append((line, None))

    # Build anchor map from body headers
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

    # Check each TOC entry has a matching section
    for entry_text, entry_anchor in toc_entries:
        if entry_anchor is None:
            issues.append(f'TOC entry without anchor link: "{entry_text}"')
            continue

        found = False
        # First try: exact anchor match
        for ln, h, body_anc in body_anchor_map:
            if entry_anchor == body_anc:
                found = True
                matched_headers.add((ln, h))
                break

        if not found:
            # Second try: text match (ignore trailing periods)
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
            issues.append(
                f'TOC entry orphaned (no matching ## header): "{entry_text}" (#{entry_anchor})'
            )

    # Check each body header has a TOC entry
    for ln, h, body_anc in body_anchor_map:
        if (ln, h) not in matched_headers:
            text_match = any(
                et == h or et.rstrip(".") == h.rstrip(".") for et, ea in toc_entries
            )
            anchor_match = any(ea == body_anc for et, ea in toc_entries)
            if not text_match and not anchor_match:
                issues.append(f'Section missing from TOC: "{h}" (line {ln})')

    if issues:
        results_fail.append((relpath, issues))
    else:
        results_pass.append(relpath)


# Print report
print("=" * 80)
print("TARTARIA DOCS — TOC PARITY AUDIT REPORT")
print("=" * 80)

print(f"\n=== PASS ({len(results_pass)} docs) — TOC perfectly matches sections ===")
for p in results_pass:
    print(f"  [OK] {p}")

print(f"\n=== FAIL ({len(results_fail)} docs) — Mismatches found ===")
for p, iss in results_fail:
    print(f"\n  [FAIL] {p}")
    for i in iss:
        print(f"    - {i}")

print(f"\n=== NO_TOC ({len(results_notoc)} docs) — Has ## sections but no TOC ===")
for p, c in results_notoc:
    print(f"  [NO_TOC] {p} ({c} sections)")

print()
print("=" * 80)
total = len(results_pass) + len(results_fail) + len(results_notoc)
print(
    f"TOTALS: {len(results_pass)} PASS | {len(results_fail)} FAIL | {len(results_notoc)} NO_TOC | {total} audited"
)
print("=" * 80)
