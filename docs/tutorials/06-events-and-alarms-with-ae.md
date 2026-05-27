# Events and alarms with OPC AE

Applies to Opc.Classic 1.0.0-rc.7.

OPC Alarms & Events is the event stream for OPC Classic. DA tells you current values, HDA tells you historical values, and AE tells you what happened: a simple notification, an operator tracking event, or a condition event that may require acknowledgement. This tutorial walks through event categories, area browsing, filters, condition acknowledgements, refresh, and server-hosting patterns using `Opc.Classic.Ae`.

The repository samples are the best reference: `samples\Opc.Classic.Samples.AeServer\` hosts `Opc.Classic.Samples.AeServer.1`, while `samples\Opc.Classic.Samples.AeClient\` builds an in-process loopback client over `IOPCEventServerClientProxy`, `InMemoryCallChannel`, `InProcessAeServer`, and `InProcessAeSubscription`. When `OPC_CLASSIC_SERVER_HOST` and `OPC_CLASSIC_SERVER_PORT` are set, the AE client uses `DcomCallChannelFactory.ConnectTcpAsync` against the sample server instead. The AE server reads `OPC_CLASSIC_SAMPLE_PORT` (default `51301`) or `OPC_CLASSIC_LISTEN_ADDRESS`. The public application surface is `IAeServer` and `IAeSubscription`.

## Prerequisites

- .NET 10 SDK.
- Opc.Classic packages or project references for `Opc.Classic.Core` and `Opc.Classic.Ae`.
- Basic understanding of alarms, sources, areas, and acknowledgements.
- Optional: an AE server or the repository AE loopback sample.

## What you'll learn

- How AE event types and categories differ.
- How to browse areas and sources.
- How to create a subscription and filter events.
- How to handle simple, tracking, and condition events.
- How to acknowledge active conditions.
- How refresh and cancel-refresh fit into reconnect handling.

## AE event model

`EventType` is a flags enum:

```csharp
[Flags]
public enum EventType
{
    None = 0,
    Simple = 0x0001,
    Tracking = 0x0002,
    Condition = 0x0004,
    All = Simple | Tracking | Condition,
}
```

A simple event is an informational notification. A tracking event records an action, often with an actor. A condition event represents alarm state and can require acknowledgement. Event categories are server-defined `uint` values. You query categories for a set of event types, then query condition names for condition categories.

`EventNotification` carries the common payload: `Source`, `Time`, `Message`, `Severity`, `EventCategory`, `EventType`, optional `ConditionName`, optional `SubConditionName`, `ConditionState`, `AckRequired`, `ActiveTime`, `Cookie`, optional `Actor`, `Quality`, and additional attributes.

## Browse the area tree

AE namespaces are area/source trees. Areas can contain areas or sources; sources emit events.

```csharp
using Opc.Classic.Ae;

public static async Task BrowseAreasAsync(
    IAeServer server,
    string areaQualifiedName,
    int depth,
    CancellationToken cancellationToken)
{
    await foreach (AreaBrowseElement element in server.BrowseAreasAsync(areaQualifiedName, cancellationToken)
        .ConfigureAwait(false))
    {
        string indent = new(' ', depth * 2);
        string kind = element.IsArea ? "Area" : "Source";
        Console.WriteLine($"{indent}{kind}: {element.Name} ({element.QualifiedName})");

        if (element.IsArea)
        {
            await BrowseAreasAsync(server, element.QualifiedName, depth + 1, cancellationToken).ConfigureAwait(false);
        }
    }
}
```

The AE sample returns root areas `Server` and `Demo`, with sources such as `Server.Heartbeat`, `Server.Errors`, and `Demo.Conditions`. Production servers often mirror plant topology: site, area, unit, equipment, source.

## Query categories and conditions

Categories tell you which server-defined event classes exist. Conditions are defined inside condition categories.

```csharp
IReadOnlyList<uint> categories = await server.QueryEventCategoriesAsync(
    EventType.Simple | EventType.Condition,
    cancellationToken).ConfigureAwait(false);

