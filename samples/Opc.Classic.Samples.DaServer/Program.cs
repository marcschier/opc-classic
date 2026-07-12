// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Globalization;
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

    // DIAGNOSTIC (temporary): mirror the ccw-trace.log lifecycle so the SCM-launched
    // (console-less) server records whether its generic host crashes/exits after RPCSS
    // marshals its interfaces. Gated on the same ccw-trace.enabled marker so it is inert
    // unless activation tracing is turned on.
    private static readonly Lock s_diagGate = new();

    private static void DiagLog(string message)
    {
        try
        {
            string baseDir = AppContext.BaseDirectory;
            if (string.IsNullOrEmpty(baseDir) || !File.Exists(Path.Combine(baseDir, "ccw-trace.enabled")))
            {
                return;
            }

            string line = string.Create(
                CultureInfo.InvariantCulture,
                $"{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffffffZ} [pid {Environment.ProcessId} tid {Environment.CurrentManagedThreadId}] [host] {message}{Environment.NewLine}");
            lock (s_diagGate)
            {
                File.AppendAllText(Path.Combine(baseDir, "ccw-trace.log"), line);
            }
        }
#pragma warning disable CA1031 // A diagnostic must never disrupt the server it is observing.
        catch (Exception)
#pragma warning restore CA1031
        {
            // Intentionally swallowed: a diagnostic write must never disrupt the server.
            return;
        }
    }

    public static async Task<int> Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        AppDomain.CurrentDomain.UnhandledException += static (_, e) =>
            DiagLog($"AppDomain.UnhandledException terminating={e.IsTerminating}: {e.ExceptionObject}");
        AppDomain.CurrentDomain.ProcessExit += static (_, _) => DiagLog("ProcessExit");
        TaskScheduler.UnobservedTaskException += static (_, e) =>
        {
            DiagLog($"TaskScheduler.UnobservedTaskException: {e.Exception}");
            e.SetObserved();
        };

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
        DiagLog($"Main entered embedded={embedded} args=[{string.Join(' ', args)}]");
        int port = int.TryParse(
            Environment.GetEnvironmentVariable("OPC_CLASSIC_SAMPLE_PORT"),
            out int parsed) && parsed > 0 ? parsed : 51300;
        // When SCM activates the sample (-Embedding), bind an ephemeral
        // port instead of the fixed default so multiple SCM-launched
        // instances (or repeated activations after a previous instance is
        // still alive) don't fail with EADDRINUSE during host startup.
        // DCOM activation doesn't depend on the sample's TCP listener --
        // it routes through the CoRegisterClassObject factory directly --
        // so any port is fine.
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
            DiagLog($"RegisterScmFactory returned cookie={comClassObjectCookie}");
        }

        try
        {
            DiagLog("host.RunAsync starting");
            await host.RunAsync().ConfigureAwait(false);
            DiagLog("host.RunAsync returned normally");
        }
        catch (Exception ex)
        {
            DiagLog($"host.RunAsync THREW: {ex}");
            throw;
        }
        finally
        {
            if (embedded && OperatingSystem.IsWindows() && comClassObjectCookie != 0)
            {
                DiagLog("revoking class object + CoUninitialize");
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
