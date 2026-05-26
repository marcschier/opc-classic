// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors

using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Hosting;

namespace Opc.Classic.Samples.CttServer;

internal static class Program
{
    private static readonly Guid SampleClsid = new("8F7C1B14-9A6E-4E4D-B5E6-5B7DCC1F2B3A");
    private const string SampleProgId = "Opc.Classic.DaSample.1";
    private const string SampleFriendlyName = "Opc.Classic CTT Sample DA Server";
    private const string SampleAssemblyName = "Opc.Classic.Samples.CttServer";
    private const string SampleTypeName = "Opc.Classic.Samples.CttServer.CttDaServer";

    public static async Task<int> Main(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (TryHandleRegistrationCommand(args, out int registrationExitCode))
        {
            return registrationExitCode;
        }

        bool embedded = HasEmbeddingFlag(args);

        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(static opt =>
        {
            opt.SingleLine = true;
            opt.TimestampFormat = "HH:mm:ss ";
        });

        builder.Services.AddClassicServer();
        builder.Services.AddClassicClsidRegistry(builder.Configuration);
        builder.Services.AddOpcDaServer<CttDaServer>(opt =>
        {
            opt.Clsid = SampleClsid;
            opt.ProgId = SampleProgId;
            opt.FriendlyName = SampleFriendlyName;
            opt.ListenAddress = "127.0.0.1:0";
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
                Opc.Classic.Hosting.Windows.ComClassObjectRegistrar.RevokeClassObject(comClassObjectCookie);
                Opc.Classic.Hosting.Windows.ComClassObjectRegistrar.Uninitialize();
            }
        }

        return 0;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static uint RegisterScmFactory(IServiceProvider services)
    {
        // Resolve the managed server instance from DI, build a CCW factory
        // that hands out an OpcDaServerCcw backed by it, and register the
        // class object with SCM via the ocom-6 callback overload.
        // The factory closure captures the IOpcDaServer for the process
        // lifetime (matches the SCM-activation lifecycle).
        var serverImpl = services.GetRequiredService<IOpcDaServer>();
        Opc.Classic.Hosting.Windows.ComClassObjectRegistrar.InitializeApartmentThreaded();
        uint cookie = Opc.Classic.Hosting.Windows.ComClassObjectRegistrar.RegisterClassObject(
            SampleClsid,
            createInstanceCallback: requestedIid =>
                Opc.Classic.Da.Hosting.Windows.OpcDaServerCcw.Create(serverImpl, requestedIid));
        Opc.Classic.Hosting.Windows.ComClassObjectRegistrar.ResumeClassObjects();
        return cookie;
    }

    private static bool TryHandleRegistrationCommand(string[] args, out int exitCode)
    {
        exitCode = 0;
        bool register = HasFlag(args, "--register");
        bool unregister = HasFlag(args, "--unregister");
        if (!register && !unregister)
        {
            return false;
        }

        if (register && unregister)
        {
            Console.Error.WriteLine("Specify only one of --register or --unregister.");
            exitCode = 2;
            return true;
        }

        if (!OperatingSystem.IsWindows())
        {
            Console.Error.WriteLine(
                "COM registration requires Windows; current OS is not supported.");
            exitCode = 3;
            return true;
        }

        ExecuteWindowsRegistration(args, register);
        return true;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void ExecuteWindowsRegistration(string[] args, bool register)
    {
        RegistryHive hive = ParseHive(GetFlagValue(args, "--registry-hive"));
        IReadOnlyList<RegistryView>? views = ParseViews(GetFlagValue(args, "--registry-view"));

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

        string viewsDescription = DescribeViews(views);
        string clsidText = SampleClsid.ToString("B", CultureInfo.InvariantCulture);

        if (register)
        {
            string exePath = Environment.ProcessPath
                ?? throw new InvalidOperationException(
                    "Environment.ProcessPath is unavailable; cannot resolve executable path for registration.");

            Opc.Classic.Hosting.Windows.WindowsComRegistration.RegisterLocalServer(
                registration,
                exePath,
                hive,
                views,
                implementedCategories);

            Console.WriteLine(
                $"Registered {SampleProgId} ({clsidText}) under {hive}\\Software\\Classes ({viewsDescription}).");
        }
        else
        {
            Opc.Classic.Hosting.Windows.WindowsComRegistration.UnregisterLocalServer(
                registration,
                hive,
                views);

            Console.WriteLine(
                $"Unregistered {SampleProgId} ({clsidText}) from {hive}\\Software\\Classes ({viewsDescription}).");
        }
    }

    private static bool HasFlag(string[] args, string flag)
    {
        foreach (string arg in args)
        {
            if (string.Equals(arg, flag, StringComparison.Ordinal))
            {
                return true;
            }
            if (arg.StartsWith(flag + "=", StringComparison.Ordinal))
            {
                return true;
            }
        }
        return false;
    }

    private static bool HasEmbeddingFlag(string[] args)
    {
        foreach (string arg in args)
        {
            if (string.Equals(arg, "-Embedding", StringComparison.OrdinalIgnoreCase)
                || string.Equals(arg, "/Embedding", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private static string? GetFlagValue(string[] args, string flag)
    {
        string prefix = flag + "=";
        foreach (string arg in args)
        {
            if (arg.StartsWith(prefix, StringComparison.Ordinal))
            {
                return arg[prefix.Length..];
            }
        }
        return null;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static RegistryHive ParseHive(string? value) => value?.ToLowerInvariant() switch
    {
        null or "" or "hklm" or "localmachine" => RegistryHive.LocalMachine,
        "hkcu" or "currentuser" => RegistryHive.CurrentUser,
        _ => throw new ArgumentException(
            $"Unknown --registry-hive value '{value}'. Expected hklm or hkcu.", nameof(value)),
    };

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static IReadOnlyList<RegistryView>? ParseViews(string? value) => value?.ToLowerInvariant() switch
    {
        null or "" or "both" or "all" => null,
        "32" or "registry32" => [RegistryView.Registry32],
        "64" or "registry64" => [RegistryView.Registry64],
        _ => throw new ArgumentException(
            $"Unknown --registry-view value '{value}'. Expected 32, 64, or both.", nameof(value)),
    };

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static string DescribeViews(IReadOnlyList<RegistryView>? views)
    {
        if (views is null || views.Count == 0)
        {
            return "views: 32+64";
        }
        return "views: " + string.Join('+', views);
    }
}
