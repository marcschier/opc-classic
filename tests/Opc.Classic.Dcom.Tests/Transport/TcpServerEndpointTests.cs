// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;
using System.Net.Sockets;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Transport;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Dcom.Tests.Transport;

public sealed class TcpServerEndpointTests
{
    [Test]
    public async Task LocalEndpoint_resolves_dynamic_port_zero_to_real_port()
    {
        await using var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));

        var boundEndpoint = endpoint.LocalEndpoint as IPEndPoint;

        await Assert.That(boundEndpoint).IsNotNull();
        await Assert.That(boundEndpoint!.Port).IsGreaterThan(0);
        await Assert.That(boundEndpoint.Address).IsEqualTo(IPAddress.Loopback);
    }

    [Test]
    public async Task AcceptConnectionsAsync_yields_accepted_clients()
    {
        await using var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var boundEndpoint = (IPEndPoint)endpoint.LocalEndpoint;
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        // Producer: connect three TCP clients
        var connectTasks = Enumerable.Range(0, 3)
            .Select(async _ =>
            {
                var client = new TcpClient();
                await client.ConnectAsync(boundEndpoint.Address, boundEndpoint.Port, cts.Token);
                return client;
            })
            .ToArray();

        var transports = new List<IAsyncTransport>();
        await foreach (IAsyncTransport transport in endpoint.AcceptConnectionsAsync(cts.Token))
        {
            transports.Add(transport);
            if (transports.Count == 3)
            {
                break;
            }
        }

        TcpClient[] clients = await Task.WhenAll(connectTasks);

        await Assert.That(transports.Count).IsEqualTo(3);
        foreach (TcpClient client in clients)
        {
            client.Dispose();
        }
        foreach (IAsyncTransport transport in transports)
        {
            await transport.DisposeAsync();
        }
    }

    [Test]
    public async Task AcceptConnectionsAsync_completes_on_cancellation()
    {
        await using var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);

        Task drain = Task.Run(async () =>
        {
            await foreach (IAsyncTransport transport in endpoint.AcceptConnectionsAsync(cts.Token))
            {
                await transport.DisposeAsync();
            }
        });

        cts.Cancel();
        await drain.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current!.CancellationToken);

        await Assert.That(drain.IsCompletedSuccessfully).IsTrue();
    }

    [Test]
    [NotInParallel]
    public async Task DisposeAsync_stops_listener()
    {
        var endpoint = new TcpServerEndpoint(new IPEndPoint(IPAddress.Loopback, 0));
        var boundEndpoint = (IPEndPoint)endpoint.LocalEndpoint;

        await endpoint.DisposeAsync();

        await Assert.That(async () =>
        {
            using var client = new TcpClient();
            await client.ConnectAsync(boundEndpoint.Address, boundEndpoint.Port, TestContext.Current!.CancellationToken);
        }).Throws<SocketException>();
    }

    [Test]
    public async Task Constructor_throws_on_null_endpoint()
    {
        await Assert.That(() => { _ = new TcpServerEndpoint(null!); })
            .Throws<ArgumentNullException>();
    }
}
