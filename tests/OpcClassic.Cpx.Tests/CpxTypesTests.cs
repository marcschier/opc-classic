//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System.Collections.Generic;
using OpcClassic.Cpx;
using TUnit.Core;

namespace OpcClassic.Cpx.Tests;

public sealed class StructFieldTests
{
    [Test]
    public async Task Defaults_RepeatsZeroAndNullableReferences()
    {
        var f = new StructField { Name = "X", Kind = TypeKind.Int32 };
        await Assert.That(f.Repeats).IsEqualTo(0);
        await Assert.That(f.TypeReference).IsNull();
        await Assert.That(f.CountFieldName).IsNull();
        await Assert.That(f.ByteOrder).IsNull();
    }

    [Test]
    public async Task StructReference_Kind_HasTypeReference()
    {
        var f = new StructField
        {
            Name = "Nested",
            Kind = TypeKind.StructReference,
            TypeReference = "MyOtherStruct",
        };
        await Assert.That(f.Kind).IsEqualTo(TypeKind.StructReference);
        await Assert.That(f.TypeReference).IsEqualTo("MyOtherStruct");
    }

    [Test]
    public async Task DynamicArray_HasCountFieldName()
    {
        var f = new StructField
        {
            Name = "Data",
            Kind = TypeKind.UInt8,
            Repeats = -1,
            CountFieldName = "DataLength",
        };
        await Assert.That(f.Repeats).IsLessThan(0);
        await Assert.That(f.CountFieldName).IsEqualTo("DataLength");
    }
}

public sealed class StructTypeTests
{
    [Test]
    public async Task Defaults_LittleEndian_NotDefault_NoFields()
    {
        var s = new StructType { Name = "S" };
        await Assert.That(s.DefaultByteOrder).IsEqualTo(ByteOrder.LittleEndian);
        await Assert.That(s.IsDefault).IsFalse();
        await Assert.That(s.Fields.Count).IsEqualTo(0);
    }

    [Test]
    public async Task WithFields_PreservesOrder()
    {
        var s = new StructType
        {
            Name = "Triple",
            Fields = new[]
            {
                new StructField { Name = "a", Kind = TypeKind.Int32 },
                new StructField { Name = "b", Kind = TypeKind.Single },
                new StructField { Name = "c", Kind = TypeKind.String },
            },
        };
        await Assert.That(s.Fields.Count).IsEqualTo(3);
        await Assert.That(s.Fields[0].Name).IsEqualTo("a");
        await Assert.That(s.Fields[2].Name).IsEqualTo("c");
    }
}

public sealed class TypeDictionaryTests
{
    [Test]
    public async Task FromTypes_LookupSucceeds()
    {
        var sA = new StructType { Name = "A" };
        var sB = new StructType { Name = "B", IsDefault = true };
        var dict = TypeDictionary.FromTypes(sA, sB);

        await Assert.That(dict.TryGet("A")).IsEqualTo(sA);
        await Assert.That(dict.TryGet("B")).IsEqualTo(sB);
        await Assert.That(dict.TryGet("missing")).IsNull();
    }

    [Test]
    public async Task Default_ReturnsTheTypeMarkedDefault()
    {
        var sA = new StructType { Name = "A" };
        var sB = new StructType { Name = "B", IsDefault = true };
        var dict = TypeDictionary.FromTypes(sA, sB);
        await Assert.That(dict.Default).IsEqualTo(sB);
    }

    [Test]
    public async Task Default_Null_WhenNoTypeIsDefault()
    {
        var dict = TypeDictionary.FromTypes(new StructType { Name = "X" });
        await Assert.That(dict.Default).IsNull();
    }

    [Test]
    public async Task Contains_IsCaseSensitive()
    {
        var dict = TypeDictionary.FromTypes(new StructType { Name = "Capital" });
        await Assert.That(dict.Contains("Capital")).IsTrue();
        await Assert.That(dict.Contains("capital")).IsFalse();
    }
}

public sealed class ComplexValueTests
{
    [Test]
    public async Task Indexer_RetrievesFieldByName()
    {
        var t = new StructType { Name = "T" };
        var cv = new ComplexValue
        {
            Type = t,
            Fields = new Dictionary<string, object?>
            {
                ["age"] = 42,
                ["name"] = "alice",
            },
        };
        await Assert.That(cv["age"]).IsEqualTo(42);
        await Assert.That(cv["name"]).IsEqualTo("alice");
    }

    [Test]
    public async Task TryGet_TypedRetrieval_Succeeds()
    {
        var t = new StructType { Name = "T" };
        var cv = new ComplexValue
        {
            Type = t,
            Fields = new Dictionary<string, object?> { ["x"] = 3.14 },
        };
        var ok = cv.TryGet<double>("x", out var d);
        await Assert.That(ok).IsTrue();
        await Assert.That(d).IsEqualTo(3.14);
    }

    [Test]
    public async Task TryGet_WrongType_ReturnsFalse()
    {
        var t = new StructType { Name = "T" };
        var cv = new ComplexValue
        {
            Type = t,
            Fields = new Dictionary<string, object?> { ["x"] = 42 },
        };
        var ok = cv.TryGet<string>("x", out _);
        await Assert.That(ok).IsFalse();
    }

    [Test]
    public async Task TryGet_MissingField_ReturnsFalse()
    {
        var t = new StructType { Name = "T" };
        var cv = new ComplexValue
        {
            Type = t,
            Fields = new Dictionary<string, object?>(),
        };
        var ok = cv.TryGet<int>("doesnotexist", out _);
        await Assert.That(ok).IsFalse();
    }
}
