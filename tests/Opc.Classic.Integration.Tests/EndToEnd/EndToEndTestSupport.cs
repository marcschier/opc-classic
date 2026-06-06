//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Ae.Ndr;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Da.Ndr;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Channels;
using Opc.Classic.Dcom.Orpc;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hda.Ndr;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using Opc.Classic.Samples.AeServer;
using Opc.Classic.Samples.DaServer;
using Opc.Classic.Samples.HdaServer;
using Opc.Classic.Testing;

namespace Opc.Classic.Integration.Tests.EndToEnd;

internal delegate void NdrWriteAction(ref NdrWriter writer);

internal static class EndToEndNdr
{
    private const int DefaultBufferSize = 128 * 1024;
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    public static ReadOnlyMemory<byte> Write(NdrWriteAction write, int capacity = DefaultBufferSize)
    {
        ArgumentNullException.ThrowIfNull(write);
        var buffer = new byte[capacity];
        var writer = new NdrWriter(buffer);
        write(ref writer);
        return buffer.AsMemory(0, writer.Position).ToArray();
    }

    public static void WriteInt32Array(ref NdrWriter writer, IReadOnlyList<int> values)
    {
        writer.WriteUInt32(unchecked((uint)values.Count));
        foreach (int value in values)
        {
            writer.WriteInt32(value);
        }
    }

    public static int[] ReadInt32Array(ref NdrReader reader)
    {
        int count = checked((int)reader.ReadUInt32());
        var values = new int[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = reader.ReadInt32();
        }

        return values;
    }

    public static void WriteStringArray(ref NdrWriter writer, IReadOnlyList<string> values)
    {
        writer.WriteUInt32(unchecked((uint)values.Count));
        foreach (string value in values)
        {
            writer.WriteUnicodeStringPtr(value);
        }
    }

    public static string[] ReadStringArray(ref NdrReader reader)
    {
        int count = checked((int)reader.ReadUInt32());
        var values = new string[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = reader.ReadUnicodeStringPtr() ?? string.Empty;
        }

        return values;
    }

    public static void WriteVariantArray(ref NdrWriter writer, IReadOnlyList<OpcVariant> values)
    {
        writer.WriteUInt32(unchecked((uint)values.Count));
        foreach (OpcVariant value in values)
        {
            writer.WriteVariant(value);
        }
    }

    public static OpcVariant[] ReadVariantArray(ref NdrReader reader)
    {
        int count = checked((int)reader.ReadUInt32());
        var values = new OpcVariant[count];
        for (int i = 0; i < values.Length; i++)
        {
            values[i] = reader.ReadVariant();
        }

        return values;
    }

    public static long ToFileTime(DateTimeOffset value) => value.UtcTicks - FileTimeEpochOffsetTicks;

    public static DateTimeOffset FromFileTime(long fileTimeTicks) => new(fileTimeTicks + FileTimeEpochOffsetTicks, TimeSpan.Zero);
}

internal static class DispatchResultExtensions
{
    public static async Task<NdrCallResult> ToCallResultAsync(this ValueTask<DispatchResult> dispatch) =>
        (await dispatch.ConfigureAwait(false)).ToNdrCallResult();
}

internal sealed record DaAddItemResult(
    string ItemId,
    int ClientHandle,
    int ServerHandle,
    VarType CanonicalDataType,
    int AccessRights,
    int Error);

internal sealed record DaReadResult(
    string ItemId,
    int ClientHandle,
    int ServerHandle,
    OpcVariant Value,
    OpcQuality Quality,
    DateTimeOffset Timestamp,
    int Error);

internal sealed record DaWriteObservation(int ServerHandle, OpcVariant Value, int Error);

internal sealed class DaEndToEndPipeline
{
    private const int AddGroupOpnum = 3;
    private const int HierarchicalOrganization = 1;
    private const int DataSourceCache = 1;

    private readonly object _gate = new();
    private readonly TagTree _tags = new();
    private readonly SampleDaServer _sampleServer;
    private readonly OpcDaServerDispatcher _serverDispatcher;
    private readonly IOPCGroupStateMgtServerDispatcher _groupStateDispatcher;
    private readonly IOPCItemMgtServerDispatcher _itemMgtDispatcher;
    private readonly IOPCSyncIOServerDispatcher _syncIoDispatcher;
    private readonly IOPCSyncIO2ServerDispatcher _syncIo2Dispatcher;
    private readonly IOPCAsyncIO2ServerDispatcher _asyncIo2Dispatcher;
    private readonly IOPCGroupStateMgt2ServerDispatcher _groupState2Dispatcher;
    private readonly IOPCBrowseServerAddressSpaceServerDispatcher _browseDispatcher;
    private readonly Dictionary<int, DaGroup> _groups = new();
    private int _nextItemHandle = 3000;
    private int _currentGroupHandle;

    public DaEndToEndPipeline()
    {
        _sampleServer = new SampleDaServer(_tags, NullLogger<SampleDaServer>.Instance);
        _serverDispatcher = new OpcDaServerDispatcher(_sampleServer);
        _groupStateDispatcher = new IOPCGroupStateMgtServerDispatcher(new GroupStateImpl(this));
        _itemMgtDispatcher = new IOPCItemMgtServerDispatcher(new ItemMgtImpl(this));
        _syncIoDispatcher = new IOPCSyncIOServerDispatcher(new SyncIoImpl(this));
        _syncIo2Dispatcher = new IOPCSyncIO2ServerDispatcher(new SyncIo2Impl(this));
        _asyncIo2Dispatcher = new IOPCAsyncIO2ServerDispatcher(new AsyncIo2Impl(this));
        _groupState2Dispatcher = new IOPCGroupStateMgt2ServerDispatcher(new GroupState2Impl(this));
        _browseDispatcher = new IOPCBrowseServerAddressSpaceServerDispatcher(new BrowseImpl(this));
        Channel = new InMemoryCallChannel(DispatchAsync);
        Server = new IOPCServerClientProxy(Channel);
        GroupState = new IOPCGroupStateMgtClientProxy(Channel);
        GroupState2 = new IOPCGroupStateMgt2ClientProxy(Channel);
        ItemMgt = new IOPCItemMgtClientProxy(Channel);
        SyncIo = new IOPCSyncIOClientProxy(Channel);
        SyncIo2 = new IOPCSyncIO2ClientProxy(Channel);
        AsyncIo2 = new IOPCAsyncIO2ClientProxy(Channel);
        Browse = new IOPCBrowseServerAddressSpaceClientProxy(Channel);
    }

    public InMemoryCallChannel Channel { get; }

    public IOPCServerClientProxy Server { get; }

    public IOPCGroupStateMgtClientProxy GroupState { get; }

    public IOPCGroupStateMgt2ClientProxy GroupState2 { get; }

    public IOPCItemMgtClientProxy ItemMgt { get; }

    public IOPCSyncIOClientProxy SyncIo { get; }

    public IOPCSyncIO2ClientProxy SyncIo2 { get; }

    public IOPCAsyncIO2ClientProxy AsyncIo2 { get; }

    public IOPCBrowseServerAddressSpaceClientProxy Browse { get; }

    public IReadOnlyList<DaWriteObservation> LastWrites { get; private set; } = Array.Empty<DaWriteObservation>();

    public int GroupCount
    {
        get
        {
            lock (_gate)
            {
                return _groups.Count;
            }
        }
    }

    public bool GroupExists(int groupHandle)
    {
        lock (_gate)
        {
            return _groups.ContainsKey(groupHandle);
        }
    }

    public bool IsItemActive(int serverHandle)
    {
        lock (_gate)
        {
            return FindItemCore(serverHandle)?.Active ?? false;
        }
    }

