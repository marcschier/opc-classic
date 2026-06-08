# Opc.Classic Roadmap

This document tracks what's planned beyond the current release. For implemented features, see [CHANGELOG](../CHANGELOG.md).

## 1.0.0

- [x] Cross-platform DCOM transport: `OpcServerListener`, `RpcServerConnectionProcessor`, and `OpcObjectRegistry` with per-IPID object routing.
- [x] Windows CCW DA path with real `IOPCServer`, group, item-management, sync/async I/O, connection-point, callback, VARIANT/SAFEARRAY/BSTR, item-attribute enumerator, and continuation-token bodies.
- [x] Windows CCW AE/HDA parity for release-scope server, subscription/status, handle-management, AE array-marshaling, HDA update, advise, annotation, and playback methods.
- [x] DA address-space abstraction (`IOpcAddressSpace`, browse, properties, deadband, sampling defaults) plus CPX address-space/property integration.
- [x] Unified outbound data-callback delivery through `IOpcDataCallbackSink`.
- [x] NTLMSSP audit-prep guide, NTLM wire fixtures, WINREG fixtures, and Docker native C server/client MVP source/build wiring.
- [x] Sample DCOM-over-IP path: sample servers bind configurable TCP ports and sample clients dial TCP from environment variables.
- [x] OPC Security reference sample and cookbook guidance.
- [x] Release-candidate administration through `1.0.0-rc.10`; `[Unreleased]` in `CHANGELOG.md` tracks post-rc.10 NDR completeness work (Tracks AF/AG/AH/AI/AJ/AK/AL/AM/AN/AP).
- [x] NDR wire-format completeness for Matrikon DA (Tracks AF/AG/AH/AI — `dfbf234b`, `3a1ba9c3`).
- [x] Pre-bind IID set + AlterContext for the full DA spec (Track AC — `2d96d8f9`).
- [x] OPCEnum activation auth (Track AE — `ee2425c2`) + AppID ACL helper (Track AN — `1a1de5db`) + OPCEnum bind regression closed (Track BG — `74eba65d`, `discovery.enumerate_servers` and `--da-progid` activation both work).
- [x] NDR wire-trace diagnostic infrastructure (Track AK1 hex-context + AK2 wire capture + AK3 replay parser).
- [x] Byte-exact request/response/server-dispatch wire fixtures (Track AL).
- [x] `IEnumOPCItemAttributes::Next` `pceltFetched` correctness (Track AM — `1b059d0a`).
- [x] `IOPCDataCallback` subscription queue surface + bounded sink + drain-or-pull (Track AP3/AP5/AP6 — `083c5437`).
- [x] Matrikon `IOPCItemProperties::GetProperties` decode (Track AY+ — `6a8f32ce`); `OPCITEMSTATE` decode (Track AY++ — `7fce8b45`); live 26/95 OK probe baseline against Matrikon Simulation Server.
- [x] Production inbound `IOPCDataCallback` listener bring-up (AP1/AP2/AP4 closed by Track BI — `41e30ca7`): `IObjectExporterDispatcher` registered at the well-known IID on the `DaCallbackEndpoint`, `DaClientTools.Subscribe` wires the Advise/Unadvise cycle. **Note**: Matrikon-specific group-channel limitation for IConnectionPoint documented in `docs/interop/da-callbacks.md`; production push-callback delivery against Matrikon needs a follow-up per-group channel that pre-binds IConnectionPoint(Container).
- [x] TestServer registration spec + script alignment (Track BH1+BH2+BH3 — `9d6ed944`): canonical WiX-derived `docs/interop/testserver-registration-spec.md`; `external/tools/register-testserver.ps1` mirrors the full 8-DLL MSI install order; `external/tools/grant-testserver-acl.ps1` automates the DCOM Launch/Access ACL grant that is the actual cause of `CO_E_SERVER_EXEC_FAILURE`.
- [ ] OPC CTT smoke green on a Windows Docker host (`release-100-tag`).
- [ ] NTLMv2 wire verification against a live Windows Server / AD lab (`rw-e1-ntlmv2-realserver`).
- [ ] External third-party NTLMSSP crypto/security audit (`rw-e4-ntlm-audit`).

