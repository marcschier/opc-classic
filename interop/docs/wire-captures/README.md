# Opc.Classic wire captures

This directory is the default landing zone for opt-in NDR wire captures produced
by the managed DCOM stack's diagnostic decorator (Track AK2). Each call to
`ICallChannel.InvokeAsync` writes a self-describing hex dump per call when
the `OPCCLASSIC_WIRE_CAPTURE_DIR` environment variable is set, or when
`tools/probe_servers.py --save-wire-payloads <dir>` is used.

## What the files look like

Each file is named:

```
<UTC timestamp>_<sequence>_<context tag>_iid-<iid>_op-<opnum>.hex
```

Contents:

```
# Opc.Classic wire capture
# context: da-localhost-Matrikon.OPC.Simulation.1
# iid:     39c13a4d-011e-11d0-9675-0020afd8adb3
# opnum:   3
# hresult: 0x00000000
# timestamp_utc: 20260603T194501.123

## request (148 bytes)
0000:  04 00 00 00 00 00 02 00  04 00 00 00 ... | hex + ascii gutter
...

## response (240 bytes)
0000:  ...
```

## How to enable

Either run the probe with the flag:

```powershell
python tools\probe_servers.py --da-clsid F8582CF2-88FB-11D0-B850-00C0F0104305 `
    --save-wire-payloads interop\docs\wire-captures
```

Or set the environment variable directly when invoking the MCP server:

```powershell
$env:OPCCLASSIC_WIRE_CAPTURE_DIR = "interop\docs\wire-captures"
dotnet run --project mcp\Opc.Classic.Mcp
```

## Privacy / repo hygiene

`.gitignore` keeps `.hex` files out of the repository by default — they
typically contain server-specific data (item IDs, values) that should not
land in version control. Commit a specific capture only when it is needed
as a regression fixture; when doing so, scrub server-identifying data
first.

## Related

- [`tests/Opc.Classic.Da.Tests/Wire/Replay/WireCaptureFile.cs`](../../../tests/Opc.Classic.Da.Tests/Wire/Replay/WireCaptureFile.cs) — the round-trip parser that turns a `.hex` capture back into a `byte[]` for replay-style regression tests (Track AK3).
- [`IOPCDataCallback` push delivery](../da-callbacks.md) — when callbacks are wired up, both request AND inbound callback PDUs land here.
- [OPCEnum DCOM auth](../opcenum-auth.md) — the most common source of "interesting" wire captures during initial bring-up.
- [Network packet capture](../network-capture.md) — Track CA adds an `opcclassic.capture.*` MCP tool surface that captures live OPC DCOM traffic at the network layer (Wireshark-compatible `.pcap` + decoded DCE/RPC PDU view). Capture-derived ORPC pairs are written into this directory as `live-<sessionId>/<file>.hex` so they flow into the same replay harness.
