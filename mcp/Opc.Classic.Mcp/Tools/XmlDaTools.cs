//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Xml;

namespace Opc.Classic.Mcp.Tools;

/// <summary>Creates XML-DA client state for a session.</summary>
public interface IOpcXmlDaConnectionFactory
{
    /// <summary>Connects to an XML-DA endpoint and returns client state.</summary>
    Task<XmlDaClientState> ConnectAsync(XmlDaConnectionRequest request, CancellationToken cancellationToken = default);
}

/// <summary>Connection request used by XML-DA tools.</summary>
public sealed record XmlDaConnectionRequest(string EndpointUrl);

/// <summary>Registers in-memory XML-DA clients for MCP tests and loopback scenarios.</summary>
public static class InMemoryXmlDaConnectionRegistry
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, IXmlDaClient> Clients = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers an in-memory XML-DA client by name.</summary>
    public static IDisposable Register(string name, IXmlDaClient client)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(client);

        Clients[name] = client;
        return new Registration(name);
    }

    internal static bool TryGet(string name, out IXmlDaClient client) => Clients.TryGetValue(name, out client!);

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
            Clients.TryRemove(_name, out _);
        }
    }
}

/// <summary>MCP tools for OPC XML-DA SOAP/HTTP operations.</summary>
public sealed class XmlDaTools
{
    private readonly IOpcSessionManager _sessionManager;
    private readonly IOpcXmlDaConnectionFactory _connectionFactory;

