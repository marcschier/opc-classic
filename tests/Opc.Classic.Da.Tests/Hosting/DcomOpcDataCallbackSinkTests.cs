// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Transport;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Da.Tests.Hosting;

public sealed class DcomOpcDataCallbackSinkTests
{
    private static readonly OpcDaGroup.DataChangePayload EmptyPayload =
        new(0x1234, 0x5678, 0, 0, [], [], [], [], []);

    [Test]
    public async Task Constructor_null_sinkRef_throws()
    {
        await Assert.That(() => { _ = new DcomOpcDataCallbackSink(
            sinkRef: null!,
            NewFactory(),
            () => new NoOpAuthContext(),
            "localhost"); }).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_null_channelFactory_throws()
    {
        await Assert.That(() => { _ = new DcomOpcDataCallbackSink(
            CallbackRef([]),
            channelFactory: null!,
            () => new NoOpAuthContext(),
            "localhost"); }).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_null_authContextFactory_throws()
    {
        await Assert.That(() => { _ = new DcomOpcDataCallbackSink(
            CallbackRef([]),
            NewFactory(),
            authContextFactory: null!,
            "localhost"); }).Throws<ArgumentNullException>();
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task Constructor_blank_fallbackHost_throws(string host)
    {
        await Assert.That(() => { _ = new DcomOpcDataCallbackSink(
            CallbackRef([]),
            NewFactory(),
            () => new NoOpAuthContext(),
            host); }).Throws<ArgumentException>();
    }

    [Test]
    public async Task Constructor_wrong_iid_throws()
    {
        var wrongRef = new OpcInterfaceRef(
            iid: Guid.NewGuid(),
            flags: 0,
            publicRefs: 1,
            oxid: 1,
            oid: 1,
            ipid: Guid.NewGuid(),
            securityOffset: 0,
            resolverBindings: []);

        await Assert.That(() => { _ = new DcomOpcDataCallbackSink(
            wrongRef,
            NewFactory(),
            () => new NoOpAuthContext(),
            "localhost"); }).Throws<ArgumentException>();
    }

    [Test]
    public async Task Constructor_non_positive_timeout_throws()
    {
        await Assert.That(() => { _ = new DcomOpcDataCallbackSink(
            CallbackRef([]),
            NewFactory(),
            () => new NoOpAuthContext(),
            "localhost",
            logger: null,
            deliveryTimeout: TimeSpan.Zero); }).Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task OnDataChange_marks_unreachable_when_binding_cannot_resolve()
    {
        var transport = new RecordingTransportFactory();
        using var sink = new DcomOpcDataCallbackSink(
            CallbackRef([]),
            new DcomCallChannelFactory(transport),
            () => new NoOpAuthContext(),
            "localhost");

        await Assert.That(sink.IsUnreachable).IsFalse();

        sink.OnDataChange(EmptyPayload);

        await Assert.That(sink.IsUnreachable).IsTrue();
        await Assert.That(transport.ConnectCount).IsEqualTo(0);
    }

    [Test]
    public async Task OnDataChange_after_unreachable_is_noop()
    {
        var transport = new RecordingTransportFactory();
        using var sink = new DcomOpcDataCallbackSink(
            CallbackRef([]),
            new DcomCallChannelFactory(transport),
            () => new NoOpAuthContext(),
            "localhost");

        sink.OnDataChange(EmptyPayload);
        await Assert.That(sink.IsUnreachable).IsTrue();

        // Second delivery returns early without re-attempting resolution.
        sink.OnDataChange(EmptyPayload);
        sink.OnReadComplete(EmptyPayload);

        await Assert.That(sink.IsUnreachable).IsTrue();
        await Assert.That(transport.ConnectCount).IsEqualTo(0);
    }

    [Test]
    public async Task Dispose_is_idempotent_and_delivery_after_dispose_is_noop()
    {
        var sink = new DcomOpcDataCallbackSink(
            CallbackRef([]),
            NewFactory(),
            () => new NoOpAuthContext(),
            "localhost");

        sink.Dispose();
        sink.Dispose();

        // Delivery after dispose returns early instead of throwing.
        sink.OnDataChange(EmptyPayload);

        await Assert.That(sink.IsUnreachable).IsFalse();
    }

    private static DcomCallChannelFactory NewFactory() => new(new RecordingTransportFactory());

    private static OpcInterfaceRef CallbackRef(ushort[] bindings) => new(
        iid: IOPCDataCallback.InterfaceId,
        flags: 0,
        publicRefs: 1,
        oxid: 0xCA11,
        oid: 0xDA7A,
        ipid: Guid.NewGuid(),
        securityOffset: 0,
        resolverBindings: bindings);

    private sealed class RecordingTransportFactory : IAsyncTransportFactory
    {
        public int ConnectCount { get; private set; }

        public ValueTask<IAsyncTransport> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            ConnectCount++;
            throw new InvalidOperationException("The recording transport factory should not be invoked in these tests.");
        }
    }
}
