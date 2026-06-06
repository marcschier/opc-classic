// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Collections.Concurrent;
using Opc.Classic;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Dcom;

namespace Opc.Classic.Samples.HdaServer;

/// <summary>
/// Implements the low-level OPC HDA 1.20 DCOM interfaces by translating
/// server handles to item IDs via an internal handle registry and delegating
/// to the underlying <see cref="HistoricalDataStore"/>.
/// </summary>
public sealed partial class SampleHdaServer : IOPCHDA_SyncRead, IOPCHDA_SyncUpdate, IOPCHDA_SyncAnnotations
{
    private static readonly uint GoodQualityValue = OpcQuality.Good.RawValue;
    private readonly ConcurrentDictionary<int, string> _handleRegistry = new();
    private readonly ConcurrentDictionary<int, int> _handleClientHandle = new();
    private int _nextHandle;

    // ===== IOPCHDA_Server tearoff methods =====

    Task<int[]> IOPCHDA_Server.GetItemHandlesAsync(string[] itemIds, int[] clientHandles, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        ArgumentNullException.ThrowIfNull(clientHandles);
        cancellationToken.ThrowIfCancellationRequested();

        var handles = new int[itemIds.Length];
        for (int i = 0; i < itemIds.Length; i++)
        {
            if (!_store.Contains(itemIds[i]))
            {
                handles[i] = 0;
                continue;
            }

            int handle = Interlocked.Increment(ref _nextHandle);
            _handleRegistry[handle] = itemIds[i];
            _handleClientHandle[handle] = i < clientHandles.Length ? clientHandles[i] : 0;
            handles[i] = handle;
        }
        return Task.FromResult(handles);
    }

