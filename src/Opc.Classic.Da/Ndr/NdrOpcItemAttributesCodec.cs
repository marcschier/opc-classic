// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.InteropServices;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC DA <c>OPCITEMATTRIBUTES</c> struct,
/// matching <c>tagOPCITEMATTRIBUTES</c> in opcda.h.
/// </summary>
/// <remarks>
/// Wire layout under <c>pointer_default(unique)</c>. Strings and the
/// <c>[size_is(dwBlobSize)] BYTE* pBlob</c> are unique pointers — their bodies
/// live in the deferred-pointer pile (DCE 1.1 §14.3.12.3).
/// <code>
///     Inline part:
///       UINT32  szAccessPath_referent
///       UINT32  szItemID_referent
///       INT32   bActive (Win32 BOOL: -1 TRUE, 0 FALSE)
///       UINT32  hClient
///       UINT32  hServer
///       UINT32  dwAccessRights
///       UINT32  dwBlobSize          — conformance count for deferred blob
///       UINT32  pBlob_referent
///       UINT16  vtRequestedDataType
///       UINT16  vtCanonicalDataType
///       UINT32  dwEUType            (OPCEUTYPE)
///       VARIANT vEUInfo inline header
///
///     Deferred (in element order):
///       szAccessPath conformant Unicode string  (if referent != 0)
///       szItemID conformant Unicode string      (if referent != 0)
///       BYTE[dwBlobSize] conformant byte array  (if pBlob referent != 0)
///       VARIANT body                            (handled by NdrVariantExtensions)
/// </code>
/// </remarks>
public static class NdrOpcItemAttributesCodec
{
    private const int Win32BoolTrue = unchecked((int)0xFFFFFFFFu);

    /// <summary>
    /// Encodes a conformant OPCITEMATTRIBUTES array using DCE/RPC deferred-pointer pile layout, including the outer unique-pointer
    /// referent.
    /// </summary>
    public static void WriteConformantArray(ref NdrWriter writer, OpcItemAttributes[]? attributes)
    {
        if (attributes is null || attributes.Length == 0)
        {
            writer.WriteUniquePointerReferent(false);
            return;
        }

        writer.WriteUniquePointerReferent(true);
        writer.WriteUInt32((uint)attributes.Length);
        foreach (OpcItemAttributes attr in attributes)
        {
            WriteInline(ref writer, attr);
        }
        foreach (OpcItemAttributes attr in attributes)
        {
            WriteDeferred(ref writer, attr);
        }
    }

    /// <summary>
    /// Decodes a conformant OPCITEMATTRIBUTES array using DCE/RPC deferred-pointer pile layout, including the outer unique-pointer
    /// referent.
    /// </summary>
    public static OpcItemAttributes[] ReadConformantArray(ref NdrReader reader)
    {
        if (!reader.TryReadReferentId(out _))
        {
            return [];
        }
        uint maxCount = reader.ReadUInt32();
        int count = unchecked((int)maxCount);
        if (count <= 0) { return []; }

        var inlineParts = new ItemAttributesInline[count];
        for (int i = 0; i < count; i++)
        {
            inlineParts[i] = ReadInline(ref reader);
        }

        var result = new OpcItemAttributes[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = ApplyDeferred(ref reader, inlineParts[i]);
        }

        return result;
    }

    /// <summary>
    /// Encodes a single OPCITEMATTRIBUTES in NDR using the inline + deferred shape.
    /// </summary>
    public static void Write(ref NdrWriter writer, OpcItemAttributes attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        WriteInline(ref writer, attributes);
        WriteDeferred(ref writer, attributes);
    }

    /// <summary>
    /// Decodes a single OPCITEMATTRIBUTES from NDR using the inline + deferred shape.
    /// </summary>
    public static OpcItemAttributes Read(ref NdrReader reader)
    {
        ItemAttributesInline inlinePart = ReadInline(ref reader);
        return ApplyDeferred(ref reader, inlinePart);
    }

