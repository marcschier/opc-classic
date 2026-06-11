//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Smoke test for the Opc.Classic.Generators source-generator pipeline.
// If this test fails to COMPILE, the generator wiring is broken (the
// ProjectReference OutputItemType="Analyzer" trick is the most likely
// culprit). If it compiles and asserts pass, the toolchain is alive.
//

using Opc.Classic.Generators;
using TUnit.Core;

namespace Opc.Classic.Tests;

public sealed class GeneratorSmokeTests
{
    // Indirect through a helper method so the TUnit constant-assertion analyzer
    // doesn't reject the test — the consts are inherently fixed but
    // demonstrating the generator pipeline is alive needs an assertion against
    // them.
    private static string ReadVersion() => ClassicGeneratorMarker.Version;
    private static string ReadDescription() => ClassicGeneratorMarker.Description;

    [Test]
    public async Task GeneratedMarker_HasExpectedVersion()
    {
        await Assert.That(ReadVersion()).IsEqualTo("0.2.0-dev");
    }

    [Test]
    public async Task GeneratedMarker_HasDescription()
    {
        await Assert.That(ReadDescription()).Contains("OpcInterface generators");
    }
}
