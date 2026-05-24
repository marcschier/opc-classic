# Build your first OPC DA client

Applies to Opc.Classic 0.6.0-alpha.1; the public API shape targets 1.0.0-rc.1.

This tutorial builds a complete Data Access client as a .NET 10 worker. It uses the public `Opc.Classic.Da.IDaServer` and `IDaSubscription` contracts, so the application code is the same shape whether the server is an in-process loopback target, a lab server, or a production DCOM-backed adapter. The runnable path below uses a loopback implementation so you can paste the files into a project and run the sequence without a Windows COM server. To connect to a real OPC DA server, keep the worker and replace only the `IDaServer` registration.

The sequence mirrors OPC DA 3.00: retrieve server status, browse the namespace, validate/add items to a group, read a snapshot, request a refresh, process `IOPCDataCallback::OnDataChange` batches, and remove the group on shutdown. For the shorter Linux recipe see [../cookbook/01-connect-to-matrikon-from-linux.md](../cookbook/01-connect-to-matrikon-from-linux.md); for the architectural call-channel view see [../ARCHITECTURE.md](../ARCHITECTURE.md).

## Prerequisites

- .NET 10 SDK from `global.json`.
- A checkout of `marcschier/opc-classic`, or packages from your local feed once packages are published.
- Basic familiarity with OPC DA item IDs, quality words, and groups.
- Optional: a real DA server such as Matrikon OPC Simulation Server. The tutorial runs without it by using a loopback client.

## What you'll learn

- How to create a hosted .NET 10 console client.
- How to register an `IDaServer` in dependency injection.
- How to browse, read, subscribe, refresh, and process callbacks.
- How to handle `OpcException` and per-item `OpcResultId` values.
- How to cancel cleanly and dispose subscriptions.

## Project setup

Create a folder named `FirstDaClient` beside the repository or inside a scratch workspace. If you are working directly in the repository, project references are convenient. If you consume packages, replace the `ProjectReference` entries with `PackageReference Include="Opc.Classic.Core"` and `PackageReference Include="Opc.Classic.Da"`.

`FirstDaClient.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.Extensions.Hosting" />
    <PackageReference Include="Microsoft.Extensions.Logging.Console" />
    <ProjectReference Include="..\src\Opc.Classic.Core\Opc.Classic.Core.csproj" />
    <ProjectReference Include="..\src\Opc.Classic.Da\Opc.Classic.Da.csproj" />
  </ItemGroup>
</Project>
```

Add `appsettings.json` so configuration has a place for real-server settings even while loopback mode is selected:

```json
{
  "OpcDa": {
    "Mode": "Loopback",
    "Url": "opcda://localhost/Opc.Classic.Samples.DaServer.1",
    "User": "opc-reader",
    "Domain": "PLANT",
    "ProtectionLevel": "Integrity",
    "UpdateRateMs": 500
  }
}
```

`Mode=Loopback` keeps the sample self-contained. For a real server, keep this configuration shape and register the production DCOM-backed `IDaServer` adapter for your deployment. `OpcUrl.Parse`, `OpcConnectData.WithNtlmV2`, and `OpcProtectionLevel.Integrity` are stable connection primitives described in [../ADOPTION.md](../ADOPTION.md).

## Program.cs

The program below is intentionally complete. The application service (`DaClientWorker`) depends only on `IDaServer`. The loopback implementation at the bottom is small but uses the real `Item`, `BrowseElement`, `ItemValueResult`, `SubscriptionState`, `DataChange`, and `IDaSubscription` types.

