//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// NDR wire encoding for OPC VARIANTs, including scalar values, SAFEARRAYs,
// nested VT_VARIANT values, VT_BYREF pointers, and optional VT_RECORD payloads.
//

using System;
using System.IO;

namespace Opc.Classic.Ndr;

/// <summary>
/// NDR wire-format extensions for <see cref="OpcVariant"/>.
/// </summary>
public static class NdrVariantExtensions
{
    /// <summary>Maximum nested VT_VARIANT depth accepted by the codec.</summary>
    public const int MaxVariantRecursionDepth = 64;

    private const int VariantHeaderBytes = 16;
    private const ushort BoolTrueWire = unchecked((ushort)-1);
    private const ushort BoolFalseWire = 0;

    /// <summary>Encodes a <see cref="OpcVariant"/> per [MS-OAUT] §2.2.29.</summary>
    public static void WriteVariant(this ref NdrWriter writer, OpcVariant value) =>
        WriteVariantCore(ref writer, value, depth: 0);

    /// <summary>
    /// Encodes a single VARIANT element inside an NDR conformant array of
    /// VARIANT pointers (DCE 1.1 §14.3.12.3 deferred pointer pile). The body
    /// layout is the canonical wireVARIANT (MS-OAUT §2.2.29.2) — clSize +
    /// rpcReserved + vt + 3 reserved USHORTs — followed by the [switch_is(vt)]
    /// union which is emitted with its duplicated discriminator USHORT,
    /// natural-alignment padding for the arm, and a trailing pad to 8 bytes so
    /// the next element starts at an 8-byte boundary. The per-element
    /// unique-pointer referent + inline-pad-to-8 are emitted by the caller
    /// (the generated proxy/dispatcher), not by this helper.
    /// </summary>
    public static void WriteVariantElement(this ref NdrWriter writer, OpcVariant value)
    {
        int startPos = writer.Position;
        WriteVariantElementBody(ref writer, value);
        int written = writer.Position - startPos;
        int padTo = (written + 7) & ~7;
        for (int i = written; i < padTo; i++) { writer.WriteByte(0); }
    }

    /// <summary>
    /// Decodes a single VARIANT element inside an NDR conformant array of
    /// VARIANT pointers. The per-element referent and inline-pad-to-8 must
    /// have been consumed by the caller; this reader handles the canonical
    /// wireVARIANT body, the duplicated [switch_is(vt)] union discriminator,
    /// natural-alignment padding, the arm, and a trailing pad to 8 bytes.
    /// </summary>
    public static OpcVariant ReadVariantElement(this ref NdrReader reader)
    {
        int startPos = reader.Position;
        OpcVariant value = ReadVariantElementBody(ref reader);
        int read = reader.Position - startPos;
        int padTo = (read + 7) & ~7;
        for (int i = read; i < padTo; i++) { _ = reader.ReadByte(); }
        return value;
    }

    private static void WriteVariantElementBody(ref NdrWriter writer, OpcVariant value)
    {
        var vt = value.Type;
        // Canonical 16-byte wireVARIANT header: clSize + rpcReserved + vt + 3 reserved USHORTs.
        // clSize is conventionally a small per-VARTYPE constant used by Matrikon (3 for 4-byte arms,
        // 4 for 8-byte arms, etc.); we mirror what we observe on real wire traces.
        int clSize = vt switch
        {
            VarType.VT_R8 or VarType.VT_I8 or VarType.VT_UI8 or VarType.VT_DATE or VarType.VT_CY or VarType.VT_FILETIME => 4,
            VarType.VT_BSTR or VarType.VT_DISPATCH or VarType.VT_UNKNOWN or VarType.VT_VARIANT => 6,
            _ => 3,
        };
        writer.WriteUInt32((uint)clSize);
        writer.WriteUInt32(0u);
        writer.WriteUInt16((ushort)vt);
        writer.WriteUInt16(0);
        writer.WriteUInt16(0);
        writer.WriteUInt16(0);
        // Duplicated discriminator USHORT for the [switch_is(vt)] non-encapsulated union
        // (DCE 1.1 §14.3.7.2 NDR rule: the discriminator is always written to the wire,
        // even when switch_is references a struct field already encoded).
        writer.WriteUInt16((ushort)vt);
        WriteVariantElementArm(ref writer, value);
    }

