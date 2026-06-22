// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Runtime.InteropServices;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC DA <c>OPCITEMRESULT</c> struct,
/// matching <c>tagOPCITEMRESULT</c> in opcda.h.
/// </summary>
/// <remarks>
/// Wire layout under <c>pointer_default(unique)</c>. The struct has an embedded
/// <c>[size_is(dwBlobSize)] BYTE* pBlob</c> — a unique pointer — so its body
/// goes into the deferred-pointer pile (DCE 1.1 §14.3.12.3) rather than inline.
/// <code>
///     Inline part (20 bytes):
///       UINT32  hServer
///       UINT16  vtCanonicalDataType
///       UINT16  wReserved (0)
///       UINT32  dwAccessRights
///       UINT32  dwBlobSize          — conformance count for the deferred blob
///       UINT32  pBlob_referent_id   — 0 = null, else non-zero referent
///
///     Deferred (per non-null pBlob):
///       BYTE[dwBlobSize] conformant byte array (max_count + bytes)
/// </code>
/// </remarks>
public static class NdrOpcItemResultCodec
{
    /// <summary>
    /// Encodes a conformant OPCITEMRESULT array using DCE/RPC deferred-pointer pile layout, including the outer unique-pointer referent.
    /// </summary>
    /// <remarks>
    /// Self-contained encoder for <c>[out, size_is(,N)] OPCITEMRESULT**</c> wire shape:
    /// emits the unique-pointer referent (or 0 for null/empty) followed by max_count + N inline + N deferred.
    /// Caller must NOT pre-emit the referent.
    /// </remarks>
    public static void WriteConformantArray(ref NdrWriter writer, OpcItemResult[]? results)
    {
        if (results is null || results.Length == 0)
        {
            writer.WriteUniquePointerReferent(false);  // null referent
            return;
        }

        writer.WriteUniquePointerReferent(true);
        writer.WriteUInt32((uint)results.Length);
        foreach (OpcItemResult result in results)
        {
            WriteInline(ref writer, result);
        }
        foreach (OpcItemResult result in results)
        {
            WriteDeferred(ref writer, result);
        }
    }

    /// <summary>
    /// Decodes a conformant OPCITEMRESULT array using DCE/RPC deferred-pointer pile layout.
    /// </summary>
    /// <remarks>
    /// Self-contained helper for the <c>[out, size_is(,N)] OPCITEMRESULT**</c> wire shape under
    /// <c>pointer_default(unique)</c>. Reads the outer unique-pointer referent first; a null
    /// referent returns <see cref="Array.Empty{T}"/>. Otherwise reads <c>max_count</c> + N inline
    /// parts + N deferred parts. Caller must NOT pre-consume the referent.
    /// </remarks>
    public static OpcItemResult[] ReadConformantArray(ref NdrReader reader)
    {
        if (!reader.TryReadReferentId(out _))
        {
            return [];
        }
        uint maxCount = reader.ReadUInt32();
        int count = unchecked((int)maxCount);
        if (count <= 0) { return []; }

        var inlineParts = new ItemResultInline[count];
        for (int i = 0; i < count; i++)
        {
            inlineParts[i] = ReadInline(ref reader);
        }

        var result = new OpcItemResult[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = ApplyDeferred(ref reader, inlineParts[i]);
        }

        return result;
    }

    /// <summary>
    /// Encodes a single OPCITEMRESULT in NDR using the inline + deferred shape.
    /// </summary>
    public static void Write(ref NdrWriter writer, OpcItemResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        WriteInline(ref writer, result);
        WriteDeferred(ref writer, result);
    }

    /// <summary>
    /// Decodes a single OPCITEMRESULT from NDR using the inline + deferred shape.
    /// </summary>
    public static OpcItemResult Read(ref NdrReader reader)
    {
        ItemResultInline inlinePart = ReadInline(ref reader);
        return ApplyDeferred(ref reader, inlinePart);
    }

    private static void WriteInline(ref NdrWriter writer, OpcItemResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        byte[] blob = result.Blob ?? [];
        writer.WriteUInt32(unchecked((uint)result.ServerHandle));
        writer.WriteUInt16((ushort)result.CanonicalDataType);
        writer.WriteUInt16(0);                          // wReserved
        writer.WriteUInt32(unchecked((uint)result.AccessRights));
        writer.WriteUInt32((uint)blob.Length);          // dwBlobSize
        writer.WriteUniquePointerReferent(blob.Length > 0);
    }

    private static void WriteDeferred(ref NdrWriter writer, OpcItemResult result)
    {
        if (result.Blob is { Length: > 0 } blob)
        {
            writer.WriteConformantByteArray(blob);
        }
    }

    private static ItemResultInline ReadInline(ref NdrReader reader)
    {
        uint hServer = reader.ReadUInt32();
        var vtCanonical = (VarType)reader.ReadUInt16();
        _ = reader.ReadUInt16();                        // wReserved
        uint dwAccessRights = reader.ReadUInt32();
        _ = reader.ReadUInt32();                        // dwBlobSize (conformance count)
        uint blobRef = reader.ReadUInt32();
        return new ItemResultInline(hServer, vtCanonical, dwAccessRights, blobRef);
    }

    private static OpcItemResult ApplyDeferred(ref NdrReader reader, ItemResultInline inlinePart)
    {
        byte[] blob = inlinePart.BlobRef == 0u ? [] : reader.ReadConformantByteArray();
        return new OpcItemResult(
            ServerHandle: unchecked((int)inlinePart.ServerHandle),
            CanonicalDataType: inlinePart.CanonicalDataType,
            AccessRights: unchecked((int)inlinePart.AccessRights),
            Blob: blob);
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct ItemResultInline(
        uint ServerHandle,
        VarType CanonicalDataType,
        uint AccessRights,
        uint BlobRef);
}
