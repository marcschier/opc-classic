// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Mcp.Tools;
using Opc.Classic.Testing;

namespace Opc.Classic.Mcp.Tests;

public sealed class AeClientToolsTests
{
    [Test]
    public async Task Ae_connect_status_browse_and_disconnect_round_trip_via_mcp_client()
    {
        var syntheticAe = new SyntheticAeServer();
        string channelName = "ae-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryAeConnectionRegistry.Register(channelName, syntheticAe.Channel, syntheticAe);
        await using McpAeHdaTestServer server = await McpAeHdaTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);

        OpcResultDto connected = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.ae.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);
        OpcServerStatusDto status = await server.CallToolAsync<OpcServerStatusDto>(
            "opcclassic.ae.get_status",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);
        OpcAreaBrowseElementDto[] browse = await server.CallToolAsync<OpcAreaBrowseElementDto[]>(
            "opcclassic.ae.browse_areas",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);
        OpcResultDto disconnected = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.ae.disconnect",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);

        await Assert.That(connected.Succeeded).IsTrue();
        await Assert.That(status.Spec).IsEqualTo("Ae");
        await Assert.That(status.VendorInfo).IsEqualTo("Synthetic MCP AE Server");
        await Assert.That(browse.Select(static element => element.QualifiedName)).Contains("Plant1.AreaA");
        await Assert.That(disconnected.Succeeded).IsTrue();
    }

    [Test]
    public async Task Ae_query_categories_attributes_and_condition_state_via_mcp_client()
    {
        var syntheticAe = new SyntheticAeServer();
        string channelName = "ae-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryAeConnectionRegistry.Register(channelName, syntheticAe.Channel, syntheticAe);
        await using McpAeHdaTestServer server = await McpAeHdaTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.ae.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);

        OpcEventCategoryDto[] categories = await server.CallToolAsync<OpcEventCategoryDto[]>(
            "opcclassic.ae.query_event_categories",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId, ["eventTypes"] = "all" }).ConfigureAwait(false);
        OpcEventAttributeDto[] attributes = await server.CallToolAsync<OpcEventAttributeDto[]>(
            "opcclassic.ae.query_event_attributes",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId, ["eventCategory"] = 0x1002 }).ConfigureAwait(false);
        OpcConditionStateDto condition = await server.CallToolAsync<OpcConditionStateDto>(
            "opcclassic.ae.get_condition_state",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["source"] = "Plant1.AreaA.Tank7",
                ["conditionName"] = "LevelHigh",
                ["attributeIds"] = new[] { 10, 11 },
            }).ConfigureAwait(false);

        await Assert.That(categories.Select(static category => category.EventCategory)).Contains(0x1002);
        await Assert.That(attributes.Select(static attribute => attribute.Description)).Contains("Limit");
        await Assert.That(condition.ActiveSubCondition).IsEqualTo("HiHi");
        await Assert.That(condition.EventAttributes.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Ae_subscription_filter_poll_refresh_and_cancel_via_mcp_client()
    {
        var syntheticAe = new SyntheticAeServer();
        string channelName = "ae-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryAeConnectionRegistry.Register(channelName, syntheticAe.Channel, syntheticAe);
        await using McpAeHdaTestServer server = await McpAeHdaTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.ae.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);

        OpcAeSubscriptionDto subscription = await server.CallToolAsync<OpcAeSubscriptionDto>(
            "opcclassic.ae.create_subscription",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId, ["bufferTimeMs"] = 50, ["maxBufferSize"] = 10 }).ConfigureAwait(false);
        OpcAeSubscriptionDto filtered = await server.CallToolAsync<OpcAeSubscriptionDto>(
            "opcclassic.ae.set_filter",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["subscriptionId"] = subscription.SubscriptionId,
                ["eventTypes"] = "condition",
                ["eventCategories"] = new[] { 0x1002 },
                ["minSeverity"] = 100,
                ["maxSeverity"] = 1000,
                ["sources"] = new[] { "Plant1.AreaA.Tank7" },
            }).ConfigureAwait(false);
        OpcEventNotificationDto[] initialEvents = await server.CallToolAsync<OpcEventNotificationDto[]>(
            "opcclassic.ae.poll_events",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["subscriptionId"] = subscription.SubscriptionId,
                ["waitMilliseconds"] = 1000,
            }).ConfigureAwait(false);
        OpcResultDto refreshed = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.ae.refresh_subscription",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["subscriptionId"] = subscription.SubscriptionId,
            }).ConfigureAwait(false);
        OpcEventNotificationDto[] refreshedEvents = await server.CallToolAsync<OpcEventNotificationDto[]>(
            "opcclassic.ae.poll_events",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["subscriptionId"] = subscription.SubscriptionId,
                ["waitMilliseconds"] = 1000,
            }).ConfigureAwait(false);
        OpcResultDto canceled = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.ae.cancel_subscription",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["subscriptionId"] = subscription.SubscriptionId,
            }).ConfigureAwait(false);

        await Assert.That(filtered.SubscriptionId).IsEqualTo(subscription.SubscriptionId);
        await Assert.That(initialEvents.Length).IsGreaterThan(0);
        await Assert.That(initialEvents[0].ConditionName).IsEqualTo("LevelHigh");
        await Assert.That(refreshed.Succeeded).IsTrue();
        await Assert.That(refreshedEvents.Length).IsGreaterThan(0);
        await Assert.That(canceled.Succeeded).IsTrue();
    }

    [Test]
    public async Task Ae_disconnect_without_connect_returns_not_connected_via_mcp_client()
    {
        await using McpAeHdaTestServer server = await McpAeHdaTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);

        OpcResultDto disconnected = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.ae.disconnect",
            new Dictionary<string, object> { ["sessionId"] = session.SessionId }).ConfigureAwait(false);

        await Assert.That(disconnected.Succeeded).IsFalse();
        await Assert.That(disconnected.Message).Contains("not connected");
    }

    [Test]
    public async Task Ae_ack_condition_returns_success_via_mcp_client()
    {
        var syntheticAe = new SyntheticAeServer();
        string channelName = "ae-" + Guid.NewGuid().ToString("N");
        using IDisposable registration = InMemoryAeConnectionRegistry.Register(channelName, syntheticAe.Channel, syntheticAe);
        await using McpAeHdaTestServer server = await McpAeHdaTestServer.CreateAsync().ConfigureAwait(false);
        OpcSessionDto session = await server.CallToolAsync<OpcSessionDto>("opcclassic.session.create", []).ConfigureAwait(false);
        _ = await server.CallToolAsync<OpcResultDto>(
            "opcclassic.ae.connect",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["connectionString"] = "inmemory://" + channelName,
            }).ConfigureAwait(false);

        OpcResultDto[] ack = await server.CallToolAsync<OpcResultDto[]>(
            "opcclassic.ae.ack_condition",
            new Dictionary<string, object>
            {
                ["sessionId"] = session.SessionId,
                ["source"] = "Plant1.AreaA.Tank7",
                ["conditionName"] = "LevelHigh",
                ["actor"] = "operator.test",
                ["comment"] = "ack via mcp",
            }).ConfigureAwait(false);

        await Assert.That(ack.Length).IsEqualTo(1);
        await Assert.That(ack[0].Succeeded).IsTrue();
        await Assert.That(syntheticAe.LastAckActor).IsEqualTo("operator.test");
    }
}

