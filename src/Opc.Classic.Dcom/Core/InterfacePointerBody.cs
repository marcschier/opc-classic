//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Internal;
using SharpInterop.Rpc.Core;
using System;
using System.Collections.Generic;

namespace SharpInterop.Core; 
/// <summary>
/// Interface pointer body
/// </summary>
[Serializable]
public class InterfacePointerBody {
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
    /// <param name="type"></param>
    /// <returns>object reference</returns>
    public virtual object GetObjectReference(int type) {
        if (type == InterfacePointer.OBJREF_STANDARD && _stdObjRef != null) {
            return _stdObjRef;
        }
        if (type == ObjectType) {
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
    /// <param name="iid"> </param>
    /// <param name="port"></param>
    /// <param name="objref"></param>
    internal InterfacePointerBody(string iid, int port, StdObjRef objref) {
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
    /// <param name="iid"></param>
    /// <param name="interfacePointer"></param>
    internal InterfacePointerBody(string iid, InterfacePointer interfacePointer) {
        ObjectType = InterfacePointer.OBJREF_STANDARD;
        IID = iid;
        _stdObjRef = (StdObjRef)interfacePointer.GetObjectReference(InterfacePointer.OBJREF_STANDARD);
        StringBindings = interfacePointer.StringBindings;
        Length = GetStandardLength(StringBindings);
    }

    internal InterfacePointerBody(int objectType, string iid, StdObjRef stdObjRef, DualStringArray stringBindings) {
        ObjectType = objectType;
        IID = iid;
        _stdObjRef = stdObjRef;
        StringBindings = stringBindings;
    }

    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr"></param>
    /// <param name="Flags"></param>
    /// <returns></returns>
    public static InterfacePointerBody Decode(NdrCodec ndr, int Flags) {
        if ((Flags & InteropFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) ==
                     InteropFlags.FLAG_REPRESENTATION_INTERFACEPTR_DECODE2) {
            return Decode2(ndr);
        }

        var length = ndr.ReadUnsignedLong();
        ndr.ReadUnsignedLong(); // length
        return DecodeObjRef(ndr, length);
    }


    /// <summary>
    /// Decode
    /// </summary>
    /// <param name="ndr"></param>
    /// <returns></returns>
    public static InterfacePointerBody Decode2(NdrCodec ndr) =>
        DecodeObjRef(ndr, GetRemainingByteCount(ndr));

    /// <summary>
    /// Encode
    /// </summary>
    /// <param name="ndr"></param>
    /// <param name="flags"></param>
    public void Encode(NdrCodec ndr, int flags) {
        var length = GetEncodedLength();

        ndr.WriteUnsignedLong(length);
        ndr.WriteUnsignedLong(length);
        EncodeObjRef(ndr, flags);
        Length = length;
    }

    protected virtual int GetEncodedLength() => GetStandardLength(StringBindings);

    protected virtual void EncodeBody(NdrCodec ndr, int flags) {
        _stdObjRef.Encode(ndr);
        StringBindings.Encode(ndr);
    }

    private static int GetStandardLength(DualStringArray stringBindings) =>
        ObjRefHeaderLength + StdObjRefLength + stringBindings.Length;

    public static string ReadUuid(NdrCodec ndr, string logContext) {
        try {
            var uuid = new UUID();
            uuid.Decode(ndr, ndr.Buffer);
            return uuid.ToString();
        }
        catch (NdrException e) {
            Log.Logger.Error(e, logContext);
            return UUID.NIL_UUID;
        }
    }

    public static void WriteUuid(NdrCodec ndr, string value, string logContext) {
        try {
            var uuid = new UUID(value);
            uuid.Encode(ndr, ndr.Buffer);
            ndr.Buffer.SetLength(Math.Max(ndr.Buffer.Length, ndr.Buffer.Index - ndr.Buffer.Start));
        }
        catch (NdrException e) {
            Log.Logger.Error(e, logContext);
        }
    }

    internal static int GetRemainingByteCount(NdrCodec ndr) {
        var readableLimit = ndr.Buffer.Length > 0
            ? Math.Min(ndr.Buffer.Buf.Length, ndr.Buffer.Start + ndr.Buffer.Length)
            : ndr.Buffer.Buf.Length;
        return Math.Max(0, readableLimit - ndr.Buffer.Index);
    }

    internal static byte[] ReadRemainingBytes(NdrCodec ndr, int length) {
        var bytes = new byte[Math.Max(0, length)];
        if (bytes.Length > 0) {
            ndr.ReadOctetArray(bytes, 0, bytes.Length);
        }
        return bytes;
    }

    private static InterfacePointerBody DecodeObjRef(NdrCodec ndr, int length) {
        var objRefStart = ndr.Buffer.Index;
        if (!ReadSignature(ndr)) {
            return null;
        }

        var objectType = ndr.ReadUnsignedLong();
        var iid = ReadUuid(ndr, "InterfacePointer decode");
        var bodyLength = GetBodyLength(ndr, objRefStart, length);

        InterfacePointerBody ptr;
        switch (objectType) {
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

    private static int GetBodyLength(NdrCodec ndr, int objRefStart, int length) {
        if (length > 0) {
            return Math.Max(0, length - (ndr.Buffer.Index - objRefStart));
        }
        return GetRemainingByteCount(ndr);
    }

    private static InterfacePointerBody DecodeStandardBody(NdrCodec ndr, string iid, int objectType) {
        var ptr = new InterfacePointerBody {
            ObjectType = objectType,
            IID = iid,
            _stdObjRef = StdObjRef.Decode(ndr)
        };
        ptr.StringBindings = DualStringArray.Decode(ndr);
        ptr.Length = ptr.GetEncodedLength();
        return ptr;
    }

    private static bool ReadSignature(NdrCodec ndr) {
        var b = new byte[4];
        ndr.ReadOctetArray(b, 0, 4);

        for (var i = 0; i != 4; i++) {
            if (b[i] != InterfacePointer.OBJREF_SIGNATURE[i]) {
                return false;
            }
        }
        return true;
    }

    private void EncodeObjRef(NdrCodec ndr, int flags) {
        ndr.WriteOctetArray(InterfacePointer.OBJREF_SIGNATURE, 0, 4);
        ndr.WriteUnsignedLong(ObjectType);
        WriteUuid(ndr, GetIidForEncoding(flags), "InterfacePointer encode");
        EncodeBody(ndr, flags);
    }

    protected static int GetNdrAlignmentPadding(int index, int alignment) {
        var misaligned = index % alignment;
        return misaligned == 0 ? 0 : alignment - misaligned;
    }

    private string GetIidForEncoding(int flags) {
        if (ObjectType == InterfacePointer.OBJREF_CUSTOM) {
            return IID;
        }
        if ((flags & InteropFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID) ==
                     InteropFlags.FLAG_REPRESENTATION_USE_IUNKNOWN_IID) {
            return Interfaces.IID_IUnknown;
        }
        if ((flags & InteropFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID) ==
                          InteropFlags.FLAG_REPRESENTATION_USE_IDISPATCH_IID) {
            return Interfaces.IID_IDispatch;
        }
        return IID;
    }

    private StdObjRef _stdObjRef;
#pragma warning disable IDE0052 // Remove unread private members
    private readonly int _port = -1; // to be used when doing local resolution.
#pragma warning restore IDE0052 // Remove unread private members
}

[Serializable]
public sealed class HandlerInterfacePointerBody : InterfacePointerBody {
    internal HandlerInterfacePointerBody(string iid, StdObjRef stdObjRef, string handlerClsid, DualStringArray stringBindings)
        : base(InterfacePointer.OBJREF_HANDLER, iid, stdObjRef, stringBindings) {
        HandlerCLSID = handlerClsid;
        Length = GetEncodedLength();
    }

    public string HandlerCLSID { get; private set; }

    internal static HandlerInterfacePointerBody Decode(NdrCodec ndr, string iid, int bodyLength) {
        var stdObjRef = StdObjRef.Decode(ndr);
        var handlerClsid = ReadUuid(ndr, "OBJREF_HANDLER decode");
        var stringBindings = DualStringArray.Decode(ndr);
        var body = new HandlerInterfacePointerBody(iid, stdObjRef, handlerClsid, stringBindings);
        body.Length = ObjRefHeaderLength + bodyLength;
        return body;
    }

    protected override int GetEncodedLength() =>
        ObjRefHeaderLength + StdObjRefLength + 16 + StringBindings.Length;

    protected override void EncodeBody(NdrCodec ndr, int flags) {
        ((StdObjRef)GetObjectReference(InterfacePointer.OBJREF_STANDARD)).Encode(ndr);
        WriteUuid(ndr, HandlerCLSID, "OBJREF_HANDLER encode");
        StringBindings.Encode(ndr);
    }
}

[Serializable]
public sealed class CustomInterfacePointerBody : InterfacePointerBody {
    internal CustomInterfacePointerBody(
        string iid,
        string clsid,
        int cbExtension,
        int reserved,
        byte[] objectData)
        : base(InterfacePointer.OBJREF_CUSTOM, iid, null, null) {
        CustomCLSID = clsid;
        ExtensionSize = cbExtension;
        Reserved = reserved;
        ObjectData = objectData ?? Array.Empty<byte>();
        Length = GetEncodedLength();
    }

    public string CLSID => CustomCLSID;

    public int ExtensionSize { get; private set; }

    public int Reserved { get; private set; }

    public byte[] ObjectData { get; private set; }

    internal static CustomInterfacePointerBody Decode(NdrCodec ndr, string iid, int bodyLength) {
        var clsid = ReadUuid(ndr, "OBJREF_CUSTOM decode");
        var cbExtension = ndr.ReadUnsignedLong();
        var reserved = ndr.ReadUnsignedLong();
        var objectDataLength = bodyLength > 0
            ? Math.Max(0, bodyLength - CustomBodyHeaderLength)
            : GetRemainingByteCount(ndr);
        var objectData = ReadRemainingBytes(ndr, objectDataLength);
        return new CustomInterfacePointerBody(iid, clsid, cbExtension, reserved, objectData);
    }

    protected override int GetEncodedLength() =>
        ObjRefHeaderLength + CustomBodyHeaderLength + ObjectData.Length;

    protected override void EncodeBody(NdrCodec ndr, int flags) {
        WriteUuid(ndr, CustomCLSID, "OBJREF_CUSTOM encode");
        ndr.WriteUnsignedLong(ExtensionSize);
        ndr.WriteUnsignedLong(Reserved);
        if (ObjectData.Length > 0) {
            ndr.WriteOctetArray(ObjectData, 0, ObjectData.Length);
        }
    }
}

[Serializable]
public sealed class ExtendedInterfacePointerBody : InterfacePointerBody {
    internal ExtendedInterfacePointerBody(
        string iid,
        StdObjRef stdObjRef,
        DualStringArray stringBindings,
        IReadOnlyList<ObjRefExtension> extensions)
        : this(iid, stdObjRef, stringBindings, extensions, true, true) {
    }

    private ExtendedInterfacePointerBody(
        string iid,
        StdObjRef stdObjRef,
        DualStringArray stringBindings,
        IReadOnlyList<ObjRefExtension> extensions,
        bool hasSignature1,
        bool hasSignature2)
        : base(InterfacePointer.OBJREF_EXTENDED, iid, stdObjRef, stringBindings) {
        Extensions = extensions ?? Array.Empty<ObjRefExtension>();
        _hasSignature1 = hasSignature1;
        _hasSignature2 = hasSignature2;
        Length = GetEncodedLength();
    }

    public IReadOnlyList<ObjRefExtension> Extensions { get; }

    internal static ExtendedInterfacePointerBody Decode(NdrCodec ndr, string iid, int bodyLength) {
        var bodyStart = ndr.Buffer.Index;
        var stdObjRef = StdObjRef.Decode(ndr);
        var hasSignature1 = TryReadExtendedSignature(ndr);
        var stringBindings = DualStringArray.Decode(ndr);
        var extensions = DecodeExtensions(ndr, bodyStart, bodyLength, out var hasSignature2);
        return new ExtendedInterfacePointerBody(iid, stdObjRef, stringBindings, extensions, hasSignature1, hasSignature2);
    }

    protected override int GetEncodedLength() {
        var length = ObjRefHeaderLength + StdObjRefLength + StringBindings.Length;
        if (_hasSignature1) {
            length += 4;
        }
        length += GetNdrAlignmentPadding(8 + length, 4) + 4;
        if (_hasSignature2) {
            length += 4;
        }
        foreach (var extension in Extensions) {
            length += extension.Length;
        }
        return length;
    }

    protected override void EncodeBody(NdrCodec ndr, int flags) {
        ((StdObjRef)GetObjectReference(InterfacePointer.OBJREF_STANDARD)).Encode(ndr);
        if (_hasSignature1) {
            ndr.WriteUnsignedLong(ObjRefExtendedSignature);
        }
        StringBindings.Encode(ndr);
        ndr.WriteUnsignedLong(Extensions.Count);
        if (_hasSignature2) {
            ndr.WriteUnsignedLong(ObjRefExtendedSignature);
        }
        foreach (var extension in Extensions) {
            extension.Encode(ndr);
        }
    }

    private static bool TryReadExtendedSignature(NdrCodec ndr) {
        var signatureIndex = ndr.Buffer.Index;
        var signature = ndr.ReadUnsignedLong();
        if (signature == ObjRefExtendedSignature) {
            return true;
        }
        ndr.Buffer.Index = signatureIndex;
        return false;
    }

    private static IReadOnlyList<ObjRefExtension> DecodeExtensions(
        NdrCodec ndr,
        int bodyStart,
        int bodyLength,
        out bool hasSignature2) {
        hasSignature2 = true;
        if (GetRemainingBodyBytes(ndr, bodyStart, bodyLength) < 4) {
            return Array.Empty<ObjRefExtension>();
        }

        var count = ndr.ReadUnsignedLong();
        if (count < 0) {
            throw new NdrException("OBJREF_EXTENDED extension count was negative.");
        }

        if (GetRemainingBodyBytes(ndr, bodyStart, bodyLength) >= 4) {
            var signatureIndex = ndr.Buffer.Index;
            var signature = ndr.ReadUnsignedLong();
            if (signature != ObjRefExtendedSignature) {
                hasSignature2 = false;
                ndr.Buffer.Index = signatureIndex;
            }
        }
        else {
            hasSignature2 = false;
        }

        var extensions = new List<ObjRefExtension>(count);
        for (var i = 0; i < count; i++) {
            extensions.Add(ObjRefExtension.Decode(ndr));
        }
        return extensions;
    }

    private static int GetRemainingBodyBytes(NdrCodec ndr, int bodyStart, int bodyLength) {
        if (bodyLength > 0) {
            return Math.Max(0, bodyLength - (ndr.Buffer.Index - bodyStart));
        }
        return GetRemainingByteCount(ndr);
    }

    private readonly bool _hasSignature1;
    private readonly bool _hasSignature2;
}

[Serializable]
public sealed class UnknownInterfacePointerBody : InterfacePointerBody {
    internal UnknownInterfacePointerBody(int objectType, string iid, byte[] rawBytes)
        : base(objectType, iid, null, null) {
        RawBytes = rawBytes ?? Array.Empty<byte>();
        Length = GetEncodedLength();
    }

    public byte[] RawBytes { get; }

    internal static UnknownInterfacePointerBody Decode(NdrCodec ndr, int objectType, string iid, int bodyLength) =>
        new UnknownInterfacePointerBody(objectType, iid, ReadRemainingBytes(ndr, bodyLength));

    protected override int GetEncodedLength() => ObjRefHeaderLength + RawBytes.Length;

    protected override void EncodeBody(NdrCodec ndr, int flags) {
        if (RawBytes.Length > 0) {
            ndr.WriteOctetArray(RawBytes, 0, RawBytes.Length);
        }
    }
}

[Serializable]
public sealed class ObjRefExtension {
    public ObjRefExtension(string dataId, byte[] payload)
        : this(dataId, payload?.Length ?? 0, RoundUpToEight(payload?.Length ?? 0), PadToEight(payload)) {
    }

    private ObjRefExtension(string dataId, int size, int roundedSize, byte[] data) {
        DataId = dataId;
        Size = size;
        RoundedSize = roundedSize;
        Data = data ?? Array.Empty<byte>();
    }

    public string DataId { get; }

    public int Size { get; }

    public int RoundedSize { get; }

    public byte[] Data { get; }

    public int Length => 16 + 4 + 4 + RoundedSize;

    internal static ObjRefExtension Decode(NdrCodec ndr) {
        var dataId = InterfacePointerBody.ReadUuid(ndr, "OBJREF_EXTENDED extension decode");
        var size = ndr.ReadUnsignedLong();
        var roundedSize = ndr.ReadUnsignedLong();
        var data = InterfacePointerBody.ReadRemainingBytes(ndr, roundedSize);
        return new ObjRefExtension(dataId, size, roundedSize, data);
    }

    internal void Encode(NdrCodec ndr) {
        InterfacePointerBody.WriteUuid(ndr, DataId, "OBJREF_EXTENDED extension encode");
        ndr.WriteUnsignedLong(Size);
        ndr.WriteUnsignedLong(RoundedSize);
        if (Data.Length > 0) {
            ndr.WriteOctetArray(Data, 0, Data.Length);
        }
    }

    private static int RoundUpToEight(int length) => (length + 7) & ~7;

    private static byte[] PadToEight(byte[] payload) {
        if (payload == null || payload.Length == 0) {
            return Array.Empty<byte>();
        }
        var data = new byte[RoundUpToEight(payload.Length)];
        Buffer.BlockCopy(payload, 0, data, 0, payload.Length);
        return data;
    }
}
