//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Opc.Classic;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Integration.Tests.Support;
using Opc.Classic.Transport;
using TUnit.Core;

namespace Opc.Classic.Integration.Tests.CompatMatrix;

/// <summary>
/// Phase 14D-B integration tests: a fully-managed Opc.Classic client connects
/// to a fully-managed Opc.Classic server over the real cross-platform
/// <see cref="OpcServerListener"/> transport (ocom-1). The previous
/// scaffold-only loopback tests (<see cref="Net10ServerToNativeClientTests"/>)
/// short-circuit out unless a native sample client is available; the tests
/// here exercise the real transport regardless of native-client availability.
/// </summary>
/// <remarks>
/// <para>
/// What this proves end-to-end:
/// </para>
/// <list type="bullet">
///   <item><see cref="TcpServerEndpoint"/> binds + accepts a real TCP socket</item>
///   <item><see cref="RpcServerConnectionProcessor"/> negotiates the DCE/RPC
///   bind handshake (anonymous-only, NDR32 transfer syntax)</item>
///   <item><see cref="OpcServerListener"/> wires the accept loop + processor</item>
///   <item><see cref="OpcDaServerHost"/> wires DI + lifecycle (StartAsync /
///   StopAsync)</item>
///   <item><c>IOPCServerServerDispatcher</c> (source-generated) decodes the
///   NDR request body and calls the managed <see cref="IOpcDaServer"/></item>
///   <item><see cref="IOPCServerClientProxy"/> (source-generated) encodes the
///   request and decodes the response on the client side</item>
///   <item><see cref="Opc.Classic.Dcom.Transport.PduCodec"/> + the
///   client-side <c>DcomCallChannel</c> serialize PDU framing</item>
/// </list>
/// <para>
/// The tests use loopback (<see cref="IPAddress.Loopback"/>) so no
/// privileged ports / no out-of-process activation is required.
/// </para>
/// </remarks>
public sealed class ManagedClientOverTransportTests
{
    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Managed_client_round_trips_GetStatus_over_real_listener()
    {
        await using ServiceProvider provider = BuildServiceProvider(
            "Managed transport status server",
            knownItems: ["Random.Int4"]);
        OpcDaServerHost host = ResolveHost(provider);

        await host.StartAsync(TestContext.Current!.CancellationToken);
        try
        {
            await using var channel = await ConnectClientAsync(host);
            var proxy = new IOPCServerClientProxy(channel);

            OpcServerStatus status = await proxy.GetStatusAsync(TestContext.Current!.CancellationToken);

            await Assert.That(status.State).IsEqualTo(OpcServerState.Running);
            await Assert.That(status.VendorInfo).IsEqualTo("Managed transport status server");
            await Assert.That(status.Spec).IsEqualTo(OpcStatusSpec.Da);
        }
        finally
        {
            await host.StopAsync(TestContext.Current!.CancellationToken);
        }
    }

    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Managed_client_round_trips_GetErrorString_over_real_listener()
    {
        await using ServiceProvider provider = BuildServiceProvider(
            "Managed transport error server",
            knownItems: []);
        OpcDaServerHost host = ResolveHost(provider);

        await host.StartAsync(TestContext.Current!.CancellationToken);
        try
        {
            await using var channel = await ConnectClientAsync(host);
            var proxy = new IOPCServerClientProxy(channel);

            string text = await proxy.GetErrorStringAsync(
                errorCode: unchecked((int)0x80004005),
                localeId: 1033,
                cancellationToken: TestContext.Current!.CancellationToken);

            await Assert.That(text).Contains("0x80004005");
            await Assert.That(text).Contains("0x0409");
        }
        finally
        {
            await host.StopAsync(TestContext.Current!.CancellationToken);
        }
    }

    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Managed_client_RemoveGroup_routes_to_server_and_records_call()
    {
        StubDaServer stub = StubDaServer.CompatMatrixNet10Server();
        await using ServiceProvider provider = BuildServiceProvider(stub);
        OpcDaServerHost host = ResolveHost(provider);

        await host.StartAsync(TestContext.Current!.CancellationToken);
        try
        {
            await using var channel = await ConnectClientAsync(host);
            var proxy = new IOPCServerClientProxy(channel);

            await proxy.RemoveGroupAsync(1234, force: true, TestContext.Current!.CancellationToken);

            await Assert.That(stub.RemovedGroups.Count).IsEqualTo(1);
            await Assert.That(stub.RemovedGroups[0].ServerGroupHandle).IsEqualTo(1234);
            await Assert.That(stub.RemovedGroups[0].Force).IsTrue();
        }
        finally
        {
            await host.StopAsync(TestContext.Current!.CancellationToken);
        }
    }

