# Host an OPC DA server with Opc.Classic.Hosting

Applies to Opc.Classic 0.6.0-alpha.1; the public API shape targets 1.0.0-rc.1.

This tutorial walks through hosting a managed OPC Data Access server. The canonical repository example is `samples\Opc.Classic.Samples.DaServer\`, which registers `Opc.Classic.Samples.DaServer.1`, wires a `TagTree`, and exposes a small `IOpcDaServer` implementation through `Opc.Classic.Hosting`. Here we build the same production shape from scratch and explain the pieces you need to keep stable when a legacy Windows DA client connects through `IOPCServer`, `IOPCGroupStateMgt`, and subscription callbacks.

The server-hosting surface keeps protocol hosting separate from business state: `IOpcDaServer` provides status, group lifecycle, and localized error strings, while generated dispatchers and callback publishers route protocol calls through the common host infrastructure. Design your server with tag storage separate from group lifecycle, explicit HRESULT mapping, data-change publishing as a server-side stream, and stable CLSID/ProgID metadata. The concepts in this article match OPC DA 3.00 and the IDL terminology in `External\Include\opcda.idl`.

## Prerequisites

- .NET 10 SDK.
- A local checkout or package feed for `Opc.Classic.Core`, `Opc.Classic.Da`, and `Opc.Classic.Hosting`.
- Familiarity with Windows OPC DA clients and ProgID/CLSID registration.
- Optional Windows client for interoperability tests. The tutorial code runs as a managed host without requiring a COM client.

## What you'll learn

- How `AddClassicServer`, `AddClassicClsidRegistry`, and `AddOpcDaServer<T>` fit together.
- How to implement `IOpcDaServer` with group lifecycle semantics.
- How to design a tag tree and isolate reads/writes from hosting.
- How to publish DA data-change batches.
- How to map validation and runtime failures to `HRESULT` values.

## Hosting architecture in one picture

`Opc.Classic.Hosting` bridges Microsoft.Extensions.Hosting to one or more OPC server hosts. The common `ClassicHostedService` starts each registered `IOpcServerHost`. The DA-specific `OpcDaServerHost` exposes the configured CLSID, ProgID, friendly name, and listen address, then uses an `OpcDaServerDispatcher` to translate incoming DCOM operations into calls on your `IOpcDaServer`.

```text
Windows or managed DA client
        |
        | [MS-DCOM] activation + IOPCServer calls
        v
OpcDaServerHost -> OpcDaServerDispatcher -> your IOpcDaServer
        |                                      |
        |                                      +-- GroupRegistry
        |                                      +-- TagCatalog
        +-- IOpcDaDataChangePublisher --------+-- data-change fan-out
```

This split matters in production. The host owns network, activation, and callback plumbing. Your server implementation owns business state: which tags exist, which groups have been added, what error text means, and which values should be emitted.

## Project file

Create a console project and reference the hosting libraries:

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
    <ProjectReference Include="..\src\Opc.Classic.Hosting\Opc.Classic.Hosting.csproj" />
  </ItemGroup>
</Project>
```

For package-based builds, replace the project references with `Opc.Classic.Core`, `Opc.Classic.Da`, and `Opc.Classic.Hosting` package references.

## Program.cs

This program registers the host, the CLSID registry, a tag catalog, a group registry, a periodic signal generator, and the managed DA server implementation.

