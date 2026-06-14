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

## Known coverage gaps

Generated client and server DCOM projections cover the main DA / AE / HDA /
Batch / Commands / Cpx / DX / Security / Discovery paths. Current gaps are
concentrated in advanced COM interface-pointer returns, legacy / deprecated
OPC surfaces, and optional vendor-specific payload shapes.

### Runtime and CCW gaps

- **COM interface-pointer return codecs** for the broader set of enumerators,
  browse objects, event subscriptions, class factories, and connection points
  beyond the release-scope paths. The generator infrastructure for the
  proxy + dispatcher pair is in place; adding shapes is mechanical (per-IDL
  annotation, then regenerate).
- **Multi-out record generation** for Batch enumeration-set discovery
  (`IOPCEnumerationSets`) and DX configuration record arrays
  (`IOPCConfiguration::QueryDXConnections`).
- **Complex Data conversion / filter engines** and vendor-specific XML
  payload carriers beyond the current dictionary / type / value helpers.

### Compatibility and conformance gaps

- Native-client interoperability hardening — additional cross-vendor matrix
  coverage beyond the current Matrikon Simulation Server + OPC Foundation
  TestServer profiles.
- Live Windows Server / Active Directory NTLMv2 verification (see
  the release gate above).
- External third-party NTLMSSP crypto / security review (see the release
  gate above).

### Open conformance follow-ups

Per-spec deep-dives at [`docs/conformance/`](conformance/) (one file per
spec) identify the following soft gaps that warrant follow-up work but
do not block any current OPC scenario. Each entry references the
per-spec doc that catalogued the gap.

- **OPC AE 1.10 — IOPCCommon carry-over on AE EventServer.** The AE
  host does not expose `IOPCCommon` (locale / error-text / client-name)
  on the EventServer object. Track in
  [`docs/conformance/opc-ae-1-10.md`](conformance/opc-ae-1-10.md) §3.
- **OPC AE 1.10 — server-level IConnectionPointContainer / IOPCShutdown
  connection point.** AE EventServer lacks the
  `FindConnectionPoint(IID_IOPCShutdown)` tearoff on the root object.
  See [`docs/conformance/opc-ae-1-10.md`](conformance/opc-ae-1-10.md) §3.
- **OPC DA 2.05a — Windows CCW IOPCServer::CreateGroupEnumerator
  returns E_NOTIMPL.** Needs `IEnumUnknown` group-enumerator CCW. See
  [`docs/conformance/opc-da-2-05a.md`](conformance/opc-da-2-05a.md) §3.
- **OPC DA 2.05a — Windows CCW IOPCCommon tearoff returns E_NOTIMPL
  for locale + error-string methods.** Needs routing to
  `IOpcDaServer` / `IOPCCommon` plus CCW locale / error-string tests.
  See [`docs/conformance/opc-da-2-05a.md`](conformance/opc-da-2-05a.md) §3.
- **OPC DA 2.05a — Windows DA root server CCW lacks
  IConnectionPointContainer for IOPCShutdown callbacks.** Add the
  root-server connection-point container + point for
  `IID_IOPCShutdown`. See
  [`docs/conformance/opc-da-2-05a.md`](conformance/opc-da-2-05a.md) §3.
- **OPC DA 3.00 — top-level IOPCItemIO not registered by the default
  managed DA host.** Needs an `IOPCItemIO` dispatcher in
  `OpcDaServerHost` routing `Read` / `WriteVQT` to
  `IDaServer` / `IOpcDaServer` item I/O. See
  [`docs/conformance/opc-da-3-00.md`](conformance/opc-da-3-00.md) §3.
- **OPC DA 3.00 — OPCServer IConnectionPointContainer for IOPCShutdown
  not verified as wired.** Add server-level
  `IConnectionPointContainer` / `IOPCShutdown` connection point
  support and tests. See
  [`docs/conformance/opc-da-3-00.md`](conformance/opc-da-3-00.md) §3.
- **OPC DA 3.00 — Windows CCW IOPCServer::CreateGroupEnumerator
  returns E_NOTIMPL.** Implement `IEnumUnknown` / `IEnumString` group
  enumeration in `OpcDaServerCcw`. See
  [`docs/conformance/opc-da-3-00.md`](conformance/opc-da-3-00.md) §3.
- **OPC DX 1.00 — IOPCConfiguration::DeleteDXConnections wire shape
  mismatch.** Currently projected as connection names returning
  `HRESULT[]` only; spec §5.2.2.5 / App. B.1.4 requires
  `DXConnection[]` masks and a `GeneralResponse` with
  `ConfigurationVersion` and `IdentifiedResult` entries. See
  [`docs/conformance/opc-dx-1-00.md`](conformance/opc-dx-1-00.md) §3.
- **OPC HDA 1.20 — IConnectionPointContainer::EnumConnectionPoints
  returns E_NOTIMPL.** Should enumerate the `IOPCHDA_DataCallback`
  connection point. See
  [`docs/conformance/opc-hda-1-20.md`](conformance/opc-hda-1-20.md) §3.
- **OPC HDA 1.20 — IConnectionPoint::EnumConnections returns
  E_NOTIMPL.** Implement `IEnumConnections` over the HDA CCW sink
  cookie table. See
  [`docs/conformance/opc-hda-1-20.md`](conformance/opc-hda-1-20.md) §3.
- **OPC HDA 1.20 — HDA IOPCCommon + shutdown carry-over exposure.**
  Expose / verify `IOPCCommon` and shutdown callback support on the
  HDA root object or document the cross-spec host mechanism. See
  [`docs/conformance/opc-hda-1-20.md`](conformance/opc-hda-1-20.md) §3.
- **OPC HDA 1.20 — HdaAggregate enum missing standard identifiers
  through OPCHDA_ANNOTATIONS.** Add missing standard HDA aggregate
  helper values + tests, preserving raw-int aggregate-ID wire
  compatibility. See
  [`docs/conformance/opc-hda-1-20.md`](conformance/opc-hda-1-20.md) §3.

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
