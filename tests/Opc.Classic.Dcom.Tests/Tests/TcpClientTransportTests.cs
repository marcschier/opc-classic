//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Net;
using System.Net.Sockets;
using Opc.Classic.Dcom.Transport;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dcom.Tests.Tests;

/// <summary>
/// Unit + smoke tests for the public <see cref="TcpClientTransport"/>
/// and <see cref="DcomCallChannelFactory.ConnectTcpAsync"/> surface
/// (cap-e1).
/// </summary>
public sealed class TcpClientTransportTests
{
    [Test]
    public async Task Constructor_throws_for_null_client()
    {
        await Assert.That(() => new TcpClientTransport(null!)).Throws<ArgumentNullException>();
    }

    [Test]
    public async Task Constructor_throws_for_unconnected_client()
    {
        using var client = new TcpClient();
        await Assert.That(() => new TcpClientTransport(client)).Throws<ArgumentException>();
    }

    [Test]
    public async Task ConnectAsync_throws_for_empty_host()
    {
        await Assert.That(async () =>
                await TcpClientTransport.ConnectAsync(string.Empty, 51300, TestContext.Current!.CancellationToken))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task ConnectAsync_throws_for_invalid_port()
    {
        await Assert.That(async () =>
                await TcpClientTransport.ConnectAsync("127.0.0.1", 0, TestContext.Current!.CancellationToken))
            .Throws<ArgumentOutOfRangeException>();
        await Assert.That(async () =>
                await TcpClientTransport.ConnectAsync("127.0.0.1", 99999, TestContext.Current!.CancellationToken))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task ConnectAsync_round_trip_through_a_local_listener()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync(TestContext.Current!.CancellationToken).AsTask();

        await using TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            "127.0.0.1", port, TestContext.Current.CancellationToken);

        using TcpClient acceptedServer = await acceptTask;

        await Assert.That(transport.RemoteEndpoint).IsNotNull();
        await Assert.That(transport.RemoteEndpoint).IsTypeOf<IPEndPoint>();
        await Assert.That(((IPEndPoint)transport.RemoteEndpoint).Port).IsEqualTo(port);
        await Assert.That(transport.Input).IsNotNull();
        await Assert.That(transport.Output).IsNotNull();

        listener.Stop();
    }

    [Test]
    public async Task ConnectTcpAsync_returns_DcomCallChannel_bound_to_transport()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync(TestContext.Current!.CancellationToken).AsTask();

        await using DcomCallChannel channel = await DcomCallChannelFactory.ConnectTcpAsync(
            "127.0.0.1", port,
            new NoOpAuthContext(),
            TestContext.Current.CancellationToken);

        using TcpClient acceptedServer = await acceptTask;

        await Assert.That(channel).IsNotNull();
        listener.Stop();
    }

    [Test]
    public async Task ConnectTcpAsync_throws_for_null_authContext()
    {
        await Assert.That(async () =>
                await DcomCallChannelFactory.ConnectTcpAsync("127.0.0.1", 51300, null!, TestContext.Current!.CancellationToken))
            .Throws<ArgumentNullException>();
    }

    [Test]
    public async Task DisposeAsync_is_idempotent()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        Task<TcpClient> acceptTask = listener.AcceptTcpClientAsync(TestContext.Current!.CancellationToken).AsTask();

        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            "127.0.0.1", port, TestContext.Current.CancellationToken);

        using TcpClient acceptedServer = await acceptTask;

        await transport.DisposeAsync();
        await transport.DisposeAsync();

        listener.Stop();
    }
}
