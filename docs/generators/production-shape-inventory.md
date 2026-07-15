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

None. The shrinking migration manifest remains strict and must list any future emitted unsupported-shape diagnostic exactly.

## Source suppressions

| Source | IDs |
| --- | --- |
| `src/Opc.Classic.Ae/Dcom/IOPCInterfaces.cs` | `OPCGEN104`, `OPCGEN105` |

## Hand-written generator sides

| Side | Interface.method | Shapes | Specification | Implementation |
| --- | --- | --- | --- | --- |
| `client` | `Opc.Classic.Batch.Dcom.IOPCBatchServer.GetDelimiterAsync` | `scalar` | OPC Batch IDL: IOPCBatchServer::GetDelimiter | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IOPCBatchServerClientProxy` |
| `server` | `Opc.Classic.Batch.Dcom.IOPCBatchServer.GetDelimiterAsync` | `scalar` | OPC Batch IDL: IOPCBatchServer::GetDelimiter | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IOPCBatchServerServerDispatcher` |
| `client` | `Opc.Classic.Batch.Dcom.IOPCBatchServer.CreateEnumeratorAsync` | `scalar`, `interface pointer/iid_is` | OPC Batch IDL: IOPCBatchServer::CreateEnumerator | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IOPCBatchServerClientProxy` |
| `server` | `Opc.Classic.Batch.Dcom.IOPCBatchServer.CreateEnumeratorAsync` | `scalar`, `interface pointer/iid_is` | OPC Batch IDL: IOPCBatchServer::CreateEnumerator | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IOPCBatchServerServerDispatcher` |
| `client` | `Opc.Classic.Batch.Dcom.IOPCBatchServer2.CreateFilteredEnumeratorAsync` | `scalar`, `interface pointer/iid_is`, `nested/compound records` | OPC Batch IDL: IOPCBatchServer2::CreateFilteredEnumerator | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IOPCBatchServer2ClientProxy` |
| `server` | `Opc.Classic.Batch.Dcom.IOPCBatchServer2.CreateFilteredEnumeratorAsync` | `scalar`, `interface pointer/iid_is`, `nested/compound records` | OPC Batch IDL: IOPCBatchServer2::CreateFilteredEnumerator | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IOPCBatchServer2ServerDispatcher` |
| `client` | `Opc.Classic.Batch.Dcom.IEnumOPCBatchSummary.NextAsync` | `scalar`, `array`, `count-correlated arrays`, `nested/compound records` | OPC Batch IDL: IEnumOPCBatchSummary::Next | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IEnumOPCBatchSummaryClientProxy` |
| `server` | `Opc.Classic.Batch.Dcom.IEnumOPCBatchSummary.NextAsync` | `scalar`, `array`, `count-correlated arrays`, `nested/compound records` | OPC Batch IDL: IEnumOPCBatchSummary::Next | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IEnumOPCBatchSummaryServerDispatcher` |
| `client` | `Opc.Classic.Batch.Dcom.IEnumOPCBatchSummary.SkipAsync` | `scalar` | OPC Batch IDL: IEnumOPCBatchSummary::Skip | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IEnumOPCBatchSummaryClientProxy` |
| `server` | `Opc.Classic.Batch.Dcom.IEnumOPCBatchSummary.SkipAsync` | `scalar` | OPC Batch IDL: IEnumOPCBatchSummary::Skip | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IEnumOPCBatchSummaryServerDispatcher` |
| `client` | `Opc.Classic.Batch.Dcom.IEnumOPCBatchSummary.ResetAsync` | `scalar` | OPC Batch IDL: IEnumOPCBatchSummary::Reset | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IEnumOPCBatchSummaryClientProxy` |
| `server` | `Opc.Classic.Batch.Dcom.IEnumOPCBatchSummary.ResetAsync` | `scalar` | OPC Batch IDL: IEnumOPCBatchSummary::Reset | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IEnumOPCBatchSummaryServerDispatcher` |
| `client` | `Opc.Classic.Batch.Dcom.IEnumOPCBatchSummary.CloneAsync` | `scalar`, `clone` | OPC Batch IDL: IEnumOPCBatchSummary::Clone | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IEnumOPCBatchSummaryClientProxy` |
| `server` | `Opc.Classic.Batch.Dcom.IEnumOPCBatchSummary.CloneAsync` | `scalar`, `clone` | OPC Batch IDL: IEnumOPCBatchSummary::Clone | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IEnumOPCBatchSummaryServerDispatcher` |
| `client` | `Opc.Classic.Batch.Dcom.IEnumOPCBatchSummary.CountAsync` | `scalar` | OPC Batch IDL: IEnumOPCBatchSummary::Count | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IEnumOPCBatchSummaryClientProxy` |
| `server` | `Opc.Classic.Batch.Dcom.IEnumOPCBatchSummary.CountAsync` | `scalar` | OPC Batch IDL: IEnumOPCBatchSummary::Count | `src/Opc.Classic.Batch/Dcom/IOPCBatchClientProxies.cs::IEnumOPCBatchSummaryServerDispatcher` |
| `client` | `Opc.Classic.Da.Dcom.IConnectionPoint.GetConnectionInterfaceAsync` | `scalar` | OPC DA IDL: IConnectionPoint::GetConnectionInterface | `src/Opc.Classic.Da/Dcom/IConnectionPointClientProxy.cs::IConnectionPointClientProxy` |
| `client` | `Opc.Classic.Da.Dcom.IConnectionPoint.AdviseAsync` | `scalar` | OPC DA IDL: IConnectionPoint::Advise | `src/Opc.Classic.Da/Dcom/IConnectionPointClientProxy.cs::IConnectionPointClientProxy` |
| `client` | `Opc.Classic.Da.Dcom.IConnectionPoint.UnadviseAsync` | `scalar` | OPC DA IDL: IConnectionPoint::Unadvise | `src/Opc.Classic.Da/Dcom/IConnectionPointClientProxy.cs::IConnectionPointClientProxy` |
| `client` | `Opc.Classic.Da.Dcom.IOPCSyncIO.ReadAsync` | `scalar`, `array`, `multi-out records`, `pointer arrays`, `nested/compound records` | OPC DA IDL: IOPCSyncIO::Read | `src/Opc.Classic.Da/Dcom/IOPCSyncIOClientProxy.cs::IOPCSyncIOClientProxy` |
| `client` | `Opc.Classic.Da.Dcom.IOPCSyncIO.WriteAsync` | `array`, `pointer arrays` | OPC DA IDL: IOPCSyncIO::Write | `src/Opc.Classic.Da/Dcom/IOPCSyncIOClientProxy.cs::IOPCSyncIOClientProxy` |
| `client` | `Opc.Classic.Dx.Dcom.IOPCConfiguration.QuerySourceServersAsync` | `array`, `nested/compound records` | OPC DX 1.00 IDL: IOPCConfiguration::GetServers | `src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs::IOPCConfigurationClientProxy` |
| `client` | `Opc.Classic.Dx.Dcom.IOPCConfiguration.AddSourceServersAsync` | `scalar`, `array`, `nested/compound records` | OPC DX 1.00 IDL: IOPCConfiguration::AddServers | `src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs::IOPCConfigurationClientProxy` |
| `client` | `Opc.Classic.Dx.Dcom.IOPCConfiguration.ModifySourceServersAsync` | `scalar`, `array`, `nested/compound records` | OPC DX 1.00 IDL: IOPCConfiguration::ModifyServers | `src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs::IOPCConfigurationClientProxy` |
| `client` | `Opc.Classic.Dx.Dcom.IOPCConfiguration.DeleteSourceServersAsync` | `scalar`, `array`, `nested/compound records` | OPC DX 1.00 IDL: IOPCConfiguration::DeleteServers | `src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs::IOPCConfigurationClientProxy` |
| `client` | `Opc.Classic.Dx.Dcom.IOPCConfiguration.CopyDefaultServerAttributesAsync` | `scalar`, `array`, `nested/compound records` | OPC DX 1.00 IDL: IOPCConfiguration::CopyDefaultServerAttributes | `src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs::IOPCConfigurationClientProxy` |
| `client` | `Opc.Classic.Dx.Dcom.IOPCConfiguration.QueryDXConnectionsAsync` | `scalar`, `array`, `nested/compound records` | OPC DX 1.00 IDL: IOPCConfiguration::QueryDXConnections | `src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs::IOPCConfigurationClientProxy` |
| `client` | `Opc.Classic.Dx.Dcom.IOPCConfiguration.AddDXConnectionsAsync` | `scalar`, `array`, `nested/compound records` | OPC DX 1.00 IDL: IOPCConfiguration::AddDXConnections | `src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs::IOPCConfigurationClientProxy` |
| `client` | `Opc.Classic.Dx.Dcom.IOPCConfiguration.UpdateDXConnectionsAsync` | `scalar`, `array`, `nested/compound records` | OPC DX 1.00 IDL: IOPCConfiguration::UpdateDXConnections | `src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs::IOPCConfigurationClientProxy` |
| `client` | `Opc.Classic.Dx.Dcom.IOPCConfiguration.ModifyDXConnectionsAsync` | `scalar`, `array`, `nested/compound records` | OPC DX 1.00 IDL: IOPCConfiguration::ModifyDXConnections | `src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs::IOPCConfigurationClientProxy` |
| `client` | `Opc.Classic.Dx.Dcom.IOPCConfiguration.DeleteDXConnectionsAsync` | `scalar`, `array`, `nested/compound records` | OPC DX 1.00 IDL: IOPCConfiguration::DeleteDXConnections | `src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs::IOPCConfigurationClientProxy` |
| `client` | `Opc.Classic.Dx.Dcom.IOPCConfiguration.CopyDefaultDXConnectionAttributesAsync` | `scalar`, `array`, `nested/compound records` | OPC DX 1.00 IDL: IOPCConfiguration::CopyDXConnectionDefaultAttributes | `src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs::IOPCConfigurationClientProxy` |
| `client` | `Opc.Classic.Dx.Dcom.IOPCConfiguration.ResetConfigurationAsync` | `scalar` | OPC DX 1.00 IDL: IOPCConfiguration::ResetConfiguration | `src/Opc.Classic.Dx/Dcom/IOPCConfigurationClientProxy.cs::IOPCConfigurationClientProxy` |

## Shrinking rule

`ProductionShapeMigrationManifest.json` is temporary migration state. Stale diagnostics or hand-written sides fail the audit and must be removed as generator coverage grows.
