# OCMAE001 — OPC AE event subscriptions

`OCMAE001` reports `IOPCEventSubscription` variable or parameter usage, which usually indicates callback-based AE subscription plumbing from the OPC Foundation API or `OpcRcw.Ae`. Current `Opc.Classic.Ae` code creates an `IAeSubscription` from `IAeServer` and consumes its `Events` async stream.

## Before

```csharp
OpcRcw.Ae.IOPCEventSubscription subscription = server.CreateSubscription(callback);
```

## After

```csharp
await using IAeSubscription subscription = await server.CreateSubscriptionAsync(
    active: true,
    bufferTimeMs: 0,
    maxBufferSize: 0,
    ct);

await foreach (EventNotification notification in subscription.Events.WithCancellation(ct))
{
    await handler.HandleAsync(notification, ct);
}
```

Replace `handler` with your domain event processor and keep `IAeSubscription` lifetime tied to the consuming scope or hosted service cancellation token.
