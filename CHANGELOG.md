# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

(nothing new since 0.3.0-alpha.1)

## [0.3.0-alpha.1] - 2026-05-23

### Removed

- **SharpCifs.Std (LGPL-2.1) transitional dependency fully dropped.** The OpcClassic .NET 10 assembly tree is now license-clean under EPL-1.0 with no LGPL transitive runtime dependencies.

### Changed

- **N7.6 FINAL**: `OpcClassic.Dcom.Internal.Ntlm.NtlmMessage` + `Type1Message` + `Type2Message` + `Type3Message` reimplemented self-contained per MS-NLMP §2.2.1.1-3. The Phase 2I MS-NLMP §4.2.4.1 spec test vectors continue to pass, proving wire compatibility with the SharpCifs implementation that was replaced.
- **N14B prep**: `COM/README.md` documents the native C++ sample server build process (MSBuild + Windows SDK + OPC Foundation Core Components). `COM/regserver.cmd` registers all three (DA/AE/HDA) sample servers. Phase 14B test scaffolds updated with verified CLSIDs/ProgIDs from the C++ sources.

### Status

- 1.0.0 Gate 2 (clear LGPL dep) — **MET** ✅
- 1.0.0 Gate 1 (compat matrix) — preparation complete; actual native build + conformance runs require Windows runner with VS 2022 Build Tools + Windows SDK + OPC Foundation Core Components installer (the `windows-conformance` CI job is wired to perform this when assets are available)
- 1.0.0 Gate 3 (OPC CTT) — externally blocked on OPC Foundation membership + `OPC_CTT_INSTALLER_URL` CI secret (workflow scaffold ready in `.github/workflows/opc-ctt.yml`)

## [0.2.0-alpha.1] - 2026-05-23

### Added

- **N1.1**: Real `DcomCallChannel : ICallChannel` over `IAsyncTransport` — full DCOM bind PDU + RequestCoPdu/ResponseCoPdu pipeline + fragmentation + IAuthContext abstraction
- **N1.2**: LocalCoClass modernization — BackgroundService accept loop + Channel<IAsyncTransport> worker queue + IDispatchTable expansion
- **N2**: Codec registry supports conformant arrays of primitives + complex types via OpcProxyGenerator
- **N3+N4+N5**: Per-method generator coverage applied across ~90 methods on ~30 OPC interfaces (DA + AE + HDA + Cpx + DX + Batch + Commands + Security)
- **N6**: Server-side per-method dispatchers (OpcDaServerDispatcher + AE + HDA) routing RequestCoPdu payloads to IOpcDaServer/IOpcAeServer/IOpcHdaServer impls
- **N8**: KerberosAuthContext wires Phase 3D KDC + Phase 3E SPNEGO + Phase 3F channel-binding into the auth flow via IAuthContext
- **N9.1**: `samples/OpcClassic.CttServer/` — minimal CTT-compliant managed DA server registered as `OpcClassic.DaSample.1`
- **N9.2**: Phase 14B/C/D loopback test variants exercise the full client→server pipeline via InMemoryCallChannel + OpcDaServerDispatcher

### Changed

- **N7.2**: SharpCifs.Util replaced with BCL (`Convert.ToHexString`, `Encoding.*`)
- **N7.3**: `SharpCifs.Smb.NtlmPasswordAuthentication` replaced with `System.Net.NetworkCredential`
- **N7.4**: `SharpCifs.Ntlmssp` types vendored behind `OpcClassic.Dcom.Internal.Ntlm` forwarding wrappers
- **N7.5**: `SharpCifs.Dcerpc.Ndr` types vendored behind `OpcClassic.Dcom.Internal.LegacyNdr` forwarding wrappers — 74 file migrations
- **N7.6** (partial): Self-contained `NdrException`; full reimpl of remaining wrappers (~1050 LOC) tracked in `src/OpcClassic.Dcom/Common/SharpCifsBoundary.md`

### Security

- Phase 4 + 6 call-shim pipeline now production-grade: real wire transport (DcomCallChannel), real per-method dispatch (server + client), proper NDR codec coverage including arrays
- DCOM defaults remain INTEGRITY + NTLMv2 (Phase 3B + 3C)
- Kerberos integration paths exercised through the new IAuthContext seam

### Known limitations

- `SharpCifs.Std` (LGPL-2.1) still a transitional runtime dep — full drop is N7.6 follow-up (~1050 LOC reimpl tracked in `SharpCifsBoundary.md`)
- Phase 14B/C/D real-server tests soft-skip when prerequisites missing (native COM build, Matrikon installer, OPC CTT) — loopback variants verify the test machinery
- 1.0.0 release waits for compat matrix GREEN against real native servers (Phase 14D follow-up)

## [0.1.0-alpha.2] - 2026-05-22

### Added

- Phase 4 + Phase 6 call-shim pipeline: OpcProxyGenerator emits real
  `ICallChannel.InvokeAsync` bodies for `[OpcMethod]` methods on
  decorated `[OpcInterface]` partials. First real application:
  IOPCServer (Phase 6B template), IOPCGroupStateMgt + IOPCItemIO
  (Phase 6C).
- Codec registry covers 32+ types: primitives + OpcVariant + OpcSafeArray
  + all 21 spec struct codecs.
- OpcClassic.Hosting end-to-end: IClsidRegistry + IOpcServerHost +
  Microsoft.Extensions.Hosting integration. AddOpcDaServer<T>(configure)
  registers a DA server implementation.
- OpcClassic.Discovery: LocalEnum (full impl), OpcEnum/RemoteRegistry
  scaffolds, OpcDiscoveryFactory composite with CLSID dedup.
- OpcClassic.Dcom.Kerberos: KerberosConnectionContext with real
  Kerberos.NET 4.6.146 integration (AP-REQ/AP-REP). SPNEGO encoder
  (RFC 4178). Channel binding (RFC 5056) helper.
- Phase 14A Windows CI runner + Phase 14B/C/D conformance scaffolds.
- IAsyncTransport scaffold (Phase 2C) — System.IO.Pipelines-backed
  contract for the upcoming async I/O refactor.

### Changed

- DCOM defaults: PROTECTION_LEVEL_INTEGRITY + NTLMv2 + NTLM2 sessions
  (Phase 3B + 3C).
- NTLMv1 marked [Obsolete]; gated behind explicit `rpc.ntlm.allowV1=true`.
- Phase 2I: NTLMv2 server-side key derivation per MS-NLMP §3.4.5
  (verified against §4.2.4.1 spec test vectors).

### Removed

- Serilog package dependency (replaced by Microsoft.Extensions.Logging
  via Phase 2G shim).
- SharpCifs.Util.Sharpen.Properties usage replaced with managed
  PropertyBag (Phase 2D.1; 2D.2-2D.5 queued).
