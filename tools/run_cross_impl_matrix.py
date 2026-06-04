#!/usr/bin/env python3
"""Cross-implementation matrix probe driver for Opc.Classic.

For each sample server profile (`samples-da`, `samples-hda`, `samples-ae`,
`security-da`, `ctt-da`, `testserver`, `matrikon`), runs
`tools/probe_servers.py --expect-matrix <profile>` and emits a one-page
per-profile JSON report plus an aggregate matrix summary.

Usage:

    python tools/run_cross_impl_matrix.py [--profile NAME ...]
                                          [--host HOST]
                                          [--output-dir DIR]
                                          [--username U --password P]

Each profile contributes one entry to the aggregate output:

    {
        "profile": "testserver",
        "clsid": "F8582CF9-88FB-11DA-A5ED-0060B0692061",
        "totals": {
            "MATCH": 90, "REGRESSION": 0,
            "UNEXPECTED_PASS": 0, "MISSING_CLASSIFICATION": 0
        },
        "regressions": [],
        "report_path": "matrix-out/testserver.json"
    }

Exit code 0 iff every profile completed with zero REGRESSION rows.

This driver assumes:
- Each profile's server is registered + reachable on `--host` (default
  localhost). For sample servers, run `<sampleexe>.exe --register
  --registry-hive=hkcu` first.
- The MCP server can be launched via `dotnet run` (the standard probe
  driver path).

Profiles are wired to default CLSIDs / ProgIDs that match the sample
server defaults; override via --clsid-override.
"""

from __future__ import annotations

import argparse
import json
import os
import subprocess
import sys
from typing import Optional

_HERE = os.path.dirname(os.path.abspath(__file__))
_REPO = os.path.dirname(_HERE)

if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import probe_matrix


# Default per-profile CLSIDs / ProgIDs / kind. Each profile picks ONE
# (CLSID or ProgID) -- the probe driver supports both. ProgID is preferred
# when the server is registered locally; CLSID is used for vendor servers
# like Matrikon (where the ProgID-via-OPCEnum path may be flaky).
#
# `kind` is the probe-args bucket (--<kind>-clsid / --<kind>-progid). For
# DA-class profiles this is "da"; for HDA "hda"; for AE "ae".

PROFILE_TARGETS: dict[str, dict[str, str]] = {
    "testserver": {
        "kind": "da",
        "clsid": "F8582CF9-88FB-11DA-A5ED-0060B0692061",
        "progid": "OpcTestServer_x64.1",
    },
    "matrikon": {
        "kind": "da",
        "clsid": "F8582CF1-88FB-11DA-A5ED-0060B0692061",
        "progid": "Matrikon.OPC.Simulation.1",
    },
    "samples-da": {
        "kind": "da",
        "clsid": "B3AE5D6F-2A91-4F8B-9D2C-7E5B0C8F1A3E",
        "progid": "Opc.Classic.Samples.DaServer.1",
    },
    "ctt-da": {
        "kind": "da",
        "clsid": "8F7C1B14-9A6E-4E4D-B5E6-5B7DCC1F2B3A",
        "progid": "Opc.Classic.DaSample.1",
    },
    "samples-hda": {
        "kind": "hda",
        "clsid": "A2BBEA4E-F1C6-469B-8D71-89767DCD2D48",
        "progid": "Opc.Classic.Samples.HdaServer.1",
    },
    "samples-ae": {
        "kind": "ae",
        "clsid": "C4BF6E70-3BA2-4F9C-AE3D-8F6C1D9F2B4F",
        "progid": "Opc.Classic.Samples.AeServer.1",
    },
    "security-da": {
        "kind": "da",
        "clsid": "5A0DA9C7-56D2-4768-9CB3-6FC5E57B6D51",
        "progid": "Opc.Classic.Samples.OpcSecurityServer.1",
    },
}


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=__doc__.splitlines()[0] if __doc__ else "",
        formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument(
        "--profile",
        action="append",
        default=None,
        choices=sorted(PROFILE_TARGETS.keys()),
        help="Profile to run. May be repeated. Default: every profile.")
    parser.add_argument(
        "--host", default="localhost",
        help="DCOM host to target. Default: localhost.")
    parser.add_argument(
        "--output-dir", default="matrix-out",
        help="Directory to write per-profile JSON reports + matrix.json. "
             "Default: matrix-out/ under cwd.")
    parser.add_argument(
        "--use-clsid", action="store_true",
        help="Use the per-profile CLSID instead of the ProgID. Use this when "
             "the server is registered but OPCEnum is misbehaving (e.g. "
             "Matrikon on a hardened DCOM box).")
    parser.add_argument(
        "--username", default=None,
        help="Optional DCOM authentication username (passed verbatim to "
             "probe_servers.py).")
    parser.add_argument(
        "--password", default=None,
        help="Optional DCOM authentication password.")
    parser.add_argument(
        "--use-kerberos", action="store_true",
        help="Negotiate Kerberos for the DCOM AUTH3 (default is NTLM).")
    parser.add_argument(
        "--auth-level", default=None,
        choices=["default", "none", "connect", "call", "packet", "pkt_integrity", "pkt_privacy"],
        help="DCOM RPC authentication level (overrides the probe driver default).")
    parser.add_argument(
        "--clsid-override", action="append", default=None,
        metavar="PROFILE=CLSID",
        help="Override the CLSID for a profile, e.g. "
             "--clsid-override samples-da=AAAAAAAA-...")
    parser.add_argument(
        "--save-wire-payloads", default=None,
        help="Per-profile wire-capture directory. The driver creates a "
             "subdirectory per profile under it.")
    parser.add_argument(
        "--request-timeout", type=float, default=60.0,
        help="Per-tool request timeout in seconds. Default: 60.")
    return parser.parse_args()


