//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // Dispatcher contract and browse-filter DTO are grouped for API locality.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Hda.Hosting;

/// <summary>Dispatches NDR-encoded HDA DCOM calls to a managed HDA server implementation.</summary>
public interface IOpcHdaServerDispatcher
{
    /// <summary>Routes an incoming interface/opnum request and returns an HRESULT plus NDR response body.</summary>
    Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken);

    /// <summary>Validates HDA browse filters and returns one HRESULT per filter.</summary>
    Task<int[]> ValidateBrowseFiltersAsync(
        IReadOnlyList<OpcHdaBrowseFilter> filters,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filters);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new int[filters.Count]);
    }

    /// <summary>Returns display strings for the requested HDA browse type at the supplied branch position.</summary>
    Task<IReadOnlyList<string>> BrowseAsync(
        string branchPosition,
        HdaBrowseType browseType,
        IReadOnlyList<OpcHdaBrowseFilter> filters,
        CancellationToken cancellationToken = default)
    {
        _ = branchPosition;
        _ = browseType;
        ArgumentNullException.ThrowIfNull(filters);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<string>>(Array.Empty<string>());
    }

    /// <summary>Returns the next branch position for an HDA browser cursor move.</summary>
    Task<string> ChangeBrowsePositionAsync(
        string currentBranchPosition,
        int browseDirection,
        string? browseString,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(browseDirection switch
        {
            1 => MoveUp(currentBranchPosition),
            2 when !string.IsNullOrEmpty(browseString) => string.IsNullOrEmpty(currentBranchPosition)
                ? browseString
                : currentBranchPosition + "." + browseString,
            3 => browseString ?? string.Empty,
            _ => throw new OpcException(OpcResultId.InvalidArg),
        });
    }

    /// <summary>Resolves an HDA browse node to a fully qualified item ID.</summary>
    Task<string> GetItemIdAsync(
        string branchPosition,
        string node,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(branchPosition) || string.IsNullOrEmpty(node))
        {
            return Task.FromResult(node);
        }

        return Task.FromResult(node.StartsWith(branchPosition + ".", StringComparison.Ordinal)
            ? node
            : branchPosition + "." + node);
    }

    /// <summary>Returns the browser's current branch position string.</summary>
    Task<string> GetBranchPositionAsync(
        string branchPosition,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(branchPosition);
    }

    /// <summary>Returns the server's HDA update capability bitmask.</summary>
    Task<int> UpdateCapabilitiesAsync(CancellationToken cancellationToken = default) =>
        Task.FromException<int>(NotImplemented());

    /// <summary>Synchronously inserts historical values.</summary>
    Task<int[]> InsertAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        _ = serverHandles;
        _ = timestampFileTimes;
        _ = dataValues;
        _ = qualities;
        return Task.FromException<int[]>(NotImplemented());
    }

    /// <summary>Synchronously replaces historical values at exact timestamps.</summary>
    Task<int[]> ReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        _ = serverHandles;
        _ = timestampFileTimes;
        _ = dataValues;
        _ = qualities;
        return Task.FromException<int[]>(NotImplemented());
    }

    /// <summary>Synchronously inserts or replaces historical values.</summary>
    Task<int[]> InsertReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        _ = serverHandles;
        _ = timestampFileTimes;
        _ = dataValues;
        _ = qualities;
        return Task.FromException<int[]>(NotImplemented());
    }

    /// <summary>Synchronously deletes raw values in a historical range.</summary>
    Task<int[]> DeleteRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        _ = startTime;
        _ = endTime;
        _ = serverHandles;
        return Task.FromException<int[]>(NotImplemented());
    }

    /// <summary>Synchronously deletes values at exact timestamps.</summary>
    Task<int[]> DeleteAtTimeAsync(int[] serverHandles, long[] timestampFileTimes, CancellationToken cancellationToken = default)
    {
        _ = serverHandles;
        _ = timestampFileTimes;
        return Task.FromException<int[]>(NotImplemented());
    }

    /// <summary>Begins an asynchronous insert and returns its cancel ID, callback handles, and immediate item errors.</summary>
    Task<OpcHdaAsyncUpdateResult> BeginAsyncInsertAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        _ = transactionId;
        _ = serverHandles;
        _ = timestampFileTimes;
        _ = dataValues;
        _ = qualities;
        return Task.FromException<OpcHdaAsyncUpdateResult>(NotImplemented());
    }

    /// <summary>Begins an asynchronous replace and returns its cancel ID, callback handles, and immediate item errors.</summary>
    Task<OpcHdaAsyncUpdateResult> BeginAsyncReplaceAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        _ = transactionId;
        _ = serverHandles;
        _ = timestampFileTimes;
        _ = dataValues;
        _ = qualities;
        return Task.FromException<OpcHdaAsyncUpdateResult>(NotImplemented());
    }

    /// <summary>Begins an asynchronous insert-or-replace and returns its cancel ID, callback handles, and immediate item errors.</summary>
    Task<OpcHdaAsyncUpdateResult> BeginAsyncInsertReplaceAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken = default)
    {
        _ = transactionId;
        _ = serverHandles;
        _ = timestampFileTimes;
        _ = dataValues;
        _ = qualities;
        return Task.FromException<OpcHdaAsyncUpdateResult>(NotImplemented());
    }

    /// <summary>Begins an asynchronous raw-range delete and returns its cancel ID, callback handles, and immediate item errors.</summary>
    Task<OpcHdaAsyncUpdateResult> BeginAsyncDeleteRawAsync(int transactionId, OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken = default)
    {
        _ = transactionId;
        _ = startTime;
        _ = endTime;
        _ = serverHandles;
        return Task.FromException<OpcHdaAsyncUpdateResult>(NotImplemented());
    }

    /// <summary>Begins an asynchronous exact-time delete and returns its cancel ID, callback handles, and immediate item errors.</summary>
    Task<OpcHdaAsyncUpdateResult> BeginAsyncDeleteAtTimeAsync(int transactionId, int[] serverHandles, long[] timestampFileTimes, CancellationToken cancellationToken = default)
    {
        _ = transactionId;
        _ = serverHandles;
        _ = timestampFileTimes;
        return Task.FromException<OpcHdaAsyncUpdateResult>(NotImplemented());
    }

    /// <summary>Begins raw playback and yields callback batches until complete or cancelled.</summary>
    async IAsyncEnumerable<OpcHdaPlaybackEvent> BeginPlaybackRawAsync(
        int transactionId,
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        int maxValues,
        long updateDurationFileTime,
        long updateIntervalFileTime,
        int[] serverHandles,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = transactionId;
        _ = startTime;
        _ = endTime;
        _ = maxValues;
        _ = updateDurationFileTime;
        _ = updateIntervalFileTime;
        _ = serverHandles;
        cancellationToken.ThrowIfCancellationRequested();
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }

    /// <summary>Begins processed playback and yields callback batches until complete or cancelled.</summary>
    async IAsyncEnumerable<OpcHdaPlaybackEvent> BeginPlaybackProcessedAsync(
        int transactionId,
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        long resampleIntervalFileTime,
        int intervalCount,
        long updateIntervalFileTime,
        int[] serverHandles,
        int[] aggregateIds,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _ = transactionId;
        _ = startTime;
        _ = endTime;
        _ = resampleIntervalFileTime;
        _ = intervalCount;
        _ = updateIntervalFileTime;
        _ = serverHandles;
        _ = aggregateIds;
        cancellationToken.ThrowIfCancellationRequested();
        await Task.CompletedTask.ConfigureAwait(false);
        yield break;
    }

    /// <summary>Cancels an asynchronous update or playback operation.</summary>
    Task CancelAsync(int cancelId, CancellationToken cancellationToken = default)
    {
        _ = cancelId;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    /// <summary>Inserts HDA annotations and returns one HRESULT per requested server handle.</summary>
    Task<int[]> InsertAnnotationsAsync(
        int[] serverHandles,
        long[] timestampFileTimes,
        OpcHdaAnnotation[] annotationValues,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        ArgumentNullException.ThrowIfNull(annotationValues);
        cancellationToken.ThrowIfCancellationRequested();
        throw new OpcException(OpcResultId.NotImplemented);
    }

    /// <summary>Starts an HDA raw advise stream and returns call-time item validation plus update events.</summary>
    Task<OpcHdaAdviseSubscription> AdviseRawAsync(
        int[] serverHandles,
        OpcHdaTime startTime,
        long updateIntervalFileTime,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(startTime);
        _ = updateIntervalFileTime;
        cancellationToken.ThrowIfCancellationRequested();
        throw new OpcException(OpcResultId.NotImplemented);
    }

    /// <summary>Starts an HDA processed advise stream and returns call-time item validation plus update events.</summary>
    Task<OpcHdaAdviseSubscription> AdviseProcessedAsync(
        int[] serverHandles,
        OpcHdaTime startTime,
        long resampleIntervalFileTime,
        int[] aggregateHandles,
        int intervalCount,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(aggregateHandles);
        _ = resampleIntervalFileTime;
        _ = intervalCount;
        cancellationToken.ThrowIfCancellationRequested();
        throw new OpcException(OpcResultId.NotImplemented);
    }

    private static OpcException NotImplemented() => new(OpcResultId.NotImplemented);

    private static string MoveUp(string position)
    {
        if (string.IsNullOrEmpty(position))
        {
            return string.Empty;
        }

        int lastDot = position.LastIndexOf('.');
        return lastDot < 0 ? string.Empty : position[..lastDot];
    }
}

