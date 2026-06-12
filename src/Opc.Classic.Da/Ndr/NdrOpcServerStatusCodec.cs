//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Ndr;

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
        DateTimeOffset start = ReadAndDecodeFileTime(ref reader, "ftStartTime");
        DateTimeOffset current = ReadAndDecodeFileTime(ref reader, "ftCurrentTime");
        DateTimeOffset lastUpdate = ReadAndDecodeFileTime(ref reader, "ftLastUpdateTime");
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

    private static DateTimeOffset ReadAndDecodeFileTime(ref NdrReader reader, string fieldName)
    {
        long raw = reader.ReadFileTime();
        if (FileTimeHelper.TryFromFileTime(raw, out DateTimeOffset value))
        {
            return value;
        }

        throw new InvalidDataException(
            $"OPCSERVERSTATUS.{fieldName} FILETIME value 0x{raw:X16} ({raw}) cannot be expressed as a DateTimeOffset (out of range 1601-01-01..9999-12-31)." + reader.FormatContext());
    }
}
