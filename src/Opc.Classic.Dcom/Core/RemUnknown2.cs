// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Remote unknown
/// </summary>
internal sealed class RemUnknown2 : NdrOp
{
    /// <summary>
    /// Interface pointer
    /// </summary>
    public InterfacePointer InterfacePointer { get; private set; }

    /// <inheritdoc/>
    public override int Opnum => 6;
    // opnum is 3 as this is a COM interface and 0,1,2 are occupied by IUnknown
    // 3,4,5 by IRemUnknown and we are going to call IRemUnknown2.QI so that we 
    // get MIPs.

    /// <summary>
    /// Create unknown
    /// </summary>
    /// <param name="ipidOfIUnknown">IPID of the remote IUnknown interface being managed.</param>
    /// <param name="requestedIID">Interface IID requested by the remote caller.</param>
    internal RemUnknown2(string ipidOfIUnknown, string requestedIID)
    {
        _ipidOfIUnknown = ipidOfIUnknown;
        _requestedIID = requestedIID;
    }

    /// <inheritdoc/>
    public override void Write(NdrCodec ndr)
    {
        var orpcthis = new OrpcThis();
        orpcthis.Encode(ndr);

        // now write the IPID
        var uuid = new UUID(_ipidOfIUnknown);
        try
        {
            uuid.Encode(ndr, ndr.Buffer);
        }
        catch (NdrException e)
        {
            Log.Logger.Error(e, "RemUnknown2 write");
        }

        ndr.WriteUnsignedShort(1); // 1 interfaces. (requested IID)
        ndr.WriteUnsignedShort(0); // byte alignment
        ndr.WriteUnsignedLong(1); // length of the array
        uuid = new UUID(_requestedIID);
        try
        {
            uuid.Encode(ndr, ndr.Buffer);
        }
        catch (NdrException e)
        {
            Log.Logger.Error(e, "RemUnknown2 Performing a QueryInterface for " +
                _requestedIID);
        }

        ndr.WriteUnsignedLong(0);
        // TODO Index Matching, there seems to be a bug in
        // the jarapac system, it only reads upto (length - 6) bytes and one has to have another
        // call after that or incomplete request will go.
        // in case no param is present just put an unsigned long = 0.
    }

    /// <inheritdoc/>
    public override void Read(NdrCodec ndr)
    {
        OrpcThat.Decode(ndr);
        ndr.ReadUnsignedLong(); // size will be one
        var hresult1 = ndr.ReadUnsignedLong();
        if (hresult1 != 0)
        {
            // something happened.
            throw new InteropRuntimeException(hresult1);
        }
        // array length
        ndr.ReadUnsignedLong();
        // and now the <see cref="InterfacePointer"/> itself.
        InterfacePointer = InterfacePointer.Decode(ndr, new CodecContext());
        // final hresult
        hresult1 = ndr.ReadUnsignedLong();
        if (hresult1 != 0)
        {
            // something happened.
            throw new InteropRuntimeException(hresult1);
        }
    }

    private readonly string _ipidOfIUnknown;
    private readonly string _requestedIID;
}
