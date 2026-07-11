// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using TUnit.Core;
using TUnit.Core.Interfaces;

// Run this assembly's tests serially (one at a time).
//
// Opc.Classic.Hosting.Windows.Tests exercises native Windows COM-callable
// wrappers (CCWs) and COM registration. That surface is dominated by
// process-global mutable state that is fundamentally unsafe to touch
// concurrently, which produced Windows/macOS-only flakes that never surfaced on
// push-to-main (the build workflow runs ubuntu-only on push; the full matrix
// runs only on PRs):
//
//   * Static pointer-keyed registries (OpcDaServerCcw.s_ccws,
//     OpcDaGroupCcw.s_tearoffs, OpcHdaServerCcw sessions, the test CallbackCcw
//     s_instances) are keyed by raw Marshal.AllocCoTaskMem addresses. Freed
//     addresses are reused, so two tests allocating/freeing CCW pointers in
//     parallel can collide on the same key and read each other's state (seen as
//     a rare GetReferenceCount() != -1 flake).
//   * The asynchronous OPC callback tests (HDA playback / async-update) dispatch
//     completions via Task.Run; running many in parallel saturates the
//     ThreadPool and starves those completions, so the observed callback count
//     stays below the asserted value within the wait window.
//   * COM class-object registration writes HKCU\Software\Classes.
//
// Serial execution removes the shared-state and ThreadPool-starvation races at
// their root while keeping the tests fast (they are quick unit tests). A small
// residual remains for tests that Cancel an in-flight operation, because the
// Task.Run completion still races the test's synchronous Cancel call; those
// classes additionally carry [Retry] to absorb that inherent, timing-only miss.
[assembly: ParallelLimiter<Opc.Classic.Hosting.Windows.Tests.SingleThreadedParallelLimit>]

namespace Opc.Classic.Hosting.Windows.Tests;

/// <summary>
/// An <see cref="IParallelLimit"/> of one, applied assembly-wide via
/// <c>[assembly: ParallelLimiter&lt;SingleThreadedParallelLimit&gt;]</c> so the
/// Windows COM/CCW tests never run concurrently. See the assembly-level comment
/// in this file for why serial execution is required.
/// </summary>
internal sealed class SingleThreadedParallelLimit : IParallelLimit
{
    public int Limit => 1;
}
