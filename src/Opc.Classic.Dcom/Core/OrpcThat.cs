// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal.LegacyNdr;

#pragma warning disable MA0051 // Legacy DCOM protocol methods are intentionally kept intact during analyzer cleanup.

namespace Opc.Classic.Dcom.Core;

[Serializable]
internal sealed class OrpcThat
{

    /// <summary>
    /// Create that
    /// </summary>
    private OrpcThat()
    {
    }

    /// <summary>
    /// Extent array
    /// </summary>
    public OrpcExtentArray[] ExtentArray { get; private set; }

    /// <summary>
    /// Returns an array of flags present (OrpcFlags).
    /// For now only 2 flags are returned to the user
    /// 0 and 1. Reserved flags are not returned.
    /// </summary>
    public int[] SupportedFlags
    {
        get
        {
            if (_flags == -1)
            {
                return null;
            }
            if ((_flags & 1) == 1)
            {
                return new int[] { 1 };
            }
            return new int[] { 0 };
        }
    }

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    internal static void Encode(NdrCodec ndr)
    {
        ndr.WriteUnsignedLong(0);
        ndr.WriteUnsignedLong(0);
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <returns>A new <see cref="OrpcThat"/> instance built from <paramref name="ndr"/>.</returns>
    internal static OrpcThat Decode(NdrCodec ndr)
    {
        var orpcthat = new OrpcThat
        {
            _flags = ndr.ReadUnsignedLong()
        };

        // to throw InteropRuntimeException from here.
        if (orpcthat._flags != (int)OrpcFlags.ORPCF_NULL &&
            orpcthat._flags != (int)OrpcFlags.ORPCF_LOCAL &&
            orpcthat._flags != (int)OrpcFlags.ORPCF_RESERVED1 &&
            orpcthat._flags != (int)OrpcFlags.ORPCF_RESERVED2 &&
            orpcthat._flags != (int)OrpcFlags.ORPCF_RESERVED3 &&
            orpcthat._flags != (int)OrpcFlags.ORPCF_RESERVED4)
        {
            throw new InteropRuntimeException(orpcthat._flags);
        }

        var orpcextentarray = new Struct();
        try
        {
            // create the orpcextent struct
            /*
             *  typedef struct tagORPC_EXTENT
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
             *    typedef struct tagORPC_EXTENT_ARRAY
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

        var context = new CodecContext
        {
            Flag = InteropFlags.FLAG_REPRESENTATION_ARRAY
        };
        var orpcextentarrayptr = (ComPointer)MarshalUnMarshalHelper.Deserialize(ndr, new ComPointer(orpcextentarray), context);
        System.Diagnostics.Debug.Assert(context.Flag == InteropFlags.FLAG_REPRESENTATION_ARRAY);
        context.DecodeDeferredPointers(ndr);

        var extentArrays = new List<OrpcExtentArray>();
        // now read whether extend array exists or not
        // int ptr = ndr.readUnsignedLong();
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

        orpcthat.ExtentArray = extentArrays.ToArray();
        return orpcthat;
    }
    private int _flags = -1;
}
