# Opc.Classic Roadmap

Forward-looking work toward the first Opc.Classic release and beyond. For
documented capabilities of the current code, see [CONFORMANCE.md](CONFORMANCE.md);
for architecture, [ARCHITECTURE.md](ARCHITECTURE.md); for changes already
shipped, [CHANGELOG.md](../CHANGELOG.md).

## Toward the first release

These gates are required before the first stable tag.

- **NTLMv2 wire verification against a live Windows Server / Active Directory
  lab.** The self-contained NTLMv2 + Kerberos + SPNEGO stack is validated
  against MS-NLMP / MS-KILE / MS-SPNG test vectors, but full end-to-end
  verification needs a real Windows / AD environment outside the repository
  sandbox.
- **External third-party NTLMSSP crypto / security audit.** The hand-rolled
  MD4 + RC4 + NTLMv2 + channel binding code paths need an independent
  audit before the stack ships under a stable tag. See
  [docs/security/NTLMSSP_AUDIT_GUIDE.md](security/NTLMSSP_AUDIT_GUIDE.md)
  for the prepared audit surface.
- **Windows-container Docker test fleet execution + triage.** Sources and
  build wiring are in place under [interop/docker/](../interop/docker/);
  full fleet execution + result triage is still pending.

## Recently landed

- **Full-feature SimulationServer.** `samples/Opc.Classic.Samples.SimulationServer` now provides one simulated plant model across DA, AE, HDA, Batch, Commands, Cpx, DX, Security, Discovery, and XML-DA, with MCP tooling integration and optional DA/AE/HDA real TCP hosting via `--listen`.
- **Simulation DA cold-activation handler.** `SimulationActivationServer` / `SimulationActivationHost` can serve `IActivation::RemoteActivation`, register the activated DA dispatchers in `OpcObjectRegistry`, and return a spec-conformant `OBJREF_STANDARD` encoded by `OpcInterfaceRefCodec` so the client locates the activated IPID. Full authenticated cold-activation remains in progress because the managed listener still needs server-side NTLM bind handling.

## Known coverage gaps

Generated client and server DCOM projections cover the main DA / AE / HDA /
Batch / Commands / Cpx / DX / Security / Discovery / XML-DA paths. Current gaps are
concentrated in advanced COM interface-pointer returns, legacy / deprecated
OPC surfaces, optional vendor-specific payload shapes, and native SCM/EPM
activation plumbing.

### Runtime and CCW gaps

- **COM interface-pointer return codecs** for the broader set of enumerators,
  browse objects, event subscriptions, class factories, and connection points
  beyond the release-scope paths. The generator infrastructure for the
  proxy + dispatcher pair is in place; adding shapes is mechanical (per-IDL
  annotation, then regenerate).
- **Multi-out record generation** for Batch enumeration-set discovery
  (`IOPCEnumerationSets`) and DX configuration record arrays
  (`IOPCConfiguration::QueryDXConnections`).
- **Complex Data sample coverage.** The CPX type-conversion (§7),
  data-filter (§8), `OpcCpxAddressSpace`, and `OpcCpxItemProperties`
  integration are implemented, and the SimulationServer exposes CPX
  behavior. A standalone `samples/Opc.Classic.Samples.CpxServer` does
  not exist; vendor-specific XML payload carriers beyond the current
  dictionary / type / value helpers remain follow-up work.

### Compatibility and conformance gaps

- Native-client interoperability hardening — additional cross-vendor matrix
  coverage beyond the current Matrikon Simulation Server + OPC Foundation
  TestServer profiles.
- Server-side NTLM bind handling on `RpcServerConnectionProcessor`, so the
  SimulationServer cold-activation path can progress from anonymous-denied
  integration coverage to full authenticated activation.
- Endpoint Mapper / port 135 front-end and byte-correct native-client
  `DUALSTRINGARRAY` data-port publication for unmodified native cold
  activation.
- OPCEnum server hosting for managed server discovery, complementing the
  existing OPCEnum client/discovery path.
- Live Windows Server / Active Directory NTLMv2 verification (see
  the release gate above).
- External third-party NTLMSSP crypto / security review (see the release
  gate above).

### Open conformance follow-ups

Per-spec deep-dives at [`docs/conformance/`](conformance/) (one file per
spec) identify the following soft gaps that warrant follow-up work but
do not block any current OPC scenario. Each entry references the
per-spec doc that catalogued the gap.

