// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
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

        host.Services.AddSingleton<SampleAeServer>();
        host.Services.AddSingleton<InProcessAeServer>();
        host.Services.AddSingleton<IOpcAeServer>(static sp => sp.GetRequiredService<InProcessAeServer>());
        host.Services.AddSingleton<OpcAeServerDispatcher>();
        host.Services.AddSingleton(static sp =>
        {
            var dispatcher = sp.GetRequiredService<OpcAeServerDispatcher>();
            return new InMemoryCallChannel((iid, opnum, payload, ct) =>
                dispatcher.DispatchAsync(iid, opnum, payload, ct));
        });
        host.Services.AddSingleton<IOPCEventServer>(static sp =>
            new IOPCEventServerClientProxy(sp.GetRequiredService<InMemoryCallChannel>()));
        host.Services.AddSingleton<LoopbackAeClient>();
        host.Services.AddHostedService<AeClientDemo>();

        await host.Build().RunAsync().ConfigureAwait(false);
        return 0;
    }
}
