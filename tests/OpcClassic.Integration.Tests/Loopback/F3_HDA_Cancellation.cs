//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using TUnit.Core;

namespace OpcClassic.Integration.Tests.Loopback;

public sealed class F3_HDA_Cancellation
{
    [Test, Skip("Phase 13-followup: HDA loopback cancellation needs Phase 8F hosting plus a committed cancellation contract.")]
    public void Hda_history_read_cancellation_is_observed_by_the_loopback_host()
    {
        // TODO: Phase 13-followup — route an HDA history read through the generated proxy and hosted HDA server,
        // cancel it in-flight, and verify both client-side and host-side cancellation semantics.
    }
}
