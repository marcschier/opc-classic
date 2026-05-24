//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic;
using TUnit.Core;

namespace Opc.Classic.Tests;

public sealed class OpcStringFilterTests
{
    [Test]
    [Arguments("Pump01", "Pump01", false, true)]
    [Arguments("Pump01", "Pump*", false, true)]
    [Arguments("Pump01", "*01", false, true)]
    [Arguments("Pump01", "Pump??", false, true)]
    [Arguments("Pump01", "Pump#1", false, true)]
    [Arguments("PumpA1", "Pump#1", false, false)]
    [Arguments("PumpA", "Pump[ABC]", false, true)]
    [Arguments("PumpD", "Pump[!ABC]", false, true)]
    [Arguments("PumpB", "Pump[!ABC]", false, false)]
    [Arguments("Pump7", "Pump[0-9]", false, true)]
    [Arguments("pump", "PUMP", false, true)]
    [Arguments("pump", "PUMP", true, false)]
    [Arguments("", "*", false, true)]
    [Arguments("", "?", false, false)]
    public async Task MatchPattern_EvaluatesOpcCommonWildcards(
        string value,
        string pattern,
        bool caseSensitive,
        bool expected)
    {
        bool actual = OpcStringFilter.MatchPattern(value, pattern, caseSensitive);

        await Assert.That(actual).IsEqualTo(expected);
    }
}
