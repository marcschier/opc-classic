//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Da;
using Opc.Classic.Mcp.Tools;
using Opc.Classic.Ndr;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Mcp.Tests;

public sealed class DaDataCallbackSinkTests
{
    [Test]
    public async Task OnDataChange_enqueues_batch_with_per_item_payload()
    {
        using var sink = new DaDataCallbackSink();

        await sink.OnDataChangeAsync(
            transactionId: 11,
            groupHandle: 7,
            masterQuality: 0,
            masterError: 0,
            clientHandles: [101, 102],
            values: [OpcVariant.FromInt32(42), OpcVariant.FromInt32(99)],
            qualities: [0b1100_0000, 0b1100_0000],
            timestamps: [DateTimeOffset.UtcNow.ToFileTime(), DateTimeOffset.UtcNow.ToFileTime()],
            errors: [0, 0],
            cancellationToken: TestContext.Current!.CancellationToken).ConfigureAwait(false);

        await Assert.That(sink.OnDataChangeCount).IsEqualTo(1L);
        await Assert.That(sink.HasReceivedAnyData).IsTrue();

        IReadOnlyList<DataChangeItem> drained = sink.DrainItems(maxItems: 0);
        await Assert.That(drained.Count).IsEqualTo(2);
        await Assert.That(drained[0].ClientHandle).IsEqualTo(101);
        await Assert.That(drained[1].ClientHandle).IsEqualTo(102);
        await Assert.That(drained[0].Value.Boxed).IsEqualTo(42);
        await Assert.That(drained[1].Value.Boxed).IsEqualTo(99);
    }

    [Test]
    public async Task OnReadComplete_enqueues_and_increments_its_own_counter()
    {
        using var sink = new DaDataCallbackSink();

        await sink.OnReadCompleteAsync(
            transactionId: 22,
            groupHandle: 7,
            masterQuality: 0,
            masterError: 0,
            clientHandles: [1],
            values: [OpcVariant.FromInt32(1)],
            qualities: [0b1100_0000],
            timestamps: [DateTimeOffset.UtcNow.ToFileTime()],
            errors: [0],
            cancellationToken: TestContext.Current!.CancellationToken).ConfigureAwait(false);

        await Assert.That(sink.OnReadCompleteCount).IsEqualTo(1L);
        await Assert.That(sink.OnDataChangeCount).IsEqualTo(0L);
        await Assert.That(sink.DrainItems(0).Count).IsEqualTo(1);
    }

    [Test]
    public async Task OnWriteComplete_and_OnCancelComplete_increment_counters_but_do_not_enqueue()
    {
        using var sink = new DaDataCallbackSink();

        await sink.OnWriteCompleteAsync(
            transactionId: 1,
            groupHandle: 7,
            masterError: 0,
            clientHandles: [10],
            errors: [0],
            cancellationToken: TestContext.Current!.CancellationToken).ConfigureAwait(false);
        await sink.OnCancelCompleteAsync(
            transactionId: 2,
            groupHandle: 7,
            cancellationToken: TestContext.Current!.CancellationToken).ConfigureAwait(false);

        await Assert.That(sink.OnWriteCompleteCount).IsEqualTo(1L);
        await Assert.That(sink.OnCancelCompleteCount).IsEqualTo(1L);
        await Assert.That(sink.HasReceivedAnyData).IsFalse();
        await Assert.That(sink.DrainItems(0).Count).IsEqualTo(0);
    }

    [Test]
    public async Task Mismatched_array_lengths_throw_argument_exception()
    {
        using var sink = new DaDataCallbackSink();

        await Assert.ThrowsAsync<ArgumentException>(async () => await sink.OnDataChangeAsync(
            transactionId: 1,
            groupHandle: 7,
            masterQuality: 0,
            masterError: 0,
            clientHandles: [101, 102],
            values: [OpcVariant.FromInt32(1)],
            qualities: [0b1100_0000, 0b1100_0000],
            timestamps: [0L, 0L],
            errors: [0, 0],
            cancellationToken: TestContext.Current!.CancellationToken).ConfigureAwait(false)).ConfigureAwait(false);

        // Counter must remain 0 — a failed (validation-throwing) call must NOT be
        // counted as a successful OnDataChange invocation. Otherwise monitoring
        // can't distinguish "received N valid batches" from "received N attempts,
        // some of which were rejected as malformed".
        await Assert.That(sink.OnDataChangeCount).IsEqualTo(0L);
        await Assert.That(sink.DrainItems(0).Count).IsEqualTo(0);
    }

    [Test]
    public async Task DrainItems_with_max_caps_returned_items_and_requeues_remainder()
    {
        using var sink = new DaDataCallbackSink();

        long ts = DateTimeOffset.UtcNow.ToFileTime();
        await sink.OnDataChangeAsync(
            transactionId: 1, groupHandle: 7, masterQuality: 0, masterError: 0,
            clientHandles: [1, 2, 3, 4, 5],
            values: [OpcVariant.FromInt32(1), OpcVariant.FromInt32(2), OpcVariant.FromInt32(3), OpcVariant.FromInt32(4), OpcVariant.FromInt32(5)],
            qualities: [0b1100_0000, 0b1100_0000, 0b1100_0000, 0b1100_0000, 0b1100_0000],
            timestamps: [ts, ts, ts, ts, ts],
            errors: [0, 0, 0, 0, 0],
            cancellationToken: TestContext.Current!.CancellationToken).ConfigureAwait(false);

        IReadOnlyList<DataChangeItem> firstDrain = sink.DrainItems(maxItems: 3);
        await Assert.That(firstDrain.Count).IsEqualTo(3);
        await Assert.That(firstDrain.Select(i => i.ClientHandle).ToArray()).IsEquivalentTo(new[] { 1, 2, 3 });

        IReadOnlyList<DataChangeItem> secondDrain = sink.DrainItems(maxItems: 0);
        await Assert.That(secondDrain.Count).IsEqualTo(2);
        await Assert.That(secondDrain.Select(i => i.ClientHandle).ToArray()).IsEquivalentTo(new[] { 4, 5 });

        IReadOnlyList<DataChangeItem> thirdDrain = sink.DrainItems(maxItems: 0);
        await Assert.That(thirdDrain.Count).IsEqualTo(0);
    }

