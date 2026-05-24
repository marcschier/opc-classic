// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

#pragma warning disable CA1062
#pragma warning disable CA1822
#pragma warning disable CA1848
#pragma warning disable CA1861
#pragma warning disable CA1873
#pragma warning disable CA2007
#pragma warning disable MA0048
#pragma warning disable MA0051

using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Testing;

namespace Opc.Classic.Samples.DaClient;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(static options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });

        builder.Services.AddSingleton<TagStore>();
        builder.Services.AddSingleton<LoopbackDaServer>();
        builder.Services.AddSingleton<IOpcDaServer>(static sp => sp.GetRequiredService<LoopbackDaServer>());
        builder.Services.AddSingleton<OpcDaServerDispatcher>();
        builder.Services.AddSingleton<InMemoryCallChannel>(static sp => new InMemoryCallChannel(sp.GetRequiredService<OpcDaServerDispatcher>().DispatchAsync));
        builder.Services.AddSingleton<IOPCServer>(static sp => new IOPCServerClientProxy(sp.GetRequiredService<InMemoryCallChannel>()));
        builder.Services.AddSingleton<IDaServer, LoopbackDaClient>();
        builder.Services.AddHostedService<DemoWorker>();

        await builder.Build().RunAsync().ConfigureAwait(false);
        return 0;
    }
}

public sealed class DemoWorker : BackgroundService
{
    private readonly IDaServer _client;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<DemoWorker> _logger;

    public DemoWorker(IDaServer client, IHostApplicationLifetime lifetime, ILogger<DemoWorker> logger)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunAsync(stoppingToken).ConfigureAwait(false);
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    private async Task RunAsync(CancellationToken ct)
    {
        OpcServerStatus status = await _client.GetStatusAsync(ct).ConfigureAwait(false);
        _logger.LogInformation("Connected to {Vendor}", status.VendorInfo);

        await foreach (BrowseElement element in _client.BrowseAsync(string.Empty, BrowseFilters.All, ct).ConfigureAwait(false))
        {
            _logger.LogInformation("Browse: {Name} item={ItemName}", element.Name, element.ItemName);
        }

        var items = new[]
        {
            new Item("Plant.Temperature") { ClientHandle = 1001 },
            new Item("Plant.Pressure") { ClientHandle = 1002 },
            new Item("Plant.PumpRunning") { ClientHandle = 1003 },
        };

        await using IDaSubscription group = await _client.CreateSubscriptionAsync(new SubscriptionState
        {
            Name = "da-client-demo",
            ClientHandle = 5000,
            UpdateRateMs = 250,
            Active = true,
            LocaleId = 0x0409,
        }, ct).ConfigureAwait(false);

        IReadOnlyList<IdentifiedResult> addResults = await group.AddItemsAsync(items, ct).ConfigureAwait(false);
        foreach (IdentifiedResult result in addResults)
        {
            _logger.LogInformation("Added {Item} result={Result}", result.ItemName, result.ResultId);
        }

        IReadOnlyList<ItemValueResult> values = await group.ReadAsync(items.Select(static item => item.ClientHandle).ToArray(), fromCache: true, ct).ConfigureAwait(false);
        foreach (ItemValueResult value in values)
        {
            _logger.LogInformation("Read {Item}: {Value} {Quality}", value.ItemName, value.Value, value.Quality);
        }

        Task callback = LogOneCallbackAsync(group, ct);
        await group.RefreshAsync(fromCache: true, ct).ConfigureAwait(false);
        await callback.ConfigureAwait(false);
        await _client.DisposeAsync().ConfigureAwait(false);
    }

    private async Task LogOneCallbackAsync(IDaSubscription group, CancellationToken ct)
    {
        await foreach (DataChange change in group.DataChanges.WithCancellation(ct).ConfigureAwait(false))
        {
            _logger.LogInformation("Callback transaction={TransactionId} count={Count}", change.TransactionId, change.Items.Count);
            return;
        }
    }
}

public sealed class LoopbackDaClient : IDaServer
{
    private readonly InMemoryCallChannel _channel;
    private readonly LoopbackDaServer _server;
    private readonly IOPCServer _serverProxy;
    private bool _disposed;

    public LoopbackDaClient(IOPCServer serverProxy, LoopbackDaServer server, InMemoryCallChannel channel)
    {
        _serverProxy = serverProxy ?? throw new ArgumentNullException(nameof(serverProxy));
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _channel = channel ?? throw new ArgumentNullException(nameof(channel));
    }

    public event EventHandler<ServerShutdownEventArgs>? ServerShutdown;

