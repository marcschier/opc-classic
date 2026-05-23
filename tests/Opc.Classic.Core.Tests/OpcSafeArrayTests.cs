//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic;
using TUnit.Core;

namespace Opc.Classic.Tests;

public sealed class OpcSafeArrayTests
{
    [Test]
    public async Task OfInt32_Single_DimensionDefaults()
    {
        var arr = OpcSafeArray.OfInt32(new[] { 1, 2, 3, 4 });
        await Assert.That(arr.ElementType).IsEqualTo(VarType.VT_I4);
        await Assert.That(arr.Rank).IsEqualTo(1);
        await Assert.That(arr.Lengths[0]).IsEqualTo(4);
        await Assert.That(arr.LowerBounds[0]).IsEqualTo(0);
        await Assert.That(arr.TotalElements).IsEqualTo(4);
    }

    [Test]
    public async Task OfDouble_Empty_IsValid()
    {
        var arr = OpcSafeArray.OfDouble(Array.Empty<double>());
        await Assert.That(arr.Rank).IsEqualTo(1);
        await Assert.That(arr.Lengths[0]).IsEqualTo(0);
        await Assert.That(arr.TotalElements).IsEqualTo(0);
    }

    [Test]
    public async Task OfString_PreservesElements()
    {
        var input = new[] { "Tag1", "Tag2", "Tag3" };
        var arr = OpcSafeArray.OfString(input);
        await Assert.That(arr.ElementType).IsEqualTo(VarType.VT_BSTR);
        await Assert.That(arr.Data.Length).IsEqualTo(3);
        await Assert.That((string?)arr.Data.GetValue(1)).IsEqualTo("Tag2");
    }

    [Test]
    public async Task ExplicitDimensions_PackedRowMajor()
    {
        // 2x3 logical shape, 6 elements row-major.
        var data = new int[] { 1, 2, 3, 4, 5, 6 };
        var arr = new OpcSafeArray(VarType.VT_I4, data, lengths: new[] { 2, 3 });
        await Assert.That(arr.Rank).IsEqualTo(2);
        await Assert.That(arr.Lengths[0]).IsEqualTo(2);
        await Assert.That(arr.Lengths[1]).IsEqualTo(3);
        await Assert.That(arr.TotalElements).IsEqualTo(6);
    }

    [Test]
    public async Task ExplicitDimensions_LowerBoundsPreserved()
    {
        var arr = new OpcSafeArray(
            VarType.VT_I4,
            new int[] { 10, 20, 30 },
            lengths: new[] { 3 },
            lowerBounds: new[] { 5 });
        await Assert.That(arr.LowerBounds[0]).IsEqualTo(5);
    }

    [Test]
    public async Task Constructor_RejectsMismatchedRanks()
    {
        bool threw = false;
        try
        {
            _ = new OpcSafeArray(
                VarType.VT_I4,
                new int[] { 1 },
                lengths: new[] { 1, 1 },
                lowerBounds: new[] { 0 });
        }
        catch (ArgumentException)
        {
            threw = true;
        }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Constructor_RejectsLengthProductMismatch()
    {
        bool threw = false;
        try
        {
            _ = new OpcSafeArray(
                VarType.VT_I4,
                new int[] { 1, 2, 3 },          // 3 elements
                lengths: new[] { 2, 3 });        // expects 6
        }
        catch (ArgumentException)
        {
            threw = true;
        }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Constructor_RejectsNegativeLength()
    {
        bool threw = false;
        try
        {
            _ = new OpcSafeArray(
                VarType.VT_I4,
                Array.Empty<int>(),
                lengths: new[] { -1 });
        }
        catch (ArgumentOutOfRangeException)
        {
            threw = true;
        }
        await Assert.That(threw).IsTrue();
    }

    [Test]
    public async Task Equality_IsStructural()
    {
        var a = OpcSafeArray.OfInt32(new[] { 1, 2, 3 });
        var b = OpcSafeArray.OfInt32(new[] { 1, 2, 3 });
        var c = OpcSafeArray.OfInt32(new[] { 1, 2, 4 });
        var d = OpcSafeArray.OfInt32(new[] { 1, 2 });
        var e = OpcSafeArray.OfDouble(new[] { 1.0, 2.0, 3.0 });

        await Assert.That(a).IsEqualTo(b);
        await Assert.That(a).IsNotEqualTo(c);
        await Assert.That(a).IsNotEqualTo(d);
        await Assert.That(a).IsNotEqualTo(e);
    }

    [Test]
    public async Task Equality_DifferentLowerBounds_NotEqual()
    {
        var a = new OpcSafeArray(VarType.VT_I4, new int[] { 1, 2 }, lengths: new[] { 2 }, lowerBounds: new[] { 0 });
        var b = new OpcSafeArray(VarType.VT_I4, new int[] { 1, 2 }, lengths: new[] { 2 }, lowerBounds: new[] { 1 });
        await Assert.That(a).IsNotEqualTo(b);
    }

    [Test]
    public async Task HashCode_StableForEqualArrays()
    {
        var a = OpcSafeArray.OfDouble(new[] { 1.0, 2.0, 3.0 });
        var b = OpcSafeArray.OfDouble(new[] { 1.0, 2.0, 3.0 });
        await Assert.That(a.GetHashCode()).IsEqualTo(b.GetHashCode());
    }
}
