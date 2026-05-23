# SharpCifs.Std migration plan

## Why migrate

`SharpCifs.Std` is licensed under LGPL-2.1 while this repository ships under EPL-1.0. That transitive licensing mismatch is the blocker for keeping the package in the distributable product. The package is also dormant, with no meaningful upstream activity in years, and it is not a good fit for the NativeAOT contract that the `src/*` assemblies are being moved toward. It brings Java-portability shims, NTLM helpers, SMB/NetBIOS code, and NDR helpers into the DCOM assembly even when only small slices are used.

The migration goal is to remove the package reference without changing wire behavior. Each step should be mechanical, tested, and independently buildable so regressions are isolated to one surface area at a time.

## Inventory

The current `src/OpcClassic.Dcom` SharpCifs usage, grouped by `using` namespace, is:

| SharpCifs namespace | Using count | Primary role |
| --- | ---: | --- |
| `SharpCifs.Dcerpc.Ndr` | 74 | NDR buffer/codecs used by the legacy DCOM marshalling layer. |
| `SharpCifs.Util.Sharpen` | 52 | Java-portability shims such as `Properties`, `Iterator`, `IOException`, `Collections`, and thread helpers. |
| `SharpCifs.Util` | 5 | Utility helpers such as hex formatting. |
| `SharpCifs.Ntlmssp` | 4 | NTLM Type1/Type2/Type3 message types and flags. Auth-critical. |
| `SharpCifs.Smb` | 4 | `NtlmPasswordAuthentication` credential carrier. |
| `SharpCifs` root | 2 | `Config` access, currently used by NTLM defaults. |
| `SharpCifs.Netbios` | 1 | NetBIOS name helper. |

`Properties` is concentrated in the DCOM transport/auth path: `ComServer`, `ComOxidRuntime`, `ComOxidStub`, `RemUnknown2ServerStub`, `TransportFactory`, `ITransport`, `Stub`, `NtlmAuthentication`, `NtlmConnectionContext`, `ComTransport`, `ComRuntimeTransport`, and the named-pipe `RpcTransport`. It is used for `GetProperty`, `SetProperty`, simple `Load`/`Store`, and copy-construction from defaults. One runtime call site stores a `List<string>` for supported interfaces, so the replacement must carry object values even though most keys contain strings.

The inventory also exposes a larger-than-expected `SharpCifs.Dcerpc.Ndr` dependency. Dropping `SharpCifs.Std` is not complete until the NDR buffer/object surface is either replaced with a managed in-repo implementation or vendored under an EPL-compatible disposition.

## Strategy per sub-namespace

### `SharpCifs.Util.Sharpen`

Replace Java shims with managed equivalents. `Properties` becomes `OpcClassic.Dcom.Internal.PropertyBag`, a small concurrent key/value store with the call-site API needed today: `GetProperty`, `SetProperty`, and defaults copy construction. Later passes should replace `ArrayList`, `Iterator`, `Collections`, `IOException`, and thread shims with BCL collections, `System.IO.IOException`, and `System.Threading` primitives.

### `SharpCifs.Util`

Replace utility helpers with BCL equivalents. Hex helpers should move to `Convert.ToHexString` or a tiny local formatting helper when offsets/lengths are required. Any encoding or byte helpers should be audited for exact output because the RPC and NTLM logs/tests may depend on formatting.

### `SharpCifs.Ntlmssp`

Keep behavior stable while removing the package. The safest transitional path is to vendor the minimal `NtlmFlags`, `Type1Message`, `Type2Message`, and `Type3Message` types into `src/OpcClassic.Dcom/Common/Ntlm/` under a verified licensing path, then modernize the code around the in-repo types. As an alternative, Phase 3D's Kerberos.NET/SPNEGO work may provide usable NTLM message primitives; verify API coverage before choosing it. This is auth-critical and must be covered by the Crypto/NTLM tests before deletion of the package reference.

### `SharpCifs.Smb.NtlmPasswordAuthentication`

Replace with `System.Net.NetworkCredential`. The current usage is a credential container for domain, user, and password values; `NetworkCredential` already models this and avoids carrying SMB client code into DCOM. Callers should pass credential data explicitly through the DCOM auth path rather than depending on SMB-specific types.

