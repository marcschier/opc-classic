# Host a managed OPC DA server consumed by a Windows COM client

## What this covers

Run a managed OPC DA server on Linux, macOS, or Windows while Windows DA clients connect through Classic DA COM interfaces.

The reference is the DA server sample. It uses `AddClassicServer`, `AddClassicClsidRegistry`, and `AddOpcDaServer<T>`, registers `Opc.Classic.Samples.DaServer.1`, and reads `OPC_CLASSIC_SAMPLE_PORT` (default `51300`) or `OPC_CLASSIC_LISTEN_ADDRESS`. Related samples cover AE, HDA, loopback, an additional managed DA target (CttServer), the full-feature `Opc.Classic.Samples.SimulationServer`, OPC Security, and AOT scenarios; container conventions are in [../../samples/README.docker.md](../../samples/README.docker.md).

## Hosting shape

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Hosting;

var builder = Host.CreateApplicationBuilder(args);

int port = int.TryParse(
    Environment.GetEnvironmentVariable("OPC_CLASSIC_SAMPLE_PORT"),
    out int parsedPort) && parsedPort > 0 ? parsedPort : 51300;
string listenAddress = Environment.GetEnvironmentVariable("OPC_CLASSIC_LISTEN_ADDRESS")
    ?? $"0.0.0.0:{port}";

builder.Services.AddClassicServer();
builder.Services.AddClassicClsidRegistry(builder.Configuration);
builder.Services.AddOpcDaServer<MyDaServer>(options =>
{
    options.Clsid = Guid.Parse("7f41b3e9-32ec-40c9-9e42-3e0e0fce5a11");
    options.ProgId = "Contoso.ManagedOpcDa.1";
    options.FriendlyName = "Contoso Managed OPC DA Server";
    options.ListenAddress = listenAddress;
});

await builder.Build().RunAsync();
```

## Managed DA implementation

`MyDaServer` implements `IOpcDaServer`. Generated server dispatchers route DCOM opnums to the managed methods.

```csharp
using Opc.Classic.Da.Hosting;

public sealed class MyDaServer : IOpcDaServer
{
    public Task<OpcServerStatus> GetStatusAsync(CancellationToken ct = default) => ...;

    public Task<int> AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientHandle,
        int localeId,
        CancellationToken ct = default) => ...;

    public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken ct = default) => ...;

    public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken ct = default) => ...;
}
```

## Authentication model

Use NTLMv2 or Kerberos/SPNEGO with `OpcProtectionLevel.Integrity` or `Privacy`. The DCOM stack validates NTLMv2 messages per [MS-NLMP], supports Kerberos/SPNEGO, and includes RFC 5056 / RFC 5929 channel-binding support.

For Kerberos setup, see [Kerberos in Active Directory](03-kerberos-in-active-directory.md).

## Windows client side

The Windows client asks for `Contoso.ManagedOpcDa.1`. Registry setup maps that ProgID and CLSID to the managed server endpoint. After activation, `IOPCServer`, `IOPCGroupStateMgt(2)`, `IOPCItemMgt`, `IOPCSyncIO(2)`, `IOPCAsyncIO2/3`, and `IConnectionPoint(Container)` calls flow to generated server dispatchers or Windows CCW vtables. VARIANT and SAFEARRAY marshaling is implemented for the shipped DA paths; AE has full array marshaling for its shipped CCW methods, and HDA covers sync read/update, async update, playback, annotation insert, and async advise sample paths.

Native COM clients require normal Windows COM registration, DCOM permissions, firewall rules, and process identity configuration. Use the preserved OPC Foundation C++ sample clients and servers as compatibility references when validating a deployment.

## Validation aids

- `Opc.Classic.Samples.DaServer` — hosted managed DA server.
- `Opc.Classic.Samples.CttServer` — additional managed DA sample (different CLSID from `samples-da`).
- `Opc.Classic.Samples.LoopbackDemo` — generated proxy/dispatcher loopback without Windows COM registration.