    public int LocaleId { get; private set; } = 0x0409;

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default) => _serverProxy.GetStatusAsync(cancellationToken);

    public Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LocaleId = localeId;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<int>>(new[] { 0x0409 });
    }

    public async Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default) =>
        await _serverProxy.GetErrorStringAsync(resultId.Code, LocaleId, cancellationToken).ConfigureAwait(false);

    public async IAsyncEnumerable<BrowseElement> BrowseAsync(string itemPath, BrowseFilters filters = BrowseFilters.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemPath);
        foreach (BrowseElement element in _server.Browse(itemPath, filters))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask.ConfigureAwait(false);
            yield return element;
        }
    }

    public Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) => _server.ReadAsync(items, cancellationToken);
    public Task<IReadOnlyList<IdentifiedResult>> WriteAsync(IReadOnlyList<ItemValue> values, CancellationToken cancellationToken = default) => _server.WriteAsync(values, cancellationToken);
    public Task<IReadOnlyList<IdentifiedResult>> ValidateItemsAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) => _server.ValidateAsync(items, cancellationToken);
    public Task<IReadOnlyList<ItemPropertyResult>> GetPropertiesAsync(IReadOnlyList<ItemIdentifier> itemIds, IReadOnlyList<PropertyID> propertyIds, bool returnValues, CancellationToken cancellationToken = default) => _server.GetPropertiesAsync(itemIds, propertyIds, returnValues, cancellationToken);

    public async Task<IDaSubscription> CreateSubscriptionAsync(SubscriptionState state, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(state);
        int handle = await _server.AddGroupAsync(state.Name ?? "da-client-demo", state.Active, state.UpdateRateMs, state.ClientHandle, state.LocaleId, cancellationToken).ConfigureAwait(false);
        return new LoopbackDaSubscription(_server, _serverProxy, handle, state);
    }

    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            _ = _channel.CallLog.Count;
            ServerShutdown?.Invoke(this, new ServerShutdownEventArgs { Reason = "client disconnected" });
        }

        return ValueTask.CompletedTask;
    }
}

public sealed class LoopbackDaServer : IOpcDaServer
{
    private static readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;
    private readonly Dictionary<int, string> _groups = new();
    private readonly TagStore _tags;
    private int _nextGroupHandle = 1000;

    public LoopbackDaServer(TagStore tags) => _tags = tags ?? throw new ArgumentNullException(nameof(tags));

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus { Spec = OpcStatusSpec.Da, StartTime = StartTime, CurrentTime = now, LastUpdateTime = now, State = OpcServerState.Running, GroupCount = _groups.Count, BandWidth = 0, ServerVersion = new Version(1, 0, 0), VendorInfo = "Opc.Classic DA client loopback server" });
    }

    public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        cancellationToken.ThrowIfCancellationRequested();
        _ = active; _ = requestedUpdateRate; _ = clientHandle; _ = localeId;
        int handle = Interlocked.Increment(ref _nextGroupHandle);
        _groups[handle] = name;
        return Task.FromResult(handle);
    }

    public Task AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientGroupHandle,
        int timeBias,
        float percentDeadband,
        int localeId,
        Guid requestedInterfaceId,
        out int serverGroupHandle,
        out int revisedUpdateRate,
        out IOpcInterfaceRef group,
        CancellationToken cancellationToken = default)
    {
        _ = timeBias; _ = percentDeadband;
        serverGroupHandle = Interlocked.Increment(ref _nextGroupHandle);
        revisedUpdateRate = requestedUpdateRate;
        _groups[serverGroupHandle] = name;
        group = new OpcInterfaceRef(requestedInterfaceId, 0, 1, 1, unchecked((ulong)(uint)serverGroupHandle), Guid.CreateVersion7(), 0, Array.Empty<ushort>());
        return Task.CompletedTask;
    }

    public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = force;
        _groups.Remove(serverGroupHandle);
        return Task.CompletedTask;
    }

    public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult($"Error 0x{errorCode:X8} locale=0x{localeId:X4}");
    }

    public IReadOnlyList<BrowseElement> Browse(string itemPath, BrowseFilters filters) => _tags.Browse(itemPath, filters);
    public Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<Item> items, CancellationToken ct) => Task.FromResult(_tags.Read(items));
    public Task<IReadOnlyList<IdentifiedResult>> WriteAsync(IReadOnlyList<ItemValue> values, CancellationToken ct) => Task.FromResult<IReadOnlyList<IdentifiedResult>>(values.Select(static value => new IdentifiedResult(value.ItemName) { ClientHandle = value.ClientHandle, ResultId = OpcResultId.BadRights }).ToArray());
    public Task<IReadOnlyList<IdentifiedResult>> ValidateAsync(IReadOnlyList<Item> items, CancellationToken ct) => Task.FromResult<IReadOnlyList<IdentifiedResult>>(items.Select(item => new IdentifiedResult(item.ItemName) { ClientHandle = item.ClientHandle, ResultId = _tags.Contains(item.ItemName) ? OpcResultId.Ok : OpcResultId.UnknownItemId }).ToArray());
    public Task<IReadOnlyList<ItemPropertyResult>> GetPropertiesAsync(IReadOnlyList<ItemIdentifier> itemIds, IReadOnlyList<PropertyID> propertyIds, bool returnValues, CancellationToken ct) => Task.FromResult<IReadOnlyList<ItemPropertyResult>>(Array.Empty<ItemPropertyResult>());
}

