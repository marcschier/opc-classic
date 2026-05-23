//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Testing;
using Opc.Classic.Transport;
using SharpInterop.Rpc;
using SharpInterop.Rpc.Core;
using SharpInterop.Rpc.pdu;
using TUnit.Core;

namespace Opc.Classic.Dcom.Tests;

public sealed class DcomCallChannelTests
{
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
