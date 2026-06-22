// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Integration.Tests.Loopback;

public sealed class F2AeEventDelivery
{
    [Test, Skip("Future: AE event loopback needs AE server-side hosting before event sinks can be wired end-to-end.")]
    public void Ae_event_delivery_round_trips_through_managed_loopback_subscription()
    {
        // TODO: create an AE server host, advise an event callback sink through InMemoryCallChannel,
        // raise a condition/event, and assert the generated proxy/server-host pipeline delivers the notification.
    }
}
