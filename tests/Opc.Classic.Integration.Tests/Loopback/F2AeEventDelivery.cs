//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using TUnit.Core;

namespace Opc.Classic.Integration.Tests.Loopback;

public sealed class F2AeEventDelivery
{
    [Test, Skip("Phase 13-followup: AE event loopback needs Phase 7F server-side hosting before event sinks can be wired end-to-end.")]
    public void Ae_event_delivery_round_trips_through_managed_loopback_subscription()
    {
        // TODO: Phase 13-followup — create an AE server host, advise an event callback sink through InMemoryCallChannel,
        // raise a condition/event, and assert the generated proxy/server-host pipeline delivers the notification.
    }
}
