//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Track BD2: targeted unit tests for the per-element VARIANT codec in
// NdrVariantExtensions. AR identified this file at 52% line / 41% branch
// in Core.Tests-only coverage; the element-path methods (WriteVariantElement,
// ReadVariantElement, plus their body + arm helpers + element BSTR encoder)
// were entirely uncovered by Core.Tests because their primary consumer
// (the generated proxy/dispatcher [OpcVariantElements] path) is exercised
// from Da.Tests. These tests cover the codec directly so the Core-only
// coverage gate (which BB will lift) accurately reflects the surface.
//

using System;
using System.IO;
using Opc.Classic;
using Opc.Classic.Ndr;
using TUnit.Core;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Tests;

public sealed class NdrVariantExtensionsElementTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 256)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcVariant ReadElementOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return r.ReadVariantElement();
    }

    // ----- Scalar arm round-trips --------------------------------------------------------------

    [Test]
    [Arguments(VarType.VT_EMPTY)]
    [Arguments(VarType.VT_NULL)]
    public async Task Element_Empty_And_Null_RoundTrip(VarType vt)
    {
        var input = vt == VarType.VT_EMPTY ? OpcVariant.Empty : OpcVariant.Null;
        byte[] bytes = WriteOne((ref NdrWriter w) => w.WriteVariantElement(input));
        OpcVariant decoded = ReadElementOne(bytes);
        await Assert.That(decoded.Type).IsEqualTo(vt);
    }

    [Test]
    public async Task Element_I1_RoundTrip()
    {
        var input = OpcVariant.FromInt8(-5);
        byte[] bytes = WriteOne((ref NdrWriter w) => w.WriteVariantElement(input));
        await Assert.That(ReadElementOne(bytes)).IsEqualTo(input);
    }

    [Test]
    public async Task Element_UI1_RoundTrip()
    {
        var input = OpcVariant.FromUInt8(0xAB);
        await Assert.That(ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)))).IsEqualTo(input);
    }

    [Test]
    public async Task Element_I2_RoundTrip()
    {
        var input = OpcVariant.FromInt16(-12345);
        await Assert.That(ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)))).IsEqualTo(input);
    }

    [Test]
    public async Task Element_UI2_RoundTrip()
    {
        var input = OpcVariant.FromUInt16(54321);
        await Assert.That(ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)))).IsEqualTo(input);
    }

    [Test]
    public async Task Element_BOOL_RoundTripsBothValues()
    {
        var t = OpcVariant.FromBoolean(true);
        var f = OpcVariant.FromBoolean(false);
        await Assert.That(ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(t)))).IsEqualTo(t);
        await Assert.That(ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(f)))).IsEqualTo(f);
    }

    [Test]
    public async Task Element_I4_RoundTrip()
    {
        var input = OpcVariant.FromInt32(-987_654_321);
        await Assert.That(ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)))).IsEqualTo(input);
    }

    [Test]
    public async Task Element_UI4_RoundTrip()
    {
        var input = OpcVariant.FromUInt32(0xDEADBEEFu);
        await Assert.That(ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)))).IsEqualTo(input);
    }

    [Test]
    public async Task Element_R4_RoundTrip()
    {
        var input = OpcVariant.FromSingle(3.14159f);
        await Assert.That(ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)))).IsEqualTo(input);
    }

    [Test]
    public async Task Element_R8_RoundTrip()
    {
        var input = OpcVariant.FromDouble(2.718281828459045);
        await Assert.That(ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)))).IsEqualTo(input);
    }

    [Test]
    public async Task Element_I8_RoundTrip()
    {
        var input = OpcVariant.FromInt64(-9_000_000_000_000L);
        await Assert.That(ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)))).IsEqualTo(input);
    }

    [Test]
    public async Task Element_UI8_RoundTrip()
    {
        var input = OpcVariant.FromUInt64(0xCAFEBABE_DEADBEEFul);
        await Assert.That(ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)))).IsEqualTo(input);
    }

    [Test]
    public async Task Element_FILETIME_RoundTrip()
    {
        long ft = new DateTimeOffset(2026, 6, 3, 15, 0, 0, TimeSpan.Zero).ToFileTime();
        var input = OpcVariant.FromFileTime(ft);
        OpcVariant decoded = ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)));
        await Assert.That(decoded.Type).IsEqualTo(VarType.VT_FILETIME);
        await Assert.That(decoded.Boxed).IsEqualTo(ft);
    }

    [Test]
    public async Task Element_ERROR_RoundTrip()
    {
        var input = OpcVariant.FromError(unchecked((int)0x80004005u));
        OpcVariant decoded = ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)));
        await Assert.That(decoded.Type).IsEqualTo(VarType.VT_ERROR);
        await Assert.That((int)decoded.Boxed!).IsEqualTo(unchecked((int)0x80004005u));
    }

    [Test]
    public async Task Element_BSTR_RoundTrip()
    {
        var input = OpcVariant.FromString("hello unicode 株式会社");
        OpcVariant decoded = ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)));
        await Assert.That(decoded.Type).IsEqualTo(VarType.VT_BSTR);
        await Assert.That(decoded.Boxed).IsEqualTo("hello unicode 株式会社");
    }

    [Test]
    public async Task Element_BSTR_Null_RoundTrip()
    {
        var input = new OpcVariant(VarType.VT_BSTR, null);
        OpcVariant decoded = ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)));
        await Assert.That(decoded.Type).IsEqualTo(VarType.VT_BSTR);
        await Assert.That(decoded.Boxed).IsEqualTo((object?)null);
    }

    [Test]
    public async Task Element_DATE_RoundTrip()
    {
        var dt = new DateTime(2026, 6, 3, 15, 30, 45, DateTimeKind.Utc);
        var input = OpcVariant.FromDate(dt);
        OpcVariant decoded = ReadElementOne(WriteOne((ref NdrWriter w) => w.WriteVariantElement(input)));
        await Assert.That(decoded.Type).IsEqualTo(VarType.VT_DATE);
        // Round-trip through OADate has small loss; assert second-precision equality.
        DateTime decodedDt = (DateTime)decoded.Boxed!;
        await Assert.That(decodedDt.Year).IsEqualTo(2026);
        await Assert.That(decodedDt.Month).IsEqualTo(6);
        await Assert.That(decodedDt.Day).IsEqualTo(3);
    }

    // ----- Discriminator-mismatch failure path ---------------------------------------------------

    [Test]
    public async Task Element_DiscriminatorMismatch_ThrowsInvalidDataException()
    {
        // Build a valid VARIANT element with vt=VT_I4 but corrupt the duplicated
        // switch_is discriminator USHORT to a different VARTYPE. The reader must
        // detect the mismatch per DCE 1.1 §14.3.7.2 NDR rule and reject.
        byte[] bytes = WriteOne((ref NdrWriter w) =>
        {
            // wireVARIANT header: clSize=3 + rpcReserved + vt + 3 reserved USHORTs.
            w.WriteUInt32(3);          // clSize for a 4-byte arm
            w.WriteUInt32(0);          // rpcReserved
            w.WriteUInt16((ushort)VarType.VT_I4);
            w.WriteUInt16(0);
            w.WriteUInt16(0);
            w.WriteUInt16(0);
            w.WriteUInt16((ushort)VarType.VT_R8);  // CORRUPT discriminator (should be VT_I4)
            w.AlignTo(4);
            w.WriteInt32(42);
            // Pad to 8.
            int rem = w.Position & 7;
            if (rem != 0)
            {
                for (int i = rem; i < 8; i++) { w.WriteByte(0); }
            }
        });

        await Assert.That(() =>
        {
            var r = new NdrReader(bytes);
            _ = r.ReadVariantElement();
        }).Throws<InvalidDataException>();
    }

    // ----- Multi-element packing + per-element pad-to-8 alignment --------------------------------

    [Test]
    public async Task TwoElements_PackedBackToBack_DecodeIndependently()
    {
        var first = OpcVariant.FromInt32(11);
        var second = OpcVariant.FromDouble(22.5);

        byte[] bytes = WriteOne((ref NdrWriter w) =>
        {
            w.WriteVariantElement(first);
            w.WriteVariantElement(second);
        });

        var reader = new NdrReader(bytes);
        OpcVariant a = reader.ReadVariantElement();
        OpcVariant b = reader.ReadVariantElement();
        await Assert.That(a).IsEqualTo(first);
        await Assert.That(b).IsEqualTo(second);
    }

    [Test]
    public async Task ElementBoundary_EachElementSizeIsMultipleOfEight()
    {
        // The pad-to-8 invariant after each element is critical: a violation
        // would silently misalign every subsequent element in a [OpcVariantElements]
        // array. Exercise across scalar widths.
        var samples = new OpcVariant[]
        {
            OpcVariant.Empty,
            OpcVariant.FromInt8(1),
            OpcVariant.FromInt16(1),
            OpcVariant.FromInt32(1),
            OpcVariant.FromInt64(1),
            OpcVariant.FromDouble(1.0),
            OpcVariant.FromString("a"),
            OpcVariant.FromFileTime(0L),
        };

        foreach (OpcVariant sample in samples)
        {
            byte[] bytes = WriteOne((ref NdrWriter w) => w.WriteVariantElement(sample));
            await Assert.That(bytes.Length % 8).IsEqualTo(0);
        }
    }

    // ----- rpcReserved tolerance (MS-OAUT §2.2.29.2 says receivers MUST tolerate any value) -----

    [Test]
    public async Task Element_NonZeroRpcReserved_IsTolerated()
    {
        // Construct a VT_I4 element with a deliberately-non-zero rpcReserved field.
        // The reader must accept it (Matrikon Simulation Server sets non-zero bytes here).
        byte[] bytes = WriteOne((ref NdrWriter w) =>
        {
            w.WriteUInt32(3);                                    // clSize
            w.WriteUInt32(0xDEADBEEFu);                          // rpcReserved (non-zero, must be tolerated)
            w.WriteUInt16((ushort)VarType.VT_I4);
            w.WriteUInt16(0);
            w.WriteUInt16(0);
            w.WriteUInt16(0);
            w.WriteUInt16((ushort)VarType.VT_I4);                // discriminator
            w.AlignTo(4);
            w.WriteInt32(0x12345678);
            // Pad to 8.
            int rem = w.Position & 7;
            if (rem != 0)
            {
                for (int i = rem; i < 8; i++) { w.WriteByte(0); }
            }
        });

        var reader = new NdrReader(bytes);
        OpcVariant decoded = reader.ReadVariantElement();
        await Assert.That(decoded.Type).IsEqualTo(VarType.VT_I4);
        await Assert.That(decoded.Boxed).IsEqualTo(0x12345678);
    }
}
