//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Runtime.InteropServices;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC DA <c>OPCITEMDEF</c> struct,
/// matching <c>tagOPCITEMDEF</c> in opcda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     LPWSTR   szAccessPath           - unique-pointer LPWSTR
///     LPWSTR   szItemID               - unique-pointer LPWSTR
///     INT32    bActive                - Win32 BOOL (-1 = TRUE, 0 = FALSE)
///     UINT32   hClient
///     UINT32   dwBlobSize             - conformance count for blob
///     BYTE[dwBlobSize] pBlob
///     UINT16   vtRequestedDataType
///     UINT16   wReserved (0)
/// </code>
/// </remarks>
public static class NdrOpcItemDefCodec
{
    private const int Win32BoolTrue = 1;

    /// <summary>Encodes a conformant OPCITEMDEF array using DCE/RPC deferred-pointer pile layout.</summary>
    public static void WriteConformantArray(ref NdrWriter writer, OpcItemDef[]? definitions)
    {
        if (definitions is null || definitions.Length == 0)
        {
            writer.WriteUInt32(0u);
            return;
        }

        writer.WriteUInt32((uint)definitions.Length);
        foreach (OpcItemDef definition in definitions)
        {
            WriteInline(ref writer, definition);
        }
        foreach (OpcItemDef definition in definitions)
        {
            WriteDeferred(ref writer, definition);
        }
    }

    /// <summary>Decodes a conformant OPCITEMDEF array using DCE/RPC deferred-pointer pile layout.</summary>
    public static OpcItemDef[] ReadConformantArray(ref NdrReader reader)
    {
        uint maxCount = reader.ReadUInt32();
        int count = unchecked((int)maxCount);
        if (count <= 0) { return []; }

        var inlineParts = new ItemDefInline[count];
        for (int i = 0; i < count; i++)
        {
            inlineParts[i] = ReadInline(ref reader);
        }

        var result = new OpcItemDef[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = ApplyDeferred(ref reader, inlineParts[i]);
        }

        return result;
    }

    /// <summary>Encodes a single OPCITEMDEF in NDR.</summary>
    public static void Write(ref NdrWriter writer, OpcItemDef def)
    {
        ArgumentNullException.ThrowIfNull(def);

        writer.WriteUnicodeStringPtr(def.AccessPath);
        writer.WriteUnicodeStringPtr(def.ItemId);
        writer.WriteInt32(def.Active ? Win32BoolTrue : 0);
        writer.WriteUInt32(unchecked((uint)def.ClientHandle));
        writer.WriteConformantByteArray(def.Blob ?? Array.Empty<byte>());
        writer.WriteUInt16((ushort)def.RequestedDataType);
        writer.WriteUInt16(0);  // wReserved
    }

    /// <summary>Decodes a single OPCITEMDEF from NDR.</summary>
    public static OpcItemDef Read(ref NdrReader reader)
    {
        string? accessPath = reader.ReadUnicodeStringPtr();
        string? itemId = reader.ReadUnicodeStringPtr();
        int bActive = reader.ReadInt32();
        uint hClient = reader.ReadUInt32();
        byte[] blob = reader.ReadConformantByteArray();
        var vtRequested = (VarType)reader.ReadUInt16();
        _ = reader.ReadUInt16();  // wReserved

        return new OpcItemDef(
            AccessPath: accessPath,
            ItemId: itemId,
            Active: bActive != 0,
            ClientHandle: unchecked((int)hClient),
            Blob: blob,
            RequestedDataType: vtRequested);
    }

    private static void WriteInline(ref NdrWriter writer, OpcItemDef def)
    {
        ArgumentNullException.ThrowIfNull(def);

        byte[] blob = def.Blob ?? [];
        writer.WriteUniquePointerReferent(def.AccessPath is not null);
        writer.WriteUniquePointerReferent(def.ItemId is not null);
        writer.WriteInt32(def.Active ? Win32BoolTrue : 0);
        writer.WriteUInt32(unchecked((uint)def.ClientHandle));
        writer.WriteUInt32((uint)blob.Length);
        writer.WriteUniquePointerReferent(blob.Length > 0);
        writer.WriteUInt16((ushort)def.RequestedDataType);
        writer.WriteUInt16(0);
    }

    private static void WriteDeferred(ref NdrWriter writer, OpcItemDef def)
    {
        if (def.AccessPath is not null)
        {
            writer.WriteUnicodeString(def.AccessPath);
        }
        if (def.ItemId is not null)
        {
            writer.WriteUnicodeString(def.ItemId);
        }
        if (def.Blob is { Length: > 0 } blob)
        {
            writer.WriteConformantByteArray(blob);
        }
    }

    private static ItemDefInline ReadInline(ref NdrReader reader)
    {
        uint accessPathRef = reader.ReadUInt32();
        uint itemIdRef = reader.ReadUInt32();
        int bActive = reader.ReadInt32();
        uint hClient = reader.ReadUInt32();
        _ = reader.ReadUInt32();
        uint blobRef = reader.ReadUInt32();
        var vtRequested = (VarType)reader.ReadUInt16();
        _ = reader.ReadUInt16();
        return new ItemDefInline(accessPathRef, itemIdRef, bActive, hClient, blobRef, vtRequested);
    }

    private static OpcItemDef ApplyDeferred(ref NdrReader reader, ItemDefInline inlinePart)
    {
        string? accessPath = inlinePart.AccessPathRef == 0u ? null : reader.ReadUnicodeString();
        string? itemId = inlinePart.ItemIdRef == 0u ? null : reader.ReadUnicodeString();
        byte[] blob = inlinePart.BlobRef == 0u ? [] : reader.ReadConformantByteArray();
        return new OpcItemDef(
            AccessPath: accessPath,
            ItemId: itemId,
            Active: inlinePart.Active != 0,
            ClientHandle: unchecked((int)inlinePart.ClientHandle),
            Blob: blob,
            RequestedDataType: inlinePart.RequestedDataType);
    }

    [StructLayout(LayoutKind.Auto)]
    private readonly record struct ItemDefInline(
        uint AccessPathRef,
        uint ItemIdRef,
        int Active,
        uint ClientHandle,
        uint BlobRef,
        VarType RequestedDataType);
}
