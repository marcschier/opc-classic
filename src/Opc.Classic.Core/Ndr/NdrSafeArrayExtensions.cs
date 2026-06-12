//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// NDR wire encoding for OPC SAFEARRAYs, including multi-dimensional bounds,
// FADF_* feature flags, scalar elements, and VT_VARIANT elements.
//

namespace Opc.Classic.Ndr;

/// <summary>
/// NDR wire-format extensions for <see cref="OpcSafeArray"/>.
/// </summary>
public static class NdrSafeArrayExtensions
{
    /// <summary>Maximum SAFEARRAY rank accepted by the codec.</summary>
    public const int MaxSafeArrayDimensions = 256;

    private const ulong MaxSafeArrayPayloadBytes = 2UL * 1024UL * 1024UL * 1024UL;

    /// <summary>Encodes a SAFEARRAY descriptor and flattened element data.</summary>
    public static void WriteSafeArray(this ref NdrWriter writer, OpcSafeArray value)
    {
        ArgumentNullException.ThrowIfNull(value);
        ValidateRank(value.Rank);

        writer.AlignTo(4);

        int count = value.TotalElements;
        int cbElements = ElementSize(value.ElementType);
        ValidatePayloadByteSize(unchecked((ulong)count), unchecked((uint)cbElements));

        writer.WriteUInt16(unchecked((ushort)value.Rank));
        writer.WriteUInt16((ushort)value.Features);
        writer.WriteUInt32(unchecked((uint)cbElements));
        writer.WriteUInt32(0u);
        writer.WriteUInt32(unchecked((uint)count));
        writer.WriteUInt16((ushort)value.ElementType);
        writer.WriteUInt16(0);

        for (int i = 0; i < value.Rank; i++)
        {
            writer.WriteUInt32(unchecked((uint)value.Lengths[i]));
            writer.WriteInt32(value.LowerBounds[i]);
        }

        WriteElements(ref writer, value);
    }

    private static void ValidateRank(int rank)
    {
        if (rank <= 0 || rank > MaxSafeArrayDimensions)
        {
            throw new InvalidOperationException(
                $"NDR SAFEARRAY rank must be between 1 and {MaxSafeArrayDimensions} (got {rank}).");
        }
    }

    private static int ElementSize(VarType vt) => vt switch
    {
        VarType.VT_I1 or VarType.VT_UI1 => 1,
        VarType.VT_I2 or VarType.VT_UI2 or VarType.VT_BOOL => 2,
        VarType.VT_I4 or VarType.VT_UI4 or VarType.VT_R4 or VarType.VT_ERROR => 4,
        VarType.VT_I8 or VarType.VT_UI8 or VarType.VT_R8 or VarType.VT_DATE => 8,
        VarType.VT_CLSID => 16,
        VarType.VT_BSTR or VarType.VT_VARIANT or VarType.VT_RECORD => 0,
        _ => throw new InvalidOperationException(
            $"NDR SAFEARRAY codec does not support element type {vt}."),
    };

    private static void ValidatePayloadByteSize(ulong elementCount, uint cbElements)
    {
        if (cbElements == 0)
        {
            return;
        }

        ulong bytes = elementCount * cbElements;
        if (bytes > MaxSafeArrayPayloadBytes)
        {
            throw new InvalidDataException(
                $"NDR SAFEARRAY payload size {bytes} exceeds the 2 GiB safety limit.");
        }
    }

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
            case VarType.VT_VARIANT: WriteVariantElements(ref writer, (OpcVariant[])value.Data); return;
            case VarType.VT_RECORD: WriteRecordElements(ref writer, (OpcRecordValue?[])value.Data); return;
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

    private static void WriteVariantElements(ref NdrWriter w, OpcVariant[] a)
    {
        foreach (OpcVariant v in a)
        {
            w.WriteVariant(v);
        }
    }

    private static void WriteRecordElements(ref NdrWriter w, OpcRecordValue?[] a)
    {
        foreach (OpcRecordValue? v in a)
        {
            w.WriteVariant(new OpcVariant(VarType.VT_RECORD, v));
        }
    }

    /// <summary>Decodes a SAFEARRAY descriptor and flattened element data.</summary>
    public static OpcSafeArray ReadSafeArray(this ref NdrReader reader)
    {
        reader.AlignTo(4);

        ushort cDims = reader.ReadUInt16();
        if (cDims == 0 || cDims > MaxSafeArrayDimensions)
        {
            throw new InvalidDataException(
                $"NDR SAFEARRAY rank must be between 1 and {MaxSafeArrayDimensions} (got {cDims}).");
        }

        var features = (SafeArrayFeatures)reader.ReadUInt16();
        uint cbElements = reader.ReadUInt32();
        _ = reader.ReadUInt32();
        uint conformanceCount = reader.ReadUInt32();
        var vt = (VarType)reader.ReadUInt16();
        _ = reader.ReadUInt16();

        var lengths = new int[cDims];
        var lowerBounds = new int[cDims];
        ulong totalElements = 1;
        for (int i = 0; i < cDims; i++)
        {
            uint cElements = reader.ReadUInt32();
            if (cElements > (uint)int.MaxValue)
            {
                throw new InvalidDataException($"NDR SAFEARRAY cElements {cElements} too large.");
            }

            lengths[i] = unchecked((int)cElements);
            lowerBounds[i] = reader.ReadInt32();
            totalElements *= cElements;
            if (totalElements > (ulong)int.MaxValue)
            {
                throw new InvalidDataException($"NDR SAFEARRAY element count {totalElements} too large.");
            }
        }

        if (conformanceCount != totalElements)
        {
            throw new InvalidDataException(
                $"NDR SAFEARRAY conformance count {conformanceCount} does not match bounds product {totalElements}.");
        }

        int elementSize = ElementSize(vt);
        if (elementSize != 0 && cbElements != (uint)elementSize)
        {
            throw new InvalidDataException(
                $"NDR SAFEARRAY cbElements {cbElements} does not match {vt} element size {elementSize}.");
        }
        ValidatePayloadByteSize(totalElements, cbElements);

        int count = unchecked((int)totalElements);
        Array data = ReadElements(ref reader, vt, count);
        return new OpcSafeArray(vt, data, lengths, lowerBounds, features);
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
        VarType.VT_VARIANT => ReadVariantElements(ref reader, count),
        VarType.VT_RECORD => ReadRecordElements(ref reader, count),
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

    private static OpcVariant[] ReadVariantElements(ref NdrReader r, int n)
    {
        var a = new OpcVariant[n];
        for (int i = 0; i < n; i++)
        {
            a[i] = r.ReadVariant();
        }
        return a;
    }

    private static OpcRecordValue?[] ReadRecordElements(ref NdrReader r, int n)
    {
        var a = new OpcRecordValue?[n];
        for (int i = 0; i < n; i++)
        {
            OpcVariant recordVariant = r.ReadVariant();
            if (recordVariant.Type != VarType.VT_RECORD)
            {
                throw new InvalidDataException(
                    $"NDR SAFEARRAY VT_RECORD element decoded as {recordVariant.Type}.");
            }
            a[i] = recordVariant.AsRecord();
        }
        return a;
    }
}