```csharp
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic;
using Opc.Classic.Da;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(static options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

builder.Services.AddSingleton<TagStore>();
builder.Services.AddSingleton<IDaServer, LoopbackDaClient>();
builder.Services.AddHostedService<DaClientWorker>();

await builder.Build().RunAsync();

public sealed class DaClientWorker : BackgroundService
{
    private readonly IDaServer _server;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<DaClientWorker> _logger;

    public DaClientWorker(
        IDaServer server,
        IHostApplicationLifetime lifetime,
        ILogger<DaClientWorker> logger)
    {
        _server = server;
        _lifetime = lifetime;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunClientAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        catch (OpcException ex)
        {
            _logger.LogError(ex, "OPC call failed with {ResultId}", ex.ResultId);
        }
        finally
        {
            await _server.DisposeAsync().ConfigureAwait(false);
            _lifetime.StopApplication();
        }
    }

    private async Task RunClientAsync(CancellationToken cancellationToken)
    {
        OpcServerStatus status = await _server.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Connected to {Vendor}; state={State}", status.VendorInfo, status.State);

        await foreach (BrowseElement element in _server.BrowseAsync(string.Empty, BrowseFilters.All, cancellationToken).ConfigureAwait(false))
        {
            _logger.LogInformation("Browse: {Element}", element);
        }

        var items = new[]
        {
            new Item("Plant.Temperature") { ClientHandle = 1001 },
            new Item("Plant.Pressure") { ClientHandle = 1002 },
            new Item("Plant.PumpRunning") { ClientHandle = 1003 },
        };

        await using IDaSubscription subscription = await _server.CreateSubscriptionAsync(
            new SubscriptionState
            {
                Name = "first-da-client",
                ClientHandle = 5000,
                UpdateRateMs = 500,
                Active = true,
                LocaleId = 0x0409,
                KeepAliveMs = 5_000,
            },
            cancellationToken).ConfigureAwait(false);

        IReadOnlyList<IdentifiedResult> addResults = await subscription.AddItemsAsync(items, cancellationToken).ConfigureAwait(false);
        foreach (IdentifiedResult result in addResults)
        {
            if (result.ResultId.IsFailure)
            {
                string text = await _server.GetErrorTextAsync(result.ResultId, cancellationToken).ConfigureAwait(false);
                _logger.LogWarning("AddItems failed for {Item}: {Result} {Text}", result.ItemName, result.ResultId, text);
            }
            else
            {
                _logger.LogInformation("Added {Item} as client handle {Handle}", result.ItemName, result.ClientHandle);
            }
        }

        IReadOnlyList<int> serverHandles = items.Select(static item => item.ClientHandle).ToArray();
        IReadOnlyList<ItemValueResult> snapshot = await subscription.ReadAsync(serverHandles, fromCache: true, cancellationToken).ConfigureAwait(false);
        LogValues("Read", snapshot);

        Task<DataChange> nextChange = ReadOneChangeAsync(subscription, cancellationToken);
        int transactionId = await subscription.RefreshAsync(fromCache: true, cancellationToken).ConfigureAwait(false);
        DataChange change = await nextChange.ConfigureAwait(false);
        _logger.LogInformation("Refresh transaction {TransactionId} delivered callback {CallbackTransaction}", transactionId, change.TransactionId);
        LogValues("Callback", change.Items);
    }

    private async Task<DataChange> ReadOneChangeAsync(IDaSubscription subscription, CancellationToken cancellationToken)
    {
        await foreach (DataChange change in subscription.DataChanges.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            return change;
        }

        throw new InvalidOperationException("The subscription ended before a data-change callback arrived.");
    }

    private void LogValues(string label, IEnumerable<ItemValueResult> values)
    {
        foreach (ItemValueResult value in values)
        {
            _logger.LogInformation(
                "{Label}: {Item} value={Value} quality={Quality} timestamp={Timestamp:O} result={Result}",
                label,
                value.ItemName,
                value.Value,
                value.Quality,
                value.Timestamp,
                value.ResultId);
        }
    }
}

public sealed class LoopbackDaClient : IDaServer
{
    private readonly TagStore _tags;
    private bool _disposed;

    public LoopbackDaClient(TagStore tags) => _tags = tags;

    public event EventHandler<ServerShutdownEventArgs>? ServerShutdown;

    public int LocaleId { get; private set; } = 0x0409;

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = _tags.StartTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            GroupCount = 0,
            BandWidth = 0,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = "First DA Client loopback server",
        });
    }

    public Task SetLocaleAsync(int localeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LocaleId = localeId;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<int>> GetSupportedLocalesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<int>>([0x0409]);
    }

    public Task<string> GetErrorTextAsync(OpcResultId resultId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(resultId.Description ?? $"HRESULT 0x{resultId.Code:X8}");
    }

    public async IAsyncEnumerable<BrowseElement> BrowseAsync(
        string itemPath,
        BrowseFilters filters = BrowseFilters.All,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(itemPath);
        foreach (BrowseElement element in _tags.Browse(itemPath, filters))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await Task.Yield();
            yield return element;
        }
    }

    public Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_tags.Read(items));
    }

    public Task<IReadOnlyList<IdentifiedResult>> WriteAsync(IReadOnlyList<ItemValue> values, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<IdentifiedResult>>(
            values.Select(static value => new IdentifiedResult(value.ItemName)
            {
                ClientHandle = value.ClientHandle,
                ResultId = OpcResultId.BadRights,
            }).ToArray());
    }

    public Task<IReadOnlyList<IdentifiedResult>> ValidateItemsAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_tags.Validate(items));
    }

    public Task<IReadOnlyList<ItemPropertyResult>> GetPropertiesAsync(
        IReadOnlyList<ItemIdentifier> itemIds,
        IReadOnlyList<PropertyID> propertyIds,
        bool returnValues,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<ItemPropertyResult>>(Array.Empty<ItemPropertyResult>());
    }

    public Task<IDaSubscription> CreateSubscriptionAsync(SubscriptionState state, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IDaSubscription>(new LoopbackDaSubscription(_tags, state));
    }

    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            ServerShutdown?.Invoke(this, new ServerShutdownEventArgs { Reason = "Client disposed" });
        }

        return ValueTask.CompletedTask;
    }
}

public sealed class LoopbackDaSubscription : IDaSubscription
{
    private readonly TagStore _tags;
    private readonly Dictionary<int, Item> _items = new();
    private readonly Queue<DataChange> _changes = new();
    private readonly SemaphoreSlim _signal = new(0);
    private bool _disposed;
    private int _nextTransaction;

    public LoopbackDaSubscription(TagStore tags, SubscriptionState state)
    {
        _tags = tags;
        State = state;
    }

    public SubscriptionState State { get; private set; }

    public IAsyncEnumerable<DataChange> DataChanges => ReadChangesAsync();

    public Task SetStateAsync(SubscriptionState state, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        State = state;
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<IdentifiedResult>> AddItemsAsync(IReadOnlyList<Item> items, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (Item item in items)
        {
            _items[item.ClientHandle] = item;
        }

        return Task.FromResult(_tags.Validate(items));
    }

    public Task<IReadOnlyList<IdentifiedResult>> RemoveItemsAsync(IReadOnlyList<int> serverHandles, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var results = serverHandles.Select(handle =>
        {
            _items.Remove(handle);
            return new IdentifiedResult($"#{handle}") { ClientHandle = handle };
        }).ToArray();
        return Task.FromResult<IReadOnlyList<IdentifiedResult>>(results);
    }

    public Task<IReadOnlyList<IdentifiedResult>> SetActiveStateAsync(IReadOnlyList<int> serverHandles, bool active, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<IdentifiedResult>>(
            serverHandles.Select(handle => new IdentifiedResult($"#{handle}") { ClientHandle = handle }).ToArray());
    }

    public Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<int> serverHandles, bool fromCache, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Item[] items = serverHandles.Select(handle => _items[handle]).ToArray();
        return Task.FromResult(_tags.Read(items));
    }

    public Task<IReadOnlyList<IdentifiedResult>> WriteAsync(IReadOnlyList<int> serverHandles, IReadOnlyList<object?> values, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<IdentifiedResult>>(Array.Empty<IdentifiedResult>());
    }

    public Task<int> RefreshAsync(bool fromCache, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int transaction = Interlocked.Increment(ref _nextTransaction);
        Item[] items = _items.Values.ToArray();
        _changes.Enqueue(new DataChange
        {
            TransactionId = transaction,
            Items = _tags.Read(items),
        });
        _signal.Release();
        return Task.FromResult(transaction);
    }

    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            _signal.Release();
            _signal.Dispose();
        }

        return ValueTask.CompletedTask;
    }

    private async IAsyncEnumerable<DataChange> ReadChangesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (true)
        {
            await _signal.WaitAsync(cancellationToken).ConfigureAwait(false);
            if (_changes.TryDequeue(out DataChange? change))
            {
                yield return change;
                continue;
            }

            if (_disposed)
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
        ["Plant.Temperature"] = static () => Math.Round(20.0 + DateTimeOffset.UtcNow.Second / 10.0, 2),
        ["Plant.Pressure"] = static () => Math.Round(1.0 + DateTimeOffset.UtcNow.Millisecond / 1000.0, 3),
        ["Plant.PumpRunning"] = static () => DateTimeOffset.UtcNow.Second % 2 == 0,
    };

    public DateTimeOffset StartTime { get; } = DateTimeOffset.UtcNow;

    public IReadOnlyList<BrowseElement> Browse(string itemPath, BrowseFilters filters)
    {
        _ = itemPath;
        _ = filters;
        return _tags.Keys.Select(static item => new BrowseElement
        {
            Name = item,
            ItemName = item,
            IsItem = true,
        }).ToArray();
    }

    public IReadOnlyList<ItemValueResult> Read(IReadOnlyList<Item> items) =>
        items.Select(item =>
        {
            bool known = _tags.TryGetValue(item.ItemName, out Func<object?>? read);
            return new ItemValueResult(item.ItemName, item.Path)
            {
                ClientHandle = item.ClientHandle,
                Value = known ? read!() : null,
                Quality = known ? OpcQuality.Good : OpcQuality.Bad,
                Timestamp = DateTimeOffset.UtcNow,
                ResultId = known ? OpcResultId.Ok : OpcResultId.UnknownItemId,
            };
        }).ToArray();

    public IReadOnlyList<IdentifiedResult> Validate(IReadOnlyList<Item> items) =>
        items.Select(item => new IdentifiedResult(item.ItemName, item.Path)
        {
            ClientHandle = item.ClientHandle,
            ResultId = _tags.ContainsKey(item.ItemName) ? OpcResultId.Ok : OpcResultId.UnknownItemId,
        }).ToArray();
}
```

