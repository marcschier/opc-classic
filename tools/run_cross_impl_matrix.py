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
import shutil
import socket
import subprocess
import sys
import time
from typing import Optional

_HERE = os.path.dirname(os.path.abspath(__file__))
_REPO = os.path.dirname(_HERE)

if _HERE not in sys.path:
    sys.path.insert(0, _HERE)

import probe_matrix


LEFTOVER_PROCESS_NAMES = (
    "Opc.Classic.Samples.DaServer",
    "Opc.Classic.Samples.CttServer",
    "Opc.Classic.Samples.AeServer",
    "Opc.Classic.Samples.HdaServer",
    "Opc.Classic.Samples.OpcSecurityServer",
    "Opc.Classic.Mcp",
    "OpcTestServer_x64",
)


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
    # samples-ae-managed connects directly to the AE sample's managed TCP
    # listener instead of going through the OS SCM + opcae_ps.dll native
    # MIDL stub. The probe driver passes `--ae-connection-string
    # tcp://127.0.0.1:<port>` and the MCP AE connect tool short-circuits
    # to a TcpClientTransport + DcomCallChannel. Used to validate the AE
    # condition-state / ack-condition round-trip without the opcae_ps.dll
    # native marshaller (which is the unfixable blocker for the
    # `samples-ae` native-CCW profile). The sample server reads its bind
    # port from OPC_CLASSIC_LISTEN_ADDRESS / OPC_CLASSIC_SAMPLE_PORT; the
    # runner sets 127.0.0.1:51301 when starting this profile.
    "samples-ae-managed": {
        "kind": "ae",
        "clsid": "C4BF6E70-3BA2-4F9C-AE3D-8F6C1D9F2B4F",
        "progid": "Opc.Classic.Samples.AeServer.1",
        "connection_string": "tcp://127.0.0.1:51301",
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


def cleanup_leftover_processes(reason: str) -> None:
    """Stop Windows SCM/MCP processes that can survive a single profile run."""
    if os.name != "nt":
        return
    shell = shutil.which("pwsh") or shutil.which("powershell")
    if shell is None:
        return

    names = "@(" + ",".join("'" + name.replace("'", "''") + "'" for name in LEFTOVER_PROCESS_NAMES) + ")"
    script = f"""
$ErrorActionPreference = 'SilentlyContinue'
$names = {names}
$stopped = New-Object System.Collections.Generic.List[string]
Get-Process | Where-Object {{ $names -contains $_.ProcessName }} | ForEach-Object {{
        try {{
            Stop-Process -Id $_.Id -Force -ErrorAction Stop
            $stopped.Add("$($_.ProcessName):$($_.Id)")
        }} catch {{ }}
}}
Get-CimInstance Win32_Process | Where-Object {{
        $_.Name -ieq 'dotnet.exe' -and $_.CommandLine -and
        ($_.CommandLine -like '*mcp*Opc.Classic.Mcp*' -or $_.CommandLine -like '*Opc.Classic.Mcp.dll*')
}} | ForEach-Object {{
        try {{
            Stop-Process -Id $_.ProcessId -Force -ErrorAction Stop
            $stopped.Add("dotnet:$($_.ProcessId)")
        }} catch {{ }}
}}
if ($stopped.Count -gt 0) {{ $stopped -join ', ' }}
"""
    result = subprocess.run(
        [shell, "-NoLogo", "-NoProfile", "-NonInteractive", "-Command", script],
        cwd=_REPO,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True,
        check=False,
    )
    stopped = result.stdout.strip()
    if stopped:
        print(f"Stopped leftover process(es) {reason}: {stopped}", file=sys.stderr)


# Profiles whose probe driver uses a tcp:// connection_string require a
# pre-started managed sample listener bound to the URI's host:port. The
# matrix runner spawns the corresponding sample EXE with
# OPC_CLASSIC_LISTEN_ADDRESS=<host>:<port> just before invoking
# probe_servers.py, then tears it down in the same profile's finally.
# Bound to OS-assigned port 0 would defeat the purpose -- the probe
# driver hardcodes the port in connection_string. Sample EXEs live under
# samples/<assembly>/bin/Debug/net10.0/<assembly>.exe (matches the
# `dotnet build` defaults that the matrix wrapper produces).
TCP_LISTENER_SAMPLE_EXES: dict[str, str] = {
    "samples-ae-managed": os.path.join(
        _REPO, "samples", "Opc.Classic.Samples.AeServer",
        "bin", "Debug", "net10.0", "Opc.Classic.Samples.AeServer.exe"),
}


def _parse_tcp_endpoint(connection_string: str | None) -> tuple[str, int] | None:
    """Return (host, port) for tcp:// URIs; None otherwise. Mirrors the
    MCP-side OpcMcpDcomConnectionHelper.TryGetTcpEndpoint logic."""
    if not connection_string:
        return None
    if "://" not in connection_string:
        return None
    scheme, rest = connection_string.split("://", 1)
    if scheme.lower() != "tcp":
        return None
    if not rest or "/" in rest.rstrip("/"):
        rest = rest.split("/", 1)[0]
    if ":" not in rest:
        return None
    host, port_text = rest.rsplit(":", 1)
    try:
        port = int(port_text)
    except ValueError:
        return None
    if port <= 0 or port > 65535 or not host:
        return None
    return host, port


def _wait_for_tcp_listener(host: str, port: int, timeout_seconds: float) -> bool:
    """Block until host:port accepts a TCP connection or timeout expires."""
    deadline = time.monotonic() + timeout_seconds
    probe_host = "127.0.0.1" if host in ("0.0.0.0", "localhost") else host
    last_error: Exception | None = None
    while time.monotonic() < deadline:
        try:
            with socket.create_connection((probe_host, port), timeout=1.0):
                return True
        except OSError as ex:
            last_error = ex
            time.sleep(0.2)
    if last_error is not None:
        print(f"Sample listener {host}:{port} never came up: {last_error}", file=sys.stderr)
    return False


def start_tcp_listener_sample(profile: str, target: dict[str, str]) -> subprocess.Popen | None:
    """Spawn the sample EXE configured for a tcp:// connection_string and
    wait for the port to listen. Returns the Popen handle (caller is
    responsible for terminate+wait in a finally), or None when the
    profile doesn't need a sample listener (no tcp:// connection_string
    or no curated EXE mapping)."""
    connection_string = target.get("connection_string")
    endpoint = _parse_tcp_endpoint(connection_string)
    if endpoint is None:
        return None
    exe = TCP_LISTENER_SAMPLE_EXES.get(profile)
    if exe is None:
        return None
    if not os.path.exists(exe):
        print(
            f"Sample EXE for {profile} not found at {exe}; "
            "run `dotnet build` on the sample first.",
            file=sys.stderr,
        )
        return None
    host, port = endpoint
    env = os.environ.copy()
    env["OPC_CLASSIC_LISTEN_ADDRESS"] = f"{host}:{port}"
    print(f"==> starting sample listener for '{profile}' ({exe} on {host}:{port})", file=sys.stderr)
    proc = subprocess.Popen(
        [exe],
        cwd=os.path.dirname(exe),
        stdin=subprocess.DEVNULL,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
        env=env,
        creationflags=(subprocess.CREATE_NEW_PROCESS_GROUP if os.name == "nt" else 0),
    )
    if not _wait_for_tcp_listener(host, port, timeout_seconds=15.0):
        try:
            proc.terminate()
            proc.wait(timeout=5)
        except Exception:
            try:
                proc.kill()
            except Exception:
                pass
        return None
    return proc


def stop_tcp_listener_sample(proc: subprocess.Popen | None, profile: str) -> None:
    if proc is None:
        return
    try:
        proc.terminate()
        proc.wait(timeout=5)
        print(f"==> stopped sample listener for '{profile}' (exit {proc.returncode})", file=sys.stderr)
    except subprocess.TimeoutExpired:
        try:
            proc.kill()
            proc.wait(timeout=5)
        except Exception:
            pass
    except Exception:
        try:
            proc.kill()
        except Exception:
            pass


def run_profile(args: argparse.Namespace, profile: str, overrides: dict[str, str]) -> dict[str, object]:
    target = PROFILE_TARGETS[profile]
    kind = target["kind"]
    clsid = overrides.get(profile, target["clsid"])
    connection_string = target.get("connection_string")

    cmd = [
        sys.executable,
        os.path.join(_HERE, "probe_servers.py"),
        "--host", args.host,
        "--expect-matrix", profile,
        "--request-timeout", str(args.request_timeout),
    ]
    # Profile targets fall into three connection modes:
    #   1. connection_string set: TCP-direct / inmemory / explicit URL.
    #      Pass via --<kind>-connection-string; do NOT also pass CLSID/ProgID
    #      so the MCP connect tool routes via the URL parser cleanly.
    #   2. --use-clsid: DCOM activation via CLSID (for vendor servers where
    #      OPCEnum is unreliable).
    #   3. default: DCOM activation via ProgID (the normal sample path).
    if connection_string:
        cmd += [f"--{kind}-connection-string", connection_string]
    elif args.use_clsid:
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

    target_label = connection_string or (clsid if args.use_clsid else target["progid"])
    print(f"==> running profile '{profile}' ({target_label})", file=sys.stderr)
    # Profiles that drive the AE/HDA/DA sample via tcp:// need an
    # explicit long-running listener -- the SCM activation path is
    # bypassed by design, so registration alone doesn't bind a port.
    sample_listener = start_tcp_listener_sample(profile, target)
    try:
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
    finally:
        stop_tcp_listener_sample(sample_listener, profile)

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

    cleanup_leftover_processes("before matrix")
    for profile in profiles:
        cleanup_leftover_processes(f"before {profile}")
        try:
            entry = run_profile(args, profile, overrides)
        finally:
            cleanup_leftover_processes(f"after {profile}")
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
