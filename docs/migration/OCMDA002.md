# OCMDA002 — OPC DA Browse

`OCMDA002` reports synchronous `server.Browse(itemId, filters)` calls on OPC DA objects. Move browse operations to the async `IOpcDaBrowse` shape so callers can propagate `CancellationToken` and avoid blocking threads while remote DCOM/MSRPC work is in flight.

## Before

```csharp
var branches = server.Browse(itemId, filters);
```

## After

```csharp
var branches = await browser.BrowseAsync(itemId, filters, ct);
```

Use the generated code fix as a starting point, then rename the receiver to your injected `IOpcDaBrowse` instance if the source `server` variable is being removed.
