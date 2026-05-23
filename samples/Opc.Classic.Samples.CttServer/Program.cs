// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Hosting;

namespace Opc.Classic.Samples.CttServer;

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
        host.Services.AddOpcDaServer<CttDaServer>(static opt =>
        {
            opt.Clsid = new Guid("8F7C1B14-9A6E-4E4D-B5E6-5B7DCC1F2B3A");
            opt.ProgId = "Opc.Classic.DaSample.1";
            opt.FriendlyName = "Opc.Classic CTT Sample DA Server";
            opt.ListenAddress = "127.0.0.1:0";
        });

        await host.Build().RunAsync().ConfigureAwait(false);
        return 0;
    }
}