## Known coverage gaps

Generated client and server DCOM projections cover the main DA/AE/HDA, Batch, Commands, Cpx, DX, Security, and Discovery paths. The current gaps are concentrated in advanced COM interface-pointer returns, legacy/deprecated OPC surfaces, and optional vendor-specific payload shapes.

### Runtime and CCW gaps

- COM interface-pointer return codecs are still needed for the broad set of enumerators, browse objects, event subscriptions, class factories, and connection points beyond the release-scope paths.
- Additional multi-out record generation is still useful for Batch enumeration-set discovery and DX configuration record arrays.
- Complex Data conversion/filter engines and vendor-specific XML payload carriers remain future work beyond the current dictionary/type/value helpers.

#### Track BJ3 triage — 1.0.0 vs post-1.0.0

The interface-pointer return codecs and multi-out record support
(Tracks BJ1 + BJ2) are NOT required for 1.0.0 because:

1. **The release-scope paths already work end-to-end against Matrikon
   Simulation Server** (26/95 OK, zero DA failures, all DA tools pass
   including subscribe + callback wireup). The interface-pointer
   codecs listed under "still needed" cover branches the release-scope
   probes don't exercise (Batch enumeration-set discovery,
   `IConnectionPointContainer::EnumConnectionPoints`,
   `IOPCEventSubscriptionMgt::CreateEventSubscription`,
   `IClassFactory::CreateInstance` for out-of-process activation,
   etc.).
2. **All Matrikon-blocking codec bugs were fixed in Tracks AY+/AY++**
   — the remaining interface-pointer gaps are for less-common paths
   that would matter only when probing additional servers (HDA / AE /
   Batch / Commands / DX), which themselves need Track BH4-BH7 to
   bring up. The relative work for BJ1 + BJ2 in isolation is large
   and would not move the 1.0.0 quality bar.
3. **The generator infrastructure that would emit these codecs is
   already in place** (`Opc.Classic.Generators` ships incremental
   Roslyn generators for proxy + dispatcher pairs). Adding the
   additional shapes is mechanical — a follow-up adding the IDL
   annotation per missing method + running the generator.

**Decision**: Move BJ1 + BJ2 to "Future work after 1.0.0" as
`opt-in spec broadening` and ship 1.0.0 with the current release-scope
surface. Code-level cookbook entries in `docs/generators/` already
exist for the contributors who need to extend the surface.

### Compatibility and conformance gaps

- The Docker test fleet source and build wiring are in place, but Windows-container execution and CTT triage are still required before the final tag.
- Live Windows Server / Active Directory NTLMv2 verification remains outside the sandbox.
- External third-party NTLMSSP crypto/security review remains outside the sandbox.

## Future work after 1.0.0

- **Additional COM interface-pointer return codecs** (post-Track BJ1)
  for enumerators, browse objects, event subscriptions, class
  factories, and connection points beyond the current release-scope.
- **Generator support for multi-out record arrays** (post-Track BJ2)
  for Batch `IOPCEnumerationSets` + DX
  `IOPCConfiguration::QueryDXConnections` parallel out arrays.
- **Per-group channel for IConnectionPoint** so that production
  callback delivery against Matrikon (and other servers that bind
  IConnectionPoint at the group object only) works end-to-end. The
  Track BI work shipped the listener side; this is the client-side
  follow-up.
- Additional spec extensions (Web-DA, Compliance 2.0 if/when published).
- More native-server interoperability fixtures for vendor-specific DA/AE/HDA behavior.
- Expanded XML-DA serializer coverage for uncommon SOAP payload shapes and vendor-specific carriers.
- Follow-up hardening from the external NTLMSSP audit, if findings require API or behavior changes.
