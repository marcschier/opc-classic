#!/usr/bin/env python3
"""Spec-citation cross-reference scanner for the Opc.Classic compliance review.

Walks ``src/``, ``mcp/``, ``samples/``, and ``tests/`` and records every
spec citation (``MS-DCOM``, ``OPC-DA``, etc.) found in source code,
docs, or markdown.  Emits a single CSV the per-spec Phase 1 / Phase 2
docs index against to find existing implementation evidence.

Output (one row per citation):

    spec_family    e.g. ``MS-DCOM`` or ``OPC-DA``
    section        e.g. ``3.1.2.5.1`` (best-effort: any ``\\u00a7?`` digits.dots
                   following the family token; empty if unannotated)
    file           repo-relative path
    line           line number (1-based)
    text           the source line, trimmed to 240 chars

Run with ``python tools/conformance_xref.py --repo <path> --out <csv>``.
"""

from __future__ import annotations

import argparse
import csv
import re
import sys
from pathlib import Path

# Citation regex: captures the spec family token followed by an optional
# section number.  Matches MS-DCOM, MS-NLMP, OPC-DA, OPC-HDA, etc.
CITATION_RE = re.compile(
    r"\b(MS-[A-Z][A-Z0-9]+|OPC-(?:DA|HDA|AE|COMMON|BATCH|CPX|DX|SECURITY|XMLDA))\b"
    r"(?:\s*\u00a7?\s*(\d+(?:\.\d+)+))?"
)

# Bag of file extensions worth scanning.  Skips binary + lockfile + image.
SCANNED_EXT = {".cs", ".md", ".py", ".ps1", ".psm1", ".yml", ".yaml", ".json", ".idl", ".cpp", ".h", ".hpp"}

# Directories we skip wholesale (build outputs, vendor, big binary trees).
SKIP_DIR_NAMES = {
    "bin",
    "obj",
    "node_modules",
    ".git",
    ".vs",
    ".idea",
    "TestResults",
    "artifacts",
    "publish",
    "redist",
    "external",
    "matrix-out",
}

REPO_DEFAULT = Path(__file__).resolve().parent.parent


def iter_files(repo: Path, roots: list[str]) -> list[Path]:
    """Yields every file under ``repo/<root>`` whose extension is in
    ``SCANNED_EXT`` and whose path does not traverse a skipped directory.
    """

    out: list[Path] = []
    for root in roots:
        base = repo / root
        if not base.exists():
            continue
        for path in base.rglob("*"):
            if not path.is_file():
                continue
            if path.suffix.lower() not in SCANNED_EXT:
                continue
            parts = {p.lower() for p in path.parts}
            if parts & SKIP_DIR_NAMES:
                continue
            out.append(path)
    return out


def scan_file(repo: Path, path: Path) -> list[tuple[str, str, str, int, str]]:
    """Returns ``(family, section, rel_path, line_no, text)`` rows."""

    try:
        lines = path.read_text(encoding="utf-8", errors="replace").splitlines()
    except OSError:
        return []
    rel = path.relative_to(repo).as_posix()
    out: list[tuple[str, str, str, int, str]] = []
    for idx, raw in enumerate(lines, start=1):
        for m in CITATION_RE.finditer(raw):
            family = m.group(1)
            section = m.group(2) or ""
            text = raw.strip()
            if len(text) > 240:
                text = text[:237] + "..."
            out.append((family, section, rel, idx, text))
    return out


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__.splitlines()[0])
    parser.add_argument(
        "--repo",
        type=Path,
        default=REPO_DEFAULT,
        help="Path to the opc-classic repo root.",
    )
    parser.add_argument(
        "--out",
        type=Path,
        required=True,
        help="Output CSV path.",
    )
    parser.add_argument(
        "--roots",
        nargs="*",
        default=["src", "mcp", "samples", "tests", "docs"],
        help="Top-level subdirectories to scan (default: src mcp samples tests docs).",
    )
    args = parser.parse_args(argv)

    repo: Path = args.repo.resolve()
    out_path: Path = args.out.resolve()

    print(f"Scanning repo: {repo}")
    print(f"Output CSV:    {out_path}")

    files = iter_files(repo, args.roots)
    print(f"Files to scan: {len(files)}")

    out_path.parent.mkdir(parents=True, exist_ok=True)
    rows: list[tuple[str, str, str, int, str]] = []
    for path in files:
        rows.extend(scan_file(repo, path))

    with out_path.open("w", encoding="utf-8", newline="") as fh:
        writer = csv.writer(fh, quoting=csv.QUOTE_MINIMAL)
        writer.writerow(("spec_family", "section", "file", "line", "text"))
        writer.writerows(rows)

    print()
    print(f"Total citations: {len(rows)}")

    by_family: dict[str, int] = {}
    by_family_sectioned: dict[str, int] = {}
    for family, section, _file, _line, _text in rows:
        by_family[family] = by_family.get(family, 0) + 1
        if section:
            by_family_sectioned[family] = by_family_sectioned.get(family, 0) + 1

    print()
    print(f"{'Spec family':<14} {'Total':>7} {'With \u00a7':>7}")
    for family in sorted(by_family, key=lambda k: -by_family[k]):
        print(
            f"{family:<14} {by_family[family]:>7} "
            f"{by_family_sectioned.get(family, 0):>7}"
        )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
