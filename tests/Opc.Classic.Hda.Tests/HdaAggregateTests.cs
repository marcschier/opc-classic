// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Hda.Tests;

public sealed class HdaAggregateTests
{
    [Test]
    [Arguments(nameof(HdaAggregate.None), 0)]
    [Arguments(nameof(HdaAggregate.Interpolative), 1)]
    [Arguments(nameof(HdaAggregate.Total), 2)]
    [Arguments(nameof(HdaAggregate.Average), 3)]
    [Arguments(nameof(HdaAggregate.TimeAverage), 4)]
    [Arguments(nameof(HdaAggregate.Count), 5)]
    [Arguments(nameof(HdaAggregate.StandardDeviation), 6)]
    [Arguments(nameof(HdaAggregate.MinimumActualTime), 7)]
    [Arguments(nameof(HdaAggregate.Minimum), 8)]
    [Arguments(nameof(HdaAggregate.MaximumActualTime), 9)]
    [Arguments(nameof(HdaAggregate.Maximum), 10)]
    [Arguments(nameof(HdaAggregate.Start), 11)]
    [Arguments(nameof(HdaAggregate.End), 12)]
    [Arguments(nameof(HdaAggregate.Delta), 13)]
    [Arguments(nameof(HdaAggregate.RegSlope), 14)]
    [Arguments(nameof(HdaAggregate.RegConst), 15)]
    [Arguments(nameof(HdaAggregate.RegDev), 16)]
    [Arguments(nameof(HdaAggregate.Variance), 17)]
    [Arguments(nameof(HdaAggregate.Range), 18)]
    [Arguments(nameof(HdaAggregate.DurationGood), 19)]
    [Arguments(nameof(HdaAggregate.DurationBad), 20)]
    [Arguments(nameof(HdaAggregate.PercentGood), 21)]
    [Arguments(nameof(HdaAggregate.PercentBad), 22)]
    [Arguments(nameof(HdaAggregate.WorstQuality), 23)]
    [Arguments(nameof(HdaAggregate.Annotations), 24)]
    public async Task StandardAggregate_IsDeclaredWithSpecValue(string memberName, int expectedValue)
    {
        var declared = Enum.TryParse<HdaAggregate>(memberName, out var aggregate);
        var actualValue = (int)aggregate;

        await Assert.That(declared).IsTrue();
        await Assert.That(actualValue).IsEqualTo(expectedValue);
    }

    [Test]
    public async Task OpchdaAnnotations_IsDeclaredWithSpecValue()
    {
        var names = Enum.GetNames<HdaAggregate>();
        var hasAnnotations = names.Contains(nameof(HdaAggregate.Annotations));
        var actualValue = (int)HdaAggregate.Annotations;

        await Assert.That(hasAnnotations).IsTrue();
        await Assert.That(actualValue).IsEqualTo(24);
    }
}