    public async Task<int> AddGroupViaWireAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientHandle,
        int localeId,
        CancellationToken cancellationToken)
    {
        await Server.AddGroupAsync(
            name,
            active,
            requestedUpdateRate,
            clientHandle,
            timeBias: 0,
            percentDeadband: 0.0f,
            localeId,
            IOPCItemMgt.InterfaceId,
            out int serverGroupHandle,
            out _,
            out _,
            cancellationToken).ConfigureAwait(false);
        return serverGroupHandle;
    }

    public async Task<DaAddItemResult[]> AddItemsViaWireAsync(
        int groupHandle,
        IReadOnlyList<(string ItemId, int ClientHandle)> items,
        CancellationToken cancellationToken)
    {
        if (!GroupExists(groupHandle))
        {
            throw new OpcException(OpcResultId.InvalidHandle);
        }

        OpcItemDef[] definitions = items
            .Select(static item => new OpcItemDef(null, item.ItemId, Active: true, item.ClientHandle, Array.Empty<byte>(), VarType.VT_EMPTY))
            .ToArray();
        await ItemMgt.AddItemsAsync(definitions, out OpcItemResult[] itemResults, out int[] errors, cancellationToken)
            .ConfigureAwait(false);

        var results = new DaAddItemResult[items.Count];
        for (int i = 0; i < results.Length; i++)
        {
            OpcItemResult itemResult = itemResults[i];
            results[i] = new DaAddItemResult(
                items[i].ItemId,
                items[i].ClientHandle,
                itemResult.ServerHandle,
                itemResult.CanonicalDataType,
                itemResult.AccessRights,
                errors[i]);
        }

        return results;
    }

    public async Task<DaReadResult[]> ReadViaWireAsync(IReadOnlyList<DaAddItemResult> items, CancellationToken cancellationToken)
    {
        int[] serverHandles = items.Select(static item => item.ServerHandle).ToArray();
        OpcItemState[] states = await SyncIo.ReadAsync(DataSourceCache, serverHandles, out int[] errors, cancellationToken)
            .ConfigureAwait(false);

        var results = new DaReadResult[items.Count];
        for (int i = 0; i < results.Length; i++)
        {
            OpcItemState state = states[i];
            results[i] = new DaReadResult(
                items[i].ItemId,
                state.ClientHandle,
                items[i].ServerHandle,
                state.Value,
                state.Quality,
                state.Timestamp,
                errors[i]);
        }

        return results;
    }

    public Task<string[]> BrowseItemsViaWireAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_tags.Tags.Keys.Order(StringComparer.Ordinal).ToArray());
    }

    private Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (interfaceId == IOPCServer.InterfaceId)
        {
            return DispatchServerAsync(opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCGroupStateMgt.InterfaceId)
        {
            return _groupStateDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ToCallResultAsync();
        }

        if (interfaceId == IOPCGroupStateMgt2.InterfaceId)
        {
            return _groupState2Dispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ToCallResultAsync();
        }

        if (interfaceId == IOPCItemMgt.InterfaceId)
        {
            return _itemMgtDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ToCallResultAsync();
        }

        if (interfaceId == IOPCSyncIO.InterfaceId)
        {
            return _syncIoDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ToCallResultAsync();
        }

        if (interfaceId == IOPCSyncIO2.InterfaceId)
        {
            return _syncIo2Dispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ToCallResultAsync();
        }

        if (interfaceId == IOPCAsyncIO2.InterfaceId)
        {
            return _asyncIo2Dispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ToCallResultAsync();
        }

        if (interfaceId == IOPCBrowseServerAddressSpace.InterfaceId)
        {
            return _browseDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ToCallResultAsync();
        }

        return Task.FromResult(NotImplemented());
    }

    private Task<NdrCallResult> DispatchServerAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken) => opnum switch
        {
            AddGroupOpnum or IOPCServer.Opnums.GetStatusAsync or IOPCServer.Opnums.GetErrorStringAsync or IOPCServer.Opnums.RemoveGroupAsync or IOPCServer.Opnums.GetGroupByNameAsync or IOPCServer.Opnums.CreateGroupEnumeratorAsync =>
                DispatchTopLevelServerAsync(opnum, requestPayload, cancellationToken),
            _ => Task.FromResult(NotImplemented()),
        };

    private async Task<NdrCallResult> DispatchTopLevelServerAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        NdrCallResult result = await _serverDispatcher.DispatchAsync(
            IOPCServer.InterfaceId,
            opnum,
            requestPayload,
            cancellationToken).ConfigureAwait(false);
        if (opnum == IOPCServer.Opnums.AddGroupAsync && result.IsSuccess)
        {
            var requestReader = new NdrReader(requestPayload.Span);
            // [OpcRefString] on AddGroup.name → bare conformant-varying
            // string (no referent prefix) per DCE 1.1 §4.2.2.7 top-level
            // [ref] LPCWSTR semantics.
            string name = requestReader.ReadUnicodeString();
            bool active = requestReader.ReadInt32() != 0;
            int requestedUpdateRate = requestReader.ReadInt32();
            int clientHandle = requestReader.ReadInt32();
            // timeBias and percentDeadband are [OpcUniquePointer]: 4-byte referent
            // ID (always 0x00020000 for non-nullable C# scalars) + inline value.
            _ = requestReader.ReadUInt32();
            _ = requestReader.ReadInt32();
            _ = requestReader.ReadUInt32();
            _ = requestReader.ReadSingle();
            int localeId = requestReader.ReadInt32();

            var responseReader = new NdrReader(result.ResponsePayload.Span);
            int serverHandle = responseReader.ReadInt32();
            _ = responseReader.ReadInt32();
            lock (_gate)
            {
                _groups[serverHandle] = new DaGroup(serverHandle, name, active, requestedUpdateRate, clientHandle, localeId);
                _currentGroupHandle = serverHandle;
            }
        }
        else if (opnum == IOPCServer.Opnums.RemoveGroupAsync && result.IsSuccess)
        {
            var reader = new NdrReader(requestPayload.Span);
            int serverGroupHandle = reader.ReadInt32();
            lock (_gate)
            {
                _groups.Remove(serverGroupHandle);
                if (_currentGroupHandle == serverGroupHandle)
                {
                    _currentGroupHandle = 0;
                }
            }
        }

        return result;
    }

    private async Task<NdrCallResult> DispatchAddGroupAsync(
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        var reader = new NdrReader(requestPayload.Span);
        string name = reader.ReadUnicodeStringPtr() ?? "EndToEnd";
        bool active = reader.ReadInt32() != 0;
        int requestedUpdateRate = reader.ReadInt32();
        int clientHandle = reader.ReadInt32();
        int localeId = reader.ReadInt32();

        int serverHandle = await _sampleServer.AddGroupAsync(
            name,
            active,
            requestedUpdateRate,
            clientHandle,
            localeId,
            cancellationToken).ConfigureAwait(false);

        lock (_gate)
        {
            _groups[serverHandle] = new DaGroup(serverHandle, name, active, requestedUpdateRate, clientHandle, localeId);
            _currentGroupHandle = serverHandle;
        }

        return Ok(EndToEndNdr.Write((ref NdrWriter writer) => writer.WriteInt32(serverHandle)));
    }

    private Task<NdrCallResult> DispatchAddItemsAsync(ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var reader = new NdrReader(requestPayload.Span);
        int groupHandle = reader.ReadInt32();
        int count = checked((int)reader.ReadUInt32());
        var results = new List<OpcItemResult>(count);
        var errors = new List<int>(count);

        lock (_gate)
        {
            bool groupFound = _groups.TryGetValue(groupHandle, out DaGroup? group);
            for (int i = 0; i < count; i++)
            {
                OpcItemDef item = NdrOpcItemDefCodec.Read(ref reader);
                string itemId = item.ItemId ?? string.Empty;
                if (!groupFound || group is null)
                {
                    results.Add(new OpcItemResult(0, VarType.VT_EMPTY, 0, Array.Empty<byte>()));
                    errors.Add(OpcResultId.InvalidHandle.Code);
                    continue;
                }

                if (!_tags.Tags.TryGetValue(itemId, out ITagSource? source))
                {
                    results.Add(new OpcItemResult(0, VarType.VT_EMPTY, 0, Array.Empty<byte>()));
                    errors.Add(OpcResultId.UnknownItemId.Code);
                    continue;
                }

                int serverHandle = ++_nextItemHandle;
                var binding = new DaItemBinding(serverHandle, itemId, item.ClientHandle, item.Active);
                group.Items[serverHandle] = binding;
                results.Add(new OpcItemResult(serverHandle, GuessCanonicalType(source), GuessAccessRights(itemId), Array.Empty<byte>()));
                errors.Add(OpcResultId.Ok.Code);
            }
        }

        ReadOnlyMemory<byte> response = EndToEndNdr.Write((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(unchecked((uint)results.Count));
            foreach (OpcItemResult result in results)
            {
                NdrOpcItemResultCodec.Write(ref writer, result);
            }

            EndToEndNdr.WriteInt32Array(ref writer, errors);
        });
        return Task.FromResult(Ok(response));
    }

    private Task<NdrCallResult> DispatchReadAsync(ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var reader = new NdrReader(requestPayload.Span);
        _ = reader.ReadInt32();
        int[] serverHandles = EndToEndNdr.ReadInt32Array(ref reader);
        var states = new List<OpcItemState>(serverHandles.Length);
        var errors = new List<int>(serverHandles.Length);

        foreach (int serverHandle in serverHandles)
        {
            if (TryReadState(serverHandle, out OpcItemState? state, out int error) && state is not null)
            {
                states.Add(state);
            }
            else
            {
                states.Add(new OpcItemState(0, DateTimeOffset.UtcNow, OpcQuality.Bad, OpcVariant.Empty));
            }

            errors.Add(error);
        }

        ReadOnlyMemory<byte> response = EndToEndNdr.Write((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(unchecked((uint)states.Count));
            foreach (OpcItemState state in states)
            {
                NdrOpcItemStateCodec.Write(ref writer, state);
            }

            EndToEndNdr.WriteInt32Array(ref writer, errors);
        });
        return Task.FromResult(Ok(response));
    }

    private Task<NdrCallResult> DispatchBrowseItemsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string[] itemIds = _tags.Tags.Keys.Order(StringComparer.Ordinal).ToArray();
        return Task.FromResult(Ok(EndToEndNdr.Write((ref NdrWriter writer) => EndToEndNdr.WriteStringArray(ref writer, itemIds))));
    }

    private OpcGroupState CurrentGroupState()
    {
        lock (_gate)
        {
            if (_currentGroupHandle == 0 || !_groups.TryGetValue(_currentGroupHandle, out DaGroup? group))
            {
                throw new OpcException(OpcResultId.InvalidHandle);
            }

            return new OpcGroupState(
                group.ClientHandle,
                group.ServerHandle,
                group.Name,
                group.Active,
                group.UpdateRate,
                group.TimeBias,
                group.PercentDeadband,
                group.LocaleId);
        }
    }

    private int[] SetItemActiveState(int[] serverHandles, bool active)
    {
        var errors = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                DaItemBinding? item = FindItemCore(serverHandles[i]);
                if (item is null)
                {
                    errors[i] = OpcResultId.InvalidHandle.Code;
                    continue;
                }

                item.Active = active;
                errors[i] = OpcResultId.Ok.Code;
            }
        }

        return errors;
    }

    private int[] WriteValues(int[] serverHandles, OpcVariant[] values)
    {
        var errors = new int[serverHandles.Length];
        var observations = new DaWriteObservation[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                OpcVariant value = i < values.Length ? values[i] : OpcVariant.Empty;
                errors[i] = TryWriteValueCore(serverHandles[i], value) ? OpcResultId.Ok.Code : OpcResultId.BadRights.Code;
                observations[i] = new DaWriteObservation(serverHandles[i], value, errors[i]);
            }
        }

        LastWrites = observations;
        return errors;
    }

    private bool TryReadState(int serverHandle, out OpcItemState? state, out int error)
    {
        lock (_gate)
        {
            DaItemBinding? item = FindItemCore(serverHandle);
            if (item is null)
            {
                state = null;
                error = OpcResultId.InvalidHandle.Code;
                return false;
            }

            if (!_tags.Tags.TryGetValue(item.ItemId, out ITagSource? source))
            {
                state = null;
                error = OpcResultId.UnknownItemId.Code;
                return false;
            }

            state = new OpcItemState(item.ClientHandle, DateTimeOffset.UtcNow, OpcQuality.Good, ToVariant(source.Read()));
            error = OpcResultId.Ok.Code;
            return true;
        }
    }

    private bool TryWriteValueCore(int serverHandle, OpcVariant value)
    {
        DaItemBinding? item = FindItemCore(serverHandle);
        return item is not null
            && _tags.Tags.TryGetValue(item.ItemId, out ITagSource? source)
            && source.TryWrite(value.Boxed);
    }

    private DaItemBinding? FindItemCore(int serverHandle)
    {
        foreach (DaGroup group in _groups.Values)
        {
            if (group.Items.TryGetValue(serverHandle, out DaItemBinding? item))
            {
                return item;
            }
        }

        return null;
    }

    private static VarType GuessCanonicalType(ITagSource source)
    {
        try
        {
            return ToVariant(source.Read()).Type;
        }
        catch (OpcException)
        {
            return VarType.VT_EMPTY;
        }
    }

    private static int GuessAccessRights(string itemId) =>
        itemId.StartsWith("Bucket Brigade.", StringComparison.Ordinal) ? 0x3 : 0x1;

    private static OpcVariant ToVariant(object? value) => value switch
    {
        null => OpcVariant.Empty,
        bool typed => OpcVariant.FromBoolean(typed),
        byte typed => OpcVariant.FromUInt8(typed),
        sbyte typed => OpcVariant.FromInt8(typed),
        short typed => OpcVariant.FromInt16(typed),
        ushort typed => OpcVariant.FromUInt16(typed),
        int typed => OpcVariant.FromInt32(typed),
        uint typed => OpcVariant.FromUInt32(typed),
        float typed => OpcVariant.FromSingle(typed),
        double typed => OpcVariant.FromDouble(typed),
        string typed => OpcVariant.FromString(typed),
        _ => OpcVariant.FromString(value.ToString() ?? string.Empty),
    };

    private static NdrCallResult Ok(ReadOnlyMemory<byte> payload) => new(OpcResultId.Ok.Code, payload);

    private static NdrCallResult NotImplemented() => new(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);

    private static void ThrowIfFailure(NdrCallResult result)
    {
        if (result.IsFailure)
        {
            throw new OpcException(new OpcResultId(result.Hresult, null));
        }
    }

    private int UpdateCurrentGroupState(int requestedUpdateRate, bool active, int timeBias, float percentDeadband, int localeId, int clientGroupHandle)
    {
        lock (_gate)
        {
            if (_currentGroupHandle == 0 || !_groups.TryGetValue(_currentGroupHandle, out DaGroup? group))
            {
                throw new OpcException(OpcResultId.InvalidHandle);
            }

            group.UpdateRate = requestedUpdateRate;
            group.Active = active;
            group.TimeBias = timeBias;
            group.PercentDeadband = percentDeadband;
            group.LocaleId = localeId;
            group.ClientHandle = clientGroupHandle;
            return requestedUpdateRate;
        }
    }

    private void RenameCurrentGroup(string name)
    {
        lock (_gate)
        {
            if (_currentGroupHandle == 0 || !_groups.TryGetValue(_currentGroupHandle, out DaGroup? group))
            {
                throw new OpcException(OpcResultId.InvalidHandle);
            }

            group.Name = name;
        }
    }

    private IOpcInterfaceRef CloneCurrentGroup(string name, Guid requestedInterfaceId)
    {
        lock (_gate)
        {
            if (_currentGroupHandle == 0 || !_groups.TryGetValue(_currentGroupHandle, out DaGroup? source))
            {
                throw new OpcException(OpcResultId.InvalidHandle);
            }

            int cloneHandle = source.ServerHandle + 10_000;
            var clone = new DaGroup(cloneHandle, name, source.Active, source.UpdateRate, source.ClientHandle, source.LocaleId)
            {
                TimeBias = source.TimeBias,
                PercentDeadband = source.PercentDeadband,
                KeepAlive = source.KeepAlive,
            };
            foreach (var item in source.Items)
            {
                clone.Items[item.Key] = new DaItemBinding(item.Value.ServerHandle, item.Value.ItemId, item.Value.ClientHandle, item.Value.Active);
            }

            _groups[cloneHandle] = clone;
            return CreateSyntheticInterfaceRef(requestedInterfaceId, cloneHandle);
        }
    }

    private int SetCurrentGroupKeepAlive(int keepAliveTime)
    {
        lock (_gate)
        {
            if (_currentGroupHandle == 0 || !_groups.TryGetValue(_currentGroupHandle, out DaGroup? group))
            {
                throw new OpcException(OpcResultId.InvalidHandle);
            }

            group.KeepAlive = keepAliveTime;
            return keepAliveTime;
        }
    }

    private int GetCurrentGroupKeepAlive()
    {
        lock (_gate)
        {
            if (_currentGroupHandle == 0 || !_groups.TryGetValue(_currentGroupHandle, out DaGroup? group))
            {
                throw new OpcException(OpcResultId.InvalidHandle);
            }

            return group.KeepAlive;
        }
    }

    private (OpcItemResult[] Results, int[] Errors) AddItemsToCurrentGroup(OpcItemDef[] itemDefinitions, bool add)
    {
        var results = new OpcItemResult[itemDefinitions.Length];
        var errors = new int[itemDefinitions.Length];
        lock (_gate)
        {
            DaGroup? group = null;
            bool groupFound = _currentGroupHandle != 0 && _groups.TryGetValue(_currentGroupHandle, out group);
            for (int i = 0; i < itemDefinitions.Length; i++)
            {
                OpcItemDef item = itemDefinitions[i];
                string itemId = item.ItemId ?? string.Empty;
                if (!groupFound || group is null)
                {
                    results[i] = new OpcItemResult(0, VarType.VT_EMPTY, 0, Array.Empty<byte>());
                    errors[i] = OpcResultId.InvalidHandle.Code;
                    continue;
                }

                if (!_tags.Tags.TryGetValue(itemId, out ITagSource? source))
                {
                    results[i] = new OpcItemResult(0, VarType.VT_EMPTY, 0, Array.Empty<byte>());
                    errors[i] = OpcResultId.UnknownItemId.Code;
                    continue;
                }

                int serverHandle = add ? ++_nextItemHandle : 0;
                if (add)
                {
                    group.Items[serverHandle] = new DaItemBinding(serverHandle, itemId, item.ClientHandle, item.Active);
                }

                results[i] = new OpcItemResult(serverHandle, GuessCanonicalType(source), GuessAccessRights(itemId), Array.Empty<byte>());
                errors[i] = OpcResultId.Ok.Code;
            }
        }

        return (results, errors);
    }

    private int[] SetClientHandles(int[] serverHandles, int[] clientHandles)
    {
        var errors = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                DaItemBinding? item = FindItemCore(serverHandles[i]);
                if (item is null)
                {
                    errors[i] = OpcResultId.InvalidHandle.Code;
                    continue;
                }

                item.ClientHandle = i < clientHandles.Length ? clientHandles[i] : item.ClientHandle;
                errors[i] = OpcResultId.Ok.Code;
            }
        }

        return errors;
    }

    private OpcItemState[] ReadStates(int[] serverHandles, out int[] errors)
    {
        var states = new OpcItemState[serverHandles.Length];
        errors = new int[serverHandles.Length];
        for (int i = 0; i < serverHandles.Length; i++)
        {
            if (TryReadState(serverHandles[i], out OpcItemState? state, out int error) && state is not null)
            {
                states[i] = state;
            }
            else
            {
                states[i] = new OpcItemState(0, DateTimeOffset.UtcNow, OpcQuality.Bad, OpcVariant.Empty);
            }

            errors[i] = error;
        }

        return states;
    }

    private sealed class DaGroup
    {
        public DaGroup(int serverHandle, string name, bool active, int updateRate, int clientHandle, int localeId)
        {
            ServerHandle = serverHandle;
            Name = name;
            Active = active;
            UpdateRate = updateRate;
            ClientHandle = clientHandle;
            LocaleId = localeId;
        }

        public int ServerHandle { get; }

        public string Name { get; set; }

        public bool Active { get; set; }

        public int UpdateRate { get; set; }

        public int ClientHandle { get; set; }

        public int LocaleId { get; set; }

        public int TimeBias { get; set; }

        public float PercentDeadband { get; set; }

        public int KeepAlive { get; set; }

        public Dictionary<int, DaItemBinding> Items { get; } = new();
    }

    private sealed class DaItemBinding
    {
        public DaItemBinding(int serverHandle, string itemId, int clientHandle, bool active)
        {
            ServerHandle = serverHandle;
            ItemId = itemId;
            ClientHandle = clientHandle;
            Active = active;
        }

        public int ServerHandle { get; }

        public string ItemId { get; }

        public int ClientHandle { get; set; }

        public bool Active { get; set; }
    }

    private sealed class GroupStateImpl : IOPCGroupStateMgt
    {
        private readonly DaEndToEndPipeline _pipeline;

        public GroupStateImpl(DaEndToEndPipeline pipeline) => _pipeline = pipeline;

        public Task<OpcGroupState> GetStateAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_pipeline.CurrentGroupState());
        }

        public Task SetStateAsync(
            int requestedUpdateRate,
            bool active,
            int timeBias,
            float percentDeadband,
            int localeId,
            int clientGroupHandle,
            out int revisedUpdateRate,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            revisedUpdateRate = _pipeline.UpdateCurrentGroupState(requestedUpdateRate, active, timeBias, percentDeadband, localeId, clientGroupHandle);
            return Task.CompletedTask;
        }

        public Task SetNameAsync(string name, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            cancellationToken.ThrowIfCancellationRequested();
            _pipeline.RenameCurrentGroup(name);
            return Task.CompletedTask;
        }

        public Task<IOpcInterfaceRef> CloneGroupAsync(string name, Guid requestedInterfaceId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_pipeline.CloneCurrentGroup(name, requestedInterfaceId));
        }
    }

    private sealed class GroupState2Impl : IOPCGroupStateMgt2
    {
        private readonly DaEndToEndPipeline _pipeline;

        public GroupState2Impl(DaEndToEndPipeline pipeline) => _pipeline = pipeline;

        public Task<int> SetKeepAliveAsync(int keepAliveTime, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_pipeline.SetCurrentGroupKeepAlive(keepAliveTime));
        }

        public Task<int> GetKeepAliveAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_pipeline.GetCurrentGroupKeepAlive());
        }
    }

    private sealed class ItemMgtImpl : IOPCItemMgt
    {
        private readonly DaEndToEndPipeline _pipeline;

        public ItemMgtImpl(DaEndToEndPipeline pipeline) => _pipeline = pipeline;

        public Task AddItemsAsync(
            OpcItemDef[] itemDefinitions,
            out OpcItemResult[] addResults,
            out int[] errors,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            (addResults, errors) = _pipeline.AddItemsToCurrentGroup(itemDefinitions, add: true);
            return Task.CompletedTask;
        }

        public Task ValidateItemsAsync(
            OpcItemDef[] itemDefinitions,
            bool blobUpdate,
            out OpcItemResult[] validationResults,
            out int[] errors,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = blobUpdate;
            (validationResults, errors) = _pipeline.AddItemsToCurrentGroup(itemDefinitions, add: false);
            return Task.CompletedTask;
        }

        public Task<int[]> RemoveItemsAsync(int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(serverHandles.Select(_ => OpcResultId.Ok.Code).ToArray());
        }

        public Task<int[]> SetActiveStateAsync(int[] serverHandles, bool active, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_pipeline.SetItemActiveState(serverHandles, active));
        }

        public Task<int[]> SetClientHandlesAsync(int[] serverHandles, int[] clientHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_pipeline.SetClientHandles(serverHandles, clientHandles));
        }

        public Task<int[]> SetDatatypesAsync(int[] serverHandles, ushort[] requestedDataTypes, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = requestedDataTypes;
            return Task.FromResult(serverHandles.Select(_ => OpcResultId.Ok.Code).ToArray());
        }

        public Task<IOpcInterfaceRef> CreateEnumeratorAsync(Guid requestedInterfaceId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(CreateSyntheticInterfaceRef(requestedInterfaceId, _pipeline._currentGroupHandle));
        }
    }

    private sealed class SyncIoImpl : IOPCSyncIO
    {
        private readonly DaEndToEndPipeline _pipeline;

        public SyncIoImpl(DaEndToEndPipeline pipeline) => _pipeline = pipeline;

        public Task<OpcItemState[]> ReadAsync(
            int dataSource,
            int[] serverHandles,
            out int[] errors,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = dataSource;
            return Task.FromResult(_pipeline.ReadStates(serverHandles, out errors));
        }

        public Task<int[]> WriteAsync(int[] serverHandles, OpcVariant[] values, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_pipeline.WriteValues(serverHandles, values));
        }
    }

    private sealed class SyncIo2Impl : IOPCSyncIO2
    {
        private readonly DaEndToEndPipeline _pipeline;

        public SyncIo2Impl(DaEndToEndPipeline pipeline) => _pipeline = pipeline;

        public Task<OpcItemState[]> ReadAsync(int dataSource, int[] serverHandles, out int[] errors, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = dataSource;
            return Task.FromResult(_pipeline.ReadStates(serverHandles, out errors));
        }

        public Task<int[]> WriteAsync(int[] serverHandles, OpcVariant[] values, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_pipeline.WriteValues(serverHandles, values));
        }

        public Task ReadMaxAgeAsync(int[] serverHandles, int[] maxAges, out OpcVariant[] values, out ushort[] qualities, out long[] timestamps, out int[] errors, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = maxAges;
            OpcItemState[] states = _pipeline.ReadStates(serverHandles, out errors);
            values = states.Select(static state => state.Value).ToArray();
            qualities = states.Select(static state => state.Quality.RawValue).ToArray();
            timestamps = states.Select(static state => EndToEndNdr.ToFileTime(state.Timestamp)).ToArray();
            return Task.CompletedTask;
        }

        public Task<int[]> WriteVqtAsync(int[] serverHandles, OpcItemVqt[] values, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_pipeline.WriteValues(serverHandles, values.Select(static v => v.Value).ToArray()));
        }
    }

    private sealed class AsyncIo2Impl : IOPCAsyncIO2
    {
        private readonly DaEndToEndPipeline _pipeline;
        private int _nextCancelId = 7000;

        public AsyncIo2Impl(DaEndToEndPipeline pipeline) => _pipeline = pipeline;

        public Task<int> ReadAsync(int[] serverHandles, int transactionId, out int[] errors, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = transactionId;
            _ = _pipeline.ReadStates(serverHandles, out errors);
            return Task.FromResult(Interlocked.Increment(ref _nextCancelId));
        }

        public Task<int> WriteAsync(int[] serverHandles, OpcVariant[] values, int transactionId, out int[] errors, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = transactionId;
            errors = _pipeline.WriteValues(serverHandles, values);
            return Task.FromResult(Interlocked.Increment(ref _nextCancelId));
        }

        public Task<int> Refresh2Async(int dataSource, int transactionId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = dataSource;
            _ = transactionId;
            return Task.FromResult(Interlocked.Increment(ref _nextCancelId));
        }

        public Task Cancel2Async(int cancelId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = cancelId;
            return Task.CompletedTask;
        }

        public Task SetEnableAsync(bool enabled, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = enabled;
            return Task.CompletedTask;
        }

        public Task<bool> GetEnableAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(true);
        }
    }

    private sealed class BrowseImpl : IOPCBrowseServerAddressSpace
    {
        private readonly DaEndToEndPipeline _pipeline;
        private string _position = string.Empty;

        public BrowseImpl(DaEndToEndPipeline pipeline) => _pipeline = pipeline;

        public Task<int> QueryOrganizationAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(HierarchicalOrganization);
        }

        public Task ChangeBrowsePositionAsync(int browseDirection, string browsePosition, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = browseDirection;
            _position = browsePosition;
            return Task.CompletedTask;
        }

        public Task<IOpcInterfaceRef> BrowseOpcItemIdsAsync(int browseFilterType, string filterCriteria, ushort dataTypeFilter, int accessRightsFilter, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = browseFilterType;
            _ = filterCriteria;
            _ = dataTypeFilter;
            _ = accessRightsFilter;
            return Task.FromResult(CreateSyntheticInterfaceRef(OpcGuids.IID_IEnumString, _pipeline._tags.Tags.Count));
        }

        public Task<string> GetItemIdAsync(string itemDataId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string candidate = string.IsNullOrEmpty(_position) ? itemDataId : _position + "." + itemDataId;
            string itemId = _pipeline._tags.Tags.ContainsKey(candidate) ? candidate : itemDataId;
            return Task.FromResult(itemId);
        }

        public Task<IOpcInterfaceRef> BrowseAccessPathsAsync(string itemId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = itemId;
            return Task.FromResult(CreateSyntheticInterfaceRef(OpcGuids.IID_IEnumString, 1));
        }
    }

    private static IOpcInterfaceRef CreateSyntheticInterfaceRef(Guid iid, int discriminator) =>
        new OpcInterfaceRef(
            iid,
            flags: 0,
            publicRefs: 1,
            oxid: 0x1000,
            oid: unchecked((ulong)discriminator),
            ipid: Guid.Empty,
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>());
}

