//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Ndr;

namespace Opc.Classic.Hda.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC HDA <c>OPCHDA_ANNOTATION</c> struct,
/// matching <c>tagOPCHDA_ANNOTATION</c> in opchda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     UINT32   hClient
///     UINT32   dwNumValues
///     FILETIME[dwNumValues] ftTimeStamps      - conformant array (manual count + loop)
///     LPWSTR[dwNumValues]   szAnnotation      - conformant array of unique-pointer LPWSTRs
///     FILETIME[dwNumValues] ftAnnotationTime  - conformant array (manual count + loop)
///     LPWSTR[dwNumValues]   szUser            - conformant array of unique-pointer LPWSTRs
/// </code>
/// The LPWSTR arrays currently use a simplified interleaved form: each element is
/// emitted as its referent plus string body via <see cref="NdrWriter.WriteUnicodeStringPtr"/>.
/// Real Windows-emitted wire dumps remain a future validation source.
/// </remarks>
public static class NdrOpcHdaAnnotationCodec
{
    private const long FileTimeEpochOffsetTicks = 504911232000000000L;

    /// <summary>Encodes a single OPCHDA_ANNOTATION in NDR.</summary>
    public static void Write(ref NdrWriter writer, OpcHdaAnnotation annotation)
    {
        ArgumentNullException.ThrowIfNull(annotation);

        int count = annotation.Timestamps.Length;
        writer.WriteUInt32(unchecked((uint)annotation.ClientHandle));
        writer.WriteUInt32(unchecked((uint)count));

        writer.WriteUInt32(unchecked((uint)count));  // timestamp conformance
        for (int i = 0; i < count; i++)
        {
            writer.WriteFileTime(annotation.Timestamps[i].UtcTicks - FileTimeEpochOffsetTicks);
        }

        writer.WriteUInt32(unchecked((uint)count));  // annotation string conformance
        for (int i = 0; i < count; i++)
        {
            writer.WriteUnicodeStringPtr(annotation.Annotations[i]);
        }

        writer.WriteUInt32(unchecked((uint)count));  // annotation time conformance
        for (int i = 0; i < count; i++)
        {
            writer.WriteFileTime(annotation.AnnotationTimes[i].UtcTicks - FileTimeEpochOffsetTicks);
        }

        writer.WriteUInt32(unchecked((uint)count));  // user string conformance
        for (int i = 0; i < count; i++)
        {
            writer.WriteUnicodeStringPtr(annotation.Users[i]);
        }
    }

    /// <summary>Decodes a single OPCHDA_ANNOTATION from NDR.</summary>
    public static OpcHdaAnnotation Read(ref NdrReader reader)
    {
        uint hClient = reader.ReadUInt32();
        uint dwNumValues = reader.ReadUInt32();
        if (dwNumValues > (uint)int.MaxValue)
        {
            throw new System.IO.InvalidDataException($"OPCHDA_ANNOTATION dwNumValues {dwNumValues} too large.");
        }
        int count = (int)dwNumValues;

        _ = reader.ReadUInt32();  // timestamp conformance
        var timestamps = new DateTimeOffset[count];
        for (int i = 0; i < count; i++)
        {
            long ft = reader.ReadFileTime();
            timestamps[i] = new DateTimeOffset(ft + FileTimeEpochOffsetTicks, TimeSpan.Zero);
        }

        _ = reader.ReadUInt32();  // annotation string conformance
        var annotations = new string?[count];
        for (int i = 0; i < count; i++)
        {
            annotations[i] = reader.ReadUnicodeStringPtr();
        }

        _ = reader.ReadUInt32();  // annotation time conformance
        var annotationTimes = new DateTimeOffset[count];
        for (int i = 0; i < count; i++)
        {
            long ft = reader.ReadFileTime();
            annotationTimes[i] = new DateTimeOffset(ft + FileTimeEpochOffsetTicks, TimeSpan.Zero);
        }

        _ = reader.ReadUInt32();  // user string conformance
        var users = new string?[count];
        for (int i = 0; i < count; i++)
        {
            users[i] = reader.ReadUnicodeStringPtr();
        }

        return new OpcHdaAnnotation(
            clientHandle: unchecked((int)hClient),
            timestamps: timestamps,
            annotations: annotations,
            annotationTimes: annotationTimes,
            users: users);
    }
}
