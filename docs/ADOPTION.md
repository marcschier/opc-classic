# Opc.Classic adoption guide

This guide is the practical starting point for adding `Opc.Classic` to an application, test suite, gateway, or managed OPC Classic server. It focuses on current APIs and current repository capabilities. For deeper design details, see [ARCHITECTURE.md](ARCHITECTURE.md). For task-oriented walkthroughs, see [cookbook](cookbook/README.md).

Package IDs, namespaces, project names, and examples use the `Opc.Classic.*` root. 
The project is MIT licensed.

## 1. When to use Opc.Classic

Use `Opc.Classic` when an application needs OPC Classic interoperability from .NET 10:

- collect DA values from existing Classic servers;
- receive AE alarms and events;
- query HDA historians;
- host managed DA/AE/HDA servers for Classic clients;
- bridge Classic systems to OPC UA, MQTT, REST, historians, or cloud services;
- run Classic connectivity in Linux, macOS, Windows, containers, or NativeAOT workers.

OPC UA is the preferred protocol for new device models and new plant systems. `Opc.Classic` is for brownfield Classic compatibility and controlled bridging.

## 2. Installation and reference options

Install only the areas you need:

```bash
dotnet add package Opc.Classic.Core --prerelease
dotnet add package Opc.Classic.Da --prerelease
dotnet add package Opc.Classic.Ae --prerelease
dotnet add package Opc.Classic.Hda --prerelease
dotnet add package Opc.Classic.Hosting --prerelease
```

For source builds from the current checkout, use one of these approaches:

1. reference the projects directly in a repo-local solution;
2. publish packages to a local folder feed from CI;
3. consume packages produced by the repository build pipeline.

Recommended sets:

| Scenario | Packages / projects |
| --- | --- |
| DA client | `Opc.Classic.Core`, `Opc.Classic.Da`, `Opc.Classic.Dcom` |
| AE client | `Opc.Classic.Core`, `Opc.Classic.Ae`, `Opc.Classic.Dcom` |
| HDA client | `Opc.Classic.Core`, `Opc.Classic.Hda`, `Opc.Classic.Dcom` |
| Managed DA server | `Opc.Classic.Core`, `Opc.Classic.Da`, `Opc.Classic.Hosting`, `Microsoft.Extensions.Hosting` |
| Discovery | `Opc.Classic.Core`, `Opc.Classic.Discovery`, `Opc.Classic.Dcom` |
| Kerberos | `Opc.Classic.Dcom.Kerberos` plus domain/KDC configuration |

## 3. Core concepts

| Concept | Description |
| --- | --- |
| `OpcUrl` | Parses `opcda://`, `opcae://`, `opchda://`, `opcdx://`, and `opc.xml-da://` URLs where the path is a ProgID or CLSID. |
| `OpcConnectData` | Groups URL, `NetworkCredential`, auth mode, protection level, optional timeout, and optional RFC 5056 channel bindings. |
| `OpcProtectionLevel` | DCE/RPC packet protection. `Default` expands to `Integrity`. Use `Privacy` when confidentiality is required. |
| `OpcVariant` | AOT-clean COM `VARIANT` projection used by wire and value layers. |
| `OpcSafeArray` | Managed SAFEARRAY carrier for automation array payloads. |
| `OpcQuality` | Models the DA 16-bit quality word, including quality kind, substatus, limit bits, and vendor extension bits. |
| `ICallChannel` | Transport-independent generated proxy/dispatcher call surface. |
| Generated proxies | Types such as `IOPCServerClientProxy` encode DCOM requests and decode responses. |
| Generated dispatchers | Types such as `IOPCServerServerDispatcher` route DCOM opnums to managed server implementations. |

## 4. DA client patterns

The high-level DA contract is `IDaServer`. It is async-first and uses cancellation tokens throughout.

