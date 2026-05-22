//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// NDR wire encoding for OPC VARIANTs (scalar subset).
//
// Wire layout — the "encapsulated VARIANT" form documented in
// [MS-OAUT] §2.2.29.1:
//
//     UInt32 cbSize        // bytes remaining after this field
//     UInt32 rpcReserved   // must be 0
//     UInt16 vt            // VARTYPE
//     UInt16 wReserved1    // 0
//     UInt16 wReserved2    // 0
//     UInt16 wReserved3    // 0
//     body                 // per-VARTYPE encoded value
//
// SCOPE LIMITATION — this is a deliberately reduced subset:
//   * Scalar fixed-size types only (VT_EMPTY, VT_NULL, VT_I1/I2/I4/I8,
//     VT_UI1/UI2/UI4/UI8, VT_R4/R8, VT_BOOL, VT_DATE, VT_ERROR,
//     VT_FILETIME, VT_CLSID).
//   * Round-trip tested. Spec conformance against real Windows-emitted
//     wire dumps is a Phase 14 (Windows conformance) deliverable.
//   * BSTR (variable-length OLE string) is NOT yet handled — pending
//     decision on whether to emit BSTR header form or LPWSTR-like.
//   * SAFEARRAY, VT_VARIANT (nested), VT_BYREF, VT_RECORD: explicit
//     future work in Phase 5E.4+.
//

using System;
using System.Buffers.Binary;
using System.IO;

namespace OpcClassic.Ndr;

/// <summary>
/// NDR wire-format extensions for <see cref="OpcVariant"/> covering the
/// scalar VARIANT type set. See the file-level comment for scope.
/// </summary>
public static class NdrVariantExtensions
{
    private const int VariantHeaderBytes = 16;
    private const ushort BoolTrueWire = unchecked((ushort)-1);
    private const ushort BoolFalseWire = 0;

    /// <summary>Encodes a scalar <see cref="OpcVariant"/> per the wire layout above.</summary>
    public static void WriteVariant(this ref NdrWriter writer, OpcVariant value)
    {
        if (value.Type == VarType.VT_BSTR)
        {
            // BSTR has a variable-length body that includes its own internal
            // header (FLAGGED_WORD_BLOB). The cbSize field at the outer VARIANT
            // header is computed inclusive of that variable body, so we route
            // BSTR through a dedicated writer.
            WriteBstrVariant(ref writer, (string?)value.Boxed);
            return;
        }

        int bodyBytes = ComputeBodySize(value.Type);
        writer.AlignTo(4);
        writer.WriteUInt32(unchecked((uint)(VariantHeaderBytes - 8 + bodyBytes))); // cbSize: bytes after this field
        writer.WriteUInt32(0u);                                                     // rpcReserved
        writer.WriteUInt16((ushort)value.Type);                                     // vt
        writer.WriteUInt16(0);                                                      // wReserved1
        writer.WriteUInt16(0);                                                      // wReserved2
        writer.WriteUInt16(0);                                                      // wReserved3

        WriteBody(ref writer, value);
    }

    private static void WriteBstrVariant(ref NdrWriter writer, string? text)
    {
        // Variable body length = 4 (referent) + (text != null ? 4 (fFlags) +
        //                       4 (clSize) + text.Length * 2 : 0)
        int charCount = text?.Length ?? 0;
        int bodyBytes = text is null
            ? 4
            : 4 + 4 + 4 + charCount * 2;
        writer.AlignTo(4);
        writer.WriteUInt32(unchecked((uint)(VariantHeaderBytes - 8 + bodyBytes)));
        writer.WriteUInt32(0u);                       // rpcReserved
        writer.WriteUInt16((ushort)VarType.VT_BSTR);  // vt
        writer.WriteUInt16(0);
        writer.WriteUInt16(0);
        writer.WriteUInt16(0);

        if (text is null)
        {
            writer.WriteNullBstr();
        }
        else
        {
            writer.WriteBstr(text);
        }
    }

    private static int ComputeBodySize(VarType vt) => vt switch
    {
        VarType.VT_EMPTY or VarType.VT_NULL => 0,
        VarType.VT_I1 or VarType.VT_UI1 => 1,
        VarType.VT_I2 or VarType.VT_UI2 or VarType.VT_BOOL => 2,
        VarType.VT_I4 or VarType.VT_UI4 or VarType.VT_R4 or VarType.VT_ERROR => 4,
        VarType.VT_I8 or VarType.VT_UI8 or VarType.VT_R8 or VarType.VT_DATE or VarType.VT_FILETIME => 8,
        VarType.VT_CLSID => 16,
        _ => throw new InvalidOperationException(
            $"NDR VARIANT wire encoding is not supported for type {vt}."),
    };

