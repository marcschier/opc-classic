//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO;
using Opc.Classic.Ndr;

namespace Opc.Classic.Ae.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC AE <c>OPCEVENTSERVERSTATUS</c> struct,
/// matching <c>tagOPCEVENTSERVERSTATUS</c> in opc_ae.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     FILETIME ftStartTime        // 2x UInt32, alignment 4
///     FILETIME ftCurrentTime
///     FILETIME ftLastUpdateTime
///     UINT32   dwServerState      // OPCEVENTSERVERSTATE enum
///     UINT16   wMajorVersion
///     UINT16   wMinorVersion
///     UINT16   wBuildNumber
///     UINT16   wReserved (0)
///     LPWSTR   szVendorInfo       // unique-pointer LPWSTR
/// </code>
/// </remarks>
public static class NdrOpcEventServerStatusCodec
{
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    /// <summary>Encodes an OPCEVENTSERVERSTATUS in NDR (AE variant — assumes Spec == AE).</summary>
    public static void Write(ref NdrWriter writer, OpcServerStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);

        writer.WriteFileTime(ToFileTime(status.StartTime));
        writer.WriteFileTime(ToFileTime(status.CurrentTime));
        writer.WriteFileTime(ToFileTime(status.LastUpdateTime));
        writer.WriteUInt32(ToEventServerState(status.State));
        writer.WriteUInt16(checked((ushort)status.ServerVersion.Major));
        writer.WriteUInt16(checked((ushort)status.ServerVersion.Minor));
        writer.WriteUInt16(checked((ushort)Math.Max(0, status.ServerVersion.Build)));
        writer.WriteUInt16(0);   // wReserved
        writer.WriteUnicodeStringPtr(status.VendorInfo);
    }

    /// <summary>Decodes an OPCEVENTSERVERSTATUS from NDR (AE variant).</summary>
    public static OpcServerStatus Read(ref NdrReader reader)
    {
        DateTimeOffset start = ReadAndDecodeFileTime(ref reader, "ftStartTime");
        DateTimeOffset current = ReadAndDecodeFileTime(ref reader, "ftCurrentTime");
        DateTimeOffset lastUpdate = ReadAndDecodeFileTime(ref reader, "ftLastUpdateTime");
        OpcServerState state = FromEventServerState(reader.ReadUInt32());
        ushort major = reader.ReadUInt16();
        ushort minor = reader.ReadUInt16();
        ushort build = reader.ReadUInt16();
        _ = reader.ReadUInt16();   // wReserved
        string vendorInfo = reader.ReadUnicodeStringPtr() ?? string.Empty;

        return new OpcServerStatus
        {
            Spec = OpcStatusSpec.Ae,
            StartTime = start,
            CurrentTime = current,
            LastUpdateTime = lastUpdate,
            State = state,
            ServerVersion = new Version(major, minor, build),
            VendorInfo = vendorInfo,
        };
    }

    private static uint ToEventServerState(OpcServerState state) => state switch
    {
        OpcServerState.Running => 1u,
        OpcServerState.Failed => 2u,
        OpcServerState.NoConfig => 3u,
        OpcServerState.Suspended => 4u,
        OpcServerState.Test => 5u,
        OpcServerState.CommFault => 6u,
        _ => unchecked((uint)state),
    };

    private static OpcServerState FromEventServerState(uint value) => value switch
    {
        1u => OpcServerState.Running,
        2u => OpcServerState.Failed,
        3u => OpcServerState.NoConfig,
        4u => OpcServerState.Suspended,
        5u => OpcServerState.Test,
        6u => OpcServerState.CommFault,
        _ => (OpcServerState)value,
    };

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
            $"OPCEVENTSERVERSTATUS.{fieldName} FILETIME value 0x{raw:X16} ({raw}) cannot be expressed as a DateTimeOffset (out of range 1601-01-01..9999-12-31)." + reader.FormatContext());
    }
}
