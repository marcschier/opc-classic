// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Net;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using Opc.Classic.Transport;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dcom.Tests.Transport;

public sealed class DcomOxidResolverClientTests
{
    [Test]
    [Arguments(0x000006D1)]
    [Arguments(unchecked((int)0x800706D1u))]
    [Arguments(0x1C010002)]
    [Arguments(unchecked((int)0xC002002Eu))]
    public async Task ResolveOxidBindingsAsync_falls_back_from_opnum4_to_opnum0(
        int procnumOutOfRange)
    {
        var transportFactory = new RecordingTransportFactory();
        await transportFactory.Transport.WriteInboundAsync(CreateBindAckBytes());
        await transportFactory.Transport.WriteInboundAsync(CreateFaultBytes(procnumOutOfRange));
        await transportFactory.Transport.WriteInboundAsync(
            CreateResponseBytes(BuildResolveOxidResponse(includeComVersion: false)));
        var channelFactory = new DcomCallChannelFactory(transportFactory);

        byte[] bindings = await DcomOxidResolverClient.ResolveOxidBindingsAsync(
            "resolver-host",
            oxid: 1,
            Array.Empty<ushort>(),
            channelFactory,
            NoOpAuthContext.Instance);

        IReadOnlyList<int> opnums = await ReadRequestOpnumsAsync(transportFactory.Transport.Inner);
        await Assert.That(opnums).IsEquivalentTo(new[] { 4, 0 });
        await Assert.That(Convert.ToHexString(bindings)).IsEqualTo("0200010007000000");
        await Assert.That(transportFactory.Transport.IsDisposed).IsTrue();
    }

    [Test]
    public async Task ResolveOxidBindingsAsync_propagates_non_fallback_fault_and_disposes()
    {
        const int accessDenied = unchecked((int)0x80070005u);
        var transportFactory = new RecordingTransportFactory();
        await transportFactory.Transport.WriteInboundAsync(CreateBindAckBytes());
        await transportFactory.Transport.WriteInboundAsync(CreateFaultBytes(accessDenied));
        var channelFactory = new DcomCallChannelFactory(transportFactory);

        InvalidOperationException exception = await Assert.That(async () =>
            await DcomOxidResolverClient.ResolveOxidBindingsAsync(
                "resolver-host",
                oxid: 1,
                Array.Empty<ushort>(),
                channelFactory,
                NoOpAuthContext.Instance)).Throws<InvalidOperationException>();

        IReadOnlyList<int> opnums = await ReadRequestOpnumsAsync(transportFactory.Transport.Inner);
        await Assert.That(opnums).IsEquivalentTo(new[] { 4 });
        await Assert.That(exception.Message).Contains("0x80070005");
        await Assert.That(transportFactory.Transport.IsDisposed).IsTrue();
    }

    private static byte[] BuildResolveOxidResponse(bool includeComVersion)
    {
        var buffer = new byte[64];
        var writer = new NdrWriter(buffer);
        writer.WriteUInt32(0x00020000);
        writer.WriteUInt32(2);
        writer.WriteUInt16(2);
        writer.WriteUInt16(1);
        writer.WriteUInt16(0x07);
        writer.WriteUInt16(0);
        writer.AlignTo(4);
        writer.WriteGuid(new Guid("11111111-2222-3333-4444-555555555555"));
        writer.WriteUInt32(5);
        if (includeComVersion)
        {
            writer.WriteUInt16(5);
            writer.WriteUInt16(7);
        }

        writer.WriteInt32(0);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static byte[] CreateBindAckBytes()
    {
        var bindAck = new BindAcknowledgePdu
        {
            AssociationGroupId = 1,
            CallId = 1,
            MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            ResultList = [new PresentationResult()],
            SecondaryAddress = new Port(),
        };
        return EncodePdu(bindAck);
    }

    private static byte[] CreateFaultBytes(int hresult)
    {
        var fault = new FaultCoPdu
        {
            AllocationHint = 0,
            CallId = 2,
            ContextId = 0,
            Status = (FaultCode)hresult,
        };
        return EncodePdu(fault);
    }

    private static byte[] CreateResponseBytes(byte[] payload)
    {
        var response = new ResponseCoPdu
        {
            AllocationHint = payload.Length,
            CallId = 3,
            ContextId = 0,
            Stub = payload,
        };
        return EncodePdu(response);
    }

    private static byte[] EncodePdu(ConnectionOrientedPdu pdu)
    {
        var ndr = new NdrCodec { Format = pdu.Format };
        var buffer = new NdrBuffer(new byte[ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE], 0);
        pdu.Encode(ndr, buffer);
        return buffer.Buf.AsSpan(0, buffer.Length).ToArray();
    }

    private static async Task<IReadOnlyList<int>> ReadRequestOpnumsAsync(InMemoryAsyncTransport transport)
    {
        ReadResult result = await transport.ReadOutbound.ReadAsync();
        byte[] outbound = result.Buffer.ToArray();
        transport.ReadOutbound.AdvanceTo(result.Buffer.End);

        var opnums = new List<int>();
        int offset = 0;
        while (offset < outbound.Length)
        {
            int fragmentLength = BinaryPrimitives.ReadUInt16LittleEndian(
                outbound.AsSpan(offset + ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, sizeof(ushort)));
            ConnectionOrientedPdu pdu = PduCodec.DecodePdu(outbound.AsSpan(offset, fragmentLength).ToArray());
            if (pdu is RequestCoPdu request)
            {
                opnums.Add(request.Opnum);
            }

            offset += fragmentLength;
        }

        return opnums;
    }

    private sealed class RecordingTransportFactory : IAsyncTransportFactory
    {
        public TrackingTransport Transport { get; } = new();

        public ValueTask<IAsyncTransport> ConnectAsync(
            EndPoint endpoint,
            CancellationToken cancellationToken = default)
        {
            _ = endpoint;
            cancellationToken.ThrowIfCancellationRequested();
            return ValueTask.FromResult<IAsyncTransport>(Transport);
        }
    }

    private sealed class TrackingTransport : IAsyncTransport
    {
        public InMemoryAsyncTransport Inner { get; } = new();

        public bool IsDisposed { get; private set; }

        public EndPoint RemoteEndpoint => Inner.RemoteEndpoint;

        public PipeReader Input => Inner.Input;

        public PipeWriter Output => Inner.Output;

        public ValueTask FlushAsync(CancellationToken cancellationToken = default) =>
            Inner.FlushAsync(cancellationToken);

        public ValueTask WriteInboundAsync(ReadOnlyMemory<byte> data) =>
            Inner.WriteInboundAsync(data);

        public async ValueTask DisposeAsync()
        {
            IsDisposed = true;
            await Inner.DisposeAsync().ConfigureAwait(false);
        }
    }
}