    private static OpcVariant ReadVariantElementBody(ref NdrReader reader)
    {
        reader.AlignTo(4);
        _ = reader.ReadUInt32();          // clSize
        // rpcReserved per MS-OAUT §2.2.29.2: SHOULD be 0 by the sender but
        // receivers MUST tolerate any value. Matrikon Simulation sends
        // non-zero rpcReserved bytes for embedded VARIANT element envelopes.
        _ = reader.ReadUInt32();
        ushort vtRaw = reader.ReadUInt16();
        _ = reader.ReadUInt16();
        _ = reader.ReadUInt16();
        _ = reader.ReadUInt16();
        ushort discRaw = reader.ReadUInt16();
        if (discRaw != vtRaw)
        {
            throw new InvalidDataException(
                $"NDR VARIANT element discriminator (0x{discRaw:X4}) does not match vt (0x{vtRaw:X4}) " +
                $"at buffer offset {reader.Position - 2}." + reader.FormatContext());
        }
        var vt = (VarType)vtRaw;
        return ReadVariantElementArm(ref reader, vt);
    }

    private static void WriteVariantElementArm(ref NdrWriter writer, OpcVariant value)
    {
        var vt = value.Type;
        if (VarTypeMask.IsArray(vt))
        {
            writer.AlignTo(8);
            WriteSafeArrayValue(ref writer, vt, value.Boxed);
            return;
        }
        if (WriteScalarVariantArm(ref writer, vt, value.Boxed)) { return; }
        throw new InvalidOperationException(
            $"NDR VARIANT element wire encoding is not yet supported for type {vt}.");
    }

    private static bool WriteScalarVariantArm(ref NdrWriter writer, VarType vt, object? boxed)
    {
        switch (vt)
        {
            case VarType.VT_EMPTY: case VarType.VT_NULL: return true;
            case VarType.VT_I1: writer.WriteByte(unchecked((byte)((sbyte)boxed!))); return true;
            case VarType.VT_UI1: writer.WriteByte((byte)boxed!); return true;
            case VarType.VT_I2: writer.WriteInt16((short)boxed!); return true;
            case VarType.VT_UI2: writer.WriteUInt16((ushort)boxed!); return true;
            case VarType.VT_BOOL: writer.WriteUInt16(((bool)boxed!) ? BoolTrueWire : BoolFalseWire); return true;
            case VarType.VT_I4: case VarType.VT_INT: case VarType.VT_ERROR: case VarType.VT_HRESULT:
                writer.AlignTo(4); writer.WriteInt32((int)boxed!); return true;
            case VarType.VT_UI4: case VarType.VT_UINT:
                writer.AlignTo(4); writer.WriteUInt32((uint)boxed!); return true;
            case VarType.VT_R4: writer.AlignTo(4); writer.WriteSingle((float)boxed!); return true;
            case VarType.VT_I8: writer.AlignTo(8); writer.WriteInt64((long)boxed!); return true;
            case VarType.VT_UI8: writer.AlignTo(8); writer.WriteUInt64((ulong)boxed!); return true;
            case VarType.VT_R8: writer.AlignTo(8); writer.WriteDouble((double)boxed!); return true;
            case VarType.VT_DATE: writer.AlignTo(8); writer.WriteDouble(((DateTime)boxed!).ToOADate()); return true;
            case VarType.VT_FILETIME: writer.AlignTo(8); writer.WriteFileTime((long)boxed!); return true;
            case VarType.VT_BSTR: writer.AlignTo(4); WriteElementBstrBody(ref writer, (string?)boxed); return true;
            default: return false;
        }
    }

    /// <summary>
    /// Writes a BSTR in the canonical FLAGGED_WORD_BLOB wire layout used
    /// inside VARIANT array elements (mirror of <c>ReadElementBstr</c>):
    /// referent + max_count + fFlags + clSize + USHORT[clSize]. Distinct
    /// from the legacy <c>NdrWriter.WriteBstr</c> which omits max_count
    /// for loopback-only compatibility.
    /// </summary>
    private static void WriteElementBstrBody(ref NdrWriter writer, string? text)
    {
        if (text is null)
        {
            writer.WriteNullReferent();
            return;
        }
        _ = writer.WriteReferentId();
        uint clSize = unchecked((uint)text.Length);
        writer.WriteUInt32(clSize);         // max_count (= clSize per spec)
        writer.WriteUInt32(0u);             // fFlags (informational; 0 for our emit, server may use 8)
        writer.WriteUInt32(clSize);         // clSize (char count, no nul)
        for (int i = 0; i < text.Length; i++)
        {
            writer.WriteUInt16((ushort)text[i]);
        }
    }