    /// <summary>Creates the XML-DA tool set.</summary>
    public XmlDaTools(IOpcSessionManager sessionManager, IEnumerable<IOpcXmlDaConnectionFactory> connectionFactories)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        ArgumentNullException.ThrowIfNull(connectionFactories);
        _connectionFactory = connectionFactories.FirstOrDefault() ?? new DefaultOpcXmlDaConnectionFactory();
    }

    /// <summary>Connects a session to an OPC XML-DA HTTP endpoint.</summary>
    [McpServerTool(Name = "opcclassic.xmlda.connect", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Connects an existing MCP session to an OPC XML-DA HTTP/SOAP endpoint URL.")]
    public async Task<OpcResultDto> Connect(
        [Description("The sessionId returned by opcclassic.session.create.")]
        string sessionId,
        [Description("XML-DA HTTP or HTTPS endpoint URL. Use inmemory://name for a registered test client.")]
        string endpointUrl,
        CancellationToken cancellationToken = default)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        XmlDaClientState client = await _connectionFactory.ConnectAsync(new XmlDaConnectionRequest(endpointUrl), cancellationToken).ConfigureAwait(false);
        XmlDaClientState? existing = session.XmlDaClient;
        session.XmlDaClient = client;
        if (existing is not null)
        {
            await existing.DisposeAsync().ConfigureAwait(false);
        }

        _ = await client.Client.GetStatusAsync(CreateHeader(null, null), cancellationToken).ConfigureAwait(false);
        session.Touch();
        return new OpcResultDto(0, $"XML-DA client connected to {client.EndpointUrl}.", Succeeded: true, ItemName: client.EndpointUrl);
    }

    /// <summary>Gets XML-DA server status.</summary>
    [McpServerTool(Name = "opcclassic.xmlda.get_status", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Calls XML-DA GetStatus on the connected HTTP endpoint.")]
    public async Task<OpcXmlDaServerStatusDto> GetStatus(
        [Description("The sessionId returned by opcclassic.session.create and connected with opcclassic.xmlda.connect.")]
        string sessionId,
        [Description("Optional requested locale ID, such as en-US.")]
        string? localeId = null,
        [Description("Optional client request handle echoed by the server.")]
        string? clientRequestHandle = null,
        CancellationToken cancellationToken = default)
    {
        XmlDaClientState client = GetXmlDaClient(sessionId);
        XmlDaServerStatus status = await client.Client.GetStatusAsync(CreateHeader(localeId, clientRequestHandle), cancellationToken).ConfigureAwait(false);
        return ToDto(status);
    }

    /// <summary>Browses an XML-DA address space.</summary>
    [McpServerTool(Name = "opcclassic.xmlda.browse", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Calls XML-DA Browse on the connected HTTP endpoint.")]
    public async Task<OpcXmlDaBrowseResponseDto> Browse(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("The XML-DA item name/branch to browse. Use an empty string for the root.")]
        string itemName = "",
        [Description("Optional vendor-defined item path.")]
        string itemPath = "",
        [Description("Optional continuation point from a previous browse response.")]
        string continuationPoint = "",
        [Description("Maximum elements to return. Use 0 for server default/no limit.")]
        int maxElementsReturned = 0,
        [Description("Browse filter: all, branch, or item.")]
        string browseFilter = "all",
        [Description("Optional element name filter.")]
        string elementNameFilter = "",
        [Description("Optional requested locale ID, such as en-US.")]
        string? localeId = null,
        [Description("Optional client request handle echoed by the server.")]
        string? clientRequestHandle = null,
        CancellationToken cancellationToken = default)
    {
        XmlDaClientState client = GetXmlDaClient(sessionId);
        var request = new XmlDaBrowseRequest(
            CreateHeader(localeId, clientRequestHandle),
            itemName ?? string.Empty,
            itemPath ?? string.Empty,
            continuationPoint ?? string.Empty,
            maxElementsReturned,
            ParseBrowseFilter(browseFilter),
            elementNameFilter ?? string.Empty);
        XmlDaBrowseResponse response = await client.Client.BrowseAsync(request, cancellationToken).ConfigureAwait(false);
        return ToDto(response);
    }

    /// <summary>Gets XML-DA item properties.</summary>
    [McpServerTool(Name = "opcclassic.xmlda.get_properties", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Calls XML-DA GetProperties for one or more item names.")]
    public async Task<OpcXmlDaGetPropertiesResponseDto> GetProperties(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Item names whose properties should be retrieved.")]
        string[] itemNames,
        [Description("Optional vendor-defined item path applied to all items.")]
        string itemPath = "",
        [Description("Optional property names to retrieve. Omit or pass empty with returnAllProperties=true for all properties.")]
        string[]? propertyNames = null,
        [Description("True to return all properties.")]
        bool returnAllProperties = true,
        [Description("True to include property values.")]
        bool returnPropertyValues = false,
        [Description("True to include localized error text.")]
        bool returnErrorText = true,
        [Description("Optional requested locale ID, such as en-US.")]
        string? localeId = null,
        [Description("Optional client request handle echoed by the server.")]
        string? clientRequestHandle = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemNames);
        XmlDaClientState client = GetXmlDaClient(sessionId);
        var request = new XmlDaGetPropertiesRequest(
            CreateHeader(localeId, clientRequestHandle),
            itemPath ?? string.Empty,
            itemNames,
            propertyNames ?? [],
            returnAllProperties,
            returnPropertyValues,
            returnErrorText);
        XmlDaGetPropertiesResponse response = await client.Client.GetPropertiesAsync(request, cancellationToken).ConfigureAwait(false);
        return ToDto(response);
    }

    /// <summary>Synchronously reads XML-DA item values.</summary>
    [McpServerTool(Name = "opcclassic.xmlda.read", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Calls XML-DA Read for one or more items.")]
    public async Task<IReadOnlyList<OpcXmlDaItemValueDto>> Read(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Items to read, including optional client handles and MaxAge values.")]
        OpcXmlDaReadItemDto[] items,
        [Description("True to include localized error text.")]
        bool returnErrorText = true,
        [Description("Optional requested locale ID, such as en-US.")]
        string? localeId = null,
        [Description("Optional client request handle echoed by the server.")]
        string? clientRequestHandle = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        XmlDaClientState client = GetXmlDaClient(sessionId);
        XmlDaReadItem[] readItems = items.Select(static item => new XmlDaReadItem(item.ItemName, item.ClientItemHandle, item.MaxAge)).ToArray();
        XmlDaReadResponse response = await client.Client.ReadAsync(new XmlDaReadRequest(CreateHeader(localeId, clientRequestHandle), readItems, returnErrorText), cancellationToken).ConfigureAwait(false);
        return response.Items.Select(ToDto).ToArray();
    }

    /// <summary>Synchronously writes XML-DA item values.</summary>
    [McpServerTool(Name = "opcclassic.xmlda.write", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Calls XML-DA Write for one or more items.")]
    public async Task<IReadOnlyList<OpcXmlDaWriteResultDto>> Write(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Items and JSON values to write.")]
        OpcXmlDaWriteItemDto[] items,
        [Description("True to have the server echo values on reply where supported.")]
        bool returnValuesOnReply = false,
        [Description("True to include localized error text.")]
        bool returnErrorText = true,
        [Description("Optional requested locale ID, such as en-US.")]
        string? localeId = null,
        [Description("Optional client request handle echoed by the server.")]
        string? clientRequestHandle = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        XmlDaClientState client = GetXmlDaClient(sessionId);
        XmlDaWriteItem[] writeItems = items.Select(static item => new XmlDaWriteItem(item.ItemName, item.ClientItemHandle, ToXmlDaValue(item.Value, item.ValueType))).ToArray();
        XmlDaWriteResponse response = await client.Client.WriteAsync(new XmlDaWriteRequest(CreateHeader(localeId, clientRequestHandle), writeItems, returnValuesOnReply, returnErrorText), cancellationToken).ConfigureAwait(false);
        return response.Items.Select(ToDto).ToArray();
    }

    /// <summary>Creates an XML-DA subscription.</summary>
    [McpServerTool(Name = "opcclassic.xmlda.subscribe", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Calls XML-DA Subscribe. Use opcclassic.xmlda.poll_subscription to retrieve changes.")]
    public async Task<OpcXmlDaSubscriptionDto> Subscribe(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Items to subscribe to.")]
        OpcXmlDaSubscribeItemDto[] items,
        [Description("Optional vendor-defined item path applied to all items.")]
        string itemPath = "",
        [Description("Default requested sampling rate in milliseconds.")]
        int requestedSamplingRate = 0,
        [Description("Subscription ping/keep-alive rate in milliseconds.")]
        int subscriptionPingRate = 0,
        [Description("True to return initial values in the subscribe response.")]
        bool returnValuesOnReply = false,
        [Description("True to include localized error text.")]
        bool returnErrorText = true,
        [Description("True to enable server-side buffering for changes.")]
        bool enableBuffering = false,
        [Description("Optional requested locale ID, such as en-US.")]
        string? localeId = null,
        [Description("Optional client request handle echoed by the server.")]
        string? clientRequestHandle = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);
        XmlDaClientState client = GetXmlDaClient(sessionId);
        XmlDaSubscribeItem[] subscribeItems = items.Select(static item => new XmlDaSubscribeItem(item.ItemName, item.ClientItemHandle, item.RequestedSamplingRate, item.Deadband)).ToArray();
        var request = new XmlDaSubscribeRequest(
            CreateHeader(localeId, clientRequestHandle),
            subscribeItems,
            itemPath ?? string.Empty,
            requestedSamplingRate,
            subscriptionPingRate,
            returnValuesOnReply,
            returnErrorText,
            enableBuffering);
        XmlDaSubscribeResponse response = await client.Client.SubscribeAsync(request, cancellationToken).ConfigureAwait(false);
        return ToDto(response);
    }

    /// <summary>Polls XML-DA subscriptions for changes.</summary>
    [McpServerTool(Name = "opcclassic.xmlda.poll_subscription", ReadOnly = true, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Calls XML-DA SubscriptionPolledRefresh for one or more server subscription handles.")]
    public async Task<OpcXmlDaSubscriptionPollDto> PollSubscription(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Server subscription handles returned by opcclassic.xmlda.subscribe.")]
        string[] serverSubHandles,
        [Description("Optional earliest hold time. Omit to poll immediately.")]
        DateTimeOffset? holdTime = null,
        [Description("Maximum server wait time in milliseconds.")]
        int waitTime = 0,
        [Description("True to return all subscribed item values, not only changes.")]
        bool returnAllItems = false,
        [Description("True to include localized error text.")]
        bool returnErrorText = true,
        [Description("Optional requested locale ID, such as en-US.")]
        string? localeId = null,
        [Description("Optional client request handle echoed by the server.")]
        string? clientRequestHandle = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(serverSubHandles);
        XmlDaClientState client = GetXmlDaClient(sessionId);
        var request = new XmlDaSubscriptionPolledRefreshRequest(
            CreateHeader(localeId, clientRequestHandle),
            serverSubHandles,
            holdTime,
            waitTime,
            returnAllItems,
            returnErrorText);
        XmlDaSubscriptionPolledRefreshResponse response = await client.Client.SubscriptionPolledRefreshAsync(request, cancellationToken).ConfigureAwait(false);
        return ToDto(response);
    }

    /// <summary>Cancels an XML-DA subscription.</summary>
    [McpServerTool(Name = "opcclassic.xmlda.cancel_subscription", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Calls XML-DA SubscriptionCancel for a server subscription handle.")]
    public async Task<OpcResultDto> CancelSubscription(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Server subscription handle returned by opcclassic.xmlda.subscribe.")]
        string serverSubHandle,
        [Description("Optional client request handle echoed by the server.")]
        string? clientRequestHandle = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(serverSubHandle);
        XmlDaClientState client = GetXmlDaClient(sessionId);
        XmlDaSubscriptionCancelResponse response = await client.Client.SubscriptionCancelAsync(new XmlDaSubscriptionCancelRequest(serverSubHandle, clientRequestHandle), cancellationToken).ConfigureAwait(false);
        return new OpcResultDto(0, $"XML-DA subscription '{serverSubHandle}' cancelled.", Succeeded: true, SubscriptionId: serverSubHandle, ValueType: response.ClientRequestHandle);
    }

    /// <summary>Disconnects XML-DA client state from the session.</summary>
    [McpServerTool(Name = "opcclassic.xmlda.disconnect", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Disconnects the session from its OPC XML-DA endpoint and releases HTTP client state.")]
    public async Task<OpcResultDto> Disconnect(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        XmlDaClientState? client = session.XmlDaClient;
        session.XmlDaClient = null;
        if (client is not null)
        {
            await client.DisposeAsync().ConfigureAwait(false);
            return new OpcResultDto(0, "XML-DA client disconnected.", Succeeded: true);
        }

        return new OpcResultDto(1, "XML-DA client was not connected.", Succeeded: false);
    }

    private XmlDaClientState GetXmlDaClient(string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        return session.XmlDaClient ?? throw new McpException($"Session '{sessionId}' is not connected to an OPC XML-DA endpoint. Call opcclassic.xmlda.connect first.");
    }

    private static XmlDaRequestHeader CreateHeader(string? localeId, string? clientRequestHandle) =>
        new(string.IsNullOrWhiteSpace(localeId) ? null : localeId, string.IsNullOrWhiteSpace(clientRequestHandle) ? null : clientRequestHandle);

    private static OpcXmlDaServerStatusDto ToDto(XmlDaServerStatus status) => new(
        status.StartTime,
        status.ProductVersion,
        status.VendorInfo,
        status.SupportedLocaleIds,
        status.SupportedInterfaceVersions,
        status.ServerState.ToString(),
        status.StatusInfo);

    private static OpcXmlDaBrowseResponseDto ToDto(XmlDaBrowseResponse response) => new(
        response.ServerState.ToString(),
        response.Elements.Select(static element => new OpcXmlDaBrowseElementDto(element.Name, element.ItemPath, element.ItemName, element.IsItem, element.HasChildren)).ToArray(),
        response.ContinuationPoint,
        response.MoreElements);

    private static OpcXmlDaGetPropertiesResponseDto ToDto(XmlDaGetPropertiesResponse response) => new(
        response.ServerState.ToString(),
        response.PropertyLists.Select(static list => new OpcXmlDaItemPropertyListDto(
            list.ItemName,
            list.ItemPath,
            list.Properties.Select(ToDto).ToArray(),
            list.ResultId,
            list.ResultCode.ToString())).ToArray());

    private static OpcXmlDaPropertyValueDto ToDto(XmlDaPropertyValue property) => new(
        property.Name,
        property.Description,
        NormalizeValue(property.Value),
        property.Value?.Type.ToString(),
        property.Value?.RawText,
        property.ResultId,
        property.ResultCode.ToString());

    private static OpcXmlDaItemValueDto ToDto(XmlDaItemValueResult item) => new(
        item.ItemName,
        item.ClientItemHandle,
        NormalizeValue(item.Value),
        item.Value?.Type.ToString(),
        item.Value?.RawText,
        item.Quality.RawValue,
        item.Quality.ToString(),
        item.Timestamp,
        item.ResultId,
        item.ResultCode.ToString());

    private static OpcXmlDaWriteResultDto ToDto(XmlDaWriteItemResult item) => new(
        item.ItemName,
        item.ClientItemHandle,
        item.ResultId,
        item.ResultCode.ToString(),
        item.ErrorText);

    private static OpcXmlDaSubscriptionDto ToDto(XmlDaSubscribeResponse response) => new(
        response.ServerSubHandle,
        response.RevisedSamplingRate,
        response.ServerState.ToString(),
        response.Items.Select(ToDto).ToArray());

    private static OpcXmlDaSubscriptionPollDto ToDto(XmlDaSubscriptionPolledRefreshResponse response) => new(
        response.ServerState.ToString(),
        response.DataBufferOverflow,
        response.InvalidServerSubHandles,
        response.ItemLists.Select(static list => new OpcXmlDaSubscriptionItemListDto(list.SubscriptionHandle, list.Items.Select(ToDto).ToArray())).ToArray());

    private static XmlDaBrowseFilter ParseBrowseFilter(string browseFilter) => browseFilter?.Trim().ToLowerInvariant() switch
    {
        "branch" or "branches" => XmlDaBrowseFilter.Branch,
        "item" or "items" or "leaf" or "leaves" => XmlDaBrowseFilter.Item,
        _ => XmlDaBrowseFilter.All,
    };

    private static XmlDaValue ToXmlDaValue(object? value, string? valueType = null)
    {
        if (!string.IsNullOrWhiteSpace(valueType))
        {
            return ToXmlDaValueByType(value, valueType.Trim());
        }

        return value switch
        {
            null => XmlDaValue.OfString(string.Empty),
            JsonElement element => ToXmlDaValue(element),
            bool boolean => XmlDaValue.OfBoolean(boolean),
            sbyte int8 => XmlDaValue.OfInt8(int8),
            byte uint8 => XmlDaValue.OfUInt8(uint8),
            short int16 => XmlDaValue.OfInt16(int16),
            ushort uint16 => XmlDaValue.OfUInt16(uint16),
            int int32 => XmlDaValue.OfInt32(int32),
            uint uint32 => XmlDaValue.OfUInt32(uint32),
            long int64 => XmlDaValue.OfInt64(int64),
            ulong uint64 => XmlDaValue.OfUInt64(uint64),
            float single => XmlDaValue.OfSingle(single),
            double dbl => XmlDaValue.OfDouble(dbl),
            decimal dec => XmlDaValue.OfDecimal(dec),
            DateTimeOffset dateTimeOffset => XmlDaValue.OfDateTime(dateTimeOffset),
            DateTime dateTime => XmlDaValue.OfDateTime(new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc))),
            string text => StringToXmlDaValue(text),
            _ => XmlDaValue.OfString(value.ToString() ?? string.Empty),
        };
    }

    private static XmlDaValue ToXmlDaValue(JsonElement element) => element.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => XmlDaValue.OfString(string.Empty),
        JsonValueKind.True => XmlDaValue.OfBoolean(true),
        JsonValueKind.False => XmlDaValue.OfBoolean(false),
        JsonValueKind.Number when element.TryGetInt32(out int int32) => XmlDaValue.OfInt32(int32),
        JsonValueKind.Number when element.TryGetInt64(out long int64) => XmlDaValue.OfInt64(int64),
        JsonValueKind.Number when element.TryGetDouble(out double dbl) => XmlDaValue.OfDouble(dbl),
        JsonValueKind.String => StringToXmlDaValue(element.GetString()),
        _ => XmlDaValue.OfString(element.GetRawText()),
    };

    private static XmlDaValue ToXmlDaValueByType(object? value, string valueType)
    {
        string text = value is JsonElement element && element.ValueKind == JsonValueKind.String
            ? element.GetString() ?? string.Empty
            : value?.ToString() ?? string.Empty;
        return valueType.ToLowerInvariant() switch
        {
            "string" or "xsd:string" => XmlDaValue.OfString(text),
            "bool" or "boolean" or "xsd:boolean" => XmlDaValue.OfBoolean(bool.Parse(text)),
            "int" or "int32" or "xsd:int" => XmlDaValue.OfInt32(int.Parse(text, CultureInfo.InvariantCulture)),
            "long" or "int64" or "xsd:long" => XmlDaValue.OfInt64(long.Parse(text, CultureInfo.InvariantCulture)),
            "float" or "single" or "xsd:float" => XmlDaValue.OfSingle(float.Parse(text, CultureInfo.InvariantCulture)),
            "double" or "xsd:double" => XmlDaValue.OfDouble(double.Parse(text, CultureInfo.InvariantCulture)),
            "decimal" or "xsd:decimal" => XmlDaValue.OfDecimal(decimal.Parse(text, CultureInfo.InvariantCulture)),
            "datetime" or "dateTime" or "xsd:datetime" or "xsd:dateTime" => XmlDaValue.OfDateTime(DateTimeOffset.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal)),
            _ => ToXmlDaValue(value),
        };
    }

    private static XmlDaValue StringToXmlDaValue(string? text)
    {
        if (text is null)
        {
            return XmlDaValue.OfString(string.Empty);
        }

        if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset dateTime))
        {
            return XmlDaValue.OfDateTime(dateTime);
        }

        return XmlDaValue.OfString(text);
    }

    private static object? NormalizeValue(XmlDaValue? value) => value is null ? null : NormalizeValue(value.Boxed);

    private static object? NormalizeValue(object? value) => value switch
    {
        DateOnly date => date.ToString("O", CultureInfo.InvariantCulture),
        TimeOnly time => time.ToString("HH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture),
        TimeSpan duration => duration.ToString(),
        byte[] bytes => Convert.ToBase64String(bytes),
        sbyte[] bytes => bytes.Select(static b => (int)b).ToArray(),
        _ => value,
    };

    private sealed class DefaultOpcXmlDaConnectionFactory : IOpcXmlDaConnectionFactory
    {
        public Task<XmlDaClientState> ConnectAsync(XmlDaConnectionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            if (TryGetInMemoryKey(request.EndpointUrl, out string? key))
            {
                if (!InMemoryXmlDaConnectionRegistry.TryGet(key, out IXmlDaClient client))
                {
                    throw new McpException($"No in-memory XML-DA client is registered for '{key}'.");
                }

                return Task.FromResult(new XmlDaClientState("inmemory://" + key, client));
            }

            if (!Uri.TryCreate(request.EndpointUrl, UriKind.Absolute, out Uri? endpoint) || (endpoint.Scheme != Uri.UriSchemeHttp && endpoint.Scheme != Uri.UriSchemeHttps))
            {
                throw new McpException("XML-DA connect requires an absolute http:// or https:// endpoint URL, or inmemory://name for tests.");
            }

            var http = new HttpClient();
            var clientState = new XmlDaClientState(endpoint.ToString(), new HttpXmlDaClient(http, endpoint), ownedDisposable: http);
            return Task.FromResult(clientState);
        }

        private static bool TryGetInMemoryKey(string? endpointUrl, out string key)
        {
            key = string.Empty;
            if (string.IsNullOrWhiteSpace(endpointUrl))
            {
                return false;
            }

            if (Uri.TryCreate(endpointUrl, UriKind.Absolute, out Uri? uri)
                && uri.Scheme.Equals("inmemory", StringComparison.OrdinalIgnoreCase))
            {
                key = uri.Host + uri.AbsolutePath.Trim('/');
                return !string.IsNullOrWhiteSpace(key);
            }

            const string prefix = "inmemory:";
            if (endpointUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                key = endpointUrl[prefix.Length..].Trim('/');
                return !string.IsNullOrWhiteSpace(key);
            }

            return false;
        }
    }
}
