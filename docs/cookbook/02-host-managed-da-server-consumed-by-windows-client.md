# Host a managed OPC DA server consumed by a Windows COM client

Updated for Opc.Classic 0.4.0-alpha.1.

## What this covers

Run a managed OPC DA server on Linux, macOS, or Windows while legacy Windows DA clients connect through DA COM interfaces.

## Status / availability

`samples\Opc.Classic.Samples.DaServer` is the general managed DA hosting sample. It uses `AddClassicServer`, `AddClassicClsidRegistry`, and `AddOpcDaServer<T>` and registers `Opc.Classic.Samples.DaServer.1`. The related `AeServer`, `HdaServer`, and `CttServer` samples cover AE/HDA hosting and the CTT DA workflow target. Full Windows COM-client compatibility remains gated by the Phase 14B/14D conformance matrix.

## Hosting shape

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddClassicServer();
builder.Services.AddClassicClsidRegistry(builder.Configuration);
builder.Services.AddOpcDaServer<MyDaServer>(static options =>
{
    options.Clsid = Guid.Parse("7f41b3e9-32ec-40c9-9e42-3e0e0fce5a11");
    options.ProgId = "Contoso.ManagedOpcDa.1";
    options.FriendlyName = "Contoso Managed OPC DA Server";
    options.ListenAddress = "0.0.0.0:13550";
});

await builder.Build().RunAsync();
```

`MyDaServer` maps `IOPCServer` calls to managed DA operations:

```csharp
public sealed class MyDaServer : IOpcDaServer
{
    public Task<OpcServerStatus> GetStatusAsync(CancellationToken ct = default) => ...;
    public Task<int> AddGroupAsync(string name, bool active, int requestedUpdateRate, int clientHandle, int localeId, CancellationToken ct = default) => ...;
    public Task RemoveGroupAsync(int serverGroupHandle, bool force, CancellationToken ct = default) => ...;
    public Task<string> GetErrorStringAsync(int errorCode, int localeId, CancellationToken ct = default) => ...;
}
```

## Authentication model

Pre-share an NTLMv2 machine or service account. The server validates NTLM Type3 messages per MS-NLMP and requires `OpcProtectionLevel.Integrity` or stronger. Kerberos/SPNEGO follows the same `IAuthContext` seam; see [03-kerberos-in-active-directory.md](03-kerberos-in-active-directory.md).

## Windows client side

The client still asks for `Contoso.ManagedOpcDa.1`. A Windows-side proxy registration points activation at the managed host endpoint, then `IOPCServer`, `IOPCGroupStateMgt`, and related calls flow to generated dispatch instead of reflection. Generator-emitted client proxy classes now use names like `IOPCServerClientProxy`; the pre-0.4 underscore form was removed in 0.4.0-alpha.1.