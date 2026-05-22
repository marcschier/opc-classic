//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// OPC Batch DCOM-projection interfaces. Each [OpcInterface] partial interface
// is extended by the OpcInterfaceGenerator to carry a compile-time-known
// InterfaceId. Method/opnum mapping and generated proxy call-shims are
// deferred to the Phase 9C follow-up once the shared proxy template settles.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCBatchServer)
#pragma warning disable MA0048 // Multiple trivial interface stubs grouped for readability

using OpcClassic.Generators;

namespace OpcClassic.Batch.Dcom;

/// <summary><c>IOPCBatchServer</c> — Batch 1.0 top-level browse/enumeration interface (IID_IOPCBatchServer).</summary>
[OpcInterface("8BB4ED50-B314-11D3-B3EA-00C04F8ECEAA")]
public partial interface IOPCBatchServer
{
}

/// <summary><c>IOPCBatchServer2</c> — Batch 2.0 filtered batch-summary enumeration interface (IID_IOPCBatchServer2).</summary>
[OpcInterface("895A78CF-B0C5-11D4-A0B7-000102A980B1")]
public partial interface IOPCBatchServer2
{
}

/// <summary><c>IEnumOPCBatchSummary</c> — enumeration of batch summaries (IID_IEnumOPCBatchSummary).</summary>
[OpcInterface("A8080DA2-E23E-11D2-AFA7-00C04F539421")]
public partial interface IEnumOPCBatchSummary
{
}

/// <summary><c>IOPCEnumerationSets</c> — Batch enumeration sets for physical/procedural/state/mode classification (IID_IOPCEnumerationSets).</summary>
[OpcInterface("A8080DA3-E23E-11D2-AFA7-00C04F539421")]
public partial interface IOPCEnumerationSets
{
}
