// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Hosting;

namespace Opc.Classic.Samples.DaServer;

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
        host.Services.AddSingleton<TagTree>();
        host.Services.AddOpcDaServer<SampleDaServer>(static opt =>
        {
            opt.Clsid = new Guid("B3AE5D6F-2A91-4F8B-9D2C-7E5B0C8F1A3E");
            opt.ProgId = "Opc.Classic.Samples.DaServer.1";
            opt.FriendlyName = "Opc.Classic Sample DA Server";
            opt.ListenAddress = "127.0.0.1:0";
        });

        await host.Build().RunAsync().ConfigureAwait(false);
        return 0;
    }
}