```csharp
using Opc.Classic.Da;

public static async Task ReadValuesAsync(IDaServer server, CancellationToken ct)
{
    OpcServerStatus status = await server.GetStatusAsync(ct);
    Console.WriteLine($"{status.VendorInfo}: {status.State}");

    var items = new[]
    {
        new Item("Plant.Temperature") { ClientHandle = 1001 },
        new Item("Plant.Pressure") { ClientHandle = 1002 },
    };

    IReadOnlyList<ItemValueResult> values = await server.ReadAsync(items, ct);
    foreach (ItemValueResult value in values)
    {
        Console.WriteLine($"{value.ItemName}: {value.Value} {value.Quality} {value.Timestamp:O}");
    }
}
```

For group-oriented work, create a subscription:

```csharp
await using IDaSubscription group = await server.CreateSubscriptionAsync(
    new SubscriptionState
    {
        Name = "process",
        ClientHandle = 5000,
        UpdateRateMs = 1000,
        Active = true,
        LocaleId = 0x0409,
    },
    cancellationToken);

await group.AddItemsAsync(items, cancellationToken);
IReadOnlyList<ItemValueResult> snapshot = await group.ReadAsync(
    items.Select(static item => item.ClientHandle).ToArray(),
    fromCache: true,
    cancellationToken);

await foreach (DataChange change in group.DataChanges.WithCancellation(cancellationToken))
{
    foreach (ItemValueResult item in change.Items)
    {
        Console.WriteLine($"{item.ItemName}: {item.Value} {item.Quality}");
    }
}
```

Set the OPC Common client name through the high-level convenience helper when servers or audits expect it:

```csharp
await server.SetClientNameAsync("ContosoGateway", ct);
```

Generated DCOM proxies are available when you work directly at the IDL projection layer:

```csharp
using Opc.Classic.Da.Dcom;
using Opc.Classic.Testing;

var channel = new InMemoryCallChannel(dispatcher.DispatchAsync);
var proxy = new IOPCServerClientProxy(channel);
OpcServerStatus status = await proxy.GetStatusAsync(cancellationToken);
```

For runnable client patterns, see `Opc.Classic.Samples.DaClient` and the full-feature `Opc.Classic.Samples.SimulationServer`.

## 5. Managed DA server hosting

Managed server hosting uses `Microsoft.Extensions.Hosting`. Register the shared Classic hosted service, a CLSID registry, and your per-spec server implementation.

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Opc.Classic;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddClassicServer();
builder.Services.AddClassicClsidRegistry(builder.Configuration);
builder.Services.AddOpcDaServer<MyDaServer>(static options =>
{
    options.Clsid = new Guid("8F7C1B14-9A6E-4E4D-B5E6-5B7DCC1F2B3A");
    options.ProgId = "My.OPC.Server.1";
    options.FriendlyName = "My Managed DA Server";
    options.ListenAddress = "127.0.0.1:0";
});

await builder.Build().RunAsync();
```

A minimal DA server implements `IOpcDaServer`:

```csharp
using Opc.Classic.Da.Hosting;

public sealed class MyDaServer : IOpcDaServer
{
    public Task<OpcServerStatus> GetStatusAsync(CancellationToken ct = default) =>
        Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = DateTimeOffset.UtcNow,
            CurrentTime = DateTimeOffset.UtcNow,
            LastUpdateTime = DateTimeOffset.UtcNow,
            State = OpcServerState.Running,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = "My Company",
            GroupCount = 0,
            BandWidth = 0xFFFFFFFF,
        });

    public Task<int> AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientHandle,
        int localeId,
        CancellationToken ct = default) =>
        Task.FromResult(clientHandle + 0x1000);

    public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken ct = default) =>
        Task.FromResult($"Error 0x{errorCode:X8}");
}
```

`AddClassicServer()` wires the hosted-service lifecycle. `AddClassicClsidRegistry(...)` supplies CLSID/ProgID metadata. `AddOpcDaServer<T>()` connects the implementation to generated DA dispatchers. AE and HDA hosting follow the same model in their sample apps.

## 6. Authentication scenarios

OPC Classic over DCOM is sensitive to authentication and packet-protection policy. The safe baseline is NTLMv2 or Kerberos with packet integrity.

### NTLMv2

```csharp
using System.Net;
using Opc.Classic;

var url = OpcUrl.Parse("opcda://plc-gateway/Matrikon.OPC.Simulation.1");
var credential = new NetworkCredential("opc-reader", "password", "PLANT");

