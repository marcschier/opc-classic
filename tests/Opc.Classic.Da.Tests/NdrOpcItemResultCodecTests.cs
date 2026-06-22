// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Da.Ndr;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Tests;

public sealed class NdrOpcItemResultCodecTests
{
    private delegate void NdrWriteAction(ref NdrWriter w);

    private static byte[] WriteOne(NdrWriteAction write, int capacity = 128)
    {
        var buf = new byte[capacity];
        var w = new NdrWriter(buf);
        write(ref w);
        return buf[..w.Position];
    }

    private static OpcItemResult ReadOne(byte[] bytes)
    {
        var r = new NdrReader(bytes);
        return NdrOpcItemResultCodec.Read(ref r);
    }

    [Test]
    public async Task RoundTrip_TypicalDoubleItem()
    {
        var input = new OpcItemResult(
            ServerHandle: 12345,
            CanonicalDataType: VarType.VT_R8,
            AccessRights: 3,
            Blob: Array.Empty<byte>());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemResultCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.ServerHandle).IsEqualTo(12345);
        await Assert.That(back.CanonicalDataType).IsEqualTo(VarType.VT_R8);
        await Assert.That(back.AccessRights).IsEqualTo(3);
        await Assert.That(back.Blob.Length).IsEqualTo(0);
    }

    [Test]
    public async Task RoundTrip_WithBstrCanonicalType()
    {
        var input = new OpcItemResult(
            ServerHandle: 7,
            CanonicalDataType: VarType.VT_BSTR,
            AccessRights: 1,
            Blob: Array.Empty<byte>());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemResultCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.CanonicalDataType).IsEqualTo(VarType.VT_BSTR);
    }

    [Test]
    public async Task RoundTrip_WithBlobPayload()
    {
        var blob = new byte[] { 0x01, 0x02, 0x03, 0xFF };
        var input = new OpcItemResult(
            ServerHandle: 99,
            CanonicalDataType: VarType.VT_I4,
            AccessRights: 3,
            Blob: blob);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemResultCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Blob.SequenceEqual(blob)).IsTrue();
    }

    [Test]
    public async Task ServerHandle_LayoutAtOffsetZero()
    {
        var input = new OpcItemResult(
            ServerHandle: unchecked((int)0xCAFEBABE),
            CanonicalDataType: VarType.VT_I4,
            AccessRights: 3,
            Blob: Array.Empty<byte>());
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemResultCodec.Write(ref w, input));
        await Assert.That(BitConverter.ToUInt32(bytes, 0)).IsEqualTo(0xCAFEBABEu);
    }

    [Test]
    public async Task NullBlob_TreatedAsEmpty()
    {
        // null Blob implicitly converts to ReadOnlySpan<byte>.Empty in
        // WriteConformantByteArray; both round-trip back as a 0-length byte[].
        var input = new OpcItemResult(1, VarType.VT_I4, 1, null!);
        var bytes = WriteOne((ref NdrWriter w) => NdrOpcItemResultCodec.Write(ref w, input));
        var back = ReadOne(bytes);
        await Assert.That(back.Blob.Length).IsEqualTo(0);
    }
}