    [Test]
    public async Task DrainItems_returns_items_across_multiple_batches_in_fifo_order()
    {
        using var sink = new DaDataCallbackSink();
        long ts = DateTimeOffset.UtcNow.ToFileTime();
        for (int batch = 0; batch < 3; batch++)
        {
            await sink.OnDataChangeAsync(
                transactionId: batch, groupHandle: 7, masterQuality: 0, masterError: 0,
                clientHandles: [batch * 10 + 1, batch * 10 + 2],
                values: [OpcVariant.FromInt32(0), OpcVariant.FromInt32(0)],
                qualities: [0b1100_0000, 0b1100_0000],
                timestamps: [ts, ts],
                errors: [0, 0],
                cancellationToken: TestContext.Current!.CancellationToken).ConfigureAwait(false);
        }

        IReadOnlyList<DataChangeItem> drained = sink.DrainItems(maxItems: 4);
        await Assert.That(drained.Count).IsEqualTo(4);
        await Assert.That(drained.Select(i => i.ClientHandle).ToArray())
            .IsEquivalentTo(new[] { 1, 2, 11, 12 });

        IReadOnlyList<DataChangeItem> remainder = sink.DrainItems(maxItems: 0);
        await Assert.That(remainder.Select(i => i.ClientHandle).ToArray())
            .IsEquivalentTo(new[] { 21, 22 });
    }

    [Test]
    public async Task Bounded_queue_drops_oldest_when_capacity_exceeded()
    {
        using var sink = new DaDataCallbackSink(capacity: 2, clock: null);
        long ts = DateTimeOffset.UtcNow.ToFileTime();

        async Task PushAsync(int txn, int clientHandle) => await sink.OnDataChangeAsync(
            transactionId: txn, groupHandle: 7, masterQuality: 0, masterError: 0,
            clientHandles: [clientHandle],
            values: [OpcVariant.FromInt32(clientHandle)],
            qualities: [0b1100_0000],
            timestamps: [ts],
            errors: [0],
            cancellationToken: TestContext.Current!.CancellationToken).ConfigureAwait(false);

        await PushAsync(1, 100).ConfigureAwait(false);
        await PushAsync(2, 200).ConfigureAwait(false);
        await PushAsync(3, 300).ConfigureAwait(false);
        await PushAsync(4, 400).ConfigureAwait(false);

        // Capacity 2, 4 pushes -> exactly 2 dropped (deterministic with DropOldest).
        await Assert.That(sink.DroppedNotifications).IsEqualTo(2L);

        IReadOnlyList<DataChangeItem> drained = sink.DrainItems(maxItems: 0);
        await Assert.That(drained.Count).IsEqualTo(2);
        // The two surviving notifications should be the most recent two (300, 400).
        int[] surviving = drained.Select(i => i.ClientHandle).ToArray();
        await Assert.That(surviving).IsEquivalentTo(new[] { 300, 400 });
    }

    [Test]
    public async Task Drain_after_dispose_throws()
    {
        var sink = new DaDataCallbackSink();
        sink.Dispose();
        await Assert.That(() => { _ = sink.DrainItems(0); }).Throws<ObjectDisposedException>();
    }

    [Test]
    public async Task Enqueue_after_dispose_silently_no_ops()
    {
        var sink = new DaDataCallbackSink();
        sink.Dispose();

        await sink.OnDataChangeAsync(
            transactionId: 1, groupHandle: 7, masterQuality: 0, masterError: 0,
            clientHandles: [1],
            values: [OpcVariant.FromInt32(1)],
            qualities: [0b1100_0000],
            timestamps: [DateTimeOffset.UtcNow.ToFileTime()],
            errors: [0],
            cancellationToken: TestContext.Current!.CancellationToken).ConfigureAwait(false);

        // Counter still increments (we observed the call), but the queue is closed.
        await Assert.That(sink.OnDataChangeCount).IsEqualTo(1L);
    }

    [Test]
    public async Task Timestamp_is_decoded_from_file_time()
    {
        using var sink = new DaDataCallbackSink();
        var expected = new DateTimeOffset(2025, 11, 19, 14, 30, 0, TimeSpan.Zero);
        long fileTime = expected.ToFileTime();

        await sink.OnDataChangeAsync(
            transactionId: 1, groupHandle: 7, masterQuality: 0, masterError: 0,
            clientHandles: [1],
            values: [OpcVariant.FromInt32(1)],
            qualities: [0b1100_0000],
            timestamps: [fileTime],
            errors: [0],
            cancellationToken: TestContext.Current!.CancellationToken).ConfigureAwait(false);

        DataChangeItem item = sink.DrainItems(0)[0];
        await Assert.That(item.Timestamp).IsEqualTo(expected);
    }
}
