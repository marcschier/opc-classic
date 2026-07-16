# Opc.Classic.Mcp.Capture

Native OPC Classic network packet capture + DCE/RPC PDU decode for
the [Opc.Classic.Mcp](../Opc.Classic.Mcp) server. Inspired by
[netcap](https://github.com/marcschier/netcap); built fresh here so
the decoder reuses our existing
`PduCodec` +
`OrpcEnvelope`
and writes capture-derived `.hex` files in the same format as
`OpcWireCapture`.

## API

| Type | Role |
|---|---|
| `ICaptureSource` | Abstraction for a network trace source. |
| `PcapCaptureSource` | SharpPcap-backed live NIC capture. Default BPF: TCP port 135 + dynamic ephemeral range (OPC DCOM). |
| `OpcWireCaptureSource` | Replays existing `.hex` dirs as if they were live frames. |
| `CaptureSession` | State machine plus atomic filter transitions, retained source segments, bounded named cursors, and target metadata. |
| `CaptureSessionManager` | Owns the session/notification registries, enforces active-session and retention caps, and LRU evicts. |
| `CaptureTargetResolver` | Normalizes host/ProgID/CLSID/connection strings, starts capture before OPCEnum/activation, and returns bindings/OXID/ports used to narrow the live filter. |
| `OpcDcomDecoder` | Stateful sequence-aware TCP reassembler + PDU dispatcher with optional NTLM integrity/privacy unwrap. |
| `CaptureFileProcessor` | Bounded single-handle pcap/pcapng reader with Ethernet IPv4/IPv6 TCP decode, gap/truncation status, and file replay. |
| `DecodedOpcPdu` | Structured PDU view: iid, opnum, ipid, call_id, hresult, context list, result list. |
| `CaptureSummarizer` | Top-N roll-ups over decoded PDU streams. |
| `HexFormatBridge` | Writes capture-derived ORPC pairs as `.hex` files (same shape as `OpcWireCapture`). |
| `OrpcReplayTool` | Walks captured ORPC bodies through `NdrReader`; per-(IID,opnum) counts. |

Live capture can begin with a broad DCOM filter before target resolution and
then transition atomically to discovered ports. Sources update the filter live
when supported or use a start-before-retire replacement that preserves the
session id, prior packet segments, decoder state, target metadata, and cursor
indexes.

Named tail cursors retain bounded, independent replay windows. MCP
notifications are advisory index/state/drop messages only; decoded bodies stay
behind the authoritative `capture.tail` call, and a slow notification consumer
cannot block capture.

External pcap/pcapng decode and replay enforce file, packet, PDU, failure-list,
and payload bounds. TCP segments are reordered by sequence number,
retransmissions are deduplicated, overlaps are merged, and gaps are reported
with bounded resynchronization.

## Hosting note

Lives outside `src/` because SharpPcap + PacketDotNet use reflection
patterns that the repo's AOT-strict rule (see
`Directory.Build`) would reject. The MCP host is a
self-contained single-file deployment, not NativeAOT — so the relaxed
AOT settings here are appropriate.

## Adopter cookbook

See [network-capture](../../interop/docs/network-capture.md)
for the end-to-end capture / decode / replay walkthrough, privilege
requirements, BPF filter examples, and Wireshark integration.

Developer-only NTLM auth-trailer unwrap is documented in
[docs/capture/ntlm-unwrap.md](../../docs/capture/ntlm-unwrap.md).