internal sealed class McpAeHdaTestServer : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly IHost _host;
    private readonly Pipe _clientToServer;
    private readonly Pipe _serverToClient;

    private McpAeHdaTestServer(IHost host, McpClient client, Pipe clientToServer, Pipe serverToClient)
    {
        _host = host;
        Client = client;
        _clientToServer = clientToServer;
        _serverToClient = serverToClient;
    }

    public McpClient Client { get; }

    public static async Task<McpAeHdaTestServer> CreateAsync(Action<IServiceCollection>? configureServices = null)
    {
        var clientToServer = new Pipe();
        var serverToClient = new Pipe();
        HostApplicationBuilder builder = Host.CreateApplicationBuilder([]);
        builder.Logging.ClearProviders();
        builder.Services.AddSingleton<IOpcSessionManager, OpcSessionManager>();
        configureServices?.Invoke(builder.Services);
        builder.Services
            .AddMcpServer()
            .WithStreamServerTransport(clientToServer.Reader.AsStream(), serverToClient.Writer.AsStream())
            .WithTools<SessionTools>()
            .WithTools<AeClientTools>()
            .WithTools<HdaClientTools>();

        IHost host = builder.Build();
        await host.StartAsync().ConfigureAwait(false);

        var transport = new StreamClientTransport(
            clientToServer.Writer.AsStream(),
            serverToClient.Reader.AsStream(),
            NullLoggerFactory.Instance);
        McpClient client = await McpClient.CreateAsync(
            transport,
            loggerFactory: NullLoggerFactory.Instance).ConfigureAwait(false);

        return new McpAeHdaTestServer(host, client, clientToServer, serverToClient);
    }

    public async Task<T> CallToolAsync<T>(string toolName, Dictionary<string, object> arguments)
    {
        Dictionary<string, object?> nullableArguments = arguments.ToDictionary(static pair => pair.Key, static pair => (object?)pair.Value);
        CallToolResult result = await Client.CallToolAsync(toolName, nullableArguments).ConfigureAwait(false);
        if (result.IsError == true)
        {
            string error = string.Join("\n", result.Content.OfType<TextContentBlock>().Select(static content => content.Text));
            throw new InvalidOperationException(error);
        }

        TextContentBlock text = result.Content.OfType<TextContentBlock>().Single();
        T? value = JsonSerializer.Deserialize<T>(text.Text, JsonOptions);
        return value ?? throw new InvalidOperationException($"Tool '{toolName}' returned null JSON.");
    }

    public async ValueTask DisposeAsync()
    {
        await Client.DisposeAsync().ConfigureAwait(false);
        await _host.StopAsync().ConfigureAwait(false);
        _host.Dispose();
        await _clientToServer.Reader.CompleteAsync().ConfigureAwait(false);
        await _clientToServer.Writer.CompleteAsync().ConfigureAwait(false);
        await _serverToClient.Reader.CompleteAsync().ConfigureAwait(false);
        await _serverToClient.Writer.CompleteAsync().ConfigureAwait(false);
    }
}

