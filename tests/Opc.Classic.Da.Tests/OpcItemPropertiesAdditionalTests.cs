//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Da.Tests;

public sealed class OpcItemPropertiesAdditionalTests {
    [Test]
    public async Task Constructor_PreservesErrorIdAndPropertyArrayReference() {
        OpcItemPropertyResult[] properties =
        [
            new(
                DataType: VarType.VT_BSTR,
                PropertyId: 100,
                ItemId: "Pump.Speed.Value",
                Description: "Item Value",
                Value: OpcVariant.FromString("running"),
                ErrorId: 0),
            new(
                DataType: VarType.VT_I4,
                PropertyId: 101,
                ItemId: null,
                Description: "Quality",
                Value: OpcVariant.FromInt32(192),
                ErrorId: OpcResultId.InvalidPid.Code),
        ];

        var itemProperties = new OpcItemProperties(OpcResultId.UnknownItemId.Code, properties);

        await Assert.That(itemProperties.ErrorId).IsEqualTo(unchecked((int)0xC0040007u));
        await Assert.That(itemProperties.Properties).IsSameReferenceAs(properties);
        await Assert.That(itemProperties.Properties[0].PropertyId).IsEqualTo(100);
        await Assert.That(itemProperties.Properties[0].ItemId).IsEqualTo("Pump.Speed.Value");
        await Assert.That(itemProperties.Properties[0].Value.AsString()).IsEqualTo("running");
        await Assert.That(itemProperties.Properties[1].ErrorId).IsEqualTo(unchecked((int)0xC0040203u));
    }

    [Test]
    public async Task WithExpression_ReplacesPropertiesThroughInitSetter() {
        var original = new OpcItemProperties(0, Array.Empty<OpcItemPropertyResult>());
        OpcItemPropertyResult[] replacement =
        [
            new(
                DataType: VarType.VT_BOOL,
                PropertyId: 103,
                ItemId: "Pump.Running",
                Description: "Running",
                Value: OpcVariant.FromBoolean(true),
                ErrorId: 0),
        ];

        OpcItemProperties updated = original with {
            ErrorId = OpcResultId.Ok.Code,
            Properties = replacement,
        };

        await Assert.That(updated.ErrorId).IsEqualTo(0);
        await Assert.That(updated.Properties).IsSameReferenceAs(replacement);
        await Assert.That(updated.Properties[0].DataType).IsEqualTo(VarType.VT_BOOL);
        await Assert.That(updated.Properties[0].Value.AsBoolean()).IsTrue();
    }

    [Test]
    public async Task RecordEquality_UsesArrayReferenceForProperties() {
        OpcItemPropertyResult[] shared = new OpcItemPropertyResult[0];
        var left = new OpcItemProperties(0, shared);
        var sameArray = new OpcItemProperties(0, shared);
        var differentArray = new OpcItemProperties(0, new OpcItemPropertyResult[0]);
        var differentError = new OpcItemProperties(OpcResultId.InvalidPid.Code, shared);

        await Assert.That(left.Equals(sameArray)).IsTrue();
        await Assert.That(left.GetHashCode()).IsEqualTo(sameArray.GetHashCode());
        await Assert.That(left.Equals(differentArray)).IsFalse();
        await Assert.That(left.Equals(differentError)).IsFalse();
    }

    [Test]
    public async Task NullProperties_InConstructorOrInitSetter_ThrowArgumentNullException() {
        var original = new OpcItemProperties(0, Array.Empty<OpcItemPropertyResult>());

        await Assert.That(() => new OpcItemProperties(0, null!))
            .Throws<ArgumentNullException>();
        await Assert.That(() => original with { Properties = null! })
            .Throws<ArgumentNullException>();
    }
}
