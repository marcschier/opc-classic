// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Samples.HdaServer;

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

        host.Services.AddSingleton<HistoricalDataStore>();
        host.Services.AddSingleton<IOpcHdaServer, SampleHdaServer>();
        host.Services.AddSingleton<LoopbackHdaCallRouter>();
        host.Services.AddSingleton<LoopbackHdaClient>();
        host.Services.AddHostedService<HdaClientDemo>();

        await host.Build().RunAsync().ConfigureAwait(false);
        return 0;
    }
}
