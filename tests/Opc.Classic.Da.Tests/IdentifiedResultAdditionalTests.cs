//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Da.Tests;

public sealed class IdentifiedResultAdditionalTests
{
    [Test]
    public async Task Constructor_WithItemNameAndPath_PopulatesIdentifierAndDefaults()
    {
        var result = new IdentifiedResult("Area1.Motor.Speed", "PLC1");

        await Assert.That(result.ItemName).IsEqualTo("Area1.Motor.Speed");
        await Assert.That(result.Path).IsEqualTo("PLC1");
        await Assert.That(result.ToString()).IsEqualTo("PLC1::Area1.Motor.Speed");
        await Assert.That(result.ClientHandle).IsEqualTo(0);
        await Assert.That(result.ResultId).IsEqualTo(OpcResultId.Ok);
        await Assert.That(result.DiagnosticInfo).IsNull();
    }

    [Test]
    public async Task CopyConstructor_CopiesItemNameAndPathOnly()
    {
        var item = new Item("Tank.Level", "LineA")
        {
            ClientHandle = 77,
            RequestedDataType = typeof(double),
            DeadbandPercent = 1.25f,
            SamplingRateMs = 250,
        };

        var result = new IdentifiedResult(item);

        await Assert.That(result.ItemName).IsEqualTo("Tank.Level");
        await Assert.That(result.Path).IsEqualTo("LineA");
        await Assert.That(result.ClientHandle).IsEqualTo(0);
        await Assert.That(result.ResultId).IsEqualTo(OpcResultId.Ok);
        await Assert.That(result.DiagnosticInfo).IsNull();
    }

    [Test]
    public async Task Initializers_PreserveResultIdCodeAndDiagnosticInfo()
    {
        var result = new IdentifiedResult("Valve.Command", "Unit1")
        {
            ClientHandle = 123,
            ResultId = OpcResultId.InvalidHandle,
            DiagnosticInfo = "server handle rejected",
        };

        await Assert.That(result.ClientHandle).IsEqualTo(123);
        await Assert.That(result.ResultId).IsEqualTo(OpcResultId.InvalidHandle);
        await Assert.That(result.ResultId.Code).IsEqualTo(unchecked((int)0xC0040001u));
        await Assert.That(result.DiagnosticInfo).IsEqualTo("server handle rejected");
    }

    [Test]
    public async Task Equality_UsesIdentifierFieldsAndIgnoresResultPayload()
    {
        var left = new IdentifiedResult("Item.A", "Path")
        {
            ClientHandle = 1,
            ResultId = OpcResultId.Ok,
            DiagnosticInfo = "ok",
        };
        var right = new IdentifiedResult("Item.A", "Path")
        {
            ClientHandle = 999,
            ResultId = OpcResultId.BadRights,
            DiagnosticInfo = "denied",
        };
        var differentPath = new IdentifiedResult("Item.A", "OtherPath");

        await Assert.That(left.Equals(right)).IsTrue();
        await Assert.That(left == right).IsTrue();
        await Assert.That(left.GetHashCode()).IsEqualTo(right.GetHashCode());
        await Assert.That(left.Equals(differentPath)).IsFalse();
        await Assert.That(left != differentPath).IsTrue();
    }

    [Test]
    public async Task Constructors_NullIdentifierArguments_ThrowArgumentNullException()
    {
        await Assert.That(() => new IdentifiedResult(null!))
            .Throws<ArgumentNullException>();
        await Assert.That(() => new IdentifiedResult((ItemIdentifier)null!))
            .Throws<ArgumentNullException>();
    }
}
