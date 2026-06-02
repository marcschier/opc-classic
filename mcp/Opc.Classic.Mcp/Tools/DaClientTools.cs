//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using System.ComponentModel;
using System.Globalization;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Opc.Classic;
using Opc.Classic.Cpx.Dcom;
using Opc.Classic.Da;
using Opc.Classic.Discovery;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Remoting;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Ndr;
using Opc.Classic.Transport;

namespace Opc.Classic.Mcp.Tools;

/// <summary>Creates DA client state for a session.</summary>
public interface IOpcDaConnectionFactory
{
    /// <summary>Connects to a DA server and returns a client state object.</summary>
    Task<DaClientState> ConnectAsync(DaConnectionRequest request, CancellationToken cancellationToken = default);
}

/// <summary>Connection request used by DA tools.</summary>
public sealed record DaConnectionRequest(
    string Host,
    string? ProgId,
    string? Clsid,
    string? Username,
    string? Password,
    bool UseKerberos,
    string? ConnectionString,
    bool UseSso = false);

/// <summary>Registers in-memory DA call channels for MCP tests and loopback scenarios.</summary>
public static class InMemoryDaConnectionRegistry
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, ICallChannel> Channels = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers an in-memory DA call channel by name.</summary>
    public static IDisposable Register(string name, ICallChannel channel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(channel);

        Channels[name] = channel;
        return new Registration(name);
    }

    internal static bool TryGet(string name, out ICallChannel channel) => Channels.TryGetValue(name, out channel!);

    private sealed class Registration : IDisposable
    {
        private readonly string _name;
        private bool _disposed;

        public Registration(string name) => _name = name;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Channels.TryRemove(_name, out _);
        }
    }
}

/// <summary>MCP tools for OPC DA client operations.</summary>
public sealed class DaClientTools
{
    private static readonly IReadOnlyList<Guid> DaSessionPreBindIids = BuildDaSessionPreBindIids();

    private static readonly Guid[] GroupInterfaceIds =
    {
        IOPCItemMgt.InterfaceId,
        IOPCSyncIO.InterfaceId,
        IOPCSyncIO2.InterfaceId,
        IOPCAsyncIO2.InterfaceId,
        IOPCAsyncIO3.InterfaceId,
        IOPCGroupStateMgt.InterfaceId,
        IOPCGroupStateMgt2.InterfaceId,
        IConnectionPoint.InterfaceId,
        IConnectionPointContainer.InterfaceId,
    };

    private static IReadOnlyList<Guid> BuildDaSessionPreBindIids()
    {
        var iids = new List<Guid>(OpcSpecCatalog.Da.Count + 3);
        AddPreBindIid(iids, OpcSpecCatalog.Da);
        AddPreBindIid(iids, IOPCComplexDataItem.InterfaceId);
        AddPreBindIid(iids, IOPCComplexDataItem2.InterfaceId);
        AddPreBindIid(iids, IOPCTypeLibrary.InterfaceId);
        return iids;
    }

    private static void AddPreBindIid(List<Guid> iids, IReadOnlyList<Guid> values)
    {
        for (int i = 0; i < values.Count; i++)
        {
            AddPreBindIid(iids, values[i]);
        }
    }

    private static void AddPreBindIid(List<Guid> iids, Guid value)
    {
        if (value != Guid.Empty && !iids.Contains(value))
        {
            iids.Add(value);
        }
    }

    private readonly IOpcSessionManager _sessionManager;
    private readonly IOpcDaConnectionFactory _connectionFactory;

