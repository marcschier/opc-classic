//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom;

/// <summary>
/// NDR codec for the <c>REMQIRESULT</c> struct (MS-DCOM §2.2.19), returned as a
/// conformant array by <c>IRemUnknown::RemQueryInterface</c>:
/// <code>
/// typedef struct tagREMQIRESULT {
///   HRESULT  hResult;
///   STDOBJREF std;  // flags + cPublicRefs + OXID + OID + IPID
/// } REMQIRESULT;
/// </code>
/// </summary>
/// <remarks>
/// 4 (HRESULT) + 4 (flags) + 4 (cPublicRefs) + 8 (OXID) + 8 (OID) + 16 (IPID)
/// = 44 bytes per element. NdrWriter auto-aligns the 8-byte OXID/OID writes.
/// </remarks>
public static class NdrRemQIResultCodec
{
    /// <summary>Encodes a single REMQIRESULT (used by managed-server loopback fakes).</summary>
    public static void Write(ref NdrWriter writer, OpcRemQIResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        writer.WriteInt32(result.Hresult);
        writer.WriteUInt32(result.Flags);
        writer.WriteUInt32(result.PublicRefs);
        writer.WriteUInt64(result.Oxid);
        writer.WriteUInt64(result.Oid);
        writer.WriteGuid(result.Ipid);
    }

    /// <summary>Decodes a single REMQIRESULT element.</summary>
    public static OpcRemQIResult Read(ref NdrReader reader)
    {
        int hresult = reader.ReadInt32();
        uint flags = reader.ReadUInt32();
        uint publicRefs = reader.ReadUInt32();
        ulong oxid = reader.ReadUInt64();
        ulong oid = reader.ReadUInt64();
        Guid ipid = reader.ReadGuid();
        return new OpcRemQIResult(hresult, flags, publicRefs, oxid, oid, ipid);
    }
}
