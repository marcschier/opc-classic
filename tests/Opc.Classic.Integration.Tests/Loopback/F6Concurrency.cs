//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using TUnit.Core;

namespace Opc.Classic.Integration.Tests.Loopback;

public sealed class F6Concurrency
{
    [Test, Skip("Future: concurrency validation needs a stress harness and a hosted runtime.")]
    public void One_hundred_clients_poll_one_thousand_items_every_100ms_for_60s()
    {
        // TODO: run 100 clients against the hosted runtime, poll 1000 items every 100ms for 60s,
        // and assert no lost calls, no data corruption, bounded latency, and deterministic teardown.
    }
}