    /// <summary>Creates the DA client tool set.</summary>
    public DaClientTools(IOpcSessionManager sessionManager, IEnumerable<IOpcDaConnectionFactory> connectionFactories)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        ArgumentNullException.ThrowIfNull(connectionFactories);
        _connectionFactory = connectionFactories.FirstOrDefault() ?? new DefaultOpcDaConnectionFactory();
    }

    /// <summary>Connects a session to an OPC DA server.</summary>
    [McpServerTool(Name = "opcclassic.da.connect", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Connects an existing MCP session to an OPC DA server using DCOM or an in-memory test channel.")]
    public async Task<OpcSessionDto> Connect(
        [Description("The sessionId returned by opcclassic.session.create.")]
        string sessionId,
        [Description("OPC DA server host name or IP address. Ignored when connectionString uses inmemory://.")]
        string host = "localhost",
        [Description("OPC DA server ProgID, for example Matrikon.OPC.Simulation.1. Optional when clsid or connectionString is supplied.")]
        string? progId = null,
        [Description("OPC DA server CLSID as a GUID string. Optional when progId or connectionString is supplied.")]
        string? clsid = null,
        [Description("Optional user name for NTLMv2 or Kerberos authentication. Use DOMAIN\\user when a Windows domain is required.")]
        string? username = null,
        [Description("Optional password for NTLMv2 or Kerberos authentication. Omit only for anonymous or in-memory connections.")]
        string? password = null,
        [Description("True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied.")]
        bool useKerberos = false,
        [Description("Optional connection string. Use inmemory://name for a registered InMemoryCallChannel, or dcom://host/ProgID for DCOM.")]
        string? connectionString = null,
        [Description("True to authenticate using the current Windows logon via NegotiateAuthentication (no username/password needed). Windows-only.")]
        bool useSso = false,
        CancellationToken cancellationToken = default)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        DaClientState client = await _connectionFactory.ConnectAsync(
            new DaConnectionRequest(host, progId, clsid, username, password, useKerberos, connectionString, useSso),
            cancellationToken).ConfigureAwait(false);

        DaClientState? existing = session.DaClient;
        session.DaClient = client;
        if (existing is not null)
        {
            await existing.DisposeAsync().ConfigureAwait(false);
        }

        _ = await client.Server.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        session.Touch();
        return ToSessionDto(session);
    }

    /// <summary>Gets OPC DA server status for a connected session.</summary>
    [McpServerTool(Name = "opcclassic.da.get_status", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Gets the OPC DA server status, including runtime state, version, vendor information, and group count.")]
    public async Task<OpcServerStatusDto> GetStatus(
        [Description("The sessionId returned by opcclassic.session.create and connected with opcclassic.da.connect.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        DaClientState client = GetDaClient(sessionId);
        OpcServerStatus status = await client.Server.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        return ToDto(status);
    }

    /// <summary>Browses an OPC DA server address space.</summary>
    [McpServerTool(Name = "opcclassic.da.browse", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Browses the OPC DA address space below an item ID using DA 3.0 browse semantics.")]
    public async Task<IReadOnlyList<OpcBrowseElementDto>> Browse(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("The DA item ID to browse below. Use an empty string for the root.")]
        string itemId = "",
        [Description("Browse filter: all, branch, or leaf.")]
        string browseFilter = "all",
        [Description("Maximum elements per server browse call. Use 0 for the server default.")]
        int maxElements = 0,
        [Description("Optional element name filter, such as *Temp*.")]
        string elementNameFilter = "",
        [Description("Optional vendor-specific browse filter.")]
        string vendorFilter = "",
        [Description("Optional property IDs to include in each browse element.")]
        int[]? propertyIds = null,
        [Description("True to include property values when propertyIds are requested.")]
        bool returnPropertyValues = false,
        CancellationToken cancellationToken = default)
    {
        DaClientState client = GetDaClient(sessionId);
        string? continuationPoint = null;
        var elements = new List<OpcBrowseElementDto>();
        bool moreElements;
        do
        {
            await client.Browse.BrowseAsync(
                itemId ?? string.Empty,
                ref continuationPoint,
                maxElements,
                ParseBrowseFilter(browseFilter),
                elementNameFilter ?? string.Empty,
                vendorFilter ?? string.Empty,
                returnAllProperties: propertyIds is not null && propertyIds.Length > 0 && returnPropertyValues,
                returnPropertyValues,
                propertyIds ?? [],
                out moreElements,
                out OpcBrowseElementResult[] browseElements,
                cancellationToken).ConfigureAwait(false);

            elements.AddRange(browseElements.Select(ToBrowseElementDto));
        }
        while (moreElements && !string.IsNullOrEmpty(continuationPoint));

        return elements;
    }

    /// <summary>Gets OPC DA item properties.</summary>
    [McpServerTool(Name = "opcclassic.da.get_properties", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Gets OPC DA item properties for one or more item IDs.")]
    public async Task<IReadOnlyList<OpcBrowseElementDto>> GetProperties(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("One or more OPC DA item IDs whose properties should be queried.")]
        string[] itemIds,
        [Description("Optional property IDs to retrieve. Omit to let the server return its default property set.")]
        int[]? propertyIds = null,
        [Description("True to include property values; false to return only property metadata.")]
        bool returnValues = true,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        DaClientState client = GetDaClient(sessionId);
        OpcItemProperties[] properties = await client.Browse.GetPropertiesAsync(
            itemIds,
            returnValues,
            propertyIds ?? [],
            cancellationToken).ConfigureAwait(false);

        var results = new List<OpcBrowseElementDto>(itemIds.Length);
        for (int i = 0; i < itemIds.Length; i++)
        {
            OpcItemProperties itemProperties = i < properties.Length ? properties[i] : new OpcItemProperties(OpcResultId.Fail.Code, []);
            results.Add(new OpcBrowseElementDto(
                itemIds[i],
                itemIds[i],
                ItemPath: null,
                IsItem: true,
                HasChildren: false,
                ToPropertyDtos(itemProperties)));
        }

        return results;
    }

    /// <summary>Creates an OPC DA group.</summary>
    [McpServerTool(Name = "opcclassic.da.add_group", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Creates an OPC DA server-side group used for item add, synchronous I/O, and subscriptions.")]
    public async Task<OpcGroupStateDto> AddGroup(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Unique group name. If omitted or empty, the server may assign a name.")]
        string name = "mcp-da-group",
        [Description("True to make the group active immediately.")]
        bool active = true,
        [Description("Requested group update rate in milliseconds.")]
        int updateRateMs = 1000,
        [Description("Client-supplied group handle echoed by callbacks.")]
        int clientHandle = 1,
        [Description("Time bias in minutes from UTC.")]
        int timeBiasMinutes = 0,
        [Description("Deadband percentage, 0 to 100.")]
        float deadbandPercent = 0,
        [Description("Locale ID for server messages, such as 1033 for en-US. Use 0 for server default.")]
        int localeId = 0,
        [Description("DA 3.0 keep-alive interval in milliseconds. Use 0 to leave disabled.")]
        int keepAliveMs = 0,
        CancellationToken cancellationToken = default)
    {
        DaClientState client = GetDaClient(sessionId);
        await client.Server.AddGroupAsync(
            string.IsNullOrWhiteSpace(name) ? "mcp-da-group" : name,
            active,
            updateRateMs,
            clientHandle,
            timeBiasMinutes,
            deadbandPercent,
            localeId,
            IOPCItemMgt.InterfaceId,
            out int serverGroupHandle,
            out int revisedUpdateRate,
            out IOpcInterfaceRef groupRef,
            cancellationToken).ConfigureAwait(false);
        IReadOnlyDictionary<Guid, Guid> groupInterfaceIpids = await ResolveGroupInterfaceIpidsAsync(
            client,
            groupRef,
            cancellationToken).ConfigureAwait(false);

        if (keepAliveMs > 0)
        {
            keepAliveMs = await client.GroupState2.SetKeepAliveAsync(keepAliveMs, cancellationToken).ConfigureAwait(false);
        }

        var group = new DaGroupContext(
            serverGroupHandle,
            name,
            clientHandle,
            active,
            updateRateMs,
            revisedUpdateRate,
            timeBiasMinutes,
            deadbandPercent,
            localeId,
            keepAliveMs,
            groupInterfaceIpids);
        client.Groups[serverGroupHandle] = group;
        return ToGroupDto(group);
    }

    /// <summary>Adds OPC DA items to a group.</summary>
    [McpServerTool(Name = "opcclassic.da.add_items", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Adds item IDs to an OPC DA group and returns per-item server handles and HRESULTs.")]
    public async Task<IReadOnlyList<OpcResultDto>> AddItems(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Server group handle returned by opcclassic.da.add_group.")]
        int groupHandle,
        [Description("OPC DA item IDs to add to the group.")]
        string[] itemIds,
        [Description("Optional client handles aligned with itemIds. Defaults to 1-based handles.")]
        int[]? clientHandles = null,
        [Description("True to make the items active immediately.")]
        bool active = true,
        [Description("Requested VARTYPE numeric code. Use 0 (VT_EMPTY) for the server canonical type.")]
        ushort requestedVarType = 0,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        DaClientState client = GetDaClient(sessionId);
        DaGroupContext group = GetGroup(client, groupHandle);
        ApplyGroupInterfaceRoutes(client, group);
        OpcItemDef[] definitions = new OpcItemDef[itemIds.Length];
        for (int i = 0; i < definitions.Length; i++)
        {
            int clientHandle = clientHandles is not null && i < clientHandles.Length ? clientHandles[i] : i + 1;
            definitions[i] = new OpcItemDef(null, itemIds[i], active, clientHandle, Array.Empty<byte>(), (VarType)requestedVarType);
        }

        await client.ItemMgt.AddItemsAsync(definitions, out OpcItemResult[] addResults, out int[] errors, cancellationToken).ConfigureAwait(false);
        var results = new List<OpcResultDto>(definitions.Length);
        for (int i = 0; i < definitions.Length; i++)
        {
            OpcItemResult itemResult = i < addResults.Length ? addResults[i] : new OpcItemResult(0, VarType.VT_EMPTY, 0, []);
            int error = i < errors.Length ? errors[i] : OpcResultId.Fail.Code;
            string itemName = itemIds[i];
            int clientHandle = definitions[i].ClientHandle;
            if (error >= 0 && itemResult.ServerHandle != 0)
            {
                group.Items[itemResult.ServerHandle] = new DaItemBindingContext(itemName, null, clientHandle, itemResult.ServerHandle);
            }

            results.Add(ToResult(error, itemName, clientHandle, itemResult.ServerHandle, itemResult.CanonicalDataType.ToString(), itemResult.AccessRights));
        }

        return results;
    }

    /// <summary>Synchronously reads OPC DA item values by item ID via IOPCItemIO (stateless, no group required).</summary>
    [McpServerTool(Name = "opcclassic.da.read_items_by_id", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Reads OPC DA item values by item ID using the DA 3.0 stateless IOPCItemIO interface — no AddGroup/AddItems required. Recommended for quick reads against well-known item IDs.")]
    public async Task<IReadOnlyList<OpcItemValueDto>> ReadItemsById(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("OPC DA item IDs to read (e.g. \"Random.Int1\", \"Random.Real8\").")]
        string[] itemIds,
        [Description("Per-item max-age in milliseconds. 0 = no cache constraint (server returns whatever is fresh).")]
        int[]? maxAges = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        DaClientState client = GetDaClient(sessionId);

        int[] effectiveMaxAges;
        if (maxAges is null || maxAges.Length == 0)
        {
            effectiveMaxAges = new int[itemIds.Length];
        }
        else if (maxAges.Length == itemIds.Length)
        {
            effectiveMaxAges = maxAges;
        }
        else
        {
            throw new ArgumentException("maxAges must be empty or have the same length as itemIds.", nameof(maxAges));
        }

        await client.ItemIo.ReadAsync(
            itemIds,
            effectiveMaxAges,
            out OpcVariant[] values,
            out ushort[] qualities,
            out long[] timestamps,
            out int[] errors,
            cancellationToken).ConfigureAwait(false);

        var results = new List<OpcItemValueDto>(itemIds.Length);
        for (int i = 0; i < itemIds.Length; i++)
        {
            OpcVariant value = i < values.Length ? values[i] : default;
            ushort quality = i < qualities.Length ? qualities[i] : (ushort)0;
            long timestamp = i < timestamps.Length ? timestamps[i] : 0L;
            int error = i < errors.Length ? errors[i] : OpcResultId.Fail.Code;
            var state = new OpcItemState(
                ClientHandle: i + 1,
                Timestamp: timestamp == 0L ? DateTimeOffset.UnixEpoch : DateTimeOffset.FromFileTime(timestamp),
                Quality: new OpcQuality(quality),
                Value: value);
            results.Add(ToValueDto(itemIds[i], itemPath: null, serverHandle: null, state, error));
        }
        return results;
    }

    /// <summary>Synchronously reads OPC DA item values by group server handle.</summary>
    [McpServerTool(Name = "opcclassic.da.read_sync", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Synchronously reads item values from an OPC DA group by server handles.")]
    public async Task<IReadOnlyList<OpcItemValueDto>> ReadSync(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Server group handle returned by opcclassic.da.add_group.")]
        int groupHandle,
        [Description("Optional item server handles returned by opcclassic.da.add_items. Omit or pass an empty array to read all known group items.")]
        int[]? serverHandles = null,
        [Description("True to read from the server cache; false to read from the underlying device.")]
        bool fromCache = true,
        CancellationToken cancellationToken = default)
    {
        DaClientState client = GetDaClient(sessionId);
        DaGroupContext group = GetGroup(client, groupHandle);
        ApplyGroupInterfaceRoutes(client, group);
        int[] handles;
        if (serverHandles is null || serverHandles.Length == 0)
        {
            handles = group.Items.Keys.Order().ToArray();
        }
        else
        {
            handles = serverHandles.ToArray();
        }

        OpcItemState[] states = await client.SyncIo.ReadAsync(fromCache ? 1 : 2, handles, out int[] errors, cancellationToken).ConfigureAwait(false);
        return ToValueDtos(group, handles, states, errors);
    }

    /// <summary>Synchronously writes OPC DA item values by group server handle.</summary>
    [McpServerTool(Name = "opcclassic.da.write_sync", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Synchronously writes values to OPC DA group items by server handles.")]
    public async Task<IReadOnlyList<OpcResultDto>> WriteSync(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Server group handle returned by opcclassic.da.add_group.")]
        int groupHandle,
        [Description("Item server handles returned by opcclassic.da.add_items.")]
        int[] serverHandles,
        [Description("JSON values to write, aligned with serverHandles. Supported values: null, bool, number, string, DateTime string, or GUID string.")]
        JsonElement[] values,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(values);
        if (serverHandles.Length != values.Length)
        {
            throw new ArgumentException("serverHandles and values must have the same length.", nameof(values));
        }

        DaClientState client = GetDaClient(sessionId);
        DaGroupContext group = GetGroup(client, groupHandle);
        ApplyGroupInterfaceRoutes(client, group);
        OpcVariant[] variants = values.Select(ToVariant).ToArray();
        int[] errors = await client.SyncIo.WriteAsync(serverHandles, variants, cancellationToken).ConfigureAwait(false);
        var results = new List<OpcResultDto>(serverHandles.Length);
        for (int i = 0; i < serverHandles.Length; i++)
        {
            DaItemBindingContext? binding = group.Items.GetValueOrDefault(serverHandles[i]);
            results.Add(ToResult(
                i < errors.Length ? errors[i] : OpcResultId.Fail.Code,
                binding?.ItemName ?? $"#{serverHandles[i]}",
                binding?.ClientHandle,
                serverHandles[i],
                variants[i].Type.ToString(),
                AccessRights: null));
        }

        return results;
    }

    /// <summary>Starts a poll-based OPC DA subscription for a group.</summary>
    [McpServerTool(Name = "opcclassic.da.subscribe", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Starts a poll-based OPC DA subscription for a group. MCP cannot push callbacks, so use opcclassic.da.poll_subscription to retrieve values.")]
    public async Task<OpcResultDto> Subscribe(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Server group handle returned by opcclassic.da.add_group.")]
        int groupHandle,
        [Description("True to refresh/read from the server cache; false to use device reads where supported.")]
        bool fromCache = true,
        CancellationToken cancellationToken = default)
    {
        DaClientState client = GetDaClient(sessionId);
        DaGroupContext group = GetGroup(client, groupHandle);
        ApplyGroupInterfaceRoutes(client, group);
        string subscriptionId = Guid.NewGuid().ToString("N");
        int transactionId = Environment.TickCount & int.MaxValue;
        int? cancelId = null;
        await client.AsyncIo2.SetEnableAsync(true, cancellationToken).ConfigureAwait(false);
        try
        {
            cancelId = await client.AsyncIo2.Refresh2Async(fromCache ? 1 : 2, transactionId, cancellationToken).ConfigureAwait(false);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            cancelId = null;
        }

        client.Subscriptions[subscriptionId] = new DaSubscriptionContext(subscriptionId, groupHandle, fromCache, transactionId, cancelId);
        return new OpcResultDto(0, $"Subscription '{subscriptionId}' created. Poll for values with opcclassic.da.poll_subscription.", Succeeded: true, SubscriptionId: subscriptionId, TransactionId: transactionId, CancelId: cancelId);
    }

    /// <summary>Polls a DA subscription queue for new notifications.</summary>
    [McpServerTool(Name = "opcclassic.da.poll_subscription", ReadOnly = true, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Polls a DA subscription for values. The initial implementation uses a pull model and returns current values for known group items.")]
    public async Task<IReadOnlyList<OpcItemValueDto>> PollSubscription(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Subscription identifier returned by opcclassic.da.subscribe.")]
        string subscriptionId,
        [Description("Maximum item values to return. Use 0 for all currently known group items.")]
        int maxNotifications = 0,
        CancellationToken cancellationToken = default)
    {
        DaClientState client = GetDaClient(sessionId);
        if (!client.Subscriptions.TryGetValue(subscriptionId, out DaSubscriptionContext? subscription))
        {
            throw new McpException($"DA subscription '{subscriptionId}' was not found.");
        }

        DaGroupContext group = GetGroup(client, subscription.GroupHandle);
        ApplyGroupInterfaceRoutes(client, group);
        int[] handles = NormalizeHandles(null, group);
        if (maxNotifications > 0)
        {
            handles = handles.Take(maxNotifications).ToArray();
        }

        OpcItemState[] states = await client.SyncIo.ReadAsync(subscription.FromCache ? 1 : 2, handles, out int[] errors, cancellationToken).ConfigureAwait(false);
        return ToValueDtos(group, handles, states, errors);
    }

    /// <summary>Removes an OPC DA group.</summary>
    [McpServerTool(Name = "opcclassic.da.remove_group", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Removes an OPC DA server-side group and forgets its item handles and poll subscriptions.")]
    public async Task<OpcResultDto> RemoveGroup(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Server group handle returned by opcclassic.da.add_group.")]
        int groupHandle,
        [Description("True to force removal even if callbacks or operations are active.")]
        bool force = true,
        CancellationToken cancellationToken = default)
    {
        DaClientState client = GetDaClient(sessionId);
        await client.Server.RemoveGroupAsync(groupHandle, force, cancellationToken).ConfigureAwait(false);
        client.Groups.TryRemove(groupHandle, out _);
        foreach (KeyValuePair<string, DaSubscriptionContext> pair in client.Subscriptions)
        {
            if (pair.Value.GroupHandle == groupHandle)
            {
                client.Subscriptions.TryRemove(pair.Key, out _);
            }
        }

        return new OpcResultDto(0, $"Group {groupHandle} removed.", Succeeded: true, ServerHandle: groupHandle);
    }

    /// <summary>Translates an HRESULT to an OPC DA server-localized message.</summary>
    [McpServerTool(Name = "opcclassic.da.get_error_string", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Translates an OPC HRESULT to a server-localized message using IOPCServer::GetErrorString.")]
    public async Task<OpcResultDto> GetErrorString(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("HRESULT as a signed 32-bit integer, for example -1073479674 for 0xC0040006.")]
        int hresult,
        [Description("Locale ID for the returned message, such as 1033 for en-US.")]
        int localeId = 0,
        CancellationToken cancellationToken = default)
    {
        DaClientState client = GetDaClient(sessionId);
        string message = await client.Server.GetErrorStringAsync(hresult, localeId, cancellationToken).ConfigureAwait(false);
        return new OpcResultDto(hresult, message, new OpcResultId(hresult, null).IsSuccess);
    }

    /// <summary>Disconnects a session from its OPC DA server.</summary>
    [McpServerTool(Name = "opcclassic.da.disconnect", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Disconnects the session from its OPC DA server and releases the DA channel.")]
    public async Task<OpcResultDto> Disconnect(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        DaClientState? client = session.DaClient;
        session.DaClient = null;
        if (client is not null)
        {
            await client.DisposeAsync().ConfigureAwait(false);
            return new OpcResultDto(0, "DA client disconnected.", Succeeded: true);
        }

        return new OpcResultDto(1, "DA client was not connected.", Succeeded: false);
    }

    private DaClientState GetDaClient(string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        return session.DaClient ?? throw new McpException($"Session '{sessionId}' is not connected to an OPC DA server. Call opcclassic.da.connect first.");
    }

    private static DaGroupContext GetGroup(DaClientState client, int groupHandle) =>
        client.Groups.TryGetValue(groupHandle, out DaGroupContext? group)
            ? group
            : throw new McpException($"DA group handle {groupHandle} was not found in this session.");

    private static async Task<IReadOnlyDictionary<Guid, Guid>> ResolveGroupInterfaceIpidsAsync(
        DaClientState client,
        IOpcInterfaceRef groupRef,
        CancellationToken cancellationToken)
    {
        var routes = new Dictionary<Guid, Guid>();
        if (client.CallChannel is not DcomCallChannel channel || groupRef.Ipid.Equals(Guid.Empty))
        {
            return routes;
        }

        routes[groupRef.Iid] = groupRef.Ipid;
        Guid[] queryIids = GroupInterfaceIds.Where(static iid => iid != Guid.Empty).ToArray();
        if (queryIids.Length > 0)
        {
            var remUnknown = new IRemUnknownClientProxy(client.CallChannel);
            OpcRemQIResult[] results = await remUnknown.RemQueryInterfaceAsync(
                groupRef.Ipid,
                cRefs: 5,
                cIids: checked((ushort)queryIids.Length),
                queryIids,
                cancellationToken).ConfigureAwait(false);

            for (int i = 0; i < results.Length && i < queryIids.Length; i++)
            {
                OpcRemQIResult result = results[i];
                if (result.Hresult == 0 && !result.Ipid.Equals(Guid.Empty))
                {
                    routes[queryIids[i]] = result.Ipid;
                }
            }
        }

        RegisterInterfaceRoutes(channel, routes);
        return routes;
    }

    private static void ApplyGroupInterfaceRoutes(DaClientState client, DaGroupContext group)
    {
        if (client.CallChannel is DcomCallChannel channel)
        {
            RegisterInterfaceRoutes(channel, group.InterfaceIpids);
        }
    }

    private static void RegisterInterfaceRoutes(DcomCallChannel channel, IReadOnlyDictionary<Guid, Guid> routes)
    {
        foreach (KeyValuePair<Guid, Guid> route in routes)
        {
            channel.RegisterInterfaceIpid(route.Key, route.Value);
        }
    }

    private static int[] NormalizeHandles(int[]? serverHandles, DaGroupContext group)
    {
        if (serverHandles is { Length: > 0 })
        {
            return serverHandles;
        }

        return group.Items.Keys.Order().ToArray();
    }

    private static IReadOnlyList<OpcItemValueDto> ToValueDtos(DaGroupContext group, IReadOnlyList<int> handles, IReadOnlyList<OpcItemState> states, IReadOnlyList<int> errors)
    {
        var results = new List<OpcItemValueDto>(handles.Count);
        for (int i = 0; i < handles.Count; i++)
        {
            OpcItemState state = i < states.Count ? states[i] : new OpcItemState(0, DateTimeOffset.UtcNow, OpcQuality.Bad, OpcVariant.Empty);
            int error = i < errors.Count ? errors[i] : OpcResultId.Fail.Code;
            DaItemBindingContext? binding = group.Items.GetValueOrDefault(handles[i]);
            results.Add(ToValueDto(binding?.ItemName ?? $"#{handles[i]}", binding?.ItemPath, binding?.ServerHandle ?? handles[i], state, error));
        }

        return results;
    }

    private static OpcItemValueDto ToValueDto(string itemName, string? itemPath, int? serverHandle, OpcItemState state, int hresult)
    {
        object? value = OpcVariantConverter.ToObject(state.Value);
        return new OpcItemValueDto(
            itemName,
            itemPath,
            state.ClientHandle,
            serverHandle,
            NormalizeValue(value),
            state.Value.Type.ToString(),
            state.Quality.RawValue,
            state.Quality.ToString(),
            state.Timestamp,
            hresult,
            DescribeHResult(hresult));
    }

    private static OpcServerStatusDto ToDto(OpcServerStatus status) =>
        new(
            status.Spec.ToString(),
            status.StartTime,
            status.CurrentTime,
            status.LastUpdateTime,
            status.State.ToString(),
            status.ServerVersion.ToString(),
            status.VendorInfo,
            status.GroupCount,
            status.BandWidth,
            status.MaxReturnValues,
            status.IsOperational);

    private static OpcGroupStateDto ToGroupDto(DaGroupContext group) =>
        new(
            group.ServerGroupHandle,
            group.ClientHandle,
            group.Name,
            group.Active,
            group.UpdateRateMs,
            group.RevisedUpdateRateMs,
            group.TimeBiasMinutes,
            group.DeadbandPercent,
            group.LocaleId,
            group.KeepAliveMs,
            group.Items.Count);

    private static OpcBrowseElementDto ToBrowseElementDto(OpcBrowseElementResult element) =>
        new(
            element.Name ?? string.Empty,
            element.ItemId ?? string.Empty,
            ItemPath: null,
            element.IsItem,
            element.IsBranch,
            ToPropertyDtos(element.Properties));

    private static IReadOnlyList<OpcItemPropertyDto> ToPropertyDtos(OpcItemProperties properties) =>
        properties.Properties.Select(static property => new OpcItemPropertyDto(
            property.PropertyId,
            Name: null,
            property.Description ?? string.Empty,
            property.DataType.ToString(),
            NormalizeValue(OpcVariantConverter.ToObject(property.Value)),
            property.ErrorId,
            DescribeHResult(property.ErrorId),
            property.ItemId,
            ItemPath: null)).ToArray();

    private static OpcResultDto ToResult(int hresult, string? itemName, int? clientHandle, int? serverHandle, string? valueType, int? AccessRights) =>
        new(
            hresult,
            DescribeHResult(hresult),
            new OpcResultId(hresult, null).IsSuccess,
            itemName,
            clientHandle,
            serverHandle,
            valueType,
            AccessRights);

    private static int ParseBrowseFilter(string browseFilter) =>
        browseFilter?.Trim().ToLowerInvariant() switch
        {
            "branch" or "branches" => (int)BrowseFilters.Branch,
            "leaf" or "leaves" or "item" or "items" => (int)BrowseFilters.Leaf,
            _ => (int)BrowseFilters.All,
        };

    private static OpcVariant ToVariant(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => OpcVariant.Null,
        JsonValueKind.True => OpcVariant.FromBoolean(true),
        JsonValueKind.False => OpcVariant.FromBoolean(false),
        JsonValueKind.Number when value.TryGetInt32(out int int32) => OpcVariant.FromInt32(int32),
        JsonValueKind.Number when value.TryGetInt64(out long int64) => OpcVariant.FromInt64(int64),
        JsonValueKind.Number when value.TryGetDouble(out double dbl) => OpcVariant.FromDouble(dbl),
        JsonValueKind.String => StringToVariant(value.GetString()),
        _ => OpcVariant.FromString(value.GetRawText()),
    };

    private static OpcVariant StringToVariant(string? value)
    {
        if (value is null)
        {
            return OpcVariant.Null;
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime dateTime))
        {
            return OpcVariant.FromDate(dateTime);
        }

        if (Guid.TryParse(value, out Guid guid))
        {
            return OpcVariant.FromClsid(guid);
        }

        return OpcVariant.FromString(value);
    }

    private static object? NormalizeValue(object? value) => value switch
    {
        DateTime dateTime => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
        OpcVariant variant => NormalizeValue(OpcVariantConverter.ToObject(variant)),
        OpcSafeArray safeArray => safeArray.ToString(),
        _ => value,
    };

    private static string DescribeHResult(int hresult) => hresult switch
    {
        0 => "S_OK",
        1 => "S_FALSE",
        _ => new OpcResultId(hresult, null).ToString(),
    };

    private static OpcSessionDto ToSessionDto(OpcSession session)
    {
        DaClientState? da = session.DaClient;
        return new OpcSessionDto(
            session.SessionId,
            session.CreatedAt,
            session.LastUsedAt,
            session.LastUsedAt.Add(session.IdleExpiry),
            checked((int)Math.Ceiling(session.IdleExpiry.TotalSeconds)),
            da is not null,
            da?.Host,
            da?.ProgId,
            da?.Clsid);
    }

    private sealed class DefaultOpcDaConnectionFactory : IOpcDaConnectionFactory
    {
        private const int EndpointMapperPort = 135;
        private const int RemoteCreateInstanceOpnum = 4;
        private const int ClassContext = 0x14;
        private const int RpcProtocolSequenceTcp = 7;
        private const int DefaultPayloadSize = 4096;
        private const int MaximumPayloadSize = 65536;
        private const uint ObjRefSignature = 0x574F454D;
        private const ushort TcpTowerId = 0x07;
        private static readonly Guid RemoteScmActivatorInterfaceId = new("000001A0-0000-0000-C000-000000000046");

        public async Task<DaClientState> ConnectAsync(DaConnectionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            DaConnectionRequest normalized = NormalizeRequest(request);
            if (TryCreateInMemoryClient(normalized, out DaClientState? inMemoryClient))
            {
                return inMemoryClient ?? throw new InvalidOperationException("In-memory connection factory returned no client.");
            }

            Guid clsid = await ResolveClsidAsync(normalized, cancellationToken).ConfigureAwait(false);
            var channelFactory = new DcomCallChannelFactory(new TcpSocketTransportFactory());

            // Use the legacy IActivation::RemoteActivation (opnum 0) path rather than the
            // newer IRemoteSCMActivator::RemoteCreateInstance (opnum 4). The former is the
            // only activation path whose wire format the in-repo encoder produces correctly;
            // the modern SCM activator's CustomREMOTE_TYPED_HEADER + ActivationPropertiesIn
            // marshaling per MS-DCOM §2.2.22 is not yet implemented here. Modern Windows
            // (Win10/Win11/Server 2019+) still services the legacy interface for backwards
            // compatibility, so this path works end-to-end against real OPC DCOM servers
            // including Matrikon, Kepware, and OPC Foundation reference implementations.
            ActivationClient? activationClient = null;
            try
            {
                IAuthContext authContext = CreateAuthContext(normalized, clsid);
                activationClient = await ActivationClient.ConnectTcpAsync(normalized.Host, authContext, cancellationToken).ConfigureAwait(false);

                // Request all the IIDs the DA proxy chain may need. Activation gives us a
                // distinct OBJREF (with its own IPID) for each that the target object
                // implements; we register each IPID with the channel so per-call routing
                // sends to the right interface pointer. Servers that don't implement an
                // optional interface return E_NOINTERFACE for that IID's slot and we just
                // skip it (operations on that interface will fail at call time).
                Guid[] requestedIids =
                {
                    IOPCServer.InterfaceId,
                    IOPCCommon.InterfaceId,
                    IOPCBrowse.InterfaceId,
                    IOPCBrowseServerAddressSpace.InterfaceId,
                    IOPCItemProperties.InterfaceId,
                    IOPCItemIO.InterfaceId,
                };
                Opc.Classic.Dcom.Activation.RemoteActivationResponse activation = await activationClient.RemoteActivationAsync(
                    clsid,
                    new[] { "ncacn_ip_tcp" },
                    null,
                    requestedIids,
                    cancellationToken).ConfigureAwait(false);

                if (activation.Hresult != 0)
                {
                    throw new InvalidOperationException(
                        $"IActivation::RemoteActivation returned HRESULT 0x{unchecked((uint)activation.Hresult):X8}.");
                }
                if (activation.InterfaceResults is null || activation.InterfaceResults.Count == 0)
                {
                    throw new InvalidOperationException("IActivation::RemoteActivation returned no per-IID results.");
                }
                if (activation.InterfaceResults[0].Hresult != 0 || activation.InterfaceResults[0].ObjRef.Length == 0)
                {
                    throw new InvalidOperationException(
                        $"IActivation::RemoteActivation did not return an OBJREF for IOPCServer (per-IID HRESULT 0x{unchecked((uint)activation.InterfaceResults[0].Hresult):X8}).");
                }

                ReadOnlyMemory<byte> objRefBytes = activation.InterfaceResults[0].ObjRef;
                if (!TryDecodeObjRef(objRefBytes.Span, out IOpcInterfaceRef? serverRef))
                {
                    throw new InvalidOperationException("IActivation::RemoteActivation returned an OBJREF that could not be decoded.");
                }

                EndPoint endpoint = ResolveObjectEndpointFromOxidBindings(normalized.Host, activation.OxidBindings.Span)
                    ?? ResolveObjectEndpoint(normalized.Host, serverRef!);

                IAuthContext serverAuth = CreateAuthContext(normalized, clsid);
                ICallChannel serverChannel;
                if (!serverRef!.Ipid.Equals(Guid.Empty))
                {
                    var transportFactory = new TcpSocketTransportFactory();
                    var transport = await transportFactory.ConnectAsync(endpoint, cancellationToken).ConfigureAwait(false);
                    serverChannel = new DcomCallChannel(
                        transport,
                        serverAuth,
                        serverRef.Ipid,
                        DaSessionPreBindIids);
                }
                else
                {
                    serverChannel = await channelFactory.ConnectAsync(
                        endpoint,
                        Guid.Empty,
                        serverAuth,
                        DaSessionPreBindIids,
                        cancellationToken).ConfigureAwait(false);
                }

                // Register per-IID IPID routes for the optional interfaces returned by activation.
                // Index 0 is IOPCServer (the channel default); register slots 1.. as per-interface IPIDs.
                if (serverChannel is DcomCallChannel routableChannel)
                {
                    if (!activation.IpidRemUnknown.Equals(Guid.Empty))
                    {
                        routableChannel.RegisterInterfaceIpid(IRemUnknown.InterfaceId, activation.IpidRemUnknown);
                    }

                    for (int i = 0; i < activation.InterfaceResults.Count && i < requestedIids.Length; i++)
                    {
                        var ir = activation.InterfaceResults[i];
                        if (ir.Hresult != 0 || ir.ObjRef.Length == 0)
                        {
                            continue;
                        }
                        if (!TryDecodeObjRef(ir.ObjRef.Span, out IOpcInterfaceRef? ifaceRef) || ifaceRef!.Ipid.Equals(Guid.Empty))
                        {
                            continue;
                        }
                        routableChannel.RegisterInterfaceIpid(requestedIids[i], ifaceRef.Ipid);
                    }
                }

                return new DaClientState(normalized.Host, normalized.ProgId, clsid, serverChannel, ownsChannel: true);
            }
            finally
            {
                if (activationClient is not null)
                {
                    await activationClient.DisposeAsync().ConfigureAwait(false);
                }
            }
        }

        // Parse the DUALSTRINGARRAY in the activation response's OxidBindings and return
        // the first ncacn_ip_tcp string binding with a [port] suffix.
        private static EndPoint? ResolveObjectEndpointFromOxidBindings(string fallbackHost, ReadOnlySpan<byte> bindings)
        {
            if (bindings.Length < 4)
            {
                return null;
            }

            ushort secOffset = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bindings.Slice(2));
            int idx = 4;
            int entriesConsumed = 2;
            while (idx + 2 <= bindings.Length && entriesConsumed < secOffset)
            {
                ushort tower = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bindings.Slice(idx));
                idx += 2;
                entriesConsumed++;
                if (tower == 0)
                {
                    return null;
                }
                int strStart = idx;
                var sb = new System.Text.StringBuilder();
                while (idx + 2 <= bindings.Length && entriesConsumed < secOffset)
                {
                    ushort ch = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bindings.Slice(idx));
                    idx += 2;
                    entriesConsumed++;
                    if (ch == 0) break;
                    sb.Append((char)ch);
                }
                if (tower != 0x0007)
                {
                    continue;
                }
                string address = sb.ToString();
                int bracket = address.LastIndexOf('[');
                if (bracket < 0 || !address.EndsWith(']'))
                {
                    // No explicit port; this is an OXID-resolver address, skip it.
                    continue;
                }
                string portStr = address.Substring(bracket + 1, address.Length - bracket - 2);
                if (!int.TryParse(portStr, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out int port))
                {
                    continue;
                }
                string host = address.Substring(0, bracket);
                if (string.IsNullOrWhiteSpace(host))
                {
                    host = fallbackHost;
                }
                return new DnsEndPoint(host, port);
            }
            return null;
        }

        private static DaConnectionRequest NormalizeRequest(DaConnectionRequest request)
        {
            string host = string.IsNullOrWhiteSpace(request.Host) ? "localhost" : request.Host.Trim();
            string? progId = NormalizeText(request.ProgId);
            string? clsid = NormalizeText(request.Clsid);
            string? connectionString = NormalizeText(request.ConnectionString);
            if (connectionString is not null && Uri.TryCreate(connectionString, UriKind.Absolute, out Uri? uri))
            {
                if (uri.Scheme.Equals("inmemory", StringComparison.OrdinalIgnoreCase))
                {
                    return request with { Host = host, ConnectionString = connectionString };
                }

                if (uri.Scheme.Equals("dcom", StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals("opcda", StringComparison.OrdinalIgnoreCase))
                {
                    host = string.IsNullOrWhiteSpace(uri.Host) ? host : uri.Host;
                    string pathValue = uri.AbsolutePath.Trim('/');
                    if (!string.IsNullOrWhiteSpace(pathValue))
                    {
                        if (Guid.TryParse(pathValue, out _))
                        {
                            clsid = pathValue;
                        }
                        else
                        {
                            progId = pathValue;
                        }
                    }
                }
            }

            return request with { Host = host, ProgId = progId, Clsid = clsid, ConnectionString = connectionString };
        }

        private static bool TryCreateInMemoryClient(DaConnectionRequest request, out DaClientState? client)
        {
            string? key = TryGetInMemoryKey(request.ConnectionString);
            if (key is null)
            {
                client = null;
                return false;
            }

            if (!InMemoryDaConnectionRegistry.TryGet(key, out ICallChannel channel))
            {
                throw new McpException($"No in-memory DA channel is registered for '{key}'.");
            }

            client = new DaClientState("inmemory", request.ProgId ?? key, Guid.Empty, channel, ownsChannel: false);
            return true;
        }

        private static string? TryGetInMemoryKey(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return null;
            }

            if (Uri.TryCreate(connectionString, UriKind.Absolute, out Uri? uri)
                && uri.Scheme.Equals("inmemory", StringComparison.OrdinalIgnoreCase))
            {
                string key = uri.Host + uri.AbsolutePath.Trim('/');
                return string.IsNullOrWhiteSpace(key) ? null : key;
            }

            const string prefix = "inmemory:";
            return connectionString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? connectionString[prefix.Length..].Trim('/')
                : null;
        }

        private static async Task<Guid> ResolveClsidAsync(DaConnectionRequest request, CancellationToken cancellationToken)
        {
            if (Guid.TryParse(request.Clsid, out Guid clsid))
            {
                return clsid;
            }

            if (Guid.TryParse(request.ProgId, out clsid))
            {
                return clsid;
            }

            if (string.IsNullOrWhiteSpace(request.ProgId))
            {
                throw new McpException("Provide a DA server ProgID, CLSID, or connectionString.");
            }

            OpcServerDescriptor[] servers = await Opc.Classic.Discovery.OpcDiscovery.EnumerateAsync(
                request.Host,
                new[] { OpcGuids.CATID_OPCDAServer20, OpcGuids.CATID_OPCDAServer30 },
                cancellationToken).ConfigureAwait(false);
            OpcServerDescriptor? match = servers.FirstOrDefault(server =>
                string.Equals(server.ProgId, request.ProgId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(server.VerIndProgId, request.ProgId, StringComparison.OrdinalIgnoreCase));
            return match?.ClassId ?? throw new McpException($"OPC DA ProgID '{request.ProgId}' was not found on host '{request.Host}'.");
        }

        private static IAuthContext CreateAuthContext(DaConnectionRequest request, Guid clsid)
        {
            NetworkCredential? credentials = CreateCredential(request.Username, request.Password);
            OpcUrl url = OpcUrl.Parse($"opcda://{request.Host}/{(request.ProgId ?? clsid.ToString("D"))}");
            // SSO takes precedence over explicit username/password when set.
            // Microsoft DCOM hardening (KB5004442) requires Integrity for activation
            // calls; using Connect would result in rpc_s_access_denied after a
            // successful bind, so we leave the default at Integrity.
            OpcConnectData connectData = request.UseSso
                ? OpcConnectData.WithWindowsSso(url, OpcProtectionLevel.Integrity)
                : credentials is null
                    ? OpcConnectData.Anonymous(url)
                    : request.UseKerberos
                        ? OpcConnectData.WithKerberos(url, credentials)
                        : OpcConnectData.WithNtlmV2(url, credentials);
            return NtlmAuthentication.CreateAuthContext(connectData);
        }

        private static NetworkCredential? CreateCredential(string? username, string? password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            string user = username.Trim();
            string domain = string.Empty;
            int slash = user.IndexOf('\\', StringComparison.Ordinal);
            if (slash > 0 && slash < user.Length - 1)
            {
                domain = user[..slash];
                user = user[(slash + 1)..];
            }

            return new NetworkCredential(user, password ?? string.Empty, domain);
        }

        private static byte[] EncodeRemoteCreateInstanceRequest(string host, Guid clsid, Guid requestedIid)
        {
            // Tell the SCM we want PKT_INTEGRITY (level 6) for our callback channel.
            // Required by Microsoft DCOM hardening (KB5004442) which rejects activations
            // that don't declare at least PktIntegrity on Windows clients/servers shipped
            // since 2021. Without this, the SCM may return rpc_s_access_denied even after
            // a successfully authenticated bind.
            var activationProperties = new ActivationProperties(
                new SpecialPropertiesData(ActivationComVersion.V5_6, Mode: 0, ClassContext, requestedIid, Array.Empty<int>()),
                new InstanceInfo(clsid, requestedIid, ClassContext, Mode: 0),
                new LocationInfo(host, Environment.ProcessId, new[] { RpcProtocolSequenceTcp }),
                null,
                new SecurityInfo(AuthenticationLevel: 6, ImpersonationLevel: 3, Capabilities: 0));
            byte[] encodedProperties = ActivationInfoCodec.Encode(activationProperties);

            return WritePayload((ref NdrWriter writer) =>
            {
                writer.WriteGuid(clsid);
                writer.WriteGuid(requestedIid);
                writer.WriteUInt32(1);
                writer.WriteInt32(RpcProtocolSequenceTcp);
                writer.WriteUInt32((uint)encodedProperties.Length);
                writer.WriteRawBytes(encodedProperties);
            });
        }

        private static IOpcInterfaceRef DecodeRemoteCreateInstanceResponse(NdrCallResult result)
        {
            OpcException.ThrowIfFailed(new OpcResultId(result.Hresult, null), "IRemoteSCMActivator::RemoteCreateInstance");
            if (result.ResponsePayload.IsEmpty)
            {
                // An empty response payload typically means the RPC layer returned a fault PDU
                // whose status code was placed in result.Hresult by DcomCallChannel. The most
                // common cause is the DCOM SCM rejecting an anonymous activation request
                // (e.g. fault 0x00000005 == rpc_s_access_denied). Surface a clearer error so
                // operators don't chase an OBJREF-format issue when the real problem is auth
                // or LaunchPermission/AccessPermission on the target AppID.
                int rpcFault = result.Hresult;
                string hint = rpcFault switch
                {
                    0 => "no RPC fault status; the SCM may have returned an empty activation result.",
                    0x00000005 => "rpc_s_access_denied (0x05) - supply NTLMv2/Kerberos credentials with sufficient DCOM Launch/Access permission for this AppID.",
                    _ => $"RPC fault status 0x{rpcFault:X8}.",
                };
                throw new InvalidOperationException("IRemoteSCMActivator::RemoteCreateInstance returned no OPC DA OBJREF: " + hint);
            }

            ReadOnlySpan<byte> response = result.ResponsePayload.Span;
            if (TryDecodeObjRef(response, out IOpcInterfaceRef? directObjRef))
            {
                return directObjRef!;
            }

            if (TryDecodeActivationProperties(response, out IOpcInterfaceRef? activationObjRef))
            {
                return activationObjRef!;
            }

            return DecodeLengthPrefixedObjRef(response);
        }

        private static IOpcInterfaceRef DecodeLengthPrefixedObjRef(ReadOnlySpan<byte> response)
        {
            var reader = new NdrReader(response);
            int innerHresult = reader.ReadInt32();
            OpcException.ThrowIfFailed(new OpcResultId(innerHresult, null), "IRemoteSCMActivator::RemoteCreateInstance");
            uint objRefLength = reader.ReadUInt32();
            if (objRefLength > reader.RemainingBytes)
            {
                throw new InvalidOperationException("RemoteCreateInstance OBJREF length exceeds the remaining response payload.");
            }

            byte[] objRefBytes = reader.ReadRawBytes((int)objRefLength).ToArray();
            if (TryDecodeObjRef(objRefBytes, out IOpcInterfaceRef? objRef))
            {
                return objRef!;
            }

            throw new InvalidOperationException("RemoteCreateInstance returned an invalid OPC DA OBJREF.");
        }

        private static bool TryDecodeActivationProperties(ReadOnlySpan<byte> response, out IOpcInterfaceRef? objRef)
        {
            objRef = null;
            if (!ActivationInfoCodec.TryDecode(response, out ActivationProperties properties)
                || properties.ScmReplyInfo?.ObjRef is not { Length: > 0 } objRefBytes)
            {
                return false;
            }

            return TryDecodeObjRef(objRefBytes, out objRef);
        }

        private static bool TryDecodeObjRef(ReadOnlySpan<byte> payload, out IOpcInterfaceRef? objRef)
        {
            objRef = null;
            if (payload.Length < sizeof(uint) || BinaryPrimitives.ReadUInt32LittleEndian(payload) != ObjRefSignature)
            {
                return false;
            }

            try
            {
                var reader = new NdrReader(payload);
                objRef = OpcInterfaceRefCodec.Read(ref reader);
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static EndPoint ResolveObjectEndpoint(string fallbackHost, IOpcInterfaceRef interfaceRef)
        {
            if (TryFindTcpBinding(interfaceRef.ResolverBindings, out string? host, out int port))
            {
                return new DnsEndPoint(string.IsNullOrWhiteSpace(host) ? fallbackHost : host, port);
            }

            return new DnsEndPoint(fallbackHost, EndpointMapperPort);
        }

        private static bool TryFindTcpBinding(IReadOnlyList<ushort> entries, out string? host, out int port)
        {
            host = null;
            port = EndpointMapperPort;
            for (int index = 0; index < entries.Count;)
            {
                ushort towerId = entries[index++];
                if (towerId == 0)
                {
                    return false;
                }

                string networkAddress = ReadNullTerminatedString(entries, ref index);
                if (towerId != TcpTowerId)
                {
                    continue;
                }

                ParseNetworkAddress(networkAddress, out host, out port);
                return true;
            }

            return false;
        }

        private static string ReadNullTerminatedString(IReadOnlyList<ushort> entries, ref int index)
        {
            var chars = new char[Math.Max(0, entries.Count - index)];
            int length = 0;
            while (index < entries.Count)
            {
                ushort value = entries[index++];
                if (value == 0)
                {
                    break;
                }

                chars[length++] = (char)value;
            }

            return new string(chars, 0, length);
        }

        private static void ParseNetworkAddress(string networkAddress, out string? host, out int port)
        {
            host = networkAddress;
            port = EndpointMapperPort;
            int bracketStart = networkAddress.LastIndexOf('[');
            if (bracketStart < 0 || !networkAddress.EndsWith("]", StringComparison.Ordinal))
            {
                return;
            }

            string portText = networkAddress.Substring(bracketStart + 1, networkAddress.Length - bracketStart - 2);
            if (int.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out int parsedPort) && parsedPort is > 0 and <= 65535)
            {
                port = parsedPort;
                host = networkAddress[..bracketStart];
            }
        }

        private static byte[] WritePayload(NdrWriteAction action)
        {
            ArgumentNullException.ThrowIfNull(action);
            for (int size = DefaultPayloadSize; size <= MaximumPayloadSize; size *= 2)
            {
                var buffer = new byte[size];
                var writer = new NdrWriter(buffer);
                try
                {
                    action(ref writer);
                    return buffer.AsSpan(0, writer.Position).ToArray();
                }
                catch (InvalidOperationException) when (size < MaximumPayloadSize)
                {
                }
            }

            throw new InvalidOperationException("Unable to encode the RemoteCreateInstance payload.");
        }

        private static async ValueTask DisposeChannelAsync(ICallChannel? channel)
        {
            switch (channel)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        private static string? NormalizeText(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();

        private delegate void NdrWriteAction(ref NdrWriter writer);

        private sealed class TcpSocketTransportFactory : IAsyncTransportFactory
        {
            public async ValueTask<IAsyncTransport> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
            {
                ArgumentNullException.ThrowIfNull(endpoint);
                var client = new TcpClient();
                try
                {
                    switch (endpoint)
                    {
                        case DnsEndPoint dns:
                            await client.ConnectAsync(dns.Host, dns.Port, cancellationToken).ConfigureAwait(false);
                            break;
                        case IPEndPoint ip:
                            await client.ConnectAsync(ip.Address, ip.Port, cancellationToken).ConfigureAwait(false);
                            break;
                        default:
                            throw new NotSupportedException($"Endpoint type '{endpoint.GetType().FullName}' is not supported.");
                    }

                    return new TcpSocketTransport(client);
                }
                catch
                {
                    client.Dispose();
                    throw;
                }
            }
        }

        private sealed class TcpSocketTransport : IAsyncTransport
        {
            private readonly TcpClient _client;
            private readonly NetworkStream _stream;

            public TcpSocketTransport(TcpClient client)
            {
                _client = client ?? throw new ArgumentNullException(nameof(client));
                _stream = client.GetStream();
                Input = PipeReader.Create(_stream);
                Output = PipeWriter.Create(_stream);
                RemoteEndpoint = client.Client.RemoteEndPoint ?? new IPEndPoint(IPAddress.None, 0);
            }

            public EndPoint RemoteEndpoint { get; }

            public PipeReader Input { get; }

            public PipeWriter Output { get; }

            public async ValueTask FlushAsync(CancellationToken cancellationToken = default) =>
                await Output.FlushAsync(cancellationToken).ConfigureAwait(false);

            public async ValueTask DisposeAsync()
            {
                await Input.CompleteAsync().ConfigureAwait(false);
                await Output.CompleteAsync().ConfigureAwait(false);
                await _stream.DisposeAsync().ConfigureAwait(false);
                _client.Dispose();
            }
        }
    }
}
