//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic;
using Opc.Classic.Da;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Da.Tests;

public sealed class ItemIdentifierTests {
    [Test]
    public async Task Construct_WithItemName_PathDefaultsToNull() {
        var id = new ItemIdentifier("PLC1.MotorSpeed");
        await Assert.That(id.ItemName).IsEqualTo("PLC1.MotorSpeed");
        await Assert.That(id.Path).IsNull();
    }

    [Test]
    public async Task RecordEquality_IsByItemNameAndPath() {
        var a = new ItemIdentifier("X", "Path");
        var b = new ItemIdentifier("X", "Path");
        var c = new ItemIdentifier("X", "Other");
        await Assert.That(a).IsEqualTo(b);
        await Assert.That(a == b).IsTrue();
        await Assert.That(a == c).IsFalse();
        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }

    [Test]
    public async Task ToString_NoPath_JustItemName() {
        await Assert.That(new ItemIdentifier("X").ToString()).IsEqualTo("X");
    }

    [Test]
    public async Task ToString_WithPath_ConcatenatesWithDoubleColon() {
        await Assert.That(new ItemIdentifier("X", "P").ToString()).IsEqualTo("P::X");
    }
}

public sealed class ItemTests {
    [Test]
    public async Task Construct_FromName_AllOptionsDefault() {
        var item = new Item("X");
        await Assert.That(item.ItemName).IsEqualTo("X");
        await Assert.That(item.ClientHandle).IsEqualTo(0);
        await Assert.That(item.RequestedDataType).IsNull();
        await Assert.That(item.DeadbandPercent).IsNull();
        await Assert.That(item.SamplingRateMs).IsNull();
    }

    [Test]
    public async Task InitializerSyntax_AssignsAllFields() {
        var item = new Item("X") {
            ClientHandle = 42,
            RequestedDataType = typeof(double),
            DeadbandPercent = 1.5f,
            SamplingRateMs = 500,
        };
        await Assert.That(item.ClientHandle).IsEqualTo(42);
        await Assert.That(item.RequestedDataType).IsEqualTo(typeof(double));
        await Assert.That(item.DeadbandPercent).IsEqualTo(1.5f);
        await Assert.That(item.SamplingRateMs).IsEqualTo(500);
    }

    [Test]
    public async Task CopyConstructor_FromIdentifier_Throws_OnNull() {
        await Assert.That(() => { _ = new Item(null!); })
            .Throws<ArgumentNullException>();
    }
}

public sealed class ItemValueTests {
    [Test]
    public async Task QualityDefaultsToBad() {
        var v = new ItemValue("X");
        await Assert.That(v.Quality.Quality).IsEqualTo(OpcQualityKind.Bad);
    }

    [Test]
    public async Task InitializerSyntax_AssignsAllFields() {
        var ts = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);
        var v = new ItemValue("X") {
            ClientHandle = 7,
            Value = 42.0,
            Quality = OpcQuality.Good,
            Timestamp = ts,
        };
        await Assert.That(v.ClientHandle).IsEqualTo(7);
        await Assert.That(v.Value).IsEqualTo(42.0);
        await Assert.That(v.Quality.Quality).IsEqualTo(OpcQualityKind.Good);
        await Assert.That(v.Timestamp).IsEqualTo(ts);
    }
}

public sealed class ItemValueResultTests {
    [Test]
    public async Task DefaultResultId_IsOk() {
        var r = new ItemValueResult("X");
        await Assert.That(r.ResultId).IsEqualTo(OpcResultId.Ok);
    }

    [Test]
    public async Task CopyFromItemValue_PreservesFields() {
        var ts = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);
        var src = new ItemValue("X", "P") {
            ClientHandle = 3,
            Value = "hello",
            Quality = OpcQuality.Uncertain,
            Timestamp = ts,
        };
        var r = new ItemValueResult(src) { ResultId = OpcResultId.UnknownItemId };

        await Assert.That(r.ItemName).IsEqualTo("X");
        await Assert.That(r.Path).IsEqualTo("P");
        await Assert.That(r.ClientHandle).IsEqualTo(3);
        await Assert.That(r.Value).IsEqualTo("hello");
        await Assert.That(r.Quality.Quality).IsEqualTo(OpcQualityKind.Uncertain);
        await Assert.That(r.Timestamp).IsEqualTo(ts);
        await Assert.That(r.ResultId).IsEqualTo(OpcResultId.UnknownItemId);
    }
}

public sealed class IdentifiedResultTests {
    [Test]
    public async Task DefaultResultId_IsOk() {
        var r = new IdentifiedResult("X");
        await Assert.That(r.ResultId).IsEqualTo(OpcResultId.Ok);
    }

    [Test]
    public async Task InitializerSyntax_AssignsAllFields() {
        var r = new IdentifiedResult("X") {
            ClientHandle = 9,
            ResultId = OpcResultId.BadRights,
            DiagnosticInfo = "no write access",
        };
        await Assert.That(r.ClientHandle).IsEqualTo(9);
        await Assert.That(r.ResultId).IsEqualTo(OpcResultId.BadRights);
        await Assert.That(r.DiagnosticInfo).IsEqualTo("no write access");
    }
}

public sealed class SubscriptionStateTests {
    [Test]
    public async Task Default_ActiveTrue_RatesZero() {
        var s = new SubscriptionState();
        await Assert.That(s.Active).IsTrue();
        await Assert.That(s.UpdateRateMs).IsEqualTo(0);
        await Assert.That(s.DeadbandPercent).IsEqualTo(0f);
        await Assert.That(s.KeepAliveMs).IsEqualTo(0);
    }

    [Test]
    public async Task At_OneSecond_SetsUpdateRate1000Ms() {
        var s = SubscriptionState.At(TimeSpan.FromSeconds(1));
        await Assert.That(s.UpdateRateMs).IsEqualTo(1000);
        await Assert.That(s.Active).IsTrue();
    }

    [Test]
    public async Task At_Inactive_SetsActiveFalse() {
        var s = SubscriptionState.At(TimeSpan.FromMilliseconds(500), active: false);
        await Assert.That(s.UpdateRateMs).IsEqualTo(500);
        await Assert.That(s.Active).IsFalse();
    }

    [Test]
    public async Task At_ZeroOrNegativeRate_Throws() {
        await Assert.That(() => { _ = SubscriptionState.At(TimeSpan.Zero); })
            .Throws<ArgumentOutOfRangeException>();
        await Assert.That(() => { _ = SubscriptionState.At(TimeSpan.FromSeconds(-1)); })
            .Throws<ArgumentOutOfRangeException>();
    }
}
