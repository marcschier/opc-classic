// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;

namespace Opc.Classic.Samples.LoopbackDemo;

internal sealed class LoopbackDaClient
{
    private const int AddGroupOpnum = 4;
    private const int BrowseItemsOpnum = 5;
    private const int AddItemsOpnum = 3;
    private const int ReadOpnum = 3;
    private const int DataSourceCache = 1;

    private static readonly Action<ILogger, Guid, Exception?> ConnectionPointInterface = LoggerMessage.Define<Guid>(
        LogLevel.Information,
        new EventId(1, nameof(SubscribeAsync)),
        "Loopback connection point exposes callback IID {InterfaceId}");

    private readonly InMemoryCallChannel _channel;
    private readonly LoopbackDaRuntime _runtime;
    private readonly IOPCServerClientProxy _server;
    private readonly IOPCBrowseServerAddressSpaceClientProxy _browse;
    private readonly IOPCSyncIOClientProxy _syncIo;
    private readonly IOPCAsyncIO2ClientProxy _asyncIo;
    private readonly IConnectionPointClientProxy _connectionPoint;
    private readonly ILogger<LoopbackDaClient> _logger;

    public LoopbackDaClient(
        InMemoryCallChannel channel,
        LoopbackDaRuntime runtime,
        ILogger<LoopbackDaClient> logger)
    {
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _server = new IOPCServerClientProxy(channel);
        _browse = new IOPCBrowseServerAddressSpaceClientProxy(channel);
        _syncIo = new IOPCSyncIOClientProxy(channel);
        _asyncIo = new IOPCAsyncIO2ClientProxy(channel);
        _connectionPoint = new IConnectionPointClientProxy(channel);
    }

    public Task<OpcServerStatus> ConnectAsync(CancellationToken cancellationToken) =>
        _server.GetStatusAsync(cancellationToken);

    public async Task<string[]> BrowseAsync(CancellationToken cancellationToken)
    {
        _ = await _browse.QueryOrganizationAsync(cancellationToken).ConfigureAwait(false);
        ReadOnlyMemory<byte> response = await InvokeAsync(
            IOPCBrowseServerAddressSpace.InterfaceId,
            BrowseItemsOpnum,
            ReadOnlyMemory<byte>.Empty,
            cancellationToken).ConfigureAwait(false);
        var reader = new NdrReader(response.Span);
        return LoopbackNdr.ReadStringArray(ref reader);
    }