internal sealed record AeSubscriptionHandle(int ServerHandle, int ClientHandle, int RevisedBufferTime, int RevisedMaxSize);

internal sealed record AeAckObservation(
    string AcknowledgerId,
    string Comment,
    long[] ActiveTimes,
    int[] Cookies,
    string[] Sources,
    string[] ConditionNames);

internal sealed class AeEndToEndPipeline
{
    private const int CreateEventSubscriptionOpnum = 4;
    private const int BrowseAreasOpnum = 4;

    private readonly object _gate = new();
    private readonly AeServerImpl _serverImpl;
    private readonly OpcAeServerDispatcher _serverDispatcher;
    private readonly IOPCEventServer2ServerDispatcher _server2Dispatcher;
    private readonly IOPCEventSubscriptionMgtServerDispatcher _subscriptionDispatcher;
    private readonly IOPCEventSubscriptionMgt2ServerDispatcher _subscriptionMgt2Dispatcher;
    private readonly IOPCEventAreaBrowserServerDispatcher _areaBrowserDispatcher;
    private readonly Dictionary<int, AeSubscription> _subscriptions = new();
    private int _nextSubscriptionHandle = 9000;

    public AeEndToEndPipeline()
    {
        _serverImpl = new AeServerImpl(new SampleAeServer(NullLogger<SampleAeServer>.Instance));
        _serverDispatcher = new OpcAeServerDispatcher(_serverImpl);
        _server2Dispatcher = new IOPCEventServer2ServerDispatcher(new AeServer2Impl());
        _subscriptionDispatcher = new IOPCEventSubscriptionMgtServerDispatcher(new SubscriptionMgtImpl(this));
        _subscriptionMgt2Dispatcher = new IOPCEventSubscriptionMgt2ServerDispatcher(new SubscriptionMgt2Impl());
        _areaBrowserDispatcher = new IOPCEventAreaBrowserServerDispatcher(new AreaBrowserImpl());
        Channel = new InMemoryCallChannel(DispatchAsync);
        EventServer = new IOPCEventServerClientProxy(Channel);
        EventServer2 = new IOPCEventServer2ClientProxy(Channel);
        SubscriptionMgt = new IOPCEventSubscriptionMgtClientProxy(Channel);
        SubscriptionMgt2 = new IOPCEventSubscriptionMgt2ClientProxy(Channel);
        AreaBrowser = new IOPCEventAreaBrowserClientProxy(Channel);
    }

