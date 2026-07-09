// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Ndr;

namespace Opc.Classic.Hda.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC HDA <c>OPCHDA_ATTRIBUTE</c> struct,
/// matching <c>tagOPCHDA_ATTRIBUTE</c> in opchda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     UINT32    hClient
///     UINT32    dwNumValues
///     UINT32    dwAttributeID
///     FILETIME[dwNumValues] ftTimeStamps    - conformant array (manual count + loop)
///     VARIANT[dwNumValues]  vAttributeValues - conformant array (manual count + loop)
/// </code>
/// </remarks>
public static class NdrOpcHdaAttributeCodec
{
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    /// <summary>
    /// Encodes a single OPCHDA_ATTRIBUTE in NDR.
    /// </summary>
    public static void Write(ref NdrWriter writer, OpcHdaAttribute attribute)
    {
        ArgumentNullException.ThrowIfNull(attribute);

        int count = attribute.Timestamps.Length;
        writer.WriteUInt32(unchecked((uint)attribute.ClientHandle));
        writer.WriteUInt32(unchecked((uint)count));
        writer.WriteUInt32(unchecked((uint)attribute.AttributeId));

        writer.WriteUInt32(unchecked((uint)count));  // ts conformance
        for (int i = 0; i < count; i++)
        {
            writer.WriteFileTime(attribute.Timestamps[i].UtcTicks - FileTimeEpochOffsetTicks);
        }

        writer.WriteUInt32(unchecked((uint)count));  // value conformance
        for (int i = 0; i < count; i++)
        {
            writer.WriteVariant(attribute.Values[i]);
        }
    }

    /// <summary>
    /// Decodes a single OPCHDA_ATTRIBUTE from NDR.
    /// </summary>
    public static OpcHdaAttribute Read(ref NdrReader reader)
    {
        uint hClient = reader.ReadUInt32();
        uint dwNumValues = reader.ReadUInt32();
        uint dwAttributeId = reader.ReadUInt32();
        if (dwNumValues > (uint)int.MaxValue)
        {
            throw new System.IO.InvalidDataException($"OPCHDA_ATTRIBUTE dwNumValues {dwNumValues} too large.");
        }
        int count = (int)dwNumValues;

        _ = reader.ReadUInt32();  // ts conformance
        var timestamps = new DateTimeOffset[count];
        for (int i = 0; i < count; i++)
        {
            long ft = reader.ReadFileTime();
            timestamps[i] = new DateTimeOffset(ft + FileTimeEpochOffsetTicks, TimeSpan.Zero);
        }

        _ = reader.ReadUInt32();  // value conformance
        var values = new OpcVariant[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = reader.ReadVariant();
        }

        return new OpcHdaAttribute(
            clientHandle: unchecked((int)hClient),
            attributeId: unchecked((int)dwAttributeId),
            timestamps: timestamps,
            values: values);
    }
}
