// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Samples.HdaServer;
using Opc.Classic.Testing;

namespace Opc.Classic.Samples.HdaClient;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var host = Host.CreateApplicationBuilder(args);

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
            AddTcpHdaClient(host.Services, remoteHost!, remotePort);
        }
        else
        {
            AddLoopbackHdaClient(host.Services);
        }

        host.Services.AddHostedService<HdaClientDemo>();

        await host.Build().RunAsync().ConfigureAwait(false);
        return 0;
    }

    private static void AddTcpHdaClient(IServiceCollection services, string remoteHost, int remotePort)
    {
        services.AddSingleton<DcomCallChannel>(_ =>
            DcomCallChannelFactory.ConnectTcpAsync(remoteHost, remotePort, NoOpAuthContext.Instance)
                .GetAwaiter()
                .GetResult());
        services.AddSingleton<ICallChannel>(static sp => sp.GetRequiredService<DcomCallChannel>());
        services.AddSingleton<LoopbackHdaClient>();
    }

    private static void AddLoopbackHdaClient(IServiceCollection services)
    {
        services.AddSingleton<HistoricalDataStore>();
        services.AddSingleton<IOpcHdaServer, SampleHdaServer>();
        services.AddSingleton<LoopbackHdaCallRouter>();
        services.AddSingleton<InMemoryCallChannel>(static sp =>
            new InMemoryCallChannel(sp.GetRequiredService<LoopbackHdaCallRouter>().DispatchAsync));
        services.AddSingleton<ICallChannel>(static sp => sp.GetRequiredService<InMemoryCallChannel>());
        services.AddSingleton<LoopbackHdaClient>();
    }
}