Most of the original 13 follow-ups from the conformance review have
been closed in commit `9a77c9d7`; the items below are the residual
work that remains:

- **OPC DA 2.05a + DA 3.00 — Windows CCW IOPCServer::CreateGroupEnumerator
  still returns E_NOTIMPL.** Implementation requires building a
  Windows CCW `IEnumString` (for the `*_NAMES` scope) and `IEnumUnknown`
  (for the `*_CONNECTIONS` scope) backed by `IOpcDaServer.SnapshotGroupsAsync`.
  The pattern from `OpcEnumOpcItemAttributesCcw` applies (~400 lines per
  enumerator including ref-counted vtables). See
  [`docs/conformance/opc-da-2-05a.md`](conformance/opc-da-2-05a.md) §3
  and [`docs/conformance/opc-da-3-00.md`](conformance/opc-da-3-00.md) §3.

- **OpcResultId alias pattern (decided: keep as-is).** The
  `internal static readonly int E_NOINTERFACE = OpcResultId.NoInterface.Code;`
  per-file alias pattern in ~13 CCW files is intentional. The aliases
  give a short readable local symbol while still pointing at the single
  canonical source per MS-ERREF. The 2 remaining `private const int E_NOINTERFACE = unchecked((int)0x80004002);`
  declarations were migrated to the alias pattern in the Sprint A
  cleanup (commit follows). No further consolidation is planned.

## Capture engine enhancements

Follow-ups to the MCP capture surface (the `opcclassic.capture.*` tool
family in [mcp/Opc.Classic.Mcp.Capture/](../mcp/Opc.Classic.Mcp.Capture/)):

- **Per-spec auto-discover.** Plug the capture engine into
  `OpcEnumClient.ActivateServerListAsync` so `opcclassic.capture.start` can
  optionally take a target ProgID / CLSID, enumerate the activated DCOM
  endpoint via OPCEnum, learn the SCM-assigned data port, and tighten the
  BPF filter mid-capture to that specific port (today the operator hand-
  supplies `serverPorts`). Requires new `targetProgId` / `targetClsid` params
  on `CaptureStartRequest`, an `OpcEnumClient` integration call inside
  `CaptureSession.StartAsync`, and pcap mid-capture `SetFilter` plumbing on
  `PcapCaptureSource`.
- **Live-stream MCP transport.** Push decoded-PDU events as MCP
  `notifications/message` over the existing stdio transport so a client
  can subscribe to a running capture instead of polling `capture.tail`.
  Requires a new `opcclassic.capture.subscribe` tool that registers a
  subscription cursor, an `INotifyingCaptureSink` plumbed through
  `CaptureSession`, and per-session notification dispatch + back-pressure
  policy.
- **Authn-trailer unwrap (opt-in).** Developer-scenario decode of
  PKT_INTEGRITY / PKT_PRIVACY auth trailers when the operator supplies a
  known NTLM session key. Behind an opt-in flag (`--unwrap-auth` or
  equivalent) and prominently warned in docs: revealing wire payload of
  privacy-protected RPC calls is a sensitive operation. Requires a
  `NtlmSessionKey` input on `capture.decode_pdu` / `capture.tail` /
  `capture.replay`, a `NtlmAuthTrailerUnwrapper` helper that re-uses the
  existing managed NTLM unwrappers (passive mode, no session key
  derivation), and a doc section in
  [docs/security/THREAT_MODEL.md](security/THREAT_MODEL.md) covering the
  security implications.

## Future work after the first release

- **Per-group channel for `IConnectionPoint`** so that production callback
  delivery against Matrikon (and other servers that bind `IConnectionPoint`
  at the group object only) works end-to-end. The inbound listener side
  is shipped; this is the client-side follow-up. See
  [interop/docs/da-callbacks.md](../interop/docs/da-callbacks.md) for the
  documented limitation.
- **Additional spec extensions** (Web-DA, Compliance 2.0 if / when
  published by the OPC Foundation).
- **More native-server interoperability fixtures** for vendor-specific
  DA / AE / HDA behavior beyond the Matrikon + TestServer baseline.
- **Expanded XML-DA serializer coverage** for uncommon SOAP payload shapes
  and vendor-specific carriers.
- **Follow-up hardening from the external NTLMSSP audit**, if findings
  require API or behavior changes.