foreach (uint category in categories)
{
    IReadOnlyList<string> conditions = await server.QueryConditionNamesAsync(category, cancellationToken).ConfigureAwait(false);
    Console.WriteLine($"Category {category}: {string.Join(", ", conditions)}");
}
```

Do not hard-code category numbers from one server and reuse them against another vendor. Category IDs are server-defined. Store category metadata per server instance.

## Subscribe and filter

`IAeServer.CreateSubscriptionAsync` creates a live event subscription. `SubscriptionFilter` lets you filter by event type, severity, categories, areas, and sources.

```csharp
await using IAeSubscription subscription = await server.CreateSubscriptionAsync(
    active: true,
    bufferTimeMs: 100,
    maxBufferSize: 100,
    cancellationToken).ConfigureAwait(false);

await subscription.SetFilterAsync(
    new SubscriptionFilter
    {
        EventTypes = EventType.All,
        MinSeverity = 250,
        MaxSeverity = 1000,
        Areas = ["Demo"],
    },
    cancellationToken).ConfigureAwait(false);

await foreach (EventNotification notification in subscription.Events
    .WithCancellation(cancellationToken)
    .ConfigureAwait(false))
{
    Console.WriteLine($"{notification.Time:O} {notification.Source} {notification.Severity}: {notification.Message}");
}
```

The server applies the filter before delivery. Use server-side filters to reduce network and processing load. If you filter only in your application after receiving everything, older AE servers can fall behind during alarm floods.

## Capture and acknowledge conditions

Condition events can require acknowledgement. The managed API represents the acknowledgement key as `ConditionRef`.

```csharp
ConditionRef? ackTarget = null;

await foreach (EventNotification notification in subscription.Events
    .WithCancellation(cancellationToken)
    .ConfigureAwait(false))
{
    if (notification.EventType == EventType.Condition)
    {
        Console.WriteLine($"Condition {notification.ConditionName}: state={notification.NewState}, ackRequired={notification.AckRequired}");
    }

    if (notification.EventType == EventType.Condition &&
        notification.AckRequired &&
        notification.ConditionName is not null)
    {
        ackTarget = new ConditionRef(notification.Source, notification.ConditionName);
        break;
    }
}

if (ackTarget is ConditionRef condition)
{
    IReadOnlyList<AckResult> results = await server.AcknowledgeAsync(
        actor: "operator-console",
        comment: "Acknowledged after verification",
        conditions: [condition],
        cancellationToken).ConfigureAwait(false);

    foreach (AckResult result in results)
    {
        Console.WriteLine($"Ack {result.Condition}: {result.ResultId}");
    }
}
```

Acknowledge is batch-oriented. One condition can fail while others succeed. Always inspect each `AckResult.ResultId`.

## Refresh after reconnect

When an AE client reconnects, it may have missed condition state transitions. `IAeSubscription.RefreshAsync` asks the server to re-emit current active conditions. `CancelRefreshAsync` cancels an in-progress refresh.

```csharp
await subscription.RefreshAsync(cancellationToken).ConfigureAwait(false);

try
{
    await foreach (EventNotification notification in subscription.Events.WithCancellation(cancellationToken))
    {
        Console.WriteLine($"Refresh event: {notification.Source} {notification.Message}");
    }
}
finally
{
    await subscription.CancelRefreshAsync(CancellationToken.None).ConfigureAwait(false);
}
```

Use refresh after reconnect, after enabling a new area, or after changing filters. Do not run refresh continuously; it is a resynchronization tool, not a polling mechanism.

## Server-side hosting

`IOpcAeServer` exposes status and available filters. The sample server keeps the implementation small:

```csharp
using Opc.Classic;
using Opc.Classic.Ae.Hosting;

public sealed class SampleAeServer : IOpcAeServer
{
    private static readonly DateTimeOffset StartupTime = DateTimeOffset.UtcNow;

    public Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return Task.FromResult(new OpcServerStatus
        {
            Spec = OpcStatusSpec.Ae,
            StartTime = StartupTime,
            CurrentTime = now,
            LastUpdateTime = now,
            State = OpcServerState.Running,
            GroupCount = 0,
            BandWidth = 0,
            ServerVersion = new Version(1, 0, 0),
            VendorInfo = "Sample AE Server",
        });
    }

    public Task<int> QueryAvailableFiltersAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(0x1F);
    }
}
```

Hosting registration mirrors DA and HDA:

```csharp
int port = int.TryParse(
    Environment.GetEnvironmentVariable("OPC_CLASSIC_SAMPLE_PORT"),
    out int parsedPort) && parsedPort > 0 ? parsedPort : 51301;