### `SharpCifs` root `Config`

Replace with a managed config shim local to `OpcClassic.Dcom`. The immediate root usage is a boolean NTLM default. A local class can read explicit property-bag values first and then optional process/environment configuration if needed; it should not reintroduce global mutable state.

### `SharpCifs.Dcerpc.Ndr` and `SharpCifs.Netbios`

These were discovered during the inventory and must be included before package removal. For NDR, either vendor the minimal `NdrBuffer`, `NdrObject`, and codec surface used by the DCOM marshalling layer, or hand-roll the small subset needed by the generated/legacy stubs. For NetBIOS, replace the single name helper with a local parser/formatter or remove it if named-pipe transport is dropped.

## Phases

1. **Phase 2D.1 (this commit):** Replace `SharpCifs.Util.Sharpen.Properties` with `OpcClassic.Dcom.Internal.PropertyBag`; update the DCOM call sites and add focused tests.
2. **Phase 2D.2:** Replace `SharpCifs.Util` helpers with BCL/local equivalents.
3. **Phase 2D.3:** Replace `SharpCifs.Smb.NtlmPasswordAuthentication` with `System.Net.NetworkCredential`.
4. **Phase 2D.4:** Vendor or replace `SharpCifs.Ntlmssp` message/flag types; verify whether Phase 3D Kerberos.NET can supply equivalent NTLM types.
5. **Phase 2D.4b:** Replace or vendor the discovered `SharpCifs.Dcerpc.Ndr` and `SharpCifs.Netbios` surface so the package can actually be removed.
6. **Phase 2D.5:** Drop the `SharpCifs.Std` `PackageReference`, remove remaining `using SharpCifs.*`, and run full solution build/test plus the AOT canary once available.

## Risk and validation

Each phase is intentionally a series of mechanical replacements. Build after every batch, keep the DCOM tests green, and avoid changing authentication or marshalling behavior while replacing types. The Crypto.Tests project added in Phase 2I is the safety net for MD4/RC4 and should be expanded with NTLM message vectors before Phase 2D.4. The DCOM legacy tests should continue to pass after every phase, and full `dotnet build OpcClassic.slnx` plus `dotnet test OpcClassic.slnx` gates Phase 2D.5.

## Phase 2C — Async I/O end-to-end

Phase 2C introduces the managed async transport surface in `OpcClassic.Transport` without rewiring the legacy SharpInterop call sites yet. `IAsyncTransport` exposes the ncacn_ip_tcp byte stream as `System.IO.Pipelines.PipeReader` and `PipeWriter`, `IAsyncTransportFactory.ConnectAsync` becomes the future connector entry point, and `IAsyncEndpoint.AcceptConnectionsAsync` models server-side accepts as an `IAsyncEnumerable<IAsyncTransport>` for hosted-service consumers.

The rollout is intentionally staged:

1. Keep `SharpInterop.Rpc.ITransport` and its synchronous `Send` / `Receive` callers stable while focused tests cover the new contracts and pipe-backed test double.
2. Add a DCOM or transport package implementation that adapts TCP sockets to `IAsyncTransport`, including cancellation-aware reads, writes, flushes, and disposal.
3. Move connection-oriented RPC framing to consume `PipeReader` / `PipeWriter` directly, then bridge or replace legacy `IEndpoint.Call` with async call flow and `ICallChannel` integration.
4. Convert server socket-accept loops from dedicated threads to hosted services that iterate `await foreach` over `AcceptConnectionsAsync` and dispatch accepted transports through channel-style workers.
5. Retire the legacy `SharpInterop.Rpc.ITransport` surface after all client activation, call, callback, and subscription paths use the async contracts. OPC DA subscriptions already expose pushed changes as `IAsyncEnumerable`, so the transport refactor can align connection handling with that consumer pattern.

## Phase 2D.2 — SharpCifs.Util replacement done

N7.2 removed the remaining direct `SharpCifs.Util` helper usage from `src/OpcClassic.Dcom`. Hex formatting now uses `Convert.ToHexString`, HMAC-MD5 uses `System.Security.Cryptography.HMACMD5`, and little-endian primitive reads/writes use `System.Buffers.Binary.BinaryPrimitives` plus `BitConverter` for floating-point bit conversion.

