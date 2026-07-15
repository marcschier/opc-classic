# OPC Complex Data server sample

`Opc.Classic.Samples.CpxServer` is a deterministic, managed DA-backed CPX server model. It implements `IOpcDaServer`, decorates its DA namespace with `OpcCpxAddressSpace`, and publishes properties 600–609 through `OpcCpxItemProperties`.

```powershell
dotnet run --project samples\Opc.Classic.Samples.CpxServer
```

The catalog includes:

- OPCBinary primitive and nested records, a counted array, and a 9-bit bit string.
- XMLSchema nested/array values with an optional element present and deliberately absent.
- Truncated OPCBinary input.
- A vendor type system that is advertised but intentionally not decoded.

The sample is read-only. DA group creation and vendor-defined decoding are
explicitly unsupported. The missing optional XML element is valid because the
parsed `minOccurs="0"` constraint is preserved. Property 604 contains a
parseable serialized type-description fragment rather than display text. The
server advertises unknown vendor type systems without selecting a codec or
interpreting their payloads.

The project inherits the repository's trimming and NativeAOT analyzers and uses no reflection, dynamic code, native COM runtime, or third-party dependencies.
