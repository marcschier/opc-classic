#!/usr/bin/env python3
"""
mcp_driver.py — end-to-end demo / smoke test for the Opc.Classic MCP
server against a live OPC DA server (typically Matrikon.OPC.Simulation.1).

This script drives the MCP server over stdio via JSON-RPC and exercises
the Track Y demo path:

  1. session.create
  2. da.connect           — multi-IID activation, registers per-IID IPIDs
  3. da.get_status        — exercises Y1-Y9 (NDR unique pointer + MInterfacePointer)
  4. da.read_items_by_id  — exercises Y10 (VARIANT marshaling) via the
                            DA 3.0 stateless IOPCItemIO interface,
                            avoiding the need for AddGroup/AddItems QI
                            navigation (Y9b deferred-pointer work).

KNOWN ISSUE — IOPCItemIO::Read fails with RPC_S_INVALID_TAG (0x800706F7)
against real DCOM servers because the request body needs the explicit
`dwCount` field (the IDL `size_is(dwCount)` sibling parameter) emitted
*before* the conformant arrays, in addition to each array's own
max_count prefix. Our generator currently uses the array length once
as max_count and never writes the separate dwCount field. Tracked as
follow-up work: add an `[OpcCountField]` / `[OpcSizeIs("dwCount")]`
attribute mechanism so the generator can emit the count both as a
standalone scalar and as the array conformance header.

GetStatus (the simpler `[out] OPCSERVERSTATUS**` path) DOES work
end-to-end against Matrikon today via Y1-Y9, confirming the NDR shape
model + MInterfacePointer codec are wire-compatible.

Requirements (on the host running this script):
  - Python 3.10+ (uses subprocess + json built-ins, no external deps).
  - .NET 10 SDK so `dotnet run` can launch the MCP server.
  - A reachable OPC DA server. Defaults assume Matrikon Simulation at
    localhost via the well-known CLSID F8582CF2-88FB-11D0-B850-00C0F0104305
    (which bypasses OPCEnum — OPCEnum has a separate DCOM ACL that often
    rejects unprivileged callers even when the actual server accepts
    them). Pass --progid or --host to override.
  - Windows SSO is used by default (no username/password). On non-Windows
    or for non-default credentials, supply --username / --password and
    set --no-sso.

Usage:
  python mcp_driver.py [--host HOST] [--clsid CLSID] [--progid PROGID]
                       [--username U] [--password P] [--no-sso]
                       [--items Random.Int1,Random.Real8,Random.String]

Examples (PowerShell):
  python mcp_driver.py
  python mcp_driver.py --host 192.168.1.50 --progid Matrikon.OPC.Simulation.1
  python mcp_driver.py --items "Random.Int1,Random.Real8,Random.Boolean,Random.String"

Exit codes: 0 = all stages reached (read may fail with known issue noted above);
            non-zero = unexpected error (network/auth failure, etc.).
"""

from __future__ import annotations

import argparse
import json
import os
import shutil
import subprocess
import sys
import threading
import time
from typing import Any, Optional


DEFAULT_ITEMS = ["Random.Int1", "Random.Real8", "Random.Boolean", "Random.String"]


class McpClient:
    """Minimal JSON-RPC over stdio client for the MCP server."""

    def __init__(self, proc: subprocess.Popen[bytes]) -> None:
        self._proc = proc
        self._stdout = proc.stdout
        assert self._stdout is not None
        self._stdin = proc.stdin
        assert self._stdin is not None
        self._lock = threading.Lock()
        self._next_id = 1
        self._stderr_drain = threading.Thread(target=self._drain_stderr, daemon=True)
        self._stderr_drain.start()

    def _drain_stderr(self) -> None:
        assert self._proc.stderr is not None
        for line in iter(self._proc.stderr.readline, b""):
            sys.stderr.write("[mcp.stderr] " + line.decode("utf-8", errors="replace"))

    def _send(self, payload: dict[str, Any]) -> None:
        body = json.dumps(payload) + "\n"
        encoded = body.encode("utf-8")
        with self._lock:
            self._stdin.write(encoded)
            self._stdin.flush()

    def _read(self) -> dict[str, Any]:
        line = self._stdout.readline()
        if not line:
            raise RuntimeError("MCP server closed stdout.")
        return json.loads(line.decode("utf-8"))

    def initialize(self) -> None:
        """Performs the MCP initialize handshake before any tools/call requests."""
        self.request("initialize", {
            "protocolVersion": "2025-03-26",
            "capabilities": {},
            "clientInfo": {"name": "mcp_driver.py", "version": "1.0"},
        })
        # The "initialized" notification has no id and expects no response.
        self._send({"jsonrpc": "2.0", "method": "notifications/initialized", "params": {}})

    def request(self, method: str, params: Optional[dict[str, Any]] = None) -> Any:
        with self._lock:
            req_id = self._next_id
            self._next_id += 1
        self._send({"jsonrpc": "2.0", "id": req_id, "method": method, "params": params or {}})
        while True:
            msg = self._read()
            if msg.get("id") == req_id:
                if "error" in msg:
                    raise RuntimeError(f"{method} failed: {msg['error']}")
                return msg.get("result")

    def call_tool(self, name: str, arguments: dict[str, Any]) -> Any:
        # MCP tools/call returns { content: [{ type: "text", text: "<json>" }], ... }.
        # Unwrap the inner JSON for ergonomic use by the demo script.
        envelope = self.request("tools/call", {"name": name, "arguments": arguments})
        content = (envelope or {}).get("content") if isinstance(envelope, dict) else None
        if isinstance(content, list) and content:
            first = content[0]
            if isinstance(first, dict) and first.get("type") == "text":
                text = first.get("text") or ""
                try:
                    return json.loads(text)
                except json.JSONDecodeError:
                    return text
        return envelope


