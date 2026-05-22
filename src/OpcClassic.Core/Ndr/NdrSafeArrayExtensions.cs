//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// NDR wire encoding for OPC SAFEARRAYs (1-D scalar subset).
//
// Wire layout — a simplified rendering of the OAUT SAFEARRAY marshalling
// form documented in [MS-OAUT] §2.2.30. The scope is intentionally
// narrow:
//
//   * 1-dimensional arrays only.
//   * Element types: VT_I1/I2/I4/I8, VT_UI1/UI2/UI4/UI8, VT_R4, VT_R8,
//     VT_BOOL, VT_DATE, VT_BSTR, VT_CLSID, VT_ERROR.
//   * fFeatures always carries FADF_HAVEVARTYPE so the receiver can
//     dispatch on the element vartype.
//
// Wire structure (after outer 4-byte alignment):
//
//   UInt16 cDims              always 1 in this subset
//   UInt16 fFeatures          FADF_HAVEVARTYPE = 0x0080
//   UInt32 cbElements         element size in bytes (0 = variable, e.g. BSTR)
//   UInt32 cLocks             0
//   UInt32 conformanceCount   cElements (NDR conformance prefix)
//   UInt16 vt                 element VARTYPE
//   UInt16 padding            0
//   UInt32 cElements          repeat for the dimension bound
//   Int32  lLbound            dimension lower bound
//   element[cElements]        each element encoded per its VARTYPE
//
// Spec-conformance validation against real OAUT marshalling is a
// Phase 14 deliverable; this form is consistent with itself
// (round-trip tested) and suffices for the call-shim generator.
//

using System;
using System.IO;

namespace OpcClassic.Ndr;

/// <summary>
/// NDR wire-format extensions for <see cref="OpcSafeArray"/>. See the
/// file-level comment for scope.
/// </summary>
public static class NdrSafeArrayExtensions
{
    private const ushort FadfHaveVartype = 0x0080;

    /// <summary>Encodes a 1-D scalar SAFEARRAY.</summary>
    public static void WriteSafeArray(this ref NdrWriter writer, OpcSafeArray value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value.Rank != 1)
        {
            throw new InvalidOperationException(
                $"NDR SAFEARRAY codec currently supports rank=1 only (got rank={value.Rank}).");
        }

        writer.AlignTo(4);

        int count = value.Lengths[0];
        int cbElements = ElementSize(value.ElementType);

        writer.WriteUInt16(1);                          // cDims
        writer.WriteUInt16(FadfHaveVartype);            // fFeatures
        writer.WriteUInt32(unchecked((uint)cbElements));// cbElements (0 for variable, e.g. BSTR)
        writer.WriteUInt32(0u);                         // cLocks
        writer.WriteUInt32(unchecked((uint)count));     // conformance header
        writer.WriteUInt16((ushort)value.ElementType);  // vt
        writer.WriteUInt16(0);                          // padding
        writer.WriteUInt32(unchecked((uint)count));     // cElements (bound)
        writer.WriteInt32(value.LowerBounds[0]);        // lLbound

