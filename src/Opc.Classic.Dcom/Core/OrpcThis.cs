// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;

#pragma warning disable MA0051 // Legacy DCOM protocol methods are intentionally kept intact during analyzer cleanup.

namespace Opc.Classic.Dcom.Core;

[Serializable]
internal sealed class OrpcThis
{

    /// <summary>
    /// Create orpcthis
    /// </summary>
    public OrpcThis() =>
        CasualityIdentifier = Guid.NewGuid().ToString();

#pragma warning disable RECS0154 // Parameter is never used
    /// <summary>
    /// Create orpcthis
    /// </summary>
    /// <param name="casualityIdentifier">ORPC causality identifier used to correlate the call chain.</param>
    public OrpcThis(UUID casualityIdentifier) =>
#pragma warning restore RECS0154 // Parameter is never used
        CasualityIdentifier = casualityIdentifier.ToString();

    /// <summary>
    /// Flags
    /// </summary>
    public int ORPCFlags { set; get; }

    /// <summary>
    /// Extent array
    /// </summary>
    public OrpcExtentArray[] ExtentArray { set; get; }

    /// <summary>
    /// Cid
    /// </summary>
    public string CasualityIdentifier { get; private set; }

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    public void Encode(NdrCodec ndr)
    {
        ndr.WriteUnsignedShort(_version.MajorVersion); // COM Major version
        ndr.WriteUnsignedShort(_version.MinorVersion); // COM minor version
        ndr.WriteUnsignedLong(ORPCFlags); // No Flags
        ndr.WriteUnsignedLong(0); // Reserved ...always 0.

        // the order here is important since the cid is always filled from the ctor hence will never be null.
        var cid2 = kCidForCallback.Value ?? CasualityIdentifier;
        var uuid = new UUID(cid2);
        try
        {
            uuid.Encode(ndr, ndr.Buffer);
        }
        catch (NdrException e)
        {
            Log.Logger.Error(e, "OrpcThis encode");
        }

        var i = 0;
        if (ExtentArray != null && ExtentArray.Length != 0)
        {
            ndr.WriteUnsignedLong(ExtentArray.Length);
            ndr.WriteUnsignedLong(0);
            while (i < ExtentArray.Length)
            {
                var arryy = ExtentArray[i];
                uuid = new UUID(arryy.GUID);
                try
                {
                    uuid.Encode(ndr, ndr.Buffer);
                }
                catch (NdrException e)
                {
                    Log.Logger.Error(e, "OrpcThis encode");
                }

                ndr.WriteUnsignedLong(arryy.SizeOfData);
                ndr.WriteOctetArray(arryy.Data, 0, arryy.SizeOfData);
                i++;
            }
        }
        else
        {
            ndr.WriteUnsignedLong(0);
        }
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <returns>A new <see cref="OrpcThis"/> instance built from <paramref name="ndr"/>.</returns>
    internal static OrpcThis Decode(NdrCodec ndr)
    {

        var retval = new OrpcThis();
        var context = new CodecContext();
        var majorVersion = (int)(short)MarshalUnMarshalHelper.Deserialize(ndr, typeof(short), context);
        var minorVersion = (int)(short)MarshalUnMarshalHelper.Deserialize(ndr, typeof(short), context);
        retval._version = new ComVersion(majorVersion, minorVersion);
        retval.ORPCFlags = (int)MarshalUnMarshalHelper.Deserialize(ndr, typeof(int), context);

        MarshalUnMarshalHelper.Deserialize(ndr, typeof(int), context); // reserved.

        var uuid = new UUID();
        try
        {
            uuid.Decode(ndr, ndr.Buffer);
            retval.CasualityIdentifier = uuid.ToString();
        }
        catch (NdrException e)
        {
            Log.Logger.Error(e, "OrpcThis decode");
        }


        var orpcextentarray = new Struct();
        try
        {
            // create the orpcextent struct
            /*
             typedef struct tagORPC_EXTENT
        {
            GUID                    id;          // Extension identifier.
            unsigned long           size;        // Extension size.
            [size_is((size+7)&~7)]  byte data[]; // Extension data.
        } ORPC_EXTENT;

             */
            var orpcextent = new Struct();
            orpcextent.AddMember(typeof(UUID));
            orpcextent.AddMember(typeof(int)); // length
            orpcextent.AddMember(new ComArray(typeof(sbyte), null, 1, true));
            // create the orpcextentarray struct
            /*
             typedef struct tagORPC_EXTENT_ARRAY
        {
            unsigned long size;     // Num extents.
            unsigned long reserved; // Must be zero.
            [size_is((size+1)&~1,), unique] ORPC_EXTENT **extent; // extents
        } ORPC_EXTENT_ARRAY;
             */
            orpcextentarray.AddMember(typeof(int));
            orpcextentarray.AddMember(typeof(int));
            // this is since the pointer is [unique]
            orpcextentarray.AddMember(new ComPointer(new ComArray(new ComPointer(orpcextent), null, 1, true)));
        }
        catch (InteropException)
        {
            // this won't fail...i am certain :)...
        }

        var orpcextentarrayptr = (ComPointer)MarshalUnMarshalHelper.Deserialize(ndr,
            new ComPointer(orpcextentarray), context);
        context.DecodeDeferredPointers(ndr);

        var extentArrays = new List<OrpcExtentArray>();
        // now read whether extend array exists or not
        if (!orpcextentarrayptr.IsNull)
        {
            var pointers = (ComPointer[])((ComArray)((ComPointer)((Struct)orpcextentarrayptr.Referent).GetMember(2)).Referent).ArrayInstance;
            for (var i = 0; i < pointers.Length; i++)
            {
                if (pointers[i].IsNull)
                {
                    continue;
                }

                var orpcextent2 = (Struct)pointers[i].Referent;
                var byteArray = (byte[])((ComArray)orpcextent2.GetMember(2)).ArrayInstance;

                extentArrays.Add(new OrpcExtentArray(((UUID)orpcextent2.GetMember(0)).ToString(), byteArray.Length, byteArray));
            }

        }
        retval.ExtentArray = extentArrays.ToArray();
        // decode can only be executed incase of a request made from the
        // server side in case of a callback. so the thread making this
        // callback will store the cid from the decode operation in the 
        // threadlocal variable. In case an encode is performed using the
        // same thread then we know that this is a nested call. Hence will 
        // replace the cid with the thread local cid. For the calls being in
        // case of encode this value will not be used if the encode thread 
        // is of the client and not of ComOxidRuntimeHelper.
        kCidForCallback.Value = retval.CasualityIdentifier;
        return retval;
    }

    private static readonly ThreadLocal<string> kCidForCallback = new ThreadLocal<string>();
    private ComVersion _version = Interop.COMVersion;
}
