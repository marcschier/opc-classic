# OPC Complex Data client sample

`Opc.Classic.Samples.CpxClient` browses the managed CPX server sample, reads DA properties 600–604 to discover each type system/dictionary/type, and then decodes OPCBinary and XMLSchema payloads.

```powershell
dotnet run --project samples\Opc.Classic.Samples.CpxClient
```

The demo also:

- applies the bounded `OpcCpxReferenceDataFilter`;
- converts nested and counted-array fields with `OpcCpxReferenceTypeConverter`;
- reports the committed nesting, array, expression, and comparison limits;
- rejects `LIKE` vendor filter syntax with `OPCCPX_E_FILTER_INVALID`;
- rejects bit-string-to-integer conversion with `OPC_E_BADTYPE`;
- reports a vendor type system without guessing its codec;
- catches the deterministic truncated OPCBinary payload;
- accepts and round-trips the omitted optional XML element;
- parses each serialized property-604 type-description fragment before decoding.

The client references only repository projects. Discovery and decode use static, AOT-safe APIs with no runtime reflection or external packages.

The reference converter limits complex nesting to 32 and repeated fields to
65,536 elements. The filter limits expressions to 4,096 characters, 32 nested
parentheses, 128 comparisons, 32 path segments, and 1,024-character literals.
Unsupported syntax fails with CPX HRESULTs rather than invoking a dynamic
expression engine.
