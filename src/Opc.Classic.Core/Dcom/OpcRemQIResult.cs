// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom;

/// <summary>
/// One element of the <c>REMQIRESULT</c> array returned by
/// <c>IRemUnknown::RemQueryInterface</c> (MS-DCOM §2.2.19): per-IID HRESULT
/// plus the <c>STDOBJREF</c> identifying where on the original OXID to reach
/// the queried interface.
/// </summary>
/// <remarks>
/// STDOBJREF is the "small" form of OBJREF used inside <c>REMQIRESULT</c>:
/// it omits the DUALSTRINGARRAY OXID resolver bindings because the QI happens
/// inside an already-resolved OXID context. To reach the returned IPID, route
/// new calls over the same DCOM channel using the same OXID resolver bindings
/// as the parent OBJREF.
/// </remarks>
public sealed class OpcRemQIResult
{
    /// <summary>
    /// Creates a new RemQI result entry.
    /// </summary>
    public OpcRemQIResult(int hresult, uint flags, uint publicRefs, ulong oxid, ulong oid, Guid ipid)
    {
        Hresult = hresult;
        Flags = flags;
        PublicRefs = publicRefs;
        Oxid = oxid;
        Oid = oid;
        Ipid = ipid;
    }

    /// <summary>
    /// HRESULT for the IID's QI attempt (S_OK on success, E_NOINTERFACE on miss).
    /// </summary>
    public int Hresult { get; }

    /// <summary>
    /// STDOBJREF flags (typically 0).
    /// </summary>
    public uint Flags { get; }

    /// <summary>
    /// STDOBJREF cPublicRefs (number of references the server-side proxy must hold).
    /// </summary>
    public uint PublicRefs { get; }

    /// <summary>
    /// The OXID this IPID lives on (matches the parent OBJREF's OXID).
    /// </summary>
    public ulong Oxid { get; }

    /// <summary>
    /// OID — object identifier, distinct per object within the OXID.
    /// </summary>
    public ulong Oid { get; }

    /// <summary>
    /// IPID — interface pointer identifier; the routing key for subsequent calls.
    /// </summary>
    public Guid Ipid { get; }
}
