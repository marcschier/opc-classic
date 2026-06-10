# Opc.Classic.Mcp.Capture

Native OPC Classic network packet capture + DCE/RPC PDU decode for
the [Opc.Classic.Mcp](../Opc.Classic.Mcp) server. Inspired by
[`netcap`](https://github.com/marcschier/netcap); built fresh here so
the decoder reuses our existing
[`PduCodec`](../../src/Opc.Classic.Dcom/Transport/PduCodec.cs) +
[`OrpcEnvelope`](../../src/Opc.Classic.Dcom/Transport/OrpcEnvelope.cs)
and writes capture-derived `.hex` files in the same format as
`Opc.Classic.Diagnostics.OpcWireCapture`.

## API

| Type | Role |
|---|---|
| `ICaptureSource` | Abstraction for a network trace source. |
| `PcapCaptureSource` | SharpPcap-backed live NIC capture. Default BPF: TCP port 135 + dynamic ephemeral range (OPC DCOM). |
| `OpcWireCaptureSource` | Replays existing `.hex` dirs as if they were live frames. |
| `CaptureSession` | State machine (Starting → Running → Stopping → Completed/Failed → Disposed). |
| `CaptureSessionManager` | Owns the session registry, enforces the active-session + retention caps, LRU evicts. |
| `OpcDcomDecoder` | Stateful TCP reassembler + PDU dispatcher. Maps `CapturedPacket` → `DecodedOpcPdu`. |
| `DecodedOpcPdu` | Structured PDU view: iid, opnum, ipid, call_id, hresult, context list, result list. |
| `CaptureSummarizer` | Top-N roll-ups over decoded PDU streams. |
| `HexFormatBridge` | Writes capture-derived ORPC pairs as `.hex` files (same shape as `OpcWireCapture`). |
| `OrpcReplayTool` | Walks captured ORPC bodies through `NdrReader`; per-(IID,opnum) counts. |

## Hosting note

Lives outside `src/` because SharpPcap + PacketDotNet use reflection
patterns that the repo's AOT-strict rule (see
`src/Directory.Build.props`) would reject. The MCP host is a
self-contained single-file deployment, not NativeAOT — so the relaxed
AOT settings here are appropriate.

## Adopter cookbook

See [`interop/docs/network-capture.md`](../../interop/docs/network-capture.md)
for the end-to-end capture / decode / replay walkthrough, privilege
requirements, BPF filter examples, and Wireshark integration.
