// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.
// OPC Batch DCOM-projection interfaces for delimiter, enumeration, and Batch summary methods.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCBatchServer)
#pragma warning disable MA0048 // Multiple small interface projections grouped for readability

using Opc.Classic.Dcom;
using Opc.Classic.Generators;

namespace Opc.Classic.Batch.Dcom;

/// <summary>
/// <c>IOPCBatchServer</c> — Batch 1.0 top-level browse/enumeration interface (IID_IOPCBatchServer).
/// </summary>
[OpcInterface("8BB4ED50-B314-11D3-B3EA-00C04F8ECEAA")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCBatchServer
{
    /// <summary>
    /// <c>IOPCBatchServer::GetDelimiter</c> (opnum 3).
    /// </summary>
    [OpcMethod(3)]
    Task<string> GetDelimiterAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCBatchServer::CreateEnumerator</c> (opnum 4).
    /// </summary>
    [OpcMethod(4)]
    [return: OpcIidIs(nameof(riid))]
    Task<IOpcInterfaceRef> CreateEnumeratorAsync(Guid riid, CancellationToken cancellationToken = default);
}

/// <summary>
/// <c>IOPCBatchServer2</c> — Batch 2.0 filtered batch-summary enumeration interface (IID_IOPCBatchServer2).
/// </summary>
[OpcInterface("895A78CF-B0C5-11D4-A0B7-000102A980B1")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCBatchServer2
{
    /// <summary>
    /// <c>IOPCBatchServer2::CreateFilteredEnumerator</c> (opnum 3).
    /// </summary>
    [OpcMethod(3)]
    [return: OpcIidIs(nameof(riid))]
    Task<IOpcInterfaceRef> CreateFilteredEnumeratorAsync(Guid riid, OpcBatchSummaryFilter filter, string model, CancellationToken cancellationToken = default);
}

/// <summary>
/// <c>IEnumOPCBatchSummary</c> — enumeration of batch summaries (IID_IEnumOPCBatchSummary).
/// </summary>
[OpcInterface("A8080DA2-E23E-11D2-AFA7-00C04F539421")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IEnumOPCBatchSummary
{
    /// <summary>
    /// <c>IEnumOPCBatchSummary::Next</c> (opnum 3).
    /// </summary>
    /// <remarks>
    /// The response array length is the fetched count and can be less than
    /// <paramref name="count"/> at the end of the enumeration.
    /// </remarks>
    [OpcMethod(3)]
    [return: OpcUniquePointer]
    [return: OpcEnumeratorArray(nameof(count), conformantVarying: false)]
    Task<OpcBatchSummary[]> NextAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IEnumOPCBatchSummary::Skip</c> (opnum 4).
    /// </summary>
    [OpcMethod(4)]
    Task SkipAsync(int count, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IEnumOPCBatchSummary::Reset</c> (opnum 5).
    /// </summary>
    [OpcMethod(5)]
    Task ResetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IEnumOPCBatchSummary::Clone</c> (opnum 6).
    /// </summary>
    [OpcMethod(6)]
    Task<IOpcInterfaceRef> CloneAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IEnumOPCBatchSummary::Count</c> (opnum 7).
    /// </summary>
    [OpcMethod(7)]
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// <c>IOPCEnumerationSets</c> — Batch enumeration sets for physical/procedural/state/mode classification (IID_IOPCEnumerationSets).
/// </summary>
[OpcInterface("A8080DA3-E23E-11D2-AFA7-00C04F539421")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCEnumerationSets
{
    /// <summary>
    /// <c>IOPCEnumerationSets::QueryEnumerationSets</c> (opnum 3).
    /// </summary>
    [OpcMethod(3)]
    [OpcGenerateMultiOutRecord]
    Task QueryEnumerationSetsAsync(out int[] enumerationSetIds, out string[] enumerationSetNames, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCEnumerationSets::QueryEnumeration</c> (opnum 4).
    /// </summary>
    [OpcMethod(4)]
    Task<string> QueryEnumerationAsync(int enumerationSetId, int enumerationValue, CancellationToken cancellationToken = default);

    /// <summary>
    /// <c>IOPCEnumerationSets::QueryEnumerationList</c> (opnum 5).
    /// </summary>
    [OpcMethod(5)]
    [OpcGenerateMultiOutRecord]
    Task QueryEnumerationListAsync(int enumerationSetId, out int[] enumerationValues, out string[] enumerationNames, CancellationToken cancellationToken = default);
}
