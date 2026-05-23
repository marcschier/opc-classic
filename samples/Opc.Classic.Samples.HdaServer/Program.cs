// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hosting;

namespace Opc.Classic.Samples.HdaServer;

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

        host.Services.AddClassicServer();
        host.Services.AddClassicClsidRegistry(host.Configuration);
        host.Services.AddSingleton<HistoricalDataStore>();
        host.Services.AddOpcHdaServer<SampleHdaServer>(static opt =>
        {
            opt.Clsid = new Guid("A2BBEA4E-F1C6-469B-8D71-89767DCD2D48");
            opt.ProgId = "Opc.Classic.Samples.HdaServer.1";
            opt.FriendlyName = "Opc.Classic Sample HDA Server";
            opt.ListenAddress = "127.0.0.1:0";
        });

        await host.Build().RunAsync().ConfigureAwait(false);
        return 0;
    }
}
