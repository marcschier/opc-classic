//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
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
    private const int Win32BoolTrue = unchecked((int)0xFFFFFFFFu);

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
}
