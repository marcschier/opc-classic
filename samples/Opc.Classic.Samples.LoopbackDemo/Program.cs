// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Hosting;
using Opc.Classic.Testing;

namespace Opc.Classic.Samples.LoopbackDemo;

internal static class Program {
    public static async Task<int> Main(string[] args) {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Warning);
        builder.Logging.AddSimpleConsole(static options => {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });

        builder.Services.AddClassicServer();
        builder.Services.AddSingleton<LoopbackTagStore>();
        builder.Services.AddOpcDaServer<SampleDaServer>(static options => {
            options.Clsid = new Guid("14E7FD8D-15D3-43F3-8D2F-93B41DA8D7B5");
            options.ProgId = "Opc.Classic.Samples.LoopbackDemo.1";
            options.FriendlyName = "Opc.Classic Loopback Demo DA Server";
            options.ListenAddress = "inmemory://loopback";
        });
        builder.Services.AddSingleton<LoopbackDaRuntime>();
        builder.Services.AddSingleton(static services => new InMemoryCallChannel(
            services.GetRequiredService<LoopbackDaRuntime>().DispatchAsync));
        builder.Services.AddSingleton<LoopbackDaClient>();
        builder.Services.AddHostedService<LoopbackDemoService>();

        await builder.Build().RunAsync().ConfigureAwait(false);
        return 0;
    }
}
