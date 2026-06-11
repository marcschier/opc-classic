// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hosting;
using Opc.Classic.Hosting.Windows;

namespace Opc.Classic.Samples.HdaServer;

internal static class Program
{
    private static readonly Guid SampleClsid = new("A2BBEA4E-F1C6-469B-8D71-89767DCD2D48");
    private const string SampleProgId = "Opc.Classic.Samples.HdaServer.1";
    private const string SampleFriendlyName = "Opc.Classic Sample HDA Server";
    private const string SampleAssemblyName = "Opc.Classic.Samples.HdaServer";
    private const string SampleTypeName = "Opc.Classic.Samples.HdaServer.SampleHdaServer";

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
            OpcComponentCategories.OpcHdaServer10,
        ];

        if (SampleServerRegistrationCommand.TryHandle(args, registration, implementedCategories, out int registrationExitCode))
        {
            return registrationExitCode;
        }

        bool embedded = SampleServerRegistrationCommand.HasEmbeddingFlag(args);
        int port = int.TryParse(
            Environment.GetEnvironmentVariable("OPC_CLASSIC_SAMPLE_PORT"),
            out int parsed) && parsed > 0 ? parsed : 51302;
        // When SCM activates the sample (-Embedding), bind an ephemeral
        // port to avoid EADDRINUSE if a previous SCM-launched instance
        // is still alive. DCOM activation doesn't depend on the sample's
        // TCP listener -- it routes through CoRegisterClassObject.
        string defaultBind = embedded ? "127.0.0.1:0" : $"0.0.0.0:{port}";
        string listenAddress = Environment.GetEnvironmentVariable("OPC_CLASSIC_LISTEN_ADDRESS")
            ?? defaultBind;
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
        builder.Services.AddSingleton<HistoricalDataStore>();
        builder.Services.AddOpcHdaServer<SampleHdaServer>(opt =>
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
        var serverImpl = services.GetRequiredService<IOpcHdaServer>();
        ComClassObjectRegistrar.InitializeMultithreaded();
        uint cookie = ComClassObjectRegistrar.RegisterClassObject(
            SampleClsid,
            createInstanceCallback: requestedIid =>
                Opc.Classic.Hda.Hosting.Windows.OpcHdaServerCcw.Create(serverImpl, requestedIid));
        ComClassObjectRegistrar.ResumeClassObjects();
        return cookie;
    }
}
