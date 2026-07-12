// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using System.Text.Json;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Hosting;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Tools;
using Opc.Classic.Testing;

namespace Opc.Classic.Mcp.Tests;

public sealed class DaClientToolsTests
{
    [Test]
    public async Task Da_tools_exercise_browse_group_items_sync_io_subscription_and_remove_via_mcp_client()
    {
        var syntheticDa = new SyntheticDaServer();
        string channelName = "da-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryDaConnectionRegistry.Register(channelName, syntheticDa.Channel);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);

        OpcSessionDto connected = await server.CallToolAsync<OpcSessionDto>(
            "opcclassic.da.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);
        OpcServerStatusDto status = await server.CallToolAsync<OpcServerStatusDto>(
            "opcclassic.da.get_status",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);
        OpcBrowseElementDto[] browse = await server.CallToolAsync<OpcBrowseElementDto[]>(
            "opcclassic.da.browse",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);
        OpcBrowseElementDto[] properties = await server.CallToolAsync<OpcBrowseElementDto[]>(
            "opcclassic.da.get_properties",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["itemIds"] = new[] { "Plant.Temperature" },
                ["propertyIds"] = new[] { 1, 2, 3 },
                ["returnValues"] = true,
            }).ConfigureAwait(false);
        OpcGroupStateDto group = await server.CallToolAsync<OpcGroupStateDto>(
            "opcclassic.da.add_group",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["name"] = "mcp-test-group",
                ["clientHandle"] = 500,
                ["updateRateMs"] = 100,
            }).ConfigureAwait(false);
        OpcResultDto[] addResults = await server.CallToolAsync<OpcResultDto[]>(
            "opcclassic.da.add_items",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["groupHandle"] = group.ServerGroupHandle,
                ["itemIds"] = new[] { "Plant.Temperature", "Bucket Brigade.Boolean" },
                ["clientHandles"] = new[] { 11, 12 },
            }).ConfigureAwait(false);
        int[] serverHandles = addResults.Select(static result => result.ServerHandle.GetValueOrDefault()).ToArray();
        OpcItemValueDto[] initialRead = await server.CallToolAsync<OpcItemValueDto[]>(
            "opcclassic.da.read_sync",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["groupHandle"] = group.ServerGroupHandle,
                ["serverHandles"] = serverHandles,
            }).ConfigureAwait(false);
        OpcResultDto[] writeResults = await server.CallToolAsync<OpcResultDto[]>(
            "opcclassic.da.write_sync",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["groupHandle"] = group.ServerGroupHandle,
                ["serverHandles"] = serverHandles,
                ["values"] = new object[] { 42.25, true },
            }).ConfigureAwait(false);
        OpcItemValueDto[] afterWrite = await server.CallToolAsync<OpcItemValueDto[]>(
            "opcclassic.da.read_sync",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["groupHandle"] = group.ServerGroupHandle,
                ["serverHandles"] = serverHandles,
            }).ConfigureAwait(false);
        OpcResultDto subscription = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.da.subscribe",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["groupHandle"] = group.ServerGroupHandle,
            }).ConfigureAwait(false);
        OpcItemValueDto[] polled = await server.CallToolAsync<OpcItemValueDto[]>(
            "opcclassic.da.poll_subscription",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["subscriptionId"] = subscription.SubscriptionId!,
            }).ConfigureAwait(false);
        OpcResultDto removed = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.da.remove_group",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["groupHandle"] = group.ServerGroupHandle,
            }).ConfigureAwait(false);

        await Assert.That(connected.DaConnected).IsTrue();
        await Assert.That(status.VendorInfo).IsEqualTo("Synthetic MCP DA Server");
        await Assert.That(browse.Select(static element => element.ItemName)).Contains("Plant.Temperature");
        await Assert.That(properties[0].Properties.Count).IsEqualTo(3);
        await Assert.That(addResults.All(static result => result.Succeeded)).IsTrue();
        await Assert.That(initialRead.Length).IsEqualTo(2);
        await Assert.That(writeResults.All(static result => result.Succeeded)).IsTrue();
        await Assert.That(GetDouble(afterWrite[0].Value)).IsEqualTo(42.25);
        await Assert.That(GetBoolean(afterWrite[1].Value)).IsTrue();
        await Assert.That(subscription.SubscriptionId).IsNotNull();
        await Assert.That(polled.Length).IsEqualTo(2);
        await Assert.That(removed.Succeeded).IsTrue();
        await Assert.That(syntheticDa.GroupCount).IsEqualTo(0);
    }

    [Test]
    public async Task Da_error_string_and_disconnect_round_trip_via_mcp_client()
    {
        var syntheticDa = new SyntheticDaServer();
        string channelName = "da-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryDaConnectionRegistry.Register(channelName, syntheticDa.Channel);
        await using McpTestServer server = await McpTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcSessionDto>(
            "opcclassic.da.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);

        OpcResultDto error = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.da.get_error_string",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["hresult"] = OpcResultId.BadRights.Code,
                ["localeId"] = 1033,
            }).ConfigureAwait(false);
        OpcResultDto disconnected = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.da.disconnect",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);

        await Assert.That(error.Message).Contains("0xC0040006");
        await Assert.That(error.Message).Contains("1033");
        await Assert.That(disconnected.Succeeded).IsTrue();
    }

    [Test]
    public async Task Modern_activation_mapping_selects_iopcserver_by_iid_when_results_are_reordered()
    {
        Guid optionalIid = IOPCCommon.InterfaceId;
        byte[] serverObjRef = { 0x4d, 0x45, 0x4f, 0x57, 0x01 };
        byte[] optionalObjRef = { 0x4d, 0x45, 0x4f, 0x57, 0x02 };
        ActivationInterfaceResult[] modernResults =
        [
            new(optionalIid, 0, optionalObjRef),
            new(IOPCServer.InterfaceId, 0, serverObjRef),
        ];

        var normalized = DaClientTools.DefaultOpcDaConnectionFactory.ToDaInterfaceResults(modernResults);
        var server = DaClientTools.DefaultOpcDaConnectionFactory.FindInterfaceResult(normalized, IOPCServer.InterfaceId);

        await Assert.That(server).IsNotNull();
        await Assert.That(Convert.ToHexString(server!.ObjRef.Span)).IsEqualTo(Convert.ToHexString(serverObjRef));
        await Assert.That(normalized[0].Iid).IsEqualTo(optionalIid);
        await Assert.That(normalized[1].Iid).IsEqualTo(IOPCServer.InterfaceId);
    }

    [Test]
    public async Task Modern_activation_mapping_handles_subset_without_positional_requested_iids()
    {
        ActivationInterfaceResult[] modernResults =
        [
            new(IOPCServer.InterfaceId, 0, [0x4d, 0x45, 0x4f, 0x57]),
        ];

        var normalized = DaClientTools.DefaultOpcDaConnectionFactory.ToDaInterfaceResults(modernResults);
        var browse = DaClientTools.DefaultOpcDaConnectionFactory.FindInterfaceResult(normalized, IOPCBrowse.InterfaceId);

        await Assert.That(normalized.Length).IsEqualTo(1);
        await Assert.That(normalized[0].Iid).IsEqualTo(IOPCServer.InterfaceId);
        await Assert.That(browse).IsNull();
    }

    [Test]
    public async Task ResolveOxid2_parser_reads_pointer_conformance_dsa_and_final_status()
    {
        var ipid = new Guid("11111111-2222-3333-4444-555555555555");
        byte[] payload = BuildResolveOxidResponse(ipid, includeComVersion: true);

        byte[] bindings = DaClientTools.DefaultOpcDaConnectionFactory.ReadResolveOxidBindings(
            payload,
            expectComVersion: true,
            out Guid actualIpid,
            out int hresult);

        await Assert.That(Convert.ToHexString(bindings)).IsEqualTo("0200010007000000");
        await Assert.That(actualIpid).IsEqualTo(ipid);
        await Assert.That(hresult).IsEqualTo(0);
    }

    private static double GetDouble(object? value) => ((JsonElement)value!).GetDouble();
    private static bool GetBoolean(object? value) => ((JsonElement)value!).GetBoolean();

    private static byte[] BuildResolveOxidResponse(Guid ipid, bool includeComVersion)
    {
        var buffer = new byte[64];
        var writer = new Opc.Classic.Ndr.NdrWriter(buffer);
        writer.WriteUInt32(0x00020000);
        writer.WriteUInt32(2);
        writer.WriteUInt16(2);
        writer.WriteUInt16(1);
        writer.WriteUInt16(0x07);
        writer.WriteUInt16(0);
        writer.AlignTo(4);
        writer.WriteGuid(ipid);
        writer.WriteUInt32(5);
        if (includeComVersion)
        {
            writer.WriteUInt16(5);
            writer.WriteUInt16(7);
        }

        writer.WriteInt32(0);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }
}