    private static void WriteBody(ref NdrWriter writer, OpcVariant value)
    {
        switch (value.Type)
        {
            case VarType.VT_EMPTY:
            case VarType.VT_NULL:
                return;
            case VarType.VT_I1:
                writer.WriteByte(unchecked((byte)((sbyte)value.Boxed!)));
                return;
            case VarType.VT_UI1:
                writer.WriteByte((byte)value.Boxed!);
                return;
            case VarType.VT_I2:
                writer.WriteInt16((short)value.Boxed!);
                return;
            case VarType.VT_UI2:
                writer.WriteUInt16((ushort)value.Boxed!);
                return;
            case VarType.VT_BOOL:
                writer.WriteUInt16(((bool)value.Boxed!) ? BoolTrueWire : BoolFalseWire);
                return;
            case VarType.VT_I4:
                writer.WriteInt32((int)value.Boxed!);
                return;
            case VarType.VT_UI4:
                writer.WriteUInt32((uint)value.Boxed!);
                return;
            case VarType.VT_R4:
                writer.WriteSingle((float)value.Boxed!);
                return;
            case VarType.VT_ERROR:
                writer.WriteInt32((int)value.Boxed!);
                return;
            case VarType.VT_I8:
                writer.WriteInt64((long)value.Boxed!);
                return;
            case VarType.VT_UI8:
                writer.WriteUInt64((ulong)value.Boxed!);
                return;
            case VarType.VT_R8:
                writer.WriteDouble((double)value.Boxed!);
                return;
            case VarType.VT_DATE:
                writer.WriteDouble(((DateTime)value.Boxed!).ToOADate());
                return;
            case VarType.VT_FILETIME:
                writer.WriteFileTime((long)value.Boxed!);
                return;
            case VarType.VT_CLSID:
                writer.WriteGuid((Guid)value.Boxed!);
                return;
            default:
                throw new InvalidOperationException(
                    $"NDR VARIANT wire encoding is not supported for type {value.Type}.");
        }
    }

    /// <summary>Decodes a scalar <see cref="OpcVariant"/>.</summary>
    public static OpcVariant ReadVariant(this ref NdrReader reader)
    {
        reader.AlignTo(4);
        _ = reader.ReadUInt32();           // cbSize — not validated; reader is single-shot
        uint rpcReserved = reader.ReadUInt32();
        if (rpcReserved != 0u)
        {
            throw new InvalidDataException(
                $"NDR VARIANT rpcReserved must be 0 but was {rpcReserved}.");
        }
        ushort vtRaw = reader.ReadUInt16();
        _ = reader.ReadUInt16();           // wReserved1
        _ = reader.ReadUInt16();           // wReserved2
        _ = reader.ReadUInt16();           // wReserved3

        var vt = (VarType)vtRaw;
        return ReadBody(ref reader, vt);
    }

    private static OpcVariant ReadBody(ref NdrReader reader, VarType vt) => vt switch
    {
        VarType.VT_EMPTY => OpcVariant.Empty,
        VarType.VT_NULL => OpcVariant.Null,
        VarType.VT_I1 => OpcVariant.FromInt8(unchecked((sbyte)reader.ReadByte())),
        VarType.VT_UI1 => OpcVariant.FromUInt8(reader.ReadByte()),
        VarType.VT_I2 => OpcVariant.FromInt16(reader.ReadInt16()),
        VarType.VT_UI2 => OpcVariant.FromUInt16(reader.ReadUInt16()),
        VarType.VT_BOOL => OpcVariant.FromBoolean(reader.ReadUInt16() != 0),
        VarType.VT_I4 => OpcVariant.FromInt32(reader.ReadInt32()),
        VarType.VT_UI4 => OpcVariant.FromUInt32(reader.ReadUInt32()),
        VarType.VT_R4 => OpcVariant.FromSingle(reader.ReadSingle()),
        VarType.VT_ERROR => OpcVariant.FromError(reader.ReadInt32()),
        VarType.VT_I8 => OpcVariant.FromInt64(reader.ReadInt64()),
        VarType.VT_UI8 => OpcVariant.FromUInt64(reader.ReadUInt64()),
        VarType.VT_R8 => OpcVariant.FromDouble(reader.ReadDouble()),
        VarType.VT_DATE => OpcVariant.FromDate(DateTime.FromOADate(reader.ReadDouble())),
        VarType.VT_FILETIME => OpcVariant.FromFileTime(reader.ReadFileTime()),
        VarType.VT_CLSID => OpcVariant.FromClsid(reader.ReadGuid()),
        VarType.VT_BSTR => ReadBstrBody(ref reader),
        _ => throw new InvalidDataException(
            $"NDR VARIANT wire decoding is not supported for type {vt}."),
    };

    private static OpcVariant ReadBstrBody(ref NdrReader reader)
    {
        string? text = reader.ReadBstr();
        // Null BSTR is represented as a VT_BSTR variant carrying a null payload.
        return new OpcVariant(VarType.VT_BSTR, text);
    }
}
