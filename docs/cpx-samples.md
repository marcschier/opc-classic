# OPC Complex Data samples

The paired [`CpxServer`](../samples/Opc.Classic.Samples.CpxServer/README.md) and [`CpxClient`](../samples/Opc.Classic.Samples.CpxClient/README.md) projects demonstrate CPX on top of a managed DA address space.

The server uses `OpcCpxAddressSpace` and `OpcCpxItemProperties` to expose the reserved CPX browse tree and properties 600–609. Its deterministic catalog covers OPCBinary primitives, nested structures, counted arrays, bit strings, XMLSchema nesting/arrays/optional metadata, malformed payloads, and an advertised vendor type system.

The client discovers dictionaries and type IDs from DA properties rather than hard-coding them. It decodes supported OPCBinary/XML values and exercises the bounded reference converter/filter.

## Reference semantics

`OpcCpxReferenceTypeConverter` performs deterministic, invariant-culture
primitive conversions and structural conversion of nested records and repeated
fields. It validates exact CLR runtime types and declared field constraints,
including string/blob capacities and bit-string length/padding. Malformed or
unsupported values return `OPC_E_BADTYPE`; overflow and configured bounds return
`OPC_E_RANGE`. `OPCCPX_E_TYPE_CHANGED` is reserved for changed type metadata.

`OpcCpxReferenceDataFilter` is a deliberately small reference evaluator, not a
general expression engine. It supports nested field paths, parentheses,
`AND`/`OR` (or `&&`/`||`), equality/inequality, and ordered comparisons for
supported scalar values. Every path segment is resolved against its declared
`TypeField`, runtime values are checked against that declaration, integer
literals use the declared width/sign, and FILETIME comparisons normalize UTC
`DateTimeOffset` instants. It limits expressions to 4,096 UTF-16 code units, 32
parenthesis levels, 128 comparisons, 32 path segments, and 1,024 decoded
literal characters. Functions, arithmetic, unary operators, `LIKE`, and other
vendor syntax return `OPCCPX_E_FILTER_INVALID`.

Unsupported behavior is deliberate:

- vendor type systems are reported, not guessed;
- vendor filter operators such as `LIKE` return `OPCCPX_E_FILTER_INVALID`;
- unsupported conversions return `OPC_E_BADTYPE`;
- XML Schema `minOccurs` constraints are preserved, so omitted optional elements round-trip;
- malformed payloads are reported without terminating the browse.

Property 604 publishes a parseable serialized type-description fragment. The
client parses that fragment before decoding the corresponding item value.

Both standalone projects inherit the sample tree's NativeAOT and trimming
analyzers and use no external dependencies or runtime reflection. They are
reference/conformance samples rather than a claim that unknown vendor type
systems are portable.
