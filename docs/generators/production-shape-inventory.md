# Production generator shape inventory

The executable audit in `ProductionShapeInventoryTests` covers every production method on an interface marked `[GenerateOpcProxy]` or `[OpcGenerateServerDispatch]`. It builds each production project from source with its real compiled project-reference graph, retains the generator `outputCompilation`, and fails on any error located in generated source, including duplicate members and missing or inaccessible types.

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

## Hand-written generator sides

None.

## Shrinking rule

`ProductionShapeMigrationManifest.json` is retained as a zero-fallback guard. Any diagnostic suppression, unsupported diagnostic, or hand-written wire side must be explicitly reintroduced and justified.
