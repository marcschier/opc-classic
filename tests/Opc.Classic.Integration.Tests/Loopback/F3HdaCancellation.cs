// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Integration.Tests.Loopback;

public sealed class F3HdaCancellation
{
    [Test, Skip("Future: HDA loopback cancellation needs HDA hosting plus a committed cancellation contract.")]
    public void Hda_history_read_cancellation_is_observed_by_the_loopback_host()
    {
        // TODO: route an HDA history read through the generated proxy and hosted HDA server,
        // cancel it in-flight, and verify both client-side and host-side cancellation semantics.
    }
}