public sealed class LoopbackDaSubscription : IDaSubscription
{
    private readonly List<DataChange> _changes = [];
    private readonly SemaphoreSlim _signal = new(0);
    private readonly Dictionary<int, Item> _items = new();
    private readonly LoopbackDaServer _server;
    private readonly IOPCServer _serverProxy;
    private readonly int _serverGroupHandle;
    private bool _disposed;
    private int _nextTransaction;

    public LoopbackDaSubscription(LoopbackDaServer server, IOPCServer serverProxy, int serverGroupHandle, SubscriptionState state)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _serverProxy = serverProxy ?? throw new ArgumentNullException(nameof(serverProxy));
        _serverGroupHandle = serverGroupHandle;
        State = state ?? throw new ArgumentNullException(nameof(state));
    }

    public SubscriptionState State { get; private set; }
    public IAsyncEnumerable<DataChange> DataChanges => ReadChangesAsync();
    public Task SetStateAsync(SubscriptionState state, CancellationToken cancellationToken = default) { State = state ?? throw new ArgumentNullException(nameof(state)); return Task.CompletedTask; }
    public Task<IReadOnlyList<IdentifiedResult>> AddItemsAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default) { foreach (Item item in items) _items[item.ClientHandle] = item; return _server.ValidateAsync(items, cancellationToken); }
    public Task<IReadOnlyList<IdentifiedResult>> RemoveItemsAsync(IReadOnlyList<int> serverHandles, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<IdentifiedResult>>(serverHandles.Select(handle => { _items.Remove(handle); return new IdentifiedResult($"#{handle}"); }).ToArray());
    public Task<IReadOnlyList<IdentifiedResult>> SetActiveStateAsync(IReadOnlyList<int> serverHandles, bool active, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<IdentifiedResult>>(serverHandles.Select(handle => new IdentifiedResult($"#{handle}")).ToArray());
    public Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<int> serverHandles, bool fromCache, CancellationToken cancellationToken = default) => _server.ReadAsync(serverHandles.Select(handle => _items[handle]).ToArray(), cancellationToken);
    public Task<IReadOnlyList<IdentifiedResult>> WriteAsync(IReadOnlyList<int> serverHandles, IReadOnlyList<object?> values, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());

    public async Task<int> RefreshAsync(bool fromCache, CancellationToken cancellationToken = default)
    {
        int transaction = Interlocked.Increment(ref _nextTransaction);
        IReadOnlyList<ItemValueResult> values = await _server.ReadAsync(_items.Values.ToArray(), cancellationToken).ConfigureAwait(false);
        _changes.Add(new DataChange { TransactionId = transaction, Items = values });
        _signal.Release();
        return transaction;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        _signal.Release();
        await _serverProxy.RemoveGroupAsync(_serverGroupHandle, force: true, CancellationToken.None).ConfigureAwait(false);
        _signal.Dispose();
    }

    private async IAsyncEnumerable<DataChange> ReadChangesAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        while (true)
        {
            await _signal.WaitAsync(ct).ConfigureAwait(false);
            if (_changes.Count > 0)
            {
                DataChange change = _changes[0];
                _changes.RemoveAt(0);
                yield return change;
            }
            else if (_disposed)
            {
                yield break;
            }
        }
    }
}

public sealed class TagStore
{
    private readonly Dictionary<string, Func<object?>> _tags = new(StringComparer.Ordinal)
    {
        ["Plant.Temperature"] = static () => Math.Round(20.0D + (DateTimeOffset.UtcNow.Second / 10.0D), 2),
        ["Plant.Pressure"] = static () => Math.Round(1.0D + (DateTimeOffset.UtcNow.Millisecond / 1000.0D), 3),
        ["Plant.PumpRunning"] = static () => DateTimeOffset.UtcNow.Second % 2 == 0,
    };

    public bool Contains(string itemName) => _tags.ContainsKey(itemName);

    public IReadOnlyList<BrowseElement> Browse(string itemPath, BrowseFilters filters)
    {
        _ = itemPath; _ = filters;
        return _tags.Keys.Select(static item => new BrowseElement { Name = item, ItemName = item, IsItem = true }).ToArray();
    }

    public IReadOnlyList<ItemValueResult> Read(IReadOnlyList<Item> items) => items.Select(item => new ItemValueResult(item.ItemName) { ClientHandle = item.ClientHandle, Value = _tags.TryGetValue(item.ItemName, out Func<object?>? read) ? read() : null, Quality = _tags.ContainsKey(item.ItemName) ? OpcQuality.Good : OpcQuality.Bad, Timestamp = DateTimeOffset.UtcNow, ResultId = _tags.ContainsKey(item.ItemName) ? OpcResultId.Ok : OpcResultId.UnknownItemId }).ToArray();
}
