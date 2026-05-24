# Opc.Classic adoption guide

This guide is the practical starting point for adding `Opc.Classic` to an application, test suite, gateway, or managed OPC Classic server. It covers the concepts you need in the first twenty minutes, then points to the deeper architecture and cookbook material when you are ready to customize transport, authentication, hosting, and discovery.

> **Status note:** the repository is still pre-1.0. The package names and namespaces use the final dotted `Opc.Classic.*` form, but packages are not expected on nuget.org until the 1.0.0 release train. Until then, reference project outputs, a local feed, or CI-produced packages.

## 1. Introduction

`Opc.Classic` is a cross-platform, NativeAOT-compatible .NET 10 implementation of OPC Classic for legacy DA, AE, HDA, DX, Cpx, Batch, Commands, Security, and XML-DA integrations. Use it to talk to existing OPC Classic servers, host managed servers, remove .NET Framework-only client dependencies, and secure DCOM-style connections with NTLMv2 or Kerberos/SPNEGO.

OPC UA is recommended for greenfield systems, but many plants still depend on DCOM-based servers. `Opc.Classic` exists for those brownfield and bridge scenarios. The MIT license keeps adoption straightforward for proprietary products, internal tools, and open-source services. The managed DCOM stack avoids Windows COM runtime APIs for normal client/server use; Windows-only registry and native COM interop features stay behind platform guards.

## 2. Installation

When the 1.0.0 packages are published, install only the spec areas you need:

```bash
dotnet add package Opc.Classic.Core
dotnet add package Opc.Classic.Da      # DA client/server types and DCOM projections
dotnet add package Opc.Classic.Ae      # AE client/server projections
dotnet add package Opc.Classic.Hda     # HDA client/server projections
dotnet add package Opc.Classic.Hosting # managed server hosting and CLSID registry support
```

During the alpha period, packages may not exist on nuget.org. Use one of these approaches instead:

1. reference the projects directly in a repo-local solution;
2. publish packages to a local folder feed from CI;
3. consume artifacts from the repository build pipeline.

Recommended minimum for a DA client is `Opc.Classic.Core`, `Opc.Classic.Da`, and the managed DCOM transport package. Recommended minimum for a managed DA server is `Opc.Classic.Core`, `Opc.Classic.Da`, `Opc.Classic.Hosting`, and `Microsoft.Extensions.Hosting`.

### Core concepts you will see everywhere

`OpcUrl` parses `opcda://`, `opcae://`, `opchda://`, `opcdx://`, and `opc.xml-da://` URLs where the path is a ProgID or CLSID. `OpcConnectData` groups URL, `NetworkCredential`, authentication mode, packet protection, and optional timeout; it defaults to NTLMv2 plus packet integrity. `OpcVariant` is the AOT-clean COM `VARIANT` projection (`FromDouble`, `FromString`, `FromBoolean`, `FromSafeArray`). `OpcQuality` models the 16-bit DA quality word, including quality kind, substatus, limit bits, and vendor extension.

## 3. Hello World — DA client

The generated DA client surface lives in `Opc.Classic.Da.Dcom`. Each `[OpcInterface]` interface has a generated client proxy class, for example `IOPCServerClientProxy`, that encodes request bodies, calls an `ICallChannel`, and decodes response bodies.

The following sample demonstrates the current low-level pattern: parse an OPC URL, create credentials, construct an auth context, connect an `ICallChannel`, and call `IOPCServer::GetStatus`. Replace the endpoint and CLSID with values discovered from your environment.

```csharp
using Opc.Classic;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Transport;
using Opc.Classic.Dcom.Rpc.Auth.ntlm; // temporary low-level NTLM auth factory
using System.Net;

var url = OpcUrl.Parse("opcda://localhost/Opc.Classic.Samples.DaServer.1");
var credentials = new NetworkCredential("user", "password", "WORKGROUP");
var connectData = OpcConnectData.WithNtlmV2(url, credentials);
var authCtx = NtlmAuthentication.CreateAuthContext(connectData);

// The DcomCallChannel requires a concrete IAsyncTransportFactory. The Phase 2C
// contract is in place; production TCP transport is queued. For tests today,
// use InMemoryAsyncTransport or InMemoryCallChannel.
var transportFactory = new TcpAsyncTransportFactory();
var channelFactory = new DcomCallChannelFactory(transportFactory);
var channel = await channelFactory.ConnectAsync(
    new IPEndPoint(IPAddress.Parse("127.0.0.1"), 12345),
    Guid.Empty, // already-bound channel; use a CLSID for activation flows
    authCtx,
    CancellationToken.None);

var server = new IOPCServerClientProxy(channel);
var status = await server.GetStatusAsync(CancellationToken.None);
Console.WriteLine($"Server state: {status.State}, vendor: {status.VendorInfo}");

if (channel is IAsyncDisposable disposable)
{
    await disposable.DisposeAsync();
}
```