var connectData = OpcConnectData.WithNtlmV2(
    url,
    credential,
    OpcProtectionLevel.Integrity,
    operationTimeout: TimeSpan.FromSeconds(30));
```

NTLMv2 uses extended session security and supports packet signing and sealing. `OpcProtectionLevel.Integrity` signs PDUs. `OpcProtectionLevel.Privacy` signs and encrypts.

### Kerberos and SPNEGO

Kerberos is preferred in Active Directory environments because it provides service-principal identity and avoids NTLM relay exposure.

```csharp
using System.Net;
using Opc.Classic;

var url = OpcUrl.Parse("opcda://opc01.plant.example.com/Matrikon.OPC.Simulation.1");
var credential = new NetworkCredential(
    "opc-reader@PLANT.EXAMPLE.COM",
    "password",
    "PLANT.EXAMPLE.COM");

var connectData = OpcConnectData.WithKerberos(
    url,
    credential,
    OpcProtectionLevel.Integrity);
```

Kerberos requires a reachable KDC, synchronized clocks, DNS that matches the service principal, and a service principal such as `RPCSS/opc01.plant.example.com`. SPNEGO wraps Kerberos or NTLM tokens for negotiation with peers that expect [MS-SPNG]/RFC 4178 semantics. Keep fallback explicit and observable in logs when policy permits NTLMv2 fallback.

### Channel binding

For TLS-protected endpoints, Extended Protection for Authentication binds authentication to the TLS server certificate. `OpcConnectData` accepts RFC 5056 channel bindings.

```csharp
using Opc.Classic;
using Opc.Classic.Security;

byte[] serverCertificateDer = GetServerCertificateBytes();
ChannelBindings bindings = ChannelBindingsFactory.ForTlsServerEndpoint(serverCertificateDer);

var connectData = OpcConnectData.WithNtlmV2(
    url,
    credential,
    OpcProtectionLevel.Integrity,
    channelBindings: bindings);
```

The channel-binding material follows RFC 5056 and RFC 5929 (`tls-server-end-point`).

## 7. Discovery

Discovery enumerates Classic servers before activation or connection.

```csharp
using Opc.Classic.Discovery;

