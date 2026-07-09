// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Rpc.Core;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Interface pointer body
/// </summary>
[Serializable]
public class InterfacePointerBody
{
    protected const int ObjRefHeaderLength = 24;
    protected const int StdObjRefLength = 40;
    protected const int CustomBodyHeaderLength = 24;
    protected const int ObjRefExtendedSignature = unchecked((int)0x4e535956);

    /// <summary>
    /// Create body
    /// </summary>
    protected InterfacePointerBody() { }

    /// <summary>
    /// Custom object
    /// </summary>
    public bool CustomObjRef => ObjectType == InterfacePointer.OBJREF_CUSTOM;

    /// <summary>
    /// Custom class id
    /// </summary>
    public string CustomCLSID { get; protected set; }

    /// <summary>
    /// Object type
    /// </summary>
    public int ObjectType { get; protected set; } = -1;

    /// <summary>
    /// Returns object reference
    /// </summary>
    /// <param name="type">COM or NDR type descriptor for the value being processed.</param>
    /// <returns>object reference</returns>
    public virtual object GetObjectReference(int type)
    {
        if (type == InterfacePointer.OBJREF_STANDARD && _stdObjRef != null)
        {
            return _stdObjRef;
        }
        if (type == ObjectType)
        {
            return this;
        }
        return null;
    }

    /// <summary>
    /// Returns the Interface Identifier for this MIP.
    /// </summary>
    /// <returns> String representation of 128 bit uuid. </returns>
    public string IID { get; protected set; }

    /// <summary>
    /// Ip id
    /// </summary>
    internal string IPID => _stdObjRef.Ipid;

    /// <summary>
    /// Oid
    /// </summary>
    internal byte[] OID => _stdObjRef.ObjectId;

    /// <summary>
    /// String bindings
    /// </summary>
    internal DualStringArray StringBindings { get; set; }

    /// <summary>
    /// Length
    /// </summary>
    public int Length { get; protected set; } = -1;

    /// <summary>
    /// Called from Oxid Resolver master, the resolver address are put in here itself
    /// </summary>
    /// <param name="iid">Interface IID identifying the COM interface being queried or marshaled.</param>
    /// <param name="port">Network port used by the RPC endpoint or string binding.</param>
    /// <param name="objref">OBJREF structure that carries the marshaled COM object reference.</param>
    internal InterfacePointerBody(string iid, int port, StdObjRef objref)
    {
        ObjectType = InterfacePointer.OBJREF_STANDARD;
        IID = iid;
        _stdObjRef = objref;
        _port = port;
        StringBindings = new DualStringArray(port);
        Length = GetStandardLength(StringBindings);
    }

    /// <summary>
    /// Create body
    /// </summary>
    /// <param name="iid">Interface IID identifying the COM interface being queried or marshaled.</param>
    /// <param name="interfacePointer">Marshaled interface pointer that describes the remote COM interface reference.</param>
    internal InterfacePointerBody(string iid, InterfacePointer interfacePointer)
    {
        ObjectType = InterfacePointer.OBJREF_STANDARD;
        IID = iid;
        _stdObjRef = (StdObjRef)interfacePointer.GetObjectReference(InterfacePointer.OBJREF_STANDARD);
        StringBindings = interfacePointer.StringBindings;
        Length = GetStandardLength(StringBindings);
    }

