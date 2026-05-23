# OCMDA001 — Legacy OPC DA server construction

`OCMDA001` reports `new OpcCom.Da.Server(url)` and `new OpcCom.Server()` constructor usage. `Opc.Classic.Da` uses async connection factories and async disposal so connection setup can honor cancellation, retries, and DI-managed options.

## Before

```csharp
var server = new OpcCom.Da.Server(url);
```

## After

```csharp
await using var server = await OpcDaClient.ConnectAsync(url, options);
```

If the client is long-lived, prefer registering the DA client or factory in DI and injecting the abstraction into consumers instead of constructing it at the call site.