var localEnum = new LocalEnum(configuration);
await foreach (OpcServerEntry entry in localEnum.DiscoverAsync(cancellationToken: cancellationToken))
{
    Console.WriteLine($"{entry.ProgId} = {entry.Clsid}: {entry.FriendlyName}");
}
```

Available discovery strategies include local configuration, Windows local registry enumeration, remote registry enumeration, and OPCEnum/DCOM discovery. Compose strategies with `OpcDiscoveryFactory` when a deployment needs multiple sources and CLSID/ProgID de-duplication.

## 8. Cross-platform considerations

### Linux and macOS

Use the normal `Opc.Classic.*` packages. The DCOM/MSRPC stack is managed and does not require the Windows COM runtime. This supports gateways, collectors, cloud-side bridges, containers, and NativeAOT workers that need Classic connectivity.

Typical deployment:

1. discover or configure a Windows OPC Classic endpoint;
2. build `OpcConnectData` with NTLMv2 or Kerberos;
3. connect through the managed DCOM channel;
4. expose values, events, or HDA data through the application.

The DA/AE/HDA sample servers can bind a real TCP listener by setting `OPC_CLASSIC_SAMPLE_PORT` (or `OPC_CLASSIC_LISTEN_ADDRESS`), and the sample clients dial that listener when `OPC_CLASSIC_SERVER_HOST` + `OPC_CLASSIC_SERVER_PORT` are set. `Opc.Classic.Samples.SimulationServer --listen` hosts DA/AE/HDA over real managed TCP transports from one simulated plant model. Without those variables, the clients keep the in-process loopback path for local development.

### Windows-specific integration

Windows-only integration is guarded with `[SupportedOSPlatform("windows")]`. Examples include registry enumeration and `WindowsRegistryClsidWriter`, which writes `HKLM\SOFTWARE\Classes\CLSID` registrations for native COM client activation compatibility.

Writing HKLM usually requires administrative rights. Treat registry writes as installer or setup-time work.

### Native COM clients

Native Windows COM clients can activate managed `Opc.Classic` servers through the Windows SCM/CCW path when CLSID/ProgID registration is configured. Full remote cold-activation through the managed listener is still gated on server-side NTLM bind handling; test native interop with the preserved C++ sample servers and representative client tools.

## 9. AOT publishing

Runtime libraries are trimmable and NativeAOT-compatible. Publish an application normally:

```bash
dotnet publish -c Release -r linux-x64 -p:PublishAot=true
```

For repository validation, publish the AOT canary with warnings as errors:

```powershell
dotnet publish samples\Opc.Classic.Samples.AotCanary -c Release -p:PublishAot=true -p:TreatWarningsAsErrors=true
```

Keep reflection-heavy plugins, runtime-generated serializers, and dynamic dispatch outside runtime libraries. Use source generators for code that must be static under trimming.

## 10. Spec coverage quick reference

| Area | What you get |
| --- | --- |
| DA | Client/server contracts, generated proxies and dispatchers, hosting, subscriptions, `IOpcAddressSpace`-backed browse/properties, read/write, callbacks, DA client/server samples. |
| AE | Event server/client contracts, categories, filters, subscriptions, generated projections, AE client/server samples. |
| HDA | Historical read/update/annotation/playback contracts, generated projections, HDA client/server samples. |
| Batch | Batch summaries, state/type models, filters, enumerations, generated projections. |
| Commands | Command metadata, state, invocation, and callback projections. |
| Cpx | Complex Data dictionaries, fields, type descriptions, and values. |
| DX | Data eXchange source server, connection, and configuration models. |
| Security | OPC Security projections plus DCOM authentication and packet-protection integration. |
| Discovery | Local, remote-registry, and OPCEnum discovery strategies. |
| XML-DA | HTTP/SOAP XML-DA DTOs, serializers, and client transport shape. |

The generated DCOM surface covers the current annotated OPC projections. The current validation sweep has 0 build errors / 0 warnings and all test projects green with only expected skipped tests.

## 11. Adoption from OPC NET API projects

OPC Foundation .NET Framework projects commonly use `Opc.URL`, `Opc.ConnectData`, `Opc.Da.Server`, synchronous group APIs, and COM runtime activation. In `Opc.Classic`, use the following mappings.

| OPC NET API concept | Opc.Classic concept |
| --- | --- |
| `Opc.URL` | `OpcUrl` |
| `Opc.ConnectData` | `OpcConnectData` |
| custom OPC credentials | `System.Net.NetworkCredential` |
| `Opc.Da.Server` | `IDaServer` or generated `IOPCServerClientProxy` |
| `Opc.Da.Subscription` | `IDaSubscription` |
| `Opc.Da.Item` | `Item` |
| `Opc.Da.ItemValue` | `ItemValue` / `ItemValueResult` |
| synchronous callbacks | `IAsyncEnumerable<DataChange>` |
| COM activation defaults | explicit `OpcConnectData` authentication and protection policy |

Connection setup with `Opc.Classic`:

```csharp
using System.Net;
using Opc.Classic;

var url = OpcUrl.Parse("opcda://win-opc01/Matrikon.OPC.Simulation.1");
var connectData = OpcConnectData.WithNtlmV2(
    url,
    new NetworkCredential("opc-reader", "password", "CORP"),
    OpcProtectionLevel.Integrity);
```

Subscription setup with `Opc.Classic`:

```csharp
await using IDaSubscription subscription = await server.CreateSubscriptionAsync(
    new SubscriptionState { Name = "process", Active = true, UpdateRateMs = 1000 },
    cancellationToken);

await subscription.AddItemsAsync(new[] { new Item("Random.Int1") { ClientHandle = 1 } }, cancellationToken);

