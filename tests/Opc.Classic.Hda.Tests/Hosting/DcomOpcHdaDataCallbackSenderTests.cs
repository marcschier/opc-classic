// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Net;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Transport;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Hda.Tests.Hosting;

public sealed class DcomOpcHdaDataCallbackSenderTests
{
    [Test]
    public async Task Constructor_null_sinkRef_throws()
    {
        await Assert.That(() => { _ = new DcomOpcHdaDataCallbackSender(
            sinkRef: null!,
            NewFactory(),
            () => new NoOpAuthContext()); }).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_null_channelFactory_throws()
    {
        await Assert.That(() => { _ = new DcomOpcHdaDataCallbackSender(
            SinkRef(),
            channelFactory: null!,
            () => new NoOpAuthContext()); }).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_null_authContextFactory_throws()
    {
        await Assert.That(() => { _ = new DcomOpcHdaDataCallbackSender(
            SinkRef(),
            NewFactory(),
            authContextFactory: null!); }).Throws<ArgumentNullException>();
    }

    [Test]
    [Arguments("")]
    [Arguments("   ")]
    public async Task Constructor_blank_fallbackHost_throws(string host)
    {
        await Assert.That(() => { _ = new DcomOpcHdaDataCallbackSender(
            SinkRef(),
            NewFactory(),
            () => new NoOpAuthContext(),
            host); }).Throws<ArgumentException>();
    }

    [Test]
    public async Task DisposeAsync_without_connecting_is_idempotent()
    {
        var sender = new DcomOpcHdaDataCallbackSender(
            SinkRef(),
            NewFactory(),
            () => new NoOpAuthContext());

        await sender.DisposeAsync();
        await sender.DisposeAsync();
    }

    private static DcomCallChannelFactory NewFactory() => new(new UnusedTransportFactory());

    private static OpcInterfaceRef SinkRef() => new(
        iid: Guid.NewGuid(),
        flags: 0,
        publicRefs: 1,
        oxid: 1,
        oid: 1,
        ipid: Guid.NewGuid(),
        securityOffset: 0,
        resolverBindings: []);

    private sealed class UnusedTransportFactory : IAsyncTransportFactory
    {
        public ValueTask<IAsyncTransport> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("The transport factory must not be invoked in these tests.");
    }
}