```csharp
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic;
using Opc.Classic.Da;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(static options =>
{
    options.SingleLine = true;
    options.TimestampFormat = "HH:mm:ss ";
});

builder.Services.AddClassicServer();
builder.Services.AddClassicClsidRegistry(builder.Configuration);
builder.Services.AddSingleton<TagCatalog>();
builder.Services.AddSingleton<GroupRegistry>();
builder.Services.AddHostedService<SignalPublisher>();
builder.Services.AddOpcDaServer<ManagedDaServer>(static options =>
{
    options.Clsid = Guid.Parse("4E3F63E7-4CC7-4E77-A59E-6462A1002001");
    options.ProgId = "Contoso.ManagedDa.1";
    options.FriendlyName = "Contoso Managed OPC DA Server";
    options.ListenAddress = "127.0.0.1:0";
});

await builder.Build().RunAsync();

public sealed class ManagedDaServer : IOpcDaServer
{
    private static readonly DateTimeOffset StartTime = DateTimeOffset.UtcNow;
    private readonly GroupRegistry _groups;
    private readonly TagCatalog _tags;
    private readonly ILogger<ManagedDaServer> _logger;

    public ManagedDaServer(GroupRegistry groups, TagCatalog tags, ILogger<ManagedDaServer> logger)
    {
        _groups = groups;
        _tags = tags;
        _logger = logger;
    }

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = StartTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            GroupCount = _groups.Count,
            BandWidth = 0,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = $"Contoso managed DA ({_tags.Count} tags)",
        });
    }

    public Task<int> AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientHandle,
        int localeId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        cancellationToken.ThrowIfCancellationRequested();

        int revisedRate = Math.Max(requestedUpdateRate, 250);
        int serverHandle = _groups.Add(name, active, revisedRate, clientHandle, localeId);
        _logger.LogInformation(
            "Added group {Name}: serverHandle={ServerHandle}, clientHandle={ClientHandle}, requestedRate={RequestedRate}, revisedRate={RevisedRate}",
            name,
            serverHandle,
            clientHandle,
            requestedUpdateRate,
            revisedRate);
        return Task.FromResult(serverHandle);
    }

    public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        bool removed = _groups.Remove(serverGroupHandle);
        _logger.LogInformation("RemoveGroup handle={Handle}, force={Force}, removed={Removed}", serverGroupHandle, force, removed);
        if (!removed && !force)
        {
            throw new OpcDaException(OpcResultId.InvalidHandle, $"Group handle {serverGroupHandle} does not exist.");
        }

        return Task.CompletedTask;
    }

    public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = new OpcResultId(errorCode, null);
        string text = result.Code switch
        {
            0 => "The operation completed successfully.",
            unchecked((int)0xC0040001u) => "The server group or item handle is invalid.",
            unchecked((int)0xC0040007u) => "The item ID does not exist.",
            unchecked((int)0xC0040006u) => "The item does not allow the requested access.",
            _ => result.ToString(),
        };
        return Task.FromResult($"LCID 0x{localeId:X4}: {text}");
    }
}

public sealed class GroupRegistry
{
    private readonly ConcurrentDictionary<int, GroupRecord> _groups = new();
    private int _nextHandle = 1000;

    public int Count => _groups.Count;

    public int Add(string name, bool active, int revisedUpdateRate, int clientHandle, int localeId)
    {
        int handle = Interlocked.Increment(ref _nextHandle);
        _groups[handle] = new GroupRecord(handle, name, active, revisedUpdateRate, clientHandle, localeId);
        return handle;
    }

    public bool Remove(int serverHandle) => _groups.TryRemove(serverHandle, out _);

    public IReadOnlyCollection<GroupRecord> Snapshot() => _groups.Values.ToArray();
}

public sealed record GroupRecord(
    int ServerHandle,
    string Name,
    bool Active,
    int RevisedUpdateRate,
    int ClientHandle,
    int LocaleId);

public sealed class TagCatalog
{
    private readonly Dictionary<string, Func<OpcVariant>> _tags = new(StringComparer.Ordinal)
    {
        ["Random.Real8"] = static () => OpcVariant.FromDouble(Random.Shared.NextDouble() * 100.0),
        ["Bucket Brigade.Int4"] = static () => OpcVariant.FromInt32(42),
        ["Plant.PumpRunning"] = static () => OpcVariant.FromBoolean(DateTimeOffset.UtcNow.Second % 2 == 0),
    };

    public int Count => _tags.Count;

    public bool TryRead(string itemId, out OpcVariant value)
    {
        if (_tags.TryGetValue(itemId, out Func<OpcVariant>? read))
        {
            value = read();
            return true;
        }

        value = OpcVariant.Empty;
        return false;
    }
}

public sealed class SignalPublisher : BackgroundService
{
    private readonly IOpcDaDataChangePublisher _publisher;
    private readonly ILogger<SignalPublisher> _logger;
    private int _transaction;

    public SignalPublisher(IOpcDaDataChangePublisher publisher, ILogger<SignalPublisher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            var change = new OpcDaDataChange(
                TransactionId: Interlocked.Increment(ref _transaction),
                GroupServerHandle: 0,
                MasterQuality: unchecked((short)OpcQuality.Good.RawValue),
                MasterError: OpcResultId.Ok.Code,
                Items:
                [
                    new OpcDaDataChangeItem(
                        ClientHandle: 1001,
                        Value: OpcVariant.FromDouble(Random.Shared.NextDouble() * 100.0),
                        Quality: OpcQuality.Good,
                        Timestamp: DateTimeOffset.UtcNow,
                        Error: OpcResultId.Ok.Code),
                ]);

            await _publisher.PublishAsync(change, stoppingToken).ConfigureAwait(false);
            _logger.LogDebug("Published DA data-change transaction {Transaction}", change.TransactionId);
        }
    }
}
```

