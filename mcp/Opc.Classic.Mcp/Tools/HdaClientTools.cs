//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;

namespace Opc.Classic.Mcp.Tools;

/// <summary>Creates HDA client state for a session.</summary>
public interface IOpcHdaConnectionFactory
{
    /// <summary>Connects to an HDA server and returns a client state object.</summary>
    Task<HdaClientState> ConnectAsync(HdaConnectionRequest request, CancellationToken cancellationToken = default);
}

/// <summary>Connection request used by HDA tools.</summary>
public sealed record HdaConnectionRequest(
    string Host,
    string? ProgId,
    string? Clsid,
    string? Username,
    string? Password,
    bool UseKerberos,
    string? ConnectionString,
    string? AuthLevel = null);

/// <summary>Registers in-memory HDA call channels for MCP tests and loopback scenarios.</summary>
public static class InMemoryHdaConnectionRegistry
{
    private static readonly ConcurrentDictionary<string, InMemoryHdaConnection> Channels = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers an in-memory HDA call channel by name.</summary>
    public static IDisposable Register(string name, ICallChannel channel, IHdaServer? managedServer = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(channel);

        Channels[name] = new InMemoryHdaConnection(channel, managedServer);
        return new Registration(name);
    }

    internal static bool TryGet(string name, out InMemoryHdaConnection connection) => Channels.TryGetValue(name, out connection!);

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

/// <summary>In-memory HDA connection endpoints.</summary>
public sealed record InMemoryHdaConnection(ICallChannel Channel, IHdaServer? ManagedServer);

/// <summary>MCP tools for OPC HDA client operations.</summary>
public sealed class HdaClientTools
{
    private readonly IOpcSessionManager _sessionManager;
    private readonly IOpcHdaConnectionFactory _connectionFactory;

    /// <summary>Creates the HDA client tool set.</summary>
    public HdaClientTools(IOpcSessionManager sessionManager, IEnumerable<IOpcHdaConnectionFactory> connectionFactories)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        ArgumentNullException.ThrowIfNull(connectionFactories);
        _connectionFactory = connectionFactories.FirstOrDefault() ?? new DefaultOpcHdaConnectionFactory();
    }

