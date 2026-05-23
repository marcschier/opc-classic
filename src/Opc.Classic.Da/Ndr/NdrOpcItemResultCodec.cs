//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Ndr;

/// <summary>
/// NDR encoder / decoder for the OPC DA <c>OPCITEMRESULT</c> struct,
/// matching <c>tagOPCITEMRESULT</c> in opcda.h.
/// </summary>
/// <remarks>
/// Wire layout (after outer 4-byte alignment):
/// <code>
///     UINT32   hServer
///     UINT16   vtCanonicalDataType
///     UINT16   wReserved (0)
///     UINT32   dwAccessRights
///     UINT32   dwBlobSize     — conformance count for the conformant byte[] that follows
///     BYTE[dwBlobSize] pBlob  — via WriteConformantByteArray
/// </code>
/// </remarks>
public static class NdrOpcItemResultCodec
{
    /// <summary>Encodes a single OPCITEMRESULT in NDR.</summary>
    public static void Write(ref NdrWriter writer, OpcItemResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        writer.WriteUInt32(unchecked((uint)result.ServerHandle));
        writer.WriteUInt16((ushort)result.CanonicalDataType);
        writer.WriteUInt16(0);  // wReserved
        writer.WriteUInt32(unchecked((uint)result.AccessRights));
        writer.WriteConformantByteArray(result.Blob);
    }

    /// <summary>Decodes a single OPCITEMRESULT from NDR.</summary>
    public static OpcItemResult Read(ref NdrReader reader)
    {
        uint hServer = reader.ReadUInt32();
        var vtCanonical = (VarType)reader.ReadUInt16();
        _ = reader.ReadUInt16();  // wReserved
        uint dwAccessRights = reader.ReadUInt32();
        byte[] blob = reader.ReadConformantByteArray();

        return new OpcItemResult(
            ServerHandle: unchecked((int)hServer),
            CanonicalDataType: vtCanonical,
            AccessRights: unchecked((int)dwAccessRights),
            Blob: blob);
    }
}