internal sealed class SyntheticAeServer : IOpcAeServer, IAeServer
{
    private static readonly DateTimeOffset Startup = DateTimeOffset.UtcNow;
    private readonly OpcAeServerDispatcher _serverDispatcher;

    public SyntheticAeServer()
    {
        _serverDispatcher = new OpcAeServerDispatcher(this);
        Channel = new InMemoryCallChannel(DispatchAsync);
    }

    public event EventHandler<EventArgs>? ServerShutdown;

    public InMemoryCallChannel Channel { get; }
    public string? LastAckActor { get; private set; }

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Ae,
            StartTime = Startup,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 2, 3),
            VendorInfo = "Synthetic MCP AE Server",
        });
    }

    public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(0x1F);
    }

    public async IAsyncEnumerable<AreaBrowseElement> BrowseAreasAsync(string areaQualifiedName, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(areaQualifiedName))
        {
            yield return new AreaBrowseElement { Name = "Plant1", QualifiedName = "Plant1", IsArea = true };
            yield return new AreaBrowseElement { Name = "AreaA", QualifiedName = "Plant1.AreaA", IsArea = true };
        }
        else if (areaQualifiedName.Equals("Plant1.AreaA", StringComparison.OrdinalIgnoreCase))
        {
            yield return new AreaBrowseElement { Name = "Tank7", QualifiedName = "Plant1.AreaA.Tank7", IsSource = true };
        }
    }

    public Task<IReadOnlyList<uint>> QueryEventCategoriesAsync(EventType eventTypes, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = eventTypes;
        return Task.FromResult<IReadOnlyList<uint>>([0x1001u, 0x1002u]);
    }

    public Task QueryEventCategoriesAsync(int eventType, out int[] eventCategories, out string[] eventCategoryDescriptions, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = eventType;
        eventCategories = [0x1001, 0x1002];
        eventCategoryDescriptions = ["Simple", "Condition"];
        return Task.CompletedTask;
    }

    public Task QueryEventAttributesAsync(int eventCategory, out int[] attributeIds, out string[] attributeDescriptions, out ushort[] attributeTypes, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = eventCategory;
        attributeIds = [10, 11];
        attributeDescriptions = ["Area", "Limit"];
        attributeTypes = [(ushort)VarType.VT_BSTR, (ushort)VarType.VT_R8];
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<string>> QueryConditionNamesAsync(uint eventCategory, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = eventCategory;
        return Task.FromResult<IReadOnlyList<string>>(["LevelHigh"]);
    }

    public Task<string[]> QueryConditionNamesAsync(int eventCategory, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = eventCategory;
        return Task.FromResult(new[] { "LevelHigh" });
    }

    public Task<string[]> QuerySubConditionNamesAsync(string conditionName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = conditionName;
        return Task.FromResult(new[] { "Hi", "HiHi" });
    }

    public Task<string[]> QuerySourceConditionsAsync(string source, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = source;
        return Task.FromResult(new[] { "LevelHigh" });
    }

    public Task TranslateToItemIDsAsync(string source, int eventCategory, string conditionName, string subconditionName, int[] associatedAttributeIds, out string[] attributeItemIds, out string[] nodeNames, out Guid[] classIds, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = eventCategory;
        _ = conditionName;
        _ = subconditionName;
        attributeItemIds = associatedAttributeIds.Select(id => source + ".Attr" + id.ToString(CultureInfo.InvariantCulture)).ToArray();
        nodeNames = associatedAttributeIds.Select(static _ => "AeNode").ToArray();
        classIds = associatedAttributeIds.Select(static _ => Guid.Empty).ToArray();
        return Task.CompletedTask;
    }

    public Task<OpcConditionState> GetConditionStateAsync(string source, string conditionName, int[] attributeIds, CancellationToken cancellationToken = default)
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
            eventAttributes: attributeIds.Select(static id => id == 11 ? OpcVariant.FromDouble(95.0) : OpcVariant.FromString("AreaA")).ToArray(),
            errors: attributeIds.Select(static _ => OpcResultId.Ok.Code).ToArray()));
    }

    public Task<IReadOnlyList<AckResult>> AcknowledgeAsync(string actor, string? comment, IReadOnlyList<ConditionRef> conditions, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = comment;
        LastAckActor = actor;
        return Task.FromResult<IReadOnlyList<AckResult>>(conditions.Select(static condition => new AckResult { Condition = condition, ResultId = OpcResultId.Ok }).ToArray());
    }

    public Task<int[]> AckConditionAsync(int dwCount, string acknowledgerId, string comment, string[] sources, string[] conditionNames, long[] activeTimes, int[] cookies, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = dwCount;
        _ = comment;
        _ = activeTimes;
        _ = sources;
        _ = conditionNames;
        LastAckActor = acknowledgerId;
        return Task.FromResult(cookies.Select(static _ => OpcResultId.Ok.Code).ToArray());
    }

    public Task<OpcResultId> EnableConditionsByAreaAsync(IReadOnlyList<string> areaQualifiedNames, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = areaQualifiedNames;
        return Task.FromResult(OpcResultId.Ok);
    }

    public Task<OpcResultId> DisableConditionsByAreaAsync(IReadOnlyList<string> areaQualifiedNames, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = areaQualifiedNames;
        return Task.FromResult(OpcResultId.Ok);
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

    public Task<IAeSubscription> CreateSubscriptionAsync(bool active, int bufferTimeMs, int maxBufferSize, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IAeSubscription>(new SyntheticAeSubscription(active));
    }

    public Task CreateEventSubscriptionAsync(bool active, int bufferTime, int maxSize, int clientSubscription, Guid requestedInterfaceId, out IOPCEventSubscriptionMgt subscription, out int revisedBufferTime, out int revisedMaxSize, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = active;
        _ = clientSubscription;
        _ = requestedInterfaceId;
        subscription = default!;
        revisedBufferTime = Math.Max(bufferTime, 50);
        revisedMaxSize = Math.Max(maxSize, 1);
        return Task.CompletedTask;
    }

    public Task CreateAreaBrowserAsync(Guid requestedInterfaceId, out IOPCEventAreaBrowser areaBrowser, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = requestedInterfaceId;
        areaBrowser = default!;
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        ServerShutdown?.Invoke(this, EventArgs.Empty);
        return ValueTask.CompletedTask;
    }

    private Task<NdrCallResult> DispatchAsync(Guid interfaceId, int opnum, ReadOnlyMemory<byte> requestPayload, CancellationToken cancellationToken) =>
        interfaceId == IOPCEventServer.InterfaceId
            ? _serverDispatcher.DispatchAsync(interfaceId, opnum, requestPayload, cancellationToken)
            : Task.FromResult(new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty));
}

