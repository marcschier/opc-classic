# Network packet capture for the Opc.Classic MCP server

The `opcclassic.capture.*` MCP tool set lets an MCP-aware client
(probe driver, LLM agent, IDE plugin) record raw OPC Classic
DCOM-over-IP traffic, decode the DCE/RPC PDU stream against our
existing `PduCodec` + `OrpcEnvelope` + `NdrReader`, and feed the
result back into the same offline diagnostic pipeline used by
probe servers tool and the replay tests
under Replay tests.

This unblocks debugging that otherwise required hand-crafting test
fixtures from stack traces: any failing live interaction can be
captured once, replayed locally an unlimited number of times, and the
captured `.pcap` can be opened in Wireshark for byte-level inspection.

## Tools

| Tool | Description |
|------|-------------|
| `opcclassic.capture.list_interfaces` | Enumerate NICs available for capture. |
| `opcclassic.capture.start` | Begin a capture session. Default BPF filter targets TCP port 135 + the dynamic ephemeral range (the OPC DCOM universe). |
| `opcclassic.capture.stop` | Stop and finalise. After return, the trace is safe to read. |
| `opcclassic.capture.list` | List capture sessions; filter by state. |
| `opcclassic.capture.get` | Return the trace as `dcom` (decoded PDU view, default), `json` (raw decoded PDUs), or `pcap-path` (path to the libpcap file for Wireshark). |
| `opcclassic.capture.summarize` | Top-N talkers, ports, IIDs, opnums, IPIDs, fault codes, and bind-reject reasons. |
| `opcclassic.capture.decode_pdu` | One-off raw-bytes → structured PDU decode for ad-hoc analysis. |
| `opcclassic.capture.replay` | Walk captured ORPC bodies through our codecs and report per-(IID,opnum) decode counts. |
| `opcclassic.capture.remove` | Stop (if needed) + dispose a capture session. |

## Privileges

- **Windows**: the MCP server process must run as Administrator and
  [Npcap](https://npcap.com/) must be installed (libpcap-compatible).
  Without elevation, `list_interfaces` reports an empty list and
  `start` fails with an actionable error.
- **Linux**: the process needs `CAP_NET_ADMIN` + `CAP_NET_RAW` (or
  root). Distributions package this via the `libpcap` package.
- **macOS**: root (or `chmod +r /dev/bpf*` on a specific BPF device).

## Default BPF filter

`tcp and (port 135 or (portrange 49152-65535))` — covers the SCM
(port 135 / endpoint mapper, used by OPCEnum activation +
`IRemoteSCMActivator`) plus the IANA dynamic / private port range that
Windows hands out to DCOM-launched OPC servers.

Override with the `bpfFilter` parameter on
`opcclassic.capture.start`. Examples:

- `host opc.example.com and port 57539` — pin to one server's data
  port.
- `tcp and port 135` — SCM only (activation traffic).
- `host 10.0.1.42 and tcp` — all TCP to/from a specific peer.

## Output formats

### `dcom` (default)

Human-readable per-PDU summary:

```text
# Opc.Classic capture session 4d8f… — 17 PDUs
2026-06-04T12:34:56.789Z  bind                 10.0.1.10:51234  -> 10.0.1.20:135     call_id=1
   ctx[0] iid=000001a0-0000-0000-c000-000000000046 ver=0.0
2026-06-04T12:34:56.812Z  bind_ack             10.0.1.20:135    -> 10.0.1.10:51234   call_id=1
   result[0] ACCEPTANCE; REASON_NOT_SPECIFIED
2026-06-04T12:34:56.823Z  request              10.0.1.10:51234  -> 10.0.1.20:135     call_id=2
   iid=000001a0-0000-0000-c000-000000000046  opnum=4  ipid=-
```

### `json`

Array of `DecodedOpcPdu` records — suitable for piping into `jq` or
storing as a regression fixture.

### `pcap-path`

Returns the absolute path to the underlying libpcap file. Open in
Wireshark:

```bash
wireshark "$(opc-classic-mcp call opcclassic.capture.get sessionId=4d8f… format=pcap-path)"
```

## Replay workflow

1. Start a capture against the failing peer:
   ```text
   opcclassic.capture.start interfaceName=eth0 bpfFilter="host matrikon and tcp"
   ```
2. Run the failing probe / driver against the live server.
3. Stop + summarize:
   ```text
   opcclassic.capture.stop sessionId=<id>
   opcclassic.capture.summarize sessionId=<id>
   ```
4. Read the per-PDU view:
   ```text
   opcclassic.capture.get sessionId=<id> format=dcom maxPdus=500
   ```
5. Feed any bad call back through `opcclassic.capture.replay` to
   confirm the codec rejection is reproducible offline.
6. (Optional) Open the `.pcap` in Wireshark via `format=pcap-path`
   for byte-level investigation.

## Inspired by

[`netcap`](https://github.com/marcschier/netcap) — pluggable
`ICaptureSource`, session manager, format registry, MCP tool surface.
Built native here so the decoder reuses
`Opc.Classic.Dcom.Transport.PduCodec` and produces `.hex` files
matching the existing `OpcWireCapture` convention, keeping live
captures, replay tests, and probe-driver wire dumps interoperable.

## Limitations

- **Encrypted traffic** (PKT_PRIVACY) cannot be decoded — the
  decoder reports the encapsulating PDU type but the body is opaque.
  Capture against PKT_INTEGRITY or no-auth flows to see the
  application payload.
- **Single interface per session** — libpcap binds one NIC. Use
  multiple sessions in parallel to capture multiple NICs.
- **In-memory sessions** — restarting the MCP host loses sessions
  and deletes their scratch folders. Export interesting captures via
  `format=pcap-path` first.
- **Bounded by the engine caps** — 50 MB / 30 min / 8 active /
  32 retained by default. Override on `start` with `maxBytes`,
  `maxPackets`, `maxDurationSeconds`.

## Related

- [`OpcWireCapture` / `.hex` format](wire-captures/README.md) —
  application-layer per-call dumps from our managed client. The
  capture engine writes the same format for matched request/response
  pairs found in live traffic.
- Replay harness
  — turns `.hex` dumps back into `byte[]` for regression tests.
- `PduCodec` +
  `OrpcEnvelope`
  — the same codecs used by our managed client + server, exercised by
  the capture decoder so any fix here also fixes the live code path.
