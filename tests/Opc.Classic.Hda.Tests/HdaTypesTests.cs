//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic;
using Opc.Classic.Hda;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Hda.Tests;

public sealed class HdaTimeTests
{
    [Test]
    public async Task Absolute_RoundTripsUnchanged()
    {
        var instant = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);
        var t = HdaTime.Absolute(instant);
        await Assert.That(t.IsRelative).IsFalse();
        await Assert.That(t.ResolveAt(DateTimeOffset.UtcNow)).IsEqualTo(instant);
    }

    [Test]
    public async Task Absolute_NormalizesNonUtcToUtc()
    {
        var local = new DateTimeOffset(2026, 5, 21, 14, 0, 0, TimeSpan.FromHours(2));
        var utc = local.ToUniversalTime();
        var t = HdaTime.Absolute(local);
        await Assert.That(t.ResolveAt(DateTimeOffset.UtcNow)).IsEqualTo(utc);
    }

    [Test]
    [Arguments("NOW", 0)]
    [Arguments("NOW-1S", -1)]
    [Arguments("NOW+60S", +60)]
    public async Task Relative_Seconds_OffsetCorrectly(string expression, int expectedSecondsOffset)
    {
        var anchor = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);
        var t = HdaTime.Relative(expression);
        var resolved = t.ResolveAt(anchor);
        await Assert.That(resolved).IsEqualTo(anchor.AddSeconds(expectedSecondsOffset));
    }

    [Test]
    [Arguments("NOW-1H", -3600)]
    [Arguments("NOW-1D", -86400)]
    [Arguments("NOW+1W", 86400 * 7)]
    public async Task Relative_HoursDaysWeeks_OffsetCorrectly(string expression, int expectedSeconds)
    {
        var anchor = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);
        var resolved = HdaTime.Relative(expression).ResolveAt(anchor);
        await Assert.That(resolved).IsEqualTo(anchor.AddSeconds(expectedSeconds));
    }

    [Test]
    public async Task Relative_ComposedExpression_ChainsArithmetic()
    {
        // NOW - 1 day + 12 hours = -12h from anchor
        var anchor = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);
        var resolved = HdaTime.Relative("NOW-1D+12H").ResolveAt(anchor);
        await Assert.That(resolved).IsEqualTo(anchor.AddHours(-12));
    }

    [Test]
    public async Task Relative_Months_AddsCalendarMonths()
    {
        var anchor = new DateTimeOffset(2026, 1, 31, 0, 0, 0, TimeSpan.Zero);
        var resolved = HdaTime.Relative("NOW+1MO").ResolveAt(anchor);
        // .AddMonths clamps Feb 31 -> Feb 28 (2026 not leap)
        await Assert.That(resolved).IsEqualTo(new DateTimeOffset(2026, 2, 28, 0, 0, 0, TimeSpan.Zero));
    }

    [Test]
    public async Task Relative_BadGrammar_Throws()
    {
        await Assert.That(() => { _ = HdaTime.Relative("NOPE"); })
            .Throws<FormatException>();
        await Assert.That(() => { _ = HdaTime.Relative("NOW-XYZ"); })
            .Throws<FormatException>();
        await Assert.That(() => { _ = HdaTime.Relative("NOW1H"); })
            .Throws<FormatException>();
    }

    [Test]
    public async Task Relative_NullOrEmpty_Throws()
    {
        await Assert.That(() => { _ = HdaTime.Relative(null!); })
            .Throws<ArgumentNullException>();
        await Assert.That(() => { _ = HdaTime.Relative("  "); })
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task Equality_AbsoluteVsRelative()
    {
        var a = HdaTime.Absolute(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var b = HdaTime.Absolute(new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));
        var c = HdaTime.Relative("NOW");
        await Assert.That(a).IsEqualTo(b);
        await Assert.That(a == b).IsTrue();
        await Assert.That(a == c).IsFalse();
    }

    [Test]
    public async Task ToString_AbsoluteUsesIso8601()
    {
        var instant = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);
        var s = HdaTime.Absolute(instant).ToString();
        await Assert.That(s).IsEqualTo("2026-05-21T12:00:00.0000000+00:00");
    }

    [Test]
    public async Task ToString_RelativeReturnsExpression()
    {
        await Assert.That(HdaTime.Relative("NOW-1H").ToString()).IsEqualTo("NOW-1H");
    }
}

public sealed class HdaItemValueTests
{
    [Test]
    public async Task DefaultQuality_IsBad()
    {
        var v = new HdaItemValue();
        await Assert.That(v.Quality.Quality).IsEqualTo(OpcQualityKind.Bad);
    }

    [Test]
    public async Task Initializer_AssignsAll()
    {
        var ts = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);
        var v = new HdaItemValue
        {
            Timestamp = ts,
            Value = 42.0,
            Quality = OpcQuality.Good,
        };
        await Assert.That(v.Timestamp).IsEqualTo(ts);
        await Assert.That(v.Value).IsEqualTo(42.0);
        await Assert.That(v.Quality.Quality).IsEqualTo(OpcQualityKind.Good);
    }
}

public sealed class HdaAnnotationTests
{
    [Test]
    public async Task DefaultStrings_AreEmpty()
    {
        var a = new HdaAnnotation();
        await Assert.That(a.AnnotationText).IsEqualTo(string.Empty);
        await Assert.That(a.User).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Initializer_AssignsAll()
    {
        var ts = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);
        var at = new DateTimeOffset(2026, 5, 21, 12, 30, 0, TimeSpan.Zero);
        var a = new HdaAnnotation
        {
            Timestamp = ts,
            AnnotationTime = at,
            AnnotationText = "Calibrated sensor",
            User = "alice@CORP",
        };
        await Assert.That(a.Timestamp).IsEqualTo(ts);
        await Assert.That(a.AnnotationTime).IsEqualTo(at);
        await Assert.That(a.AnnotationText).IsEqualTo("Calibrated sensor");
        await Assert.That(a.User).IsEqualTo("alice@CORP");
    }
}
