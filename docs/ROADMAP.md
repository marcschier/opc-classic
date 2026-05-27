# Opc.Classic Roadmap

This document tracks what's planned beyond the current release. For implemented features, see [CHANGELOG](../CHANGELOG.md).

Current release candidate: **1.0.0-rc.7** (annotated local tag). The `1.0.0` FINAL tag waits on the gates in [release-blockers.md](release-blockers.md).

## 1.0.0 FINAL

Delivered during the rc.1..rc.7 cycle:

- [x] Cross-platform DCOM transport: `OpcServerListener`, `RpcServerConnectionProcessor`, and `OpcObjectRegistry`.
- [x] Windows CCW DA path with real `IOPCServer`, group, item-management, sync/async I/O, connection-point, callback, VARIANT/SAFEARRAY/BSTR, and item-attribute enumerator bodies.
- [x] Windows CCW AE/HDA parity for release-scope server, subscription/status, and handle-management methods.
- [x] DA address-space abstraction (`IOpcAddressSpace`, browse, properties, deadband, sampling defaults).
- [x] Unified outbound data-callback delivery through `IOpcDataCallbackSink`.
- [x] NTLMSSP audit-prep guide, NTLM wire fixtures, WINREG fixtures, and Docker native C server/client MVP source/build wiring.
- [x] Sample DCOM-over-IP path: sample servers bind configurable TCP ports and sample clients dial TCP from environment variables.
- [x] Release-candidate administration through `1.0.0-rc.7`; `[Unreleased]` in `CHANGELOG.md` is intentionally empty.

Remaining open gates are environment/process-bound and are tracked with owners in [release-blockers.md](release-blockers.md):

- [ ] OPC CTT smoke green on a Windows Docker host (`release-100-tag`).
- [ ] NTLMv2 wire verification against a live Windows Server / AD lab (`rw-e1-ntlmv2-realserver`).
- [ ] External third-party NTLMSSP crypto/security audit (`rw-e4-ntlm-audit`).

## 2.0.0 and beyond

- Broader generated coverage for COM interface-pointer returns and enumerator-producing methods.
- Additional spec extensions (Web-DA, Compliance 2.0 if/when published).
- More native-server interoperability fixtures for vendor-specific DA/AE/HDA behavior.
- Expanded XML-DA serializer coverage for uncommon SOAP payload shapes and vendor-specific carriers.
- Follow-up hardening from the external NTLMSSP audit, if findings require API or behavior changes.

## Known coverage gaps

Generated client and server DCOM projections cover the main DA/AE/HDA, Batch, Commands, Cpx, DX, Security, and Discovery paths. The current gaps are concentrated in advanced COM interface-pointer returns, legacy/deprecated OPC surfaces, and a few CCW-side payload shapes.

### Runtime and CCW gaps

- Full `IEnumConnections` / `IEnumConnectionPoints` CCW infrastructure remains future work; current connection-point coverage supports release-scope `Advise`, `Unadvise`, and `FindConnectionPoint` paths.
- `IOPCAsyncIO3.WriteVqt` remains deferred on the Windows CCW side.
- AE `IOPCEventServer.CreateEventSubscription` / advanced `EVENTFILTER` marshaling and HDA `IOPCHDA_SyncRead` / `IOPCHDA_AsyncRead` `OPCHDA_ITEM[]` read marshaling remain CCW-side follow-ups.
- COM interface-pointer return codecs are still needed for the broad set of enumerators, browse objects, event subscriptions, class factories, and connection points beyond the release-scope paths.
- Additional multi-out record generation is still useful for less common AE catalog/state calls, Batch enumeration-set discovery, and DX configuration record arrays.
- Complex Data dictionary, type-description, binary, and XML payload codecs remain limited to the current metadata projections.

### Compatibility and conformance gaps

- The Docker test fleet source and build wiring are in place, but Windows-container execution and CTT triage are still required before the final tag.
- Live Windows Server / Active Directory NTLMv2 verification remains outside the sandbox and is tracked in `docs\release-blockers.md`.