internal sealed class SyntheticDaServer : IOpcDaServer, IOPCBrowse, IOPCItemMgt, IOPCSyncIO, IOPCAsyncIO2
{
    private readonly object _gate = new();
    private readonly Dictionary<string, OpcVariant> _values = new(StringComparer.Ordinal)
    {
        ["Plant.Temperature"] = OpcVariant.FromDouble(21.5),
        ["Bucket Brigade.Boolean"] = OpcVariant.FromBoolean(false),
    };
    private readonly Dictionary<int, SyntheticGroup> _groups = new();
    private readonly OpcDaServerDispatcher _serverDispatcher;
    private readonly IOPCBrowseServerDispatcher _browseDispatcher;
    private readonly IOPCItemMgtServerDispatcher _itemMgtDispatcher;
    private readonly IOPCSyncIOServerDispatcher _syncIoDispatcher;
    private readonly IOPCAsyncIO2ServerDispatcher _asyncIoDispatcher;
    private int _nextGroupHandle = 1000;
    private int _nextItemHandle = 2000;
    private int _nextCancelId = 3000;
    private int _currentGroupHandle;
    private bool _asyncEnabled;

    public SyntheticDaServer()
    {
        _serverDispatcher = new OpcDaServerDispatcher(this);
        _browseDispatcher = new IOPCBrowseServerDispatcher(this);
        _itemMgtDispatcher = new IOPCItemMgtServerDispatcher(this);
        _syncIoDispatcher = new IOPCSyncIOServerDispatcher(this);
        _asyncIoDispatcher = new IOPCAsyncIO2ServerDispatcher(this);
        Channel = new InMemoryCallChannel(DispatchAsync);
    }

