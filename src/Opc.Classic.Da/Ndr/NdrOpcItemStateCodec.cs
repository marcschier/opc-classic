//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC DA <c>OPCITEMSTATE</c> struct,
/// matching <c>tagOPCITEMSTATE</c> in opcda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     UINT32   hClient
///     UINT32   filetime.dwLowDateTime
///     UINT32   filetime.dwHighDateTime
///     UINT16   wQuality
///     UINT16   wReserved (0)
///     VARIANT  vDataValue (16-byte header + body)
/// </code>
/// </remarks>
public static class NdrOpcItemStateCodec
{
    private const long FileTimeEpochOffsetTicks = 504911232000000000L; // 1601-01-01 UTC in .NET ticks

    /// <summary>Encodes a single OPCITEMSTATE in NDR.</summary>
    public static void Write(ref NdrWriter writer, OpcItemState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        writer.WriteUInt32(unchecked((uint)state.ClientHandle));
        writer.WriteFileTime(ToFileTime(state.Timestamp));
        writer.WriteUInt16(unchecked((ushort)(state.Quality.RawValue & 0xFFFF)));
        writer.WriteUInt16(0);   // wReserved
        writer.WriteVariant(state.Value);
    }

    /// <summary>Decodes a single OPCITEMSTATE from NDR.</summary>
    public static OpcItemState Read(ref NdrReader reader)
    {
        uint hClient = reader.ReadUInt32();
        DateTimeOffset timestamp = ReadAndDecodeFileTime(ref reader, "ftTimeStamp");
        ushort wQuality = reader.ReadUInt16();
        _ = reader.ReadUInt16();  // wReserved
        OpcVariant value = reader.ReadVariant();

        return new OpcItemState(
            ClientHandle: unchecked((int)hClient),
            Timestamp: timestamp,
            Quality: new OpcQuality(wQuality),
            Value: value);
    }

    private static long ToFileTime(DateTimeOffset value) =>
        value.UtcTicks - FileTimeEpochOffsetTicks;

    private static DateTimeOffset ReadAndDecodeFileTime(ref NdrReader reader, string fieldName)
    {
        long raw = reader.ReadFileTime();
        if (FileTimeHelper.TryFromFileTime(raw, out DateTimeOffset value))
        {
            return value;
        }
        throw new InvalidDataException(
            $"OPCITEMSTATE.{fieldName} FILETIME value 0x{raw:X16} ({raw}) cannot be expressed as a DateTimeOffset (out of range 1601-01-01..9999-12-31)." + reader.FormatContext());
    }
}
