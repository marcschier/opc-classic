// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal.LegacyNdr;

namespace Opc.Classic.Dcom.Core;

[Serializable]
public sealed class ExtendedInterfacePointerBody : InterfacePointerBody
{
    internal ExtendedInterfacePointerBody(
        string iid,
        StdObjRef stdObjRef,
        DualStringArray stringBindings,
        IReadOnlyList<ObjRefExtension> extensions)
        : this(iid, stdObjRef, stringBindings, extensions, true, true)
    {
    }

    private ExtendedInterfacePointerBody(
        string iid,
        StdObjRef stdObjRef,
        DualStringArray stringBindings,
        IReadOnlyList<ObjRefExtension> extensions,
        bool hasSignature1,
        bool hasSignature2)
        : base(InterfacePointer.OBJREF_EXTENDED, iid, stdObjRef, stringBindings)
    {
        Extensions = extensions ?? Array.Empty<ObjRefExtension>();
        _hasSignature1 = hasSignature1;
        _hasSignature2 = hasSignature2;
        Length = GetEncodedLength();
    }

    public IReadOnlyList<ObjRefExtension> Extensions { get; }

    internal static ExtendedInterfacePointerBody Decode(NdrCodec ndr, string iid, int bodyLength)
    {
        var bodyStart = ndr.Buffer.Index;
        var stdObjRef = StdObjRef.Decode(ndr);
        var hasSignature1 = TryReadExtendedSignature(ndr);
        var stringBindings = DualStringArray.Decode(ndr);
        var extensions = DecodeExtensions(ndr, bodyStart, bodyLength, out var hasSignature2);
        return new ExtendedInterfacePointerBody(iid, stdObjRef, stringBindings, extensions, hasSignature1, hasSignature2);
    }

    protected override int GetEncodedLength()
    {
        var length = ObjRefHeaderLength + StdObjRefLength + StringBindings.Length;
        if (_hasSignature1)
        {
            length += 4;
        }
        length += GetNdrAlignmentPadding(8 + length, 4) + 4;
        if (_hasSignature2)
        {
            length += 4;
        }
        foreach (var extension in Extensions)
        {
            length += extension.Length;
        }
        return length;
    }

    protected override void EncodeBody(NdrCodec ndr, int flags)
    {
        ((StdObjRef)GetObjectReference(InterfacePointer.OBJREF_STANDARD)).Encode(ndr);
        if (_hasSignature1)
        {
            ndr.WriteUnsignedLong(ObjRefExtendedSignature);
        }
        StringBindings.Encode(ndr);
        ndr.WriteUnsignedLong(Extensions.Count);
        if (_hasSignature2)
        {
            ndr.WriteUnsignedLong(ObjRefExtendedSignature);
        }
        foreach (var extension in Extensions)
        {
            extension.Encode(ndr);
        }
    }

    private static bool TryReadExtendedSignature(NdrCodec ndr)
    {
        var signatureIndex = ndr.Buffer.Index;
        var signature = ndr.ReadUnsignedLong();
        if (signature == ObjRefExtendedSignature)
        {
            return true;
        }
        ndr.Buffer.Index = signatureIndex;
        return false;
    }

    private static IReadOnlyList<ObjRefExtension> DecodeExtensions(
        NdrCodec ndr,
        int bodyStart,
        int bodyLength,
        out bool hasSignature2)
    {
        hasSignature2 = true;
        if (GetRemainingBodyBytes(ndr, bodyStart, bodyLength) < 4)
        {
            return Array.Empty<ObjRefExtension>();
        }

        var count = ndr.ReadUnsignedLong();
        if (count < 0)
        {
            throw new NdrException("OBJREF_EXTENDED extension count was negative.");
        }

        if (GetRemainingBodyBytes(ndr, bodyStart, bodyLength) >= 4)
        {
            var signatureIndex = ndr.Buffer.Index;
            var signature = ndr.ReadUnsignedLong();
            if (signature != ObjRefExtendedSignature)
            {
                hasSignature2 = false;
                ndr.Buffer.Index = signatureIndex;
            }
        }
        else
        {
            hasSignature2 = false;
        }

        var extensions = new List<ObjRefExtension>(count);
        for (var i = 0; i < count; i++)
        {
            extensions.Add(ObjRefExtension.Decode(ndr));
        }
        return extensions;
    }

    private static int GetRemainingBodyBytes(NdrCodec ndr, int bodyStart, int bodyLength)
    {
        if (bodyLength > 0)
        {
            return Math.Max(0, bodyLength - (ndr.Buffer.Index - bodyStart));
        }
        return GetRemainingByteCount(ndr);
    }

    private readonly bool _hasSignature1;
    private readonly bool _hasSignature2;
}
