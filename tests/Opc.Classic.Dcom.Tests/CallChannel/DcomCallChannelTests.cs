//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Net;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Testing;
using Opc.Classic.Transport;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;

namespace Opc.Classic.Dcom.Tests;

public sealed class DcomCallChannelTests
{
    private static readonly IReadOnlyList<Guid> PreBindIids = OpcSpecCatalog.Da;
    private static readonly Guid FirstInterfaceId = PreBindIids[0];
    private static readonly Guid SecondInterfaceId = PreBindIids[1];
    private static readonly Guid RejectedOptionalInterfaceId = IOPCAsyncIO3.InterfaceId;

    [Test]
    public async Task InvokeAsync_via_InMemoryAsyncTransport_round_trips()
    {
        await using var transport = new InMemoryAsyncTransport();
        byte[] responsePayload = [0x21, 0x22, 0x23, 0x24];
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateResponseBytes(responsePayload));
        var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);

        NdrCallResult result = await channel.InvokeAsync(Guid.NewGuid(), 3, new byte[] { 0x10, 0x11 });

        await Assert.That(result.Hresult).IsEqualTo(0);
        await Assert.That(result.ResponsePayload.ToArray()).IsEquivalentTo(responsePayload);
    }

    [Test]
    public async Task InvokeAsync_FaultPdu_returns_hresult()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateFaultBytes(ReadEFail()));
        var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);

        NdrCallResult result = await channel.InvokeAsync(Guid.NewGuid(), 7, ReadOnlyMemory<byte>.Empty);

        await Assert.That(result.Hresult).IsEqualTo(ReadEFail());
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.ResponsePayload.Length).IsEqualTo(0);
    }

    [Test]
    public async Task InvokeAsync_fragmented_response_assembles_correctly()
    {
        await using var transport = new InMemoryAsyncTransport();
        byte[] responsePayload = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07];
        byte[] responseStub = CreateResponseStub(responsePayload);
        await transport.WriteInboundAsync(CreateBindAckBytes());
        await transport.WriteInboundAsync(CreateResponseFragmentBytes(
            responseStub[0..5],
            ConnectionOrientedPdu.PFC_FIRST_FRAG));
        await transport.WriteInboundAsync(CreateResponseFragmentBytes(
            responseStub[5..],
            ConnectionOrientedPdu.PFC_LAST_FRAG));
        var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);

        NdrCallResult result = await channel.InvokeAsync(Guid.NewGuid(), 9, ReadOnlyMemory<byte>.Empty);

        await Assert.That(result.Hresult).IsEqualTo(0);
        await Assert.That(result.ResponsePayload.ToArray()).IsEquivalentTo(responsePayload);
    }

    [Test]
    public async Task InvokeAsync_predeclares_Da_contexts_in_initial_bind_order()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindAckBytes(PreBindIids.Count));
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        var channel = new DcomCallChannel(
            transport,
            NoOpAuthContext.Instance,
            PreBindIids);

        _ = await channel.InvokeAsync(FirstInterfaceId, 6, ReadOnlyMemory<byte>.Empty);

        IReadOnlyList<ConnectionOrientedPdu> outbound = await ReadOutboundPdusAsync(transport);
        var bind = (BindPdu)outbound[0];
        await Assert.That(bind.ContextList.Length).IsEqualTo(PreBindIids.Count);
        for (int i = 0; i < PreBindIids.Count; i++)
        {
            await Assert.That(bind.ContextList[i].ContextId).IsEqualTo(i);
            Guid actualInterfaceId = Guid.Parse(bind.ContextList[i].AbstractSyntax.Uuid.ToString());
            await Assert.That(actualInterfaceId).IsEqualTo(PreBindIids[i]);
        }
    }

    [Test]
    public async Task InvokeAsync_keeps_first_call_iid_at_context_zero()
    {
        await using var transport = new InMemoryAsyncTransport();
        await transport.WriteInboundAsync(CreateBindAckBytes(PreBindIids.Count));
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        var channel = new DcomCallChannel(
            transport,
            NoOpAuthContext.Instance,
            PreBindIids);

        _ = await channel.InvokeAsync(SecondInterfaceId, 3, ReadOnlyMemory<byte>.Empty);

        IReadOnlyList<ConnectionOrientedPdu> outbound = await ReadOutboundPdusAsync(transport);
        var bind = (BindPdu)outbound[0];
        await Assert.That(bind.ContextList[0].ContextId).IsEqualTo(0);
        Guid actualInterfaceId = Guid.Parse(bind.ContextList[0].AbstractSyntax.Uuid.ToString());
        await Assert.That(actualInterfaceId).IsEqualTo(SecondInterfaceId);
    }

    [Test]
    public async Task InvokeAsync_allows_optional_predeclared_context_rejection()
    {
        await using var transport = new InMemoryAsyncTransport();
        int rejectedIndex = IndexOf(PreBindIids, RejectedOptionalInterfaceId);
        await transport.WriteInboundAsync(CreateBindAckBytes(
            PreBindIids.Count,
            rejectedIndex));
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        var channel = new DcomCallChannel(
            transport,
            NoOpAuthContext.Instance,
            PreBindIids);

        NdrCallResult result = await channel.InvokeAsync(FirstInterfaceId, 6, ReadOnlyMemory<byte>.Empty);

        await Assert.That(result.Hresult).IsEqualTo(0);
    }

    [Test]
    public async Task InvokeAsync_reuses_predeclared_context_without_alter_context()
    {
        await using var transport = new InMemoryAsyncTransport();
        Guid routedIpid = new("44444444-4444-4444-4444-444444444444");
        await transport.WriteInboundAsync(CreateBindAckBytes(PreBindIids.Count));
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        await transport.WriteInboundAsync(CreateResponseBytes([]));
        var channel = new DcomCallChannel(
            transport,
            NoOpAuthContext.Instance,
            PreBindIids);

        _ = await channel.InvokeAsync(FirstInterfaceId, 6, ReadOnlyMemory<byte>.Empty);
        channel.RegisterInterfaceIpid(SecondInterfaceId, routedIpid);
        _ = await channel.InvokeAsync(SecondInterfaceId, 3, ReadOnlyMemory<byte>.Empty);

        IReadOnlyList<ConnectionOrientedPdu> outbound = await ReadOutboundPdusAsync(transport);
        await Assert.That(ContainsPdu<AlterContextPdu>(outbound)).IsFalse();
        var secondRequest = (RequestCoPdu)outbound[2];
        await Assert.That(secondRequest.ContextId).IsEqualTo(IndexOf(PreBindIids, SecondInterfaceId));
        await Assert.That(Guid.Parse(secondRequest.Object!.ToString())).IsEqualTo(routedIpid);
    }

    [Test]
    public async Task InvokeAsync_cancellation_token_propagates()
    {
        await using var transport = new InMemoryAsyncTransport();
        var channel = new DcomCallChannel(transport, NoOpAuthContext.Instance);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var canceled = false;
        try
        {
            _ = await channel.InvokeAsync(Guid.NewGuid(), 1, ReadOnlyMemory<byte>.Empty, cts.Token);
        }
        catch (OperationCanceledException)
        {
            canceled = true;
        }

        await Assert.That(canceled).IsTrue();
    }

    [Test]
    public async Task DcomCallChannelFactory_connects_then_disposes_transport()
    {
        var transportFactory = new RecordingTransportFactory();
        var channelFactory = new DcomCallChannelFactory(transportFactory);
        var endpoint = new IPEndPoint(IPAddress.Loopback, 135);

        ICallChannel channel = await channelFactory.ConnectAsync(endpoint, Guid.Empty, NoOpAuthContext.Instance);

        await Assert.That(transportFactory.Endpoint).IsEqualTo(endpoint);
        await ((IAsyncDisposable)channel).DisposeAsync();
        await Assert.That(transportFactory.Transport.IsDisposed).IsTrue();
    }

    private static byte[] CreateBindAckBytes() => CreateBindAckBytes(resultCount: 1);

    private static byte[] CreateBindAckBytes(int resultCount, int rejectedIndex = -1)
    {
        var bindAck = new BindAcknowledgePdu
        {
            AssociationGroupId = 1,
            CallId = 1,
            MaxReceiveFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            MaxTransmitFragment = ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE,
            ResultList = CreatePresentationResults(resultCount, rejectedIndex),
            SecondaryAddress = new Port(),
        };

        return EncodePdu(bindAck);
    }

    private static PresentationResult[] CreatePresentationResults(int count, int rejectedIndex)
    {
        var results = new PresentationResult[count];
        for (int i = 0; i < results.Length; i++)
        {
            results[i] = i == rejectedIndex
                ? new PresentationResult(
                    PresentationResultCode.PROVIDER_REJECTION,
                    PresentationResultReason.ABSTRACT_SYNTAX_NOT_SUPPORTED,
                    new PresentationSyntax(NdrCodec.NDR_SYNTAX))
                : new PresentationResult();
        }

        return results;
    }

    private static byte[] CreateResponseBytes(byte[] responsePayload) =>
        CreateResponseFragmentBytes(
            CreateResponseStub(responsePayload),
            ConnectionOrientedPdu.PFC_FIRST_FRAG | ConnectionOrientedPdu.PFC_LAST_FRAG);

    private static byte[] CreateResponseFragmentBytes(byte[] responseStub, int flags)
    {
        var response = new ResponseCoPdu
        {
            AllocationHint = responseStub.Length,
            CallId = 2,
            ContextId = 0,
            Flags = flags,
            Stub = responseStub,
        };

        return EncodePdu(response);
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

    private static byte[] CreateResponseStub(byte[] responsePayload)
    {
        byte[] stub = new byte[8 + responsePayload.Length];
        responsePayload.CopyTo(stub.AsSpan(8));
        return stub;
    }

    private static async Task<IReadOnlyList<ConnectionOrientedPdu>> ReadOutboundPdusAsync(InMemoryAsyncTransport transport)
    {
        ReadResult result = await transport.ReadOutbound.ReadAsync();
        byte[] outbound = result.Buffer.ToArray();
        transport.ReadOutbound.AdvanceTo(result.Buffer.End);

        var pdus = new List<ConnectionOrientedPdu>();
        int offset = 0;
        while (offset < outbound.Length)
        {
            int fragmentLength = BinaryPrimitives.ReadUInt16LittleEndian(
                outbound.AsSpan(offset + ConnectionOrientedPdu.FRAG_LENGTH_OFFSET, sizeof(ushort)));
            byte[] frame = outbound.AsSpan(offset, fragmentLength).ToArray();
            pdus.Add(PduCodec.DecodePdu(frame));
            offset += fragmentLength;
        }

        return pdus;
    }

    private static bool ContainsPdu<T>(IReadOnlyList<ConnectionOrientedPdu> pdus)
        where T : ConnectionOrientedPdu
    {
        for (int i = 0; i < pdus.Count; i++)
        {
            if (pdus[i] is T)
            {
                return true;
            }
        }

        return false;
    }

    private static int IndexOf(IReadOnlyList<Guid> values, Guid value)
    {
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] == value)
            {
                return i;
            }
        }

        return -1;
    }

    private static byte[] EncodePdu(ConnectionOrientedPdu pdu)
    {
        var ndr = new NdrCodec { Format = pdu.Format };
        var buffer = new NdrBuffer(new byte[ConnectionOrientedPdu.MUST_RECEIVE_FRAGMENT_SIZE], 0);
        pdu.Encode(ndr, buffer);
        return buffer.Buf.AsSpan(0, buffer.Length).ToArray();
    }

    // TUnitAssertions0005 workaround: use a method call for the E_FAIL constant.
    private static int ReadEFail() => unchecked((int)0x80004005u);

    private sealed class RecordingTransportFactory : IAsyncTransportFactory
    {
        public RecordingTransport Transport { get; } = new();
        public EndPoint? Endpoint { get; private set; }

        public ValueTask<IAsyncTransport> ConnectAsync(
            EndPoint endpoint,
            CancellationToken cancellationToken = default)
        {
            Endpoint = endpoint;
            return ValueTask.FromResult<IAsyncTransport>(Transport);
        }
    }

    private sealed class RecordingTransport : IAsyncTransport
    {
        private readonly InMemoryAsyncTransport _inner = new();

        public bool IsDisposed { get; private set; }
        public EndPoint RemoteEndpoint => _inner.RemoteEndpoint;
        public System.IO.Pipelines.PipeReader Input => _inner.Input;
        public System.IO.Pipelines.PipeWriter Output => _inner.Output;

        public ValueTask FlushAsync(CancellationToken cancellationToken = default) =>
            _inner.FlushAsync(cancellationToken);

        public async ValueTask DisposeAsync()
        {
            IsDisposed = true;
            await _inner.DisposeAsync().ConfigureAwait(false);
        }
    }
}