internal sealed class SyntheticAeSubscription : IAeSubscription
{
    private readonly System.Threading.Channels.Channel<EventNotification> _events = System.Threading.Channels.Channel.CreateUnbounded<EventNotification>();

    public SyntheticAeSubscription(bool active)
    {
        Active = active;
        _events.Writer.TryWrite(CreateNotification("Initial condition event"));
    }

    public bool Active { get; private set; }
    public SubscriptionFilter Filter { get; private set; } = new();
    public IAsyncEnumerable<EventNotification> Events => ReadAllAsync();

    public Task SetActiveAsync(bool active, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Active = active;
        return Task.CompletedTask;
    }

    public Task SetFilterAsync(SubscriptionFilter filter, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Filter = filter;
        return Task.CompletedTask;
    }

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _events.Writer.TryWrite(CreateNotification("Refresh condition event"));
        return Task.CompletedTask;
    }

    public Task CancelRefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        _events.Writer.TryComplete();
        return ValueTask.CompletedTask;
    }

    private async IAsyncEnumerable<EventNotification> ReadAllAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (await _events.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
        {
            while (_events.Reader.TryRead(out EventNotification? item))
            {
                yield return item;
            }
        }
    }

    private static EventNotification CreateNotification(string message) => new()
    {
        Source = "Plant1.AreaA.Tank7",
        Time = DateTimeOffset.UtcNow,
        Message = message,
        Severity = 900,
        EventCategory = 0x1002,
        EventType = EventType.Condition,
        ConditionName = "LevelHigh",
        SubConditionName = "HiHi",
        NewState = ConditionState.Active,
        AckRequired = true,
        ActiveTime = DateTimeOffset.UnixEpoch.AddMinutes(1),
        Cookie = 1234,
        Quality = OpcQuality.Good,
        Attributes = new Dictionary<uint, object?> { [10] = "AreaA", [11] = 95.0 },
    };
}
