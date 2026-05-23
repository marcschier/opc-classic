//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// OPC Batch DCOM-projection interfaces. Proxy generation now covers delimiter,
// enumeration, and Batch summary methods whose shapes fit registered codecs.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCBatchServer)
#pragma warning disable MA0048 // Multiple small interface projections grouped for readability

using System.Threading;
using System.Threading.Tasks;
using OpcClassic.Batch;
using OpcClassic.Generators;

namespace OpcClassic.Batch.Dcom;

/// <summary><c>IOPCBatchServer</c> — Batch 1.0 top-level browse/enumeration interface (IID_IOPCBatchServer).</summary>
[OpcInterface("8BB4ED50-B314-11D3-B3EA-00C04F8ECEAA")]
[GenerateOpcProxy]
public partial interface IOPCBatchServer
{
    /// <summary><c>IOPCBatchServer::GetDelimiter</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<string> GetDelimiterAsync(CancellationToken cancellationToken = default);

    // CreateEnumerator returns an interface pointer and remains deferred.
}

/// <summary><c>IOPCBatchServer2</c> — Batch 2.0 filtered batch-summary enumeration interface (IID_IOPCBatchServer2).</summary>
[OpcInterface("895A78CF-B0C5-11D4-A0B7-000102A980B1")]
[GenerateOpcProxy]
public partial interface IOPCBatchServer2
{
    // CreateFilteredEnumerator returns an interface pointer and remains deferred.
}

/// <summary><c>IEnumOPCBatchSummary</c> — enumeration of batch summaries (IID_IEnumOPCBatchSummary).</summary>
[OpcInterface("A8080DA2-E23E-11D2-AFA7-00C04F539421")]
[GenerateOpcProxy]
public partial interface IEnumOPCBatchSummary
{
    /// <summary><c>IEnumOPCBatchSummary::Next</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<OpcBatchSummary[]> NextAsync(int count, CancellationToken cancellationToken = default);

    /// <summary><c>IEnumOPCBatchSummary::Skip</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task SkipAsync(int count, CancellationToken cancellationToken = default);

    /// <summary><c>IEnumOPCBatchSummary::Reset</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task ResetAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IEnumOPCBatchSummary::Count</c> (opnum 7).</summary>
    [OpcMethod(7)]
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    // Clone returns IEnumOPCBatchSummary and remains deferred.
}

/// <summary><c>IOPCEnumerationSets</c> — Batch enumeration sets for physical/procedural/state/mode classification (IID_IOPCEnumerationSets).</summary>
[OpcInterface("A8080DA3-E23E-11D2-AFA7-00C04F539421")]
[GenerateOpcProxy]
public partial interface IOPCEnumerationSets
{
    /// <summary><c>IOPCEnumerationSets::QueryEnumeration</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<string> QueryEnumerationAsync(int enumerationSetId, int enumerationValue, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCEnumerationSets::QueryEnumerationList</c> (opnum 5) projected as enumeration names.</summary>
    [OpcMethod(5)]
    Task<string[]> QueryEnumerationListAsync(int enumerationSetId, CancellationToken cancellationToken = default);

    // QueryEnumerationSets has two parallel output arrays and waits for an explicit record type.
}