    private static OpcVariant ReadVariantElementArm(ref NdrReader reader, VarType vt)
    {
        if (VarTypeMask.IsArray(vt))
        {
            reader.AlignTo(8);
            return ReadSafeArrayVariant(ref reader, vt);
        }
        switch (vt)
        {
            case VarType.VT_EMPTY: return OpcVariant.Empty;
            case VarType.VT_NULL: return OpcVariant.Null;
            case VarType.VT_I1: return OpcVariant.FromInt8(unchecked((sbyte)reader.ReadByte()));
            case VarType.VT_UI1: return OpcVariant.FromUInt8(reader.ReadByte());
            case VarType.VT_I2: return OpcVariant.FromInt16(reader.ReadInt16());
            case VarType.VT_UI2: return OpcVariant.FromUInt16(reader.ReadUInt16());
            case VarType.VT_BOOL: return OpcVariant.FromBoolean(reader.ReadUInt16() != 0);
            case VarType.VT_I4: reader.AlignTo(4); return OpcVariant.FromInt32(reader.ReadInt32());
            case VarType.VT_UI4: reader.AlignTo(4); return OpcVariant.FromUInt32(reader.ReadUInt32());
            case VarType.VT_INT: reader.AlignTo(4); return OpcVariant.FromInt32(reader.ReadInt32());
            case VarType.VT_UINT: reader.AlignTo(4); return OpcVariant.FromUInt32(reader.ReadUInt32());
            case VarType.VT_ERROR: reader.AlignTo(4); return OpcVariant.FromError(reader.ReadInt32());
            case VarType.VT_HRESULT: reader.AlignTo(4); return new OpcVariant(VarType.VT_HRESULT, reader.ReadInt32());
            case VarType.VT_R4: reader.AlignTo(4); return OpcVariant.FromSingle(reader.ReadSingle());
            case VarType.VT_I8: reader.AlignTo(8); return OpcVariant.FromInt64(reader.ReadInt64());
            case VarType.VT_UI8: reader.AlignTo(8); return OpcVariant.FromUInt64(reader.ReadUInt64());
            case VarType.VT_R8: reader.AlignTo(8); return OpcVariant.FromDouble(reader.ReadDouble());
            case VarType.VT_DATE: reader.AlignTo(8); return OpcVariant.FromDate(DateTime.FromOADate(reader.ReadDouble()));
            case VarType.VT_FILETIME: reader.AlignTo(8); return OpcVariant.FromFileTime(reader.ReadFileTime());
            case VarType.VT_BSTR:
                reader.AlignTo(4);
                {
                    string? s = ReadElementBstr(ref reader);
                    return s is null ? new OpcVariant(VarType.VT_BSTR, null) : OpcVariant.FromString(s);
                }
            default:
                throw new InvalidDataException(
                    $"NDR VARIANT element wire decoding is not yet supported for type {vt}." + reader.FormatContext());
        }
    }

    /// <summary>
    /// Reads a BSTR as encoded inside a VARIANT element arm (MS-OAUT
    /// FLAGGED_WORD_BLOB). Layout: referent + max_count + fFlags + clSize +
    /// USHORT[clSize] chars. The fFlags field is informational (set to 8 by
    /// Matrikon, 0 by some other servers); it is read but not validated.
    /// </summary>
    private static string? ReadElementBstr(ref NdrReader reader)
    {
        if (!reader.TryReadReferentId(out _))
        {
            return null;
        }
        uint maxCount = reader.ReadUInt32();
        _ = reader.ReadUInt32();           // fFlags — informational, not validated
        uint clSize = reader.ReadUInt32();
        if (clSize != maxCount)
        {
            throw new InvalidDataException(
                $"NDR FLAGGED_WORD_BLOB max_count ({maxCount}) does not match clSize ({clSize})." + reader.FormatContext());
        }
        if (clSize == 0u)
        {
            return string.Empty;
        }
        var chars = new char[clSize];
        for (uint i = 0; i < clSize; i++)
        {
            chars[i] = (char)reader.ReadUInt16();
        }
        return new string(chars);
    }

    private delegate OpcVariant NdrReadFunc(ref NdrReader reader);

    private static void WriteVariantCore(ref NdrWriter writer, OpcVariant value, int depth)
    {
        ThrowIfDepthExceeded(depth);

        int bodyBytes = ComputeVariantBodySize(value, depth);
        WriteVariantHeader(ref writer, value.Type, bodyBytes);
        WriteVariantBody(ref writer, value, depth);
    }