    Task<int[]> IOPCHDA_Server.ReleaseItemHandlesAsync(int[] serverHandles, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();

        var results = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            bool removed = _handleRegistry.TryRemove(serverHandles[i], out _);
            _handleClientHandle.TryRemove(serverHandles[i], out _);
            results[i] = removed ? OpcResultId.Ok.Code : OpcResultId.UnknownItemId.Code;
        }
        return Task.FromResult(results);
    }

    // ===== IOPCHDA_SyncRead =====

    Task<OpcHdaItem[]> IOPCHDA_SyncRead.ReadRawAsync(
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        int maxValues,
        bool bounds,
        int[] serverHandles,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();

        _ = bounds;
        (DateTimeOffset start, DateTimeOffset end) = NormalizeRange(ResolveTime(startTime), ResolveTime(endTime));
        var items = new OpcHdaItem[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            int clientHandle = _handleClientHandle.TryGetValue(serverHandles[i], out int ch) ? ch : 0;
            if (!_handleRegistry.TryGetValue(serverHandles[i], out string? itemId))
            {
                items[i] = CreateItem(clientHandle, 0, Array.Empty<(DateTimeOffset, double)>());
                continue;
            }

            var samples = _store.ReadRaw(itemId, start, end, maxValues).ToArray();
            items[i] = CreateItem(clientHandle, 0, samples);
        }
        return Task.FromResult(items);
    }

    Task<OpcHdaItem[]> IOPCHDA_SyncRead.ReadProcessedAsync(
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        long resampleIntervalFileTime,
        int[] serverHandles,
        int[] aggregateIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();

        TimeSpan resampleInterval = TimeSpan.FromTicks(resampleIntervalFileTime > 0 ? resampleIntervalFileTime : TimeSpan.TicksPerMinute);
        (DateTimeOffset start, DateTimeOffset end) = NormalizeRange(ResolveTime(startTime), ResolveTime(endTime));
        var items = new OpcHdaItem[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            int clientHandle = _handleClientHandle.TryGetValue(serverHandles[i], out int ch) ? ch : 0;
            HdaAggregate aggregate = i < aggregateIds.Length ? (HdaAggregate)aggregateIds[i] : HdaAggregate.Average;
            if (!_handleRegistry.TryGetValue(serverHandles[i], out string? itemId))
            {
                items[i] = CreateItem(clientHandle, (int)aggregate, Array.Empty<(DateTimeOffset, double)>());
                continue;
            }

            var processed = ProcessItem(itemId, start, end, resampleInterval, aggregate, cancellationToken);
            items[i] = CreateItem(clientHandle, (int)aggregate, processed);
        }
        return Task.FromResult(items);
    }

    Task<OpcHdaItem[]> IOPCHDA_SyncRead.ReadAtTimeAsync(
        long[] timestampFileTimes,
        int[] serverHandles,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(timestampFileTimes);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();

        var items = new OpcHdaItem[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            int clientHandle = _handleClientHandle.TryGetValue(serverHandles[i], out int ch) ? ch : 0;
            if (!_handleRegistry.TryGetValue(serverHandles[i], out string? itemId))
            {
                items[i] = CreateItem(clientHandle, 0, Array.Empty<(DateTimeOffset, double)>());
                continue;
            }

            var samples = new (DateTimeOffset Time, double Value)[timestampFileTimes.Length];
            for (int t = 0; t < timestampFileTimes.Length; t++)
            {
                DateTimeOffset stamp = DateTimeOffset.FromFileTime(timestampFileTimes[t]);
                samples[t] = (stamp, 0d);
            }
            items[i] = CreateItem(clientHandle, 0, samples);
        }
        return Task.FromResult(items);
    }

    Task<OpcHdaModifiedItem[]> IOPCHDA_SyncRead.ReadModifiedAsync(
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        int maxValues,
        int[] serverHandles,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();

        _ = startTime; _ = endTime; _ = maxValues;
        var items = new OpcHdaModifiedItem[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            int clientHandle = _handleClientHandle.TryGetValue(serverHandles[i], out int ch) ? ch : 0;
            items[i] = new OpcHdaModifiedItem(clientHandle, [], [], [], [], [], []);
        }
        return Task.FromResult(items);
    }

    Task<OpcHdaAttribute[]> IOPCHDA_SyncRead.ReadAttributeAsync(
        OpcHdaTime startTime,
        OpcHdaTime endTime,
        int serverHandle,
        int[] attributeIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(attributeIds);
        cancellationToken.ThrowIfCancellationRequested();

        _ = startTime; _ = endTime; _ = serverHandle;
        var attributes = new OpcHdaAttribute[attributeIds.Length];
        var now = DateTimeOffset.UtcNow;
        for (int i = 0; i < attributeIds.Length; i++)
        {
            attributes[i] = new OpcHdaAttribute(
                serverHandle,
                attributeIds[i],
                new[] { now },
                new[] { OpcVariant.FromString($"attribute-{attributeIds[i]}") });
        }
        return Task.FromResult(attributes);
    }

    // ===== IOPCHDA_SyncUpdate =====

    Task<int> IOPCHDA_SyncUpdate.QueryCapabilitiesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // OPCHDA_INSERTCAP | OPCHDA_REPLACECAP | OPCHDA_INSERTREPLACECAP | OPCHDA_DELETERAWCAP | OPCHDA_DELETEATTIMECAP
        return Task.FromResult(0x1F);
    }

    Task<int[]> IOPCHDA_SyncUpdate.InsertAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(SuccessArray(serverHandles.Length));
    }

    Task<int[]> IOPCHDA_SyncUpdate.ReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(SuccessArray(serverHandles.Length));
    }

    Task<int[]> IOPCHDA_SyncUpdate.InsertReplaceAsync(int[] serverHandles, long[] timestampFileTimes, OpcVariant[] dataValues, int[] qualities, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(SuccessArray(serverHandles.Length));
    }

    Task<int[]> IOPCHDA_SyncUpdate.DeleteRawAsync(OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(SuccessArray(serverHandles.Length));
    }

    Task<int[]> IOPCHDA_SyncUpdate.DeleteAtTimeAsync(int[] serverHandles, long[] timestampFileTimes, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(SuccessArray(serverHandles.Length));
    }

    // ===== IOPCHDA_SyncAnnotations =====

    Task<int> IOPCHDA_SyncAnnotations.QueryCapabilitiesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // OPCHDA_READANNOTATIONCAP | OPCHDA_INSERTANNOTATIONCAP
        return Task.FromResult(0x03);
    }

    Task<OpcHdaAnnotation[]> IOPCHDA_SyncAnnotations.ReadAsync(OpcHdaTime startTime, OpcHdaTime endTime, int[] serverHandles, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(startTime);
        ArgumentNullException.ThrowIfNull(endTime);
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();

        _ = startTime; _ = endTime;
        var annotations = new OpcHdaAnnotation[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            int clientHandle = _handleClientHandle.TryGetValue(serverHandles[i], out int ch) ? ch : 0;
            annotations[i] = new OpcHdaAnnotation(clientHandle, Array.Empty<DateTimeOffset>(), Array.Empty<string?>(), Array.Empty<DateTimeOffset>(), Array.Empty<string?>());
        }
        return Task.FromResult(annotations);
    }

    Task<int[]> IOPCHDA_SyncAnnotations.InsertAsync(int[] serverHandles, long[] timestampFileTimes, OpcHdaAnnotation[] annotationValues, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(SuccessArray(serverHandles.Length));
    }

    private static int[] SuccessArray(int length)
    {
        var arr = new int[length];
        for (int i = 0; i < length; i++)
        {
            arr[i] = OpcResultId.Ok.Code;
        }
        return arr;
    }
}
