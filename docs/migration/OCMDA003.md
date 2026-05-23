# OCMDA003 — Legacy OPC DA synchronous Read

`OCMDA003` reports synchronous `group.Read(items)` usage. The `Opc.Classic.Da` replacement is an async read abstraction such as `IOpcDaSyncIO.ReadAsync(items, ct)` so polling loops and UI applications do not block while waiting for DA values.

## Before

```csharp
var values = group.Read(items);
```

## After

```csharp
var values = await syncIo.ReadAsync(items, ct);
```

The code fix updates the containing method to `async` and adds a cancellation token parameter when needed. After applying it, thread the token from the caller or use your existing operation timeout policy.
