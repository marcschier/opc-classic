//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;

namespace Opc.Classic.Mcp.Tools;

/// <summary>
/// One DA item value delivered inside a <see cref="DataChangeNotification"/>.
/// Identified by client handle because that's what the OPC DA wire delivers
/// on the <c>IOPCDataCallback::OnDataChange</c> path (MS-OPC-DA §3.5).
/// </summary>
public sealed record DataChangeItem(
    int ClientHandle,
    OpcVariant Value,
    OpcQuality Quality,
    DateTimeOffset Timestamp,
    int HResult);

/// <summary>
/// A single <c>IOPCDataCallback::OnDataChange</c> / <c>OnReadComplete</c>
/// batch as received from the server. <see cref="Items"/> may be empty when
/// the server signals a master-level transition without per-item values.
/// </summary>
public sealed record DataChangeNotification(
    int TransactionId,
    int GroupHandle,
    int MasterQuality,
    int MasterError,
    IReadOnlyList<DataChangeItem> Items,
    DateTimeOffset ReceivedAt);

/// <summary>
/// Server-pushed callback sink for one DA subscription. Buffers
/// <see cref="IOPCDataCallback.OnDataChangeAsync"/> and
/// <see cref="IOPCDataCallback.OnReadCompleteAsync"/> batches into a bounded
/// channel that <c>opcclassic.da.poll_subscription</c> drains.
/// </summary>
/// <remarks>
/// <para>
/// The channel is bounded with drop-oldest semantics so a stalled MCP client
/// (or an over-eager Matrikon callback fan-out) cannot grow the queue
/// without bound. The drop counter is exposed via <see cref="DroppedNotifications"/>
/// for diagnostics.
/// </para>
/// <para>
/// <see cref="IOPCDataCallback.OnWriteCompleteAsync"/> and
/// <see cref="IOPCDataCallback.OnCancelCompleteAsync"/> are accepted to honor
/// the full interface contract but are not enqueued — they're recorded as
/// counters only because <c>poll_subscription</c> doesn't surface them today.
/// </para>
/// <para>
/// Production inbound-callback wiring (an <c>OpcServerListener</c> hosting an
/// <c>IOPCDataCallbackServerDispatcher</c> with this sink as the
/// implementation) is still deferred — see <c>interop/docs/da-callbacks.md</c>
/// for the AP track status. Today the sink is constructed eagerly when
/// <c>opcclassic.da.subscribe</c> creates a <c>DaSubscriptionContext</c> so
/// the queue contract is testable in isolation.
/// </para>
/// </remarks>
public sealed class DaDataCallbackSink : IOPCDataCallback, IDisposable
{
    /// <summary>Default queue capacity if the caller does not specify one.</summary>
    public const int DefaultCapacity = 1024;

    private readonly Channel<DataChangeNotification> _queue;
    private readonly Func<DateTimeOffset> _clock;
    private readonly int _capacity;
    private long _droppedNotifications;
    private long _onDataChangeCount;
    private long _onReadCompleteCount;
    private long _onWriteCompleteCount;
    private long _onCancelCompleteCount;
    private bool _disposed;

    /// <summary>Creates a sink with the default capacity (<see cref="DefaultCapacity"/>).</summary>
    public DaDataCallbackSink()
        : this(DefaultCapacity, clock: null)
    {
    }

