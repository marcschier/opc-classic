//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC DA <c>OPCITEMPROPERTIES</c> struct,
/// matching <c>tagOPCITEMPROPERTIES</c> in opcda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     INT32    hrErrorID
///     UINT32   dwNumProperties
///     UINT32   conformance count for pItemProperties
///     OPCITEMPROPERTY[pItemProperties count] pItemProperties
///     UINT32   dwReserved (0)
/// </code>
/// </remarks>
public static class NdrOpcItemPropertiesCodec {
    /// <summary>Encodes a single OPCITEMPROPERTIES in NDR.</summary>
    public static void Write(ref NdrWriter writer, OpcItemProperties itemProperties) {
        ArgumentNullException.ThrowIfNull(itemProperties);

        OpcItemPropertyResult[] properties = itemProperties.Properties;
        int count = properties.Length;
        writer.WriteInt32(itemProperties.ErrorId);
        writer.WriteUInt32(unchecked((uint)count));
        writer.WriteUInt32(unchecked((uint)count));
        for (int i = 0; i < count; i++) {
            NdrOpcItemPropertyCodec.Write(ref writer, properties[i]);
        }
        writer.WriteUInt32(0u);
    }

    /// <summary>Decodes a single OPCITEMPROPERTIES from NDR.</summary>
    public static OpcItemProperties Read(ref NdrReader reader) {
        int hrErrorId = reader.ReadInt32();
        _ = reader.ReadUInt32();
        uint arrayCount = reader.ReadUInt32();
        if (arrayCount > (uint)int.MaxValue) {
            throw new InvalidDataException($"OPCITEMPROPERTIES pItemProperties conformance count {arrayCount} too large.");
        }

        int count = (int)arrayCount;
        var properties = new OpcItemPropertyResult[count];
        for (int i = 0; i < count; i++) {
            properties[i] = NdrOpcItemPropertyCodec.Read(ref reader);
        }
        _ = reader.ReadUInt32();

        return new OpcItemProperties(hrErrorId, properties);
    }
}
