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

The sample is read-only. DA group creation and vendor-defined decoding are explicitly unsupported. The missing optional XML element is retained as an invalid-payload example because the bounded reference XML serializer currently requires every parsed field.

The project inherits the repository's trimming and NativeAOT analyzers and uses no reflection, dynamic code, native COM runtime, or third-party dependencies.
