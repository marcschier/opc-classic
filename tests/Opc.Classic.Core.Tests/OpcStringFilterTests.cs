// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Tests;

public sealed class OpcStringFilterTests
{
    [Test]
    [Arguments("Pump01", "Pump01", false, true)]
    [Arguments("Pump01", "Pump*", false, true)]
    [Arguments("Pump01", "*01", false, true)]
    [Arguments("Pump01Status", "Pump*Status", false, true)]
    [Arguments("Area/Pump01/Status", "*/Pump*/Status", false, true)]
    [Arguments("Pump01", "P**u***mp*01****", false, true)]
    [Arguments("Pump01", "**************************************************Pump**************************************************01**************************************************", false, true)]
    [Arguments("Pump01", "Pump??", false, true)]
    [Arguments("Pump1", "Pump?", false, true)]
    [Arguments("Pump", "Pump?", false, false)]
    [Arguments("Pump01", "Pump#1", false, true)]
    [Arguments("PumpA1", "Pump#1", false, false)]
    [Arguments("PumpA", "Pump[ABC]", false, true)]
    [Arguments("PumpD", "Pump[ABC]", false, false)]
    [Arguments("PumpM", "Pump[A-Z]", false, true)]
    [Arguments("Pumpp", "Pump[A-Z]", false, true)]
    [Arguments("Pumpp", "Pump[A-Z]", true, false)]
    [Arguments("Pump7", "Pump[0-9]", false, true)]
    [Arguments("PumpX", "Pump[0-9]", false, false)]
    [Arguments("PumpD", "Pump[!ABC]", false, true)]
    [Arguments("PumpB", "Pump[!ABC]", false, false)]
    [Arguments("Pump7", "Pump[!A-Z]", false, true)]
    [Arguments("PumpM", "Pump[!A-Z]", false, false)]
    [Arguments("PumpM", "Pump[Z-A]", false, true)]
    [Arguments("Pump7", "Pump[Z-A]", false, false)]
    [Arguments("pump", "PUMP", false, true)]
    [Arguments("pump", "PUMP", true, false)]
    [Arguments("", "", false, true)]
    [Arguments("Pump", "", false, false)]
    [Arguments("", "*", false, true)]
    [Arguments("", "?", false, false)]
    [Arguments("Café", "Caf?", false, true)]
    [Arguments("München", "M[Ü]nchen", false, true)]
    [Arguments("München", "M[Ü]nchen", true, false)]
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