/// <summary>One HDA browser attribute filter supplied to <c>IOPCHDA_Server::CreateBrowse</c>.</summary>
/// <param name="AttributeId">The HDA attribute ID being filtered.</param>
/// <param name="OperatorCode">The <c>OPCHDA_OPERATORCODES</c> comparison operator.</param>
/// <param name="Value">The native VARIANT filter value converted to a managed carrier.</param>
public sealed record OpcHdaBrowseFilter(int AttributeId, int OperatorCode, OpcVariant Value);

/// <summary>Immediate result metadata for an HDA asynchronous update call.</summary>
/// <param name="CancelId">Server-supplied cancel ID, or 0 when the CCW should allocate one.</param>
/// <param name="ClientHandles">Handles to echo through <c>IOPCHDA_DataCallback::OnUpdateComplete</c>.</param>
/// <param name="Errors">Per-item immediate HRESULTs.</param>
public sealed record OpcHdaAsyncUpdateResult(int CancelId, int[] ClientHandles, int[] Errors);

/// <summary>One playback callback batch for <c>IOPCHDA_DataCallback::OnPlayback</c>.</summary>
/// <param name="Status">Master HRESULT for the batch.</param>
/// <param name="Items">Playback item values.</param>
/// <param name="Errors">Per-item HRESULTs.</param>
public sealed record OpcHdaPlaybackEvent(int Status, OpcHdaItem[] Items, int[] Errors);

/// <summary>One update emitted by an HDA advise stream.</summary>
/// <param name="ItemValues">HDA item values for the update interval.</param>
/// <param name="Errors">Per-item HRESULTs for <paramref name="ItemValues" />.</param>
public sealed record OpcHdaDataUpdate(OpcHdaItem[] ItemValues, int[] Errors);

/// <summary>Call-time validation plus the update stream for an HDA advise request.</summary>
/// <param name="Errors">Per-requested-item HRESULTs returned from the initiating advise call.</param>
/// <param name="Updates">Periodic HDA data-change updates.</param>
public sealed record OpcHdaAdviseSubscription(int[] Errors, IAsyncEnumerable<OpcHdaDataUpdate> Updates);
