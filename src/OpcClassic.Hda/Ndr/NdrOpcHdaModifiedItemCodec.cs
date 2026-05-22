//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.IO;
using OpcClassic.Ndr;

namespace OpcClassic.Hda.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC HDA <c>OPCHDA_MODIFIEDITEM</c> struct,
/// matching <c>tagOPCHDA_MODIFIEDITEM</c> in opchda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     UINT32   hClient
///     UINT32   dwCount
///     FILETIME[dwCount] pftTimeStamps       - conformant array
///     UINT32[dwCount]   pdwQualities        - conformant array
///     VARIANT[dwCount]  pvDataValues        - conformant array
///     FILETIME[dwCount] pftModificationTime - conformant array
///     UINT32[dwCount]   pEditType           - conformant array
///     LPWSTR[dwCount]   szUser              - conformant array; LPWSTR bodies interleaved per element
/// </code>
/// </remarks>
public static class NdrOpcHdaModifiedItemCodec
{
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    /// <summary>Encodes a single OPCHDA_MODIFIEDITEM in NDR.</summary>
    public static void Write(ref NdrWriter writer, OpcHdaModifiedItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        int count = item.Timestamps.Length;
        writer.WriteUInt32(unchecked((uint)item.ClientHandle));
        writer.WriteUInt32(unchecked((uint)count));
        WriteFileTimeArray(ref writer, item.Timestamps);
        writer.WriteConformantUInt32Array(item.Qualities);
        WriteVariantArray(ref writer, item.Values);
        WriteFileTimeArray(ref writer, item.ModificationTimes);
        writer.WriteConformantUInt32Array(item.EditTypes);
        WriteUserArray(ref writer, item.Users);
    }

    /// <summary>Decodes a single OPCHDA_MODIFIEDITEM from NDR.</summary>
    public static OpcHdaModifiedItem Read(ref NdrReader reader)
    {
        uint hClient = reader.ReadUInt32();
        int count = ReadCount(ref reader);
        var timestamps = ReadFileTimeArray(ref reader, count);
        uint[] qualities = reader.ReadConformantUInt32Array();
        var values = ReadVariantArray(ref reader, count);
        var modificationTimes = ReadFileTimeArray(ref reader, count);
        uint[] editTypes = reader.ReadConformantUInt32Array();
        string?[] users = ReadUserArray(ref reader, count);

        return new OpcHdaModifiedItem(
            clientHandle: unchecked((int)hClient),
            timestamps: timestamps,
            qualities: qualities,
            values: values,
            modificationTimes: modificationTimes,
            editTypes: editTypes,
            users: users);
    }

    private static int ReadCount(ref NdrReader reader)
    {
        uint dwCount = reader.ReadUInt32();
        if (dwCount > (uint)int.MaxValue)
        {
            throw new InvalidDataException($"OPCHDA_MODIFIEDITEM dwCount {dwCount} too large.");
        }
        return (int)dwCount;
    }

    private static void WriteFileTimeArray(ref NdrWriter writer, ReadOnlySpan<DateTimeOffset> timestamps)
    {
        writer.WriteConformanceHeader(timestamps.Length);
        for (int i = 0; i < timestamps.Length; i++)
        {
            writer.WriteFileTime(timestamps[i].UtcTicks - FileTimeEpochOffsetTicks);
        }
    }

    private static DateTimeOffset[] ReadFileTimeArray(ref NdrReader reader, int count)
    {
        _ = reader.ReadConformanceHeader();
        var timestamps = new DateTimeOffset[count];
        for (int i = 0; i < count; i++)
        {
            long fileTime = reader.ReadFileTime();
            timestamps[i] = new DateTimeOffset(fileTime + FileTimeEpochOffsetTicks, TimeSpan.Zero);
        }
        return timestamps;
    }

    private static void WriteVariantArray(ref NdrWriter writer, ReadOnlySpan<OpcVariant> values)
    {
        writer.WriteConformanceHeader(values.Length);
        for (int i = 0; i < values.Length; i++)
        {
            writer.WriteVariant(values[i]);
        }
    }

    private static OpcVariant[] ReadVariantArray(ref NdrReader reader, int count)
    {
        _ = reader.ReadConformanceHeader();
        var values = new OpcVariant[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = reader.ReadVariant();
        }
        return values;
    }

    private static void WriteUserArray(ref NdrWriter writer, ReadOnlySpan<string?> users)
    {
        writer.WriteConformanceHeader(users.Length);
        for (int i = 0; i < users.Length; i++)
        {
            writer.WriteUnicodeStringPtr(users[i]);
        }
    }

    private static string?[] ReadUserArray(ref NdrReader reader, int count)
    {
        _ = reader.ReadConformanceHeader();
        var users = new string?[count];
        for (int i = 0; i < count; i++)
        {
            users[i] = reader.ReadUnicodeStringPtr();
        }
        return users;
    }
}
