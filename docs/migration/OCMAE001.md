# OCMAE001 — OPC AE event subscriptions

`OCMAE001` reports `IOPCEventSubscription` variable or parameter usage, which usually indicates callback-based AE subscription plumbing from the OPC Foundation API or `OpcRcw.Ae`. `Opc.Classic.Ae` models event notifications as an async stream.

## Before

```csharp
OpcRcw.Ae.IOPCEventSubscription subscription = server.CreateSubscription(callback);
```

## After

```csharp
await foreach (OpcEventNotification notification in server.SubscribeAsync(ct))
{
    await handler.HandleAsync(notification, ct);
}
```

Replace `handler` with your domain event processor and keep subscription lifetime tied to the consuming scope or hosted service cancellation token.