    private static void WriteVariantHeader(ref NdrWriter writer, VarType vt, int bodyBytes)
    {
        writer.AlignTo(4);
        writer.WriteUInt32(unchecked((uint)(VariantHeaderBytes - 8 + bodyBytes)));
        writer.WriteUInt32(0u);
        writer.WriteUInt16((ushort)vt);
        writer.WriteUInt16(0);
        writer.WriteUInt16(0);
        writer.WriteUInt16(0);
    }

    private static void WriteVariantBody(ref NdrWriter writer, OpcVariant value, int depth)
    {
        if (VarTypeMask.IsByRef(value.Type))
        {
            WriteByRefBody(ref writer, value, depth);
            return;
        }

        if (VarTypeMask.IsArray(value.Type))
        {
            WriteSafeArrayValue(ref writer, value.Type, value.Boxed);
            return;
        }

        switch (value.Type)
        {
            case VarType.VT_BSTR:
                WriteBstrBody(ref writer, (string?)value.Boxed);
                return;
            case VarType.VT_VARIANT:
                if (value.Boxed is not OpcVariant nested)
                {
                    throw new InvalidOperationException("VT_VARIANT payload must be an OpcVariant.");
                }
                WriteVariantCore(ref writer, nested, depth + 1);
                return;
            case VarType.VT_RECORD:
                WriteRecordPayload(ref writer, value.Boxed as OpcRecordValue, depth);
                return;
            default:
                WriteScalarBody(ref writer, value.Type, value.Boxed);
                return;
        }
    }

    private static void WriteByRefBody(ref NdrWriter writer, OpcVariant value, int depth)
    {
        if (value.Boxed is null)
        {
            writer.WriteNullReferent();
            return;
        }

        _ = writer.WriteReferentId();
        VarType dereferencedType = RemoveByRef(value.Type);
        WriteDereferencedBody(ref writer, dereferencedType, value.Boxed, depth);
    }

    private static void WriteDereferencedBody(ref NdrWriter writer, VarType vt, object? boxed, int depth)
    {
        if (VarTypeMask.IsArray(vt))
        {
            WriteSafeArrayValue(ref writer, vt, boxed);
            return;
        }

        switch (vt)
        {
            case VarType.VT_BSTR:
                WriteBstrBody(ref writer, (string?)boxed);
                return;
            case VarType.VT_VARIANT:
                if (boxed is not OpcVariant nested)
                {
                    throw new InvalidOperationException("VT_VARIANT BYREF payload must be an OpcVariant.");
                }
                WriteVariantCore(ref writer, nested, depth + 1);
                return;
            case VarType.VT_RECORD:
                WriteRecordPayload(ref writer, boxed as OpcRecordValue, depth);
                return;
            default:
                WriteScalarBody(ref writer, vt, boxed);
                return;
        }
    }

    private static void WriteSafeArrayValue(ref NdrWriter writer, VarType vt, object? boxed)
    {
        if (boxed is not OpcSafeArray array)
        {
            throw new InvalidOperationException(
                $"NDR VARIANT SAFEARRAY encoding requires an {nameof(OpcSafeArray)} payload.");
        }

        var expectedType = (VarType)((ushort)array.ElementType | (ushort)VarType.VT_ARRAY);
        if (vt != expectedType)
        {
            throw new InvalidOperationException(
                $"NDR VARIANT SAFEARRAY type {vt} does not match element type {array.ElementType}.");
        }

        writer.WriteSafeArray(array);
    }

    private static void WriteBstrBody(ref NdrWriter writer, string? text)
    {
        if (text is null)
        {
            writer.WriteNullBstr();
        }
        else
        {
            writer.WriteBstr(text);
        }
    }

