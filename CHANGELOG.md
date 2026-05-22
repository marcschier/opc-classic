# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

(nothing new since 0.1.0-alpha.2)

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