    /// <summary>Connects a session to an OPC HDA server.</summary>
    [McpServerTool(Name = "opcclassic.hda.connect", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Connects an existing MCP session to an OPC HDA server using DCOM or an in-memory test channel.")]
    public async Task<OpcResultDto> Connect(
        [Description("The sessionId returned by opcclassic.session.create.")]
        string sessionId,
        [Description("OPC HDA server host name or IP address. Ignored when connectionString uses inmemory://.")]
        string host = "localhost",
        [Description("OPC HDA server ProgID. Optional when clsid or connectionString is supplied.")]
        string? progId = null,
        [Description("OPC HDA server CLSID as a GUID string. Optional when progId or connectionString is supplied.")]
        string? clsid = null,
        [Description("Optional user name for NTLMv2 or Kerberos authentication. Use DOMAIN\\user when a Windows domain is required.")]
        string? username = null,
        [Description("Optional password for NTLMv2 or Kerberos authentication. Omit only for anonymous or in-memory connections.")]
        string? password = null,
        [Description("True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied.")]
        bool useKerberos = false,
        [Description("Optional connection string. Use inmemory://name for a registered InMemoryCallChannel, or opchda://host/ProgID for DCOM.")]
        string? connectionString = null,
        [Description(OpcMcpAuthLevel.Description)]
        string? authLevel = null,
        CancellationToken cancellationToken = default)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        HdaClientState client = await _connectionFactory.ConnectAsync(
            new HdaConnectionRequest(host, progId, clsid, username, password, useKerberos, connectionString, authLevel),
            cancellationToken).ConfigureAwait(false);

        HdaClientState? existing = session.HdaClient;
        session.HdaClient = client;
        if (existing is not null)
        {
            await existing.DisposeAsync().ConfigureAwait(false);
        }

        OpcServerStatus status = await GetStatusAsync(client, cancellationToken).ConfigureAwait(false);
        session.Touch();
        return new OpcResultDto(0, $"HDA client connected to {status.VendorInfo}.", Succeeded: true);
    }

    /// <summary>Gets OPC HDA historian status.</summary>
    [McpServerTool(Name = "opcclassic.hda.get_status", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Gets the OPC HDA historian status, including runtime state, version, vendor information, and max return values.")]
    public async Task<OpcServerStatusDto> GetStatus(
        [Description("The sessionId returned by opcclassic.session.create and connected with opcclassic.hda.connect.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        HdaClientState client = GetHdaClient(sessionId);
        OpcServerStatus status = await GetStatusAsync(client, cancellationToken).ConfigureAwait(false);
        return ToStatusDto(status);
    }

    /// <summary>Browses the OPC HDA address space.</summary>
    [McpServerTool(Name = "opcclassic.hda.browse", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Browses the OPC HDA address space below an item ID prefix. In-memory loopback connections return branch and leaf metadata directly.")]
    public async Task<IReadOnlyList<OpcHdaBrowseElementDto>> Browse(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Item ID prefix or branch to browse. Use an empty string for the root.")]
        string itemIdPrefix = "",
        [Description("Browse type: branch, leaf, or flat.")]
        string browseType = "leaf",
        CancellationToken cancellationToken = default)
    {
        HdaClientState client = GetHdaClient(sessionId);
        HdaBrowseType type = ParseBrowseType(browseType);
        if (client.ManagedServer is null)
        {
            return [];
        }

        var elements = new List<OpcHdaBrowseElementDto>();
        await foreach (HdaBrowseElement element in client.ManagedServer.BrowseAsync(itemIdPrefix ?? string.Empty, type, cancellationToken).ConfigureAwait(false))
        {
            elements.Add(new OpcHdaBrowseElementDto(element.Name, element.ItemId, element.BrowseType.ToString()));
        }

        return elements;
    }

    /// <summary>Validates HDA item IDs.</summary>
    [McpServerTool(Name = "opcclassic.hda.validate_items", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Validates OPC HDA item IDs and returns per-item HRESULTs.")]
    public async Task<IReadOnlyList<OpcHdaItemHandleDto>> ValidateItems(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("OPC HDA item IDs to validate.")]
        string[] itemIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        HdaClientState client = GetHdaClient(sessionId);
        int[] errors = await client.Server.ValidateItemIDsAsync(itemIds, cancellationToken).ConfigureAwait(false);
        return itemIds.Select((itemId, index) => ToHandleDto(itemId, index + 1, 0, index < errors.Length ? errors[index] : OpcResultId.Fail.Code)).ToArray();
    }

    /// <summary>Gets HDA server handles for item IDs.</summary>
    [McpServerTool(Name = "opcclassic.hda.get_item_handles", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Gets server handles for OPC HDA item IDs and stores them in the MCP session for subsequent reads and updates.")]
    public async Task<IReadOnlyList<OpcHdaItemHandleDto>> GetItemHandles(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("OPC HDA item IDs to bind.")]
        string[] itemIds,
        [Description("Optional client handles aligned with itemIds. Defaults to 1-based handles.")]
        int[]? clientHandles = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemIds);
        HdaClientState client = GetHdaClient(sessionId);
        int[] requestedClientHandles = NormalizeClientHandles(itemIds.Length, clientHandles);
        int[] handles = await client.Server.GetItemHandlesAsync(itemIds, requestedClientHandles, cancellationToken).ConfigureAwait(false);
        var results = new List<OpcHdaItemHandleDto>(itemIds.Length);
        for (int i = 0; i < itemIds.Length; i++)
        {
            int serverHandle = i < handles.Length ? handles[i] : 0;
            int hresult = serverHandle != 0 ? OpcResultId.Ok.Code : OpcResultId.UnknownItemId.Code;
            if (serverHandle != 0)
            {
                client.ItemHandles[serverHandle] = new HdaItemHandleContext(itemIds[i], requestedClientHandles[i], serverHandle);
            }

            results.Add(ToHandleDto(itemIds[i], requestedClientHandles[i], serverHandle, hresult));
        }

        return results;
    }

    /// <summary>Releases HDA server handles.</summary>
    [McpServerTool(Name = "opcclassic.hda.release_item_handles", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Releases OPC HDA server handles and removes them from the MCP session.")]
    public async Task<IReadOnlyList<OpcResultDto>> ReleaseItemHandles(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("HDA server handles returned by opcclassic.hda.get_item_handles.")]
        int[] serverHandles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        HdaClientState client = GetHdaClient(sessionId);
        int[] errors = await client.Server.ReleaseItemHandlesAsync(serverHandles, cancellationToken).ConfigureAwait(false);
        var results = new List<OpcResultDto>(serverHandles.Length);
        for (int i = 0; i < serverHandles.Length; i++)
        {
            client.ItemHandles.TryRemove(serverHandles[i], out HdaItemHandleContext? context);
            int hresult = i < errors.Length ? errors[i] : OpcResultId.Fail.Code;
            results.Add(new OpcResultDto(hresult, AeClientTools.DescribeHResult(hresult), new OpcResultId(hresult, null).IsSuccess, context?.ItemId, context?.ClientHandle, serverHandles[i]));
        }

        return results;
    }

    /// <summary>Reads raw historical values.</summary>
    [McpServerTool(Name = "opcclassic.hda.read_raw", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Synchronously reads raw historical values over a time range using HDA server handles or item IDs.")]
    public async Task<IReadOnlyList<OpcHdaReadResultDto>> ReadRaw(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Start time as ISO-8601 UTC or HDA relative expression such as NOW-1H.")]
        string startTime,
        [Description("End time as ISO-8601 UTC or HDA relative expression such as NOW.")]
        string endTime,
        [Description("Optional HDA server handles. If omitted, itemIds are bound automatically or all known handles are used.")]
        int[]? serverHandles = null,
        [Description("Optional item IDs to bind automatically when serverHandles are omitted.")]
        string[]? itemIds = null,
        [Description("Maximum values per item. Use 0 for server default or unlimited subject to server limits.")]
        int maxValuesPerItem = 0,
        [Description("True to include bounding values at the start and end times when supported.")]
        bool includeBounds = false,
        CancellationToken cancellationToken = default)
    {
        HdaClientState client = GetHdaClient(sessionId);
        int[] handles = await ResolveHandlesAsync(client, serverHandles, itemIds, cancellationToken).ConfigureAwait(false);
        OpcHdaItem[] items = await client.SyncRead.ReadRawAsync(ToOpcHdaTime(startTime), ToOpcHdaTime(endTime), maxValuesPerItem, includeBounds, handles, cancellationToken).ConfigureAwait(false);
        return ToReadResultDtos(client, handles, items);
    }

    /// <summary>Reads processed historical values.</summary>
    [McpServerTool(Name = "opcclassic.hda.read_processed", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Synchronously reads processed/aggregated historical values over fixed resample intervals.")]
    public async Task<IReadOnlyList<OpcHdaReadResultDto>> ReadProcessed(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Start time as ISO-8601 UTC or HDA relative expression such as NOW-1H.")]
        string startTime,
        [Description("End time as ISO-8601 UTC or HDA relative expression such as NOW.")]
        string endTime,
        [Description("Resample interval in seconds for aggregate buckets.")]
        double resampleIntervalSeconds,
        [Description("Aggregate name or numeric ID, such as Average, Minimum, Maximum, StandardDeviation, or 3.")]
        string aggregate = "Average",
        [Description("Optional HDA server handles. If omitted, itemIds are bound automatically or all known handles are used.")]
        int[]? serverHandles = null,
        [Description("Optional item IDs to bind automatically when serverHandles are omitted.")]
        string[]? itemIds = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        HdaClientState client = GetHdaClient(sessionId);
        int[] handles = await ResolveHandlesAsync(client, serverHandles, itemIds, cancellationToken).ConfigureAwait(false);
        int aggregateId = (int)ParseAggregate(aggregate);
        int[] aggregateIds = Enumerable.Repeat(aggregateId, handles.Length).ToArray();
        OpcHdaItem[] items = await client.SyncRead.ReadProcessedAsync(ToOpcHdaTime(startTime), ToOpcHdaTime(endTime), TimeSpan.FromSeconds(resampleIntervalSeconds).Ticks, handles, aggregateIds, cancellationToken).ConfigureAwait(false);
        return ToReadResultDtos(client, handles, items);
    }

    /// <summary>Reads historical values at specific timestamps.</summary>
    [McpServerTool(Name = "opcclassic.hda.read_at_time", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Synchronously reads interpolated or nearest historical values at specific timestamps.")]
    public async Task<IReadOnlyList<OpcHdaReadResultDto>> ReadAtTime(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Timestamps to read at, as ISO-8601 UTC strings.")]
        DateTimeOffset[] timestamps,
        [Description("Optional HDA server handles. If omitted, itemIds are bound automatically or all known handles are used.")]
        int[]? serverHandles = null,
        [Description("Optional item IDs to bind automatically when serverHandles are omitted.")]
        string[]? itemIds = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(timestamps);
        HdaClientState client = GetHdaClient(sessionId);
        int[] handles = await ResolveHandlesAsync(client, serverHandles, itemIds, cancellationToken).ConfigureAwait(false);
        long[] fileTimes = timestamps.Select(static timestamp => timestamp.ToFileTime()).ToArray();
        OpcHdaItem[] items = await client.SyncRead.ReadAtTimeAsync(fileTimes, handles, cancellationToken).ConfigureAwait(false);
        return ToReadResultDtos(client, handles, items);
    }

    /// <summary>Reads modified/audit historical data.</summary>
    [McpServerTool(Name = "opcclassic.hda.read_modified", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Synchronously reads modified historical data, including modification time, edit type, and user metadata.")]
    public async Task<IReadOnlyList<OpcHdaModifiedReadResultDto>> ReadModified(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Start time as ISO-8601 UTC or HDA relative expression such as NOW-1H.")]
        string startTime,
        [Description("End time as ISO-8601 UTC or HDA relative expression such as NOW.")]
        string endTime,
        [Description("Maximum modified values per item. Use 0 for server default.")]
        int maxValuesPerItem = 0,
        [Description("Optional HDA server handles. If omitted, itemIds are bound automatically or all known handles are used.")]
        int[]? serverHandles = null,
        [Description("Optional item IDs to bind automatically when serverHandles are omitted.")]
        string[]? itemIds = null,
        CancellationToken cancellationToken = default)
    {
        HdaClientState client = GetHdaClient(sessionId);
        int[] handles = await ResolveHandlesAsync(client, serverHandles, itemIds, cancellationToken).ConfigureAwait(false);
        OpcHdaModifiedItem[] items = await client.SyncRead.ReadModifiedAsync(ToOpcHdaTime(startTime), ToOpcHdaTime(endTime), maxValuesPerItem, handles, cancellationToken).ConfigureAwait(false);
        return ToModifiedResultDtos(client, handles, items);
    }

    /// <summary>Reads HDA item attributes.</summary>
    [McpServerTool(Name = "opcclassic.hda.read_attribute", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Synchronously reads one or more HDA attributes for a server handle over a time range.")]
    public async Task<IReadOnlyList<OpcHdaAttributeResultDto>> ReadAttribute(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("HDA server handle returned by opcclassic.hda.get_item_handles.")]
        int serverHandle,
        [Description("HDA attribute IDs to read, such as 1 for DataType or 2 for Description.")]
        int[] attributeIds,
        [Description("Start time as ISO-8601 UTC or HDA relative expression such as NOW-1H.")]
        string startTime,
        [Description("End time as ISO-8601 UTC or HDA relative expression such as NOW.")]
        string endTime,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attributeIds);
        HdaClientState client = GetHdaClient(sessionId);
        OpcHdaAttribute[] attributes = await client.SyncRead.ReadAttributeAsync(ToOpcHdaTime(startTime), ToOpcHdaTime(endTime), serverHandle, attributeIds, cancellationToken).ConfigureAwait(false);
        return ToAttributeResultDtos(client, serverHandle, attributes);
    }

    /// <summary>Reads HDA annotations.</summary>
    [McpServerTool(Name = "opcclassic.hda.read_annotations", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Synchronously reads annotations for HDA items over a time range.")]
    public async Task<IReadOnlyList<OpcHdaAnnotationResultDto>> ReadAnnotations(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Start time as ISO-8601 UTC or HDA relative expression such as NOW-1H.")]
        string startTime,
        [Description("End time as ISO-8601 UTC or HDA relative expression such as NOW.")]
        string endTime,
        [Description("Optional HDA server handles. If omitted, itemIds are bound automatically or all known handles are used.")]
        int[]? serverHandles = null,
        [Description("Optional item IDs to bind automatically when serverHandles are omitted.")]
        string[]? itemIds = null,
        CancellationToken cancellationToken = default)
    {
        HdaClientState client = GetHdaClient(sessionId);
        int[] handles = await ResolveHandlesAsync(client, serverHandles, itemIds, cancellationToken).ConfigureAwait(false);
        OpcHdaAnnotation[] annotations = await client.SyncAnnotations.ReadAsync(ToOpcHdaTime(startTime), ToOpcHdaTime(endTime), handles, cancellationToken).ConfigureAwait(false);
        return ToAnnotationResultDtos(client, handles, annotations);
    }

    /// <summary>Inserts HDA historical data.</summary>
    [McpServerTool(Name = "opcclassic.hda.insert_data", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Inserts historical values for HDA server handles.")]
    public Task<IReadOnlyList<OpcResultDto>> InsertData(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("HDA server handles aligned with timestamps and values.")]
        int[] serverHandles,
        [Description("UTC timestamps aligned with serverHandles and values.")]
        DateTimeOffset[] timestamps,
        [Description("JSON values aligned with serverHandles and timestamps.")]
        JsonElement[] values,
        [Description("Optional HDA quality DWORDs aligned with values. Defaults to OPC Good quality.")]
        int[]? qualities = null,
        CancellationToken cancellationToken = default) =>
        UpdateDataAsync(sessionId, serverHandles, timestamps, values, qualities, UpdateKind.Insert, cancellationToken);

    /// <summary>Replaces HDA historical data.</summary>
    [McpServerTool(Name = "opcclassic.hda.replace_data", ReadOnly = false, Idempotent = false, Destructive = true, OpenWorld = true)]
    [Description("Replaces existing historical values for HDA server handles.")]
    public Task<IReadOnlyList<OpcResultDto>> ReplaceData(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("HDA server handles aligned with timestamps and values.")]
        int[] serverHandles,
        [Description("UTC timestamps aligned with serverHandles and values.")]
        DateTimeOffset[] timestamps,
        [Description("JSON values aligned with serverHandles and timestamps.")]
        JsonElement[] values,
        [Description("Optional HDA quality DWORDs aligned with values. Defaults to OPC Good quality.")]
        int[]? qualities = null,
        CancellationToken cancellationToken = default) =>
        UpdateDataAsync(sessionId, serverHandles, timestamps, values, qualities, UpdateKind.Replace, cancellationToken);

    /// <summary>Inserts or replaces HDA historical data.</summary>
    [McpServerTool(Name = "opcclassic.hda.insert_replace_data", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Inserts historical values or replaces existing values for HDA server handles.")]
    public Task<IReadOnlyList<OpcResultDto>> InsertReplaceData(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("HDA server handles aligned with timestamps and values.")]
        int[] serverHandles,
        [Description("UTC timestamps aligned with serverHandles and values.")]
        DateTimeOffset[] timestamps,
        [Description("JSON values aligned with serverHandles and timestamps.")]
        JsonElement[] values,
        [Description("Optional HDA quality DWORDs aligned with values. Defaults to OPC Good quality.")]
        int[]? qualities = null,
        CancellationToken cancellationToken = default) =>
        UpdateDataAsync(sessionId, serverHandles, timestamps, values, qualities, UpdateKind.InsertReplace, cancellationToken);

    /// <summary>Deletes raw HDA historical data over a time range.</summary>
    [McpServerTool(Name = "opcclassic.hda.delete_raw", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Deletes raw historical values over a time range for one or more HDA server handles.")]
    public async Task<IReadOnlyList<OpcResultDto>> DeleteRaw(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Start time as ISO-8601 UTC or HDA relative expression such as NOW-1H.")]
        string startTime,
        [Description("End time as ISO-8601 UTC or HDA relative expression such as NOW.")]
        string endTime,
        [Description("HDA server handles returned by opcclassic.hda.get_item_handles.")]
        int[] serverHandles,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        HdaClientState client = GetHdaClient(sessionId);
        int[] errors = await client.SyncUpdate.DeleteRawAsync(ToOpcHdaTime(startTime), ToOpcHdaTime(endTime), serverHandles, cancellationToken).ConfigureAwait(false);
        return ToUpdateResults(client, serverHandles, errors);
    }

    /// <summary>Deletes HDA historical data at specific timestamps.</summary>
    [McpServerTool(Name = "opcclassic.hda.delete_at_time", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Deletes historical values at exact timestamps for HDA server handles.")]
    public async Task<IReadOnlyList<OpcResultDto>> DeleteAtTime(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("HDA server handles aligned with timestamps.")]
        int[] serverHandles,
        [Description("UTC timestamps to delete, aligned with serverHandles.")]
        DateTimeOffset[] timestamps,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(timestamps);
        ValidateEqualLength(serverHandles.Length, timestamps.Length, nameof(timestamps));
        HdaClientState client = GetHdaClient(sessionId);
        int[] errors = await client.SyncUpdate.DeleteAtTimeAsync(serverHandles, timestamps.Select(static timestamp => timestamp.ToFileTime()).ToArray(), cancellationToken).ConfigureAwait(false);
        return ToUpdateResults(client, serverHandles, errors);
    }

    /// <summary>Inserts HDA annotations.</summary>
    [McpServerTool(Name = "opcclassic.hda.insert_annotations", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Inserts annotations attached to exact HDA timestamps for server handles.")]
    public async Task<IReadOnlyList<OpcResultDto>> InsertAnnotations(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("HDA server handles aligned with timestamps and annotation texts.")]
        int[] serverHandles,
        [Description("Historical timestamps to annotate, aligned with serverHandles.")]
        DateTimeOffset[] timestamps,
        [Description("Annotation texts aligned with serverHandles.")]
        string[] annotationTexts,
        [Description("Annotation users aligned with serverHandles. Defaults to mcp.")]
        string[]? users = null,
        [Description("Optional annotation creation times aligned with serverHandles. Defaults to now.")]
        DateTimeOffset[]? annotationTimes = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(timestamps);
        ArgumentNullException.ThrowIfNull(annotationTexts);
        ValidateEqualLength(serverHandles.Length, timestamps.Length, nameof(timestamps));
        ValidateEqualLength(serverHandles.Length, annotationTexts.Length, nameof(annotationTexts));
        if (users is not null)
        {
            ValidateEqualLength(serverHandles.Length, users.Length, nameof(users));
        }

        if (annotationTimes is not null)
        {
            ValidateEqualLength(serverHandles.Length, annotationTimes.Length, nameof(annotationTimes));
        }

        HdaClientState client = GetHdaClient(sessionId);
        var annotations = new OpcHdaAnnotation[serverHandles.Length];
        long[] fileTimes = new long[serverHandles.Length];
        for (int i = 0; i < annotations.Length; i++)
        {
            fileTimes[i] = timestamps[i].ToFileTime();
            annotations[i] = new OpcHdaAnnotation(
                GetHandleContext(client, serverHandles[i]).ClientHandle,
                [timestamps[i]],
                [annotationTexts[i]],
                [annotationTimes is not null ? annotationTimes[i] : DateTimeOffset.UtcNow],
                [users is not null ? users[i] : "mcp"]);
        }

        int[] errors = await client.SyncAnnotations.InsertAsync(serverHandles, fileTimes, annotations, cancellationToken).ConfigureAwait(false);
        return ToUpdateResults(client, serverHandles, errors);
    }

    /// <summary>Gets supported HDA aggregates.</summary>
    [McpServerTool(Name = "opcclassic.hda.get_aggregates", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Enumerates aggregate functions supported by the HDA server.")]
    public async Task<IReadOnlyList<OpcHdaAggregateDto>> GetAggregates(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        HdaClientState client = GetHdaClient(sessionId);
        try
        {
            await client.Server.GetAggregatesAsync(out int[] ids, out string[] names, out string[] descriptions, cancellationToken).ConfigureAwait(false);
            return ids.Select((id, index) => new OpcHdaAggregateDto(
                id,
                index < names.Length ? names[index] : ((HdaAggregate)id).ToString(),
                index < descriptions.Length ? descriptions[index] : string.Empty)).ToArray();
        }
        catch (OpcException ex) when (client.ManagedServer is not null && ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            IReadOnlyList<HdaAggregate> aggregates = await client.ManagedServer.GetSupportedAggregatesAsync(cancellationToken).ConfigureAwait(false);
            return aggregates.Select(aggregate => new OpcHdaAggregateDto((int)aggregate, aggregate.ToString(), string.Empty)).ToArray();
        }
    }

    /// <summary>Disconnects from an HDA server.</summary>
    [McpServerTool(Name = "opcclassic.hda.disconnect", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Disconnects the session from its OPC HDA server and releases HDA state.")]
    public async Task<OpcResultDto> Disconnect(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        HdaClientState? client = session.HdaClient;
        session.HdaClient = null;
        if (client is not null)
        {
            await client.DisposeAsync().ConfigureAwait(false);
            return new OpcResultDto(0, "HDA client disconnected.", Succeeded: true);
        }

        return new OpcResultDto(1, "HDA client was not connected.", Succeeded: false);
    }

    private HdaClientState GetHdaClient(string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        return session.HdaClient ?? throw new McpException($"Session '{sessionId}' is not connected to an OPC HDA server. Call opcclassic.hda.connect first.");
    }

    private async Task<IReadOnlyList<OpcResultDto>> UpdateDataAsync(
        string sessionId,
        int[] serverHandles,
        DateTimeOffset[] timestamps,
        JsonElement[] values,
        int[]? qualities,
        UpdateKind updateKind,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(serverHandles);
        ArgumentNullException.ThrowIfNull(timestamps);
        ArgumentNullException.ThrowIfNull(values);
        ValidateEqualLength(serverHandles.Length, timestamps.Length, nameof(timestamps));
        ValidateEqualLength(serverHandles.Length, values.Length, nameof(values));
        if (qualities is not null)
        {
            ValidateEqualLength(serverHandles.Length, qualities.Length, nameof(qualities));
        }

        HdaClientState client = GetHdaClient(sessionId);
        long[] fileTimes = timestamps.Select(static timestamp => timestamp.ToFileTime()).ToArray();
        OpcVariant[] variants = values.Select(AeClientTools.ToVariant).ToArray();
        int[] qualityValues = qualities ?? Enumerable.Repeat((int)OpcQuality.Good.RawValue, values.Length).ToArray();
        int[] errors = updateKind switch
        {
            UpdateKind.Insert => await client.SyncUpdate.InsertAsync(serverHandles, fileTimes, variants, qualityValues, cancellationToken).ConfigureAwait(false),
            UpdateKind.Replace => await client.SyncUpdate.ReplaceAsync(serverHandles, fileTimes, variants, qualityValues, cancellationToken).ConfigureAwait(false),
            _ => await client.SyncUpdate.InsertReplaceAsync(serverHandles, fileTimes, variants, qualityValues, cancellationToken).ConfigureAwait(false),
        };
        return ToUpdateResults(client, serverHandles, errors);
    }

    private static async Task<OpcServerStatus> GetStatusAsync(HdaClientState client, CancellationToken cancellationToken) =>
        client.ManagedServer is not null
            ? await client.ManagedServer.GetStatusAsync(cancellationToken).ConfigureAwait(false)
            : await client.Server.GetStatusAsync(cancellationToken).ConfigureAwait(false);

    private static async Task<int[]> ResolveHandlesAsync(HdaClientState client, int[]? serverHandles, string[]? itemIds, CancellationToken cancellationToken)
    {
        if (serverHandles is { Length: > 0 })
        {
            return serverHandles;
        }

        if (itemIds is { Length: > 0 })
        {
            int[] clientHandles = NormalizeClientHandles(itemIds.Length, null);
            int[] handles = await client.Server.GetItemHandlesAsync(itemIds, clientHandles, cancellationToken).ConfigureAwait(false);
            for (int i = 0; i < itemIds.Length; i++)
            {
                int handle = i < handles.Length ? handles[i] : 0;
                if (handle != 0)
                {
                    client.ItemHandles[handle] = new HdaItemHandleContext(itemIds[i], clientHandles[i], handle);
                }
            }

            return handles.Where(static handle => handle != 0).ToArray();
        }

        return client.ItemHandles.Keys.Order().ToArray();
    }

    private static OpcServerStatusDto ToStatusDto(OpcServerStatus status) =>
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

    private static IReadOnlyList<OpcHdaReadResultDto> ToReadResultDtos(HdaClientState client, IReadOnlyList<int> handles, IReadOnlyList<OpcHdaItem> items)
    {
        var results = new List<OpcHdaReadResultDto>(items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            int handle = i < handles.Count ? handles[i] : 0;
            HdaItemHandleContext context = GetHandleContext(client, handle, items[i].ClientHandle);
            results.Add(new OpcHdaReadResultDto(
                context.ItemId,
                context.ClientHandle,
                context.ServerHandle == 0 ? null : context.ServerHandle,
                items[i].AggregateHandle,
                items[i].AggregateHandle == 0 ? null : ((HdaAggregate)items[i].AggregateHandle).ToString(),
                OpcResultId.Ok.Code,
                AeClientTools.DescribeHResult(OpcResultId.Ok.Code),
                ContinuationHandle: null,
                ToValueDtos(items[i])));
        }

        return results;
    }

    private static IReadOnlyList<OpcHdaItemValueDto> ToValueDtos(OpcHdaItem item)
    {
        var values = new List<OpcHdaItemValueDto>(item.Values.Length);
        for (int i = 0; i < item.Values.Length; i++)
        {
            OpcVariant value = item.Values[i];
            uint quality = i < item.Qualities.Length ? item.Qualities[i] : OpcQuality.Bad.RawValue;
            values.Add(new OpcHdaItemValueDto(
                i < item.Timestamps.Length ? item.Timestamps[i] : DateTimeOffset.UnixEpoch,
                AeClientTools.NormalizeValue(OpcVariantConverter.ToObject(value)),
                value.Type.ToString(),
                quality,
                new OpcQuality(unchecked((ushort)quality)).ToString()));
        }

        return values;
    }

    private static IReadOnlyList<OpcHdaModifiedReadResultDto> ToModifiedResultDtos(HdaClientState client, IReadOnlyList<int> handles, IReadOnlyList<OpcHdaModifiedItem> items)
    {
        var results = new List<OpcHdaModifiedReadResultDto>(items.Count);
        for (int i = 0; i < items.Count; i++)
        {
            int handle = i < handles.Count ? handles[i] : 0;
            HdaItemHandleContext context = GetHandleContext(client, handle, items[i].ClientHandle);
            var values = new List<OpcHdaModifiedValueDto>(items[i].Values.Length);
            for (int j = 0; j < items[i].Values.Length; j++)
            {
                OpcVariant value = items[i].Values[j];
                uint quality = j < items[i].Qualities.Length ? items[i].Qualities[j] : OpcQuality.Bad.RawValue;
                values.Add(new OpcHdaModifiedValueDto(
                    j < items[i].Timestamps.Length ? items[i].Timestamps[j] : DateTimeOffset.UnixEpoch,
                    AeClientTools.NormalizeValue(OpcVariantConverter.ToObject(value)),
                    value.Type.ToString(),
                    quality,
                    new OpcQuality(unchecked((ushort)quality)).ToString(),
                    j < items[i].ModificationTimes.Length ? items[i].ModificationTimes[j] : DateTimeOffset.UnixEpoch,
                    j < items[i].EditTypes.Length ? items[i].EditTypes[j] : 0,
                    j < items[i].Users.Length ? items[i].Users[j] : null));
            }

            results.Add(new OpcHdaModifiedReadResultDto(context.ItemId, context.ClientHandle, context.ServerHandle == 0 ? null : context.ServerHandle, OpcResultId.Ok.Code, AeClientTools.DescribeHResult(OpcResultId.Ok.Code), values));
        }

        return results;
    }

    private static IReadOnlyList<OpcHdaAttributeResultDto> ToAttributeResultDtos(HdaClientState client, int serverHandle, IReadOnlyList<OpcHdaAttribute> attributes)
    {
        HdaItemHandleContext context = GetHandleContext(client, serverHandle);
        return attributes.Select(attribute => new OpcHdaAttributeResultDto(
            context.ItemId,
            attribute.ClientHandle == 0 ? context.ClientHandle : attribute.ClientHandle,
            context.ServerHandle,
            attribute.AttributeId,
            OpcResultId.Ok.Code,
            AeClientTools.DescribeHResult(OpcResultId.Ok.Code),
            ToAttributeValueDtos(attribute))).ToArray();
    }

    private static IReadOnlyList<OpcHdaAttributeValueDto> ToAttributeValueDtos(OpcHdaAttribute attribute)
    {
        var values = new List<OpcHdaAttributeValueDto>(attribute.Values.Length);
        for (int i = 0; i < attribute.Values.Length; i++)
        {
            OpcVariant value = attribute.Values[i];
            values.Add(new OpcHdaAttributeValueDto(
                i < attribute.Timestamps.Length ? attribute.Timestamps[i] : DateTimeOffset.UnixEpoch,
                AeClientTools.NormalizeValue(OpcVariantConverter.ToObject(value)),
                value.Type.ToString()));
        }

        return values;
    }

    private static IReadOnlyList<OpcHdaAnnotationResultDto> ToAnnotationResultDtos(HdaClientState client, IReadOnlyList<int> handles, IReadOnlyList<OpcHdaAnnotation> annotations)
    {
        var results = new List<OpcHdaAnnotationResultDto>(annotations.Count);
        for (int i = 0; i < annotations.Count; i++)
        {
            int handle = i < handles.Count ? handles[i] : 0;
            HdaItemHandleContext context = GetHandleContext(client, handle, annotations[i].ClientHandle);
            var values = new List<OpcHdaAnnotationDto>(annotations[i].Annotations.Length);
            for (int j = 0; j < annotations[i].Annotations.Length; j++)
            {
                values.Add(new OpcHdaAnnotationDto(
                    j < annotations[i].Timestamps.Length ? annotations[i].Timestamps[j] : DateTimeOffset.UnixEpoch,
                    j < annotations[i].AnnotationTimes.Length ? annotations[i].AnnotationTimes[j] : DateTimeOffset.UnixEpoch,
                    annotations[i].Annotations[j] ?? string.Empty,
                    j < annotations[i].Users.Length ? annotations[i].Users[j] ?? string.Empty : string.Empty));
            }

            results.Add(new OpcHdaAnnotationResultDto(context.ItemId, context.ClientHandle, context.ServerHandle == 0 ? null : context.ServerHandle, OpcResultId.Ok.Code, AeClientTools.DescribeHResult(OpcResultId.Ok.Code), values));
        }

        return results;
    }

    private static IReadOnlyList<OpcResultDto> ToUpdateResults(HdaClientState client, IReadOnlyList<int> serverHandles, IReadOnlyList<int> errors)
    {
        var results = new List<OpcResultDto>(serverHandles.Count);
        for (int i = 0; i < serverHandles.Count; i++)
        {
            HdaItemHandleContext context = GetHandleContext(client, serverHandles[i]);
            int hresult = i < errors.Count ? errors[i] : OpcResultId.Fail.Code;
            results.Add(new OpcResultDto(hresult, AeClientTools.DescribeHResult(hresult), new OpcResultId(hresult, null).IsSuccess, context.ItemId, context.ClientHandle, serverHandles[i]));
        }

        return results;
    }

    private static OpcHdaItemHandleDto ToHandleDto(string itemId, int clientHandle, int serverHandle, int hresult) =>
        new(itemId, clientHandle, serverHandle, hresult, AeClientTools.DescribeHResult(hresult), new OpcResultId(hresult, null).IsSuccess);

    private static HdaItemHandleContext GetHandleContext(HdaClientState client, int serverHandle, int clientHandle = 0)
    {
        if (serverHandle != 0 && client.ItemHandles.TryGetValue(serverHandle, out HdaItemHandleContext? context))
        {
            return context;
        }

        return new HdaItemHandleContext(serverHandle == 0 ? string.Empty : "#" + serverHandle.ToString(CultureInfo.InvariantCulture), clientHandle, serverHandle);
    }

    private static int[] NormalizeClientHandles(int count, int[]? clientHandles)
    {
        var handles = new int[count];
        for (int i = 0; i < handles.Length; i++)
        {
            handles[i] = clientHandles is not null && i < clientHandles.Length ? clientHandles[i] : i + 1;
        }

        return handles;
    }

    private static OpcHdaTime ToOpcHdaTime(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return OpcHdaTime.FromTimestamp(DateTimeOffset.UtcNow);
        }

        string trimmed = value.Trim();
        if (trimmed.StartsWith("NOW", StringComparison.OrdinalIgnoreCase))
        {
            // Convert relative "NOW-1H" expressions to absolute UTC timestamps
            // so the wire format stays struct-only (no LPWSTR field that
            // requires deferred NDR encoding inside OPCHDA_TIME).
            DateTimeOffset resolved = ResolveRelativeTime(trimmed, DateTimeOffset.UtcNow);
            return OpcHdaTime.FromTimestamp(resolved);
        }

        return DateTimeOffset.TryParse(trimmed, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset parsed)
            ? OpcHdaTime.FromTimestamp(parsed)
            : OpcHdaTime.FromTimestamp(DateTimeOffset.UtcNow);
    }

    private static DateTimeOffset ResolveRelativeTime(string expression, DateTimeOffset reference)
    {
        // Accepts "NOW", "NOW-1H", "NOW+30M", "NOW-2D", etc. Falls back to
        // reference time when the suffix is unrecognized.
        string trimmed = expression.Trim();
        if (string.Equals(trimmed, "NOW", StringComparison.OrdinalIgnoreCase))
        {
            return reference;
        }
        if (trimmed.Length < 5)
        {
            return reference;
        }
        char sign = trimmed[3];
        if (sign != '+' && sign != '-')
        {
            return reference;
        }
        string magnitudePart = trimmed[4..^1];
        char unit = char.ToUpperInvariant(trimmed[^1]);
        if (!double.TryParse(magnitudePart, NumberStyles.Number, CultureInfo.InvariantCulture, out double magnitude))
        {
            return reference;
        }
        TimeSpan delta = unit switch
        {
            'S' => TimeSpan.FromSeconds(magnitude),
            'M' => TimeSpan.FromMinutes(magnitude),
            'H' => TimeSpan.FromHours(magnitude),
            'D' => TimeSpan.FromDays(magnitude),
            _ => TimeSpan.Zero,
        };
        return sign == '+' ? reference + delta : reference - delta;
    }

    private static HdaBrowseType ParseBrowseType(string browseType) => browseType?.Trim().ToLowerInvariant() switch
    {
        "branch" or "branches" => HdaBrowseType.Branch,
        "flat" or "all" => HdaBrowseType.Flat,
        _ => HdaBrowseType.Leaf,
    };

    private static HdaAggregate ParseAggregate(string aggregate)
    {
        if (int.TryParse(aggregate, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id))
        {
            return (HdaAggregate)id;
        }

        string normalized = aggregate.Replace(" ", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("_", string.Empty, StringComparison.OrdinalIgnoreCase);
        return normalized.ToLowerInvariant() switch
        {
            "min" => HdaAggregate.Minimum,
            "max" => HdaAggregate.Maximum,
            "stdev" or "stddev" => HdaAggregate.StandardDeviation,
            _ when Enum.TryParse(normalized, ignoreCase: true, out HdaAggregate parsed) => parsed,
            _ => throw new ArgumentException($"Unknown HDA aggregate '{aggregate}'.", nameof(aggregate)),
        };
    }

    private static void ValidateEqualLength(int expected, int actual, string parameterName)
    {
        if (expected != actual)
        {
            throw new ArgumentException($"Expected {expected} values but received {actual}.", parameterName);
        }
    }

    private enum UpdateKind
    {
        Insert,
        Replace,
        InsertReplace,
    }

    private sealed class DefaultOpcHdaConnectionFactory : IOpcHdaConnectionFactory
    {
        public async Task<HdaClientState> ConnectAsync(HdaConnectionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            OpcMcpDcomConnectionRequest normalized = OpcMcpDcomConnectionHelper.NormalizeRequest(
                request.Host,
                request.ProgId,
                request.Clsid,
                request.Username,
                request.Password,
                request.UseKerberos,
                request.ConnectionString,
                request.AuthLevel,
                "opchda");

            string? inMemoryKey = OpcMcpDcomConnectionHelper.TryGetInMemoryKey(normalized.ConnectionString);
            if (inMemoryKey is not null)
            {
                if (!InMemoryHdaConnectionRegistry.TryGet(inMemoryKey, out InMemoryHdaConnection connection))
                {
                    throw new McpException($"No in-memory HDA channel is registered for '{inMemoryKey}'.");
                }

                return new HdaClientState("inmemory", normalized.ProgId ?? inMemoryKey, Guid.Empty, connection.Channel, ownsChannel: false, connection.ManagedServer);
            }

            (ICallChannel channel, Guid clsid) = await OpcMcpDcomConnectionHelper.ConnectDcomAsync(
                normalized,
                IOPCHDA_Server.InterfaceId,
                [OpcGuids.CATID_OPCHDAServer10],
                "opchda",
                cancellationToken,
                additionalIids: new[]
                {
                    IOPCHDA_SyncRead.InterfaceId,
                    IOPCHDA_SyncUpdate.InterfaceId,
                    IOPCHDA_SyncAnnotations.InterfaceId,
                    IOPCHDA_AsyncRead.InterfaceId,
                    IOPCHDA_AsyncUpdate.InterfaceId,
                    IOPCHDA_AsyncAnnotations.InterfaceId,
                    IOPCHDA_Playback.InterfaceId,
                }).ConfigureAwait(false);
            return new HdaClientState(normalized.Host, normalized.ProgId, clsid, channel, ownsChannel: true);
        }
    }
}
