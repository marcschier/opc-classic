// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Hosting;
using Opc.Classic.Hosting.Windows;

namespace Opc.Classic.Samples.DaServer;

internal static class Program
{
    private static readonly Guid SampleClsid = new("B3AE5D6F-2A91-4F8B-9D2C-7E5B0C8F1A3E");
    private const string SampleProgId = "Opc.Classic.Samples.DaServer.1";
    private const string SampleFriendlyName = "Opc.Classic Sample DA Server";
    private const string SampleAssemblyName = "Opc.Classic.Samples.DaServer";
    private const string SampleTypeName = "Opc.Classic.Samples.DaServer.SampleDaServer";

    public static async Task<int> Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var registration = new OpcClsidRegistration(
            Clsid: SampleClsid,
            ProgId: SampleProgId,
            AssemblyName: SampleAssemblyName,
            TypeName: SampleTypeName,
            FriendlyName: SampleFriendlyName);
        IReadOnlyList<OpcComponentCategory> implementedCategories =
        [
            OpcComponentCategories.OpcDaServer20,
            OpcComponentCategories.OpcDaServer30,
        ];

        if (SampleServerRegistrationCommand.TryHandle(args, registration, implementedCategories, out int registrationExitCode))
        {
            return registrationExitCode;
        }

        bool embedded = SampleServerRegistrationCommand.HasEmbeddingFlag(args);
        int port = int.TryParse(
            Environment.GetEnvironmentVariable("OPC_CLASSIC_SAMPLE_PORT"),
            out int parsed) && parsed > 0 ? parsed : 51300;
        string listenAddress = Environment.GetEnvironmentVariable("OPC_CLASSIC_LISTEN_ADDRESS")
            ?? $"0.0.0.0:{port}";
        Console.WriteLine($"Listening on {listenAddress}");

        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(static opt =>
        {
            opt.SingleLine = true;
            opt.TimestampFormat = "HH:mm:ss ";
        });

        builder.Services.AddClassicServer();
        builder.Services.AddClassicClsidRegistry(builder.Configuration);
        builder.Services.AddSingleton<TagTree>();
        builder.Services.AddOpcDaServer<SampleDaServer>(opt =>
        {
            opt.Clsid = SampleClsid;
            opt.ProgId = SampleProgId;
            opt.FriendlyName = SampleFriendlyName;
            opt.ListenAddress = listenAddress;
        });

        var host = builder.Build();

        uint comClassObjectCookie = 0;
        if (embedded && OperatingSystem.IsWindows())
        {
            comClassObjectCookie = RegisterScmFactory(host.Services);
        }

        try
        {
            await host.RunAsync().ConfigureAwait(false);
        }
        finally
        {
            if (embedded && OperatingSystem.IsWindows() && comClassObjectCookie != 0)
            {
                ComClassObjectRegistrar.RevokeClassObject(comClassObjectCookie);
                ComClassObjectRegistrar.Uninitialize();
            }
        }

        return 0;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static uint RegisterScmFactory(IServiceProvider services)
    {
        var serverImpl = services.GetRequiredService<IOpcDaServer>();
        ComClassObjectRegistrar.InitializeMultithreaded();
        uint cookie = ComClassObjectRegistrar.RegisterClassObject(
            SampleClsid,
            createInstanceCallback: requestedIid =>
                Opc.Classic.Da.Hosting.Windows.OpcDaServerCcw.Create(serverImpl, requestedIid));
        ComClassObjectRegistrar.ResumeClassObjects();
        return cookie;
    }
}
