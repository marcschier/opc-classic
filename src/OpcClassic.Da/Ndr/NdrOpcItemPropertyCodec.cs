//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using OpcClassic.Ndr;

namespace OpcClassic.Da.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC DA <c>OPCITEMPROPERTY</c> struct,
/// matching <c>tagOPCITEMPROPERTY</c> in opcda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     UINT16   vtDataType
///     UINT16   wReserved (0)
///     UINT32   dwPropertyID
///     LPWSTR   szItemID            - WriteUnicodeStringPtr
///     LPWSTR   szDescription       - WriteUnicodeStringPtr
///     VARIANT  vValue              - WriteVariant
///     INT32    hrErrorID
///     UINT32   dwReserved (0)
/// </code>
/// </remarks>
public static class NdrOpcItemPropertyCodec
{
    /// <summary>Encodes a single OPCITEMPROPERTY in NDR.</summary>
    public static void Write(ref NdrWriter writer, OpcItemPropertyResult prop)
    {
        ArgumentNullException.ThrowIfNull(prop);

        writer.WriteUInt16((ushort)prop.DataType);
        writer.WriteUInt16(0);  // wReserved
        writer.WriteUInt32(unchecked((uint)prop.PropertyId));
        writer.WriteUnicodeStringPtr(prop.ItemId);
        writer.WriteUnicodeStringPtr(prop.Description);
        writer.WriteVariant(prop.Value);
        writer.WriteInt32(prop.ErrorId);
        writer.WriteUInt32(0u);  // dwReserved
    }

    /// <summary>Decodes a single OPCITEMPROPERTY from NDR.</summary>
    public static OpcItemPropertyResult Read(ref NdrReader reader)
    {
        var vt = (VarType)reader.ReadUInt16();
        _ = reader.ReadUInt16();   // wReserved
        uint propertyId = reader.ReadUInt32();
        string? itemId = reader.ReadUnicodeStringPtr();
        string? description = reader.ReadUnicodeStringPtr();
        OpcVariant value = reader.ReadVariant();
        int hrErrorId = reader.ReadInt32();
        _ = reader.ReadUInt32();   // dwReserved

        return new OpcItemPropertyResult(
            DataType: vt,
            PropertyId: unchecked((int)propertyId),
            ItemId: itemId,
            Description: description,
            Value: value,
            ErrorId: hrErrorId);
    }
}
