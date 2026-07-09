#!/usr/bin/env python3
"""Spec inventory extractor for the Opc.Classic compliance review.

Reads the vendored spec markdown corpus at ``opc-classic-docs`` (sibling
repository path resolved at runtime) and emits per-spec inventory CSV
files used by Phase 1 / Phase 2 of the compliance plan.

Output (relative to the ``--out`` directory):

    <spec>-headings.csv      section number, title, depth, line
    <spec>-clauses.csv       normative MUST/SHALL clauses by section
    <spec>-interfaces.csv    (OPC specs only) interface/method
                             references parsed from prose

Run with ``python tools/conformance_extract.py --docs <path> --out <path>``.

NOTE: This is a pure-Python helper; no third-party deps. AOT analyzer
banned-symbols do not apply to tools/. Output is plain ASCII CSV.
"""

from __future__ import annotations

import argparse
import csv
import re
import sys
from collections import defaultdict
from pathlib import Path

# 10 OPC + 12 directly-cited MS-* specs from docs/conformance plan.md scope.
SCOPED_SPECS = (
    "OPC-DA-2.05A",
    "OPC-DA-3.00",
    "OPC-HDA-1.20",
    "OPC-AE-1.10",
    "OPC-COMMON-1.10",
    "OPC-BATCH-2.00",
    "OPC-CPX-1.00",
    "OPC-DX-1.00",
    "OPC-SECURITY-1.00",
    "OPC-XMLDA-1.01",
    "MS-DCOM",
    "MS-RPCE",
    "MS-NLMP",
    "MS-KILE",
    "MS-SPNG",
    "MS-SMB2",
    "MS-CIFS",
    "MS-OAUT",
    "MS-RRP",
    "MS-FSCC",
    "MS-CSSP",
    "MS-ERREF",
)

HEADING_RE = re.compile(r"^(#{1,6})\s+(.*?)\s*$")
SECTION_NUM_RE = re.compile(r"^(\d+(?:\.\d+)*)(?:\s+|$)(.*)")

# Normative-clause regex.  MUST and SHALL are case-sensitive per RFC 2119
# convention; the negatives (MUST NOT / SHALL NOT) are checked separately.
NORMATIVE_RE = re.compile(r"\b(MUST NOT|SHALL NOT|MUST|SHALL)\b")

# Interface name patterns we extract from OPC specs (for the OPC interface
# mode).  Permits underscores so IOPCHDA_Browser-style HDA / Batch /
# annotation interfaces are captured.
OPC_INTERFACE_RE = re.compile(r"\bI(OPC[A-Za-z][A-Za-z0-9_]*)\b")
OPC_METHOD_RE = re.compile(r"\bI(OPC[A-Za-z][A-Za-z0-9_]*)::([A-Z_][A-Za-z0-9_]*)\b")


def find_docs_repo(start: Path) -> Path | None:
    """Walk up from ``start`` looking for a sibling ``opc-classic-docs``.

    Returns the absolute path or ``None`` if not found.
    """

    cur = start.resolve()
    for candidate in [cur] + list(cur.parents):
        sibling = candidate.parent / "opc-classic-docs"
        if (sibling / "README.md").exists():
            return sibling
    return None


def parse_headings(lines: list[str]) -> list[tuple[str, str, int, int]]:
    """Returns ``(section_number, title, depth, line_number)`` tuples.

    ``section_number`` may be empty for headings that lack one
    (table-of-contents anchors, appendices).
    """

    out: list[tuple[str, str, int, int]] = []
    for idx, raw in enumerate(lines, start=1):
        m = HEADING_RE.match(raw)
        if not m:
            continue
        depth = len(m.group(1))
        text = m.group(2).strip()
        section_num = ""
        title = text
        m_num = SECTION_NUM_RE.match(text)
        if m_num:
            section_num = m_num.group(1)
            title = m_num.group(2).strip()
        out.append((section_num, title, depth, idx))
    return out


def attach_section(line_no: int, headings: list[tuple[str, str, int, int]]) -> tuple[str, str]:
    """Returns the most-recent ``(section_number, title)`` heading at or
    before the given line number.  Falls back to ``("", "")``.
    """

    section_num = ""
    title = ""
    for sec_num, sec_title, _depth, sec_line in headings:
        if sec_line > line_no:
            break
        section_num = sec_num
        title = sec_title
    return section_num, title


def extract_clauses(lines: list[str], headings: list[tuple[str, str, int, int]]) -> list[tuple[str, str, int, str, str]]:
    """Returns ``(section_number, section_title, line, keyword, sentence)``
    tuples for every normative clause in the document.
    """

    out: list[tuple[str, str, int, str, str]] = []
    for idx, raw in enumerate(lines, start=1):
        if not NORMATIVE_RE.search(raw):
            continue
        # Skip table-of-contents / table rows that just mention the keyword.
        stripped = raw.strip()
        if stripped.startswith("|") or stripped.startswith("- ") and len(stripped) < 80:
            # Tables and short bullets rarely carry normative weight in
            # their own right; downstream readers should still glance.
            pass
        # Best-effort sentence segmentation: split on '.' followed by
        # whitespace.  Keep only the sentence that contains the keyword.
        for sentence in re.split(r"(?<=[\.\?\!])\s+", stripped):
            m = NORMATIVE_RE.search(sentence)
            if not m:
                continue
            sec_num, sec_title = attach_section(idx, headings)
            out.append((sec_num, sec_title, idx, m.group(1), sentence.strip()))
    return out