> **Current transport caveat:** the generated client proxies are real, but the production TCP `IAsyncTransportFactory` is still part of the follow-up work. Use `Opc.Classic.Testing.InMemoryAsyncTransport` for transport-level unit tests and `Opc.Classic.Testing.InMemoryCallChannel` for proxy and loopback integration tests until the TCP factory lands.

For tests, build an `InMemoryCallChannel` that returns fixture `NdrCallResult` payloads and pass it to the generated proxy. Current high-value DA surfaces include `IOPCServer`, `IOPCGroupStateMgt`, `IOPCItemMgt`, `IOPCSyncIO`, `IOPCSyncIO2`, `IOPCAsyncIO2`, `IOPCAsyncIO3`, `IOPCDataCallback`, `IOPCBrowse`, and discovery-related `IOPCServerList` interfaces. Multi-output COM pointer shapes are being filled in incrementally.

## 4. Hello World — DA server

Managed server hosting uses `Microsoft.Extensions.Hosting`. Register the shared OPC Classic hosted service, register a CLSID registry, then register your DA implementation with `AddOpcDaServer<T>()`.

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Opc.Classic;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddClassicServer();
builder.Services.AddClassicClsidRegistry(builder.Configuration);
builder.Services.AddOpcDaServer<MyDaServer>(opt =>
{
    opt.Clsid = new Guid("8F7C1B14-9A6E-4E4D-B5E6-5B7DCC1F2B3A");
    opt.ProgId = "My.OPC.Server.1";
    opt.FriendlyName = "My Managed DA Server";
    opt.ListenAddress = "127.0.0.1:0";
});

await builder.Build().RunAsync();

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

    public Task RemoveGroupAsync(
        int serverGroupHandle,
        bool force,
        CancellationToken ct = default) =>
        Task.CompletedTask;

    public Task<string> GetErrorStringAsync(
        int errorCode,
        int localeId,
        CancellationToken ct = default) =>
        Task.FromResult($"Error 0x{errorCode:X8}");
}
```

The current extension method names are `AddClassicServer()` and `AddClassicClsidRegistry(...)`. If you see older draft snippets using `AddOpcClassicServer()` or `AddOpcClassicClsidRegistry(...)`, use the current names from `Opc.Classic.Hosting`.

A configuration-backed registry uses the `Opc.Classic:Servers` section with `Clsid`, `ProgId`, `AssemblyName`, `TypeName`, and optional `FriendlyName` entries. See `samples/Opc.Classic.Samples.CttServer/Program.cs` for the repository's minimal CTT-oriented managed DA server pattern.

## 5. Authentication scenarios

OPC Classic over DCOM is sensitive to authentication settings. Hardened Windows hosts reject anonymous and low-protection activation. The safe baseline is NTLMv2 with packet integrity.

### NTLMv2 default

Use `NetworkCredential` and `OpcConnectData.WithNtlmV2(...)`:

```csharp
var url = OpcUrl.Parse("opcda://plc-gateway/Matrikon.OPC.Simulation.1");
var credential = new NetworkCredential("opc-reader", "password", "PLANT");
var connectData = OpcConnectData.WithNtlmV2(
    url,
    credential,
    OpcProtectionLevel.Integrity);

var authCtx = NtlmAuthentication.CreateAuthContext(connectData);
```

NTLMv2 is the cross-platform default. It uses extended session security, 128-bit keys, and packet signing when `OpcProtectionLevel.Integrity` or `Privacy` is selected. Use `Privacy` when you need encryption in addition to signing.

### Kerberos via Kerberos.NET

Kerberos is preferred in Active Directory environments because it avoids NTLM relay risks and gives you service-principal based identity. The public Kerberos context takes a `KerberosAuthInfo` record:

```csharp
using Opc.Classic;
using Opc.Classic.Dcom.Kerberos;

var kerberos = new KerberosAuthInfo(
    realm: "PLANT.EXAMPLE.COM",
    spn: "RPCSS/opc-host.plant.example.com",
    username: "opc-reader@PLANT.EXAMPLE.COM",
    domain: "PLANT",
    password: "password",
    keytabPath: null);