    public InMemoryCallChannel Channel { get; }

    public IOPCEventServerClientProxy EventServer { get; }

    public IOPCEventServer2ClientProxy EventServer2 { get; }

    public IOPCEventSubscriptionMgtClientProxy SubscriptionMgt { get; }

    public IOPCEventSubscriptionMgt2ClientProxy SubscriptionMgt2 { get; }

    public IOPCEventAreaBrowserClientProxy AreaBrowser { get; }

    public AeAckObservation? LastAck => _serverImpl.LastAck;

    public int ActiveSubscriptionCount
    {
        get
        {
            lock (_gate)
            {
                return _subscriptions.Count;
            }
        }
    }

    public async Task<AeSubscriptionHandle> CreateEventSubscriptionViaWireAsync(
        bool active,
        int bufferTime,
        int maxSize,
        int clientHandle,
        AeEventSink sink,
        CancellationToken cancellationToken)
    {
        ReadOnlyMemory<byte> payload = EndToEndNdr.Write((ref NdrWriter writer) =>
        {
            writer.WriteInt32(active ? -1 : 0);
            writer.WriteInt32(bufferTime);
            writer.WriteInt32(maxSize);
            writer.WriteInt32(clientHandle);
        });
        NdrCallResult result = await Channel.InvokeAsync(
            IOPCEventServer.InterfaceId,
            CreateEventSubscriptionOpnum,
            payload,
            cancellationToken).ConfigureAwait(false);
        ThrowIfFailure(result);

        var reader = new NdrReader(result.ResponsePayload.Span);
        int serverHandle = reader.ReadInt32();
        int revisedBufferTime = reader.ReadInt32();
        int revisedMaxSize = reader.ReadInt32();
        lock (_gate)
        {
            _subscriptions[serverHandle] = new AeSubscription(serverHandle, clientHandle, sink);
        }

        return new AeSubscriptionHandle(serverHandle, clientHandle, revisedBufferTime, revisedMaxSize);
    }

