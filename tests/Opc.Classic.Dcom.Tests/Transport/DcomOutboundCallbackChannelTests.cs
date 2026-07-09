// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.IO.Pipelines;
using System.Net;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Tests.Transport;

public sealed class DcomOutboundCallbackChannelTests
{
    private static readonly Guid CallbackIid = new("11111111-2222-3333-4444-555555555555");

    [Test]
    public async Task ConnectAsync_null_sinkRef_throws()
    {
        var factory = new DcomCallChannelFactory(new RecordingTransportFactory());

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await DcomOutboundCallbackChannel.ConnectAsync(
                sinkRef: null!,
                factory,
                () => new NoOpAuthContext(),
                "localhost",
                CallbackIid));
    }

    [Test]
    public async Task ConnectAsync_null_channelFactory_throws()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await DcomOutboundCallbackChannel.ConnectAsync(
                new FakeSinkRef([]),
                channelFactory: null!,
                () => new NoOpAuthContext(),
                "localhost",
                CallbackIid));
    }

    [Test]
    public async Task ConnectAsync_null_authContextFactory_throws()
    {
        var factory = new DcomCallChannelFactory(new RecordingTransportFactory());

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await DcomOutboundCallbackChannel.ConnectAsync(
                new FakeSinkRef([]),
                factory,
                authContextFactory: null!,
                "localhost",
                CallbackIid));
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task ConnectAsync_blank_fallbackHost_throws(string host)
    {
        var factory = new DcomCallChannelFactory(new RecordingTransportFactory());

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await DcomOutboundCallbackChannel.ConnectAsync(
                new FakeSinkRef([]),
                factory,
                () => new NoOpAuthContext(),
                host,
                CallbackIid));
    }

    [Test]
    public async Task ConnectAsync_no_resolvable_binding_throws()
    {
        var transport = new RecordingTransportFactory();
        var factory = new DcomCallChannelFactory(transport);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await DcomOutboundCallbackChannel.ConnectAsync(
                new FakeSinkRef([]),
                factory,
                () => new NoOpAuthContext(),
                "localhost",
                CallbackIid));

        await Assert.That(transport.ConnectCount).IsEqualTo(0);
    }

    [Test]
    public async Task ConnectAsync_resolves_tcp_binding_and_routes_to_endpoint()
    {
        var transport = new RecordingTransportFactory();
        var factory = new DcomCallChannelFactory(transport);
        var sinkRef = new FakeSinkRef(BuildTcpBinding("host", 8194), ipid: Guid.NewGuid());

        await using DcomCallChannel channel = await DcomOutboundCallbackChannel.ConnectAsync(
            sinkRef,
            factory,
            () => new NoOpAuthContext(),
            "fallback",
            CallbackIid);

        await Assert.That(channel).IsNotNull();
        await Assert.That(transport.ConnectCount).IsEqualTo(1);
        await Assert.That(transport.LastEndpoint).IsTypeOf<DnsEndPoint>();
        var dns = (DnsEndPoint)transport.LastEndpoint!;
        await Assert.That(dns.Host).IsEqualTo("host");
        await Assert.That(dns.Port).IsEqualTo(8194);
    }

    [Test]
    public async Task ConnectAsync_honours_cancellation_when_transport_hangs()
    {
        var factory = new DcomCallChannelFactory(new HangingTransportFactory());
        var sinkRef = new FakeSinkRef(BuildTcpBinding("host", 8194), ipid: Guid.NewGuid());
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(50));

        await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await DcomOutboundCallbackChannel.ConnectAsync(
                sinkRef,
                factory,
                () => new NoOpAuthContext(),
                "fallback",
                CallbackIid,
                cts.Token));
    }

    private static ushort[] BuildTcpBinding(string host, int port)
    {
        var words = new List<ushort> { 0x0007 };
        foreach (char ch in $"{host}[{port}]")
        {
            words.Add(ch);
        }
        words.Add(0);
        words.Add(0);
        return words.ToArray();
    }

    private sealed class FakeSinkRef(ushort[] bindings, Guid ipid = default) : IOpcInterfaceRef
    {
        public Guid Iid => Guid.Empty;
        public uint Flags => 0;
        public uint PublicRefs => 0;
        public ulong Oxid => 0;
        public ulong Oid => 0;
        public Guid Ipid => ipid;
        public ushort SecurityOffset => 0;
        public IReadOnlyList<ushort> ResolverBindings => bindings;
    }

    private sealed class RecordingTransportFactory : IAsyncTransportFactory
    {
        public int ConnectCount { get; private set; }

        public EndPoint? LastEndpoint { get; private set; }

        public ValueTask<IAsyncTransport> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            ConnectCount++;
            LastEndpoint = endpoint;
            return ValueTask.FromResult<IAsyncTransport>(new FakeTransport(endpoint));
        }
    }

    private sealed class HangingTransportFactory : IAsyncTransportFactory
    {
        public async ValueTask<IAsyncTransport> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);
            throw new InvalidOperationException("unreachable");
        }
    }

    private sealed class FakeTransport(EndPoint endpoint) : IAsyncTransport
    {
        private readonly Pipe _input = new();
        private readonly Pipe _output = new();

        public EndPoint RemoteEndpoint => endpoint;
        public PipeReader Input => _input.Reader;
        public PipeWriter Output => _output.Writer;

        public ValueTask FlushAsync(CancellationToken cancellationToken = default) => ValueTask.CompletedTask;

        public ValueTask DisposeAsync()
        {
            _input.Writer.Complete();
            _output.Writer.Complete();
            return ValueTask.CompletedTask;
        }
    }
}