This code uses the real hosting extension names: `AddClassicServer`, `AddClassicClsidRegistry`, and `AddOpcDaServer<T>`. Older draft snippets used longer `AddOpcClassic*` names; do not copy those.

## appsettings.json and CLSID metadata

`AddClassicClsidRegistry` reads `Opc.Classic:Servers`. The options passed to `AddOpcDaServer` are enough for the managed host, but keeping configuration metadata beside your application helps discovery tools and deployment automation.

```json
{
  "Opc.Classic": {
    "Servers": [
      {
        "Clsid": "4E3F63E7-4CC7-4E77-A59E-6462A1002001",
        "ProgId": "Contoso.ManagedDa.1",
        "FriendlyName": "Contoso Managed OPC DA Server",
        "AssemblyName": "Contoso.ManagedDa",
        "TypeName": "Contoso.ManagedDa.ManagedDaServer"
      }
    ]
  }
}
```

In Windows interoperability scenarios a registry writer or deployment script still needs to map the ProgID/CLSID to an activation endpoint that the Windows client can discover. Keep the ProgID stable: many HMIs store the ProgID in project files.

## Tag tree wiring

The repository sample `TagTree` shows a Matrikon-style namespace with `Random.*`, `Bucket Brigade.*`, wave tags, and error tags. Production systems should keep the same separation:

1. The tree answers browse and metadata questions.
2. Each leaf has a read delegate, write delegate, canonical type, and access-right mask.
3. The server implementation maps missing tags to `OPC_E_UNKNOWNITEMID`, denied writes to `OPC_E_BADRIGHTS`, and type mismatches to `OPC_E_BADTYPE`.
4. The host layer should not know anything about process values.

A simple tree entry can be represented as:

```csharp
public sealed record TagNode(
    string ItemId,
    VarType CanonicalType,
    bool Readable,
    bool Writable,
    Func<OpcVariant> Read,
    Func<OpcVariant, bool> Write);
```

That record is not part of the library; it is your domain model. Keeping it small makes it easy to unit test validation and error mapping without opening DCOM sockets.

## Group lifecycle

OPC DA groups are server-side state. A client calls `AddGroup`, then performs item management and synchronous/asynchronous I/O through group interfaces. The managed `IOpcDaServer.AddGroupAsync` should therefore return an opaque server handle and store all group state under that handle. Do not use the group name as the primary key. Names can collide, be empty, or be generated by the server.

When a client calls `RemoveGroup`, respect the `force` flag. With `force=false`, a real server may reject removal if callbacks or outstanding asynchronous operations are in flight. With `force=true`, remove the group and release callbacks. If the handle is unknown, return or throw an error that maps to `OPC_E_INVALIDHANDLE`. Throwing `OpcDaException` with `OpcResultId.InvalidHandle` preserves those semantics in the managed interface.

## Subscriptions and data changes

DA subscriptions are callback connections. `IOpcDaDataChangePublisher` is the server-side fan-out point. Your implementation produces `OpcDaDataChange` batches; the publisher delivers them to advised `IOPCDataCallback` subscribers. Keep batches coherent:

- `TransactionId` identifies refresh or async transactions.
- `GroupServerHandle` identifies the group that produced the callback.
- `MasterError` is the batch-level HRESULT.
- Each `OpcDaDataChangeItem` carries the client handle, value, quality, timestamp, and per-item error.

The sample publisher code above emits a single synthetic item once per second. A real server should publish only active items in active groups, respect deadband, and send keep-alive callbacks when configured.

## HRESULT mapping

OPC clients depend on HRESULT semantics. `S_OK` means success, `S_FALSE` means partial success or no more data depending on the method, and `OPC_S_*` values are successful warnings. `E_FAIL` is a last resort. Prefer specific values from `OpcResultId`:

```csharp
OpcResultId result = tagExists
    ? OpcResultId.Ok
    : OpcResultId.UnknownItemId;

OpcException.ThrowIfFailed(result, "ValidateItems");
```

