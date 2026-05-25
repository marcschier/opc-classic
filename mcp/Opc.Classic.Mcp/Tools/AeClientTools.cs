//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Discovery;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Ndr;
using Opc.Classic.Transport;

namespace Opc.Classic.Mcp.Tools;

/// <summary>Creates AE client state for a session.</summary>
public interface IOpcAeConnectionFactory
{
    /// <summary>Connects to an AE server and returns a client state object.</summary>
    Task<AeClientState> ConnectAsync(AeConnectionRequest request, CancellationToken cancellationToken = default);
}

/// <summary>Connection request used by AE tools.</summary>
public sealed record AeConnectionRequest(
    string Host,
    string? ProgId,
    string? Clsid,
    string? Username,
    string? Password,
    bool UseKerberos,
    string? ConnectionString);

/// <summary>Registers in-memory AE call channels for MCP tests and loopback scenarios.</summary>
public static class InMemoryAeConnectionRegistry
{
    private static readonly ConcurrentDictionary<string, InMemoryAeConnection> Channels = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers an in-memory AE call channel by name.</summary>
    public static IDisposable Register(string name, ICallChannel channel, IAeServer? managedServer = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(channel);

        Channels[name] = new InMemoryAeConnection(channel, managedServer);
        return new Registration(name);
    }

    internal static bool TryGet(string name, out InMemoryAeConnection connection) => Channels.TryGetValue(name, out connection!);

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

/// <summary>In-memory AE connection endpoints.</summary>
public sealed record InMemoryAeConnection(ICallChannel Channel, IAeServer? ManagedServer);

/// <summary>MCP tools for OPC AE client operations.</summary>
public sealed class AeClientTools
{
    private readonly IOpcSessionManager _sessionManager;
    private readonly IOpcAeConnectionFactory _connectionFactory;

    /// <summary>Creates the AE client tool set.</summary>
    public AeClientTools(IOpcSessionManager sessionManager, IEnumerable<IOpcAeConnectionFactory> connectionFactories)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        ArgumentNullException.ThrowIfNull(connectionFactories);
        _connectionFactory = connectionFactories.FirstOrDefault() ?? new DefaultOpcAeConnectionFactory();
    }

