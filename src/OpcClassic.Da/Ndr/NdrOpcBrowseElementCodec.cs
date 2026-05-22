//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using OpcClassic.Ndr;

namespace OpcClassic.Da.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC DA <c>OPCBROWSEELEMENT</c> struct,
/// matching <c>tagOPCBROWSEELEMENT</c> in opcda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     LPWSTR              szName            - WriteUnicodeStringPtr
///     LPWSTR              szItemID          - WriteUnicodeStringPtr
///     UINT32              dwFlagValue
///     UINT32              dwReserved (0)
///     OPCITEMPROPERTIES   ItemProperties
/// </code>
/// </remarks>
public static class NdrOpcBrowseElementCodec
{
    /// <summary>Encodes a single OPCBROWSEELEMENT in NDR.</summary>
    public static void Write(ref NdrWriter writer, OpcBrowseElementResult element)
    {
        ArgumentNullException.ThrowIfNull(element);

        writer.WriteUnicodeStringPtr(element.Name);
        writer.WriteUnicodeStringPtr(element.ItemId);
        writer.WriteUInt32(unchecked((uint)element.FlagValue));
        writer.WriteUInt32(0u);
        NdrOpcItemPropertiesCodec.Write(ref writer, element.Properties);
    }

    /// <summary>Decodes a single OPCBROWSEELEMENT from NDR.</summary>
    public static OpcBrowseElementResult Read(ref NdrReader reader)
    {
        string? name = reader.ReadUnicodeStringPtr();
        string? itemId = reader.ReadUnicodeStringPtr();
        uint flagValue = reader.ReadUInt32();
        _ = reader.ReadUInt32();
        OpcItemProperties properties = NdrOpcItemPropertiesCodec.Read(ref reader);

        return new OpcBrowseElementResult(
            Name: name,
            ItemId: itemId,
            FlagValue: unchecked((int)flagValue),
            Properties: properties);
    }
}