    public async Task EmitEventAsync(int subscriptionHandle, OpcEventNotification notification, CancellationToken cancellationToken)
    {
        AeSubscription subscription;
        lock (_gate)
        {
            subscription = _subscriptions[subscriptionHandle];
        }

        var sinkProxy = new IOPCEventSinkClientProxy(subscription.Sink.Channel);
        await sinkProxy.OnEventAsync(subscription.ClientHandle, refresh: false, lastRefresh: true, [notification], cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<string[]> BrowseAreasViaWireAsync(CancellationToken cancellationToken)
    {
        NdrCallResult result = await Channel.InvokeAsync(
            IOPCEventAreaBrowser.InterfaceId,
            BrowseAreasOpnum,
            ReadOnlyMemory<byte>.Empty,
            cancellationToken).ConfigureAwait(false);
        ThrowIfFailure(result);
        var reader = new NdrReader(result.ResponsePayload.Span);
        return EndToEndNdr.ReadStringArray(ref reader);
    }

    public bool CleanupSubscription(int subscriptionHandle)
    {
        lock (_gate)
        {
            return _subscriptions.Remove(subscriptionHandle);
        }
    }

    private Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (interfaceId == IOPCEventServer.InterfaceId)
        {
            return opnum == CreateEventSubscriptionOpnum
                ? DispatchCreateEventSubscriptionAsync(requestPayload, cancellationToken)
                : _serverDispatcher.DispatchAsync(interfaceId, opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCEventServer2.InterfaceId)
        {
            return _server2Dispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ToCallResultAsync();
        }

        if (interfaceId == IOPCEventSubscriptionMgt.InterfaceId)
        {
            return _subscriptionDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ToCallResultAsync();
        }

        if (interfaceId == IOPCEventSubscriptionMgt2.InterfaceId)
        {
            return _subscriptionMgt2Dispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ToCallResultAsync();
        }

        if (interfaceId == IOPCEventAreaBrowser.InterfaceId)
        {
            return opnum == BrowseAreasOpnum
                ? DispatchBrowseAreasAsync(cancellationToken)
                : _areaBrowserDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ToCallResultAsync();
        }

        return Task.FromResult(NotImplemented());
    }

    private Task<NdrCallResult> DispatchCreateEventSubscriptionAsync(ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var reader = new NdrReader(requestPayload.Span);
        _ = reader.ReadInt32() != 0;
        int bufferTime = reader.ReadInt32();
        int maxSize = reader.ReadInt32();
        int clientHandle = reader.ReadInt32();
        int serverHandle = Interlocked.Increment(ref _nextSubscriptionHandle);
        int revisedBufferTime = Math.Max(bufferTime, 250);
        int revisedMaxSize = Math.Max(maxSize, 1);
        _ = clientHandle;
        ReadOnlyMemory<byte> response = EndToEndNdr.Write((ref NdrWriter writer) =>
        {
            writer.WriteInt32(serverHandle);
            writer.WriteInt32(revisedBufferTime);
            writer.WriteInt32(revisedMaxSize);
        });
        return Task.FromResult(Ok(response));
    }

    private static Task<NdrCallResult> DispatchBrowseAreasAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string[] areas = ["Plant1", "Plant1.AreaA", "Plant1.AreaB"];
        return Task.FromResult(Ok(EndToEndNdr.Write((ref NdrWriter writer) => EndToEndNdr.WriteStringArray(ref writer, areas))));
    }

    private static NdrCallResult Ok(ReadOnlyMemory<byte> payload) => new(OpcResultId.Ok.Code, payload);

    private static NdrCallResult NotImplemented() => new(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);

    private static void ThrowIfFailure(NdrCallResult result)
    {
        if (result.IsFailure)
        {
            throw new OpcException(new OpcResultId(result.Hresult, null));
        }
    }

    private sealed record AeSubscription(int ServerHandle, int ClientHandle, AeEventSink Sink);

    private sealed class AeServerImpl : IOpcAeServer
    {
        private readonly SampleAeServer _sample;

        public AeServerImpl(SampleAeServer sample) => _sample = sample;

        public AeAckObservation? LastAck { get; private set; }

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            _sample.GetStatusAsync(cancellationToken);

        public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default) =>
            _sample.QueryAvailableFiltersAsync(cancellationToken);

        public Task QueryEventCategoriesAsync(
            int eventType,
            out int[] eventCategories,
            out string[] eventCategoryDescriptions,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = eventType;
            eventCategories = [0x1001, 0x1002, 0x1003];
            eventCategoryDescriptions = ["Simple", "Condition", "Tracking"];
            return Task.CompletedTask;
        }

        public Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = eventCategory;
            return Task.FromResult(new[] { "LevelHigh", "PressureLow" });
        }