Run it with:

```bash
dotnet run
```

You should see a status line, three browse entries, three AddItems results, a snapshot read, and one callback produced by `RefreshAsync`.

## Sequence walkthrough

The worker starts with `GetStatusAsync`. In OPC DA this corresponds to `IOPCServer::GetStatus` and gives the client the server state, vendor string, version, start time, current time, and group count. Treat this as both a health check and a useful diagnostic stamp: every production connection log should include vendor and version.

Next, `BrowseAsync(string.Empty, BrowseFilters.All, ct)` streams `BrowseElement` values from the root. The managed API hides continuation tokens; a production adapter may need several `IOPCBrowse` calls under the covers, but the app reads a normal `IAsyncEnumerable<BrowseElement>`. Each element tells you whether it is an item, a branch, or both. Only items can be read or added to a subscription.

The subscription maps to a DA group. `SubscriptionState` carries the group name, client handle, requested update rate, active flag, LCID, deadband, and keep-alive interval. Servers may revise the requested rate and return `OPC_S_UNSUPPORTEDRATE`; never assume your requested rate is accepted. In this tutorial the loopback class uses client handles as server handles to keep the code small. Real servers return independent server handles; persist those handles for future `ReadAsync`, `WriteAsync`, `SetActiveStateAsync`, and `RemoveItemsAsync` calls.

