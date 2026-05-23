# Host a managed OPC DA server consumed by a Windows COM client

## What this covers

Run a managed OPC DA server on Linux or macOS while legacy Windows DA clients connect through DA COM interfaces.

## Status / availability

Forward-looking: this requires Phase 4 server hosting, generated `LocalCoClass` dispatch, and the planned `OpcLocalCoClass` API. `docs\ARCHITECTURE.md` explains the transport split above `ICallChannel`. End-to-end interop is the Phase 14B GOLD STANDARD deliverable.

## Planned hosting shape

```csharp
using Microsoft.Extensions.Hosting;
using Opc.Classic;
using Opc.Classic.Da;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Hosting; // planned Phase 4 API

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<MyDaHandler>();

builder.Services.AddOpcLocalCoClass(new OpcLocalCoClass
{
    ProgId = "Contoso.ManagedOpcDa.1",
    ClassId = Guid.Parse("7f41b3e9-32ec-40c9-9e42-3e0e0fce5a11"),
    Interfaces = { typeof(IOPCServer), typeof(IOPCGroupStateMgt), typeof(IOPCSyncIO) },
    HandlerType = typeof(MyDaHandler),
});

builder.Services.AddOpcDcomServer(o =>
{
    o.Listen("0.0.0.0", port: 13550); // TCP RPC endpoint
    o.RequireProtectionLevel(OpcProtectionLevel.Integrity);
});

await builder.Build().RunAsync();
```

`MyDaHandler` maps `IOPCServer` calls to managed DA operations:

```csharp
public sealed class MyDaHandler
{
    public Task<OpcServerStatus> GetStatusAsync(CancellationToken ct) => ...;
    public Task<IReadOnlyList<ItemValueResult>> ReadAsync(IReadOnlyList<Item> items, CancellationToken ct) => ...;
}
```

## Authentication model

Pre-share an NTLMv2 machine or service account. The server validates NTLM Type3 messages per MS-NLMP and requires `OpcProtectionLevel.Integrity` or stronger. Kerberos/SPNEGO follows Phase 3D-F; see [03-kerberos-in-active-directory.md](03-kerberos-in-active-directory.md).

## Windows client side

The client still asks for `Contoso.ManagedOpcDa.1`. A Windows-side proxy registration points activation at the managed host endpoint, then `IOPCServer`, `IOPCGroupStateMgt`, and related calls flow to generated dispatch instead of reflection.

