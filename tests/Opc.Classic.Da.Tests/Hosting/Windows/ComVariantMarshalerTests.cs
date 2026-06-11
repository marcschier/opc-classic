//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Opc.Classic.Da.Hosting.Windows;
using TUnit.Core;

namespace Opc.Classic.Da.Tests.Hosting.Windows;

/// <summary>
/// Round-trip tests for the COM VARIANT marshaler (cap-c1). Each scalar
/// VARTYPE writes through to native memory then reads back with the same
/// managed type + value. SAFEARRAY round-trips cover the most common
/// element types (I4, R8, BSTR).
/// </summary>
[SupportedOSPlatform("windows")]
public sealed class ComVariantMarshalerTests
{
    private static int VariantSize => IntPtr.Size == 8 ? 24 : 16;

    [Test]
    [Arguments(VarType.VT_I1, (sbyte)-42)]
    [Arguments(VarType.VT_UI1, (byte)200)]
    [Arguments(VarType.VT_I2, (short)-12345)]
    [Arguments(VarType.VT_UI2, (ushort)54321)]
    [Arguments(VarType.VT_I4, -1234567)]
    [Arguments(VarType.VT_UI4, 4000000000u)]
    [Arguments(VarType.VT_I8, -9000000000L)]
    [Arguments(VarType.VT_UI8, 18000000000ul)]
    [Arguments(VarType.VT_R4, 3.14159f)]
    [Arguments(VarType.VT_R8, 2.718281828)]
    [Arguments(VarType.VT_BOOL, true)]
    [Arguments(VarType.VT_BOOL, false)]
    [Arguments(VarType.VT_ERROR, unchecked((int)0x80004005))]
    public async Task Scalar_round_trip(VarType type, object boxedValue)
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr buf = Marshal.AllocCoTaskMem(VariantSize);
        try
        {
            OpcVariant source = new(type, boxedValue);
            CallMarshaler.Write(buf, source);
            OpcVariant readBack = CallMarshaler.Read(buf);

            await Assert.That(readBack.Type).IsEqualTo(type);
            await Assert.That(readBack.Boxed).IsEqualTo(boxedValue);
        }
        finally
        {
            CallMarshaler.Clear(buf);
            Marshal.FreeCoTaskMem(buf);
        }
    }

    [Test]
    public async Task Empty_variant_writes_vt_empty()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr buf = Marshal.AllocCoTaskMem(VariantSize);
        try
        {
            CallMarshaler.Write(buf, OpcVariant.Empty);
            OpcVariant readBack = CallMarshaler.Read(buf);

            await Assert.That(readBack.Type).IsEqualTo(VarType.VT_EMPTY);
            await Assert.That(readBack.Boxed).IsNull();
        }
        finally
        {
            Marshal.FreeCoTaskMem(buf);
        }
    }

    [Test]
    public async Task Bstr_round_trip()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr buf = Marshal.AllocCoTaskMem(VariantSize);
        try
        {
            CallMarshaler.Write(buf, OpcVariant.FromString("hello world"));
            OpcVariant readBack = CallMarshaler.Read(buf);

            await Assert.That(readBack.Type).IsEqualTo(VarType.VT_BSTR);
            await Assert.That(readBack.AsString()).IsEqualTo("hello world");
        }
        finally
        {
            CallMarshaler.Clear(buf);
            Marshal.FreeCoTaskMem(buf);
        }
    }

    [Test]
    public async Task Date_round_trip()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        DateTime expected = new(2026, 5, 27, 12, 34, 56, DateTimeKind.Utc);
        IntPtr buf = Marshal.AllocCoTaskMem(VariantSize);
        try
        {
            CallMarshaler.Write(buf, OpcVariant.FromDate(expected));
            OpcVariant readBack = CallMarshaler.Read(buf);

            await Assert.That(readBack.Type).IsEqualTo(VarType.VT_DATE);
            DateTime? actual = readBack.AsDate();
            await Assert.That(actual).IsNotNull();
            // OLE date round-trip is millisecond-faithful but may drop sub-ms; compare day/hour/min/sec.
            await Assert.That(actual!.Value.Year).IsEqualTo(expected.Year);
            await Assert.That(actual.Value.Month).IsEqualTo(expected.Month);
            await Assert.That(actual.Value.Hour).IsEqualTo(expected.Hour);
            await Assert.That(actual.Value.Second).IsEqualTo(expected.Second);
        }
        finally
        {
            Marshal.FreeCoTaskMem(buf);
        }
    }

    [Test]
    public async Task SafeArray_of_int32_round_trip()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        int[] expected = { 1, 2, 3, 4, 5 };
        IntPtr buf = Marshal.AllocCoTaskMem(VariantSize);
        try
        {
            var array = new OpcSafeArray(VarType.VT_I4, expected);
            CallMarshaler.Write(buf, OpcVariant.FromSafeArray(array));
            OpcVariant readBack = CallMarshaler.Read(buf);

            OpcSafeArray? actual = readBack.AsSafeArray();
            await Assert.That(actual).IsNotNull();
            await Assert.That(actual!.ElementType).IsEqualTo(VarType.VT_I4);
            int[] data = (int[])actual.Data;
            await Assert.That(data).IsEquivalentTo(expected);
        }
        finally
        {
            CallMarshaler.Clear(buf);
            Marshal.FreeCoTaskMem(buf);
        }
    }

    [Test]
    public async Task SafeArray_of_double_round_trip()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        double[] expected = { 1.5, 2.5, 3.14159, -0.0 };
        IntPtr buf = Marshal.AllocCoTaskMem(VariantSize);
        try
        {
            var array = new OpcSafeArray(VarType.VT_R8, expected);
            CallMarshaler.Write(buf, OpcVariant.FromSafeArray(array));
            OpcVariant readBack = CallMarshaler.Read(buf);

            OpcSafeArray? actual = readBack.AsSafeArray();
            await Assert.That(actual).IsNotNull();
            await Assert.That(actual!.ElementType).IsEqualTo(VarType.VT_R8);
            double[] data = (double[])actual.Data;
            await Assert.That(data).IsEquivalentTo(expected);
        }
        finally
        {
            CallMarshaler.Clear(buf);
            Marshal.FreeCoTaskMem(buf);
        }
    }

    [Test]
    public async Task SafeArray_of_bstr_round_trip()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        string[] expected = { "alpha", "beta", "gamma" };
        IntPtr buf = Marshal.AllocCoTaskMem(VariantSize);
        try
        {
            var array = new OpcSafeArray(VarType.VT_BSTR, expected);
            CallMarshaler.Write(buf, OpcVariant.FromSafeArray(array));
            OpcVariant readBack = CallMarshaler.Read(buf);

            OpcSafeArray? actual = readBack.AsSafeArray();
            await Assert.That(actual).IsNotNull();
            string[] data = (string[])actual!.Data;
            await Assert.That(data).IsEquivalentTo(expected);
        }
        finally
        {
            CallMarshaler.Clear(buf);
            Marshal.FreeCoTaskMem(buf);
        }
    }

    [Test]
    public async Task Bstr_allocate_and_read_round_trip()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr bstr = CallMarshaler.AllocBstr("test string");
        try
        {
            string? readBack = CallMarshaler.ReadBstr(bstr);
            await Assert.That(readBack).IsEqualTo("test string");
        }
        finally
        {
            Marshal.FreeBSTR(bstr);
        }
    }

    [Test]
    public async Task Clear_variant_resets_to_VT_EMPTY()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        IntPtr buf = Marshal.AllocCoTaskMem(VariantSize);
        try
        {
            CallMarshaler.Write(buf, OpcVariant.FromString("temp"));
            CallMarshaler.Clear(buf);

            short vt = Marshal.ReadInt16(buf);
            await Assert.That(vt).IsEqualTo((short)VarType.VT_EMPTY);
        }
        finally
        {
            Marshal.FreeCoTaskMem(buf);
        }
    }

    private static class CallMarshaler
    {
        internal static void Write(IntPtr dest, OpcVariant variant) =>
            ComVariantMarshaler.WriteVariant(dest, variant);
        internal static OpcVariant Read(IntPtr src) => ComVariantMarshaler.ReadVariant(src);
        internal static void Clear(IntPtr p) => ComVariantMarshaler.ClearVariant(p);
        internal static IntPtr AllocBstr(string? s) => ComVariantMarshaler.AllocateBstr(s);
        internal static string? ReadBstr(IntPtr p) => ComVariantMarshaler.ReadBstr(p);
    }
}
