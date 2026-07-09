# OCMDA002 — OPC DA Browse

`OCMDA002` reports synchronous `server.Browse(itemId, filters)` calls on OPC DA objects. Move browse operations to `IDaServer.BrowseAsync(itemId, filters, ct)` so callers can propagate `CancellationToken` and stream continuation pages without blocking remote DCOM/MSRPC work.

## Before

```csharp
var branches = server.Browse(itemId, filters);
```

## After

```csharp
await foreach (BrowseElement branch in server.BrowseAsync(itemId, filters, ct))
{
    await handler.HandleAsync(branch, ct);
}
```

Use the generated code fix as a starting point, then rename the receiver to your injected `IDaServer`. If legacy browse setup also set a client name, do that once with `IDaServer.SetClientNameAsync` after connection.
