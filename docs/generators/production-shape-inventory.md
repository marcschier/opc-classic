# Production generator shape inventory

The executable audit in `ProductionShapeInventoryTests` covers all **254 production method shapes** on interfaces marked `[GenerateOpcProxy]` or `[OpcGenerateServerDispatch]`. It builds each production project from source with its real compiled project-reference graph, retains the generator `outputCompilation`, and fails on any error located in generated source, including duplicate members and missing or inaccessible types.

## Semantic shape rules

| Category | Source of truth |
| --- | --- |
| `scalar` / `array` | Resolved Roslyn parameter and `Task<T>` result types. |
| `count-correlated arrays` | Resolved `[OpcArrayCount]` on a parameter or return value; an unannotated array is never claimed as correlated. |
| `interface pointer/iid_is` | Resolved `[OpcIidIs]` on a parameter or return value; type names and GUID parameter names are not inferred. |
| `multi-out records` | Resolved `[OpcGenerateMultiOutRecord]` or multiple `ref`/`out` parameters. |
| `pointer arrays` | Array pointer/ref shape and resolved `[OpcUniquePointer]`. |
| `clone` | COM clone method name. |
| `nested/compound records` | Resolved non-primitive record element/value types. |

`tests/Opc.Classic.Generators.Tests/ProductionShapeInventory.json` stores the exact interface, method, generator side, semantic categories, and specification reference. Production correlation attributes are inventory annotations for the IDL relationship; they are not migration exceptions.

## Active unsupported diagnostics

None. The migration manifest is at zero and remains strict: any future unsupported-shape diagnostic must be listed exactly.

## Source suppressions

None.

## Hand-written wire paths

Generated interface fallbacks remain at zero. The separate `manualWirePaths`
manifest records handwritten adapters that intentionally sit outside an
`[OpcInterface]` contract, including OPC Common/Discovery dispatchers and the
DA `OpcDaGroupEnumerators` dispatcher/codec path. The audit discovers those
types from source and fails when the manifest omits or misstates one.

## Production annotation contract

| Attribute | Wire meaning |
| --- | --- |
| `[OpcInterface(iid)]` | Declares the interface IID and emits static interface metadata. |
| `[OpcMethod(opnum)]` | Declares the DCE/RPC opnum used by client and server generation. |
| `[GenerateOpcProxy]` | Requests the typed client proxy. |
| `[OpcGenerateServerDispatch]` | Requests the typed server dispatcher. |
| `[OpcGenerateMultiOutRecord]` | Requests a result record for multiple response values. |
| `[OpcArrayCount(name)]` | Correlates an array with the named count parameter. The two-argument form identifies an array member inside a result record. |
| `[OpcEnumeratorArray(name, varying)]` | Emits an enumerator array plus `pceltFetched`; `varying=true` selects max/offset/actual framing. |
| `[OpcIidIs(name)]` | Correlates an interface pointer with the named IID parameter. |
| `[OpcUniquePointer]` | Selects NDR unique-pointer encoding for a parameter or return value. |
| `[OpcEmitArrayCount]` | Emits the standalone IDL count field before a correlated input array or output array return. |
| `[OpcDeferredElements]` | Uses deferred unique-pointer element layout for string arrays. |
| `[OpcFileTimeElements]` | Encodes `long[]` elements as Windows `FILETIME` pairs. |
| `[OpcVariantElements]` | Encodes `OpcVariant[]` elements in MS-OAUT wire-VARIANT form. |
| `[OpcRefString]` | Encodes a top-level operation string as an NDR reference pointer. |

The inventory derives semantic categories from resolved symbols and these
attributes. It does not infer count or IID correlations from parameter names.

## Shrinking rule

`ProductionShapeMigrationManifest.json` is retained as a zero-fallback and
explicit-manual-path guard. Any diagnostic suppression, unsupported diagnostic,
generated fallback, or handwritten dispatcher/codec path must be listed.
