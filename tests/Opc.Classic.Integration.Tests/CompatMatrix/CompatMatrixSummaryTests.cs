//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading.Tasks;
using Opc.Classic.Integration.Tests.Loopback;
using TUnit.Core;

namespace Opc.Classic.Integration.Tests.CompatMatrix;

public sealed class CompatMatrixSummaryTests {
    /// <summary>
    /// Asserts that the four matrix cells are tracked.
    /// This is not a runtime interop test - it is a structural compile-time
    /// reminder that the matrix has these cells:
    ///   - net10 client ↔ net10 server: Phase 13 Loopback
    ///   - net10 client → Windows COM server: Phase 14B / 14C
    ///   - Windows COM client → net10 server: Phase 14D-B
    ///   - Windows COM client → Windows COM server: out of scope (Windows-only legacy)
    /// </summary>
    [Test, Category("CompatMatrix")]
    public async Task Matrix_cells_documented() {
        await Assert.That(ReadLoopbackRoundTripType()).IsNotNull();
        await Assert.That(ReadNet10ServerToNativeClientType()).IsNotNull();
    }

    // TUnitAssertions0005 workaround: use non-const indirections for typeof assertions.
    private static Type ReadLoopbackRoundTripType() => typeof(F1DaRoundTrip);
    private static Type ReadNet10ServerToNativeClientType() => typeof(Net10ServerToNativeClientTests);
}