var authCtx = new KerberosAuthContext(
    kerberos,
    channelBindings: null,
    protectionLevel: OpcProtectionLevel.Integrity);
```

Kerberos requires a reachable KDC, correct DNS, time synchronization, and an SPN that matches the service account. Test environments in the repository use containerized KDC fixtures where possible.

You can also ask `OpcConnectData` for Kerberos semantics and let the DCOM authentication factory build the `KerberosAuthInfo` from the URL and `NetworkCredential`:

```csharp
var credential = new NetworkCredential(
    "opc-reader@PLANT.EXAMPLE.COM",
    "password",
    "PLANT.EXAMPLE.COM");
var connectData = OpcConnectData.WithKerberos(url, credential);
var authCtx = NtlmAuthentication.CreateAuthContext(connectData);
```

### SPNEGO negotiation

`KerberosAuthContext` emits SPNEGO tokens around the Kerberos AP-REQ/AP-REP exchange. In a domain environment, prefer Kerberos first and fall back to NTLMv2 only when policy allows it:

```csharp
IAuthContext authCtx;
try
{
    authCtx = new KerberosAuthContext(kerberos);
}
catch (Exception) when (allowNtlmFallback)
{
    authCtx = NtlmAuthentication.CreateAuthContext(
        OpcConnectData.WithNtlmV2(url, credential));
}
```

Keep fallback explicit. Some regulated environments disable NTLM entirely. If fallback is enabled, log that the connection did not use Kerberos so operators can fix SPN or KDC issues.

### Channel binding for TLS-protected endpoints

Extended Protection for Authentication (EPA) binds the authentication token to the outer TLS channel. For TLS-protected DCOM endpoints, compute channel bindings from the server certificate and pass them to `KerberosAuthContext`:

```csharp
using Opc.Classic.Security;

byte[] serverCertificateDer = GetServerCertificateBytes();
var bindings = ChannelBindingsFactory.ForTlsServerEndpoint(serverCertificateDer);
var authCtx = new KerberosAuthContext(
    kerberos,
    bindings,
    OpcProtectionLevel.Integrity);
```

For NTLMv2, the stack preserves the same EPA concept via the channel-binding AV pair. When you terminate TLS in a proxy, make sure the certificate hash given to the authentication layer is the certificate seen by the client.

## 6. Discovery — finding OPC servers

Discovery is modeled as `IOpcDiscovery`, which returns an async stream of `OpcServerEntry` records.

Use `LocalEnum` for local configuration and, on Windows, local COM registry enumeration:

```csharp
using Opc.Classic.Discovery;

var localEnum = new LocalEnum(configuration);
await foreach (var entry in localEnum.DiscoverAsync())
{
    Console.WriteLine($"{entry.ProgId} = {entry.Clsid}: {entry.FriendlyName}");
}
```

You can also seed `LocalEnum` directly from memory when you do not want configuration. Combine strategies with `OpcDiscoveryFactory`; it fans out and de-duplicates by CLSID:

```csharp
var remoteRegistry = new RemoteRegistryEnum(
    "opc-host.plant.example.com",
    new NetworkCredential("opc-reader", "password", "PLANT"));

var opcEnum = new OpcEnumClient(
    OpcUrl.Parse("opcda://opc-host.plant.example.com/OPC.ServerList.1"));

