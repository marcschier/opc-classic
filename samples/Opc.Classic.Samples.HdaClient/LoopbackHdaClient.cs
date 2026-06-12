// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Runtime.CompilerServices;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Testing;

namespace Opc.Classic.Samples.HdaClient;

internal sealed class LoopbackHdaClient : IAsyncDisposable
{
    private readonly LoopbackHdaCallRouter? _router;
    private readonly ICallChannel _channel;
    private readonly IOPCHDA_ServerClientProxy _server;
    private readonly IOPCHDA_SyncReadClientProxy _syncRead;
    private readonly IOPCHDA_SyncAnnotationsClientProxy _syncAnnotations;
    private readonly IOPCHDA_AsyncReadClientProxy _asyncRead;
    private bool _connected;

    public LoopbackHdaClient(ICallChannel channel)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _server = new IOPCHDA_ServerClientProxy(_channel);
        _syncRead = new IOPCHDA_SyncReadClientProxy(_channel);
        _syncAnnotations = new IOPCHDA_SyncAnnotationsClientProxy(_channel);
        _asyncRead = new IOPCHDA_AsyncReadClientProxy(_channel);
    }

    public LoopbackHdaClient(LoopbackHdaCallRouter router, InMemoryCallChannel channel)
        : this(channel)
    {
        _router = router ?? throw new ArgumentNullException(nameof(router));
    }

    public ValueTask ConnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _connected = true;
        return ValueTask.CompletedTask;
    }

    public ValueTask DisconnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _connected = false;
        return ValueTask.CompletedTask;
    }

    public async Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        return await _server.GetStatusAsync(cancellationToken).ConfigureAwait(false);
    }

    public async IAsyncEnumerable<HdaBrowseElement> BrowseAsync(
        string itemIdPrefix,
        HdaBrowseType browseType,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        await Task.Yield();
        IEnumerable<HdaBrowseElement> elements = _router is not null
            ? _router.Browse(itemIdPrefix, browseType)
            : BrowseFallback(itemIdPrefix, browseType);
        foreach (HdaBrowseElement element in elements)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return element;
        }
    }

    public Task<int[]> GetItemHandlesAsync(
        IReadOnlyList<string> itemIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        EnsureConnected();

        string[] itemIdArray = itemIds.ToArray();
        int[] clientHandles = Enumerable.Range(1, itemIdArray.Length).ToArray();
        return _server.GetItemHandlesAsync(itemIdArray, clientHandles, cancellationToken);
    }

    public Task<int[]> ReleaseItemHandlesAsync(int[] serverHandles, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        EnsureConnected();
        return _server.ReleaseItemHandlesAsync(serverHandles, cancellationToken);
    }

    public Task<OpcHdaItem[]> ReadRawWithSyncReadAsync(
        HdaTime startTime,
        HdaTime endTime,
        int maxValuesPerItem,
        bool includeBounds,
        int[] serverHandles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        EnsureConnected();
        return _syncRead.ReadRawAsync(
            ToOpcHdaTime(startTime),
            ToOpcHdaTime(endTime),
            maxValuesPerItem,
            includeBounds,
            serverHandles,
            cancellationToken);
    }

    public Task<OpcHdaItem[]> ReadProcessedWithSyncReadAsync(
        HdaTime startTime,
        HdaTime endTime,
        TimeSpan resampleInterval,
        HdaAggregate aggregate,
        int[] serverHandles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        EnsureConnected();
        int[] aggregateIds = Enumerable.Repeat((int)aggregate, serverHandles.Length).ToArray();
        return _syncRead.ReadProcessedAsync(
            ToOpcHdaTime(startTime),
            ToOpcHdaTime(endTime),
            resampleInterval.Ticks,
            serverHandles,
            aggregateIds,
            cancellationToken);
    }

    public Task<int> QueryAnnotationCapabilitiesAsync(CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        return _syncAnnotations.QueryCapabilitiesAsync(cancellationToken);
    }

    public Task<IReadOnlyList<HdaAnnotationResult>> ReadAnnotationsAsync(
        IReadOnlyList<string> itemIds,
        HdaTime startTime,
        HdaTime endTime,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        EnsureConnected();
        cancellationToken.ThrowIfCancellationRequested();
        _ = startTime;
        _ = endTime;
        return Task.FromResult(_router is not null
            ? _router.ReadAnnotations(itemIds)
            : itemIds.Select(static itemId => new HdaAnnotationResult { ItemId = itemId }).ToArray());
    }

    public Task<int> BeginAsyncReadRawAsync(
        int transactionId,
        HdaTime startTime,
        HdaTime endTime,
        int maxValuesPerItem,
        bool includeBounds,
        int[] serverHandles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        EnsureConnected();
        return _asyncRead.ReadRawAsync(
            transactionId,
            ToOpcHdaTime(startTime),
            ToOpcHdaTime(endTime),
            maxValuesPerItem,
            includeBounds,
            serverHandles,
            cancellationToken);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private void EnsureConnected()
    {
        if (!_connected)
        {
            throw new InvalidOperationException("Connect before using the HDA loopback client.");
        }
    }

    private static IEnumerable<HdaBrowseElement> BrowseFallback(string itemIdPrefix, HdaBrowseType browseType)
    {
        if (string.IsNullOrWhiteSpace(itemIdPrefix))
        {
            if (browseType is HdaBrowseType.Branch or HdaBrowseType.Flat)
            {
                yield return new HdaBrowseElement { Name = "Sensor", ItemId = "Sensor", BrowseType = HdaBrowseType.Branch };
            }

            if (browseType is HdaBrowseType.Leaf or HdaBrowseType.Flat)
            {
                yield return CreateLeaf("Sensor.Temperature");
            }

            yield break;
        }

        if (itemIdPrefix.Equals("Sensor", StringComparison.OrdinalIgnoreCase) && browseType != HdaBrowseType.Branch)
        {
            yield return CreateLeaf("Sensor.Temperature");
        }
    }

    private static HdaBrowseElement CreateLeaf(string itemId) => new()
    {
        Name = itemId[(itemId.LastIndexOf('.') + 1)..],
        ItemId = itemId,
        BrowseType = HdaBrowseType.Leaf,
    };

    private static OpcHdaTime ToOpcHdaTime(HdaTime time) => time.IsRelative
        ? OpcHdaTime.FromString(time.Expression ?? "NOW")
        : OpcHdaTime.FromTimestamp(time.ResolveAt(DateTimeOffset.UtcNow));

}
