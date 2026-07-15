# OPC Complex Data samples

The paired [`CpxServer`](../samples/Opc.Classic.Samples.CpxServer/README.md) and [`CpxClient`](../samples/Opc.Classic.Samples.CpxClient/README.md) projects demonstrate CPX on top of a managed DA address space.

The server uses `OpcCpxAddressSpace` and `OpcCpxItemProperties` to expose the reserved CPX browse tree and properties 600–609. Its deterministic catalog covers OPCBinary primitives, nested structures, counted arrays, bit strings, XMLSchema nesting/arrays/optional metadata, malformed payloads, and an advertised vendor type system.

The client discovers dictionaries and type IDs from DA properties rather than hard-coding them. It decodes supported OPCBinary/XML values and exercises the bounded reference converter/filter.

## Reference semantics

`OpcCpxReferenceTypeConverter` performs deterministic, invariant-culture
primitive conversions and structural conversion of nested records and repeated
fields. Numeric overflow, incompatible field shape, unresolved type reference,
unsupported bit-string conversion, depth above 32, or more than 65,536 elements
in one repeated field returns `OPCCPX_E_TYPE_CHANGED`.

`OpcCpxReferenceDataFilter` is a deliberately small reference evaluator, not a
general expression engine. It supports nested field paths, parentheses,
`AND`/`OR` (or `&&`/`||`), equality/inequality, and ordered comparisons for
supported scalar values. It limits expressions to 4,096 UTF-16 code units, 32
parenthesis levels, 128 comparisons, 32 path segments, and 1,024 decoded
literal characters. Functions, arithmetic, unary operators, `LIKE`, and other
vendor syntax return `OPCCPX_E_FILTER_INVALID`.

Unsupported behavior is deliberate:

- vendor type systems are reported, not guessed;
- vendor filter operators such as `LIKE` return `OPCCPX_E_FILTER_INVALID`;
- unsupported conversions return `OPCCPX_E_TYPE_CHANGED`;
- omitted XML optional elements currently fail closed because the reference serializer requires every parsed field;
- malformed payloads are reported without terminating the browse.

Both standalone projects inherit the sample tree's NativeAOT and trimming
analyzers and use no external dependencies or runtime reflection. They are
reference/conformance samples rather than a claim that unknown vendor type
systems are portable.