    private static void WriteScalarBody(ref NdrWriter writer, VarType vt, object? boxed)
    {
        switch (vt)
        {
            case VarType.VT_EMPTY:
            case VarType.VT_NULL:
                return;
            case VarType.VT_I1:
                writer.WriteByte(unchecked((byte)((sbyte)boxed!)));
                return;
            case VarType.VT_UI1:
                writer.WriteByte((byte)boxed!);
                return;
            case VarType.VT_I2:
                writer.WriteInt16((short)boxed!);
                return;
            case VarType.VT_UI2:
                writer.WriteUInt16((ushort)boxed!);
                return;
            case VarType.VT_BOOL:
                writer.WriteUInt16(((bool)boxed!) ? BoolTrueWire : BoolFalseWire);
                return;
            case VarType.VT_I4:
                writer.WriteInt32((int)boxed!);
                return;
            case VarType.VT_UI4:
                writer.WriteUInt32((uint)boxed!);
                return;
            case VarType.VT_R4:
                writer.WriteSingle((float)boxed!);
                return;
            case VarType.VT_ERROR:
                writer.WriteInt32((int)boxed!);
                return;
            case VarType.VT_I8:
                writer.WriteInt64((long)boxed!);
                return;
            case VarType.VT_UI8:
                writer.WriteUInt64((ulong)boxed!);
                return;
            case VarType.VT_R8:
                writer.WriteDouble((double)boxed!);
                return;
            case VarType.VT_DATE:
                writer.WriteDouble(((DateTime)boxed!).ToOADate());
                return;
            case VarType.VT_FILETIME:
                writer.WriteFileTime((long)boxed!);
                return;
            case VarType.VT_CLSID:
                writer.WriteGuid((Guid)boxed!);
                return;
            default:
                throw new InvalidOperationException(
                    $"NDR VARIANT wire encoding is not supported for type {vt}.");
        }
    }

    private static void WriteRecordPayload(ref NdrWriter writer, OpcRecordValue? record, int depth)
    {
        if (record is null)
        {
            writer.WriteNullReferent();
            writer.WriteNullReferent();
            return;
        }

        IRecordInfo info = GetRecordInfoForWrite(record.RecordInfoId);
        ValidateRecordValue(info, record);

        _ = writer.WriteReferentId();
        _ = writer.WriteReferentId();
        writer.WriteGuid(info.Id);
        for (int i = 0; i < info.Fields.Count; i++)
        {
            WriteRecordField(ref writer, info.Fields[i], record.Values[i], depth + 1);
        }
    }

    private static IRecordInfo GetRecordInfoForWrite(Guid id)
    {
        if (RecordInfoRegistry.TryGet(id, out IRecordInfo? info))
        {
            return info;
        }

        throw new InvalidOperationException(
            $"No VT_RECORD layout is registered for {id}. Register it with {nameof(RecordInfoRegistry)} before encoding.");
    }

    private static void ValidateRecordValue(IRecordInfo info, OpcRecordValue record)
    {
        if (record.Values.Count != info.Fields.Count)
        {
            throw new InvalidOperationException(
                $"VT_RECORD value has {record.Values.Count} fields but layout {info.Name} declares {info.Fields.Count}.");
        }
    }

    private static void WriteRecordField(ref NdrWriter writer, OpcRecordField field, object? value, int depth)
    {
        if (VarTypeMask.IsArray(field.Type) || VarTypeMask.IsByRef(field.Type))
        {
            throw new InvalidOperationException($"VT_RECORD field {field.Name} uses unsupported type {field.Type}.");
        }

        switch (field.Type)
        {
            case VarType.VT_BSTR:
                WriteBstrBody(ref writer, (string?)value);
                return;
            case VarType.VT_VARIANT:
                if (value is not OpcVariant nested)
                {
                    throw new InvalidOperationException($"VT_RECORD field {field.Name} must be an OpcVariant.");
                }
                WriteVariantCore(ref writer, nested, depth);
                return;
            case VarType.VT_RECORD:
                WriteRecordPayload(ref writer, value as OpcRecordValue, depth);
                return;
            default:
                WriteScalarBody(ref writer, field.Type, value);
                return;
        }
    }

    /// <summary>Decodes a <see cref="OpcVariant"/>.</summary>
    public static OpcVariant ReadVariant(this ref NdrReader reader) => ReadVariantCore(ref reader, depth: 0);

    private static OpcVariant ReadVariantCore(ref NdrReader reader, int depth)
    {
        ThrowIfDepthExceeded(depth);

        reader.AlignTo(4);
        _ = reader.ReadUInt32();
        // rpcReserved per MS-OAUT §2.2.29.2: SHOULD be 0 by the sender but receivers
        // MUST tolerate any value. Matrikon Simulation observed sending non-zero
        // rpcReserved (e.g. 2) for VARIANT fields embedded in OPCITEMSTATE results.
        _ = reader.ReadUInt32();
        ushort vtRaw = reader.ReadUInt16();
        _ = reader.ReadUInt16();
        _ = reader.ReadUInt16();
        _ = reader.ReadUInt16();

        var vt = (VarType)vtRaw;
        if (VarTypeMask.IsByRef(vt))
        {
            return ReadByRefVariant(ref reader, vt, depth);
        }
        if (VarTypeMask.IsArray(vt))
        {
            return ReadSafeArrayVariant(ref reader, vt);
        }

        return ReadBody(ref reader, vt, depth);
    }

