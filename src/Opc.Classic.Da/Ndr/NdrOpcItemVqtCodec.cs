// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.
// NDR wire codec for OPC DA's OPCITEMVQT struct.
//

using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC DA <c>OPCITEMVQT</c> struct,
/// matching <c>tagOPCITEMVQT</c> in opcda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     VARIANT  vDataValue
///     INT32    bQualitySpecified    (-1 or 0 — Win32 BOOL)
///     UINT16   wQuality
///     UINT16   wReserved            (0)
///     INT32    bTimeStampSpecified
///     UINT32   dwReserved           (0)
///     UINT32   filetime.dwLowDateTime
///     UINT32   filetime.dwHighDateTime
/// </code>
/// The FILETIME alignment is 4 (NDR rule for the {DWORD, DWORD} pair),
/// not 8, even though the value is a 64-bit count of 100-ns intervals.
/// </remarks>
public static class NdrOpcItemVqtCodec
{
    private const int Win32BoolTrue = unchecked((int)0xFFFFFFFFu); // -1 / TRUE
    private const int Win32BoolFalse = 0;
    private const long FileTimeEpochOffsetTicks = 504911232000000000L; // 1601-01-01 UTC in DateTimeOffset.Ticks

    /// <summary>
    /// Encodes a single OPCITEMVQT in NDR.
    /// </summary>
    public static void Write(ref NdrWriter writer, OpcItemVqt vqt)
    {
        ArgumentNullException.ThrowIfNull(vqt);

        writer.WriteVariant(vqt.Value);

        bool qSpec = vqt.Quality.HasValue;
        writer.WriteInt32(qSpec ? Win32BoolTrue : Win32BoolFalse);
        writer.WriteUInt16(qSpec ? unchecked((ushort)(vqt.Quality!.Value.RawValue & 0xFFFF)) : (ushort)0);
        writer.WriteUInt16(0);   // wReserved

        bool tsSpec = vqt.Timestamp.HasValue;
        writer.WriteInt32(tsSpec ? Win32BoolTrue : Win32BoolFalse);
        writer.WriteUInt32(0u);  // dwReserved

        long fileTimeTicks = tsSpec ? ToFileTime(vqt.Timestamp!.Value) : 0L;
        writer.WriteFileTime(fileTimeTicks);
    }

    /// <summary>
    /// Decodes a single OPCITEMVQT from NDR.
    /// </summary>
    public static OpcItemVqt Read(ref NdrReader reader)
    {
        OpcVariant value = reader.ReadVariant();

        int bQuality = reader.ReadInt32();
        ushort wQuality = reader.ReadUInt16();
        _ = reader.ReadUInt16();   // wReserved

        int bTimestamp = reader.ReadInt32();
        _ = reader.ReadUInt32();    // dwReserved
        long fileTimeTicks = reader.ReadFileTime();

        OpcQuality? quality = bQuality != 0 ? new OpcQuality(wQuality) : null;
        DateTimeOffset? timestamp;
        if (bTimestamp != 0)
        {
            if (!FileTimeHelper.TryFromFileTime(fileTimeTicks, out DateTimeOffset decoded))
            {
                throw new InvalidDataException(
                    $"OPCITEMVQT.ftTimeStamp FILETIME value 0x{fileTimeTicks:X16} ({fileTimeTicks}) cannot be expressed as a DateTimeOffset (out of range 1601-01-01..9999-12-31)." + reader.FormatContext());
            }
            timestamp = decoded;
        }
        else
        {
            timestamp = null;
        }

        return new OpcItemVqt(value, quality, timestamp);
    }

    private static long ToFileTime(DateTimeOffset value)
    {
        // FILETIME is 100-ns intervals since 1601-01-01 UTC.
        long utcTicks = value.UtcTicks;
        return utcTicks - FileTimeEpochOffsetTicks;
    }
}
