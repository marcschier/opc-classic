//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using Opc.Classic.Dcom.Channels;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Orpc;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Ndr;
using Opc.Classic.Testing;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;

namespace Opc.Classic.Dcom.Tests;

public sealed class DcomCallChannelOrpcTests
{
    [Test]
    public async Task InvokeAsync_writes_OrpcThis_before_user_payload()
    {
        await using var transport = new InMemoryAsyncTransport();
        byte[] requestPayload = [0x10, 0x11, 0x12, 0x13];
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);

        _ = await channel.InvokeAsync(Guid.NewGuid(), 3, requestPayload);

        RequestCoPdu request = await ReadSingleOutboundRequestAsync(transport);
        var reader = new NdrReader(request.Stub);
        OrpcThis orpcThis = OrpcThis.Read(ref reader);
        byte[] actualPayload = request.Stub.AsSpan(reader.Position).ToArray();

        await Assert.That(orpcThis.Version).IsEqualTo(OrpcComVersion.Default);
        await Assert.That(orpcThis.Flags).IsEqualTo(0u);
        await Assert.That(orpcThis.CausalityId).IsNotEqualTo(Guid.Empty);
        await Assert.That(orpcThis.Extensions is null).IsTrue();
        await Assert.That(actualPayload.SequenceEqual(requestPayload)).IsTrue();
    }

    [Test]
    public async Task InvokeAsync_reads_OrpcThat_before_user_response()
    {
        await using var transport = new InMemoryAsyncTransport();
        byte[] responsePayload = [0x21, 0x22, 0x23, 0x24];
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateResponseBytes(responsePayload));
        var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);

        NdrCallResult result = await channel.InvokeAsync(Guid.NewGuid(), 4, ReadOnlyMemory<byte>.Empty);

        await Assert.That(result.Hresult).IsEqualTo(0);
        await Assert.That(result.ResponsePayload.ToArray().SequenceEqual(responsePayload)).IsTrue();
    }

    [Test]
    public async Task InvokeAsync_reuses_causality_id_for_nested_logical_calls()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);
        Guid interfaceId = Guid.NewGuid();

        using IDisposable scope = CausalityContext.BeginCall();
        Guid expectedCausalityId = CausalityContext.Current.Value.GetValueOrDefault();
        _ = await channel.InvokeAsync(interfaceId, 5, ReadOnlyMemory<byte>.Empty);
        _ = await InvokeNestedAsync(channel, interfaceId);

        IReadOnlyList<RequestCoPdu> requests = await ReadOutboundRequestsAsync(transport);
        OrpcThis first = ReadOrpcThis(requests[0]);
        OrpcThis second = ReadOrpcThis(requests[1]);

        await Assert.That(first.CausalityId).IsEqualTo(expectedCausalityId);
        await Assert.That(second.CausalityId).IsEqualTo(expectedCausalityId);
    }

    [Test]
    public async Task InvokeAsync_reuses_parent_causality_context()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);
        Guid parentCausalityId = Guid.NewGuid();

        using IDisposable scope = CausalityContext.BeginCall(parentCausalityId);
        _ = await channel.InvokeAsync(Guid.NewGuid(), 7, ReadOnlyMemory<byte>.Empty);

        RequestCoPdu request = await ReadSingleOutboundRequestAsync(transport);
        OrpcThis orpcThis = ReadOrpcThis(request);

        await Assert.That(orpcThis.CausalityId).IsEqualTo(parentCausalityId);
    }

    private static Task<NdrCallResult> InvokeNestedAsync(DcomCallChannel channel, Guid interfaceId) =>
        channel.InvokeAsync(interfaceId, 6, ReadOnlyMemory<byte>.Empty);

    private static OrpcThis ReadOrpcThis(RequestCoPdu request)
    {
        var reader = new NdrReader(request.Stub);
        return OrpcThis.Read(ref reader);
    }

    private static async Task<RequestCoPdu> ReadSingleOutboundRequestAsync(InMemoryAsyncTransport transport)
    {
        IReadOnlyList<RequestCoPdu> requests = await ReadOutboundRequestsAsync(transport);
        await Assert.That(requests.Count).IsEqualTo(1);
        return requests[0];
    }

    private static async Task<IReadOnlyList<RequestCoPdu>> ReadOutboundRequestsAsync(InMemoryAsyncTransport transport)
    {
        ReadResult result = await transport.ReadOutbound.ReadAsync();
        byte[] outbound = result.Buffer.ToArray();
        transport.ReadOutbound.AdvanceTo(result.Buffer.End);

        var requests = new List<RequestCoPdu>();
        foreach (ConnectionOrientedPdu pdu in DecodePdus(outbound))
        {
            if (pdu is RequestCoPdu request)
            {
                requests.Add(request);
            }
        }

        return requests;
    }

    private static IEnumerable<ConnectionOrientedPdu> DecodePdus(byte[] bytes)
    {
        int offset = 0;
        while (offset < bytes.Length)
        {
            int fragmentLength = BinaryPrimitives.ReadUInt16LittleEndian(
                bytes.AsSpan(offset + ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, sizeof(ushort)));
            byte[] frame = bytes.AsSpan(offset, fragmentLength).ToArray();
            yield return DecodePdu(frame);
            offset += fragmentLength;
        }
    }

    private static ConnectionOrientedPdu DecodePdu(byte[] bytes)
    {
        byte type = bytes[ConnectionOrientedPdu.TYPE_OFFSET];
        ConnectionOrientedPdu pdu = type switch
        {
            RequestCoPdu.REQUEST_TYPE => new RequestCoPdu(),
            BindPdu.BIND_TYPE => new BindPdu(),
            _ => throw new InvalidOperationException($"Unexpected outbound PDU type 0x{type:X2}."),
        };

        var ndr = new NdrCodec { Format = pdu.Format };
        var buffer = new NdrBuffer(bytes, 0) { Length = bytes.Length };
        pdu.Decode(ndr, buffer);
        return pdu;
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

    private static byte[] CreateResponseBytes(byte[] responsePayload)
    {
        byte[] responseStub = CreateResponseStub(responsePayload);
        var response = new ResponseCoPdu
        {
            AllocationHint = responseStub.Length,
            CallId = 2,
            ContextId = 0,
            Flags = ConnectionOrientedPdu.PFC_FIRST_FRAG | ConnectionOrientedPdu.PFC_LAST_FRAG,
            Stub = responseStub,
        };

        return EncodePdu(response);
    }

    private static byte[] CreateResponseStub(byte[] responsePayload)
    {
        byte[] stub = new byte[OrpcThat.NullExtensionsWireSize + responsePayload.Length];
        var writer = new NdrWriter(stub);
        new OrpcThat().Write(ref writer);
        responsePayload.CopyTo(stub.AsSpan(writer.Position));
        return stub;
    }

    private static byte[] EncodePdu(ConnectionOrientedPdu pdu)
    {
        var ndr = new NdrCodec { Format = pdu.Format };
        var buffer = new NdrBuffer(new byte[ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE], 0);
        pdu.Encode(ndr, buffer);
        return buffer.Buf.AsSpan(0, buffer.Length).ToArray();
    }
}
