//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// OPC HDA DCOM-projection interfaces. [GenerateOpcProxy] and
// [OpcGenerateServerDispatch] are applied broadly; [OpcMethod] opnums
// follow interop\inc\opchda.idl exactly (IUnknown slots 0-2).
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCHDA_Server with underscore)
#pragma warning disable MA0048 // Multiple small interface projections grouped for readability

using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dcom;
using Opc.Classic.Generators;
using Opc.Classic.Hda;

namespace Opc.Classic.Hda.Dcom;

/// <summary><c>IOPCHDA_Server</c> — top-level HDA server interface (IID_IOPCHDA_Server).</summary>
[OpcInterface("1F1217B0-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_Server
{
    /// <summary><c>IOPCHDA_Server::GetItemAttributes</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task GetItemAttributesAsync(out int[] attributeIds, out string[] attributeNames, out string[] attributeDescriptions, out int[] attributeDataTypes, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Server::GetAggregates</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task GetAggregatesAsync(out int[] aggregateIds, out string[] aggregateNames, out string[] aggregateDescriptions, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Server::GetHistorianStatus</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Server::GetItemHandles</c> (opnum 6).</summary>
    [OpcMethod(6)]
    [return: OpcUniquePointer]
    Task<int[]> GetItemHandlesAsync([OpcEmitArrayCount, OpcDeferredElements] string[] itemIds, int[] clientHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Server::ReleaseItemHandles</c> (opnum 7).</summary>
    [OpcMethod(7)]
    [return: OpcUniquePointer]
    Task<int[]> ReleaseItemHandlesAsync([OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Server::ValidateItemIDs</c> (opnum 8).</summary>
    [OpcMethod(8)]
    [return: OpcUniquePointer]
    Task<int[]> ValidateItemIDsAsync([OpcEmitArrayCount, OpcDeferredElements] string[] itemIds, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCHDA_Browser</c> — HDA address-space browse (IID_IOPCHDA_Browser).</summary>
[OpcInterface("1F1217B1-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
public partial interface IOPCHDA_Browser
{
    /// <summary><c>IOPCHDA_Browser::GetEnum</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<IOpcInterfaceRef> GetEnumAsync(int browseType, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Browser::ChangeBrowsePosition</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task ChangeBrowsePositionAsync(int browseDirection, string browseString, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Browser::GetItemID</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task<string> GetItemIDAsync(string node, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Browser::GetBranchPosition</c> (opnum 6).</summary>
    [OpcMethod(6)]
    Task<string> GetBranchPositionAsync(CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCHDA_SyncRead</c> — synchronous HDA read (IID_IOPCHDA_SyncRead).</summary>
[OpcInterface("1F1217B2-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_SyncRead
{
    /// <summary><c>IOPCHDA_SyncRead::ReadRaw</c> (opnum 3).</summary>
    [OpcMethod(3)]
    [return: OpcUniquePointer]
    Task<OpcHdaItem[]> ReadRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, bool bounds, [OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncRead::ReadProcessed</c> (opnum 4).</summary>
    [OpcMethod(4)]
    [return: OpcUniquePointer]
    Task<OpcHdaItem[]> ReadProcessedAsync(OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, [OpcEmitArrayCount] int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncRead::ReadAtTime</c> (opnum 5).</summary>
    [OpcMethod(5)]
    [return: OpcUniquePointer]
    Task<OpcHdaItem[]> ReadAtTimeAsync([OpcEmitArrayCount, OpcFileTimeElements] long[] timestampFileTimes, [OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncRead::ReadModified</c> (opnum 6).</summary>
    [OpcMethod(6)]
    [return: OpcUniquePointer]
    Task<OpcHdaModifiedItem[]> ReadModifiedAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, [OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncRead::ReadAttribute</c> (opnum 7).</summary>
    [OpcMethod(7)]
    [return: OpcUniquePointer]
    Task<OpcHdaAttribute[]> ReadAttributeAsync(OpcHdaTime startTime, OpcHdaTime endTime, int serverHandle, [OpcEmitArrayCount] int[] attributeIds, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCHDA_SyncUpdate</c> — synchronous HDA insert/replace/delete (IID_IOPCHDA_SyncUpdate).</summary>
[OpcInterface("1F1217B3-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_SyncUpdate
{
    /// <summary><c>IOPCHDA_SyncUpdate::QueryCapabilities</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncUpdate::Insert</c> (opnum 4).</summary>
    [OpcMethod(4)]
    [return: OpcUniquePointer]
    Task<int[]> InsertAsync([OpcEmitArrayCount] int[] serverHandles, [OpcFileTimeElements] long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncUpdate::Replace</c> (opnum 5).</summary>
    [OpcMethod(5)]
    [return: OpcUniquePointer]
    Task<int[]> ReplaceAsync([OpcEmitArrayCount] int[] serverHandles, [OpcFileTimeElements] long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncUpdate::InsertReplace</c> (opnum 6).</summary>
    [OpcMethod(6)]
    [return: OpcUniquePointer]
    Task<int[]> InsertReplaceAsync([OpcEmitArrayCount] int[] serverHandles, [OpcFileTimeElements] long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncUpdate::DeleteRaw</c> (opnum 7).</summary>
    [OpcMethod(7)]
    [return: OpcUniquePointer]
    Task<int[]> DeleteRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, [OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncUpdate::DeleteAtTime</c> (opnum 8).</summary>
    [OpcMethod(8)]
    [return: OpcUniquePointer]
    Task<int[]> DeleteAtTimeAsync([OpcEmitArrayCount] int[] serverHandles, [OpcFileTimeElements] long[] timestampFileTimes, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCHDA_SyncAnnotations</c> — synchronous HDA annotation management (IID_IOPCHDA_SyncAnnotations).</summary>
[OpcInterface("1F1217B4-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_SyncAnnotations
{
    /// <summary><c>IOPCHDA_SyncAnnotations::QueryCapabilities</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncAnnotations::Read</c> (opnum 4).</summary>
    [OpcMethod(4)]
    [return: OpcUniquePointer]
    Task<OpcHdaAnnotation[]> ReadAsync(OpcHdaTime startTime, OpcHdaTime endTime, [OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncAnnotations::Insert</c> (opnum 5).</summary>
    [OpcMethod(5)]
    [return: OpcUniquePointer]
    Task<int[]> InsertAsync([OpcEmitArrayCount] int[] serverHandles, [OpcFileTimeElements] long[] timestampFileTimes, OpcHdaAnnotation[] annotationValues, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCHDA_AsyncRead</c> — asynchronous HDA read (IID_IOPCHDA_AsyncRead).</summary>
[OpcInterface("1F1217B5-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_AsyncRead
{
    /// <summary><c>IOPCHDA_AsyncRead::ReadRaw</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<int> ReadRawAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, bool bounds, [OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncRead::AdviseRaw</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<int> AdviseRawAsync(int transactionId, OpcHdaTime startTime, long updateIntervalFileTime, [OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncRead::ReadProcessed</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task<int> ReadProcessedAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, [OpcEmitArrayCount] int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncRead::AdviseProcessed</c> (opnum 6).</summary>
    [OpcMethod(6)]
    Task<int> AdviseProcessedAsync(int transactionId, OpcHdaTime startTime, long resampleIntervalFileTime, [OpcEmitArrayCount] int[] serverHandles, int[] aggregateIds, int intervalCount, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncRead::ReadAtTime</c> (opnum 7).</summary>
    [OpcMethod(7)]
    Task<int> ReadAtTimeAsync(int transactionId, [OpcEmitArrayCount, OpcFileTimeElements] long[] timestampFileTimes, int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncRead::ReadModified</c> (opnum 8).</summary>
    [OpcMethod(8)]
    Task<int> ReadModifiedAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, [OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncRead::ReadAttribute</c> (opnum 9).</summary>
    [OpcMethod(9)]
    Task<int> ReadAttributeAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int serverHandle, [OpcEmitArrayCount] int[] attributeIds, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncRead::Cancel</c> (opnum 10).</summary>
    [OpcMethod(10)]
    Task CancelAsync(int cancelId, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCHDA_AsyncUpdate</c> — asynchronous HDA insert/replace/delete (IID_IOPCHDA_AsyncUpdate).</summary>
[OpcInterface("1F1217B6-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_AsyncUpdate
{
    /// <summary><c>IOPCHDA_AsyncUpdate::QueryCapabilities</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncUpdate::Insert</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<int> InsertAsync(int transactionId, [OpcEmitArrayCount] int[] serverHandles, [OpcFileTimeElements] long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncUpdate::Replace</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task<int> ReplaceAsync(int transactionId, [OpcEmitArrayCount] int[] serverHandles, [OpcFileTimeElements] long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncUpdate::InsertReplace</c> (opnum 6).</summary>
    [OpcMethod(6)]
    Task<int> InsertReplaceAsync(int transactionId, [OpcEmitArrayCount] int[] serverHandles, [OpcFileTimeElements] long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncUpdate::DeleteRaw</c> (opnum 7).</summary>
    [OpcMethod(7)]
    Task<int> DeleteRawAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, [OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncUpdate::DeleteAtTime</c> (opnum 8).</summary>
    [OpcMethod(8)]
    Task<int> DeleteAtTimeAsync(int transactionId, [OpcEmitArrayCount] int[] serverHandles, [OpcFileTimeElements] long[] timestampFileTimes, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncUpdate::Cancel</c> (opnum 9).</summary>
    [OpcMethod(9)]
    Task CancelAsync(int cancelId, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCHDA_AsyncAnnotations</c> — asynchronous HDA annotation management (IID_IOPCHDA_AsyncAnnotations).</summary>
[OpcInterface("1F1217B7-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_AsyncAnnotations
{
    /// <summary><c>IOPCHDA_AsyncAnnotations::QueryCapabilities</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncAnnotations::Read</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<int> ReadAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, [OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncAnnotations::Insert</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task<int> InsertAsync(int transactionId, [OpcEmitArrayCount] int[] serverHandles, [OpcFileTimeElements] long[] timestampFileTimes, OpcHdaAnnotation[] annotationValues, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncAnnotations::Cancel</c> (opnum 6).</summary>
    [OpcMethod(6)]
    Task CancelAsync(int cancelId, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCHDA_Playback</c> — HDA playback, server pushes history at rate (IID_IOPCHDA_Playback).</summary>
[OpcInterface("1F1217B8-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_Playback
{
    /// <summary><c>IOPCHDA_Playback::ReadRawWithUpdate</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<int> ReadRawWithUpdateAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, long updateDurationFileTime, long updateIntervalFileTime, [OpcEmitArrayCount] int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Playback::ReadProcessedWithUpdate</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<int> ReadProcessedWithUpdateAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, int intervalCount, long updateIntervalFileTime, [OpcEmitArrayCount] int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Playback::Cancel</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task CancelAsync(int cancelId, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCHDA_DataCallback</c> — HDA async-read / playback callback sink (IID_IOPCHDA_DataCallback).</summary>
/// <remarks><c>OpcInterfaceAttribute</c> has no callback marker yet; callback hosting is projected with the same generated proxy/dispatch path.</remarks>
[OpcInterface("1F1217B9-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_DataCallback
{
    /// <summary><c>IOPCHDA_DataCallback::OnDataChange</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task OnDataChangeAsync(int transactionId, int status, OpcHdaItem[] itemValues, int[] errors, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_DataCallback::OnReadComplete</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task OnReadCompleteAsync(int transactionId, int status, OpcHdaItem[] itemValues, int[] errors, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_DataCallback::OnReadModifiedComplete</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task OnReadModifiedCompleteAsync(int transactionId, int status, OpcHdaModifiedItem[] itemValues, int[] errors, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_DataCallback::OnReadAttributeComplete</c> (opnum 6).</summary>
    [OpcMethod(6)]
    Task OnReadAttributeCompleteAsync(int transactionId, int status, int clientHandle, OpcHdaAttribute[] attributeValues, int[] errors, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_DataCallback::OnReadAnnotations</c> (opnum 7).</summary>
    [OpcMethod(7)]
    Task OnReadAnnotationsAsync(int transactionId, int status, OpcHdaAnnotation[] annotationValues, int[] errors, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_DataCallback::OnInsertAnnotations</c> (opnum 8).</summary>
    [OpcMethod(8)]
    Task OnInsertAnnotationsAsync(int transactionId, int status, int[] clientHandles, int[] errors, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_DataCallback::OnPlayback</c> (opnum 9).</summary>
    [OpcMethod(9)]
    Task OnPlaybackAsync(int transactionId, int status, OpcHdaItem[] itemValues, int[] errors, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_DataCallback::OnUpdateComplete</c> (opnum 10).</summary>
    [OpcMethod(10)]
    Task OnUpdateCompleteAsync(int transactionId, int status, int[] clientHandles, int[] errors, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_DataCallback::OnCancelComplete</c> (opnum 11).</summary>
    [OpcMethod(11)]
    Task OnCancelCompleteAsync(int cancelId, CancellationToken cancellationToken = default);
}
