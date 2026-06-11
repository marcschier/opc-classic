//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Ndr;

namespace Opc.Classic.Hda.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC HDA <c>OPCHDA_ITEM</c> struct,
/// matching <c>tagOPCHDA_ITEM</c> in opchda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     UINT32    hClient
///     UINT32    haAggregate
///     UINT32    dwCount
///     FILETIME[dwCount]  pftTimeStamps     - conformant array; emit count then loop
///     UINT32[dwCount]    pdwQualities      - conformant array
///     VARIANT[dwCount]   pvDataValues      - conformant array; emit count then loop
/// </code>
/// </remarks>
public static class NdrOpcHdaItemCodec
{
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    /// <summary>Encodes a single OPCHDA_ITEM in NDR.</summary>
    public static void Write(ref NdrWriter writer, OpcHdaItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        int count = item.Timestamps.Length;
        writer.WriteUInt32(unchecked((uint)item.ClientHandle));
        writer.WriteUInt32(unchecked((uint)item.AggregateHandle));
        writer.WriteUInt32(unchecked((uint)count));

        writer.WriteUInt32(unchecked((uint)count));  // conformance for timestamps
        for (int i = 0; i < count; i++)
        {
            writer.WriteFileTime(item.Timestamps[i].UtcTicks - FileTimeEpochOffsetTicks);
        }

        writer.WriteConformantUInt32Array(item.Qualities);

        writer.WriteUInt32(unchecked((uint)count));  // conformance for variants
        for (int i = 0; i < count; i++)
        {
            writer.WriteVariant(item.Values[i]);
        }
    }

    /// <summary>Decodes a single OPCHDA_ITEM from NDR.</summary>
    public static OpcHdaItem Read(ref NdrReader reader)
    {
        uint hClient = reader.ReadUInt32();
        uint haAggregate = reader.ReadUInt32();
        uint dwCount = reader.ReadUInt32();
        if (dwCount > (uint)int.MaxValue)
        {
            throw new System.IO.InvalidDataException($"OPCHDA_ITEM dwCount {dwCount} too large.");
        }
        int count = (int)dwCount;

        uint tsConformance = reader.ReadUInt32();
        var timestamps = new DateTimeOffset[count];
        for (int i = 0; i < count; i++)
        {
            long ft = reader.ReadFileTime();
            timestamps[i] = new DateTimeOffset(ft + FileTimeEpochOffsetTicks, TimeSpan.Zero);
        }
        _ = tsConformance;

        uint[] qualities = reader.ReadConformantUInt32Array();

        uint variantConformance = reader.ReadUInt32();
        _ = variantConformance;
        var values = new OpcVariant[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = reader.ReadVariant();
        }

        return new OpcHdaItem(
            clientHandle: unchecked((int)hClient),
            aggregateHandle: unchecked((int)haAggregate),
            timestamps: timestamps,
            qualities: qualities,
            values: values);
    }
}
