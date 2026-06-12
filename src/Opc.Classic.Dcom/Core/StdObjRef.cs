// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal;

namespace Opc.Classic.Dcom.Core;

[Serializable]
internal sealed class StdObjRef
{

    /// <summary>
    /// Flags
    /// </summary>
    public int Flags { get; private set; }

    /// <summary>
    /// Public refs
    /// </summary>
    public int PublicRefs { get; private set; } = -1;

    /// <summary>
    /// Oxid
    /// </summary>
    public byte[] Oxid { get; private set; }

    /// <summary>
    /// Object id
    /// </summary>
    public byte[] ObjectId { get; private set; }

    /// <summary>
    /// Ip id
    /// </summary>
    public string Ipid { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private StdObjRef()
    {
    }

    /// <summary>
    /// Resolver address are taken of localhost
    /// </summary>
    /// <param name="ipid">DCOM IPID identifying the per-interface object reference.</param>
    /// <param name="oxid">DCOM OXID identifying the object exporter process.</param>
    /// <param name="oid">DCOM OID identifying the exported object instance.</param>
    internal StdObjRef(string ipid, Oxid oxid, ObjectId oid)
    {
        Ipid = ipid;
        Oxid = oxid.OXID;
        ObjectId = oid.OID;
        PublicRefs = 5;
    }

    /// <summary>
    /// This is used to instantiate an empty StdObjRef for
    /// cases where the interface is not supported.
    /// </summary>
    /// <param name="ipid">DCOM IPID identifying the per-interface object reference.</param>
    internal StdObjRef(string ipid)
    {
        Ipid = ipid;
        Flags = 0x0;
        Oxid = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        ObjectId = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 };
        PublicRefs = 0;
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <returns>A new <see cref="StdObjRef"/> instance built from <paramref name="ndr"/>.</returns>
    internal static StdObjRef Decode(NdrCodec ndr)
    {
        var objRef = new StdObjRef
        {
            Flags = ndr.ReadUnsignedLong(),
            PublicRefs = ndr.ReadUnsignedLong(),
            Oxid = MarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8),
            ObjectId = MarshalUnMarshalHelper.ReadOctetArrayLE(ndr, 8)
        };
        try
        {
            var ipid2 = new UUID();
            ipid2.Decode(ndr, ndr.Buffer);
            objRef.Ipid = ipid2.ToString();
        }
        catch (NdrException e)
        {
            Log.Logger.Error(e, "StdObjRef decode");
        }
        return objRef;
    }

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    public void Encode(NdrCodec ndr)
    {
        ndr.WriteUnsignedLong(Flags);
        ndr.WriteUnsignedLong(PublicRefs);
        MarshalUnMarshalHelper.WriteOctetArrayLE(ndr, Oxid);
        MarshalUnMarshalHelper.WriteOctetArrayLE(ndr, ObjectId);
        try
        {
            var ipid = new UUID(Ipid);
            ipid.Encode(ndr, ndr.Buffer);
        }
        catch (NdrException e)
        {

            Log.Logger.Error(e, "StdObjRef encode");
        }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        var retVal = "IPID: " + Ipid; // + ", OID: " + oidString;
        return retVal;
    }
}
