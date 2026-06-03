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
- [x] OPCEnum activation auth (Track AE — `ee2425c2`) + AppID ACL helper (Track AN — `1a1de5db`).
- [x] NDR wire-trace diagnostic infrastructure (Track AK1 hex-context + AK2 wire capture + AK3 replay parser).
- [x] Byte-exact request/response/server-dispatch wire fixtures (Track AL).
- [x] `IEnumOPCItemAttributes::Next` `pceltFetched` correctness (Track AM — `1b059d0a`).
- [x] `IOPCDataCallback` subscription queue surface + bounded sink + drain-or-pull (Track AP3/AP5/AP6 — `083c5437`).
- [ ] OPC CTT smoke green on a Windows Docker host (`release-100-tag`).
- [ ] NTLMv2 wire verification against a live Windows Server / AD lab (`rw-e1-ntlmv2-realserver`).
- [ ] External third-party NTLMSSP crypto/security audit (`rw-e4-ntlm-audit`).
- [ ] Production inbound `IOPCDataCallback` listener bring-up (Track AP1/AP2/AP4) — requires Matrikon-reachable verification environment.

## Known coverage gaps

Generated client and server DCOM projections cover the main DA/AE/HDA, Batch, Commands, Cpx, DX, Security, and Discovery paths. The current gaps are concentrated in advanced COM interface-pointer returns, legacy/deprecated OPC surfaces, and optional vendor-specific payload shapes.

### Runtime and CCW gaps

- COM interface-pointer return codecs are still needed for the broad set of enumerators, browse objects, event subscriptions, class factories, and connection points beyond the release-scope paths.
- Additional multi-out record generation is still useful for Batch enumeration-set discovery and DX configuration record arrays.
- Complex Data conversion/filter engines and vendor-specific XML payload carriers remain future work beyond the current dictionary/type/value helpers.

### Compatibility and conformance gaps

- The Docker test fleet source and build wiring are in place, but Windows-container execution and CTT triage are still required before the final tag.
- Live Windows Server / Active Directory NTLMv2 verification remains outside the sandbox.
- External third-party NTLMSSP crypto/security review remains outside the sandbox.

## Future work after 1.0.0

- Additional spec extensions (Web-DA, Compliance 2.0 if/when published).
- More native-server interoperability fixtures for vendor-specific DA/AE/HDA behavior.
- Expanded XML-DA serializer coverage for uncommon SOAP payload shapes and vendor-specific carriers.
- Follow-up hardening from the external NTLMSSP audit, if findings require API or behavior changes.
