// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Ae.Dcom;
using Opc.Classic;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Samples.AeServer;
using Opc.Classic.Testing;

namespace Opc.Classic.Samples.AeClient;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        HostApplicationBuilder host = Host.CreateApplicationBuilder(args);

        host.Logging.ClearProviders();
        host.Logging.AddSimpleConsole(static opt =>
        {
            opt.SingleLine = true;
            opt.TimestampFormat = "HH:mm:ss ";
        });

        string? remoteHost = Environment.GetEnvironmentVariable("OPC_CLASSIC_SERVER_HOST");
        string? remotePortText = Environment.GetEnvironmentVariable("OPC_CLASSIC_SERVER_PORT");
        int remotePort = 0;
        bool useTcp = !string.IsNullOrWhiteSpace(remoteHost)
            && int.TryParse(remotePortText, out remotePort)
            && remotePort > 0;

        Console.WriteLine(useTcp
            ? $"Connecting over TCP to {remoteHost}:{remotePort}"
            : "Running in-process via InMemoryCallChannel + LoopbackDaServer");

        if (useTcp)
        {
            AddTcpAeClient(host.Services, remoteHost!, remotePort);
        }
        else
        {
            AddLoopbackAeClient(host.Services);
        }

        host.Services.AddHostedService<AeClientDemo>();

        await host.Build().RunAsync().ConfigureAwait(false);
        return 0;
    }

    private static void AddTcpAeClient(IServiceCollection services, string remoteHost, int remotePort)
    {
        services.AddSingleton<DcomCallChannel>(_ =>
            DcomCallChannelFactory.ConnectTcpAsync(remoteHost, remotePort, NoOpAuthContext.Instance)
                .GetAwaiter()
                .GetResult());
        services.AddSingleton<ICallChannel>(static sp => sp.GetRequiredService<DcomCallChannel>());
        services.AddSingleton<IOPCEventServer>(static sp => new IOPCEventServerClientProxy(sp.GetRequiredService<ICallChannel>()));
        services.AddSingleton<LoopbackAeClient>();
    }

    private static void AddLoopbackAeClient(IServiceCollection services)
    {
        services.AddSingleton<SampleAeServer>();
        services.AddSingleton<InProcessAeServer>();
        services.AddSingleton<IOpcAeServer>(static sp => sp.GetRequiredService<InProcessAeServer>());
        services.AddSingleton<OpcAeServerDispatcher>();
        services.AddSingleton(static sp =>
        {
            var dispatcher = sp.GetRequiredService<OpcAeServerDispatcher>();
            return new InMemoryCallChannel((iid, opnum, payload, ct) =>
                dispatcher.DispatchAsync(iid, opnum, payload, ct));
        });
        services.AddSingleton<ICallChannel>(static sp => sp.GetRequiredService<InMemoryCallChannel>());
        services.AddSingleton<IOPCEventServer>(static sp =>
            new IOPCEventServerClientProxy(sp.GetRequiredService<InMemoryCallChannel>()));
        services.AddSingleton<LoopbackAeClient>();
    }
}