    public async Task<int> AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientHandle,
        int localeId,
        CancellationToken cancellationToken)
    {
        ReadOnlyMemory<byte> payload = LoopbackNdr.Write((ref NdrWriter writer) =>
        {
            writer.WriteUnicodeStringPtr(name);
            writer.WriteInt32(active ? -1 : 0);
            writer.WriteInt32(requestedUpdateRate);
            writer.WriteInt32(clientHandle);
            writer.WriteInt32(localeId);
        });
        ReadOnlyMemory<byte> response = await InvokeAsync(
            IOPCServer.InterfaceId,
            AddGroupOpnum,
            payload,
            cancellationToken).ConfigureAwait(false);
        var reader = new NdrReader(response.Span);
        return reader.ReadInt32();
    }

    public async Task<IReadOnlyList<LoopbackAddItemResult>> AddItemsAsync(
        int groupHandle,
        IReadOnlyList<LoopbackItemRequest> items,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(items);
        ReadOnlyMemory<byte> payload = LoopbackNdr.Write((ref NdrWriter writer) =>
        {
            writer.WriteInt32(groupHandle);
            writer.WriteUInt32((uint)items.Count);
            foreach (LoopbackItemRequest item in items)
            {
                var def = new OpcItemDef(
                    AccessPath: null,
                    ItemId: item.ItemId,
                    Active: true,
                    ClientHandle: item.ClientHandle,
                    Blob: Array.Empty<byte>(),
                    RequestedDataType: VarType.VT_EMPTY);
                NdrOpcItemDefCodec.Write(ref writer, def);
            }
        });

        ReadOnlyMemory<byte> response = await InvokeAsync(
            IOPCItemMgt.InterfaceId,
            AddItemsOpnum,
            payload,
            cancellationToken).ConfigureAwait(false);
        var reader = new NdrReader(response.Span);
        int count = checked((int)reader.ReadUInt32());
        var itemResults = new OpcItemResult[count];
        for (var index = 0; index < itemResults.Length; index++)
        {
            itemResults[index] = NdrOpcItemResultCodec.Read(ref reader);
        }

        int[] errors = LoopbackNdr.ReadInt32Array(ref reader);
        var results = new List<LoopbackAddItemResult>(items.Count);
        for (var index = 0; index < items.Count; index++)
        {
            OpcItemResult itemResult = index < itemResults.Length
                ? itemResults[index]
                : new OpcItemResult(0, VarType.VT_EMPTY, 0, Array.Empty<byte>());
            int error = index < errors.Length ? errors[index] : OpcResultId.Fail.Code;
            results.Add(new LoopbackAddItemResult(
                items[index].ItemId,
                items[index].ClientHandle,
                itemResult.ServerHandle,
                itemResult.CanonicalDataType,
                itemResult.AccessRights,
                error));
        }

        return results;
    }

    public async Task<IReadOnlyList<LoopbackReadResult>> ReadAsync(
        IReadOnlyList<LoopbackAddItemResult> items,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(items);
        int[] serverHandles = items.Select(static item => item.ServerHandle).ToArray();
        ReadOnlyMemory<byte> payload = LoopbackNdr.Write((ref NdrWriter writer) =>
        {
            writer.WriteInt32(DataSourceCache);
            LoopbackNdr.WriteInt32Array(ref writer, serverHandles);
        });
        ReadOnlyMemory<byte> response = await InvokeAsync(
            IOPCSyncIO.InterfaceId,
            ReadOpnum,
            payload,
            cancellationToken).ConfigureAwait(false);

        var reader = new NdrReader(response.Span);
        int count = checked((int)reader.ReadUInt32());
        var states = new OpcItemState[count];
        for (var index = 0; index < states.Length; index++)
        {
            states[index] = NdrOpcItemStateCodec.Read(ref reader);
        }

        int[] errors = LoopbackNdr.ReadInt32Array(ref reader);
        var results = new List<LoopbackReadResult>(items.Count);
        for (var index = 0; index < items.Count; index++)
        {
            OpcItemState state = index < states.Length
                ? states[index]
                : new OpcItemState(0, DateTimeOffset.UtcNow, OpcQuality.Bad, OpcVariant.Empty);
            int error = index < errors.Length ? errors[index] : OpcResultId.Fail.Code;
            results.Add(new LoopbackReadResult(
                items[index].ItemId,
                state.ClientHandle,
                items[index].ServerHandle,
                state.Value,
                state.Quality,
                state.Timestamp,
                error));
        }

        return results;
    }

    public async Task<IReadOnlyList<LoopbackWriteResult>> WriteAsync(
        IReadOnlyList<LoopbackAddItemResult> items,
        IReadOnlyList<OpcVariant> values,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(values);
        if (items.Count != values.Count)
        {
            throw new ArgumentException("Item and value counts must match.", nameof(values));
        }

        int[] serverHandles = items.Select(static item => item.ServerHandle).ToArray();
        int[] errors = await _syncIo.WriteAsync(serverHandles, values.ToArray(), cancellationToken).ConfigureAwait(false);
        var results = new List<LoopbackWriteResult>(items.Count);
        for (var index = 0; index < items.Count; index++)
        {
            int error = index < errors.Length ? errors[index] : OpcResultId.Fail.Code;
            results.Add(new LoopbackWriteResult(items[index].ItemId, items[index].ServerHandle, values[index], error));
        }

        return results;
    }

    public Task RemoveGroupAsync(int groupHandle, CancellationToken cancellationToken) =>
        _server.RemoveGroupAsync(groupHandle, force: true, cancellationToken);

    public async Task<LoopbackSubscription> SubscribeAsync(CancellationToken cancellationToken)
    {
        Guid callbackInterface = await _connectionPoint.GetConnectionInterfaceAsync(cancellationToken).ConfigureAwait(false);
        ConnectionPointInterface(_logger, callbackInterface, null);

        var sink = new LoopbackCallbackSink();
        var callbackProxy = new IOPCDataCallbackClientProxy(sink.Channel);
        int cookie = _runtime.Advise(callbackProxy);
        await _asyncIo.SetEnableAsync(enabled: true, cancellationToken).ConfigureAwait(false);
        return new LoopbackSubscription(_runtime, _asyncIo, sink, cookie);
    }

    public IReadOnlyList<InMemoryCall> CallLog => _channel.CallLog;

    private async Task<ReadOnlyMemory<byte>> InvokeAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        NdrCallResult result = await _channel.InvokeAsync(interfaceId, opnum, payload, cancellationToken).ConfigureAwait(false);
        if (result.IsFailure)
        {
            throw new OpcException(new OpcResultId(result.Hresult, null));
        }

        return result.ResponsePayload;
    }
}

