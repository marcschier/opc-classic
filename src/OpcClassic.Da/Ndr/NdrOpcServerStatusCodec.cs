//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using OpcClassic.Ndr;

namespace OpcClassic.Da.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC DA <c>OPCSERVERSTATUS</c> struct,
/// matching <c>tagOPCSERVERSTATUS</c> in opcda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     FILETIME ftStartTime        // 2x UInt32, alignment 4
///     FILETIME ftCurrentTime
///     FILETIME ftLastUpdateTime
///     UINT32   dwServerState      // OPCSERVERSTATE enum
///     UINT32   dwGroupCount
///     UINT32   dwBandWidth
///     UINT16   wMajorVersion
///     UINT16   wMinorVersion
///     UINT16   wBuildNumber
///     UINT16   wReserved (0)
///     LPWSTR   szVendorInfo       // unique-pointer LPWSTR
/// </code>
/// </remarks>
public static class NdrOpcServerStatusCodec
{
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    /// <summary>Encodes an OPCSERVERSTATUS in NDR (DA variant — assumes Spec == DA).</summary>
    public static void Write(ref NdrWriter writer, OpcServerStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);

        writer.WriteFileTime(ToFileTime(status.StartTime));
        writer.WriteFileTime(ToFileTime(status.CurrentTime));
        writer.WriteFileTime(ToFileTime(status.LastUpdateTime));
        writer.WriteUInt32((uint)status.State);
        writer.WriteUInt32(unchecked((uint)status.GroupCount));
        writer.WriteUInt32(status.BandWidth);
        writer.WriteUInt16(checked((ushort)status.ServerVersion.Major));
        writer.WriteUInt16(checked((ushort)status.ServerVersion.Minor));
        writer.WriteUInt16(checked((ushort)Math.Max(0, status.ServerVersion.Build)));
        writer.WriteUInt16(0);   // wReserved
        writer.WriteUnicodeStringPtr(status.VendorInfo);
    }

    /// <summary>Decodes an OPCSERVERSTATUS from NDR (DA variant).</summary>
    public static OpcServerStatus Read(ref NdrReader reader)
    {
        DateTimeOffset start = FromFileTime(reader.ReadFileTime());
        DateTimeOffset current = FromFileTime(reader.ReadFileTime());
        DateTimeOffset lastUpdate = FromFileTime(reader.ReadFileTime());
        var state = (OpcServerState)reader.ReadUInt32();
        int groupCount = unchecked((int)reader.ReadUInt32());
        uint bandWidth = reader.ReadUInt32();
        ushort major = reader.ReadUInt16();
        ushort minor = reader.ReadUInt16();
        ushort build = reader.ReadUInt16();
        _ = reader.ReadUInt16();   // wReserved
        string vendorInfo = reader.ReadUnicodeStringPtr() ?? string.Empty;

        return new OpcServerStatus
        {
            Spec = OpcStatusSpec.Da,
            StartTime = start,
            CurrentTime = current,
            LastUpdateTime = lastUpdate,
            State = state,
            ServerVersion = new Version(major, minor, build),
            GroupCount = groupCount,
            BandWidth = bandWidth,
            VendorInfo = vendorInfo,
        };
    }

    private static long ToFileTime(DateTimeOffset value) =>
        value.UtcTicks - FileTimeEpochOffsetTicks;

    private static DateTimeOffset FromFileTime(long fileTimeTicks) =>
        new(fileTimeTicks + FileTimeEpochOffsetTicks, TimeSpan.Zero);
}