    /// <summary>Connects a session to an OPC AE server.</summary>
    [McpServerTool(Name = "opcclassic.ae.connect", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Connects an existing MCP session to an OPC AE server using DCOM or an in-memory test channel.")]
    public async Task<OpcResultDto> Connect(
        [Description("The sessionId returned by opcclassic.session.create.")]
        string sessionId,
        [Description("OPC AE server host name or IP address. Ignored when connectionString uses inmemory://.")]
        string host = "localhost",
        [Description("OPC AE server ProgID. Optional when clsid or connectionString is supplied.")]
        string? progId = null,
        [Description("OPC AE server CLSID as a GUID string. Optional when progId or connectionString is supplied.")]
        string? clsid = null,
        [Description("Optional user name for NTLMv2 or Kerberos authentication. Use DOMAIN\\user when a Windows domain is required.")]
        string? username = null,
        [Description("Optional password for NTLMv2 or Kerberos authentication. Omit only for anonymous or in-memory connections.")]
        string? password = null,
        [Description("True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied.")]
        bool useKerberos = false,
        [Description("Optional connection string. Use inmemory://name for a registered InMemoryCallChannel, or opcae://host/ProgID for DCOM.")]
        string? connectionString = null,
        CancellationToken cancellationToken = default)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        AeClientState client = await _connectionFactory.ConnectAsync(
            new AeConnectionRequest(host, progId, clsid, username, password, useKerberos, connectionString),
            cancellationToken).ConfigureAwait(false);

        AeClientState? existing = session.AeClient;
        session.AeClient = client;
        if (existing is not null)
        {
            await existing.DisposeAsync().ConfigureAwait(false);
        }

        OpcServerStatus status = await GetStatusAsync(client, cancellationToken).ConfigureAwait(false);
        session.Touch();
        return new OpcResultDto(0, $"AE client connected to {status.VendorInfo}.", Succeeded: true);
    }

    /// <summary>Gets OPC AE server status for a connected session.</summary>
    [McpServerTool(Name = "opcclassic.ae.get_status", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Gets the OPC AE event server status, including runtime state, version, vendor information, and operational state.")]
    public async Task<OpcServerStatusDto> GetStatus(
        [Description("The sessionId returned by opcclassic.session.create and connected with opcclassic.ae.connect.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        AeClientState client = GetAeClient(sessionId);
        OpcServerStatus status = await GetStatusAsync(client, cancellationToken).ConfigureAwait(false);
        return ToStatusDto(status);
    }

    /// <summary>Browses the OPC AE area space.</summary>
    [McpServerTool(Name = "opcclassic.ae.browse_areas", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Browses the OPC AE area/source tree below a qualified area name. Use an empty area for the root.")]
    public async Task<IReadOnlyList<OpcAreaBrowseElementDto>> BrowseAreas(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Qualified area name to browse below. Use an empty string for the root.")]
        string areaQualifiedName = "",
        CancellationToken cancellationToken = default)
    {
        AeClientState client = GetAeClient(sessionId);
        if (client.ManagedServer is null)
        {
            return [];
        }

        var elements = new List<OpcAreaBrowseElementDto>();
        await foreach (AreaBrowseElement element in client.ManagedServer.BrowseAreasAsync(areaQualifiedName ?? string.Empty, cancellationToken).ConfigureAwait(false))
        {
            elements.Add(new OpcAreaBrowseElementDto(element.Name, element.QualifiedName, element.IsArea, element.IsSource));
        }

        return elements;
    }

    /// <summary>Queries OPC AE event categories.</summary>
    [McpServerTool(Name = "opcclassic.ae.query_event_categories", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Queries event categories supported by the AE server for simple, tracking, condition, or all event types.")]
    public async Task<IReadOnlyList<OpcEventCategoryDto>> QueryEventCategories(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Event type filter: all, simple, tracking, condition, or a comma-separated combination.")]
        string eventTypes = "all",
        CancellationToken cancellationToken = default)
    {
        AeClientState client = GetAeClient(sessionId);
        EventType parsed = ParseEventTypes(eventTypes);
        try
        {
            await client.EventServer.QueryEventCategoriesAsync((int)parsed, out int[] categories, out string[] descriptions, cancellationToken).ConfigureAwait(false);
            return categories.Select((category, index) => new OpcEventCategoryDto(
                category,
                index < descriptions.Length ? descriptions[index] : string.Empty,
                parsed.ToString())).ToArray();
        }
        catch (OpcException ex) when (client.ManagedServer is not null && ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            IReadOnlyList<uint> categories = await client.ManagedServer.QueryEventCategoriesAsync(parsed, cancellationToken).ConfigureAwait(false);
            return categories.Select(category => new OpcEventCategoryDto(unchecked((int)category), string.Empty, parsed.ToString())).ToArray();
        }
    }

    /// <summary>Queries OPC AE event attributes for a category.</summary>
    [McpServerTool(Name = "opcclassic.ae.query_event_attributes", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Queries server-defined attribute metadata for an OPC AE event category.")]
    public async Task<IReadOnlyList<OpcEventAttributeDto>> QueryEventAttributes(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Server-defined event category ID returned by opcclassic.ae.query_event_categories.")]
        int eventCategory,
        CancellationToken cancellationToken = default)
    {
        AeClientState client = GetAeClient(sessionId);
        await client.EventServer.QueryEventAttributesAsync(eventCategory, out int[] ids, out string[] descriptions, out ushort[] types, cancellationToken).ConfigureAwait(false);
        return ids.Select((id, index) => new OpcEventAttributeDto(
            id,
            index < descriptions.Length ? descriptions[index] : string.Empty,
            index < types.Length ? types[index] : (ushort)VarType.VT_EMPTY,
            ((VarType)(index < types.Length ? types[index] : (ushort)VarType.VT_EMPTY)).ToString())).ToArray();
    }

    /// <summary>Creates an OPC AE event subscription.</summary>
    [McpServerTool(Name = "opcclassic.ae.create_subscription", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Creates a poll-based AE subscription. MCP cannot push callbacks, so use opcclassic.ae.poll_events to retrieve queued events.")]
    public async Task<OpcAeSubscriptionDto> CreateSubscription(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("True to make the subscription active immediately.")]
        bool active = true,
        [Description("Requested event buffer time in milliseconds.")]
        int bufferTimeMs = 1000,
        [Description("Requested maximum event buffer size. Use 0 for the server default.")]
        int maxBufferSize = 0,
        [Description("Client subscription handle echoed by callbacks. Defaults to a generated positive handle when 0.")]
        int clientSubscription = 0,
        CancellationToken cancellationToken = default)
    {
        AeClientState client = GetAeClient(sessionId);
        string subscriptionId = Guid.NewGuid().ToString("N");
        int clientHandle = clientSubscription == 0 ? Environment.TickCount & int.MaxValue : clientSubscription;
        IAeSubscription? managedSubscription = null;
        int revisedBuffer = bufferTimeMs;
        int revisedMax = maxBufferSize;

        if (client.ManagedServer is not null)
        {
            managedSubscription = await client.ManagedServer.CreateSubscriptionAsync(active, bufferTimeMs, maxBufferSize, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await client.EventServer.CreateEventSubscriptionAsync(
                active,
                bufferTimeMs,
                maxBufferSize,
                clientHandle,
                IOPCEventSubscriptionMgt.InterfaceId,
                out _,
                out revisedBuffer,
                out revisedMax,
                cancellationToken).ConfigureAwait(false);
        }

        var context = new AeSubscriptionContext(subscriptionId, clientHandle, active, bufferTimeMs, maxBufferSize, revisedBuffer, revisedMax, managedSubscription);
        client.Subscriptions[subscriptionId] = context;
        if (managedSubscription is not null)
        {
            context.PumpTask = Task.Run(() => PumpEventsAsync(context), CancellationToken.None);
        }

        return ToSubscriptionDto(context);
    }

    /// <summary>Sets an AE subscription filter.</summary>
    [McpServerTool(Name = "opcclassic.ae.set_filter", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Sets an AE subscription filter using event type, category, severity, area, and source criteria.")]
    public async Task<OpcAeSubscriptionDto> SetFilter(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Subscription identifier returned by opcclassic.ae.create_subscription.")]
        string subscriptionId,
        [Description("Event type filter: all, simple, tracking, condition, or a comma-separated combination.")]
        string eventTypes = "all",
        [Description("Optional event category IDs to include. Empty means all categories.")]
        int[]? eventCategories = null,
        [Description("Minimum severity to include, from 0 to 1000.")]
        int minSeverity = 0,
        [Description("Maximum severity to include, from 0 to 1000.")]
        int maxSeverity = 1000,
        [Description("Optional qualified areas to include. Empty means all areas.")]
        string[]? areas = null,
        [Description("Optional source names to include. Empty means all sources.")]
        string[]? sources = null,
        CancellationToken cancellationToken = default)
    {
        AeClientState client = GetAeClient(sessionId);
        AeSubscriptionContext context = GetSubscription(client, subscriptionId);
        var filter = new SubscriptionFilter
        {
            EventTypes = ParseEventTypes(eventTypes),
            EventCategories = (eventCategories ?? []).Select(static category => unchecked((uint)category)).ToArray(),
            MinSeverity = minSeverity,
            MaxSeverity = maxSeverity,
            Areas = areas ?? [],
            Sources = sources ?? [],
        };
        context.Filter = filter;

        if (context.Subscription is not null)
        {
            await context.Subscription.SetFilterAsync(filter, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await client.SubscriptionMgt.SetFilterAsync(
                (int)filter.EventTypes,
                filter.EventCategories.Select(static category => unchecked((int)category)).ToArray(),
                filter.MinSeverity,
                filter.MaxSeverity,
                filter.Areas.ToArray(),
                filter.Sources.ToArray(),
                cancellationToken).ConfigureAwait(false);
        }

        return ToSubscriptionDto(context);
    }

    /// <summary>Polls queued AE event notifications.</summary>
    [McpServerTool(Name = "opcclassic.ae.poll_events", ReadOnly = true, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Polls a subscription queue for AE notifications. MCP cannot receive pushed callbacks directly.")]
    public async Task<IReadOnlyList<OpcEventNotificationDto>> PollEvents(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Subscription identifier returned by opcclassic.ae.create_subscription.")]
        string subscriptionId,
        [Description("Maximum notifications to return. Use 0 for all queued notifications.")]
        int maxNotifications = 0,
        [Description("Milliseconds to wait for at least one event when the queue is empty.")]
        int waitMilliseconds = 100,
        CancellationToken cancellationToken = default)
    {
        AeSubscriptionContext context = GetSubscription(GetAeClient(sessionId), subscriptionId);
        if (waitMilliseconds > 0 && context.Events.Reader.Count == 0)
        {
            using var waitCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, context.Cancellation.Token);
            waitCts.CancelAfter(TimeSpan.FromMilliseconds(waitMilliseconds));
            try
            {
                _ = await context.Events.Reader.WaitToReadAsync(waitCts.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (waitCts.IsCancellationRequested)
            {
            }
        }

        int limit = maxNotifications <= 0 ? int.MaxValue : maxNotifications;
        var events = new List<OpcEventNotificationDto>();
        while (events.Count < limit && context.Events.Reader.TryRead(out OpcEventNotification? notification))
        {
            events.Add(ToEventDto(notification));
        }

        return events;
    }

    /// <summary>Refreshes an AE subscription.</summary>
    [McpServerTool(Name = "opcclassic.ae.refresh_subscription", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Triggers an AE condition refresh so active conditions are re-emitted to the subscription queue.")]
    public async Task<OpcResultDto> RefreshSubscription(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Subscription identifier returned by opcclassic.ae.create_subscription.")]
        string subscriptionId,
        CancellationToken cancellationToken = default)
    {
        AeClientState client = GetAeClient(sessionId);
        AeSubscriptionContext context = GetSubscription(client, subscriptionId);
        if (context.Subscription is not null)
        {
            await context.Subscription.RefreshAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await client.SubscriptionMgt.RefreshAsync(context.ClientSubscription, cancellationToken).ConfigureAwait(false);
        }

        return new OpcResultDto(0, $"AE subscription '{subscriptionId}' refreshed.", Succeeded: true, SubscriptionId: subscriptionId);
    }

    /// <summary>Acknowledges an AE condition.</summary>
    [McpServerTool(Name = "opcclassic.ae.ack_condition", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Acknowledges an AE condition by source and condition name. For DCOM servers, activeTime and cookie identify the event instance.")]
    public async Task<IReadOnlyList<OpcResultDto>> AckCondition(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Event source name that owns the condition.")]
        string source,
        [Description("Condition name to acknowledge.")]
        string conditionName,
        [Description("Acknowledging actor or operator ID.")]
        string actor = "mcp",
        [Description("Optional acknowledgement comment.")]
        string? comment = null,
        [Description("Optional active time for DCOM acknowledgements. Use the event ActiveTime returned by poll_events.")]
        DateTimeOffset? activeTime = null,
        [Description("Optional AE cookie for DCOM acknowledgements. Use the event Cookie returned by poll_events.")]
        int cookie = 0,
        CancellationToken cancellationToken = default)
    {
        AeClientState client = GetAeClient(sessionId);
        if (client.ManagedServer is not null)
        {
            IReadOnlyList<AckResult> results = await client.ManagedServer.AcknowledgeAsync(actor, comment, [new ConditionRef(source, conditionName)], cancellationToken).ConfigureAwait(false);
            return results.Select(static result => new OpcResultDto(result.ResultId.Code, DescribeHResult(result.ResultId.Code), result.ResultId.IsSuccess, ItemName: result.Condition.ToString())).ToArray();
        }

        long activeFileTime = (activeTime ?? DateTimeOffset.UnixEpoch).ToFileTime();
        int[] errors = await client.EventServer.AckConditionAsync(
            actor,
            comment ?? string.Empty,
            [activeFileTime],
            [cookie],
            [source],
            [conditionName],
            cancellationToken).ConfigureAwait(false);
        return errors.Select(error => new OpcResultDto(error, DescribeHResult(error), new OpcResultId(error, null).IsSuccess, ItemName: source + "::" + conditionName)).ToArray();
    }

    /// <summary>Gets current state for an AE condition.</summary>
    [McpServerTool(Name = "opcclassic.ae.get_condition_state", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Gets the current server state for a named AE condition and optional attribute IDs.")]
    public async Task<OpcConditionStateDto> GetConditionState(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Event source name that owns the condition.")]
        string source,
        [Description("Condition name to inspect.")]
        string conditionName,
        [Description("Optional event attribute IDs whose current values should be returned.")]
        int[]? attributeIds = null,
        CancellationToken cancellationToken = default)
    {
        AeClientState client = GetAeClient(sessionId);
        OpcConditionState state = await client.EventServer.GetConditionStateAsync(source, conditionName, attributeIds ?? [], cancellationToken).ConfigureAwait(false);
        return ToConditionStateDto(state, attributeIds ?? []);
    }

    /// <summary>Cancels an AE subscription.</summary>
    [McpServerTool(Name = "opcclassic.ae.cancel_subscription", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Cancels and removes an AE subscription from the MCP session.")]
    public async Task<OpcResultDto> CancelSubscription(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Subscription identifier returned by opcclassic.ae.create_subscription.")]
        string subscriptionId)
    {
        AeClientState client = GetAeClient(sessionId);
        if (!client.Subscriptions.TryRemove(subscriptionId, out AeSubscriptionContext? context))
        {
            return new OpcResultDto(1, $"AE subscription '{subscriptionId}' was not found.", Succeeded: false, SubscriptionId: subscriptionId);
        }

        await context.DisposeAsync().ConfigureAwait(false);
        return new OpcResultDto(0, $"AE subscription '{subscriptionId}' canceled.", Succeeded: true, SubscriptionId: subscriptionId);
    }

    /// <summary>Disconnects from an AE server.</summary>
    [McpServerTool(Name = "opcclassic.ae.disconnect", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Disconnects the session from its OPC AE server and releases AE subscriptions and channels.")]
    public async Task<OpcResultDto> Disconnect(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        AeClientState? client = session.AeClient;
        session.AeClient = null;
        if (client is not null)
        {
            await client.DisposeAsync().ConfigureAwait(false);
            return new OpcResultDto(0, "AE client disconnected.", Succeeded: true);
        }

        return new OpcResultDto(1, "AE client was not connected.", Succeeded: false);
    }

    private AeClientState GetAeClient(string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        return session.AeClient ?? throw new McpException($"Session '{sessionId}' is not connected to an OPC AE server. Call opcclassic.ae.connect first.");
    }

    private static AeSubscriptionContext GetSubscription(AeClientState client, string subscriptionId) =>
        client.Subscriptions.TryGetValue(subscriptionId, out AeSubscriptionContext? subscription)
            ? subscription
            : throw new McpException($"AE subscription '{subscriptionId}' was not found.");

    private static async Task PumpEventsAsync(AeSubscriptionContext context)
    {
        try
        {
            await foreach (EventNotification notification in context.Subscription!.Events.WithCancellation(context.Cancellation.Token).ConfigureAwait(false))
            {
                await context.Events.Writer.WriteAsync(ToRawNotification(notification), context.Cancellation.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static async Task<OpcServerStatus> GetStatusAsync(AeClientState client, CancellationToken cancellationToken) =>
        client.ManagedServer is not null
            ? await client.ManagedServer.GetStatusAsync(cancellationToken).ConfigureAwait(false)
            : await client.EventServer.GetStatusAsync(cancellationToken).ConfigureAwait(false);

    private static OpcAeSubscriptionDto ToSubscriptionDto(AeSubscriptionContext context) =>
        new(
            context.SubscriptionId,
            context.ClientSubscription,
            context.Active,
            context.BufferTimeMs,
            context.MaxBufferSize,
            context.RevisedBufferTimeMs,
            context.RevisedMaxBufferSize,
            context.Events.Reader.Count);

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

    private static OpcEventNotification ToRawNotification(EventNotification notification)
    {
        OpcVariant[] attributes = notification.Attributes.Values.Select(ToVariantObject).ToArray();
        return new OpcEventNotification(
            changeMask: 0,
            newState: unchecked((ushort)notification.NewState),
            notification.Source,
            notification.Time,
            notification.Message,
            unchecked((uint)notification.EventType),
            notification.EventCategory,
            unchecked((uint)Math.Max(0, notification.Severity)),
            notification.ConditionName,
            notification.SubConditionName,
            notification.Quality,
            notification.AckRequired,
            notification.ActiveTime,
            unchecked((uint)notification.Cookie),
            attributes,
            notification.Actor);
    }

    private static OpcEventNotificationDto ToEventDto(OpcEventNotification notification) =>
        new(
            notification.ChangeMask,
            notification.NewState,
            notification.Source,
            notification.Time,
            notification.Message,
            notification.EventType,
            ((EventType)notification.EventType).ToString(),
            notification.EventCategory,
            notification.Severity,
            notification.ConditionName,
            notification.SubconditionName,
            notification.Quality.RawValue,
            notification.Quality.ToString(),
            notification.AckRequired,
            notification.ActiveTime,
            notification.Cookie,
            notification.ActorId,
            ToAttributeDtos(notification.EventAttributes, null, null));

    private static OpcConditionStateDto ToConditionStateDto(OpcConditionState state, IReadOnlyList<int> attributeIds) =>
        new(
            state.State,
            ((ConditionState)state.State).ToString(),
            state.ActiveSubCondition,
            state.ActiveSubConditionDefinition,
            state.ActiveSubConditionSeverity,
            state.ActiveSubConditionDescription,
            state.Quality.RawValue,
            state.Quality.ToString(),
            state.LastAckTime,
            state.SubConditionLastActive,
            state.ConditionLastActive,
            state.ConditionLastInactive,
            state.AcknowledgerId,
            state.Comment,
            state.SubConditionNames,
            state.SubConditionDefinitions,
            state.SubConditionSeverities,
            state.SubConditionDescriptions,
            ToAttributeDtos(state.EventAttributes, attributeIds, state.Errors));

    private static IReadOnlyList<OpcEventAttributeValueDto> ToAttributeDtos(IReadOnlyList<OpcVariant> values, IReadOnlyList<int>? attributeIds, IReadOnlyList<int>? errors)
    {
        var results = new List<OpcEventAttributeValueDto>(values.Count);
        for (int i = 0; i < values.Count; i++)
        {
            int hresult = errors is not null && i < errors.Count ? errors[i] : OpcResultId.Ok.Code;
            results.Add(new OpcEventAttributeValueDto(
                attributeIds is not null && i < attributeIds.Count ? attributeIds[i] : null,
                NormalizeValue(OpcVariantConverter.ToObject(values[i])),
                values[i].Type.ToString(),
                hresult,
                DescribeHResult(hresult)));
        }

        return results;
    }

    private static EventType ParseEventTypes(string? eventTypes)
    {
        if (string.IsNullOrWhiteSpace(eventTypes) || eventTypes.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return EventType.All;
        }

        EventType result = EventType.None;
        foreach (string token in eventTypes.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            result |= token.ToLowerInvariant() switch
            {
                "simple" => EventType.Simple,
                "tracking" => EventType.Tracking,
                "condition" or "conditions" => EventType.Condition,
                "all" => EventType.All,
                _ when Enum.TryParse(token, ignoreCase: true, out EventType parsed) => parsed,
                _ => throw new ArgumentException($"Unknown AE event type '{token}'.", nameof(eventTypes)),
            };
        }

        return result == EventType.None ? EventType.All : result;
    }

    private static OpcVariant ToVariantObject(object? value)
    {
        try
        {
            return value is JsonElement element ? ToVariant(element) : OpcVariantConverter.FromObject(NormalizeInputValue(value));
        }
        catch (ArgumentException)
        {
            return OpcVariant.FromString(value?.ToString() ?? string.Empty);
        }
    }

    private static object? NormalizeInputValue(object? value) => value switch
    {
        DateTimeOffset dto => dto.UtcDateTime,
        _ => value,
    };

    internal static OpcVariant ToVariant(JsonElement value) => value.ValueKind switch
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

    internal static OpcVariant StringToVariant(string? value)
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

    internal static object? NormalizeValue(object? value) => value switch
    {
        DateTime dateTime => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
        DateTimeOffset dateTimeOffset => dateTimeOffset.UtcDateTime,
        OpcVariant variant => NormalizeValue(OpcVariantConverter.ToObject(variant)),
        OpcSafeArray safeArray => safeArray.ToString(),
        _ => value,
    };

    internal static string DescribeHResult(int hresult) => hresult switch
    {
        0 => "S_OK",
        1 => "S_FALSE",
        _ => new OpcResultId(hresult, null).ToString(),
    };

    private sealed class DefaultOpcAeConnectionFactory : IOpcAeConnectionFactory
    {
        public async Task<AeClientState> ConnectAsync(AeConnectionRequest request, CancellationToken cancellationToken = default)
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
                "opcae");

            string? inMemoryKey = OpcMcpDcomConnectionHelper.TryGetInMemoryKey(normalized.ConnectionString);
            if (inMemoryKey is not null)
            {
                if (!InMemoryAeConnectionRegistry.TryGet(inMemoryKey, out InMemoryAeConnection connection))
                {
                    throw new McpException($"No in-memory AE channel is registered for '{inMemoryKey}'.");
                }

                return new AeClientState("inmemory", normalized.ProgId ?? inMemoryKey, Guid.Empty, connection.Channel, ownsChannel: false, connection.ManagedServer);
            }

            (ICallChannel channel, Guid clsid) = await OpcMcpDcomConnectionHelper.ConnectDcomAsync(
                normalized,
                IOPCEventServer.InterfaceId,
                [OpcGuids.CATID_OPCAEServer10],
                "opcae",
                cancellationToken).ConfigureAwait(false);
            return new AeClientState(normalized.Host, normalized.ProgId, clsid, channel, ownsChannel: true);
        }
    }
}

internal sealed record OpcMcpDcomConnectionRequest(
    string Host,
    string? ProgId,
    string? Clsid,
    string? Username,
    string? Password,
    bool UseKerberos,
    string? ConnectionString);

internal static class OpcMcpDcomConnectionHelper
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

    public static OpcMcpDcomConnectionRequest NormalizeRequest(
        string host,
        string? progId,
        string? clsid,
        string? username,
        string? password,
        bool useKerberos,
        string? connectionString,
        string opcScheme)
    {
        string normalizedHost = string.IsNullOrWhiteSpace(host) ? "localhost" : host.Trim();
        string? normalizedProgId = NormalizeText(progId);
        string? normalizedClsid = NormalizeText(clsid);
        string? normalizedConnectionString = NormalizeText(connectionString);
        if (normalizedConnectionString is not null && Uri.TryCreate(normalizedConnectionString, UriKind.Absolute, out Uri? uri))
        {
            if (uri.Scheme.Equals("inmemory", StringComparison.OrdinalIgnoreCase))
            {
                return new OpcMcpDcomConnectionRequest(normalizedHost, normalizedProgId, normalizedClsid, username, password, useKerberos, normalizedConnectionString);
            }

            if (uri.Scheme.Equals("dcom", StringComparison.OrdinalIgnoreCase) || uri.Scheme.Equals(opcScheme, StringComparison.OrdinalIgnoreCase))
            {
                normalizedHost = string.IsNullOrWhiteSpace(uri.Host) ? normalizedHost : uri.Host;
                string pathValue = uri.AbsolutePath.Trim('/');
                if (!string.IsNullOrWhiteSpace(pathValue))
                {
                    if (Guid.TryParse(pathValue, out _))
                    {
                        normalizedClsid = pathValue;
                    }
                    else
                    {
                        normalizedProgId = pathValue;
                    }
                }
            }
        }

        return new OpcMcpDcomConnectionRequest(normalizedHost, normalizedProgId, normalizedClsid, username, password, useKerberos, normalizedConnectionString);
    }

    public static string? TryGetInMemoryKey(string? connectionString)
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

    public static async Task<(ICallChannel Channel, Guid Clsid)> ConnectDcomAsync(
        OpcMcpDcomConnectionRequest request,
        Guid requestedIid,
        Guid[] categoryIds,
        string opcScheme,
        CancellationToken cancellationToken)
    {
        Guid clsid = await ResolveClsidAsync(request, categoryIds, cancellationToken).ConfigureAwait(false);
        var channelFactory = new DcomCallChannelFactory(new TcpSocketTransportFactory());
        ICallChannel? activationChannel = null;
        try
        {
            activationChannel = await channelFactory.ConnectAsync(
                new DnsEndPoint(request.Host, EndpointMapperPort),
                clsid,
                CreateAuthContext(request, clsid, opcScheme),
                cancellationToken).ConfigureAwait(false);
            byte[] payload = EncodeRemoteCreateInstanceRequest(request.Host, clsid, requestedIid);
            NdrCallResult activationResult = await activationChannel.InvokeAsync(
                RemoteScmActivatorInterfaceId,
                RemoteCreateInstanceOpnum,
                payload,
                cancellationToken).ConfigureAwait(false);
            IOpcInterfaceRef serverRef = DecodeRemoteCreateInstanceResponse(activationResult);
            EndPoint endpoint = ResolveObjectEndpoint(request.Host, serverRef);
            ICallChannel serverChannel = await channelFactory.ConnectAsync(
                endpoint,
                Guid.Empty,
                CreateAuthContext(request, clsid, opcScheme),
                cancellationToken).ConfigureAwait(false);
            return (serverChannel, clsid);
        }
        finally
        {
            await DisposeChannelAsync(activationChannel).ConfigureAwait(false);
        }
    }

    private static async Task<Guid> ResolveClsidAsync(OpcMcpDcomConnectionRequest request, Guid[] categoryIds, CancellationToken cancellationToken)
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
            throw new McpException("Provide an OPC server ProgID, CLSID, or connectionString.");
        }

        OpcServerDescriptor[] servers = await OpcDiscovery.EnumerateAsync(
            request.Host,
            categoryIds,
            cancellationToken).ConfigureAwait(false);
        OpcServerDescriptor? match = servers.FirstOrDefault(server =>
            string.Equals(server.ProgId, request.ProgId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(server.VerIndProgId, request.ProgId, StringComparison.OrdinalIgnoreCase));
        return match?.ClassId ?? throw new McpException($"OPC ProgID '{request.ProgId}' was not found on host '{request.Host}'.");
    }

    private static IAuthContext CreateAuthContext(OpcMcpDcomConnectionRequest request, Guid clsid, string opcScheme)
    {
        NetworkCredential? credentials = CreateCredential(request.Username, request.Password);
        OpcUrl url = OpcUrl.Parse($"{opcScheme}://{request.Host}/{(request.ProgId ?? clsid.ToString("D"))}");
        OpcConnectData connectData = credentials is null
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
        var activationProperties = new ActivationProperties(
            new SpecialPropertiesData(ActivationComVersion.V5_6, Mode: 0, ClassContext, requestedIid, Array.Empty<int>()),
            new InstanceInfo(clsid, requestedIid, ClassContext, Mode: 0),
            new LocationInfo(host, Environment.ProcessId, new[] { RpcProtocolSequenceTcp }),
            null,
            new SecurityInfo(AuthenticationLevel: 0, ImpersonationLevel: 3, Capabilities: 0));
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
            throw new InvalidOperationException("RemoteCreateInstance did not return an OPC OBJREF.");
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

        throw new InvalidOperationException("RemoteCreateInstance returned an invalid OPC OBJREF.");
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
