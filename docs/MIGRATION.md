# Migration to Opc.Classic 1.0.0

## From the rc series (`1.0.0-rc.1` .. `1.0.0-rc.5`)

No breaking API changes are documented between the rc tags and `1.0.0`. The rc
series filled in implementation, Windows CCW, marshaling, and test-fleet gaps
while keeping the public release-candidate surface compatible. Rebuild and
redeploy against the final package version.

## From the preview series (`OpcClassic.*` preview namespace)

If you integrated with the pre-rc preview namespace, update package references,
project references, and `using` directives from `OpcClassic.*` to
`Opc.Classic.*`.

| Old preview root | New 1.0.0 root |
| --- | --- |
| `OpcClassic.Core` | `Opc.Classic.Core` |
| `OpcClassic.Da` | `Opc.Classic.Da` |
| `OpcClassic.Ae` | `Opc.Classic.Ae` |
| `OpcClassic.Hda` | `Opc.Classic.Hda` |
| `OpcClassic.Dcom` | `Opc.Classic.Dcom` |
| `OpcClassic.Dcom.Kerberos` | `Opc.Classic.Dcom.Kerberos` |
| `OpcClassic.Hosting` | `Opc.Classic.Hosting` |
| `OpcClassic.Discovery` | `Opc.Classic.Discovery` |
| `OpcClassic.Xml` | `Opc.Classic.Xml` |
| `OpcClassic.Batch` | `Opc.Classic.Batch` |
| `OpcClassic.Commands` | `Opc.Classic.Commands` |
| `OpcClassic.Cpx` | `Opc.Classic.Cpx` |
| `OpcClassic.Dx` | `Opc.Classic.Dx` |
| `OpcClassic.Security` | `Opc.Classic.Security` |

## API-surface compatibility

The DA group surface (`IOPCGroupStateMgt`, `IOPCItemMgt`, `IOPCSyncIO`,
`IOPCAsyncIO2/3`, and `IConnectionPoint`) has been stable since rc.3. Later
rcs added optional Windows-CCW integration points without removing the existing
cross-platform path.

| Member | First shipped in | Notes |
| --- | --- | --- |
| `IOpcDataCallbackSink` | rc.4 (`cap-c8`) | Optional direct callback sink for advanced Windows CCW integration. The cross-platform `IOpcInterfaceRef` Advise path remains supported. |
| `OpcDaGroup.AdviseAsync(IOpcDataCallbackSink, CancellationToken)` | rc.4 (`cap-c8`) | Additive overload; existing `IOpcInterfaceRef` overload unchanged. |
| `IOpcDaServer.ResolveGroupAsync` / `ResolveGroupByNameAsync` | rc.2 | Additive default interface methods. Override them only if your hosted server needs Windows CCW and managed DCOM paths to share group lookup. |

## Pre-rc changes to check

| Area | What changed | Migration action |
| --- | --- | --- |
| Package and namespace identity | Preview `OpcClassic.*` names were standardized to `Opc.Classic.*`. | Rename package references, project references, and `using` directives. |
| Hosting extension methods | Older draft snippets used longer `AddOpcClassic*` names. Current names are `AddClassicServer`, `AddClassicClsidRegistry`, and `AddOpcDaServer<T>`. | Update startup code and prefer the patterns in `docs\ADOPTION.md`. |
| `OpcSafeArray` shape | `Lengths` and `LowerBounds` expose `ReadOnlySpan<int>`. | Iterate spans directly or call `.ToArray()` where an array is required. |
| Dispatch payloads | `DispatchResult.Payload` and generated call paths use `ReadOnlyMemory<byte>`. | Use `.Span`, `.Memory`, or `.ToArray()` at interop boundaries. |
| Connection-point errors | Unknown `UnadviseAsync` cookies now fail with `CONNECT_E_NOCONNECTION` instead of silently succeeding. | Treat double-unadvise as an error or guard it in caller state. |
| Authentication defaults | The supported baseline is NTLMv2 or Kerberos/SPNEGO with packet integrity; NTLMv1 requires explicit opt-in. | Use `OpcConnectData.WithNtlmV2` or `WithKerberos` and configure `OpcProtectionLevel` intentionally. |

## Behavioral changes

No additional rc-to-final adoption hazards were found in the rc.1..rc.5
changelog. The notable behavioral differences are the pre-rc items above and
the rc-series completion of previously stubbed Windows CCW methods, which should
make OPC client/server interop more complete rather than less compatible.

## See also

- `CHANGELOG.md` — full release history
- `docs\ADOPTION.md` — getting-started guide
- `docs\release-blockers.md` — remaining gates before the FINAL tag
- `docs\migration\README.md` — migration analyzer diagnostics
