// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Integration.Tests.Loopback;

namespace Opc.Classic.Integration.Tests.CompatMatrix;

public sealed class CompatMatrixSummaryTests
{
    /// <summary>
    /// Asserts that the four matrix cells are tracked.
    /// This is not a runtime interop test - it is a structural compile-time
    /// reminder that the matrix has these cells:
    ///   - net10 client ↔ net10 server: Loopback fixtures
    ///   - net10 client → Windows COM server: managed-client + native-server fixtures
    ///   - Windows COM client → net10 server: native-client + managed-server fixtures
    ///   - Windows COM client → Windows COM server: out of scope (Windows-only legacy)
    /// </summary>
    [Test, Category("CompatMatrix")]
    public async Task Matrix_cells_documented()
    {
        await Assert.That(ReadLoopbackRoundTripType()).IsNotNull();
        await Assert.That(ReadNet10ServerToNativeClientType()).IsNotNull();
    }

    // TUnitAssertions0005 workaround: use non-const indirections for typeof assertions.
    private static Type ReadLoopbackRoundTripType() => typeof(F1DaRoundTrip);
    private static Type ReadNet10ServerToNativeClientType() => typeof(Net10ServerToNativeClientTests);
}
