//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Orpc;

internal static class OrpcExtentArrayCodec
{
    public static void Write(ref NdrWriter writer, IReadOnlyList<OrpcExtent>? extensions)
    {
        if (extensions is null)
        {
            writer.WriteNullReferent();
            return;
        }

        _ = writer.WriteReferentId();
        writer.WriteUInt32(unchecked((uint)extensions.Count));
        writer.WriteUInt32(0u);
        WriteExtentPointerArray(ref writer, extensions);
    }

    public static IReadOnlyList<OrpcExtent>? Read(ref NdrReader reader)
    {
        if (!reader.TryReadReferentId(out _))
        {
            return null;
        }

        uint count = reader.ReadUInt32();
        uint reserved = reader.ReadUInt32();
        if (reserved != 0u)
        {
            throw new InvalidOperationException($"ORPC_EXTENT_ARRAY reserved must be zero but was {reserved}.");
        }

        return ReadExtentPointerArray(ref reader, count);
    }

    private static void WriteExtentPointerArray(ref NdrWriter writer, IReadOnlyList<OrpcExtent> extensions)
    {
        if (extensions.Count == 0)
        {
            writer.WriteNullReferent();
            return;
        }

        int pointerCount = RoundPointerCount(unchecked((uint)extensions.Count));
        _ = writer.WriteReferentId();
        writer.WriteConformanceHeader(pointerCount);
        for (int i = 0; i < extensions.Count; i++)
        {
            _ = writer.WriteReferentId();
        }

        for (int i = extensions.Count; i < pointerCount; i++)
        {
            writer.WriteNullReferent();
        }

        for (int i = 0; i < extensions.Count; i++)
        {
            WriteExtent(ref writer, extensions[i]);
        }
    }

    private static IReadOnlyList<OrpcExtent> ReadExtentPointerArray(ref NdrReader reader, uint count)
    {
        if (!reader.TryReadReferentId(out _))
        {
            if (count != 0u)
            {
                throw new InvalidOperationException("ORPC_EXTENT_ARRAY extent pointer is null for a non-empty array.");
            }

            return Array.Empty<OrpcExtent>();
        }

        int declaredPointerCount = RoundPointerCount(count);
        int encodedPointerCount = reader.ReadConformanceHeader();
        if (encodedPointerCount < declaredPointerCount)
        {
            throw new InvalidOperationException(
                $"ORPC_EXTENT_ARRAY pointer count {encodedPointerCount} is smaller than declared count {declaredPointerCount}.");
        }

        var hasReferent = new bool[encodedPointerCount];
        for (int i = 0; i < encodedPointerCount; i++)
        {
            hasReferent[i] = reader.TryReadReferentId(out _);
        }

        int declaredCount = checked((int)count);
        var extents = new List<OrpcExtent>(declaredCount);
        for (int i = 0; i < encodedPointerCount; i++)
        {
            if (!hasReferent[i])
            {
                continue;
            }

            OrpcExtent extent = ReadExtent(ref reader);
            if (i < declaredCount)
            {
                extents.Add(extent);
            }
        }

        return extents;
    }

    private static void WriteExtent(ref NdrWriter writer, OrpcExtent extent)
    {
        writer.WriteGuid(extent.Id);
        ReadOnlySpan<byte> data = extent.Data.Span;
        writer.WriteUInt32(unchecked((uint)data.Length));
        int paddedLength = RoundExtentDataLength(unchecked((uint)data.Length));
        writer.WriteConformanceHeader(paddedLength);
        writer.WriteRawBytes(data);
        WriteZeroPadding(ref writer, paddedLength - data.Length);
    }

    private static OrpcExtent ReadExtent(ref NdrReader reader)
    {
        Guid id = reader.ReadGuid();
        uint size = reader.ReadUInt32();
        int paddedLength = RoundExtentDataLength(size);
        int encodedLength = reader.ReadConformanceHeader();
        if (encodedLength < checked((int)size) || encodedLength < paddedLength)
        {
            throw new InvalidOperationException(
                $"ORPC_EXTENT encoded length {encodedLength} is smaller than size {size}.");
        }

        ReadOnlySpan<byte> encodedData = reader.ReadRawBytes(encodedLength);
        byte[] data = encodedData[..checked((int)size)].ToArray();
        return OrpcExtent.FromOwnedData(id, data);
    }

    private static int RoundPointerCount(uint count)
    {
        if (count > int.MaxValue - 1u)
        {
            throw new InvalidOperationException($"ORPC_EXTENT_ARRAY count {count} is too large.");
        }

        return checked((int)((count + 1u) & ~1u));
    }

    private static int RoundExtentDataLength(uint size)
    {
        if (size > int.MaxValue - 7u)
        {
            throw new InvalidOperationException($"ORPC_EXTENT size {size} is too large.");
        }

        return checked((int)((size + 7u) & ~7u));
    }

    private static void WriteZeroPadding(ref NdrWriter writer, int count)
    {
        for (int i = 0; i < count; i++)
        {
            writer.WriteByte(0);
        }
    }
}
