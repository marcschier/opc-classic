// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Hosting;
using Opc.Classic.Hosting.Windows;

namespace Opc.Classic.Samples.AeServer;

internal static class Program
{
    private static readonly Guid SampleClsid = new("C4BF6E70-3BA2-4F9C-AE3D-8F6C1D9F2B4F");
    private const string SampleProgId = "Opc.Classic.Samples.AeServer.1";
    private const string SampleFriendlyName = "Opc.Classic Sample AE Server";
    private const string SampleAssemblyName = "Opc.Classic.Samples.AeServer";
    private const string SampleTypeName = "Opc.Classic.Samples.AeServer.SampleAeServer";

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
            OpcComponentCategories.OpcAeServer10,
        ];

        if (SampleServerRegistrationCommand.TryHandle(args, registration, implementedCategories, out int registrationExitCode))
        {
            return registrationExitCode;
        }

        bool embedded = SampleServerRegistrationCommand.HasEmbeddingFlag(args);
        int port = int.TryParse(
            Environment.GetEnvironmentVariable("OPC_CLASSIC_SAMPLE_PORT"),
            out int parsed) && parsed > 0 ? parsed : 51301;
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
        builder.Services.AddOpcAeServer<SampleAeServer>(opt =>
        {
            opt.Clsid = SampleClsid;
            opt.ProgId = SampleProgId;
            opt.FriendlyName = SampleFriendlyName;
            opt.ListenAddress = listenAddress;
        });
        builder.Services.AddHostedService<EventEmitter>();

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
        var serverImpl = services.GetRequiredService<IOpcAeServer>();
        ComClassObjectRegistrar.InitializeMultithreaded();
        uint cookie = ComClassObjectRegistrar.RegisterClassObject(
            SampleClsid,
            createInstanceCallback: requestedIid =>
                Opc.Classic.Ae.Hosting.Windows.OpcAeServerCcw.Create(serverImpl, requestedIid));
        ComClassObjectRegistrar.ResumeClassObjects();
        return cookie;
    }
}