    private static void WriteInline(ref NdrWriter writer, OpcItemAttributes attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        byte[] blob = attributes.Blob ?? [];
        writer.WriteUniquePointerReferent(attributes.AccessPath is not null);
        writer.WriteUniquePointerReferent(attributes.ItemId is not null);
        writer.WriteInt32(attributes.Active ? Win32BoolTrue : 0);
        writer.WriteUInt32(unchecked((uint)attributes.ClientHandle));
        writer.WriteUInt32(unchecked((uint)attributes.ServerHandle));
        writer.WriteUInt32(unchecked((uint)attributes.AccessRights));
        writer.WriteUInt32((uint)blob.Length);            // dwBlobSize
        writer.WriteUniquePointerReferent(blob.Length > 0);
        writer.WriteUInt16((ushort)attributes.RequestedDataType);
        writer.WriteUInt16((ushort)attributes.CanonicalDataType);
        writer.WriteUInt32(unchecked((uint)attributes.EUType));
        writer.WriteVariant(attributes.EUInfo);
    }

    private static void WriteDeferred(ref NdrWriter writer, OpcItemAttributes attributes)
    {
        if (attributes.AccessPath is not null)
        {
            writer.WriteUnicodeString(attributes.AccessPath);
        }
        if (attributes.ItemId is not null)
        {
            writer.WriteUnicodeString(attributes.ItemId);
        }
        if (attributes.Blob is { Length: > 0 } blob)
        {
            writer.WriteConformantByteArray(blob);
        }
    }

    private static ItemAttributesInline ReadInline(ref NdrReader reader)
    {
        uint accessPathRef = reader.ReadUInt32();
        uint itemIdRef = reader.ReadUInt32();
        int bActive = reader.ReadInt32();
        uint hClient = reader.ReadUInt32();
        uint hServer = reader.ReadUInt32();
        uint dwAccessRights = reader.ReadUInt32();
        _ = reader.ReadUInt32();                          // dwBlobSize (conformance count)
        uint blobRef = reader.ReadUInt32();
        var vtRequested = (VarType)reader.ReadUInt16();
        var vtCanonical = (VarType)reader.ReadUInt16();
        uint dwEUType = reader.ReadUInt32();
        OpcVariant euInfo = reader.ReadVariant();
        return new ItemAttributesInline(accessPathRef, itemIdRef, bActive, hClient, hServer, dwAccessRights, blobRef, vtRequested, vtCanonical, dwEUType, euInfo);
    }

    private static OpcItemAttributes ApplyDeferred(ref NdrReader reader, ItemAttributesInline inlinePart)
    {
        string? accessPath = inlinePart.AccessPathRef == 0u ? null : reader.ReadUnicodeString();
        string? itemId = inlinePart.ItemIdRef == 0u ? null : reader.ReadUnicodeString();
        byte[] blob = inlinePart.BlobRef == 0u ? [] : reader.ReadConformantByteArray();
        return new OpcItemAttributes(
            AccessPath: accessPath,
            ItemId: itemId,
            Active: inlinePart.Active != 0,
            ClientHandle: unchecked((int)inlinePart.ClientHandle),
            ServerHandle: unchecked((int)inlinePart.ServerHandle),
            AccessRights: unchecked((int)inlinePart.AccessRights),
            Blob: blob,
            RequestedDataType: inlinePart.RequestedDataType,
            CanonicalDataType: inlinePart.CanonicalDataType,
            EUType: unchecked((int)inlinePart.EUType),
            EUInfo: inlinePart.EUInfo);
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct ItemAttributesInline(
        uint AccessPathRef,
        uint ItemIdRef,
        int Active,
        uint ClientHandle,
        uint ServerHandle,
        uint AccessRights,
        uint BlobRef,
        VarType RequestedDataType,
        VarType CanonicalDataType,
        uint EUType,
        OpcVariant EUInfo);
}
