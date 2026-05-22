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
