# OCMDA003 — OPC DA synchronous Read

`OCMDA003` reports synchronous `group.Read(items)` usage. The current `Opc.Classic.Da` replacements are `IDaServer.ReadAsync(items, ct)` for one-shot reads or `IDaSubscription.ReadAsync(serverHandles, fromCache, ct)` for subscription-bound group reads.

## Before

```csharp
var values = group.Read(items);
```

## After

```csharp
var values = await subscription.ReadAsync(serverHandles, fromCache: true, ct);
```

The code fix updates the containing method to `async` and adds a cancellation token parameter when needed. After applying it, choose `IDaServer.ReadAsync` or `IDaSubscription.ReadAsync`, thread the token from the caller, and keep any client-name setup on `IDaServer.SetClientNameAsync`.
