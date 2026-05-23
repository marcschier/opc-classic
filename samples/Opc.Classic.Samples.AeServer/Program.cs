// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Hosting;

namespace Opc.Classic.Samples.AeServer;

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
        host.Services.AddOpcAeServer<SampleAeServer>(static opt =>
        {
            opt.Clsid = new Guid("C4BF6E70-3BA2-4F9C-AE3D-8F6C1D9F2B4F");
            opt.ProgId = "Opc.Classic.Samples.AeServer.1";
            opt.FriendlyName = "Opc.Classic Sample AE Server";
            opt.ListenAddress = "127.0.0.1:0";
        });
        host.Services.AddHostedService<EventEmitter>();

        await host.Build().RunAsync().ConfigureAwait(false);
        return 0;
    }
}
