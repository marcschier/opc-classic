# OCMDA001 — OPC DA server construction

`OCMDA001` reports `new OpcCom.Da.Server(url)` and `new OpcCom.Server()` constructor usage. Current `Opc.Classic.Da` consumers should depend on `IDaServer` instances created by DI or an application-specific async factory so connection setup can honor cancellation, retries, and async disposal.

## Before

```csharp
var server = new OpcCom.Da.Server(url);
```

## After

```csharp
await using IDaServer server = await daServerFactory.ConnectAsync(url, options, ct);
await server.SetClientNameAsync(clientName, ct);
```

Replace `daServerFactory` with your adapter for the target transport. If the client is long-lived, register `IDaServer` or its factory in DI; call `IDaServer.SetClientNameAsync` when legacy code used a COM client name for diagnostics.