    private static OpcVariant ReadByRefVariant(ref NdrReader reader, VarType vt, int depth)
    {
        if (!reader.TryReadReferentId(out _))
        {
            return new OpcVariant(vt, null);
        }

        VarType dereferencedType = RemoveByRef(vt);
        object? value = ReadDereferencedValue(ref reader, dereferencedType, depth);
        return new OpcVariant(vt, value);
    }

    private static object? ReadDereferencedValue(ref NdrReader reader, VarType vt, int depth)
    {
        if (VarTypeMask.IsArray(vt))
        {
            OpcSafeArray array = reader.ReadSafeArray();
            var expectedType = (VarType)((ushort)array.ElementType | (ushort)VarType.VT_ARRAY);
            if (vt != expectedType)
            {
                throw new InvalidDataException(
                    $"NDR VARIANT SAFEARRAY type {vt} does not match element type {array.ElementType}.");
            }
            return array;
        }

        return vt switch
        {
            VarType.VT_BSTR => reader.ReadBstr(),
            VarType.VT_VARIANT => ReadVariantCore(ref reader, depth + 1),
            VarType.VT_RECORD => ReadRecordPayload(ref reader, depth),
            _ => ReadScalarValue(ref reader, vt),
        };
    }

    private static OpcVariant ReadSafeArrayVariant(ref NdrReader reader, VarType vt)
    {
        OpcSafeArray array = reader.ReadSafeArray();
        var expectedType = (VarType)((ushort)array.ElementType | (ushort)VarType.VT_ARRAY);
        if (vt != expectedType)
        {
            throw new InvalidDataException(
                $"NDR VARIANT SAFEARRAY type {vt} does not match element type {array.ElementType}.");
        }

        return OpcVariant.FromSafeArray(array);
    }