    [Test]
    [Category("CompatMatrix.Loopback")]
    public async Task Managed_client_two_back_to_back_calls_share_connection()
    {
        await using ServiceProvider provider = BuildServiceProvider(
            "Managed transport multi-call server",
            knownItems: []);
        OpcDaServerHost host = ResolveHost(provider);

        await host.StartAsync(TestContext.Current!.CancellationToken);
        try
        {
            await using var channel = await ConnectClientAsync(host);
            var proxy = new IOPCServerClientProxy(channel);

            OpcServerStatus first = await proxy.GetStatusAsync(TestContext.Current!.CancellationToken);
            OpcServerStatus second = await proxy.GetStatusAsync(TestContext.Current!.CancellationToken);

            await Assert.That(first.VendorInfo).IsEqualTo("Managed transport multi-call server");
            await Assert.That(second.VendorInfo).IsEqualTo("Managed transport multi-call server");
        }
        finally
        {
            await host.StopAsync(TestContext.Current!.CancellationToken);
        }
    }

    // ----- helpers -----

    private static ServiceProvider BuildServiceProvider(string vendorInfo, IReadOnlyCollection<string> knownItems)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IOpcDaServer>(_ => new StubDaServer(vendorInfo, knownItems));
        services.AddSingleton<IOpcDaDataChangePublisher, OpcDaDataChangePublisher>();
        services.AddSingleton<OpcObjectRegistry>();
        services.AddSingleton<OpcDaServerHost>();
        services.AddSingleton<IOpcServerHost>(sp => sp.GetRequiredService<OpcDaServerHost>());
        services.Configure<OpcDaServerOptions>(o =>
        {
            o.Clsid = Guid.NewGuid();
            o.ProgId = "Managed.Compat.1";
            o.FriendlyName = "Managed transport test server";
            o.ListenAddress = "127.0.0.1:0";
        });
        return services.BuildServiceProvider();
    }

    private static ServiceProvider BuildServiceProvider(StubDaServer server)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IOpcDaServer>(server);
        services.AddSingleton<IOpcDaDataChangePublisher, OpcDaDataChangePublisher>();
        services.AddSingleton<OpcObjectRegistry>();
        services.AddSingleton<OpcDaServerHost>();
        services.AddSingleton<IOpcServerHost>(sp => sp.GetRequiredService<OpcDaServerHost>());
        services.Configure<OpcDaServerOptions>(o =>
        {
            o.Clsid = Guid.NewGuid();
            o.ProgId = "Managed.Compat.1";
            o.FriendlyName = "Managed transport test server";
            o.ListenAddress = "127.0.0.1:0";
        });
        return services.BuildServiceProvider();
    }

    private static OpcDaServerHost ResolveHost(ServiceProvider provider) =>
        (OpcDaServerHost)provider.GetRequiredService<IOpcServerHost>();

    private static async Task<Opc.Classic.Dcom.Transport.DcomCallChannel> ConnectClientAsync(OpcDaServerHost host)
    {
        var bound = (IPEndPoint?)host.LocalEndpoint
            ?? throw new InvalidOperationException("Host did not expose a bound endpoint after StartAsync.");

        // Uses the public TcpClientTransport (cap-e1) which lifts what used to
        // be a private test helper into Opc.Classic.Dcom.Transport.
        Opc.Classic.Dcom.Transport.TcpClientTransport transport =
            await Opc.Classic.Dcom.Transport.TcpClientTransport.ConnectAsync(
                bound.Address.ToString(),
                bound.Port,
                TestContext.Current!.CancellationToken);
        return new Opc.Classic.Dcom.Transport.DcomCallChannel(
            transport,
            new Opc.Classic.NoOpAuthContext());
    }
}
