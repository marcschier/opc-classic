//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Opc.Classic;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Integration.Tests.Support;
using TUnit.Core;

namespace Opc.Classic.Integration.Tests.CompatMatrix;

public sealed class AeManagedClientOverTransportTests {
    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Ae_managed_client_round_trips_GetStatus_over_real_listener() {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        CancellationToken cancellationToken = timeout.Token;

        var stub = new StubAeServer();
        await using ServiceProvider provider = BuildServiceProvider(stub);
        OpcAeServerHost host = ResolveHost(provider);

        await host.StartAsync(cancellationToken);
        try {
            await using DcomCallChannel channel = await ConnectClientAsync(host, cancellationToken);
            var proxy = new IOPCEventServerClientProxy(channel);

            OpcServerStatus status = await proxy.GetStatusAsync(cancellationToken);

            await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
            await Assert.That(status.VendorInfo).IsEqualTo("Loopback AE Stub Server");
            await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Ae);
            await Assert.That(status.ServerVersion).IsEqualTo(new Version(1, 10, 1));
            await Assert.That(stub.StatusCallCount).IsEqualTo(1);
        }
        finally {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Ae_managed_client_round_trips_QueryAvailableFilters_and_categories_over_real_listener() {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        CancellationToken cancellationToken = timeout.Token;

        var stub = new StubAeServer();
        await using ServiceProvider provider = BuildServiceProvider(stub);
        OpcAeServerHost host = ResolveHost(provider);

        await host.StartAsync(cancellationToken);
        try {
            await using DcomCallChannel channel = await ConnectClientAsync(host, cancellationToken);
            var proxy = new IOPCEventServerClientProxy(channel);

            int filters = await proxy.QueryAvailableFiltersAsync(cancellationToken);
            await proxy.QueryEventCategoriesAsync(
                0x07,
                out int[] categories,
                out string[] descriptions,
                cancellationToken);

            await Assert.That(filters).IsEqualTo(0x07);
            await Assert.That(categories).IsEquivalentTo([1001, 1002]);
            await Assert.That(descriptions).IsEquivalentTo(["Process", "System"]);
            await Assert.That(stub.LastEventType).IsEqualTo(0x07);
            await Assert.That(stub.FilterCallCount).IsEqualTo(1);
            await Assert.That(stub.CategoryCallCount).IsEqualTo(1);
        }
        finally {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Ae_managed_client_two_back_to_back_calls_share_connection() {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        CancellationToken cancellationToken = timeout.Token;

        var stub = new StubAeServer();
        await using ServiceProvider provider = BuildServiceProvider(stub);
        OpcAeServerHost host = ResolveHost(provider);

        await host.StartAsync(cancellationToken);
        try {
            await using DcomCallChannel channel = await ConnectClientAsync(host, cancellationToken);
            var proxy = new IOPCEventServerClientProxy(channel);

            OpcServerStatus status = await proxy.GetStatusAsync(cancellationToken);
            int filters = await proxy.QueryAvailableFiltersAsync(cancellationToken);

            await Assert.That(status.VendorInfo).IsEqualTo("Loopback AE Stub Server");
            await Assert.That(filters).IsEqualTo(0x07);
            await Assert.That(stub.StatusCallCount).IsEqualTo(1);
            await Assert.That(stub.FilterCallCount).IsEqualTo(1);
        }
        finally {
            await host.StopAsync(CancellationToken.None);
        }
    }

    // AE subscription tearoff routing is deferred: OpcAeServerHost currently
    // builds a root-only RpcServerConnectionProcessor and does not pass an
    // OpcObjectRegistry for object-IPID routing like the DA host does.

    private static ServiceProvider BuildServiceProvider(StubAeServer server) {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IOpcAeServer>(server);
        services.AddSingleton<OpcObjectRegistry>();
        services.AddSingleton<OpcAeServerHost>();
        services.AddSingleton<IOpcServerHost>(static sp => sp.GetRequiredService<OpcAeServerHost>());
        services.Configure<OpcAeServerOptions>(static options => {
            options.Clsid = Guid.NewGuid();
            options.ProgId = "Managed.Ae.Compat.1";
            options.FriendlyName = "Managed AE transport test server";
            options.ListenAddress = "127.0.0.1:0";
        });
        return services.BuildServiceProvider();
    }

    private static OpcAeServerHost ResolveHost(ServiceProvider provider) =>
        (OpcAeServerHost)provider.GetRequiredService<IOpcServerHost>();

    private static async Task<DcomCallChannel> ConnectClientAsync(
        OpcAeServerHost host,
        CancellationToken cancellationToken) {
        var endpoint = (IPEndPoint?)host.LocalEndpoint
            ?? throw new InvalidOperationException("Host did not expose a bound endpoint after StartAsync.");
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            cancellationToken);
        return new DcomCallChannel(transport, NoOpAuthContext.Instance);
    }
}