Do not collapse everything to `E_FAIL`. Good clients show per-item text to operators; bad mapping makes troubleshooting impossible.

## Production pitfalls

- Start with `ListenAddress = "127.0.0.1:0"` only for development. Production hosts need a stable endpoint and firewall rules.
- Do not expose writeable tags until access control is in place.
- Keep group handles unique for the process lifetime; handle reuse can confuse stale callbacks.
- Watch memory pressure from slow callback consumers. `OpcDaDataChangePublisher` isolates subscriber exceptions, but your server still controls queueing and sampling.
- Test with at least one strict Windows client and one managed loopback client.

## Interoperability test plan

A managed DA server is not production-ready just because it starts. Build an interoperability matrix early. At minimum, test one managed client, one strict Windows OPC DA client, and one diagnostic browser. For every client, record whether it can discover the ProgID, activate the CLSID, call `GetStatus`, add a group, add a known item, read a value, receive one callback, remove the group, and disconnect without leaving server-side state behind.

Test negative cases too. Browse or validate an unknown item and verify the client sees `OPC_E_UNKNOWNITEMID`. Attempt to write a read-only tag and verify `OPC_E_BADRIGHTS`. Request an unrealistically fast update rate and verify the server revises the rate or returns `OPC_S_UNSUPPORTEDRATE` semantics instead of silently promising a rate it cannot deliver. Remove a group twice and ensure the second call returns a handle-related error instead of corrupting the registry.

Use packet-integrity settings that match production from the beginning. A server tested with anonymous or connect-level authentication may fail the first time a hardened Windows client attempts packet integrity. DCOM hardening changed the default reality for OPC Classic; make the secure path the normal path, not a late-stage toggle.

## Designing the tag catalog for change

The sample `TagTree` is intentionally small, but production tag catalogs evolve. Put tag metadata in one place: item ID, canonical `VarType`, access rights, engineering-unit metadata, EU range, description, and read/write delegates. A browse operation should be a projection over the same catalog used by validation and reads. If browse and read use different sources, clients will eventually browse a tag that cannot be read or read a tag that never appears in browse.

Treat simulated tags, calculated tags, and device-backed tags uniformly at the API boundary. The server should not care whether `Random.Real8` comes from a random generator or a PLC driver; it should only care that the read delegate returns an `OpcVariant`, `OpcQuality`, timestamp, and HRESULT. This abstraction is what lets you run deterministic unit tests around error mapping without connecting to process hardware.

## Operational lifecycle

Plan for graceful stop. When the host receives shutdown, stop accepting new groups, cancel sampling timers, publish final keep-alive or shutdown state if your client profile expects it, unadvise callbacks, and then release group state. Avoid abrupt process exit during writes. If writes are forwarded to field devices, track in-flight write operations and expose clear logs when shutdown interrupts them.

Document ownership of ProgIDs and CLSIDs. Changing either one is a breaking change for legacy clients. If you must create a new major version, register a new ProgID such as `Contoso.ManagedDa.2` and keep the old one available until clients migrate.

## Versioning and compatibility policy

Treat the server's public namespace as an API. A tag rename can break a client just as surely as a method signature change breaks code. Publish a compatibility policy for item IDs, access rights, canonical data types, and engineering-unit metadata. When tags must be retired, keep aliases or return clear vendor text for a deprecation period. Legacy HMI projects are often hard to update, so compatibility discipline saves commissioning time.

## Next steps

- Compare your implementation with `samples\Opc.Classic.Samples.DaServer\SampleDaServer.cs` and `TagTree.cs`.
- Build a client with [01-build-your-first-da-client.md](01-build-your-first-da-client.md).
- Deploy the server with [03-cross-platform-deployment.md](03-cross-platform-deployment.md).
- Review DCOM packet-integrity requirements in [../cookbook/05-dcom-hardening-pkt-integrity-explainer.md](../cookbook/05-dcom-hardening-pkt-integrity-explainer.md).

## References

- OPC Data Access 3.00, especially `IOPCServer`, `IOPCGroupStateMgt`, `IOPCItemMgt`, `IOPCSyncIO`, and `IOPCDataCallback`.
- [MS-DCOM] for activation and object references.
- [MS-RPCE] for packet signing, fragmentation, and call semantics.
- Repository: `samples\Opc.Classic.Samples.DaServer\` and `src\Opc.Classic.Da\Hosting\`.