    public InMemoryCallChannel Channel { get; }

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

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = DateTimeOffset.UnixEpoch,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 2, 3),
            VendorInfo = "Synthetic MCP DA Server",
            GroupCount = GroupCount,
            BandWidth = 0,
        });
    }

    public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int handle = Interlocked.Increment(ref _nextGroupHandle);
        lock (_gate)
        {
            _groups[handle] = new SyntheticGroup(handle, name, active, requestedUpdateRate, clientHandle, localeId);
            _currentGroupHandle = handle;
        }

        return Task.FromResult(handle);
    }

    public Task AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientGroupHandle, int timeBias, float percentDeadband, int localeId, Guid requestedInterfaceId, out int serverGroupHandle, out int revisedUpdateRate, out IOpcInterfaceRef group, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        serverGroupHandle = Interlocked.Increment(ref _nextGroupHandle);
        revisedUpdateRate = requestedUpdateRate;
        group = CreateSyntheticInterfaceRef(requestedInterfaceId, serverGroupHandle);
        lock (_gate)
        {
            _groups[serverGroupHandle] = new SyntheticGroup(serverGroupHandle, name, active, requestedUpdateRate, clientGroupHandle, localeId)
            {
                TimeBias = timeBias,
                PercentDeadband = percentDeadband,
            };
            _currentGroupHandle = serverGroupHandle;
        }

        return Task.CompletedTask;
    }

    public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
    {
        _ = force;
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _groups.Remove(serverGroupHandle);
            if (_currentGroupHandle == serverGroupHandle)
            {
                _currentGroupHandle = 0;
            }
        }

        return Task.CompletedTask;
    }

    public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult($"Synthetic error 0x{errorCode:X8} locale={localeId}");
    }

    public Task<OpcItemProperties[]> GetPropertiesAsync(string[] itemIds, bool returnPropertyValues, int[] propertyIds, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int[] requested = propertyIds.Length == 0 ? [1, 2, 3] : propertyIds;
        return Task.FromResult(itemIds.Select(itemId => new OpcItemProperties(0, requested.Select(id => new OpcItemPropertyResult(
            VarType.VT_BSTR,
            id,
            ItemId: null,
            Description: "Property " + id.ToString(CultureInfo.InvariantCulture),
            returnPropertyValues ? OpcVariant.FromString(itemId + ":" + id.ToString(CultureInfo.InvariantCulture)) : OpcVariant.Empty,
            ErrorId: 0)).ToArray())).ToArray());
    }

    public Task BrowseAsync(string itemId, ref string? continuationPoint, int maxElementsReturned, int browseFilter, string elementNameFilter, string vendorFilter, bool returnAllProperties, bool returnPropertyValues, int[] propertyIds, out bool moreElements, out OpcBrowseElementResult[] browseElements, CancellationToken cancellationToken = default)
    {
        _ = itemId;
        _ = continuationPoint;
        _ = maxElementsReturned;
        _ = browseFilter;
        _ = elementNameFilter;
        _ = vendorFilter;
        _ = returnAllProperties;
        _ = returnPropertyValues;
        _ = propertyIds;
        cancellationToken.ThrowIfCancellationRequested();
        continuationPoint = null;
        moreElements = false;
        browseElements = _values.Keys
            .Order(StringComparer.Ordinal)
            .Select(static tag => new OpcBrowseElementResult(tag.Split('.')[^1], tag, FlagValue: 2, new OpcItemProperties(0, [])))
            .ToArray();
        return Task.CompletedTask;
    }

    public Task AddItemsAsync(OpcItemDef[] itemDefinitions, out OpcItemResult[] addResults, out int[] errors, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        addResults = new OpcItemResult[itemDefinitions.Length];
        errors = new int[itemDefinitions.Length];
        lock (_gate)
        {
            SyntheticGroup? group = null;
            bool hasGroup = _currentGroupHandle != 0 && _groups.TryGetValue(_currentGroupHandle, out group);
            for (int i = 0; i < itemDefinitions.Length; i++)
            {
                string itemId = itemDefinitions[i].ItemId ?? string.Empty;
                if (!hasGroup || group is null)
                {
                    addResults[i] = new OpcItemResult(0, VarType.VT_EMPTY, 0, []);
                    errors[i] = OpcResultId.InvalidHandle.Code;
                    continue;
                }

                if (!_values.TryGetValue(itemId, out OpcVariant value))
                {
                    addResults[i] = new OpcItemResult(0, VarType.VT_EMPTY, 0, []);
                    errors[i] = OpcResultId.UnknownItemId.Code;
                    continue;
                }

                int serverHandle = Interlocked.Increment(ref _nextItemHandle);
                group.Items[serverHandle] = new SyntheticItem(serverHandle, itemId, itemDefinitions[i].ClientHandle);
                addResults[i] = new OpcItemResult(serverHandle, value.Type, AccessRights: 3, []);
                errors[i] = OpcResultId.Ok.Code;
            }
        }

        return Task.CompletedTask;
    }

    public Task ValidateItemsAsync(OpcItemDef[] itemDefinitions, bool blobUpdate, out OpcItemResult[] validationResults, out int[] errors, CancellationToken cancellationToken = default)
    {
        _ = blobUpdate;
        cancellationToken.ThrowIfCancellationRequested();
        validationResults = itemDefinitions.Select(definition => new OpcItemResult(0, _values.TryGetValue(definition.ItemId ?? string.Empty, out OpcVariant value) ? value.Type : VarType.VT_EMPTY, 3, [])).ToArray();
        errors = itemDefinitions.Select(definition => _values.ContainsKey(definition.ItemId ?? string.Empty) ? OpcResultId.Ok.Code : OpcResultId.UnknownItemId.Code).ToArray();
        return Task.CompletedTask;
    }

    public Task<int[]> RemoveItemsAsync(int[] serverHandles, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            foreach (SyntheticGroup group in _groups.Values)
            {
                foreach (int handle in serverHandles)
                {
                    group.Items.Remove(handle);
                }
            }
        }

        return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
    }

    public Task<int[]> SetActiveStateAsync(int[] serverHandles, bool active, CancellationToken cancellationToken = default)
    {
        _ = active;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
    }

    public Task<int[]> SetClientHandlesAsync(int[] serverHandles, int[] clientHandles, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
    }

    public Task<int[]> SetDatatypesAsync(int[] serverHandles, ushort[] requestedDataTypes, CancellationToken cancellationToken = default)
    {
        _ = requestedDataTypes;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(serverHandles.Select(static _ => OpcResultId.Ok.Code).ToArray());
    }

    public Task<IOpcInterfaceRef> CreateEnumeratorAsync(Guid requestedInterfaceId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(CreateSyntheticInterfaceRef(requestedInterfaceId, _currentGroupHandle));
    }

    public Task<OpcItemState[]> ReadAsync(int dataSource, int[] serverHandles, out int[] errors, CancellationToken cancellationToken = default)
    {
        _ = dataSource;
        cancellationToken.ThrowIfCancellationRequested();
        OpcItemState[] states = new OpcItemState[serverHandles.Length];
        errors = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                if (TryFindItem(serverHandles[i], out SyntheticItem? item) && item is not null && _values.TryGetValue(item.ItemId, out OpcVariant value))
                {
                    states[i] = new OpcItemState(item.ClientHandle, DateTimeOffset.UtcNow, OpcQuality.Good, value);
                    errors[i] = OpcResultId.Ok.Code;
                }
                else
                {
                    states[i] = new OpcItemState(0, DateTimeOffset.UtcNow, OpcQuality.Bad, OpcVariant.Empty);
                    errors[i] = OpcResultId.InvalidHandle.Code;
                }
            }
        }

        return Task.FromResult(states);
    }

    public Task<int[]> WriteAsync(int[] serverHandles, OpcVariant[] values, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int[] errors = new int[serverHandles.Length];
        lock (_gate)
        {
            for (int i = 0; i < serverHandles.Length; i++)
            {
                if (TryFindItem(serverHandles[i], out SyntheticItem? item) && item is not null && i < values.Length)
                {
                    _values[item.ItemId] = values[i];
                    errors[i] = OpcResultId.Ok.Code;
                }
                else
                {
                    errors[i] = OpcResultId.InvalidHandle.Code;
                }
            }
        }

        return Task.FromResult(errors);
    }

    public Task<int> ReadAsync(int[] serverHandles, int transactionId, out int[] errors, CancellationToken cancellationToken = default)
    {
        _ = transactionId;
        _ = ReadAsync(1, serverHandles, out errors, cancellationToken);
        return Task.FromResult(Interlocked.Increment(ref _nextCancelId));
    }

    public Task<int> WriteAsync(int[] serverHandles, OpcVariant[] values, int transactionId, out int[] errors, CancellationToken cancellationToken = default)
    {
        _ = transactionId;
        errors = WriteAsync(serverHandles, values, cancellationToken).GetAwaiter().GetResult();
        return Task.FromResult(Interlocked.Increment(ref _nextCancelId));
    }

    public Task<int> Refresh2Async(int dataSource, int transactionId, CancellationToken cancellationToken = default)
    {
        _ = dataSource;
        _ = transactionId;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Interlocked.Increment(ref _nextCancelId));
    }

    public Task Cancel2Async(int cancelId, CancellationToken cancellationToken = default)
    {
        _ = cancelId;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public Task SetEnableAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _asyncEnabled = enabled;
        return Task.CompletedTask;
    }

    public Task<bool> GetEnableAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_asyncEnabled);
    }

    private Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken)
    {
        if (interfaceId == IOPCServer.InterfaceId)
        {
            return _serverDispatcher.DispatchAsync(interfaceId, opnum, requestPayload, cancellationToken);
        }

        if (interfaceId == IOPCBrowse.InterfaceId)
        {
            return ToCallResultAsync(_browseDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        if (interfaceId == IOPCItemMgt.InterfaceId)
        {
            return ToCallResultAsync(_itemMgtDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        if (interfaceId == IOPCSyncIO.InterfaceId)
        {
            return ToCallResultAsync(_syncIoDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        if (interfaceId == IOPCAsyncIO2.InterfaceId)
        {
            return ToCallResultAsync(_asyncIoDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken));
        }

        return Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
    }

    private bool TryFindItem(int serverHandle, out SyntheticItem? item)
    {
        foreach (SyntheticGroup group in _groups.Values)
        {
            if (group.Items.TryGetValue(serverHandle, out item))
            {
                return true;
            }
        }

        item = null;
        return false;
    }

    private static async Task<NdrCallResult> ToCallResultAsync(ValueTask<DispatchResult> dispatch) =>
        (await dispatch.ConfigureAwait(false)).ToNdrCallResult();

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

    private sealed class SyntheticGroup
    {
        public SyntheticGroup(int serverHandle, string name, bool active, int updateRate, int clientHandle, int localeId)
        {
            ServerHandle = serverHandle;
            Name = name;
            Active = active;
            UpdateRate = updateRate;
            ClientHandle = clientHandle;
            LocaleId = localeId;
        }

        public int ServerHandle { get; }
        public string Name { get; }
        public bool Active { get; }
        public int UpdateRate { get; }
        public int ClientHandle { get; }
        public int LocaleId { get; }
        public int TimeBias { get; init; }
        public float PercentDeadband { get; init; }
        public Dictionary<int, SyntheticItem> Items { get; } = new();
    }

    private sealed record SyntheticItem(int ServerHandle, string ItemId, int ClientHandle);
}
