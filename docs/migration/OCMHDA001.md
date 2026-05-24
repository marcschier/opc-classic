# OCMHDA001 — OPC HDA SyncReadRaw

`OCMHDA001` reports synchronous HDA `SyncReadRaw(...)` calls. Move history reads to the async `IOpcHdaSyncReadAsync` equivalent so bulk history requests can run without blocking worker threads.

## Before

```csharp
var values = historian.SyncReadRaw(itemId, start, end);
```

## After

```csharp
var values = await historian.ReadRawAsync(itemId, start, end, ct);
```

After applying the code fix, review HDA time range, bounds, and continuation behavior because applications often wrap synchronous calls with custom retry loops.