        public Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(conditionName == "LevelHigh" ? ["Hi", "HiHi"] : Array.Empty<string>());
        }

        public Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = source;
            return Task.FromResult(new[] { "LevelHigh" });
        }

        public Task QueryEventAttributesAsync(
            int eventCategory,
            out int[] attributeIds,
            out string[] attributeDescriptions,
            out ushort[] attributeTypes,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = eventCategory;
            attributeIds = [10, 11];
            attributeDescriptions = ["Area", "Limit"];
            attributeTypes = [(ushort)VarType.VT_BSTR, (ushort)VarType.VT_R8];
            return Task.CompletedTask;
        }

        public Task TranslateToItemIDsAsync(
            string source,
            int eventCategory,
            string conditionName,
            string subconditionName,
            int[] associatedAttributeIds,
            out string[] attributeItemIds,
            out string[] nodeNames,
            out Guid[] classIds,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = eventCategory;
            _ = conditionName;
            _ = subconditionName;
            attributeItemIds = associatedAttributeIds.Select(id => source + ".Attr" + id.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            nodeNames = associatedAttributeIds.Select(static _ => "AeNode").ToArray();
            classIds = associatedAttributeIds.Select(static _ => Guid.Empty).ToArray();
            return Task.CompletedTask;
        }

        public Task<OpcConditionState> GetConditionStateAsync(
            string source,
            string conditionName,
            int[] attributeIds,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = source;
            _ = conditionName;
            return Task.FromResult(new OpcConditionState(
                state: 3,
                activeSubCondition: "HiHi",
                activeSubConditionDefinition: "High high level",
                activeSubConditionSeverity: 900,
                activeSubConditionDescription: "Level exceeded high-high threshold",
                quality: OpcQuality.Good,
                lastAckTime: DateTimeOffset.UnixEpoch,
                subConditionLastActive: DateTimeOffset.UnixEpoch.AddSeconds(1),
                conditionLastActive: DateTimeOffset.UnixEpoch.AddSeconds(1),
                conditionLastInactive: DateTimeOffset.UnixEpoch,
                acknowledgerId: null,
                comment: null,
                subConditionNames: ["Hi", "HiHi"],
                subConditionDefinitions: ["High level", "High high level"],
                subConditionSeverities: [700, 900],
                subConditionDescriptions: ["High", "High high"],
                eventAttributes: attributeIds.Select(static _ => OpcVariant.FromString("attr")).ToArray(),
                errors: attributeIds.Select(static _ => OpcResultId.Ok.Code).ToArray()));
        }

        public Task EnableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = areas;
            return Task.CompletedTask;
        }

        public Task EnableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = sources;
            return Task.CompletedTask;
        }

        public Task DisableConditionByAreaAsync(string[] areas, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = areas;
            return Task.CompletedTask;
        }

        public Task DisableConditionBySourceAsync(string[] sources, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = sources;
            return Task.CompletedTask;
        }

        public Task<int[]> AckConditionAsync(
            int dwCount,
            string acknowledgerId,
            string comment,
            string[] sources,
            string[] conditionNames,
            long[] activeTimes,
            int[] cookies,
            CancellationToken cancellationToken = default)
        {
            _ = dwCount;
            cancellationToken.ThrowIfCancellationRequested();
            LastAck = new AeAckObservation(acknowledgerId, comment, activeTimes, cookies, sources, conditionNames);
            return Task.FromResult(cookies.Select(_ => OpcResultId.Ok.Code).ToArray());
        }
    }

    private sealed class AeServer2Impl : IOPCEventServer2
    {
        public Task<int[]> EnableConditionByArea2Async(string[] areas, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(AreaResults(areas));
        }

        public Task<int[]> EnableConditionBySource2Async(string[] sources, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(sources.Select(static _ => OpcResultId.Ok.Code).ToArray());
        }

        public Task<int[]> DisableConditionByArea2Async(string[] areas, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(AreaResults(areas));
        }

        public Task<int[]> DisableConditionBySource2Async(string[] sources, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(sources.Select(static _ => OpcResultId.Ok.Code).ToArray());
        }

        public Task GetEnableStateByAreaAsync(
            string[] areas,
            out bool[] enabled,
            out bool[] effectivelyEnabled,
            out int[] errors,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            enabled = areas.Select(static area => area != "Missing").ToArray();
            effectivelyEnabled = areas.Select(static area => area != "Missing").ToArray();
            errors = AreaResults(areas);
            return Task.CompletedTask;
        }

        public Task GetEnableStateBySourceAsync(
            string[] sources,
            out bool[] enabled,
            out bool[] effectivelyEnabled,
            out int[] errors,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            enabled = sources.Select(static source => source != "Missing").ToArray();
            effectivelyEnabled = sources.Select(static source => source != "Missing").ToArray();
            errors = sources.Select(static source => source == "Missing" ? OpcResultId.InvalidArg.Code : OpcResultId.Ok.Code).ToArray();
            return Task.CompletedTask;
        }

        private static int[] AreaResults(string[] areas) =>
            areas.Select(static area => area == "Missing" ? OpcResultId.InvalidArg.Code : OpcResultId.Ok.Code).ToArray();
    }

    private sealed class SubscriptionMgtImpl : IOPCEventSubscriptionMgt
    {
        private readonly AeEndToEndPipeline _pipeline;
        private int _eventType = 0x1F;
        private int[] _eventCategories = [0x1001, 0x1002];
        private int _lowSeverity;
        private int _highSeverity = 1000;
        private string[] _areas = ["Plant1.AreaA"];
        private string[] _sources = ["Plant1.AreaA.Tank7"];
        private int[] _returnedAttributes = [1, 2, 3];
        private bool _active = true;
        private int _bufferTime = 250;
        private int _maxSize = 10;
        private int _clientSubscription = 0xAA01;
        private int _lastCanceledConnection;

        public SubscriptionMgtImpl(AeEndToEndPipeline pipeline) => _pipeline = pipeline;

        public int LastCanceledConnection => _lastCanceledConnection;

        public Task SetFilterAsync(
            int eventType,
            int[] eventCategories,
            int lowSeverity,
            int highSeverity,
            string[] areas,
            string[] sources,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _eventType = eventType;
            _eventCategories = eventCategories;
            _lowSeverity = lowSeverity;
            _highSeverity = highSeverity;
            _areas = areas;
            _sources = sources;
            return Task.CompletedTask;
        }

        public Task GetFilterAsync(
            out int eventType,
            out int[] eventCategories,
            out int lowSeverity,
            out int highSeverity,
            out string[] areas,
            out string[] sources,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            eventType = _eventType;
            eventCategories = _eventCategories;
            lowSeverity = _lowSeverity;
            highSeverity = _highSeverity;
            areas = _areas;
            sources = _sources;
            return Task.CompletedTask;
        }

        public Task SetReturnedAttributesAsync(int eventCategory, int[] attributeIds, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = eventCategory;
            _returnedAttributes = attributeIds;
            return Task.CompletedTask;
        }

        public Task<int[]> GetReturnedAttributesAsync(int eventCategory, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = eventCategory;
            return Task.FromResult(_returnedAttributes);
        }

        public Task RefreshAsync(int connection, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = connection;
            return Task.CompletedTask;
        }

        public Task CancelRefreshAsync(int connection, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _lastCanceledConnection = connection;
            _ = _pipeline;
            return Task.CompletedTask;
        }

        public Task GetStateAsync(
            out bool active,
            out int bufferTime,
            out int maxSize,
            out int clientSubscription,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            active = _active;
            bufferTime = _bufferTime;
            maxSize = _maxSize;
            clientSubscription = _clientSubscription;
            return Task.CompletedTask;
        }

        public Task SetStateAsync(
            bool active,
            int bufferTime,
            int maxSize,
            int clientSubscription,
            out int revisedBufferTime,
            out int revisedMaxSize,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _active = active;
            _bufferTime = Math.Max(bufferTime, 250);
            _maxSize = Math.Max(maxSize, 1);
            _clientSubscription = clientSubscription;
            revisedBufferTime = _bufferTime;
            revisedMaxSize = _maxSize;
            return Task.CompletedTask;
        }
    }

    private sealed class SubscriptionMgt2Impl : IOPCEventSubscriptionMgt2
    {
        private int _keepAlive = 1000;

        public Task<int> SetKeepAliveAsync(int keepAliveTime, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _keepAlive = Math.Max(keepAliveTime, 1000);
            return Task.FromResult(_keepAlive);
        }

        public Task<int> GetKeepAliveAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(_keepAlive);
        }
    }

    private sealed class AreaBrowserImpl : IOPCEventAreaBrowser
    {
        private string _position = string.Empty;

        public Task ChangeBrowsePositionAsync(int browseDirection, string? position, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = browseDirection;
            _position = position ?? string.Empty;
            return Task.CompletedTask;
        }

        public Task BrowseOPCAreasAsync(
            int browseFilterType,
            string filterCriteria,
            out IEnumString enumString,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = browseFilterType;
            _ = filterCriteria;
            enumString = default!;
            throw new OpcException(OpcResultId.NotImplemented);
        }

        public Task<string> GetQualifiedAreaNameAsync(string areaName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string qualified = string.IsNullOrEmpty(_position) ? areaName : _position + "." + areaName;
            return Task.FromResult(qualified);
        }

        public Task<string> GetQualifiedSourceNameAsync(string sourceName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string qualified = string.IsNullOrEmpty(_position) ? sourceName : _position + "." + sourceName;
            return Task.FromResult(qualified);
        }
    }
}

internal sealed class AeEventSink
{
    private readonly Channel<OpcEventNotification> _events = System.Threading.Channels.Channel.CreateUnbounded<OpcEventNotification>();
    private readonly IOPCEventSinkServerDispatcher _dispatcher;

    public AeEventSink()
    {
        _dispatcher = new IOPCEventSinkServerDispatcher(new SinkImpl(_events));
        Channel = new InMemoryCallChannel((interfaceId, opnum, payload, cancellationToken) =>
            interfaceId == IOPCEventSink.InterfaceId
                ? _dispatcher.DispatchAsync(opnum, payload, cancellationToken).ToCallResultAsync()
                : Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty)));
    }

    public InMemoryCallChannel Channel { get; }

    public async IAsyncEnumerable<OpcEventNotification> ReadAllAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await _events.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            while (_events.Reader.TryRead(out OpcEventNotification? item))
            {
                yield return item;
            }
        }
    }

    private sealed class SinkImpl : IOPCEventSink
    {
        private readonly Channel<OpcEventNotification> _events;

        public SinkImpl(Channel<OpcEventNotification> events) => _events = events;

        public Task OnEventAsync(
            int clientSubscription,
            bool refresh,
            bool lastRefresh,
            OpcEventNotification[] events,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = clientSubscription;
            _ = refresh;
            _ = lastRefresh;
            foreach (OpcEventNotification item in events)
            {
                _events.Writer.TryWrite(item);
            }

            return Task.CompletedTask;
        }
    }
}

internal sealed record HdaAnnotationWireResult(string ItemId, HdaAnnotation[] Annotations, int Error);

internal sealed record HdaBrowseWireElement(string Name, string ItemId, HdaBrowseType BrowseType);

internal sealed class HdaEndToEndPipeline
{
    private const int ReadAnnotationsOpnum = 4;
    private const int BrowseOpnum = 3;

    private readonly HdaServerImpl _serverImpl;
    private readonly OpcHdaServerDispatcher _serverDispatcher;
    private readonly IOPCHDA_SyncReadServerDispatcher _syncReadDispatcher;
    private readonly IOPCHDA_SyncAnnotationsServerDispatcher _syncAnnotationsDispatcher;