internal sealed class LoopbackSubscription : IAsyncDisposable
{
    private readonly LoopbackDaRuntime _runtime;
    private readonly IOPCAsyncIO2ClientProxy _asyncIo;
    private readonly LoopbackCallbackSink _sink;
    private readonly int _cookie;
    private bool _disposed;

    public LoopbackSubscription(
        LoopbackDaRuntime runtime,
        IOPCAsyncIO2ClientProxy asyncIo,
        LoopbackCallbackSink sink,
        int cookie)
    {
        _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        _asyncIo = asyncIo ?? throw new ArgumentNullException(nameof(asyncIo));
        _sink = sink ?? throw new ArgumentNullException(nameof(sink));
        _cookie = cookie;
    }

    public IAsyncEnumerable<LoopbackNotification> Notifications(CancellationToken cancellationToken) =>
        _sink.Notifications(cancellationToken);

    public Task<int> RefreshAsync(int transactionId, CancellationToken cancellationToken) =>
        _asyncIo.Refresh2Async(1, transactionId, cancellationToken);

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _runtime.Unadvise(_cookie);
        await _asyncIo.SetEnableAsync(enabled: false, CancellationToken.None).ConfigureAwait(false);
        _sink.Complete();
    }
}

internal sealed class LoopbackCallbackSink
{
    private readonly Channel<LoopbackNotification> _notifications = System.Threading.Channels.Channel.CreateUnbounded<LoopbackNotification>();

    public LoopbackCallbackSink()
    {
        Channel = new InMemoryCallChannel(DispatchAsync);
    }

    public InMemoryCallChannel Channel { get; }

    public async IAsyncEnumerable<LoopbackNotification> Notifications(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (await _notifications.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            while (_notifications.Reader.TryRead(out LoopbackNotification? notification))
            {
                yield return notification;
            }
        }
    }

    public void Complete() => _notifications.Writer.TryComplete();

    private Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> payload,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (interfaceId != IOPCDataCallback.InterfaceId || opnum != IOPCDataCallback.Opnums.OnDataChangeAsync)
        {
            return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
        }

        var reader = new NdrReader(payload.Span);
        int transactionId = reader.ReadInt32();
        int groupHandle = reader.ReadInt32();
        int masterQuality = reader.ReadInt32();
        int masterError = reader.ReadInt32();
        int[] clientHandles = LoopbackNdr.ReadInt32Array(ref reader);
        OpcVariant[] values = LoopbackNdr.ReadVariantArray(ref reader);
        ushort[] qualities = LoopbackNdr.ReadUInt16Array(ref reader);
        long[] timestamps = LoopbackNdr.ReadInt64Array(ref reader);
        int[] errors = LoopbackNdr.ReadInt32Array(ref reader);
        int itemCount = new[] { clientHandles.Length, values.Length, qualities.Length, timestamps.Length, errors.Length }.Min();
        var items = new List<LoopbackNotificationItem>(itemCount);
        for (var index = 0; index < itemCount; index++)
        {
            items.Add(new LoopbackNotificationItem(
                clientHandles[index],
                values[index],
                new OpcQuality(qualities[index]),
                LoopbackNdr.FromFileTime(timestamps[index]),
                errors[index]));
        }

        _notifications.Writer.TryWrite(new LoopbackNotification(
            transactionId,
            groupHandle,
            masterQuality,
            masterError,
            items));
        return Task.FromResult(new NdrCallResult(OpcResultId.Ok.Code, ReadOnlyMemory<byte>.Empty));
    }
}
