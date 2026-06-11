//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Transport;
using TUnit.Core;

namespace Opc.Classic.Integration.Tests.CompatMatrix;

/// <summary>
/// Phase 14D-C: outbound callback dispatch. Proves that the same
/// <see cref="OpcServerListener"/> infrastructure used server-side (ocom-1)
/// hosts a CLIENT-SIDE callback sink, and that a managed outbound caller
/// (the role of an OPC server pushing data into its subscribed clients)
/// can invoke <see cref="IOPCDataCallback"/> methods on it via the
/// source-generated <see cref="IOPCDataCallbackClientProxy"/>.
/// </summary>
/// <remarks>
/// <para>
/// In real OPC DA the flow is:
/// </para>
/// <list type="number">
///   <item>Client creates an <see cref="IOPCDataCallback"/> sink object</item>
///   <item>Client calls <see cref="IConnectionPoint.AdviseAsync"/> passing the sink ref</item>
///   <item>Server stores the sink ref</item>
///   <item>On data change, server invokes the sink's <c>OnDataChange</c> over RPC</item>
/// </list>
/// <para>
/// ocom-7 demonstrates step 4 -- the outbound call from server's perspective
/// to a sink hosted on the client's listener. The DCOM-level subscription
/// state (storing the sink ref, triggering callbacks on data change) is
/// application-level wiring that builds on this infrastructure. The
/// architectural piece that needed proving -- that
/// <c>DcomCallChannel</c> + <c>IOPCDataCallbackClientProxy</c> +
/// <c>OpcServerListener</c> + <c>IOPCDataCallbackServerDispatcher</c>
/// compose for the reverse direction -- is what these tests cover.
/// </para>
/// <para>
/// What is NOT covered here:
/// </para>
/// <list type="bullet">
///   <item>Resolving an <c>IOpcInterfaceRef</c>'s <c>ResolverBindings</c> +
///   <c>Ipid</c> into a concrete <c>EndPoint</c> for the outbound channel.
///   (Tests construct the channel pointing at a known endpoint directly.)</item>
///   <item>End-to-end Advise → store-sink → trigger-on-change application
///   wiring. The infrastructure here is the building block.</item>
/// </list>
/// </remarks>
public sealed class OutboundCallbackOverTransportTests
{
    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Outbound_OnCancelComplete_delivers_to_client_side_sink()
    {
        var sink = new RecordingDataCallback();
        await using var sinkListener = StartSinkListener(sink);
        var sinkEndpoint = (IPEndPoint)sinkListener.LocalEndpoint;

        await using var channel = await ConnectAsync(sinkEndpoint);
        var proxy = new IOPCDataCallbackClientProxy(channel);

        await proxy.OnCancelCompleteAsync(
            transactionId: 12345,
            groupHandle: 7,
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(sink.OnCancelCompleteCallCount).IsEqualTo(1);
        await Assert.That(sink.LastTransactionId).IsEqualTo(12345);
        await Assert.That(sink.LastGroupHandle).IsEqualTo(7);
    }

    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Outbound_OnWriteComplete_delivers_per_item_errors()
    {
        var sink = new RecordingDataCallback();
        await using var sinkListener = StartSinkListener(sink);
        var sinkEndpoint = (IPEndPoint)sinkListener.LocalEndpoint;

        await using var channel = await ConnectAsync(sinkEndpoint);
        var proxy = new IOPCDataCallbackClientProxy(channel);

        int[] clientHandles = [10, 20, 30];
        int[] errors = [0, unchecked((int)0x80004005), 0];

        await proxy.OnWriteCompleteAsync(
            transactionId: 99,
            groupHandle: 1,
            masterError: 0,
            clientHandles: clientHandles,
            errors: errors,
            cancellationToken: TestContext.Current!.CancellationToken);

        await Assert.That(sink.LastClientHandles).IsEquivalentTo(clientHandles);
        await Assert.That(sink.LastWriteErrors).IsEquivalentTo(errors);
    }

    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Multiple_outbound_callbacks_share_one_channel()
    {
        var sink = new RecordingDataCallback();
        await using var sinkListener = StartSinkListener(sink);
        var sinkEndpoint = (IPEndPoint)sinkListener.LocalEndpoint;

        await using var channel = await ConnectAsync(sinkEndpoint);
        var proxy = new IOPCDataCallbackClientProxy(channel);

        await proxy.OnCancelCompleteAsync(1, 1, TestContext.Current!.CancellationToken);
        await proxy.OnCancelCompleteAsync(2, 1, TestContext.Current!.CancellationToken);
        await proxy.OnCancelCompleteAsync(3, 1, TestContext.Current!.CancellationToken);

        await Assert.That(sink.OnCancelCompleteCallCount).IsEqualTo(3);
        await Assert.That(sink.LastTransactionId).IsEqualTo(3);
    }

    // ----- helpers -----

    private static OpcServerListener StartSinkListener(IOPCDataCallback sink)
    {
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var dispatcher = new IOPCDataCallbackServerDispatcher(sink);
        var processor = new RpcServerConnectionProcessor(
            new Dictionary<Guid, IOpcServerDispatcher> { [IOPCDataCallback.InterfaceId] = dispatcher });
        var listener = new OpcServerListener(endpoint, processor);
        listener.StartAsync(TestContext.Current!.CancellationToken).GetAwaiter().GetResult();
        return listener;
    }

    private static async Task<Opc.Classic.Dcom.Transport.DcomCallChannel> ConnectAsync(IPEndPoint endpoint)
    {
        var client = new TcpClient();
        await client.ConnectAsync(endpoint.Address, endpoint.Port, TestContext.Current!.CancellationToken);
        return new Opc.Classic.Dcom.Transport.DcomCallChannel(
            new TcpClientTransport(client),
            new Opc.Classic.NoOpAuthContext());
    }

    private sealed class RecordingDataCallback : IOPCDataCallback
    {
        public int OnCancelCompleteCallCount { get; private set; }

        public int LastTransactionId { get; private set; }

        public int LastGroupHandle { get; private set; }

        public int[] LastClientHandles { get; private set; } = Array.Empty<int>();

        public int[] LastWriteErrors { get; private set; } = Array.Empty<int>();

        public Task OnDataChangeAsync(
            int transactionId, int groupHandle, int masterQuality, int masterError,
            int[] clientHandles, OpcVariant[] values, ushort[] qualities, long[] timestamps,
            int[] errors, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task OnReadCompleteAsync(
            int transactionId, int groupHandle, int masterQuality, int masterError,
            int[] clientHandles, OpcVariant[] values, ushort[] qualities, long[] timestamps,
            int[] errors, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task OnWriteCompleteAsync(
            int transactionId, int groupHandle, int masterError,
            int[] clientHandles, int[] errors, CancellationToken cancellationToken = default)
        {
            LastTransactionId = transactionId;
            LastGroupHandle = groupHandle;
            LastClientHandles = clientHandles;
            LastWriteErrors = errors;
            return Task.CompletedTask;
        }

        public Task OnCancelCompleteAsync(int transactionId, int groupHandle, CancellationToken cancellationToken = default)
        {
            OnCancelCompleteCallCount++;
            LastTransactionId = transactionId;
            LastGroupHandle = groupHandle;
            return Task.CompletedTask;
        }
    }

    private sealed class TcpClientTransport : IAsyncTransport
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        public TcpClientTransport(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
            Input = PipeReader.Create(_stream);
            Output = PipeWriter.Create(_stream);
            RemoteEndpoint = client.Client.RemoteEndPoint ?? new IPEndPoint(IPAddress.None, 0);
        }

        public EndPoint RemoteEndpoint { get; }

        public PipeReader Input { get; }

        public PipeWriter Output { get; }

        public async ValueTask FlushAsync(CancellationToken cancellationToken = default) =>
            await Output.FlushAsync(cancellationToken).ConfigureAwait(false);

        public async ValueTask DisposeAsync()
        {
            await Input.CompleteAsync().ConfigureAwait(false);
            await Output.CompleteAsync().ConfigureAwait(false);
            await _stream.DisposeAsync().ConfigureAwait(false);
            _client.Dispose();
        }
    }
}