Bulk operations return per-item results. `AddItemsAsync` returns `IdentifiedResult` rows and `ReadAsync` returns `ItemValueResult` rows. A batch can partially fail. For example, one unknown tag should not discard the good values for the other tags. Inspect `ResultId.IsFailure` before using `Value`, and call `GetErrorTextAsync` when you need localized vendor text.

Finally, `RefreshAsync` demonstrates the callback path. DA subscriptions normally push spontaneous changes through `DataChanges`; refresh forces the server to send current values for all active items. Each `DataChange` preserves DA's callback granularity, including a transaction ID, master result, and per-item values. Keeping callback batches intact helps with latency measurement and auditing because all item values in the batch were delivered together.

## Error handling and cancellation

Catch `OpcException` for protocol failures. The exception exposes `ResultId`, which wraps the HRESULT. Some success values, such as `S_FALSE` or `OPC_S_UNSUPPORTEDRATE`, are not failures. They should be logged as warnings or handled as negotiated results. Do not turn every non-zero HRESULT into an exception.

Use `CancellationToken` on every call. Network DCOM calls can hang behind firewalls or endpoint mapper misconfiguration. A hosted service should also dispose the subscription before disposing the server so the remote group is removed while the connection is still valid.

## Clean shutdown checklist

- Stop reading `DataChanges` by cancelling the application token.
- Dispose each `IDaSubscription` with `await using`.
- Dispose the `IDaServer` connection.
- Log the server shutdown callback if `ServerShutdown` fires.
- Do not block the finalizer or process exit on long network timeouts.