    private static OpcVariant ReadBody(ref NdrReader reader, VarType vt, int depth) => vt switch
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
        VarType.VT_BSTR => new OpcVariant(VarType.VT_BSTR, reader.ReadBstr()),
        VarType.VT_VARIANT => OpcVariant.FromVariant(ReadVariantCore(ref reader, depth + 1)),
        VarType.VT_RECORD => new OpcVariant(VarType.VT_RECORD, ReadRecordPayload(ref reader, depth)),
        _ => throw new InvalidDataException(
            $"NDR VARIANT wire decoding is not supported for type {vt}." + reader.FormatContext()),
    };

    private static object? ReadScalarValue(ref NdrReader reader, VarType vt) => vt switch
    {
        VarType.VT_EMPTY or VarType.VT_NULL => null,
        VarType.VT_I1 => unchecked((sbyte)reader.ReadByte()),
        VarType.VT_UI1 => reader.ReadByte(),
        VarType.VT_I2 => reader.ReadInt16(),
        VarType.VT_UI2 => reader.ReadUInt16(),
        VarType.VT_BOOL => reader.ReadUInt16() != 0,
        VarType.VT_I4 => reader.ReadInt32(),
        VarType.VT_UI4 => reader.ReadUInt32(),
        VarType.VT_R4 => reader.ReadSingle(),
        VarType.VT_ERROR => reader.ReadInt32(),
        VarType.VT_I8 => reader.ReadInt64(),
        VarType.VT_UI8 => reader.ReadUInt64(),
        VarType.VT_R8 => reader.ReadDouble(),
        VarType.VT_DATE => DateTime.FromOADate(reader.ReadDouble()),
        VarType.VT_FILETIME => reader.ReadFileTime(),
        VarType.VT_CLSID => reader.ReadGuid(),
        _ => throw new InvalidDataException(
            $"NDR VARIANT wire decoding is not supported for type {vt}." + reader.FormatContext()),
    };

    private static OpcRecordValue? ReadRecordPayload(ref NdrReader reader, int depth)
    {
        bool hasRecord = reader.TryReadReferentId(out _);
        bool hasRecordInfo = reader.TryReadReferentId(out _);
        if (!hasRecord && !hasRecordInfo)
        {
            return null;
        }
        if (!hasRecord || !hasRecordInfo)
        {
            throw new InvalidDataException("VT_RECORD payload must carry both pvRecord and pRecInfo referents.");
        }

        Guid recordInfoId = reader.ReadGuid();
        if (!RecordInfoRegistry.TryGet(recordInfoId, out IRecordInfo? info))
        {
            throw new InvalidDataException(
                $"No VT_RECORD layout is registered for {recordInfoId}. Register it before decoding.");
        }

        var values = new object?[info.Fields.Count];
        for (int i = 0; i < info.Fields.Count; i++)
        {
            values[i] = ReadRecordField(ref reader, info.Fields[i], depth + 1);
        }

        return new OpcRecordValue(recordInfoId, values);
    }

    private static object? ReadRecordField(ref NdrReader reader, OpcRecordField field, int depth)
    {
        if (VarTypeMask.IsArray(field.Type) || VarTypeMask.IsByRef(field.Type))
        {
            throw new InvalidDataException($"VT_RECORD field {field.Name} uses unsupported type {field.Type}.");
        }

        return field.Type switch
        {
            VarType.VT_BSTR => reader.ReadBstr(),
            VarType.VT_VARIANT => ReadVariantCore(ref reader, depth),
            VarType.VT_RECORD => ReadRecordPayload(ref reader, depth),
            _ => ReadScalarValue(ref reader, field.Type),
        };
    }

    private static int ComputeVariantBodySize(OpcVariant value, int depth)
    {
        if (VarTypeMask.IsByRef(value.Type))
        {
            if (value.Boxed is null)
            {
                return 4;
            }
            return 4 + ComputeDereferencedBodySize(RemoveByRef(value.Type), value.Boxed, depth);
        }

        if (VarTypeMask.IsArray(value.Type))
        {
            if (value.Boxed is not OpcSafeArray array)
            {
                throw new InvalidOperationException(
                    $"NDR VARIANT SAFEARRAY encoding requires an {nameof(OpcSafeArray)} payload.");
            }
            return ComputeSafeArrayBodySize(array, depth);
        }

        return value.Type switch
        {
            VarType.VT_BSTR => ComputeBstrBodySize((string?)value.Boxed),
            VarType.VT_VARIANT => value.Boxed is OpcVariant nested
                ? ComputeVariantTotalSize(nested, depth + 1)
                : throw new InvalidOperationException("VT_VARIANT payload must be an OpcVariant."),
            VarType.VT_RECORD => ComputeRecordBodySize(value.Boxed as OpcRecordValue, depth),
            _ => ComputeScalarBodySize(value.Type),
        };
    }

    private static int ComputeDereferencedBodySize(VarType vt, object? boxed, int depth)
    {
        if (VarTypeMask.IsArray(vt))
        {
            return boxed is OpcSafeArray array
                ? ComputeSafeArrayBodySize(array, depth)
                : throw new InvalidOperationException(
                    $"NDR VARIANT SAFEARRAY encoding requires an {nameof(OpcSafeArray)} payload.");
        }

        return vt switch
        {
            VarType.VT_BSTR => ComputeBstrBodySize((string?)boxed),
            VarType.VT_VARIANT => boxed is OpcVariant nested
                ? ComputeVariantTotalSize(nested, depth + 1)
                : throw new InvalidOperationException("VT_VARIANT BYREF payload must be an OpcVariant."),
            VarType.VT_RECORD => ComputeRecordBodySize(boxed as OpcRecordValue, depth),
            _ => ComputeScalarBodySize(vt),
        };
    }

    private static int ComputeVariantTotalSize(OpcVariant value, int depth)
    {
        ThrowIfDepthExceeded(depth);
        return VariantHeaderBytes + ComputeVariantBodySize(value, depth);
    }

    private static int ComputeSafeArrayBodySize(OpcSafeArray array, int depth)
    {
        int position = 20 + checked(8 * array.Rank);
        int count = array.TotalElements;
        if (count == 0)
        {
            return position;
        }

        return array.ElementType switch
        {
            VarType.VT_I1 or VarType.VT_UI1 => position + count,
            VarType.VT_I2 or VarType.VT_UI2 or VarType.VT_BOOL => FixedArrayBodySize(position, count, 2, 2),
            VarType.VT_I4 or VarType.VT_UI4 or VarType.VT_R4 or VarType.VT_ERROR => FixedArrayBodySize(position, count, 4, 4),
            VarType.VT_I8 or VarType.VT_UI8 or VarType.VT_R8 or VarType.VT_DATE => FixedArrayBodySize(position, count, 8, 8),
            VarType.VT_CLSID => FixedArrayBodySize(position, count, 4, 16),
            VarType.VT_BSTR => ComputeBstrSafeArrayBodySize(position, (string?[])array.Data),
            VarType.VT_VARIANT => ComputeVariantSafeArrayBodySize(position, (OpcVariant[])array.Data, depth),
            VarType.VT_RECORD => ComputeRecordSafeArrayBodySize(position, (OpcRecordValue?[])array.Data, depth),
            _ => throw new InvalidOperationException(
                $"NDR SAFEARRAY codec does not support element type {array.ElementType}."),
        };
    }

    private static int FixedArrayBodySize(int position, int count, int alignment, int elementBytes) =>
        checked(AlignOffset(position, alignment) + count * elementBytes);

    private static int ComputeBstrBodySize(string? value) => value is null ? 4 : checked(12 + value.Length * 2);

    private static int ComputeBstrSafeArrayBodySize(int position, string?[] values)
    {
        for (int i = 0; i < values.Length; i++)
        {
            position = checked(AlignOffset(position, 4) + ComputeBstrBodySize(values[i]));
        }
        return position;
    }

    private static int ComputeVariantSafeArrayBodySize(int position, OpcVariant[] values, int depth)
    {
        for (int i = 0; i < values.Length; i++)
        {
            position = checked(AlignOffset(position, 4) + ComputeVariantTotalSize(values[i], depth + 1));
        }
        return position;
    }

    private static int ComputeRecordSafeArrayBodySize(int position, OpcRecordValue?[] values, int depth)
    {
        for (int i = 0; i < values.Length; i++)
        {
            var recordVariant = new OpcVariant(VarType.VT_RECORD, values[i]);
            position = checked(AlignOffset(position, 4) + ComputeVariantTotalSize(recordVariant, depth + 1));
        }
        return position;
    }

    private static int ComputeRecordBodySize(OpcRecordValue? record, int depth)
    {
        if (record is null)
        {
            return 8;
        }

        IRecordInfo info = GetRecordInfoForWrite(record.RecordInfoId);
        ValidateRecordValue(info, record);

        int position = 24;
        for (int i = 0; i < info.Fields.Count; i++)
        {
            position = ComputeRecordFieldSize(position, info.Fields[i], record.Values[i], depth + 1);
        }
        return position;
    }

    private static int ComputeRecordFieldSize(int position, OpcRecordField field, object? value, int depth)
    {
        return field.Type switch
        {
            VarType.VT_BSTR => checked(AlignOffset(position, 4) + ComputeBstrBodySize((string?)value)),
            VarType.VT_VARIANT => value is OpcVariant nested
                ? checked(AlignOffset(position, 4) + ComputeVariantTotalSize(nested, depth))
                : throw new InvalidOperationException($"VT_RECORD field {field.Name} must be an OpcVariant."),
            VarType.VT_RECORD => checked(AlignOffset(position, 4) + ComputeRecordBodySize(value as OpcRecordValue, depth)),
            VarType.VT_I1 or VarType.VT_UI1 => position + 1,
            VarType.VT_I2 or VarType.VT_UI2 or VarType.VT_BOOL => FixedArrayBodySize(position, 1, 2, 2),
            VarType.VT_I4 or VarType.VT_UI4 or VarType.VT_R4 or VarType.VT_ERROR => FixedArrayBodySize(position, 1, 4, 4),
            VarType.VT_I8 or VarType.VT_UI8 or VarType.VT_R8 or VarType.VT_DATE => FixedArrayBodySize(position, 1, 8, 8),
            VarType.VT_FILETIME or VarType.VT_CLSID => FixedArrayBodySize(position, 1, 4, field.Type == VarType.VT_CLSID ? 16 : 8),
            VarType.VT_EMPTY or VarType.VT_NULL => position,
            _ => throw new InvalidOperationException($"VT_RECORD field {field.Name} uses unsupported type {field.Type}."),
        };
    }

    private static int ComputeScalarBodySize(VarType vt) => vt switch
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

    private static int AlignOffset(int position, int boundary)
    {
        int misaligned = position & (boundary - 1);
        return misaligned == 0 ? position : position + boundary - misaligned;
    }

    private static VarType RemoveByRef(VarType vt) => (VarType)((ushort)vt & ~(ushort)VarType.VT_BYREF);

    private static void ThrowIfDepthExceeded(int depth)
    {
        if (depth > MaxVariantRecursionDepth)
        {
            throw new InvalidDataException(
                $"NDR VARIANT nesting exceeds the supported depth of {MaxVariantRecursionDepth}.");
        }
    }
}
