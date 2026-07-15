# OPC Complex Data samples

The paired [`CpxServer`](../samples/Opc.Classic.Samples.CpxServer/README.md) and [`CpxClient`](../samples/Opc.Classic.Samples.CpxClient/README.md) projects demonstrate CPX on top of a managed DA address space.

The server uses `OpcCpxAddressSpace` and `OpcCpxItemProperties` to expose the reserved CPX browse tree and properties 600–609. Its deterministic catalog covers OPCBinary primitives, nested structures, counted arrays, bit strings, XMLSchema nesting/arrays/optional metadata, malformed payloads, and an advertised vendor type system.

The client discovers dictionaries and type IDs from DA properties rather than hard-coding them. It decodes supported OPCBinary/XML values and exercises the bounded reference converter/filter.

Unsupported behavior is deliberate:

- vendor type systems are reported, not guessed;
- vendor filter operators such as `LIKE` return `OPCCPX_E_FILTER_INVALID`;
- unsupported conversions return `OPCCPX_E_TYPE_CHANGED`;
- omitted XML optional elements currently fail closed because the reference serializer requires every parsed field;
- malformed payloads are reported without terminating the browse.

Both projects inherit the sample tree's NativeAOT and trimming analyzers and use no external dependencies or runtime reflection.