## Pitfalls

- `ClientHandle` is your correlation value. Do not reuse it for two active items in the same group.
- `UpdateRateMs` is a request. Record the server-confirmed state when the adapter reports it.
- `OpcQuality.Good` means the data source considers the value good; it does not prove the value is fresh enough for your process.
- Real DCOM servers usually require `OpcProtectionLevel.Integrity` after Microsoft DCOM hardening. See [../cookbook/05-dcom-hardening-pkt-integrity-explainer.md](../cookbook/05-dcom-hardening-pkt-integrity-explainer.md).
- Relative item paths and access paths are server-specific. Prefer canonical item names returned by browse.

## From loopback to a real server

The loopback implementation proves your application workflow without network variables. Connecting to a real server should be a dependency-injection change, not a rewrite of the worker. Keep the `DaClientWorker` exactly as it is and replace `LoopbackDaClient` with the adapter that creates a DCOM call channel, activates the configured ProgID/CLSID, and implements `IDaServer`. The connection settings line up with the stable core types:

```csharp
using System.Net;
using Opc.Classic;

OpcUrl url = OpcUrl.Parse("opcda://opc01.plant.example.com/Matrikon.OPC.Simulation.1");
OpcConnectData connectData = OpcConnectData.WithNtlmV2(
    url,
    new NetworkCredential("opc-reader", password, "PLANT"),
    OpcProtectionLevel.Integrity,
    TimeSpan.FromSeconds(30));
```

When you switch to a real server, validate one layer at a time. First parse the URL and log the host and server ID. Next complete authentication and `GetStatusAsync`. Then browse a small branch. Then validate or add one item. Only after those steps work should you enable production subscription rates. This order prevents a bad tag name from being misdiagnosed as a Kerberos or firewall issue.

## Production hardening checklist

A production client should persist the last known good server status, the list of subscribed item IDs, and the negotiated group state. That information makes restart diagnostics much faster. Record every server handle only for the lifetime of the connection; handles are not durable identifiers and must not be stored in a database for reuse after reconnect. On reconnect, recreate groups and items from canonical item names.

Add metrics around batch size, callback lag, and per-item result codes. A counter such as `opc.da.read.items{result="OPC_E_UNKNOWNITEMID"}` is more useful than one generic error counter. For callbacks, measure the time between `DataChange` arrival and your application finishing processing. If that duration grows beyond the update rate, your consumer is falling behind.

Finally, treat write operations differently from reads. Writes should carry stronger audit logging, smaller batches, and clearer operator messages. It is acceptable for a read batch to contain hundreds of values; a write batch that changes hundreds of setpoints may be operationally unsafe even if the protocol allows it.

## Next steps

- Host the sample DA server in [02-host-an-opc-server.md](02-host-an-opc-server.md) and point your adapter at `Opc.Classic.Samples.DaServer.1`.
- Deploy the client on Linux or in containers with [03-cross-platform-deployment.md](03-cross-platform-deployment.md).
- Harden authentication with [04-security-with-kerberos-and-channel-binding.md](04-security-with-kerberos-and-channel-binding.md).
- Use [09-troubleshooting-and-diagnostics.md](09-troubleshooting-and-diagnostics.md) when connection, HRESULT, or callback behavior differs from the loopback run.

## References

- OPC Data Access 3.00: `IOPCServer`, `IOPCItemMgt`, `IOPCSyncIO`, `IOPCAsyncIO2`, `IOPCDataCallback`.
- [MS-DCOM] and [MS-RPCE] for activation, bind, request, response, fragmentation, and packet protection.
- Repository samples: `samples\Opc.Classic.Samples.DaClient` and `samples\Opc.Classic.Samples.DaServer`.

