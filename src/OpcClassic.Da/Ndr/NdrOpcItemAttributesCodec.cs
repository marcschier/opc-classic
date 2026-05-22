//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using OpcClassic.Ndr;

namespace OpcClassic.Da.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC DA <c>OPCITEMATTRIBUTES</c> struct,
/// matching <c>tagOPCITEMATTRIBUTES</c> in opcda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     LPWSTR   szAccessPath           - unique-pointer LPWSTR
///     LPWSTR   szItemID               - unique-pointer LPWSTR
///     INT32    bActive                - Win32 BOOL (-1 = TRUE, 0 = FALSE)
///     UINT32   hClient
///     UINT32   hServer
///     UINT32   dwAccessRights
///     UINT32   dwBlobSize             - conformance count for blob
///     BYTE[dwBlobSize] pBlob
///     UINT16   vtRequestedDataType
///     UINT16   vtCanonicalDataType    - no reserved padding between VARTYPEs
///     UINT32   dwEUType               - OPCEUTYPE enum value
///     VARIANT  vEUInfo
/// </code>
/// </remarks>
public static class NdrOpcItemAttributesCodec
{
    private const int Win32BoolTrue = unchecked((int)0xFFFFFFFFu);

    /// <summary>Encodes a single OPCITEMATTRIBUTES in NDR.</summary>
    public static void Write(ref NdrWriter writer, OpcItemAttributes attributes)
    {
        ArgumentNullException.ThrowIfNull(attributes);

        writer.WriteUnicodeStringPtr(attributes.AccessPath);
        writer.WriteUnicodeStringPtr(attributes.ItemId);
        writer.WriteInt32(attributes.Active ? Win32BoolTrue : 0);
        writer.WriteUInt32(unchecked((uint)attributes.ClientHandle));
        writer.WriteUInt32(unchecked((uint)attributes.ServerHandle));
        writer.WriteUInt32(unchecked((uint)attributes.AccessRights));
        writer.WriteConformantByteArray(attributes.Blob ?? Array.Empty<byte>());
        writer.WriteUInt16((ushort)attributes.RequestedDataType);
        writer.WriteUInt16((ushort)attributes.CanonicalDataType);
        writer.WriteUInt32(unchecked((uint)attributes.EUType));
        writer.WriteVariant(attributes.EUInfo);
    }

    /// <summary>Decodes a single OPCITEMATTRIBUTES from NDR.</summary>
    public static OpcItemAttributes Read(ref NdrReader reader)
    {
        string? accessPath = reader.ReadUnicodeStringPtr();
        string? itemId = reader.ReadUnicodeStringPtr();
        int bActive = reader.ReadInt32();
        uint hClient = reader.ReadUInt32();
        uint hServer = reader.ReadUInt32();
        uint dwAccessRights = reader.ReadUInt32();
        byte[] blob = reader.ReadConformantByteArray();
        var vtRequested = (VarType)reader.ReadUInt16();
        var vtCanonical = (VarType)reader.ReadUInt16();
        uint dwEUType = reader.ReadUInt32();
        OpcVariant euInfo = reader.ReadVariant();

        return new OpcItemAttributes(
            AccessPath: accessPath,
            ItemId: itemId,
            Active: bActive != 0,
            ClientHandle: unchecked((int)hClient),
            ServerHandle: unchecked((int)hServer),
            AccessRights: unchecked((int)dwAccessRights),
            Blob: blob,
            RequestedDataType: vtRequested,
            CanonicalDataType: vtCanonical,
            EUType: unchecked((int)dwEUType),
            EUInfo: euInfo);
    }
}
