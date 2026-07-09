// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;

namespace Opc.Classic.Dcom.Tests.Transport;

public sealed class RpcPduCodecRegressionTests
{
    [Test]
    public async Task Auth3Pdu_Encode_WritesHeaderAndZeroBody()
    {
        var pdu = new Auth3Pdu { CallId = 0 };

        byte[] encoded = PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);

        await Assert.That(Convert.ToHexString(encoded)).IsEqualTo("0500100310000000140000000000000000000000");
        await Assert.That(pdu.FragmentLength).IsEqualTo(20);
    }

    [Test]
    public async Task Auth3Pdu_Decode_RoundTripsHeaderFields()
    {
        byte[] encoded = Convert.FromHexString("0500100310000000140000007856341200000000");

        var decoded = (Auth3Pdu)PduCodec.DecodePdu(encoded);

        await Assert.That(decoded.Type).IsEqualTo(Auth3Pdu.AUTH3_TYPE);
        await Assert.That(decoded.CallId).IsEqualTo(0x12345678);
        await Assert.That(decoded.FragmentLength).IsEqualTo(20);
        await Assert.That(decoded.AuthenticatorLength).IsEqualTo(0);
    }

    [Test]
    public async Task FaultCoPdu_EncodeWithStub_WritesAlignedBodyAndStub()
    {
        var pdu = new FaultCoPdu
        {
            CallId = 0x01020304,
            AllocationHint = 0x11223344,
            ContextId = 0x5566,
            CancelCount = 0x77,
            Status = FaultCode.OPERATION_RANGE_ERROR,
            Stub = [0xDE, 0xAD, 0xBE],
        };

        byte[] encoded = PduCodec.EncodePdu(pdu, ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE);

        await Assert.That(Convert.ToHexString(encoded)).IsEqualTo(
            "05000303" +
            "10000000" +
            "23000000" +
            "04030201" +
            "44332211" +
            "6655" +
            "77" +
            "00" +
            "0200011C" +
            "00000000" +
            "DEADBE");
        await Assert.That(pdu.FragmentLength).IsEqualTo(35);
    }

    [Test]
    public async Task FaultCoPdu_DecodeWithStub_RoundTripsFields()
    {
        byte[] encoded = Convert.FromHexString(
            "05000303" +
            "10000000" +
            "23000000" +
            "04030201" +
            "44332211" +
            "6655" +
            "77" +
            "00" +
            "0200011C" +
            "00000000" +
            "DEADBE");

        var decoded = (FaultCoPdu)PduCodec.DecodePdu(encoded);

        await Assert.That(decoded.AllocationHint).IsEqualTo(0x11223344);
        await Assert.That(decoded.ContextId).IsEqualTo(0x5566);
        await Assert.That(decoded.CancelCount).IsEqualTo(0x77);
        await Assert.That(decoded.Status).IsEqualTo(FaultCode.OPERATION_RANGE_ERROR);
        await Assert.That(Convert.ToHexString(decoded.Stub)).IsEqualTo("DEADBE");
    }

    [Test]
    public async Task FaultCoPdu_GetFragments_SplitsStubAndFlags()
    {
        byte[] stub = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19];
        var pdu = new FaultCoPdu
        {
            CallId = 9,
            AllocationHint = stub.Length,
            Stub = stub,
        };

        List<FaultCoPdu> fragments = CollectFragments(pdu, 32);

        await Assert.That(fragments.Count).IsEqualTo(3);
        await Assert.That(fragments[0].AllocationHint).IsEqualTo(20);
        await Assert.That(Convert.ToHexString(fragments[0].Stub)).IsEqualTo("0001020304050607");
        await Assert.That(fragments[0].Flags).IsEqualTo(ConnectionOrientedPdu.PFC_FIRST_FRAG);
        await Assert.That(fragments[1].AllocationHint).IsEqualTo(12);
        await Assert.That(Convert.ToHexString(fragments[1].Stub)).IsEqualTo("08090A0B0C0D0E0F");
        await Assert.That(fragments[1].Flags).IsEqualTo(0);
        await Assert.That(fragments[2].AllocationHint).IsEqualTo(4);
        await Assert.That(Convert.ToHexString(fragments[2].Stub)).IsEqualTo("10111213");
        await Assert.That(fragments[2].Flags).IsEqualTo(ConnectionOrientedPdu.PFC_LAST_FRAG);
    }

    [Test]
    public async Task FaultCoPdu_Reassemble_CombinesFragmentStubsAndHints()
    {
        byte[] stub = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        var pdu = new FaultCoPdu
        {
            CallId = 7,
            AllocationHint = stub.Length,
            Stub = stub,
        };

        var reassembled = (FaultCoPdu)pdu.Reassemble(pdu.GetFragments(32));

        await Assert.That(reassembled.CallId).IsEqualTo(7);
        await Assert.That(reassembled.AllocationHint).IsEqualTo(11);
        await Assert.That(Convert.ToHexString(reassembled.Stub)).IsEqualTo("000102030405060708090A");
        await Assert.That(reassembled.Flags).IsEqualTo(
            ConnectionOrientedPdu.PFC_FIRST_FRAG | ConnectionOrientedPdu.PFC_LAST_FRAG);
    }

    [Test]
    public async Task FaultCoPdu_GetFragments_SmallStubReturnsOriginalPdu()
    {
        var pdu = new FaultCoPdu
        {
            AllocationHint = 3,
            Stub = [1, 2, 3],
        };

        List<FaultCoPdu> fragments = CollectFragments(pdu, 32);

        await Assert.That(fragments.Count).IsEqualTo(1);
        await Assert.That(ReferenceEquals(fragments[0], pdu)).IsTrue();
        await Assert.That(Convert.ToHexString(fragments[0].Stub)).IsEqualTo("010203");
    }

    [Test]
    public async Task FaultCoPdu_ReassembleEmptyIterator_ThrowsIOException()
    {
        var pdu = new FaultCoPdu();

        IOException exception = Capture<IOException>(() =>
            _ = pdu.Reassemble(new List<ConnectionOrientedPdu>()));

        await Assert.That(exception.Message).IsEqualTo("No fragments available.");
    }

    [Test]
    public async Task ConnectionOrientedPdu_SetFlag_TogglesSingleFlag()
    {
        var pdu = new Auth3Pdu { Flags = ConnectionOrientedPdu.PFC_FIRST_FRAG };

        pdu.SetFlag(ConnectionOrientedPdu.PFC_LAST_FRAG, true);
        await Assert.That(pdu.Flags).IsEqualTo(ConnectionOrientedPdu.PFC_FIRST_FRAG | ConnectionOrientedPdu.PFC_LAST_FRAG);

        pdu.SetFlag(ConnectionOrientedPdu.PFC_FIRST_FRAG, false);
        await Assert.That(pdu.Flags).IsEqualTo(ConnectionOrientedPdu.PFC_LAST_FRAG);
    }

    [Test]
    public async Task PduCodec_TryGetFragmentLengthRejectsHeaderWithShortAdvertisedLength()
    {
        byte[] frame = Convert.FromHexString("05001003100000000F00000000000000");

        bool ok = PduCodec.TryGetFragmentLength(new System.Buffers.ReadOnlySequence<byte>(frame), out int length);

        await Assert.That(ok).IsFalse();
        await Assert.That(length).IsEqualTo(15);
    }

    private static List<FaultCoPdu> CollectFragments(FaultCoPdu pdu, int fragmentSize)
    {
        var fragments = new List<FaultCoPdu>();
        foreach (var fragment in pdu.GetFragments(fragmentSize))
        {
            fragments.Add((FaultCoPdu)fragment);
        }

        return fragments;
    }

    private static TException Capture<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new InvalidOperationException($"Expected exception of type {typeof(TException).Name}.");
    }
}
