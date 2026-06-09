// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.Logging;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;

namespace Opc.Classic.Samples.LoopbackDemo;

internal sealed class LoopbackDaRuntime {
    private const int AddGroupOpnum = 3;
    private const int BrowseItemsOpnum = 5;
    private const int AddItemsOpnum = 3;
    private const int ReadOpnum = 3;
    private const int DataSourceCache = 1;

    private static readonly Action<ILogger, Guid, int, Exception?> DispatchingCall = LoggerMessage.Define<Guid, int>(
        LogLevel.Debug,
        new EventId(1, nameof(DispatchAsync)),
        "Loopback dispatch IID={InterfaceId}, opnum={Opnum}");

    private static readonly Action<ILogger, int, int, Exception?> PublishingChange = LoggerMessage.Define<int, int>(
        LogLevel.Information,
        new EventId(2, nameof(PublishDataChangeAsync)),
        "Publishing OnDataChange for group {GroupHandle} to {SubscriberCount} subscriber(s)");

    private readonly object _gate = new();
    private readonly IOpcDaServer _server;
    private readonly OpcDaServerDispatcher _serverDispatcher;
    private readonly LoopbackTagStore _tags;
    private readonly ILogger<LoopbackDaRuntime> _logger;
    private readonly Dictionary<int, LoopbackGroup> _groups = new();
    private readonly Dictionary<int, IOPCDataCallback> _subscribers = new();
    private int _nextItemHandle = 2000;
    private int _nextCookie;
    private bool _callbacksEnabled;

