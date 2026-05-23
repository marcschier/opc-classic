//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
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
        long fileTimeTicks = reader.ReadFileTime();
        ushort wQuality = reader.ReadUInt16();
        _ = reader.ReadUInt16();  // wReserved
        OpcVariant value = reader.ReadVariant();

        return new OpcItemState(
            ClientHandle: unchecked((int)hClient),
            Timestamp: FromFileTime(fileTimeTicks),
            Quality: new OpcQuality(wQuality),
            Value: value);
    }

    private static long ToFileTime(DateTimeOffset value) =>
        value.UtcTicks - FileTimeEpochOffsetTicks;

    private static DateTimeOffset FromFileTime(long fileTimeTicks) =>
        new(fileTimeTicks + FileTimeEpochOffsetTicks, TimeSpan.Zero);
}