def build_overrides(arg: Optional[list[str]]) -> dict[str, str]:
    out: dict[str, str] = {}
    if not arg:
        return out
    for entry in arg:
        if "=" not in entry:
            raise SystemExit(f"Invalid --clsid-override {entry!r}; expected PROFILE=CLSID.")
        key, value = entry.split("=", 1)
        if key not in PROFILE_TARGETS:
            raise SystemExit(f"Unknown profile in --clsid-override: {key}.")
        out[key] = value
    return out


def run_profile(args: argparse.Namespace, profile: str, overrides: dict[str, str]) -> dict[str, object]:
    target = PROFILE_TARGETS[profile]
    kind = target["kind"]
    clsid = overrides.get(profile, target["clsid"])

    cmd = [
        sys.executable,
        os.path.join(_HERE, "probe_servers.py"),
        "--host", args.host,
        "--expect-matrix", profile,
        "--request-timeout", str(args.request_timeout),
    ]
    if args.use_clsid:
        cmd += [f"--{kind}-clsid", clsid]
    else:
        cmd += [f"--{kind}-progid", target["progid"]]
    if args.username:
        cmd += ["--username", args.username]
    if args.password is not None:
        cmd += ["--password", args.password]
    if args.use_kerberos:
        cmd += ["--use-kerberos"]
    if args.auth_level:
        cmd += ["--auth-level", args.auth_level]
    if args.save_wire_payloads:
        cap_dir = os.path.join(os.path.abspath(args.save_wire_payloads), profile)
        os.makedirs(cap_dir, exist_ok=True)
        cmd += ["--save-wire-payloads", cap_dir]

    print(f"==> running profile '{profile}' ({clsid if args.use_clsid else target['progid']})", file=sys.stderr)
    # Capture stdout (for the JSON result) but PASS THROUGH stderr in real
    # time. The MCP server's diagnostic logs go to stderr and we want them
    # visible while the probe runs so that activation hangs / auth failures
    # surface live instead of being buried in a post-run dump.
    result = subprocess.run(
        cmd,
        cwd=_REPO,
        stdout=subprocess.PIPE,
        stderr=None,  # inherit parent stderr -> visible in real time
        text=True,
        check=False,
    )
    try:
        payload = json.loads(result.stdout)
    except json.JSONDecodeError as ex:
        # stderr was streamed live; we don't have it to include in the
        # fatal record, but the operator saw it on their terminal.
        return {
            "profile": profile,
            "clsid": clsid,
            "totals": {},
            "regressions": [],
            "fatal": f"non-JSON probe output: {ex}. See the live stderr above for details.",
        }

    totals = probe_matrix.summarize_verdicts(payload)
    regressions = [
        {
            "tool": row.get("tool"),
            "error": row.get("error"),
            "expectedOutcome": row.get("expectedOutcome"),
        }
        for row in payload
        if isinstance(row, dict) and row.get("verdict") == "REGRESSION"
    ]
    return {
        "profile": profile,
        "clsid": clsid,
        "progid": target["progid"],
        "kind": kind,
        "totals": totals,
        "regressions": regressions,
        "probe_exit_code": result.returncode,
        "raw_results": payload,
    }


def main() -> int:
    args = parse_args()
    overrides = build_overrides(args.clsid_override)

    os.makedirs(args.output_dir, exist_ok=True)

    profiles = args.profile or sorted(PROFILE_TARGETS.keys())
    aggregate: list[dict[str, object]] = []
    any_fatal = False

    for profile in profiles:
        entry = run_profile(args, profile, overrides)
        report_path = os.path.join(args.output_dir, f"{profile}.json")
        with open(report_path, "w", encoding="utf-8") as fh:
            json.dump(entry.get("raw_results", []), fh, indent=2, default=str)
        entry["report_path"] = os.path.relpath(report_path, _REPO)
        # Drop raw_results from the aggregate -- keeps the top-level
        # matrix.json scannable.
        entry.pop("raw_results", None)
        aggregate.append(entry)
        if entry.get("fatal"):
            any_fatal = True

    matrix_path = os.path.join(args.output_dir, "matrix.json")
    with open(matrix_path, "w", encoding="utf-8") as fh:
        json.dump({"profiles": aggregate}, fh, indent=2, default=str)

    print_summary(aggregate)
    print(f"\nReports written under {args.output_dir}/", file=sys.stderr)

    if any_fatal:
        return 3
    has_regression = any(entry.get("regressions") for entry in aggregate)
    return 2 if has_regression else 0


def print_summary(aggregate: list[dict[str, object]]) -> None:
    print("\n=== Cross-impl matrix summary ===", file=sys.stderr)
    print(f"{'profile':<14} {'kind':<5} {'MATCH':>6} {'REGR':>6} {'UNEXP':>6} {'MISS':>6}", file=sys.stderr)
    for entry in aggregate:
        totals = entry.get("totals", {}) or {}
        regr = totals.get("REGRESSION", 0) if isinstance(totals, dict) else 0
        match = totals.get("MATCH", 0) if isinstance(totals, dict) else 0
        unexp = totals.get("UNEXPECTED_PASS", 0) if isinstance(totals, dict) else 0
        miss = totals.get("MISSING_CLASSIFICATION", 0) if isinstance(totals, dict) else 0
        marker = " !!!" if regr else ""
        print(f"{entry['profile']:<14} {entry.get('kind') or '?':<5} {match:>6} {regr:>6} {unexp:>6} {miss:>6}{marker}", file=sys.stderr)


if __name__ == "__main__":
    sys.exit(main())
