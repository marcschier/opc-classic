//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace Opc.Classic.Hosting.Windows;

/// <summary>
/// Shared <c>--register</c> / <c>--unregister</c> command-line handler for
/// the OPC Classic sample servers. Wraps <see cref="WindowsComRegistration"/>
/// with the standard CLI surface (<c>--register</c>, <c>--unregister</c>,
/// <c>--registry-hive=hklm|hkcu</c>, <c>--registry-view=32|64|both</c>) so
/// every server sample exposes the same shape.
/// </summary>
/// <remarks>
/// <para>
/// Used by <c>Opc.Classic.Samples.DaServer</c>,
/// <c>Opc.Classic.Samples.CttServer</c>,
/// <c>Opc.Classic.Samples.AeServer</c>,
/// <c>Opc.Classic.Samples.HdaServer</c>, and
/// <c>Opc.Classic.Samples.OpcSecurityServer</c>. Each sample wires this in
/// the first lines of <c>Main</c> so the EXE can self-register as an OPC
/// LocalServer32 without requiring an external installer or
/// <c>regsvr32</c>-style proxy.
/// </para>
/// <para>
/// Per-user (<c>--registry-hive=hkcu</c>) registration is the elevation-free
/// default for developer machines. Production deployments use
/// <c>--registry-hive=hklm</c> (the default) which requires an elevated
/// process. The Windows COM SCM resolves <c>HKCR</c> as a merged view of
/// both hives, so OPCEnum / OpcTestClient see entries from either path.
/// </para>
/// </remarks>
public static class SampleServerRegistrationCommand
{
    /// <summary>
    /// Inspects <paramref name="args"/> for <c>--register</c> /
    /// <c>--unregister</c> and, if present, executes the requested registry
    /// operation against the merged <c>HKCR</c> layout used by Windows COM.
    /// Returns <see langword="true"/> when the command was handled (caller
    /// should then exit with <paramref name="exitCode"/>) or
    /// <see langword="false"/> when no registration flag was present
    /// (caller proceeds with normal server startup).
    /// </summary>
    /// <param name="args">Process <c>Main</c> arguments.</param>
    /// <param name="registration">
    /// CLSID / ProgID / friendly-name metadata to write. The
    /// <see cref="OpcClsidRegistration.AssemblyName"/> and
    /// <see cref="OpcClsidRegistration.TypeName"/> fields are written into
    /// the InProcServer32 hint metadata but are not used for activation —
    /// <see cref="WindowsComRegistration.RegisterLocalServer"/> registers
    /// the EXE under <c>LocalServer32</c> using <see cref="Environment.ProcessPath"/>.
    /// </param>
    /// <param name="implementedCategories">
    /// OPC component categories (e.g. <c>CATID_OPCDAServer20</c>,
    /// <c>CATID_OPCDAServer30</c>) to record under
    /// <c>HKCR\CLSID\{...}\Implemented Categories\</c>. OPCEnum and
    /// <c>OpcTestClient.exe</c> filter on these CATIDs when enumerating
    /// servers via <c>IOPCServerList::EnumClassesOfCategories</c>.
    /// </param>
    /// <param name="exitCode">
    /// Process exit code when the command was handled. Standard codes:
    /// <c>0</c> on success, <c>2</c> when both <c>--register</c> and
    /// <c>--unregister</c> are passed (mutually exclusive), <c>3</c> when
    /// the OS is not Windows (registration is not supported off-Windows),
    /// <c>4</c> when an argument is malformed.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the command was handled and the caller
    /// should terminate (use <paramref name="exitCode"/>);
    /// <see langword="false"/> otherwise.
    /// </returns>
    public static bool TryHandle(
        string[] args,
        OpcClsidRegistration registration,
        IReadOnlyList<OpcComponentCategory> implementedCategories,
        out int exitCode)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentNullException.ThrowIfNull(implementedCategories);

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

        try
        {
            ExecuteWindowsRegistration(args, register, registration, implementedCategories);
            return true;
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine(ex.Message);
            exitCode = 4;
            return true;
        }
    }

    [SupportedOSPlatform("windows")]
    private static void ExecuteWindowsRegistration(
        string[] args,
        bool register,
        OpcClsidRegistration registration,
        IReadOnlyList<OpcComponentCategory> implementedCategories)
    {
        RegistryHive hive = ParseHive(GetFlagValue(args, "--registry-hive"));
        IReadOnlyList<RegistryView>? views = ParseViews(GetFlagValue(args, "--registry-view"));

        string viewsDescription = DescribeViews(views);
        string clsidText = registration.Clsid.ToString("B", CultureInfo.InvariantCulture);

        if (register)
        {
            string exePath = Environment.ProcessPath
                ?? throw new InvalidOperationException(
                    "Environment.ProcessPath is unavailable; cannot resolve executable path for registration.");

            WindowsComRegistration.RegisterLocalServer(
                registration,
                exePath,
                hive,
                views,
                implementedCategories);

            Console.WriteLine(
                $"Registered {registration.ProgId} ({clsidText}) under {hive}\\Software\\Classes ({viewsDescription}).");
        }
        else
        {
            WindowsComRegistration.UnregisterLocalServer(
                registration,
                hive,
                views);

            Console.WriteLine(
                $"Unregistered {registration.ProgId} ({clsidText}) from {hive}\\Software\\Classes ({viewsDescription}).");
        }
    }

    /// <summary>
    /// Returns <see langword="true"/> when <paramref name="args"/> contains
    /// the COM SCM <c>-Embedding</c> / <c>/Embedding</c> flag indicating
    /// that the process was launched by the Windows Service Control
    /// Manager to satisfy an out-of-process activation. Samples register
    /// their class object via
    /// <see cref="ComClassObjectRegistrar.RegisterClassObject(Guid, Func{Guid, IntPtr}?, bool)"/>
    /// only when this flag is present, so direct console launches don't
    /// pollute the SCM registry.
    /// </summary>
    public static bool HasEmbeddingFlag(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
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

    [SupportedOSPlatform("windows")]
    private static RegistryHive ParseHive(string? value) => value?.ToLowerInvariant() switch
    {
        null or "" or "hklm" or "localmachine" => RegistryHive.LocalMachine,
        "hkcu" or "currentuser" => RegistryHive.CurrentUser,
        _ => throw new ArgumentException(
            $"Unknown --registry-hive value '{value}'. Expected hklm or hkcu.", nameof(value)),
    };

    [SupportedOSPlatform("windows")]
    private static IReadOnlyList<RegistryView>? ParseViews(string? value) => value?.ToLowerInvariant() switch
    {
        null or "" or "both" or "all" => null,
        "32" or "registry32" => [RegistryView.Registry32],
        "64" or "registry64" => [RegistryView.Registry64],
        _ => throw new ArgumentException(
            $"Unknown --registry-view value '{value}'. Expected 32, 64, or both.", nameof(value)),
    };

    [SupportedOSPlatform("windows")]
    private static string DescribeViews(IReadOnlyList<RegistryView>? views)
    {
        if (views is null || views.Count == 0)
        {
            return "views: 32+64";
        }
        return "views: " + string.Join('+', views);
    }
}
