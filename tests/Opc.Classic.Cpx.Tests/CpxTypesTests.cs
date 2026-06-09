//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Opc.Classic.Cpx.Dcom;
using TUnit.Assertions.AssertConditions.Throws;
using TUnit.Core;

namespace Opc.Classic.Cpx.Tests;

public sealed class TypeDescriptionTests {
    [Test]
    public async Task ValueEquality_IncludesFieldSequence() {
        var first = new TypeDescription(
            "MotorStatus",
            "ns=vendor;MotorStatus",
            TypeKind.StructReference,
            isComplex: true,
            new[]
            {
                new TypeField("Running", TypeKind.Boolean),
                new TypeField("Speed", TypeKind.Double),
            });

        var second = new TypeDescription(
            "MotorStatus",
            "ns=vendor;MotorStatus",
            TypeKind.StructReference,
            isComplex: true,
            new[]
            {
                new TypeField("Running", TypeKind.Boolean),
                new TypeField("Speed", TypeKind.Double),
            });

        await Assert.That(first).IsEqualTo(second);
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
    }

    [Test]
    public async Task Constructor_RejectsInvalidIdentityOrType() {
        await Assert.That(() => { _ = new TypeDescription("", "id", TypeKind.Int32, isComplex: false); })
            .Throws<ArgumentException>();

        await Assert.That(() => { _ = new TypeDescription("Name", "", TypeKind.Int32, isComplex: false); })
            .Throws<ArgumentException>();

        var unknownKind = TypeKind.Unknown;
        await Assert.That(() => { _ = new TypeDescription("Name", "id", unknownKind, isComplex: false); })
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task TypeField_NormalizesOptionalStrings_AndRejectsNegativeCounts() {
        var field = new TypeField("Nested", TypeKind.StructReference, "  ", ElementCountFieldName: "Count");

        await Assert.That(field.TypeId).IsNull();
        await Assert.That(field.ElementCountFieldName).IsEqualTo("Count");

        await Assert.That(() => { _ = new TypeField("Bad", TypeKind.Int16, Length: -1); })
            .Throws<ArgumentOutOfRangeException>();
    }
}

public sealed class InstanceDescriptionTests {
    [Test]
    public async Task ValueEquality_IncludesFieldValuesRegardlessOfDictionaryOrder() {
        var first = new InstanceDescription(
            "Channel1.Device1.Motor",
            "MotorStatus",
            isComplex: true,
            new Dictionary<string, object?> {
                ["Running"] = true,
                ["Speed"] = 1200.0,
            },
            dictionaryId: "MotorDictionary");

        var second = new InstanceDescription(
            "Channel1.Device1.Motor",
            "MotorStatus",
            isComplex: true,
            new Dictionary<string, object?> {
                ["Speed"] = 1200.0,
                ["Running"] = true,
            },
            dictionaryId: "MotorDictionary");

        var retrieved = first.TryGet<double>("Speed", out var speed);

        await Assert.That(first).IsEqualTo(second);
        await Assert.That(first.GetHashCode()).IsEqualTo(second.GetHashCode());
        await Assert.That(retrieved).IsTrue();
        await Assert.That(speed).IsEqualTo(1200.0);
    }

    [Test]
    public async Task Constructor_RejectsInvalidIdentifiers() {
        await Assert.That(() => { _ = new InstanceDescription("", "Type", isComplex: true); })
            .Throws<ArgumentException>();

        await Assert.That(() => { _ = new InstanceDescription("Item", "", isComplex: true); })
            .Throws<ArgumentException>();

        await Assert.That(() => { _ = new InstanceDescription("Item", "Type", isComplex: true, typeSystemId: ""); })
            .Throws<ArgumentException>();
    }
}

public sealed class TypeDictionaryTests {
    [Test]
    public async Task FromTypes_LookupByNameAndTypeIdSucceeds() {
        var simple = new TypeDescription("Temperature", "TemperatureType", TypeKind.Double, isComplex: false);
        var complex = new TypeDescription(
            "MotorStatus",
            "MotorStatusType",
            TypeKind.StructReference,
            isComplex: true,
            new[] { new TypeField("Running", TypeKind.Boolean) });

        var dict = new TypeDictionary("PlantTypes", new[] { simple, complex }, defaultBigEndian: false);

        await Assert.That(dict.Name).IsEqualTo("PlantTypes");
        await Assert.That(dict.DefaultBigEndian).IsFalse();
        await Assert.That(dict.TryGet("Temperature")).IsEqualTo(simple);
        await Assert.That(dict.TryGetByTypeId("MotorStatusType")).IsEqualTo(complex);
        await Assert.That(dict.TryGet("Missing")).IsNull();
    }

    [Test]
    public async Task Lookup_IsCaseSensitive() {
        var dict = TypeDictionary.FromTypes(new TypeDescription("Capital", "CapitalType", TypeKind.String, isComplex: false));

        await Assert.That(dict.Contains("Capital")).IsTrue();
        await Assert.That(dict.Contains("capital")).IsFalse();
    }

    [Test]
    public async Task Constructor_RejectsDuplicateNamesAndTypeIds() {
        var duplicateNameA = new TypeDescription("Same", "A", TypeKind.Int16, isComplex: false);
        var duplicateNameB = new TypeDescription("Same", "B", TypeKind.Int32, isComplex: false);
        var duplicateIdA = new TypeDescription("A", "SameId", TypeKind.Int16, isComplex: false);
        var duplicateIdB = new TypeDescription("B", "SameId", TypeKind.Int32, isComplex: false);

        await Assert.That(() => { _ = TypeDictionary.FromTypes(duplicateNameA, duplicateNameB); })
            .Throws<ArgumentException>();

        await Assert.That(() => { _ = TypeDictionary.FromTypes(duplicateIdA, duplicateIdB); })
            .Throws<ArgumentException>();
    }
}

public sealed class DcomInterfaceIdTests {
    [Test]
    public async Task IOPCComplexDataItem_InterfaceId_MatchesSpec() {
        var expected = new Guid("7ECE6649-2C1E-494A-BB99-22D36FB3B0C3");
        await Assert.That(IOPCComplexDataItem.InterfaceId).IsEqualTo(expected);
    }

    [Test]
    public async Task IOPCComplexDataItem2_InterfaceId_MatchesSpec() {
        var expected = new Guid("44F68398-60AF-4F02-9442-172D058CB16F");
        await Assert.That(IOPCComplexDataItem2.InterfaceId).IsEqualTo(expected);
    }

    [Test]
    public async Task IOPCTypeLibrary_InterfaceId_MatchesSpec() {
        var expected = new Guid("B8C1B2C6-ACB7-4B7B-87B5-6EAC2CF63C31");
        await Assert.That(IOPCTypeLibrary.InterfaceId).IsEqualTo(expected);
    }
}