        WriteElements(ref writer, value);
    }

    private static int ElementSize(VarType vt) => vt switch
    {
        VarType.VT_I1 or VarType.VT_UI1 => 1,
        VarType.VT_I2 or VarType.VT_UI2 or VarType.VT_BOOL => 2,
        VarType.VT_I4 or VarType.VT_UI4 or VarType.VT_R4 or VarType.VT_ERROR => 4,
        VarType.VT_I8 or VarType.VT_UI8 or VarType.VT_R8 or VarType.VT_DATE => 8,
        VarType.VT_CLSID => 16,
        VarType.VT_BSTR => 0,  // variable
        _ => throw new InvalidOperationException(
            $"NDR SAFEARRAY codec does not support element type {vt}."),
    };

    private static void WriteElements(ref NdrWriter writer, OpcSafeArray value)
    {
        switch (value.ElementType)
        {
            case VarType.VT_I1: WriteInt8Elements(ref writer, (sbyte[])value.Data); return;
            case VarType.VT_UI1: writer.WriteRawBytes((byte[])value.Data); return;
            case VarType.VT_I2: WriteInt16Elements(ref writer, (short[])value.Data); return;
            case VarType.VT_UI2: WriteUInt16Elements(ref writer, (ushort[])value.Data); return;
            case VarType.VT_BOOL: WriteBoolElements(ref writer, (bool[])value.Data); return;
            case VarType.VT_I4: WriteInt32Elements(ref writer, (int[])value.Data); return;
            case VarType.VT_UI4: WriteUInt32Elements(ref writer, (uint[])value.Data); return;
            case VarType.VT_R4: WriteSingleElements(ref writer, (float[])value.Data); return;
            case VarType.VT_ERROR: WriteInt32Elements(ref writer, (int[])value.Data); return;
            case VarType.VT_I8: WriteInt64Elements(ref writer, (long[])value.Data); return;
            case VarType.VT_UI8: WriteUInt64Elements(ref writer, (ulong[])value.Data); return;
            case VarType.VT_R8: WriteDoubleElements(ref writer, (double[])value.Data); return;
            case VarType.VT_DATE: WriteDateElements(ref writer, (DateTime[])value.Data); return;
            case VarType.VT_CLSID: WriteGuidElements(ref writer, (Guid[])value.Data); return;
            case VarType.VT_BSTR: WriteBstrElements(ref writer, (string?[])value.Data); return;
            default:
                throw new InvalidOperationException(
                    $"NDR SAFEARRAY codec does not support element type {value.ElementType}.");
        }
    }

    private static void WriteInt8Elements(ref NdrWriter w, sbyte[] a) { foreach (sbyte v in a) { w.WriteByte(unchecked((byte)v)); } }
    private static void WriteInt16Elements(ref NdrWriter w, short[] a) { foreach (short v in a) { w.WriteInt16(v); } }
    private static void WriteUInt16Elements(ref NdrWriter w, ushort[] a) { foreach (ushort v in a) { w.WriteUInt16(v); } }
    private static void WriteBoolElements(ref NdrWriter w, bool[] a) { foreach (bool v in a) { w.WriteUInt16(v ? unchecked((ushort)-1) : (ushort)0); } }
    private static void WriteInt32Elements(ref NdrWriter w, int[] a) { foreach (int v in a) { w.WriteInt32(v); } }
    private static void WriteUInt32Elements(ref NdrWriter w, uint[] a) { foreach (uint v in a) { w.WriteUInt32(v); } }
    private static void WriteSingleElements(ref NdrWriter w, float[] a) { foreach (float v in a) { w.WriteSingle(v); } }
    private static void WriteInt64Elements(ref NdrWriter w, long[] a) { foreach (long v in a) { w.WriteInt64(v); } }
    private static void WriteUInt64Elements(ref NdrWriter w, ulong[] a) { foreach (ulong v in a) { w.WriteUInt64(v); } }
    private static void WriteDoubleElements(ref NdrWriter w, double[] a) { foreach (double v in a) { w.WriteDouble(v); } }
    private static void WriteDateElements(ref NdrWriter w, DateTime[] a) { foreach (DateTime v in a) { w.WriteDouble(v.ToOADate()); } }
    private static void WriteGuidElements(ref NdrWriter w, Guid[] a) { foreach (Guid v in a) { w.WriteGuid(v); } }
    private static void WriteBstrElements(ref NdrWriter w, string?[] a)
    {
        foreach (string? v in a)
        {
            if (v is null) { w.WriteNullBstr(); }
            else { w.WriteBstr(v); }
        }
    }

    /// <summary>Decodes a 1-D scalar SAFEARRAY.</summary>
    public static OpcSafeArray ReadSafeArray(this ref NdrReader reader)
    {
        reader.AlignTo(4);

        ushort cDims = reader.ReadUInt16();
        if (cDims != 1)
        {
            throw new InvalidDataException(
                $"NDR SAFEARRAY codec currently supports rank=1 only (got rank={cDims}).");
        }

        _ = reader.ReadUInt16();              // fFeatures - ignored
        _ = reader.ReadUInt32();              // cbElements - ignored, dispatched on vt instead
        _ = reader.ReadUInt32();              // cLocks - must be 0 on the wire
        _ = reader.ReadUInt32();              // conformance count
        var vt = (VarType)reader.ReadUInt16();
        _ = reader.ReadUInt16();              // padding
        uint cElements = reader.ReadUInt32(); // bound count
        int lLbound = reader.ReadInt32();     // lower bound

        if (cElements > (uint)int.MaxValue)
        {
            throw new InvalidDataException($"NDR SAFEARRAY cElements {cElements} too large.");
        }

        int count = (int)cElements;
        Array data = ReadElements(ref reader, vt, count);
        return new OpcSafeArray(vt, data, new[] { count }, new[] { lLbound });
    }

    private static Array ReadElements(ref NdrReader reader, VarType vt, int count) => vt switch
    {
        VarType.VT_I1 => ReadInt8Elements(ref reader, count),
        VarType.VT_UI1 => reader.ReadRawBytes(count).ToArray(),
        VarType.VT_I2 => ReadInt16Elements(ref reader, count),
        VarType.VT_UI2 => ReadUInt16Elements(ref reader, count),
        VarType.VT_BOOL => ReadBooleanElements(ref reader, count),
        VarType.VT_I4 => ReadInt32Elements(ref reader, count),
        VarType.VT_UI4 => ReadUInt32Elements(ref reader, count),
        VarType.VT_R4 => ReadSingleElements(ref reader, count),
        VarType.VT_ERROR => ReadInt32Elements(ref reader, count),
        VarType.VT_I8 => ReadInt64Elements(ref reader, count),
        VarType.VT_UI8 => ReadUInt64Elements(ref reader, count),
        VarType.VT_R8 => ReadDoubleElements(ref reader, count),
        VarType.VT_DATE => ReadDateElements(ref reader, count),
        VarType.VT_CLSID => ReadGuidElements(ref reader, count),
        VarType.VT_BSTR => ReadBstrElements(ref reader, count),
        _ => throw new InvalidDataException(
            $"NDR SAFEARRAY codec does not support element type {vt}."),
    };

    private static sbyte[] ReadInt8Elements(ref NdrReader r, int n) { var a = new sbyte[n]; for (int i = 0; i < n; i++) { a[i] = unchecked((sbyte)r.ReadByte()); } return a; }
    private static short[] ReadInt16Elements(ref NdrReader r, int n) { var a = new short[n]; for (int i = 0; i < n; i++) { a[i] = r.ReadInt16(); } return a; }
    private static ushort[] ReadUInt16Elements(ref NdrReader r, int n) { var a = new ushort[n]; for (int i = 0; i < n; i++) { a[i] = r.ReadUInt16(); } return a; }
    private static bool[] ReadBooleanElements(ref NdrReader r, int n) { var a = new bool[n]; for (int i = 0; i < n; i++) { a[i] = r.ReadUInt16() != 0; } return a; }
    private static int[] ReadInt32Elements(ref NdrReader r, int n) { var a = new int[n]; for (int i = 0; i < n; i++) { a[i] = r.ReadInt32(); } return a; }
    private static uint[] ReadUInt32Elements(ref NdrReader r, int n) { var a = new uint[n]; for (int i = 0; i < n; i++) { a[i] = r.ReadUInt32(); } return a; }
    private static float[] ReadSingleElements(ref NdrReader r, int n) { var a = new float[n]; for (int i = 0; i < n; i++) { a[i] = r.ReadSingle(); } return a; }
    private static long[] ReadInt64Elements(ref NdrReader r, int n) { var a = new long[n]; for (int i = 0; i < n; i++) { a[i] = r.ReadInt64(); } return a; }
    private static ulong[] ReadUInt64Elements(ref NdrReader r, int n) { var a = new ulong[n]; for (int i = 0; i < n; i++) { a[i] = r.ReadUInt64(); } return a; }
    private static double[] ReadDoubleElements(ref NdrReader r, int n) { var a = new double[n]; for (int i = 0; i < n; i++) { a[i] = r.ReadDouble(); } return a; }
    private static DateTime[] ReadDateElements(ref NdrReader r, int n) { var a = new DateTime[n]; for (int i = 0; i < n; i++) { a[i] = DateTime.FromOADate(r.ReadDouble()); } return a; }
    private static Guid[] ReadGuidElements(ref NdrReader r, int n) { var a = new Guid[n]; for (int i = 0; i < n; i++) { a[i] = r.ReadGuid(); } return a; }
    private static string?[] ReadBstrElements(ref NdrReader r, int n) { var a = new string?[n]; for (int i = 0; i < n; i++) { a[i] = r.ReadBstr(); } return a; }
}