    public LoopbackDaRuntime(
        IOpcDaServer server,
        LoopbackTagStore tags,
        ILogger<LoopbackDaRuntime> logger) {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _serverDispatcher = new OpcDaServerDispatcher(server);
        _tags = tags ?? throw new ArgumentNullException(nameof(tags));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        DispatchingCall(_logger, interfaceId, opnum, null);

        if (interfaceId == IOPCServer.InterfaceId) {
            return DispatchServerAsync(opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCBrowseServerAddressSpace.InterfaceId) {
            return DispatchBrowseAsync(opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCItemMgt.InterfaceId) {
            return DispatchItemManagementAsync(opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCSyncIO.InterfaceId) {
            return DispatchSyncIoAsync(opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCAsyncIO2.InterfaceId) {
            return DispatchAsyncIoAsync(opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IConnectionPoint.InterfaceId) {
            return DispatchConnectionPointAsync(opnum, cancellationToken);
        }

        return Task.FromResult(NotImplemented());
    }

    public int Advise(IOPCDataCallback callback) {
        ArgumentNullException.ThrowIfNull(callback);
        lock (_gate) {
            int cookie = ++_nextCookie;
            _subscribers[cookie] = callback;
            return cookie;
        }
    }

    public void Unadvise(int cookie) {
        lock (_gate) {
            _subscribers.Remove(cookie);
        }
    }

    public async Task RunPublisherAsync(TimeSpan duration, TimeSpan interval, CancellationToken cancellationToken) {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(duration, TimeSpan.Zero);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(interval, TimeSpan.Zero);

        DateTimeOffset stopAt = DateTimeOffset.UtcNow + duration;
        while (DateTimeOffset.UtcNow < stopAt) {
            cancellationToken.ThrowIfCancellationRequested();
            await PublishDataChangeAsync(transactionId: 0, cancellationToken).ConfigureAwait(false);
            await Task.Delay(interval, cancellationToken).ConfigureAwait(false);
        }
    }

    private Task<NdrCallResult> DispatchServerAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) {
        return opnum switch {
            IOPCServer.Opnums.GetStatusAsync => _serverDispatcher.DispatchAsync(
                IOPCServer.InterfaceId,
                opnum,
                requestPayload,
                cancellationToken),
            AddGroupOpnum => DispatchAddGroupAsync(requestPayload, cancellationToken),
            IOPCServer.Opnums.RemoveGroupAsync => DispatchRemoveGroupAsync(requestPayload, cancellationToken),
            IOPCServer.Opnums.GetErrorStringAsync => _serverDispatcher.DispatchAsync(
                IOPCServer.InterfaceId,
                opnum,
                requestPayload,
                cancellationToken),
            _ => Task.FromResult(NotImplemented()),
        };
    }

    private async Task<NdrCallResult> DispatchAddGroupAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) {
        var reader = new NdrReader(requestPayload.Span);
        string name = reader.ReadUnicodeStringPtr() ?? "Loopback";
        bool active = reader.ReadInt32() != 0;
        int requestedUpdateRate = reader.ReadInt32();
        int clientHandle = reader.ReadInt32();
        int localeId = reader.ReadInt32();

        int serverHandle = await _server.AddGroupAsync(
            name,
            active,
            requestedUpdateRate,
            clientHandle,
            localeId,
            cancellationToken).ConfigureAwait(false);

        lock (_gate) {
            _groups[serverHandle] = new LoopbackGroup(serverHandle, name, active, requestedUpdateRate, clientHandle);
        }

        return Ok(LoopbackNdr.Write((ref NdrWriter writer) => writer.WriteInt32(serverHandle)));
    }

    private async Task<NdrCallResult> DispatchRemoveGroupAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) {
        var reader = new NdrReader(requestPayload.Span);
        int serverGroupHandle = reader.ReadInt32();
        bool force = reader.ReadInt32() != 0;

        await _server.RemoveGroupAsync(serverGroupHandle, force, cancellationToken).ConfigureAwait(false);
        lock (_gate) {
            _groups.Remove(serverGroupHandle);
        }

        return Ok(ReadOnlyMemory<byte>.Empty);
    }

    private Task<NdrCallResult> DispatchBrowseAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        if (opnum == IOPCBrowseServerAddressSpace.Opnums.QueryOrganizationAsync) {
            return Task.FromResult(Ok(LoopbackNdr.Write((ref NdrWriter writer) => writer.WriteInt32(1))));
        }

        if (opnum == BrowseItemsOpnum) {
            string[] itemIds = _tags.Browse();
            return Task.FromResult(Ok(LoopbackNdr.Write((ref NdrWriter writer) => LoopbackNdr.WriteStringArray(ref writer, itemIds))));
        }

        if (opnum == IOPCBrowseServerAddressSpace.Opnums.GetItemIdAsync) {
            var reader = new NdrReader(requestPayload.Span);
            string itemDataId = reader.ReadUnicodeStringPtr() ?? string.Empty;
            string itemId = _tags.TryGet(itemDataId, out _) ? itemDataId : string.Empty;
            return Task.FromResult(Ok(LoopbackNdr.Write((ref NdrWriter writer) => writer.WriteUnicodeStringPtr(itemId))));
        }

        return Task.FromResult(NotImplemented());
    }

    private Task<NdrCallResult> DispatchItemManagementAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        if (opnum != AddItemsOpnum) {
            return Task.FromResult(NotImplemented());
        }

        var reader = new NdrReader(requestPayload.Span);
        int groupHandle = reader.ReadInt32();
        int count = checked((int)reader.ReadUInt32());
        var results = new List<OpcItemResult>(count);
        var errors = new List<int>(count);

        lock (_gate) {
            if (!_groups.TryGetValue(groupHandle, out LoopbackGroup? group)) {
                for (var index = 0; index < count; index++) {
                    _ = NdrOpcItemDefCodec.Read(ref reader);
                    results.Add(new OpcItemResult(0, VarType.VT_EMPTY, 0, Array.Empty<byte>()));
                    errors.Add(OpcResultId.InvalidHandle.Code);
                }
            }
            else {
                for (var index = 0; index < count; index++) {
                    OpcItemDef item = NdrOpcItemDefCodec.Read(ref reader);
                    string itemId = item.ItemId ?? string.Empty;
                    if (_tags.TryGet(itemId, out LoopbackTag? tag)) {
                        int serverHandle = ++_nextItemHandle;
                        group.Items[serverHandle] = new LoopbackItemBinding(
                            serverHandle,
                            itemId,
                            item.ClientHandle,
                            item.Active);
                        results.Add(new OpcItemResult(serverHandle, tag.CanonicalDataType, tag.AccessRights, Array.Empty<byte>()));
                        errors.Add(OpcResultId.Ok.Code);
                    }
                    else {
                        results.Add(new OpcItemResult(0, VarType.VT_EMPTY, 0, Array.Empty<byte>()));
                        errors.Add(OpcResultId.UnknownItemId.Code);
                    }
                }
            }
        }

        ReadOnlyMemory<byte> response = LoopbackNdr.Write((ref NdrWriter writer) => {
            writer.WriteUInt32((uint)results.Count);
            foreach (OpcItemResult result in results) {
                NdrOpcItemResultCodec.Write(ref writer, result);
            }

            LoopbackNdr.WriteInt32Array(ref writer, errors);
        });
        return Task.FromResult(Ok(response));
    }

