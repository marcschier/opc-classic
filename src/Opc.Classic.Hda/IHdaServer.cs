//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Hda;

/// <summary>
/// The managed async-first OPC HDA server contract.
/// </summary>
/// <remarks>
/// Unlike DA/AE, HDA does not have a long-lived subscription concept on the
/// client side — each read is a one-shot operation. The async OPC HDA
/// interfaces (IOPCHDA_AsyncRead etc.) deliver completion via callback;
/// this managed shape hides that complexity behind <see cref="Task{T}"/>.
/// </remarks>
public interface IHdaServer : IAsyncDisposable
{
    /// <summary>Raised when the server emits a shutdown notification.</summary>
    event EventHandler<EventArgs>? ServerShutdown;

    /// <summary>Retrieve HDA server runtime state.</summary>
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Browse the HDA address space starting at <paramref name="itemIdPrefix"/>
    /// (use empty string for the root).
    /// </summary>
    IAsyncEnumerable<HdaBrowseElement> BrowseAsync(
        string itemIdPrefix,
        HdaBrowseType browseType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// List the built-in aggregates this server supports (subset of
    /// <see cref="HdaAggregate"/>).
    /// </summary>
    Task<IReadOnlyList<HdaAggregate>> GetSupportedAggregatesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Read raw historical values for <paramref name="itemIds"/> in the
    /// half-open interval [<paramref name="startTime"/>, <paramref name="endTime"/>).
    /// </summary>
    /// <param name="itemIds">Item identifiers.</param>
    /// <param name="startTime">Inclusive lower bound (absolute or NOW-relative).</param>
    /// <param name="endTime">Exclusive upper bound (absolute or NOW-relative).</param>
    /// <param name="maxValuesPerItem">
    /// Server-side cap on values per item, or 0 for unlimited (subject to
    /// server-side limits — see <see cref="OpcServerStatus.MaxReturnValues"/>).
    /// </param>
    /// <param name="includeBounds">If true, the values at exactly startTime / endTime are included.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<HdaReadResult>> ReadRawAsync(
        IReadOnlyList<string> itemIds,
        HdaTime startTime,
        HdaTime endTime,
        int maxValuesPerItem,
        bool includeBounds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Read processed (aggregated) values across <paramref name="resampleInterval"/>
    /// buckets in the interval [<paramref name="startTime"/>, <paramref name="endTime"/>).
    /// </summary>
    Task<IReadOnlyList<HdaReadResult>> ReadProcessedAsync(
        IReadOnlyList<AggregateRequest> requests,
        HdaTime startTime,
        HdaTime endTime,
        TimeSpan resampleInterval,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Read interpolated values at specific <paramref name="timestamps"/>
    /// (server interpolates between raw samples).
    /// </summary>
    Task<IReadOnlyList<HdaReadResult>> ReadAtTimeAsync(
        IReadOnlyList<string> itemIds,
        IReadOnlyList<DateTimeOffset> timestamps,
        CancellationToken cancellationToken = default);

    /// <summary>Read annotations for the given items in the given time range.</summary>
    Task<IReadOnlyList<HdaAnnotationResult>> ReadAnnotationsAsync(
        IReadOnlyList<string> itemIds,
        HdaTime startTime,
        HdaTime endTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Continue a paged read using a continuation handle from a prior
    /// <see cref="HdaReadResult.ContinuationHandle"/>.
    /// </summary>
    Task<IReadOnlyList<HdaReadResult>> ReadNextAsync(
        IReadOnlyList<string> itemIds,
        IReadOnlyList<int> continuationHandles,
        int maxValuesPerItem,
        CancellationToken cancellationToken = default);
}