    internal InterfacePointerBody(int objectType, string iid, StdObjRef stdObjRef, DualStringArray stringBindings)
    {
        ObjectType = objectType;
        IID = iid;
        _stdObjRef = stdObjRef;
        StringBindings = stringBindings;
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="Flags">Bit flags stored in the serialized descriptor.</param>
    /// <returns>A new <see cref="InterfacePointerBody"/> instance built from <paramref name="ndr"/>.</returns>
    public static InterfacePointerBody Decode(NdrCodec ndr, int Flags)
    {
        if ((Flags & InteropFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) ==
                     InteropFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2)
        {
            return Decode2(ndr);
        }

        var length = ndr.ReadUnsignedLong();
        ndr.ReadUnsignedLong(); // length
        return DecodeObjRef(ndr, length);
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <returns>A new <see cref="InterfacePointerBody"/> instance built from <paramref name="ndr"/>.</returns>
    public static InterfacePointerBody Decode2(NdrCodec ndr) =>
        DecodeObjRef(ndr, GetRemainingByteCount(ndr));

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr">NDR buffer used to encode or decode the wire representation.</param>
    /// <param name="flags">Bit flags governing the requested operation.</param>
    public void Encode(NdrCodec ndr, int flags)
    {
        var length = GetEncodedLength();

        ndr.WriteUnsignedLong(length);
        ndr.WriteUnsignedLong(length);
        EncodeObjRef(ndr, flags);
        Length = length;
    }

    protected virtual int GetEncodedLength() => GetStandardLength(StringBindings);

    protected virtual void EncodeBody(NdrCodec ndr, int flags)
    {
        _stdObjRef.Encode(ndr);
        StringBindings.Encode(ndr);
    }

    private static int GetStandardLength(DualStringArray stringBindings) =>
        ObjRefHeaderLength + StdObjRefLength + stringBindings.Length;

    public static string ReadUuid(NdrCodec ndr, string logContext)
    {
        try
        {
            var uuid = new UUID();
            uuid.Decode(ndr, ndr.Buffer);
            return uuid.ToString();
        }
        catch (NdrException e)
        {
            Log.Logger.Error(e, logContext);
            return UUID.NIL_UUID;
        }
    }

    public static void WriteUuid(NdrCodec ndr, string value, string logContext)
    {
        try
        {
            var uuid = new UUID(value);
            uuid.Encode(ndr, ndr.Buffer);
            ndr.Buffer.SetLength(Math.Max(ndr.Buffer.Length, ndr.Buffer.Index - ndr.Buffer.Start));
        }
        catch (NdrException e)
        {
            Log.Logger.Error(e, logContext);
        }
    }

    internal static int GetRemainingByteCount(NdrCodec ndr)
    {
        var readableLimit = ndr.Buffer.Length > 0
            ? Math.Min(ndr.Buffer.Buf.Length, ndr.Buffer.Start + ndr.Buffer.Length)
            : ndr.Buffer.Buf.Length;
        return Math.Max(0, readableLimit - ndr.Buffer.Index);
    }

    internal static byte[] ReadRemainingBytes(NdrCodec ndr, int length)
    {
        var bytes = new byte[Math.Max(0, length)];
        if (bytes.Length > 0)
        {
            ndr.ReadOctetArray(bytes, 0, bytes.Length);
        }
        return bytes;
    }

    private static InterfacePointerBody DecodeObjRef(NdrCodec ndr, int length)
    {
        var objRefStart = ndr.Buffer.Index;
        if (!ReadSignature(ndr))
        {
            return null;
        }

        var objectType = ndr.ReadUnsignedLong();
        var iid = ReadUuid(ndr, "InterfacePointer decode");
        var bodyLength = GetBodyLength(ndr, objRefStart, length);

        InterfacePointerBody ptr;
        switch (objectType)
        {
            case InterfacePointer.OBJREF_STANDARD:
                ptr = DecodeStandardBody(ndr, iid, objectType);
                break;
            case InterfacePointer.OBJREF_HANDLER:
                ptr = HandlerInterfacePointerBody.Decode(ndr, iid, bodyLength);
                break;
            case InterfacePointer.OBJREF_CUSTOM:
                ptr = CustomInterfacePointerBody.Decode(ndr, iid, bodyLength);
                break;
            case InterfacePointer.OBJREF_EXTENDED:
                ptr = ExtendedInterfacePointerBody.Decode(ndr, iid, bodyLength);
                break;
            default:
                ptr = UnknownInterfacePointerBody.Decode(ndr, objectType, iid, bodyLength);
                break;
        }

        ptr.Length = length > 0 ? length : ndr.Buffer.Index - objRefStart;
        return ptr;
    }

    private static int GetBodyLength(NdrCodec ndr, int objRefStart, int length)
    {
        if (length > 0)
        {
            return Math.Max(0, length - (ndr.Buffer.Index - objRefStart));
        }
        return GetRemainingByteCount(ndr);
    }

    private static InterfacePointerBody DecodeStandardBody(NdrCodec ndr, string iid, int objectType)
    {
        var ptr = new InterfacePointerBody
        {
            ObjectType = objectType,
            IID = iid,
            _stdObjRef = StdObjRef.Decode(ndr)
        };
        ptr.StringBindings = DualStringArray.Decode(ndr);
        ptr.Length = ptr.GetEncodedLength();
        return ptr;
    }

    private static bool ReadSignature(NdrCodec ndr)
    {
        var b = new byte[4];
        ndr.ReadOctetArray(b, 0, 4);

        for (var i = 0; i != 4; i++)
        {
            if (b[i] != InterfacePointer.OBJREF_SIGNATURE[i])
            {
                return false;
            }
        }
        return true;
    }

    private void EncodeObjRef(NdrCodec ndr, int flags)
    {
        ndr.WriteOctetArray(InterfacePointer.OBJREF_SIGNATURE, 0, 4);
        ndr.WriteUnsignedLong(ObjectType);
        WriteUuid(ndr, GetIidForEncoding(flags), "InterfacePointer encode");
        EncodeBody(ndr, flags);
    }

    protected static int GetNdrAlignmentPadding(int index, int alignment)
    {
        var misaligned = index % alignment;
        return misaligned == 0 ? 0 : alignment - misaligned;
    }

    private string GetIidForEncoding(int flags)
    {
        if (ObjectType == InterfacePointer.OBJREF_CUSTOM)
        {
            return IID;
        }
        if ((flags & InteropFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID) ==
                     InteropFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID)
        {
            return Interfaces.IID_IUnknown;
        }
        if ((flags & InteropFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID) ==
                          InteropFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID)
        {
            return Interfaces.IID_IDispatch;
        }
        return IID;
    }

    private StdObjRef _stdObjRef;
#pragma warning disable IDE0052 // Remove unread private members
    private readonly int _port = -1; // to be used when doing local resolution.
#pragma warning restore IDE0052 // Remove unread private members
}