    private Task<NdrCallResult> DispatchSyncIoAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) {
        return opnum switch {
            ReadOpnum => DispatchReadAsync(requestPayload, cancellationToken),
            IOPCSyncIO.Opnums.WriteAsync => DispatchWriteAsync(requestPayload, cancellationToken),
            _ => Task.FromResult(NotImplemented()),
        };
    }

    private Task<NdrCallResult> DispatchReadAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        var reader = new NdrReader(requestPayload.Span);
        _ = reader.ReadInt32();
        int[] serverHandles = LoopbackNdr.ReadInt32Array(ref reader);

        List<OpcItemState> states = new(serverHandles.Length);
        List<int> errors = new(serverHandles.Length);
        foreach (int serverHandle in serverHandles) {
            if (TryReadState(serverHandle, out OpcItemState? state, out int error) && state is not null) {
                states.Add(state);
            }
            else {
                states.Add(new OpcItemState(0, DateTimeOffset.UtcNow, OpcQuality.Bad, OpcVariant.Empty));
            }

            errors.Add(error);
        }

        ReadOnlyMemory<byte> response = LoopbackNdr.Write((ref NdrWriter writer) => {
            writer.WriteUInt32((uint)states.Count);
            foreach (OpcItemState state in states) {
                NdrOpcItemStateCodec.Write(ref writer, state);
            }

            LoopbackNdr.WriteInt32Array(ref writer, errors);
        });
        return Task.FromResult(Ok(response));
    }

    private async Task<NdrCallResult> DispatchWriteAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        var reader = new NdrReader(requestPayload.Span);
        int[] serverHandles = LoopbackNdr.ReadInt32Array(ref reader);
        OpcVariant[] values = LoopbackNdr.ReadVariantArray(ref reader);
        var errors = new int[serverHandles.Length];
        bool wroteAny = false;

        for (var index = 0; index < serverHandles.Length; index++) {
            OpcVariant value = index < values.Length ? values[index] : OpcVariant.Empty;
            errors[index] = TryWriteValue(serverHandles[index], value) ? OpcResultId.Ok.Code : OpcResultId.BadRights.Code;
            wroteAny |= errors[index] == OpcResultId.Ok.Code;
        }

        if (wroteAny) {
            await PublishDataChangeAsync(transactionId: 0, cancellationToken).ConfigureAwait(false);
        }

        return Ok(LoopbackNdr.Write((ref NdrWriter writer) => LoopbackNdr.WriteInt32Array(ref writer, errors)));
    }

    private Task<NdrCallResult> DispatchAsyncIoAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) {
        if (opnum == IOPCAsyncIO2.Opnums.SetEnableAsync) {
            var reader = new NdrReader(requestPayload.Span);
            bool enabled = reader.ReadInt32() != 0;
            lock (_gate) {
                _callbacksEnabled = enabled;
            }

            return Task.FromResult(Ok(ReadOnlyMemory<byte>.Empty));
        }

        if (opnum == IOPCAsyncIO2.Opnums.GetEnableAsync) {
            bool enabled;
            lock (_gate) {
                enabled = _callbacksEnabled;
            }

            return Task.FromResult(Ok(LoopbackNdr.Write((ref NdrWriter writer) => writer.WriteInt32(enabled ? -1 : 0))));
        }

        if (opnum == IOPCAsyncIO2.Opnums.Refresh2Async) {
            return DispatchRefreshAsync(requestPayload, cancellationToken);
        }

        if (opnum == IOPCAsyncIO2.Opnums.Cancel2Async) {
            return Task.FromResult(Ok(ReadOnlyMemory<byte>.Empty));
        }

        return Task.FromResult(NotImplemented());
    }

    private async Task<NdrCallResult> DispatchRefreshAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) {
        var reader = new NdrReader(requestPayload.Span);
        _ = reader.ReadInt32();
        int transactionId = reader.ReadInt32();
        await PublishDataChangeAsync(transactionId, cancellationToken).ConfigureAwait(false);
        int cancelId = transactionId + 10_000;
        return Ok(LoopbackNdr.Write((ref NdrWriter writer) => writer.WriteInt32(cancelId)));
    }

    private static Task<NdrCallResult> DispatchConnectionPointAsync(int opnum, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        if (opnum == IConnectionPoint.Opnums.GetConnectionInterfaceAsync) {
            return Task.FromResult(Ok(LoopbackNdr.Write((ref NdrWriter writer) => writer.WriteGuid(IOPCDataCallback.InterfaceId))));
        }

        return Task.FromResult(NotImplemented());
    }

    private async Task PublishDataChangeAsync(int transactionId, CancellationToken cancellationToken) {
        (IOPCDataCallback[] subscribers, LoopbackPublishGroup[] groups) = SnapshotSubscribersAndGroups();
        if (subscribers.Length == 0 || groups.Length == 0) {
            return;
        }

        foreach (LoopbackPublishGroup group in groups) {
            if (group.Items.Length == 0) {
                continue;
            }

            PublishingChange(_logger, group.ServerHandle, subscribers.Length, null);
            OpcVariant[] values = new OpcVariant[group.Items.Length];
            ushort[] qualities = new ushort[group.Items.Length];
            long[] timestamps = new long[group.Items.Length];
            int[] errors = new int[group.Items.Length];
            int[] clientHandles = new int[group.Items.Length];

            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            for (var index = 0; index < group.Items.Length; index++) {
                LoopbackItemBinding item = group.Items[index];
                clientHandles[index] = item.ClientHandle;
                timestamps[index] = LoopbackNdr.ToFileTime(timestamp);
                if (_tags.TryGet(item.ItemId, out LoopbackTag? tag)) {
                    values[index] = tag.Read();
                    qualities[index] = OpcQuality.Good.RawValue;
                    errors[index] = OpcResultId.Ok.Code;
                }
                else {
                    values[index] = OpcVariant.Empty;
                    qualities[index] = OpcQuality.Bad.RawValue;
                    errors[index] = OpcResultId.UnknownItemId.Code;
                }
            }

            foreach (IOPCDataCallback subscriber in subscribers) {
                await subscriber.OnDataChangeAsync(
                    transactionId,
                    group.ServerHandle,
                    OpcQuality.Good.RawValue,
                    OpcResultId.Ok.Code,
                    clientHandles,
                    values,
                    qualities,
                    timestamps,
                    errors,
                    cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private (IOPCDataCallback[] Subscribers, LoopbackPublishGroup[] Groups) SnapshotSubscribersAndGroups() {
        lock (_gate) {
            if (!_callbacksEnabled) {
                return (Array.Empty<IOPCDataCallback>(), Array.Empty<LoopbackPublishGroup>());
            }

            var subscribers = _subscribers.Values.ToArray();
            var groups = new List<LoopbackPublishGroup>(_groups.Count);
            foreach (LoopbackGroup group in _groups.Values) {
                if (!group.Active) {
                    continue;
                }

                LoopbackItemBinding[] items = group.Items.Values.Where(static item => item.Active).ToArray();
                groups.Add(new LoopbackPublishGroup(group.ServerHandle, items));
            }

            return (subscribers, groups.ToArray());
        }
    }

    private bool TryReadState(int serverHandle, out OpcItemState? state, out int error) {
        LoopbackItemBinding? item = FindItem(serverHandle);
        if (item is null) {
            state = null;
            error = OpcResultId.InvalidHandle.Code;
            return false;
        }

        if (!_tags.TryGet(item.ItemId, out LoopbackTag? tag)) {
            state = null;
            error = OpcResultId.UnknownItemId.Code;
            return false;
        }

        state = new OpcItemState(item.ClientHandle, DateTimeOffset.UtcNow, OpcQuality.Good, tag.Read());
        error = OpcResultId.Ok.Code;
        return true;
    }

    private bool TryWriteValue(int serverHandle, OpcVariant value) {
        LoopbackItemBinding? item = FindItem(serverHandle);
        return item is not null && _tags.TryGet(item.ItemId, out LoopbackTag? tag) && tag.TryWrite(value);
    }

    private LoopbackItemBinding? FindItem(int serverHandle) {
        lock (_gate) {
            foreach (LoopbackGroup group in _groups.Values) {
                if (group.Items.TryGetValue(serverHandle, out LoopbackItemBinding? item)) {
                    return item;
                }
            }
        }

        return null;
    }

    private static NdrCallResult Ok(ReadOnlyMemory<byte> response) => new(OpcResultId.Ok.Code, response);

    private static NdrCallResult NotImplemented() => new(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
}
