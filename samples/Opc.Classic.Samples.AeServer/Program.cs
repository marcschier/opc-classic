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
        int port = int.TryParse(
            Environment.GetEnvironmentVariable("OPC_CLASSIC_SAMPLE_PORT"),
            out int parsed) && parsed > 0 ? parsed : 51301;
        string listenAddress = Environment.GetEnvironmentVariable("OPC_CLASSIC_LISTEN_ADDRESS")
            ?? $"0.0.0.0:{port}";
        Console.WriteLine($"Listening on {listenAddress}");

        var host = Host.CreateApplicationBuilder(args);

        host.Logging.ClearProviders();
        host.Logging.AddSimpleConsole(static opt =>
        {
            opt.SingleLine = true;
            opt.TimestampFormat = "HH:mm:ss ";
        });

        host.Services.AddClassicServer();
        host.Services.AddClassicClsidRegistry(host.Configuration);
        host.Services.AddOpcAeServer<SampleAeServer>(opt =>
        {
            opt.Clsid = new Guid("C4BF6E70-3BA2-4F9C-AE3D-8F6C1D9F2B4F");
            opt.ProgId = "Opc.Classic.Samples.AeServer.1";
            opt.FriendlyName = "Opc.Classic Sample AE Server";
            opt.ListenAddress = listenAddress;
        });
        host.Services.AddHostedService<EventEmitter>();

        await host.Build().RunAsync().ConfigureAwait(false);
        return 0;
    }
}
