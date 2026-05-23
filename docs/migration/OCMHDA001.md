# OCMHDA001 — Legacy OPC HDA SyncReadRaw

`OCMHDA001` reports synchronous HDA `SyncReadRaw(...)` calls. Migrate history reads to the async `IOpcHdaSyncReadAsync` equivalent so bulk history requests can run without blocking worker threads.

## Before

```csharp
var values = historian.SyncReadRaw(itemId, start, end);
```

## After

```csharp
var values = await historian.ReadRawAsync(itemId, start, end, ct);
```

After applying the code fix, review HDA time range, bounds, and continuation behavior because applications often wrapped legacy synchronous calls with custom retry loops.
