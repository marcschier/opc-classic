//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Integration.Tests.Support;

namespace Opc.Classic.Integration.Tests.CompatMatrix;

public sealed class LoopbackConcurrencyAndCancellationTests
{
    private const string ConcurrencyVendorInfo = "Loopback concurrency DA Stub Server";

    [Test, NotInParallel]
    [Category("CompatMatrix.Loopback")]
    public async Task Da_loopback_concurrent_clients_round_trip_status_without_exceptions()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        CancellationToken cancellationToken = timeout.Token;

        await using ServiceProvider provider = BuildServiceProvider(new StubDaServer(ConcurrencyVendorInfo, []));
        OpcDaServerHost host = ResolveDaHost(provider);
        int successCount = 0;

        await host.StartAsync(cancellationToken);
        try
        {
            Task[] clients = Enumerable.Range(0, 8)
                .Select(_ => RunStatusClientAsync(host, () => Interlocked.Increment(ref successCount), cancellationToken))
                .ToArray();

            await Task.WhenAll(clients);

            await Assert.That(successCount).IsEqualTo(80);
        }
        finally
        {
            await host.StopAsync(CancellationToken.None);
        }
    }

    [Test, NotInParallel]
    [Category("CompatMatrix.Loopback")]
    public async Task Da_loopback_client_side_cancellation_of_in_flight_call_does_not_hang_host_stop()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(10));
        CancellationToken cancellationToken = timeout.Token;

        var server = new GatedDaServer();
        await using ServiceProvider provider = BuildServiceProvider(server);
        OpcDaServerHost host = ResolveDaHost(provider);
        DcomCallChannel? channel = null;
        bool hostStarted = false;

        await host.StartAsync(cancellationToken);
        hostStarted = true;
        try
        {
            channel = await ConnectDaClientAsync(host, cancellationToken);
            var proxy = new IOPCServerClientProxy(channel);
            using var clientCancellation = new CancellationTokenSource();

            Task<OpcServerStatus> call = proxy.GetStatusAsync(clientCancellation.Token);
            await server.Entered.WaitAsync(cancellationToken);
            await clientCancellation.CancelAsync();

            _ = await CaptureAsync<OperationCanceledException>(async () =>
                _ = await call.ConfigureAwait(false));
        }
        finally
        {
            server.Release();
            if (hostStarted)
            {
                using var stopTimeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current!.CancellationToken);
                stopTimeout.CancelAfter(TimeSpan.FromSeconds(10));
                Task stopTask = host.StopAsync(stopTimeout.Token);
                await stopTask.WaitAsync(stopTimeout.Token);
                await Assert.That(stopTask.IsCompletedSuccessfully).IsTrue();
            }

            if (channel is not null)
            {
                await channel.DisposeAsync();
            }
        }
    }

    private static ServiceProvider BuildServiceProvider(IOpcDaServer server)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IOpcDaServer>(server);
        services.AddSingleton<IOpcDaDataChangePublisher, OpcDaDataChangePublisher>();
        services.AddSingleton<OpcObjectRegistry>();
        services.AddSingleton<OpcDaServerHost>();
        services.AddSingleton<IOpcServerHost>(static sp => sp.GetRequiredService<OpcDaServerHost>());
        services.Configure<OpcDaServerOptions>(static options =>
        {
            options.Clsid = Guid.NewGuid();
            options.ProgId = "Managed.Da.Concurrency.1";
            options.FriendlyName = "Managed DA concurrency test server";
            options.ListenAddress = "127.0.0.1:0";
        });
        return services.BuildServiceProvider();
    }

    private static OpcDaServerHost ResolveDaHost(ServiceProvider provider) =>
        (OpcDaServerHost)provider.GetRequiredService<IOpcServerHost>();

    private static async Task RunStatusClientAsync(
        OpcDaServerHost host,
        Action recordSuccess,
        CancellationToken cancellationToken)
    {
        await using DcomCallChannel channel = await ConnectDaClientAsync(host, cancellationToken);
        var proxy = new IOPCServerClientProxy(channel);

        for (int i = 0; i < 10; i++)
        {
            OpcServerStatus status = await proxy.GetStatusAsync(cancellationToken);
            await Assert.That(status.VendorInfo).IsEqualTo(ConcurrencyVendorInfo);
            recordSuccess();
        }
    }

    private static async Task<DcomCallChannel> ConnectDaClientAsync(
        OpcDaServerHost host,
        CancellationToken cancellationToken)
    {
        var endpoint = (IPEndPoint?)host.LocalEndpoint
            ?? throw new InvalidOperationException("Host did not expose a bound endpoint after StartAsync.");
        TcpClientTransport transport = await TcpClientTransport.ConnectAsync(
            endpoint.Address.ToString(),
            endpoint.Port,
            cancellationToken);
        return new DcomCallChannel(transport, NoOpAuthContext.Instance);
    }

    private static async Task<TException> CaptureAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (TException exception)
        {
            return exception;
        }
        catch (Exception exception)
        {
            throw new InvalidOperationException($"Expected {typeof(TException).Name}, but caught {exception.GetType().Name}.", exception);
        }

        throw new InvalidOperationException($"Expected {typeof(TException).Name}, but no exception was thrown.");
    }
}