def extract_opc_interfaces(lines: list[str], headings: list[tuple[str, str, int, int]]) -> list[tuple[str, str, int, str, str]]:
    """For OPC specs, extracts interface and method references seen in the
    prose.  Returns ``(section_number, section_title, line, kind,
    symbol)`` where ``kind`` is ``interface`` or ``method``.
    """

    seen: set[tuple[str, str, int, str, str]] = set()
    out: list[tuple[str, str, int, str, str]] = []
    for idx, raw in enumerate(lines, start=1):
        for m in OPC_METHOD_RE.finditer(raw):
            sec_num, sec_title = attach_section(idx, headings)
            key = (sec_num, sec_title, idx, "method", f"I{m.group(1)}::{m.group(2)}")
            if key not in seen:
                seen.add(key)
                out.append(key)
        for m in OPC_INTERFACE_RE.finditer(raw):
            sec_num, sec_title = attach_section(idx, headings)
            key = (sec_num, sec_title, idx, "interface", f"I{m.group(1)}")
            if key not in seen:
                seen.add(key)
                out.append(key)
    return out


def write_csv(path: Path, header: tuple[str, ...], rows: list[tuple]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="") as fh:
        writer = csv.writer(fh, quoting=csv.QUOTE_MINIMAL)
        writer.writerow(header)
        writer.writerows(rows)


def process_spec(docs_root: Path, out_root: Path, spec: str) -> dict[str, int]:
    md_path = docs_root / f"{spec}.md"
    if not md_path.exists():
        return {"missing": 1}

    lines = md_path.read_text(encoding="utf-8", errors="replace").splitlines()
    headings = parse_headings(lines)
    clauses = extract_clauses(lines, headings)

    spec_slug = spec.lower().replace(".", "-")

    write_csv(
        out_root / f"{spec_slug}-headings.csv",
        ("section_number", "title", "depth", "line"),
        headings,
    )
    write_csv(
        out_root / f"{spec_slug}-clauses.csv",
        ("section_number", "section_title", "line", "keyword", "sentence"),
        clauses,
    )

    summary: dict[str, int] = {
        "headings": len(headings),
        "clauses": len(clauses),
    }

    if spec.startswith("OPC-"):
        interfaces = extract_opc_interfaces(lines, headings)
        write_csv(
            out_root / f"{spec_slug}-interfaces.csv",
            ("section_number", "section_title", "line", "kind", "symbol"),
            interfaces,
        )
        unique_interfaces = len({sym for _s, _t, _l, k, sym in interfaces if k == "interface"})
        unique_methods = len({sym for _s, _t, _l, k, sym in interfaces if k == "method"})
        summary["interfaces"] = unique_interfaces
        summary["methods"] = unique_methods

    return summary


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument(
        "--docs",
        type=Path,
        default=None,
        help="Path to opc-classic-docs.  Defaults to sibling repo lookup.",
    )
    parser.add_argument(
        "--out",
        type=Path,
        required=True,
        help="Output directory for the CSV inventory files.",
    )
    parser.add_argument(
        "--specs",
        nargs="*",
        default=list(SCOPED_SPECS),
        help="Spec list to process (default: 22 scoped specs).",
    )
    args = parser.parse_args(argv)

    docs_root = args.docs or find_docs_repo(Path(__file__).resolve())
    if docs_root is None or not (docs_root / "README.md").exists():
        print(
            "ERROR: could not locate opc-classic-docs.  Pass --docs explicitly.",
            file=sys.stderr,
        )
        return 2

    out_root: Path = args.out
    out_root.mkdir(parents=True, exist_ok=True)

    print(f"Reading specs from: {docs_root}")
    print(f"Writing CSV inventory to: {out_root}")
    print()

    totals: defaultdict[str, int] = defaultdict(int)
    print(f"{'Spec':<22} {'Headings':>9} {'Clauses':>8} {'Ifaces':>7} {'Methods':>8}")
    for spec in args.specs:
        summary = process_spec(docs_root, out_root, spec)
        if summary.get("missing"):
            print(f"{spec:<22}   MISSING")
            continue
        print(
            f"{spec:<22} {summary.get('headings', 0):>9} {summary.get('clauses', 0):>8} "
            f"{summary.get('interfaces', 0):>7} {summary.get('methods', 0):>8}"
        )
        for k, v in summary.items():
            totals[k] += v

    print()
    print("Totals:")
    for k in ("headings", "clauses", "interfaces", "methods"):
        print(f"  {k:<10} {totals[k]}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
