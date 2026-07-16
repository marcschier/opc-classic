// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.InteropServices;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC DA <c>OPCITEMSTATE</c> struct.
/// </summary>
/// <remarks>
/// <para>Inline part (20 bytes): hClient, FILETIME, wQuality, wReserved,
/// vDataValue referent. The VARIANT is a [unique] pointer per MS-OAUT
/// 2.2.29.2 — the wireVARIANT body is deferred.</para>
/// <para>For a conformant array, NDR (DCE 1.1 §14.3.12.3) emits all N inline
/// parts back-to-back followed by all N deferred VARIANT bodies in order.
/// Use <see cref="ReadConformantArray"/> / <see cref="WriteConformantArray"/>
/// for the array case; <see cref="Read"/>/<see cref="Write"/> emit inline +
/// deferred for a single element.</para>
/// </remarks>
public static class NdrOpcItemStateCodec
{
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    /// <summary>
    /// Encodes a single OPCITEMSTATE (inline + deferred VARIANT body).
    /// </summary>
    public static void Write(ref NdrWriter writer, OpcItemState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        WriteInlinePart(ref writer, state);
        writer.WriteVariant(state.Value);
    }

    /// <summary>
    /// Decodes a single OPCITEMSTATE (inline + immediately deferred VARIANT body).
    /// </summary>
    public static OpcItemState Read(ref NdrReader reader)
    {
        InlinePart inline = ReadInlinePart(ref reader);
        return ApplyDeferred(ref reader, inline);
    }

    /// <summary>
    /// Writes a conformant array of OPCITEMSTATE in NDR deferred-pile layout
    /// prefixed by the standard <c>[out] T**</c> envelope: <c>[unique]</c>
    /// referent + max_count + N inline parts + N deferred wireVARIANT bodies.
    /// Matches the wire shape produced by other deferred-pile helpers
    /// (<see cref="NdrOpcItemResultCodec.WriteConformantArray"/>) and
    /// consumed by <see cref="ReadConformantArray"/> when paired with a
    /// preceding referent + count read.
    /// </summary>
    public static void WriteConformantArray(ref NdrWriter writer, OpcItemState[]? states)
    {
        if (states is null || states.Length == 0)
        {
            writer.WriteUniquePointerReferent(false);
            return;
        }

        writer.WriteUniquePointerReferent(true);
        writer.WriteUInt32(unchecked((uint)states.Length));
        foreach (OpcItemState s in states) { WriteInlinePart(ref writer, s); }
        foreach (OpcItemState s in states) { writer.WriteVariant(s.Value); }
    }

    /// <summary>
    /// Reads <paramref name="count"/> OPCITEMSTATE entries in deferred-pile
    /// layout. The caller must have already consumed any outer max_count /
    /// referent and pass the element count via <paramref name="count"/>.
    /// </summary>
    public static OpcItemState[] ReadConformantArray(ref NdrReader reader, int count)
    {
        if (count <= 0) { return []; }
        var inlines = new InlinePart[count];
        for (int i = 0; i < count; i++) { inlines[i] = ReadInlinePart(ref reader); }
        var results = new OpcItemState[count];
        for (int i = 0; i < count; i++) { results[i] = ApplyDeferred(ref reader, inlines[i]); }
        return results;
    }

    /// <summary>
    /// Reads an outer unique pointer followed by a conformant OPCITEMSTATE array.
    /// </summary>
    public static OpcItemState[] ReadUniqueConformantArray(ref NdrReader reader)
    {
        if (!reader.TryReadReferentId(out _))
        {
            return [];
        }

        int count = checked((int)reader.ReadUInt32());
        return ReadConformantArray(ref reader, count);
    }

    private static void WriteInlinePart(ref NdrWriter writer, OpcItemState state)
    {
        writer.WriteUInt32(unchecked((uint)state.ClientHandle));
        writer.WriteFileTime(ToFileTime(state.Timestamp));
        writer.WriteUInt16(unchecked((ushort)(state.Quality.RawValue & 0xFFFF)));
        writer.WriteUInt16(0);
        // VARIANT vDataValue is a [unique] pointer per MS-OAUT 2.2.29.2 — emit
        // a non-null referent so the receiver decodes the deferred body. (We
        // never emit a null OPCITEMSTATE value: VT_EMPTY is encoded as a real
        // wireVARIANT with vt=0.)
        writer.WriteUniquePointerReferent(true);
    }

    private static InlinePart ReadInlinePart(ref NdrReader reader)
    {
        uint hClient = reader.ReadUInt32();
        DateTimeOffset timestamp = ReadAndDecodeFileTime(ref reader, "ftTimeStamp");
        ushort wQuality = reader.ReadUInt16();
        _ = reader.ReadUInt16();
        uint variantRef = reader.ReadUInt32();
        return new InlinePart(unchecked((int)hClient), timestamp, wQuality, variantRef);
    }

    private static OpcItemState ApplyDeferred(ref NdrReader reader, InlinePart inline)
    {
        OpcVariant value = inline.VariantRef == 0u
            ? OpcVariant.Empty
            : reader.ReadVariant();
        return new OpcItemState(
            ClientHandle: inline.ClientHandle,
            Timestamp: inline.Timestamp,
            Quality: new OpcQuality(inline.Quality),
            Value: value);
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly struct InlinePart
    {
        public InlinePart(int clientHandle, DateTimeOffset timestamp, ushort quality, uint variantRef)
        {
            ClientHandle = clientHandle;
            Timestamp = timestamp;
            Quality = quality;
            VariantRef = variantRef;
        }
        public int ClientHandle { get; }
        public DateTimeOffset Timestamp { get; }
        public ushort Quality { get; }
        public uint VariantRef { get; }
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
