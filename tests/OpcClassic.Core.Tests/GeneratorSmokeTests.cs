//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// Smoke test for the OpcClassic.Generators source-generator pipeline.
// If this test fails to COMPILE, the generator wiring is broken (the
// ProjectReference OutputItemType="Analyzer" trick is the most likely
// culprit). If it compiles and asserts pass, the toolchain is alive.
//

using OpcClassic.Generators;
using TUnit.Core;

namespace OpcClassic.Tests;

public sealed class GeneratorSmokeTests
{
    // Indirect through a helper method so the TUnit constant-assertion analyzer
    // doesn't reject the test — the consts are inherently fixed but
    // demonstrating the generator pipeline is alive needs an assertion against
    // them.
    private static string ReadVersion() => OpcClassicGeneratorMarker.Version;
    private static string ReadDescription() => OpcClassicGeneratorMarker.Description;

    [Test]
    public async Task GeneratedMarker_HasExpectedVersion()
    {
        await Assert.That(ReadVersion()).IsEqualTo("0.2.0-dev");
    }

    [Test]
    public async Task GeneratedMarker_HasDescription()
    {
        await Assert.That(ReadDescription()).Contains("Phase 4A.1");
    }
}