var discovery = new OpcDiscoveryFactory(localEnum, remoteRegistry, opcEnum);
await foreach (var entry in discovery.DiscoverAsync("opc-host.plant.example.com"))
{
    Console.WriteLine($"{entry.Host} {entry.ProgId} {entry.Clsid}");
}
```

Current status:

- `LocalEnum` is implemented for configuration and Windows local registry enumeration.
- `RemoteRegistryEnum` is the remote registry strategy scaffold.
- `OpcEnumClient` is the OPC ServerList / OpcEnum strategy scaffold.
- `OpcDiscoveryFactory` skips scaffold strategies that throw `NotImplementedException`, so you can compose the future strategies now without breaking local discovery.

## 7. Cross-platform considerations

`Opc.Classic` is designed to run on Linux, macOS, and Windows.

### Linux and macOS

Use the normal `Opc.Classic.*` packages. The DCOM/MSRPC pieces are managed and do not require the Windows COM runtime. This is the primary value proposition for gateways, collectors, cloud-side bridges, containerized services, and AOT workers that still need to reach legacy OPC Classic servers.

Typical deployment: discover or configure a Windows OPC Classic endpoint, authenticate with NTLMv2 or Kerberos/SPNEGO, connect through the managed DCOM channel, then expose the data through your application.

### Windows-specific integration

Windows-only features are guarded with `[SupportedOSPlatform("windows")]`. Examples include local registry enumeration and `WindowsRegistryClsidWriter`, which writes `HKLM\SOFTWARE\Classes\CLSID` registrations for native COM client activation compatibility.

Writing HKLM usually requires administrative rights. Treat registry writing as an installer or setup-time operation, not something your service does on every startup.

### Native COM client interop

Native C++ COM clients can connect to managed `Opc.Classic` servers on Windows through the server activation flow. That interop path is Windows-only because native COM activation and registry-based COM catalogs are Windows concepts. The managed server implementation itself remains portable.

## 8. AOT publishing

The libraries are authored to be trimmable and NativeAOT-compatible. Publish your application normally with `PublishAot=true`:

```bash
dotnet publish -c Release -r linux-x64 -p:PublishAot=true
```

For stricter CI validation, publish `samples/Opc.Classic.Samples.AotCanary/` with `-p:TreatWarningsAsErrors=true`; it is the verified smoke test for AOT cleanliness. Keep reflection-heavy plugins, dynamic dispatch, or runtime-generated serializers outside `src/*` libraries or replace them with source generators.

Avoid banned source-library patterns: `Reflection.Emit`, `Expression<T>.Compile()`, `MethodInfo.Invoke`, runtime string-to-type activation, `[ComImport]`, and native COM marshal helpers. The project uses generated dispatch and proxies instead.

## 9. Spec coverage quick reference

| Spec area | Current support | Notes |
| --- | --- | --- |
| DA | Full client + server path for the Phase 6 managed stack | Generated client proxies, server dispatchers, `IOpcDaServer`, hosting registration, loopback tests, and sample CTT server. Some COM pointer and multi-output shapes continue to be filled in as feature requests. |
| AE | Client + server scaffold | Interfaces and hosting slots are in place; per-method bodies are Phase 7 work. |
| HDA | Client + server scaffold | Interfaces and hosting slots are in place; per-method bodies are Phase 8 work. |
| DX | Managed types + interface partials | Add concrete method bodies as scenarios require them. |
| Cpx | Managed types + interface partials | Complex data codecs are represented; scenario coverage grows per feature request. |
| Batch | Managed types + interface partials | Batch category IDs and projections are present; method bodies are incremental. |
| Commands | Managed types + interface partials | Command interfaces are modeled for generator expansion. |
| Security | Managed types + interface partials | DCOM authentication is active; OPC Security spec methods are partial/scaffold. |
| XML-DA | Full HTTP/SOAP client + server | Phase 9F complete; use for HTTP/SOAP XML-DA endpoints instead of DCOM. |

Treat the table as a release-readiness snapshot, not a permanent boundary. The generator architecture is intended to make method coverage additive without changing public modeling conventions.

## 10. Migration paths

### From OPC Foundation .NET API

Older OPC Foundation .NET Framework APIs use types such as `Opc.URL`, `Opc.ConnectData`, `Opc.Da.Server`, `Opc.Da.Item`, and synchronous methods. `Opc.Classic` uses immutable/AOT-clean value types, `NetworkCredential`, and async methods.

| OPC Foundation API | Opc.Classic equivalent |
| --- | --- |
| `Opc.URL` | `OpcUrl` |
| `Opc.ConnectData` | `OpcConnectData` |
| custom OPC credentials | `System.Net.NetworkCredential` |
| `Opc.Da.Server` | generated DA proxies such as `IOPCServerClientProxy` over `ICallChannel` |
| `Opc.Da.ItemValue` / `ItemValueResult` | `OpcVariant`, DA item result records, `OpcQuality` |
| synchronous `Read` / `Write` | async generated methods, e.g. `WriteAsync`, `GetStatusAsync`, and spec-specific read methods as they land |
| COM runtime activation | managed DCOM activation/channel flow |

Migration checklist: replace URL parsing with `OpcUrl.Parse(...)`, use `OpcConnectData.WithNtlmV2(...)` or `WithKerberos(...)`, move blocking calls to `await`, convert values through `OpcVariant`, dispose managed channels with `using`/`await using`, and keep packet-integrity defaults.

### From OpcDaSDK or other third-party libraries

Most third-party libraries expose a familiar flow: create a server object, connect, add a group, add items, read/write, and subscribe. In `Opc.Classic`, those operations map to generated proxy methods over an `ICallChannel` and hosted server interfaces. The biggest changes are cross-platform managed DCOM, async APIs, explicit authentication, `OpcVariant`/`OpcQuality`, no runtime code generation, and DI-based managed hosting. Audit DCOM security defaults during migration; old anonymous or connect-level samples often fail on hardened Windows servers.

### From OPC UA

OPC UA and OPC Classic are not the same protocol family. UA is modern, service-oriented, firewall-friendly, and recommended for greenfield systems. OPC Classic is the legacy COM/DCOM family.

Use `Opc.Classic` when you must integrate with existing DA/AE/HDA/XML-DA servers or expose a Classic-compatible server to legacy clients. For new plant models and new device connectivity, prefer OPC UA unless you have a specific Classic compatibility requirement.

A common pattern is a bridge: read legacy DA tags through `Opc.Classic`, normalize values and qualities, then expose OPC UA, MQTT, historian, or REST output from your application.

## 11. Common pitfalls and troubleshooting

### NTLMv2 password encoding

NTLMv2 hashes use the Unicode password representation required by MS-NLMP: UTF-16 little-endian. Do not pre-hash passwords, convert them through UTF-8 bytes, or trim domain/user casing unexpectedly. Pass the original password to `NetworkCredential` and let the stack derive the response.

### DCOM hardening and packet integrity

Microsoft DCOM hardening for KB5004442 requires at least `RPC_C_AUTHN_LEVEL_PKT_INTEGRITY` for activation against patched Windows servers. `OpcConnectData` defaults `OpcProtectionLevel.Default` to `Integrity`. If you force `None`, `Connect`, or legacy `Call`, expect activation failures or Windows Event ID 10036 on the server.

### SAFEARRAY limitations

`OpcVariant.FromSafeArray(...)` supports the current managed SAFEARRAY carrier. Today, focus on one-dimensional scalar arrays. Multi-dimensional arrays, arrays of nested variants, and unusual COM automation element types should be treated as compatibility work items.

### Server hosting registration

Calling `AddOpcDaServer<T>()` registers your `IOpcDaServer` and the DA host, but the process also needs the shared hosting service and a CLSID registry:

```csharp
services.AddClassicServer();
services.AddClassicClsidRegistry(configuration);
services.AddOpcDaServer<MyDaServer>(...);
```

The hosted service drives all registered `IOpcServerHost` instances. If the service is missing, your server implementation may be in the container but no listener lifecycle is started.

### ProgID and CLSID mismatches

An OPC URL can identify a server by ProgID or CLSID. Discovery results and server registration must agree on both. If activation by ProgID fails, try resolving the CLSID with discovery and connecting with a CLSID URL. On Windows, verify `HKLM\SOFTWARE\Classes\CLSID\{...}` and `ProgID` registrations when native COM clients are involved.

### Kerberos troubleshooting

Kerberos failures usually come from environment configuration: unreachable KDC, DNS/SPN mismatch, clock skew, missing `RPCSS/host` SPN, realm casing, or blocked KDC/DCOM ports. Validate with a known Kerberos tool first, then run the repository Kerberos/Testcontainers fixture if you are changing the auth stack.

### SPNEGO fallback surprises

SPNEGO is a negotiation container, not a guarantee that Kerberos was used. If your policy requires Kerberos, fail closed when Kerberos cannot be acquired. If fallback to NTLMv2 is acceptable, make it explicit and observable in logs.

### Transport availability

`DcomCallChannel` consumes an `IAsyncTransport`. The transport contract exists and in-memory implementations cover unit and loopback tests. Production TCP transport is tracked as follow-up work. Until that ships, do not promise remote production DA reads through the low-level DCOM channel without providing your own transport factory.

### Native COM expectations

Windows COM clients expect registry entries, apartment/threading conventions, and activation behavior. Managed hosting handles the server implementation, but native COM activation compatibility still needs Windows setup and the Phase 4C/4F activation path. Test with the native C++ samples in `COM/` when validating interoperability.

## 12. Where to next

- `docs/ARCHITECTURE.md` — full design of the managed DCOM stack, generated proxies, hosting, and AOT choices.
- `docs/cookbook/` — focused how-to articles for specific adoption tasks.
- `docs/RELEASE_PROCESS.md` — how the project builds, validates, and ships packages.
- `samples/Opc.Classic.Samples.*` — runnable examples, including the CTT sample server and AOT canary.
- `CHANGELOG.md` — feature inventory by alpha release and known limitations.

For a first proof of concept, start with local discovery or a known ProgID, use NTLMv2 with packet integrity, call `IOPCServerClientProxy.GetStatusAsync`, then move to DA item/group methods or managed hosting.
