//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Ae.Tests;

public sealed class EventTypeFlagsTests
{
    // Helper to break the analyzer's compile-time constness check on the
    // assertion expression — wire-format value tests are inherently
    // constant-vs-constant but they're still meaningful as documentation.
    private static int ValueOf(EventType e) => (int)e;

    [Test]
    public async Task FlagBits_MatchOpcAeOnTheWireValues()
    {
        // OPC AE 1.0 §4.1 — event type bit values transmitted on the wire.
        await Assert.That(ValueOf(EventType.Simple)).IsEqualTo(0x0001);
        await Assert.That(ValueOf(EventType.Tracking)).IsEqualTo(0x0002);
        await Assert.That(ValueOf(EventType.Condition)).IsEqualTo(0x0004);
    }

    [Test]
    public async Task SetMembership_IndividualFlags()
    {
        var any = EventType.Tracking | EventType.Simple;
        await Assert.That(any.HasFlag(EventType.Simple)).IsTrue();
        await Assert.That(any.HasFlag(EventType.Tracking)).IsTrue();
        await Assert.That(any.HasFlag(EventType.Condition)).IsFalse();
    }
}

public sealed class EventNotificationTests
{
    [Test]
    public async Task Initializer_AssignsRequiredAndOptionalFields()
    {
        var t = new DateTimeOffset(2026, 5, 21, 9, 0, 0, TimeSpan.Zero);
        var n = new EventNotification
        {
            Source = "Tank.Level",
            Time = t,
            Message = "Level too high",
            Severity = 750,
            EventType = EventType.Condition,
            ConditionName = "HighLimit",
            NewState = ConditionState.Active | ConditionState.Enabled,
            AckRequired = true,
            ActiveTime = t,
            Cookie = 0x1234,
            Quality = OpcQuality.Good,
        };
        await Assert.That(n.Source).IsEqualTo("Tank.Level");
        await Assert.That(n.Severity).IsEqualTo(750);
        await Assert.That(n.EventType).IsEqualTo(EventType.Condition);
        await Assert.That(n.NewState.HasFlag(ConditionState.Active)).IsTrue();
        await Assert.That(n.NewState.HasFlag(ConditionState.Acknowledged)).IsFalse();
        await Assert.That(n.AckRequired).IsTrue();
    }

    [Test]
    public async Task DefaultEventType_IsSimple()
    {
        var n = new EventNotification { Source = "X" };
        await Assert.That(n.EventType).IsEqualTo(EventType.Simple);
    }

    [Test]
    public async Task DefaultQuality_IsGood()
    {
        var n = new EventNotification { Source = "X" };
        await Assert.That(n.Quality.Quality).IsEqualTo(OpcQualityKind.Good);
    }
}

public sealed class SubscriptionFilterTests
{
    [Test]
    public async Task Default_IsEffectivelyNoFilter()
    {
        var f = new SubscriptionFilter();
        await Assert.That(f.EventTypes).IsEqualTo(EventType.All);
        await Assert.That(f.MinSeverity).IsEqualTo(0);
        await Assert.That(f.MaxSeverity).IsEqualTo(1000);
        await Assert.That(f.EventCategories.Count).IsEqualTo(0);
        await Assert.That(f.Areas.Count).IsEqualTo(0);
        await Assert.That(f.Sources.Count).IsEqualTo(0);
        await Assert.That(f.HasAnyCriterion).IsFalse();
    }

    [Test]
    public async Task HasAnyCriterion_TrueWhenSeverityRangeNarrowed()
    {
        var f = new SubscriptionFilter { MinSeverity = 100 };
        await Assert.That(f.HasAnyCriterion).IsTrue();
    }

    [Test]
    public async Task HasAnyCriterion_TrueWhenEventTypesRestricted()
    {
        var f = new SubscriptionFilter { EventTypes = EventType.Condition };
        await Assert.That(f.HasAnyCriterion).IsTrue();
    }
}