    public HdaEndToEndPipeline()
    {
        Store = new HistoricalDataStore();
        _serverImpl = new HdaServerImpl(Store, new SampleHdaServer(Store, NullLogger<SampleHdaServer>.Instance));
        _serverDispatcher = new OpcHdaServerDispatcher(_serverImpl);
        _syncReadDispatcher = new IOPCHDA_SyncReadServerDispatcher(new SyncReadImpl(_serverImpl));
        _syncAnnotationsDispatcher = new IOPCHDA_SyncAnnotationsServerDispatcher(new SyncAnnotationsImpl());
        Channel = new InMemoryCallChannel(DispatchAsync);
        Server = new IOPCHDA_ServerClientProxy(Channel);
        SyncRead = new IOPCHDA_SyncReadClientProxy(Channel);
        SyncAnnotations = new IOPCHDA_SyncAnnotationsClientProxy(Channel);
    }

    public HistoricalDataStore Store { get; }

    public InMemoryCallChannel Channel { get; }

    public IOPCHDA_ServerClientProxy Server { get; }

    public IOPCHDA_SyncReadClientProxy SyncRead { get; }

    public IOPCHDA_SyncAnnotationsClientProxy SyncAnnotations { get; }

    public HdaAggregate LastProcessedAggregate => _serverImpl.LastProcessedAggregate;

    public async Task<HdaAnnotationWireResult[]> ReadAnnotationsViaWireAsync(
        IReadOnlyList<string> itemIds,
        CancellationToken cancellationToken)
    {
        ReadOnlyMemory<byte> payload = EndToEndNdr.Write((ref NdrWriter writer) => EndToEndNdr.WriteStringArray(ref writer, itemIds));
        NdrCallResult result = await Channel.InvokeAsync(
            IOPCHDA_SyncAnnotations.InterfaceId,
            ReadAnnotationsOpnum,
            payload,
            cancellationToken).ConfigureAwait(false);
        ThrowIfFailure(result);

        var reader = new NdrReader(result.ResponsePayload.Span);
        int count = checked((int)reader.ReadUInt32());
        var annotations = new OpcHdaAnnotation[count];
        for (int i = 0; i < annotations.Length; i++)
        {
            annotations[i] = NdrOpcHdaAnnotationCodec.Read(ref reader);
        }

        int[] errors = EndToEndNdr.ReadInt32Array(ref reader);
        var results = new HdaAnnotationWireResult[count];
        for (int i = 0; i < results.Length; i++)
        {
            var hdaAnnotations = new HdaAnnotation[annotations[i].Annotations.Length];
            for (int j = 0; j < hdaAnnotations.Length; j++)
            {
                hdaAnnotations[j] = new HdaAnnotation
                {
                    Timestamp = annotations[i].Timestamps[j],
                    AnnotationTime = annotations[i].AnnotationTimes[j],
                    AnnotationText = annotations[i].Annotations[j] ?? string.Empty,
                    User = annotations[i].Users[j] ?? string.Empty,
                };
            }

            results[i] = new HdaAnnotationWireResult(itemIds[i], hdaAnnotations, errors[i]);
        }

        return results;
    }

    public async Task<HdaBrowseWireElement[]> BrowseViaWireAsync(
        string itemIdPrefix,
        HdaBrowseType browseType,
        CancellationToken cancellationToken)
    {
        ReadOnlyMemory<byte> payload = EndToEndNdr.Write((ref NdrWriter writer) =>
        {
            writer.WriteUnicodeStringPtr(itemIdPrefix);
            writer.WriteInt32((int)browseType);
        });
        NdrCallResult result = await Channel.InvokeAsync(
            IOPCHDA_Browser.InterfaceId,
            BrowseOpnum,
            payload,
            cancellationToken).ConfigureAwait(false);
        ThrowIfFailure(result);
        var reader = new NdrReader(result.ResponsePayload.Span);
        int count = checked((int)reader.ReadUInt32());
        var elements = new HdaBrowseWireElement[count];
        for (int i = 0; i < elements.Length; i++)
        {
            string name = reader.ReadUnicodeStringPtr() ?? string.Empty;
            string itemId = reader.ReadUnicodeStringPtr() ?? string.Empty;
            var type = (HdaBrowseType)reader.ReadInt32();
            elements[i] = new HdaBrowseWireElement(name, itemId, type);
        }

        return elements;
    }

    private Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (interfaceId == IOPCHDA_Server.InterfaceId)
        {
            return _serverDispatcher.DispatchAsync(interfaceId, opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCHDA_SyncRead.InterfaceId)
        {
            return _syncReadDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ToCallResultAsync();
        }

        if (interfaceId == IOPCHDA_SyncAnnotations.InterfaceId)
        {
            return opnum == ReadAnnotationsOpnum
                ? DispatchReadAnnotationsAsync(requestPayload, cancellationToken)
                : _syncAnnotationsDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ToCallResultAsync();
        }

        if (interfaceId == IOPCHDA_Browser.InterfaceId && opnum == BrowseOpnum)
        {
            return DispatchBrowseAsync(requestPayload, cancellationToken);
        }

        return Task.FromResult(NotImplemented());
    }

    private Task<NdrCallResult> DispatchReadAnnotationsAsync(ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var reader = new NdrReader(requestPayload.Span);
        string[] itemIds = EndToEndNdr.ReadStringArray(ref reader);
        var annotations = itemIds.Select(itemId => _serverImpl.GetAnnotations(itemId)).ToArray();
        var errors = itemIds.Select(_ => OpcResultId.Ok.Code).ToArray();
        ReadOnlyMemory<byte> response = EndToEndNdr.Write((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(unchecked((uint)annotations.Length));
            foreach (OpcHdaAnnotation annotation in annotations)
            {
                NdrOpcHdaAnnotationCodec.Write(ref writer, annotation);
            }

            EndToEndNdr.WriteInt32Array(ref writer, errors);
        });
        return Task.FromResult(Ok(response));
    }

    private Task<NdrCallResult> DispatchBrowseAsync(ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var reader = new NdrReader(requestPayload.Span);
        string prefix = reader.ReadUnicodeStringPtr() ?? string.Empty;
        var browseType = (HdaBrowseType)reader.ReadInt32();
        HdaBrowseWireElement[] elements = _serverImpl.Browse(prefix, browseType).ToArray();
        ReadOnlyMemory<byte> response = EndToEndNdr.Write((ref NdrWriter writer) =>
        {
            writer.WriteUInt32(unchecked((uint)elements.Length));
            foreach (HdaBrowseWireElement element in elements)
            {
                writer.WriteUnicodeStringPtr(element.Name);
                writer.WriteUnicodeStringPtr(element.ItemId);
                writer.WriteInt32((int)element.BrowseType);
            }
        });
        return Task.FromResult(Ok(response));
    }

    private static NdrCallResult Ok(ReadOnlyMemory<byte> payload) => new(OpcResultId.Ok.Code, payload);

    private static NdrCallResult NotImplemented() => new(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);

    private static void ThrowIfFailure(NdrCallResult result)
    {
        if (result.IsFailure)
        {
            throw new OpcException(new OpcResultId(result.Hresult, null));
        }
    }

    private sealed class HdaServerImpl : IOpcHdaServer
    {
        private readonly HistoricalDataStore _store;
        private readonly SampleHdaServer _sample;
        private readonly ConcurrentDictionary<int, HandleRegistration> _registrations = new();
        private int _nextServerHandle = 1100;

        public HdaServerImpl(HistoricalDataStore store, SampleHdaServer sample)
        {
            _store = store;
            _sample = sample;
        }

        public HdaAggregate LastProcessedAggregate { get; private set; } = HdaAggregate.None;

        public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) =>
            _sample.GetStatusAsync(cancellationToken);

        public Task<int[]> ValidateItemIdsAsync(string[] itemIds, CancellationToken cancellationToken = default) =>
            _sample.ValidateItemIdsAsync(itemIds, cancellationToken);

        public Task<int[]> GetItemHandlesAsync(string[] itemIds, int[] clientHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var handles = new int[itemIds.Length];
            for (int i = 0; i < itemIds.Length; i++)
            {
                if (!_store.Contains(itemIds[i]))
                {
                    continue;
                }

                int handle = Interlocked.Increment(ref _nextServerHandle);
                int clientHandle = i < clientHandles.Length ? clientHandles[i] : i + 1;
                _registrations[handle] = new HandleRegistration(itemIds[i], clientHandle);
                handles[i] = handle;
            }

            return Task.FromResult(handles);
        }

        public Task<int[]> ReleaseItemHandlesAsync(int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var results = new int[serverHandles.Length];
            for (int i = 0; i < serverHandles.Length; i++)
            {
                results[i] = _registrations.TryRemove(serverHandles[i], out _)
                    ? OpcResultId.Ok.Code
                    : OpcResultId.InvalidHandle.Code;
            }

            return Task.FromResult(results);
        }

        public Task<OpcHdaItem[]> ReadRawByHandleAsync(
            int[] serverHandles,
            OpcHdaTime startTime,
            OpcHdaTime endTime,
            int maxValues,
            CancellationToken cancellationToken)
        {
            string[] itemIds = serverHandles.Select(ResolveItemId).ToArray();
            return ReadAndReassignAsync(
                serverHandles,
                _sample.ReadRawAsync(itemIds, startTime, endTime, maxValues, cancellationToken));
        }

        public async Task<OpcHdaItem[]> ReadProcessedByHandleAsync(
            int[] serverHandles,
            int[] aggregateIds,
            OpcHdaTime startTime,
            OpcHdaTime endTime,
            TimeSpan resampleInterval,
            CancellationToken cancellationToken)
        {
            var items = new OpcHdaItem[serverHandles.Length];
            for (int i = 0; i < serverHandles.Length; i++)
            {
                string itemId = ResolveItemId(serverHandles[i]);
                var aggregate = i < aggregateIds.Length ? (HdaAggregate)aggregateIds[i] : HdaAggregate.Average;
                LastProcessedAggregate = aggregate;
                OpcHdaItem[] result = await _sample.ReadProcessedAsync(
                    [itemId],
                    startTime,
                    endTime,
                    resampleInterval,
                    aggregate,
                    cancellationToken).ConfigureAwait(false);
                items[i] = ReassignClientHandle(result[0], serverHandles[i]);
            }

            return items;
        }