    /// <summary>Creates a sink with the supplied capacity and optional clock override (for tests).</summary>
    public DaDataCallbackSink(int capacity, Func<DateTimeOffset>? clock)
    {
        if (capacity < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be at least 1.");
        }

        _queue = Channel.CreateBounded<DataChangeNotification>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = false,
            SingleWriter = false,
        });
        _clock = clock ?? (() => DateTimeOffset.UtcNow);
        _capacity = capacity;
    }

    /// <summary>Number of <see cref="OnDataChangeAsync"/> invocations seen since creation.</summary>
    public long OnDataChangeCount => Interlocked.Read(ref _onDataChangeCount);

    /// <summary>Number of <see cref="OnReadCompleteAsync"/> invocations seen since creation.</summary>
    public long OnReadCompleteCount => Interlocked.Read(ref _onReadCompleteCount);

    /// <summary>Number of <see cref="OnWriteCompleteAsync"/> invocations seen since creation.</summary>
    public long OnWriteCompleteCount => Interlocked.Read(ref _onWriteCompleteCount);

    /// <summary>Number of <see cref="OnCancelCompleteAsync"/> invocations seen since creation.</summary>
    public long OnCancelCompleteCount => Interlocked.Read(ref _onCancelCompleteCount);

    /// <summary>Number of notifications dropped due to a full bounded queue.</summary>
    public long DroppedNotifications => Interlocked.Read(ref _droppedNotifications);

    /// <summary>True once at least one <see cref="OnDataChangeAsync"/> or <see cref="OnReadCompleteAsync"/> has been observed.</summary>
    public bool HasReceivedAnyData =>
        Interlocked.Read(ref _onDataChangeCount) + Interlocked.Read(ref _onReadCompleteCount) > 0;

    /// <summary>Drains up to <paramref name="maxItems"/> flattened items from the queue.</summary>
    /// <remarks>
    /// <para>
    /// Returns one <see cref="DataChangeItem"/> per item in the order they were
    /// enqueued. A partially-drained batch leaves its trailing items at the head
    /// of the queue so the next poll picks up exactly where the previous one
    /// stopped.
    /// </para>
    /// <para>
    /// When <paramref name="maxItems"/> &lt;= 0 every queued batch is drained
    /// in full.
    /// </para>
    /// </remarks>
    public IReadOnlyList<DataChangeItem> DrainItems(int maxItems)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        bool unbounded = maxItems <= 0;
        var drained = new List<DataChangeItem>(unbounded ? 16 : maxItems);
        while (unbounded || drained.Count < maxItems)
        {
            if (!_queue.Reader.TryRead(out DataChangeNotification? batch))
            {
                break;
            }

            int batchCount = batch.Items.Count;
            int remainingSlots = unbounded ? batchCount : maxItems - drained.Count;
            int takeCount = batchCount <= remainingSlots ? batchCount : remainingSlots;
            for (int i = 0; i < takeCount; i++)
            {
                drained.Add(batch.Items[i]);
            }

            if (takeCount < batchCount)
            {
                // Re-queue the unread tail at the head of the channel so the next
                // poll picks up exactly where this drain stopped. Bounded
                // drop-oldest semantics tolerate the round-trip.
                var leftover = new List<DataChangeItem>(batchCount - takeCount);
                for (int i = takeCount; i < batchCount; i++)
                {
                    leftover.Add(batch.Items[i]);
                }

                var partial = new DataChangeNotification(
                    batch.TransactionId,
                    batch.GroupHandle,
                    batch.MasterQuality,
                    batch.MasterError,
                    leftover,
                    batch.ReceivedAt);
                if (!_queue.Writer.TryWrite(partial))
                {
                    Interlocked.Increment(ref _droppedNotifications);
                }

                break;
            }
        }

        return drained;
    }

    /// <inheritdoc/>
    public Task OnDataChangeAsync(
        int transactionId,
        int groupHandle,
        int masterQuality,
        int masterError,
        int[] clientHandles,
        OpcVariant[] values,
        ushort[] qualities,
        long[] timestamps,
        int[] errors,
        CancellationToken cancellationToken = default)
    {
        EnqueueBatch(transactionId, groupHandle, masterQuality, masterError, clientHandles, values, qualities, timestamps, errors);
        Interlocked.Increment(ref _onDataChangeCount);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task OnReadCompleteAsync(
        int transactionId,
        int groupHandle,
        int masterQuality,
        int masterError,
        int[] clientHandles,
        OpcVariant[] values,
        ushort[] qualities,
        long[] timestamps,
        int[] errors,
        CancellationToken cancellationToken = default)
    {
        EnqueueBatch(transactionId, groupHandle, masterQuality, masterError, clientHandles, values, qualities, timestamps, errors);
        Interlocked.Increment(ref _onReadCompleteCount);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task OnWriteCompleteAsync(
        int transactionId,
        int groupHandle,
        int masterError,
        int[] clientHandles,
        int[] errors,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _onWriteCompleteCount);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task OnCancelCompleteAsync(int transactionId, int groupHandle, CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _onCancelCompleteCount);
        return Task.CompletedTask;
    }

    /// <summary>Completes the channel writer; further enqueue attempts are dropped.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _queue.Writer.TryComplete();
    }

    private void EnqueueBatch(
        int transactionId,
        int groupHandle,
        int masterQuality,
        int masterError,
        int[] clientHandles,
        OpcVariant[] values,
        ushort[] qualities,
        long[] timestamps,
        int[] errors)
    {
        ArgumentNullException.ThrowIfNull(clientHandles);
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(qualities);
        ArgumentNullException.ThrowIfNull(timestamps);
        ArgumentNullException.ThrowIfNull(errors);

        int count = clientHandles.Length;
        if (values.Length != count || qualities.Length != count || timestamps.Length != count || errors.Length != count)
        {
            throw new ArgumentException(
                "DataCallback array lengths must match (clientHandles=" + clientHandles.Length
                + " values=" + values.Length
                + " qualities=" + qualities.Length
                + " timestamps=" + timestamps.Length
                + " errors=" + errors.Length + ").",
                nameof(values));
        }

        var items = new List<DataChangeItem>(count);
        for (int i = 0; i < count; i++)
        {
            items.Add(new DataChangeItem(
                clientHandles[i],
                values[i],
                new OpcQuality(qualities[i]),
                DateTimeOffset.FromFileTime(timestamps[i]),
                errors[i]));
        }

        var notification = new DataChangeNotification(
            transactionId,
            groupHandle,
            masterQuality,
            masterError,
            items,
            _clock());

        if (_disposed)
        {
            return;
        }

        // DropOldest semantics: TryWrite returns true even when displacing the
        // oldest queued item. Approximate the drop counter by sampling depth
        // immediately before the write. The check is racy under concurrent
        // drain but acceptable for a best-effort diagnostic — the counter
        // never under-reports for the steady-state "queue is full" case.
        bool wasAtCapacity = _queue.Reader.CanCount && _queue.Reader.Count >= _capacity;
        if (!_queue.Writer.TryWrite(notification))
        {
            Interlocked.Increment(ref _droppedNotifications);
            return;
        }

        if (wasAtCapacity)
        {
            Interlocked.Increment(ref _droppedNotifications);
        }
    }
}
