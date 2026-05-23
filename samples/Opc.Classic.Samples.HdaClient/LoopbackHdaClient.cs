// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Runtime.CompilerServices;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Samples.HdaClient;

internal sealed class LoopbackHdaClient : IAsyncDisposable
{
    private const int GetHistorianStatusOpnum = 5;
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    private readonly LoopbackHdaCallRouter _router;
    private readonly InMemoryCallChannel _channel;
    private readonly IOPCHDA_ServerClientProxy _server;
    private readonly IOPCHDA_SyncReadClientProxy _syncRead;
    private readonly IOPCHDA_SyncAnnotationsClientProxy _syncAnnotations;
    private readonly IOPCHDA_AsyncReadClientProxy _asyncRead;
    private bool _connected;

    public LoopbackHdaClient(LoopbackHdaCallRouter router)
    {
        _router = router ?? throw new ArgumentNullException(nameof(router));
        _channel = new InMemoryCallChannel(router.DispatchAsync);
        _server = new IOPCHDA_ServerClientProxy(_channel);
        _syncRead = new IOPCHDA_SyncReadClientProxy(_channel);
        _syncAnnotations = new IOPCHDA_SyncAnnotationsClientProxy(_channel);
        _asyncRead = new IOPCHDA_AsyncReadClientProxy(_channel);
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
        NdrCallResult result = await _channel.InvokeAsync(
            IOPCHDA_Server.InterfaceId,
            GetHistorianStatusOpnum,
            ReadOnlyMemory<byte>.Empty,
            cancellationToken).ConfigureAwait(false);
        ThrowIfFailure(result);
        return DecodeHistorianStatus(result.ResponsePayload);
    }

    public async IAsyncEnumerable<HdaBrowseElement> BrowseAsync(
        string itemIdPrefix,
        HdaBrowseType browseType,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        EnsureConnected();
        await Task.Yield();
        foreach (HdaBrowseElement element in _router.Browse(itemIdPrefix, browseType))
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
        return Task.FromResult(_router.ReadAnnotations(itemIds));
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

    private static OpcHdaTime ToOpcHdaTime(HdaTime time) => time.IsRelative
        ? OpcHdaTime.FromString(time.Expression ?? "NOW")
        : OpcHdaTime.FromTimestamp(time.ResolveAt(DateTimeOffset.UtcNow));

    private static OpcServerStatus DecodeHistorianStatus(ReadOnlyMemory<byte> payload)
    {
        var reader = new NdrReader(payload.Span);
        uint historianStatus = reader.ReadUInt32();
        DateTimeOffset currentTime = FromFileTime(reader.ReadFileTime());
        DateTimeOffset startTime = FromFileTime(reader.ReadFileTime());
        ushort major = reader.ReadUInt16();
        ushort minor = reader.ReadUInt16();
        ushort build = reader.ReadUInt16();
        _ = reader.ReadUInt16();
        uint maxReturnValues = reader.ReadUInt32();
        _ = reader.ReadUnicodeStringPtr();
        string vendorInfo = reader.ReadUnicodeStringPtr() ?? string.Empty;

        return new OpcServerStatus
        {
            Spec = OpcStatusSpec.Hda,
            StartTime = startTime,
            CurrentTime = currentTime,
            LastUpdateTime = currentTime,
            State = FromHistorianStatus(historianStatus),
            ServerVersion = new Version(major, minor, build),
            MaxReturnValues = checked((int)maxReturnValues),
            VendorInfo = vendorInfo,
        };
    }

    private static OpcServerState FromHistorianStatus(uint historianStatus) => historianStatus switch
    {
        1u => OpcServerState.Running,
        2u => OpcServerState.Failed,
        3u => OpcServerState.NoConfig,
        _ => OpcServerState.Unknown,
    };

    private static DateTimeOffset FromFileTime(long fileTime) =>
        new(fileTime + FileTimeEpochOffsetTicks, TimeSpan.Zero);

    private static void ThrowIfFailure(NdrCallResult result)
    {
        if (result.IsFailure)
        {
            throw new OpcException(new OpcResultId(result.Hresult, null));
        }
    }
}
