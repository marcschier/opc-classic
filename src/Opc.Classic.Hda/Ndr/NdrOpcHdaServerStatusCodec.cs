//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO;
using Opc.Classic.Ndr;

namespace Opc.Classic.Hda.Ndr;

/// <summary>NDR encoder / decoder for the OPC HDA historian status response.</summary>
public static class NdrOpcHdaServerStatusCodec
{
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    /// <summary>Encodes an HDA historian status structure.</summary>
    public static void Write(ref NdrWriter writer, OpcServerStatus status)
    {
        ArgumentNullException.ThrowIfNull(status);

        writer.WriteUInt32(ToHistorianStatus(status.State));
        // [out] FILETIME **pftCurrentTime / pftStartTime per IDL — the OS
        // COM proxy/stub (opchda_ps.dll) emits a 4-byte unique-pointer
        // referent before each FILETIME on the wire (MS-RPCE §14.3.10).
        // Without these referents the client-side decoder reads the
        // FILETIME bytes as the referent and the trailing fields slide.
        _ = writer.WriteReferentId();
        writer.WriteFileTime(ToFileTime(status.CurrentTime));
        _ = writer.WriteReferentId();
        writer.WriteFileTime(ToFileTime(status.StartTime));
        writer.WriteUInt16(checked((ushort)status.ServerVersion.Major));
        writer.WriteUInt16(checked((ushort)status.ServerVersion.Minor));
        writer.WriteUInt16(checked((ushort)Math.Max(0, status.ServerVersion.Build)));
        writer.WriteUInt16(0);
        writer.WriteUInt32(checked((uint)Math.Max(0, status.MaxReturnValues)));
        writer.WriteUnicodeStringPtr(status.State.ToString());
        writer.WriteUnicodeStringPtr(status.VendorInfo);
    }

    /// <summary>Decodes an HDA historian status structure.</summary>
    public static OpcServerStatus Read(ref NdrReader reader)
    {
        OpcServerState state = FromHistorianStatus(reader.ReadUInt32());
        // [out] FILETIME **pftCurrentTime / pftStartTime: skip the outer
        // unique-pointer referent that the OS COM proxy/stub emits before
        // each FILETIME (see Write for the symmetric encoding rationale).
        _ = reader.TryReadReferentId(out _);
        DateTimeOffset current = ReadAndDecodeFileTime(ref reader, "ftCurrentTime");
        _ = reader.TryReadReferentId(out _);
        DateTimeOffset start = ReadAndDecodeFileTime(ref reader, "ftStartTime");
        ushort major = reader.ReadUInt16();
        ushort minor = reader.ReadUInt16();
        ushort build = reader.ReadUInt16();
        _ = reader.ReadUInt16();
        int maxReturnValues = checked((int)reader.ReadUInt32());
        _ = reader.ReadUnicodeStringPtr();
        string vendorInfo = reader.ReadUnicodeStringPtr() ?? string.Empty;

        return new OpcServerStatus
        {
            Spec = OpcStatusSpec.Hda,
            StartTime = start,
            CurrentTime = current,
            State = state,
            ServerVersion = new Version(major, minor, build),
            MaxReturnValues = maxReturnValues,
            VendorInfo = vendorInfo,
        };
    }

    private static uint ToHistorianStatus(OpcServerState state) => state switch
    {
        OpcServerState.Running => 1u,
        OpcServerState.Failed or OpcServerState.CommFault => 2u,
        _ => 3u,
    };

    private static OpcServerState FromHistorianStatus(uint value) => value switch
    {
        1u => OpcServerState.Running,
        2u => OpcServerState.Failed,
        _ => OpcServerState.NoConfig,
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
            $"OPCHDA_SERVERSTATUS.{fieldName} FILETIME value 0x{raw:X16} ({raw}) cannot be expressed as a DateTimeOffset (out of range 1601-01-01..9999-12-31)." + reader.FormatContext());
    }
}