string listenAddress = Environment.GetEnvironmentVariable("OPC_CLASSIC_LISTEN_ADDRESS")
    ?? $"0.0.0.0:{port}";

builder.Services.AddClassicServer();
builder.Services.AddClassicClsidRegistry(builder.Configuration);
builder.Services.AddOpcAeServer<SampleAeServer>(options =>
{
    options.Clsid = Guid.Parse("C4BF6E70-3BA2-4F9C-AE3D-8F6C1D9F2B4F");
    options.ProgId = "Opc.Classic.Samples.AeServer.1";
    options.FriendlyName = "Opc.Classic Sample AE Server";
    options.ListenAddress = listenAddress;
});
```

For full application behavior, the client sample's `InProcessAeServer` implements `IAeServer` and demonstrates category queries, condition names, area browsing, acknowledgements, enabling/disabling conditions, and subscription creation. That class is a useful model for domain logic even though it is an in-process sample.

## Filter design

Start broad during commissioning and narrow before production. A useful rollout plan:

1. Subscribe to `EventType.All`, severity `0..1000`, no category or area filter.
2. Record actual categories, sources, and message rates for a week.
3. Add area filters that match the application's responsibility.
4. Add severity thresholds only after operations agrees on cutoffs.
5. Keep condition events even when simple events are too noisy.

Do not filter out acknowledgable conditions just because severity is below a dashboard threshold. A low-severity condition can still require operator action.

## Pitfalls

- AE category IDs are server-defined. Never assume `3` means condition on every server.
- `Severity` is `0..1000`, but vendors interpret ranges differently.
- `Actor` is meaningful for tracking events, not simple events.
- `ConditionName` can be null for simple and tracking events.
- Acknowledgement should be idempotent from the UI perspective; a repeated ack may return a success or a state-related warning.
- Refresh can produce a burst of events. Size buffers for reconnect storms.

## Alarm lifecycle walkthrough

A condition event is not a single message; it is a lifecycle. A typical alarm starts enabled and inactive. When the process crosses a limit, the server emits a condition event with `ConditionState.Active | ConditionState.Enabled`, a condition name, active subcondition, active time, severity, and often `AckRequired=true`. If an operator acknowledges it while the process is still abnormal, the state becomes active and acknowledged. When the process returns to normal, the active bit clears. Depending on vendor policy, acknowledgement may be required before or after return-to-normal.

Your client should not infer lifecycle solely from message text. Use `EventType`, `ConditionName`, `SubConditionName`, `NewState`, `AckRequired`, `ActiveTime`, and `Cookie`. Store enough state to show "active unacknowledged", "active acknowledged", "returned unacknowledged", and "cleared". Operators care about those distinctions.

## Buffering and burst behavior

AE servers often emit bursts during plant upsets or reconnect refreshes. `bufferTimeMs` lets the server coalesce events before delivery; `maxBufferSize` bounds each delivery. Small buffers reduce latency but increase callback rate. Large buffers reduce overhead but can delay alarms. Choose defaults with operations staff, not only developers.

If your client writes events to a message broker, decouple broker latency from AE callback processing. Read from `subscription.Events`, enqueue to an internal bounded channel, and let a separate worker publish to the broker. If the channel fills, choose an explicit policy: drop low-severity simple events, backpressure the subscription, or fail readiness. Silent unbounded queues are a common outage cause.

## Acknowledgement audit

Acknowledgement is an operator action. Log actor, comment, condition reference, result, and time. If the application authenticates users separately from OPC, use the application user as `actor` and include the workstation or service instance in structured logs. A generic actor such as `service` makes audit trails weak.

Batch acknowledgement should produce per-condition audit rows. One failed condition in a batch should not hide successful acknowledgements for other conditions. Conversely, a UI should make partial failure obvious so the operator can retry only the failed rows.

## Reconnect strategy

On disconnect, assume event state may be stale. Reconnect, get server status, recreate the subscription, reapply the filter, enable required areas, and call `RefreshAsync`. During refresh, mark events as resynchronization data if your UI distinguishes live from refreshed events. After refresh completes or is cancelled, resume normal live processing.

Do not acknowledge conditions based only on stale pre-disconnect state. Query or refresh first. A condition may have cleared, changed subcondition, or been acknowledged by another client while your connection was down.

## Event storage and replay

Many applications forward AE events into a message bus or database. Store the original event fields, not just formatted text. At minimum, keep source, time, message, severity, event category, event type, condition name, subcondition, new state, ack-required flag, active time, cookie, actor, quality, and attributes. A formatted message is useful for humans but insufficient for alarm analytics.

If you replay stored events into another system, mark them as replayed. Do not feed historical alarm events back into a live operator console as if they just occurred. For analytics, preserve the server timestamp and ingestion timestamp separately; the difference is event lag.

## Multi-client behavior

AE servers are often shared by several clients. Another client may acknowledge a condition before yours does. Your acknowledge call should handle already-acknowledged or state-changed results gracefully. Similarly, enabling or disabling conditions by area can affect server-side monitoring state depending on vendor semantics. Test with multiple clients during commissioning if the production server is shared.

## Severity taxonomy

Create a local severity taxonomy that maps server-specific ranges to operational meaning. For example, 900-1000 may be emergency, 700-899 high, 400-699 medium, and below 400 informational. Do not hard-code that mapping into the OPC adapter; keep it in application configuration so different servers can use different policies. Preserve the raw severity for audit even when you add normalized severity bands.

## Refresh storm controls

After a long outage, many active conditions may be refreshed at once. Rate-limit downstream notifications so operators are not flooded with duplicate pages or emails. A refreshed active alarm is important, but it may not require the same notification path as a new transition. Use event state and your reconnect marker to choose the right operator experience.

## Operator experience design

AE integration is successful only if operators can act on it. Design screens around alarm state, not raw protocol fields. Show source, message, severity, active/acknowledged/enabled state, active time, and acknowledgement status. Provide filters for area and severity, but make it difficult to hide unacknowledged active conditions accidentally. If an operator acknowledges an alarm, show the actor and comment in the event history.

For notification systems, distinguish new active alarms from refresh events, tracking events, and return-to-normal transitions. Paging an operator for every refresh after reconnect causes alarm fatigue. Suppressing a new unacknowledged condition is worse. The protocol gives you enough state to make that distinction; preserve it through your application pipeline.

## Maintenance review questions

At each release review, ask the same maintenance questions. Did any public configuration keys change? Did the expected server identity, ProgID, CLSID, SPN, or item namespace change? Did timeout, retry, or batch-size defaults change? Did the release add a dependency that affects deployment, security, or diagnostics? Did the runbook and screenshots still match the product? These questions are simple, but they catch many integration regressions before a plant outage does.

Also schedule periodic drills. Run the tutorial scenario in a staging environment, rotate credentials, restart the server, force a reconnect, and confirm logs explain what happened. Tutorials are most valuable when they stay executable.

## Final acceptance test

Before release, replay the complete AE scenario: browse areas, query categories, subscribe with a condition filter, receive a simple event, receive a condition event, acknowledge it, refresh after reconnect, and disable the area. Save the log output as release evidence. That replay proves the tutorial's sequence and gives operators a concrete example of normal behavior.

## Next steps

- Run `samples\Opc.Classic.Samples.AeServer` and `samples\Opc.Classic.Samples.AeClient`; for container ports and `OPC_CLASSIC_SERVER_HOST` / `OPC_CLASSIC_SERVER_PORT`, see [../../samples/README.docker.md](../../samples/README.docker.md).
- Deploy AE workloads with [03-cross-platform-deployment.md](03-cross-platform-deployment.md).
- Harden authentication with [04-security-with-kerberos-and-channel-binding.md](04-security-with-kerberos-and-channel-binding.md).
- Diagnose event stream failures with [09-troubleshooting-and-diagnostics.md](09-troubleshooting-and-diagnostics.md).

## References

- OPC AE 1.10: `IOPCEventServer`, `IOPCEventSubscriptionMgt`, `IOPCEventSink`, condition refresh, and acknowledgements.
- [MS-DCOM] for callback object references and activation.
- Repository: `src\Opc.Classic.Ae\`, `samples\Opc.Classic.Samples.AeClient\`, and `samples\Opc.Classic.Samples.AeServer\`.






Additional practice: rehearse operator acknowledgement workflows quarterly.