Remaining SharpCifs migration sub-phases are queued as N7.3-N7.5:

1. **Phase 2D.3:** Replace `SharpCifs.Smb` credential usage.
2. **Phase 2D.4:** Vendor or replace `SharpCifs.Ntlmssp` message/flag types.
3. **Phase 2D.5:** Replace or vendor the remaining `SharpCifs.Dcerpc.Ndr` surface, then remove the package reference once no `SharpCifs.*` usage remains.

## N1.1 — DcomCallChannel landed

N1.1 adds `OpcClassic.Dcom.Transport.DcomCallChannel`, the first `ICallChannel` implementation built directly on `IAsyncTransport`. It performs the async DCE/RPC bind handshake, maps the first invoked IID to a presentation context, emits `RequestCoPdu` frames with an `ORPCTHIS` prefix, reads `ResponseCoPdu` / `FaultCoPdu` replies, and reassembles fragmented responses before returning `NdrCallResult` to generated shims.

The new public auth contract lives in `OpcClassic.IAuthContext`, with `NoOpAuthContext` available for unauthenticated tests and loopback scenarios. Downstream `OpcProxyGenerator`-emitted shims can now be wired by obtaining an `ICallChannel` from `DcomCallChannelFactory.ConnectAsync(endpoint, clsid, authContext, ct)` and passing their NDR request payloads to `InvokeAsync(iid, opnum, payload, ct)`. Non-empty auth tokens and packet verifiers are carried through the PDU auth trailer; production NTLM/Kerberos contexts fill in the concrete signing/sealing behavior in the follow-up auth integration work.

## Phase 2D.3 / N7.3 — `NtlmPasswordAuthentication` replacement done

N7.3 removed the `SharpCifs.Smb.NtlmPasswordAuthentication` credential carrier from `src/OpcClassic.Dcom` and replaced it with `System.Net.NetworkCredential`. The constructor parameter order was corrected from SharpCifs `(domain, user, password)` to BCL `(userName, password, domain)`, and accessors now use `Domain`, `UserName`, and `Password`.

Remaining `SharpCifs.Smb` imports are for named-pipe SMB transport types such as `SmbException` and `SmbNamedPipe`; those are outside N7.3. Phase 2D.4 / N7.4 is next and will vendor or replace the `SharpCifs.Ntlmssp` `Type1Message`, `Type2Message`, `Type3Message`, and `NtlmFlags` types under `OpcClassic.Dcom/Common/Ntlm/`.

## Phase 2D.4 / N7.4 — `SharpCifs.Ntlmssp` forwarding shim

N7.4 introduced `OpcClassic.Dcom.Internal.Ntlm` in `src/OpcClassic.Dcom/Common/Ntlm/`. The DCOM auth call sites now depend on local `NtlmFlags`, `NtlmMessage`, `Type1Message`, `Type2Message`, and `Type3Message` types instead of importing `SharpCifs.Ntlmssp` directly.

This is intentionally a type-forwarding step: the local wrappers still delegate parsing and serialization to `SharpCifs.Std` internally so Type1/Type2/Type3 wire bytes stay unchanged. Full self-contained MS-NLMP serialization replaces those internals in the N7.4 follow-up, and the `SharpCifs.Std` package reference is removed only after the Dcerpc/NDR migration is complete.

## Phase 2D.5 / N7.5 — Dcerpc NDR wall

N7.5 moved the legacy DCE/RPC NDR surface out of the `SharpCifs.Dcerpc.Ndr` namespace and into `OpcClassic.Dcom.Internal.LegacyNdr` under `src/OpcClassic.Dcom/Common/LegacyNdr/`. The local surface covers the DCOM call sites' current NDR primitives: `NdrBuffer`, `NdrCodec`, `NdrFormat`, `NdrOp`, `NdrObject`, and `NdrException`.

All direct `SharpCifs.Dcerpc.Ndr` references in `src/OpcClassic.Dcom` were removed. The implementation is intentionally compatibility-shaped around the legacy API so the DCOM marshalling layer can keep its current call patterns while later milestones replace more of the internals with the span-based `OpcClassic.Ndr` reader/writer primitives.
