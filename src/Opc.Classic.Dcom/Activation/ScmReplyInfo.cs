//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Core;

/// <summary>SCM activation reply data with the returned OBJREF.</summary>
public sealed record ScmReplyInfo(int Hresult, Guid Oxid, Guid Oid, Guid Ipid, byte[] ObjRef)
{
    /// <summary>Creates a reply and defensively copies the OBJREF payload.</summary>
    public ScmReplyInfo(int hresult, Guid oxid, Guid oid, Guid ipid, byte[] objRef, bool copy)
        : this(hresult, oxid, oid, ipid, copy ? Copy(objRef) : objRef)
    {
    }

    private static byte[] Copy(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Length == 0 ? Array.Empty<byte>() : (byte[])value.Clone();
    }
}