        public OpcHdaAnnotation GetAnnotations(string itemId)
        {
            int clientHandle = _registrations.FirstOrDefault(pair =>
                string.Equals(pair.Value.ItemId, itemId, StringComparison.OrdinalIgnoreCase)).Value.ClientHandle;
            if (clientHandle == 0)
            {
                clientHandle = 1;
            }

            DateTimeOffset timestamp = _store.EndTime.AddMinutes(-30);
            return new OpcHdaAnnotation(
                clientHandle,
                [timestamp],
                [$"Calibration note for {itemId}"],
                [timestamp.AddMinutes(1)],
                ["operator.e2e"]);
        }

        public IEnumerable<HdaBrowseWireElement> Browse(string itemIdPrefix, HdaBrowseType browseType)
        {
            if (string.IsNullOrWhiteSpace(itemIdPrefix))
            {
                if (browseType is HdaBrowseType.Branch or HdaBrowseType.Flat)
                {
                    yield return new HdaBrowseWireElement("Sensor", "Sensor", HdaBrowseType.Branch);
                }

                if (browseType is HdaBrowseType.Leaf or HdaBrowseType.Flat)
                {
                    foreach (string itemId in _store.ItemIds.Order(StringComparer.OrdinalIgnoreCase))
                    {
                        yield return CreateLeaf(itemId);
                    }
                }

                yield break;
            }

            if (browseType != HdaBrowseType.Branch)
            {
                // Filter leaves to those that live under the requested
                // prefix (e.g. "Sensor" returns only "Sensor.*" leaves,
                // not the Random.* leaves at the same root level).
                string prefix = itemIdPrefix + ".";
                foreach (string itemId in _store.ItemIds.Order(StringComparer.OrdinalIgnoreCase))
                {
                    if (itemId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return CreateLeaf(itemId);
                    }
                }
            }
        }

        private async Task<OpcHdaItem[]> ReadAndReassignAsync(int[] serverHandles, Task<OpcHdaItem[]> readTask)
        {
            OpcHdaItem[] items = await readTask.ConfigureAwait(false);
            var correlated = new OpcHdaItem[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                correlated[i] = ReassignClientHandle(items[i], serverHandles[i]);
            }

            return correlated;
        }

        private OpcHdaItem ReassignClientHandle(OpcHdaItem item, int serverHandle)
        {
            int clientHandle = _registrations.TryGetValue(serverHandle, out HandleRegistration registration)
                ? registration.ClientHandle
                : item.ClientHandle;
            return new OpcHdaItem(clientHandle, item.AggregateHandle, item.Timestamps, item.Qualities, item.Values);
        }

        private string ResolveItemId(int serverHandle) =>
            _registrations.TryGetValue(serverHandle, out HandleRegistration registration) ? registration.ItemId : string.Empty;

        private static HdaBrowseWireElement CreateLeaf(string itemId) => new(
            itemId[(itemId.LastIndexOf('.') + 1)..],
            itemId,
            HdaBrowseType.Leaf);

        private readonly record struct HandleRegistration(string ItemId, int ClientHandle);
    }

    private sealed class SyncReadImpl : IOPCHDA_SyncRead
    {
        private readonly HdaServerImpl _server;

        public SyncReadImpl(HdaServerImpl server) => _server = server;

        public Task<OpcHdaItem[]> ReadRawAsync(
            OpcHdaTime startTime,
            OpcHdaTime endTime,
            int maxValues,
            bool bounds,
            int[] serverHandles,
            CancellationToken cancellationToken = default)
        {
            _ = bounds;
            return _server.ReadRawByHandleAsync(serverHandles, startTime, endTime, maxValues, cancellationToken);
        }

        public Task<OpcHdaItem[]> ReadProcessedAsync(
            OpcHdaTime startTime,
            OpcHdaTime endTime,
            long resampleIntervalFileTime,
            int[] serverHandles,
            int[] aggregateIds,
            CancellationToken cancellationToken = default)
        {
            TimeSpan interval = TimeSpan.FromTicks(Math.Abs(resampleIntervalFileTime));
            return _server.ReadProcessedByHandleAsync(serverHandles, aggregateIds, startTime, endTime, interval, cancellationToken);
        }

        public Task<OpcHdaItem[]> ReadAtTimeAsync(long[] timestampFileTimes, int[] serverHandles, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = timestampFileTimes;
            _ = serverHandles;
            return Task.FromResult(Array.Empty<OpcHdaItem>());
        }

        public Task<OpcHdaModifiedItem[]> ReadModifiedAsync(
            OpcHdaTime startTime,
            OpcHdaTime endTime,
            int maxValues,
            int[] serverHandles,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = maxValues;
            _ = serverHandles;
            return Task.FromResult(Array.Empty<OpcHdaModifiedItem>());
        }

        public Task<OpcHdaAttribute[]> ReadAttributeAsync(
            OpcHdaTime startTime,
            OpcHdaTime endTime,
            int serverHandle,
            int[] attributeIds,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = serverHandle;
            _ = attributeIds;
            return Task.FromResult(Array.Empty<OpcHdaAttribute>());
        }
    }

    private sealed class SyncAnnotationsImpl : IOPCHDA_SyncAnnotations
    {
        public Task<int> QueryCapabilitiesAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(1);
        }

        public Task<OpcHdaAnnotation[]> ReadAsync(
            OpcHdaTime startTime,
            OpcHdaTime endTime,
            int[] serverHandles,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = startTime;
            _ = endTime;
            _ = serverHandles;
            return Task.FromResult(Array.Empty<OpcHdaAnnotation>());
        }

        public Task<int[]> InsertAsync(
            int[] serverHandles,
            long[] timestampFileTimes,
            OpcHdaAnnotation[] annotationValues,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = timestampFileTimes;
            _ = annotationValues;
            return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
        }
    }
}

internal sealed record ObservedOrpcCall(
    Guid InterfaceId,
    int Opnum,
    OrpcThis OrpcThis,
    OrpcThat OrpcThat,
    byte[] RequestPayload,
    byte[] ResponsePayload,
    byte[]? Mic,
    byte[]? ChannelBindingToken);

internal sealed record FakeAuthContext(bool MicRequired, byte[]? Mic, byte[]? ChannelBindingToken);

internal sealed class OrpcTrackingInMemoryChannel
{
    private readonly InMemoryCallChannel _channel;
    private readonly Func<Guid, int, ReadOnlyMemory<byte>, CancellationToken, Task<NdrCallResult>> _serverHandler;
    private readonly FakeAuthContext? _authContext;
    private readonly ConcurrentQueue<ObservedOrpcCall> _calls = new();

    public OrpcTrackingInMemoryChannel(
        Func<Guid, int, ReadOnlyMemory<byte>, CancellationToken, Task<NdrCallResult>> serverHandler,
        FakeAuthContext? authContext = null)
    {
        _serverHandler = serverHandler;
        _authContext = authContext;
        _channel = new InMemoryCallChannel(InvokeWithOrpcAsync);
    }

    public InMemoryCallChannel Channel => _channel;

    public IReadOnlyList<ObservedOrpcCall> Calls => _calls.ToArray();

    private async Task<NdrCallResult> InvokeWithOrpcAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using IDisposable clientCausalityScope = CausalityContext.BeginCall();
        Guid causalityId = CausalityContext.Current.Value.GetValueOrDefault();

        ReadOnlyMemory<byte> requestFrame = EndToEndNdr.Write((ref NdrWriter writer) =>
        {
            new OrpcThis { CausalityId = causalityId }.Write(ref writer);
            writer.WriteRawBytes(requestPayload.Span);
        });

        var requestReader = new NdrReader(requestFrame.Span);
        OrpcThis orpcThis = OrpcThis.Read(ref requestReader);
        byte[] userRequest = requestFrame.Span[requestReader.Position..].ToArray();

        NdrCallResult serverResult;
        using (CausalityContext.BeginCall(orpcThis.CausalityId))
        {
            serverResult = await _serverHandler(interfaceId, opnum, userRequest, cancellationToken).ConfigureAwait(false);
        }

        ReadOnlyMemory<byte> responseFrame = EndToEndNdr.Write((ref NdrWriter writer) =>
        {
            new OrpcThat { Flags = 0x1u }.Write(ref writer);
            writer.WriteRawBytes(serverResult.ResponsePayload.Span);
        });

        var responseReader = new NdrReader(responseFrame.Span);
        OrpcThat orpcThat = OrpcThat.Read(ref responseReader);
        byte[] userResponse = responseFrame.Span[responseReader.Position..].ToArray();
        _calls.Enqueue(new ObservedOrpcCall(
            interfaceId,
            opnum,
            orpcThis,
            orpcThat,
            userRequest,
            userResponse,
            _authContext?.Mic,
            _authContext?.ChannelBindingToken));

        return new NdrCallResult(serverResult.Hresult, userResponse);
    }
}

internal sealed class DisposableCallChannel : ICallChannel, IDisposable
{
    private readonly InMemoryCallChannel _inner;
    private volatile bool _disposed;

    public DisposableCallChannel(InMemoryCallHandler handler) => _inner = new InMemoryCallChannel(handler);

    public void Dispose() => _disposed = true;

    public async Task<NdrCallResult> InvokeAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DisposableCallChannel), "The in-memory call channel has been disposed.");
        }

        NdrCallResult result = await _inner.InvokeAsync(interfaceId, opnum, requestPayload, cancellationToken).ConfigureAwait(false);
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(DisposableCallChannel), "The in-memory call channel was disposed while the call was in-flight.");
        }

        return result;
    }
}