await foreach (DataChange change in subscription.DataChanges.WithCancellation(cancellationToken))
{
    foreach (ItemValueResult item in change.Items)
    {
        Console.WriteLine($"{item.ItemName}: {item.Value} {item.Quality} {item.Timestamp:O}");
    }
}
```

Adoption checklist:

- parse Classic endpoints with `OpcUrl.Parse(...)`;
- choose `OpcConnectData.WithNtlmV2(...)` or `OpcConnectData.WithKerberos(...)`;
- keep packet integrity as the default;
- move blocking calls to `await`;
- represent values with `object?`, `OpcVariant`, and DA result records as appropriate;
- dispose channels, servers, and subscriptions with `using` or `await using`;
- use generated proxies/dispatchers or the high-level managed contracts rather than reflection-based COM wrappers.

The repository also contains `Opc.Classic.MigrationAnalyzer` for projects that want Roslyn diagnostics and code fixes around OPC NET API usage.

## 12. Common pitfalls and troubleshooting

### DCOM hardening and packet integrity

Microsoft DCOM hardening for KB5004442 requires at least `RPC_C_AUTHN_LEVEL_PKT_INTEGRITY` for activation against patched Windows servers. `OpcConnectData` defaults to `OpcProtectionLevel.Integrity`. If you force `None`, `Connect`, or `Call`, expect activation failures or Windows Event ID 10036 on the server.

### NTLMv2 password handling

NTLMv2 hashes use the Unicode password representation required by [MS-NLMP]: UTF-16 little-endian. Pass the original password to `NetworkCredential`; do not pre-hash, UTF-8 encode, or trim domain/user casing.

### Kerberos configuration

Kerberos failures usually come from environment configuration: unreachable KDC, DNS/SPN mismatch, clock skew, missing `RPCSS/host` SPN, realm casing, or blocked KDC/DCOM ports. Validate with a known Kerberos tool before debugging application code.

### SPNEGO fallback

SPNEGO is a negotiation container, not a guarantee that Kerberos is selected. If policy requires Kerberos, fail closed when a Kerberos ticket cannot be acquired. If NTLMv2 fallback is acceptable, make it explicit and log the selected mechanism.

### SAFEARRAY and VARIANT shapes

Classic servers can return scalar values, arrays, nested variants, byref values, and automation-specific shapes. Use `OpcVariant` and `OpcSafeArray` conversion helpers rather than manual casts when working at the wire layer.

### Server hosting registration

Calling `AddOpcDaServer<T>()` registers the server implementation and per-spec host. The application also needs the shared hosted service and CLSID registry:

```csharp
services.AddClassicServer();
services.AddClassicClsidRegistry(configuration);
services.AddOpcDaServer<MyDaServer>(...);
```

If the shared hosted service is missing, the DI container contains the server implementation but no listener lifecycle starts.

### ProgID and CLSID mismatches

An OPC URL can identify a server by ProgID or CLSID. Discovery results and server registration must agree on both. If activation by ProgID fails, resolve the CLSID with discovery and connect with a CLSID URL. On Windows, verify `HKLM\SOFTWARE\Classes\CLSID\{...}` and `ProgID` registrations when native COM clients are involved.

## 13. Samples to start from

The sample suite contains runnable apps for the main adopter paths.

| Sample | Start here when you need |
| --- | --- |
| `Opc.Classic.Samples.DaClient` | DA reads, browse, subscriptions, and generated proxy wiring. |
| `Opc.Classic.Samples.DaServer` | Managed DA server hosting. |
| `Opc.Classic.Samples.AeClient` | AE subscription consumption. |
| `Opc.Classic.Samples.AeServer` | Managed AE server hosting. |
| `Opc.Classic.Samples.HdaClient` | HDA query/playback client flow. |
| `Opc.Classic.Samples.HdaServer` | Managed HDA historical server hosting. |
| `Opc.Classic.Samples.LoopbackDemo` | In-memory generated proxy/dispatcher loopback. |
| `Opc.Classic.Samples.CttServer` | Conformance-test DA server behavior. |
| `Opc.Classic.Samples.OpcSecurityServer` | OPC Security reference server and ACL semantics. |
| `Opc.Classic.Samples.SimulationServer` | Full-feature simulation across DA/AE/HDA/Batch/Commands/Cpx/DX/Security/Discovery/XML-DA, MCP sessions, and optional real TCP hosting. |
| `Opc.Classic.Samples.AotCanary` | NativeAOT publish validation. |
