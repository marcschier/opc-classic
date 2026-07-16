# OPC Classic generator diagnostics

The `Opc.Classic.Generators` package reports these diagnostics while validating
the interface, client-proxy, server-dispatch, and wire-correlation attributes.
The production inventory contains 253 method shapes and
`ProductionShapeMigrationManifest.json` contains zero diagnostic suppressions,
unsupported diagnostics, or hand-written wire fallbacks.

Release tracking note: `OPCGEN001`-`OPCGEN010` are listed in `AnalyzerReleases.Shipped`. `OPCGEN011` and server-dispatch diagnostics remain tracked in `AnalyzerReleases.Unshipped` until a release ships them.

## Interface contract diagnostics

| ID | Severity | Meaning |
| --- | --- | --- |
| OPCGEN001 | Error | `[OpcInterface]` contains an invalid GUID literal. |
| OPCGEN002 | Error | `[OpcInterface]` is applied to an interface that is not `partial`. |
| OPCGEN003 | Error | Two `[OpcMethod]` members on one interface share an opnum. |

## Client proxy diagnostics

| ID | Severity | Meaning |
| --- | --- | --- |
| OPCGEN004 | Error | `[GenerateOpcProxy]` is applied to an interface that is not `partial`. |
| OPCGEN005 | Warning | `[GenerateOpcProxy]` is missing the companion `[OpcInterface]` attribute. |
| OPCGEN006 | Warning | A `ref` or `out` parameter type has no registered request/response codec. |
| OPCGEN007 | Warning | An `[OpcMethod]` signature is unsupported, such as a generic method or non-`Task` return. |
| OPCGEN008 | Info | A method uses an unsupported parameter or return type, so generation falls back to an empty-payload body. |
| OPCGEN009 | Warning | A method return type has no registered codec in the generator codec registry. |
| OPCGEN010 | Warning | A method parameter type is unsupported by the generator codec registry. |
| OPCGEN011 | Warning | Client array-count or IID correlation metadata is invalid or unsafe. |

## Server dispatch diagnostics

| ID | Severity | Meaning |
| --- | --- | --- |
| OPCGEN101 | Error | `[OpcGenerateServerDispatch]` is applied to an interface that is not partial. |
| OPCGEN102 | Warning | `[OpcGenerateServerDispatch]` is missing the companion `[OpcInterface]` attribute. |
| OPCGEN103 | Warning | An `[OpcMethod]` signature cannot be represented by the server dispatcher. |
| OPCGEN104 | Warning | A server request parameter type cannot be decoded by the generator. |
| OPCGEN105 | Warning | A server response type cannot be encoded by the generator. |
| OPCGEN107 | Warning | Server array-count or IID correlation metadata is invalid or unsafe. |

The production audit is documented in
[Production generator shape inventory](production-shape-inventory.md). It compiles generated source per production project using real project references and derives correlation categories only from resolved `[OpcArrayCount]` and `[OpcIidIs]` attributes.

## Attribute reference

| Attribute | Target | Purpose |
| --- | --- | --- |
| `[OpcInterface("guid")]` | partial interface | Defines the IID and enables interface metadata generation. |
| `[OpcMethod(opnum)]` | method | Defines the wire opnum. |
| `[GenerateOpcProxy]` | partial interface | Emits the client proxy. |
| `[OpcGenerateServerDispatch]` | partial interface | Emits the server dispatcher. |
| `[OpcGenerateMultiOutRecord]` | method | Emits a named or inferred result record for multiple outputs. |
| `[OpcArrayCount("parameter")]` | parameter/return | Declares a count correlation; invalid names or unsafe directionality produce `OPCGEN011`/`OPCGEN107`. |
| `[OpcIidIs("parameter")]` | parameter/return | Declares an `iid_is` interface-pointer correlation. |
| `[OpcUniquePointer]` | parameter/return | Selects NDR unique-pointer encoding. |
| `[OpcEmitArrayCount]` | parameter | Emits a separate IDL count field before the first correlated input array. |
| `[OpcDeferredElements]` | parameter | Selects deferred element-pointer layout for supported arrays. |
| `[OpcFileTimeElements]` | parameter | Selects `FILETIME` element encoding. |
| `[OpcVariantElements]` | parameter | Selects MS-OAUT wire-VARIANT element encoding. |
| `[OpcRefString]` | parameter | Selects top-level NDR reference-string encoding. |
| `[OpcProxyIgnore]` | method | Excludes a method from client proxy generation when a deliberate non-wire compatibility member is required. |

## Severity policy

Errors stop the affected generator output. Warnings keep the unsupported or
unsafe shape visible to the build. `OPCGEN008` identifies the legacy
empty-payload placeholder path; production inventory treats any occurrence as a
manifest failure rather than accepted coverage.
