//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// OPC HDA DCOM-projection interfaces. [GenerateOpcProxy] and
// [OpcGenerateServerDispatch] are applied broadly;
// [OpcMethod] is limited to high-value methods whose managed signatures fit
// the current NDR codec registry.
//

#pragma warning disable CA1707 // OPC IDL naming preserved (IOPCHDA_Server with underscore)
#pragma warning disable MA0048 // Multiple small interface projections grouped for readability

using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Generators;
using Opc.Classic.Hda;

namespace Opc.Classic.Hda.Dcom;

/// <summary><c>IOPCHDA_Server</c> — top-level HDA server interface (IID_IOPCHDA_Server).</summary>
[OpcInterface("1F1217B0-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_Server
{
    /// <summary><c>IOPCHDA_Server::GetHistorianStatus</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Server::GetItemHandles</c> (opnum 6).</summary>
    [OpcMethod(6)]
    Task<int[]> GetItemHandlesAsync(string[] itemIds, int[] clientHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Server::ReleaseItemHandles</c> (opnum 7).</summary>
    [OpcMethod(7)]
    Task<int[]> ReleaseItemHandlesAsync(int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Server::ValidateItemIDs</c> (opnum 8).</summary>
    [OpcMethod(8)]
    Task<int[]> ValidateItemIDsAsync(string[] itemIds, CancellationToken cancellationToken = default);

    // GetItemAttributes/GetAggregates/CreateBrowse have multi-array or interface-pointer outputs.
}

/// <summary><c>IOPCHDA_Browser</c> — HDA address-space browse (IID_IOPCHDA_Browser).</summary>
[OpcInterface("1F1217B1-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_Browser
{
    // Enum and position methods return COM enumerators or browse-state multi-outs.
}

/// <summary><c>IOPCHDA_SyncRead</c> — synchronous HDA read (IID_IOPCHDA_SyncRead).</summary>
[OpcInterface("1F1217B2-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_SyncRead
{
    /// <summary><c>IOPCHDA_SyncRead::ReadRaw</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<OpcHdaItem[]> ReadRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, bool bounds, int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncRead::ReadProcessed</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<OpcHdaItem[]> ReadProcessedAsync(OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncRead::ReadAtTime</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task<OpcHdaItem[]> ReadAtTimeAsync(long[] timestampFileTimes, int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_SyncRead::ReadModified</c> (opnum 6).</summary>
    [OpcMethod(6)]
    Task<OpcHdaModifiedItem[]> ReadModifiedAsync(OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, int[] serverHandles, CancellationToken cancellationToken = default);

    // ReadAttribute returns OPCHDA_ATTRIBUTE plus per-item HRESULT arrays; defer its record shape.
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

    // Insert/replace/delete shapes require parallel arrays with per-item HRESULT outputs.
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

    // Read/insert annotations need explicit batch result records before proxy generation.
}

/// <summary><c>IOPCHDA_AsyncRead</c> — asynchronous HDA read (IID_IOPCHDA_AsyncRead).</summary>
[OpcInterface("1F1217B5-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_AsyncRead
{
    /// <summary><c>IOPCHDA_AsyncRead::ReadRaw</c> (opnum 3).</summary>
    [OpcMethod(3)]
    Task<int> ReadRawAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, bool bounds, int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncRead::AdviseRaw</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<int> AdviseRawAsync(int transactionId, OpcHdaTime startTime, long updateIntervalFileTime, int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncRead::ReadProcessed</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task<int> ReadProcessedAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_AsyncRead::AdviseProcessed</c> (opnum 6).</summary>
    [OpcMethod(6)]
    Task<int> AdviseProcessedAsync(int transactionId, OpcHdaTime startTime, long resampleIntervalFileTime, int[] serverHandles, int[] aggregateIds, int intervalCount, CancellationToken cancellationToken = default);

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
    Task<int> ReadRawWithUpdateAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int maxValues, long updateDurationFileTime, long updateIntervalFileTime, int[] serverHandles, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Playback::ReadProcessedWithUpdate</c> (opnum 4).</summary>
    [OpcMethod(4)]
    Task<int> ReadProcessedWithUpdateAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, long resampleIntervalFileTime, int intervalCount, long updateIntervalFileTime, int[] serverHandles, int[] aggregateIds, CancellationToken cancellationToken = default);

    /// <summary><c>IOPCHDA_Playback::Cancel</c> (opnum 5).</summary>
    [OpcMethod(5)]
    Task CancelAsync(int cancelId, CancellationToken cancellationToken = default);
}

/// <summary><c>IOPCHDA_DataCallback</c> — HDA async-read / playback callback sink (IID_IOPCHDA_DataCallback).</summary>
[OpcInterface("1F1217B9-DEE0-11D2-A5E5-000086339399")]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IOPCHDA_DataCallback
{
    // Callback methods carry complex multi-array completion payloads; defer until callback records land.
}