def launch_server(repo_root: str) -> subprocess.Popen[bytes]:
    """Run the MCP server in stdio mode from the repository's mcp/ project."""
    dotnet = shutil.which("dotnet")
    if dotnet is None:
        raise RuntimeError("`dotnet` not found in PATH. Install the .NET 10 SDK.")
    csproj = os.path.join(repo_root, "mcp", "Opc.Classic.Mcp", "Opc.Classic.Mcp.csproj")
    if not os.path.exists(csproj):
        raise RuntimeError(f"MCP project not found at {csproj}.")
    return subprocess.Popen(
        [dotnet, "run", "--project", csproj, "--no-build", "-c", "Debug"],
        stdin=subprocess.PIPE,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        cwd=repo_root,
    )


def banner(title: str) -> None:
    line = "=" * 68
    print(f"\n{line}\n  {title}\n{line}", flush=True)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--host", default="localhost")
    parser.add_argument("--progid", default="Matrikon.OPC.Simulation.1")
    parser.add_argument("--clsid", default="F8582CF2-88FB-11D0-B850-00C0F0104305",
                        help="OPC DA server CLSID. Default is Matrikon Simulation. Passing a CLSID lets the client skip OPCEnum (which has its own DCOM ACL).")
    parser.add_argument("--username", default=None)
    parser.add_argument("--password", default=None)
    parser.add_argument("--no-sso", action="store_true",
                        help="Disable Windows SSO (use explicit --username/--password).")
    parser.add_argument("--items", default=",".join(DEFAULT_ITEMS),
                        help="Comma-separated OPC item IDs to read.")
    args = parser.parse_args()

    repo_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    use_sso = not args.no_sso

    banner("Starting MCP server (dotnet run --project mcp/Opc.Classic.Mcp)")
    proc = launch_server(repo_root)
    try:
        time.sleep(2)
        client = McpClient(proc)
        client.initialize()

        banner("session.create")
        session = client.call_tool("opcclassic.session.create", {})
        session_id = session["sessionId"]
        print(f"sessionId = {session_id}", flush=True)

        banner("da.connect")
        connect_args: dict[str, Any] = {
            "sessionId": session_id,
            "host": args.host,
            "progId": args.progid,
            "useSso": use_sso,
        }
        if args.clsid:
            connect_args["clsid"] = args.clsid
        if args.username:
            connect_args["username"] = args.username
        if args.password:
            connect_args["password"] = args.password
        connected = client.call_tool("opcclassic.da.connect", connect_args)
        print(json.dumps(connected, indent=2, default=str), flush=True)

        banner("da.get_status")
        status = client.call_tool("opcclassic.da.get_status", {"sessionId": session_id})
        print(json.dumps(status, indent=2, default=str), flush=True)

        banner("da.read_items_by_id")
        item_ids = [s.strip() for s in args.items.split(",") if s.strip()]
        values = client.call_tool("opcclassic.da.read_items_by_id", {
            "sessionId": session_id,
            "itemIds": item_ids,
        })
        if not isinstance(values, list):
            print(f"read_items_by_id returned non-list response: {values!r}", flush=True)
        else:
            for v in values:
                if not isinstance(v, dict):
                    print(f"  (unexpected element: {v!r})", flush=True)
                    continue
                print(
                    f"  {v.get('itemName',''):>32s}  value={v.get('value')!r:<24s}"
                    f"  type={v.get('valueType')}  q=0x{v.get('quality',0):04X}"
                    f"  hr=0x{(v.get('hResult',0) & 0xFFFFFFFF):08X}",
                    flush=True,
                )

        banner("da.disconnect")
        client.call_tool("opcclassic.da.disconnect", {"sessionId": session_id})
        print("Done. All Track Y NDR/MInterfacePointer paths exercised against the live server.", flush=True)
        return 0
    finally:
        try:
            proc.terminate()
            proc.wait(timeout=5)
        except Exception:
            proc.kill()


if __name__ == "__main__":
    sys.exit(main())
