//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Versioning;
using Microsoft.Win32;
using Opc.Classic.Hosting;
using Opc.Classic.Hosting.Windows;
using TUnit.Core;

namespace Opc.Classic.Hosting.Tests.Windows;

/// <summary>
/// HKCU-isolated unit tests for <see cref="WindowsComRegistration" />. Each test
/// uses a unique CLSID + ProgID generated per-run so they don't collide on the
/// per-server registration tree. The tests are serialized via
/// <see cref="NotInParallelAttribute" /> so they don't race on the shared
/// <c>Component Categories</c> subtree, and every test wraps registration in
/// <c>try/finally</c> so HKCU is left clean even on failure.
/// </summary>
[SupportedOSPlatform("windows")]
[NotInParallel]
public sealed class WindowsComRegistrationTests
{
    [Test]
    public async Task RegisterLocalServer_WritesAllExpectedKeys()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        await RunWindowsAsync(static (registration, exePath) =>
        {
            string clsidKeyPath = ClsidKeyPath(registration);
            using RegistryKey hkcuClasses = OpenHkcuClasses(RegistryView.Registry64);
            using RegistryKey? clsidKey = hkcuClasses.OpenSubKey(clsidKeyPath);
            Assert(clsidKey is not null, $"Missing CLSID subkey {clsidKeyPath}");

            object? friendly = clsidKey!.GetValue(null);
            Assert(string.Equals((string?)friendly, registration.FriendlyName, StringComparison.Ordinal),
                $"CLSID default value should be FriendlyName, got '{friendly}'");

            using RegistryKey? localServer = clsidKey.OpenSubKey("LocalServer32");
            Assert(localServer is not null, "Missing LocalServer32 subkey");
            object? localServerPath = localServer!.GetValue(null);
            Assert(string.Equals((string?)localServerPath, $"\"{exePath}\"", StringComparison.Ordinal),
                $"LocalServer32 should be quoted exe path, got '{localServerPath}'");

            using RegistryKey? progIdSubkey = clsidKey.OpenSubKey("ProgID");
            Assert(progIdSubkey is not null, "Missing CLSID\\ProgID subkey");
            Assert(string.Equals((string?)progIdSubkey!.GetValue(null), registration.ProgId, StringComparison.Ordinal),
                "CLSID\\ProgID value mismatch");

            using RegistryKey? viSubkey = clsidKey.OpenSubKey("VersionIndependentProgID");
            Assert(viSubkey is not null, "Missing CLSID\\VersionIndependentProgID subkey");
            Assert(string.Equals((string?)viSubkey!.GetValue(null), VersionIndependent(registration.ProgId), StringComparison.Ordinal),
                "CLSID\\VersionIndependentProgID value mismatch");
        });
    }

    [Test]
    public async Task RegisterLocalServer_WritesAppIdAsNamedValueNotSubkey()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        await RunWindowsAsync(static (registration, _) =>
        {
            using RegistryKey hkcuClasses = OpenHkcuClasses(RegistryView.Registry64);
            using RegistryKey? clsidKey = hkcuClasses.OpenSubKey(ClsidKeyPath(registration));
            Assert(clsidKey is not null, "CLSID key missing");

            // AppID must be a NAMED VALUE, not a subkey. COM relies on the named value.
            object? appIdValue = clsidKey!.GetValue("AppID");
            Assert(appIdValue is not null, "AppID named value is missing on CLSID key");
            Assert(string.Equals((string?)appIdValue, $"{{{registration.Clsid:D}}}", StringComparison.Ordinal),
                $"AppID named value should be '{{clsid}}', got '{appIdValue}'");

            using RegistryKey? appIdSubkey = clsidKey.OpenSubKey("AppID");
            Assert(appIdSubkey is null, "AppID must be a named value, not a subkey");
        });
    }

    [Test]
    public async Task RegisterLocalServer_WritesImplementedCategoriesAndComponentCategoryDescriptions()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        await RunWindowsAsync(static (registration, _) =>
        {
            using RegistryKey hkcuClasses = OpenHkcuClasses(RegistryView.Registry64);

            using RegistryKey? clsidKey = hkcuClasses.OpenSubKey(ClsidKeyPath(registration));
            Assert(clsidKey is not null, "CLSID key missing");
            using RegistryKey? implKey = clsidKey!.OpenSubKey("Implemented Categories");
            Assert(implKey is not null, "Implemented Categories subkey missing");

            string da20 = $"{{{OpcComponentCategories.OpcDaServer20.CategoryId:D}}}";
            string da30 = $"{{{OpcComponentCategories.OpcDaServer30.CategoryId:D}}}";
            Assert(implKey!.OpenSubKey(da20) is not null, $"Missing {da20} membership");
            Assert(implKey.OpenSubKey(da30) is not null, $"Missing {da30} membership");

            using RegistryKey? categoriesRoot = hkcuClasses.OpenSubKey("Component Categories");
            Assert(categoriesRoot is not null, "Component Categories root missing");
            using RegistryKey? da20Desc = categoriesRoot!.OpenSubKey($"{da20}\\409");
            Assert(da20Desc is not null, $"Component Categories\\{da20}\\409 missing");
            Assert(string.Equals(
                (string?)da20Desc!.GetValue(null),
                OpcComponentCategories.OpcDaServer20.Description,
                StringComparison.Ordinal),
                "DA 2.0 description mismatch");
        });
    }

    [Test]
    public async Task RegisterLocalServer_WritesProgIdAndVersionIndependentProgIdReverseAliases()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        await RunWindowsAsync(static (registration, _) =>
        {
            using RegistryKey hkcuClasses = OpenHkcuClasses(RegistryView.Registry64);

            using RegistryKey? versionedProgId = hkcuClasses.OpenSubKey(registration.ProgId);
            Assert(versionedProgId is not null, $"Missing {registration.ProgId} alias");
            using RegistryKey? versionedClsid = versionedProgId!.OpenSubKey("CLSID");
            Assert(versionedClsid is not null, $"Missing {registration.ProgId}\\CLSID");
            Assert(string.Equals(
                (string?)versionedClsid!.GetValue(null),
                $"{{{registration.Clsid:D}}}",
                StringComparison.Ordinal),
                "Versioned ProgID->CLSID alias mismatch");

            string viProgId = VersionIndependent(registration.ProgId);
            Assert(!string.Equals(viProgId, registration.ProgId, StringComparison.Ordinal),
                "Test ProgID should have a numeric version suffix");

            using RegistryKey? viKey = hkcuClasses.OpenSubKey(viProgId);
            Assert(viKey is not null, $"Missing {viProgId} alias");
            using RegistryKey? viClsid = viKey!.OpenSubKey("CLSID");
            Assert(viClsid is not null, $"Missing {viProgId}\\CLSID");
            Assert(string.Equals(
                (string?)viClsid!.GetValue(null),
                $"{{{registration.Clsid:D}}}",
                StringComparison.Ordinal),
                "Version-independent ProgID->CLSID alias mismatch");
            using RegistryKey? curVer = viKey.OpenSubKey("CurVer");
            Assert(curVer is not null, $"Missing {viProgId}\\CurVer");
            Assert(string.Equals(
                (string?)curVer!.GetValue(null),
                registration.ProgId,
                StringComparison.Ordinal),
                "CurVer should point to versioned ProgID");
        });
    }

    [Test]
    public async Task UnregisterLocalServer_RemovesPerServerKeysAndLeavesComponentCategoriesAlone()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcClsidRegistration registration = NewRegistration();
        string exePath = TestExePath();
        try
        {
            WindowsComRegistration.RegisterLocalServer(
                registration,
                exePath,
                RegistryHive.CurrentUser,
                views: null,
                implementedCategories:
                [
                    OpcComponentCategories.OpcDaServer20,
                ]);

            WindowsComRegistration.UnregisterLocalServer(
                registration,
                RegistryHive.CurrentUser);

            using RegistryKey hkcuClasses = OpenHkcuClasses(RegistryView.Registry64);
            Assert(hkcuClasses.OpenSubKey(ClsidKeyPath(registration)) is null,
                "CLSID subtree was not removed");
            Assert(hkcuClasses.OpenSubKey($"AppID\\{{{registration.Clsid:D}}}") is null,
                "AppID subtree was not removed");
            Assert(hkcuClasses.OpenSubKey(registration.ProgId) is null,
                "Versioned ProgID alias was not removed");
            Assert(hkcuClasses.OpenSubKey(VersionIndependent(registration.ProgId)) is null,
                "Version-independent ProgID alias was not removed");

            // Component Categories description is shared and must remain after unregister.
            string da20 = $"{{{OpcComponentCategories.OpcDaServer20.CategoryId:D}}}";
            using RegistryKey? categoriesRoot = hkcuClasses.OpenSubKey("Component Categories");
            Assert(categoriesRoot?.OpenSubKey($"{da20}\\409") is not null,
                "Component Categories description should not be removed on unregister");
        }
        finally
        {
            // Best-effort cleanup if the test failed partway through
            WindowsComRegistration.UnregisterLocalServer(registration, RegistryHive.CurrentUser);
            CleanupSharedCategoryDescription(OpcComponentCategories.OpcDaServer20.CategoryId);
        }

        await Task.CompletedTask;
    }

    [Test]
    public async Task RegisterLocalServer_WritesToBothRegistryViewsByDefault()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        await RunWindowsAsync(static (registration, _) =>
        {
            foreach (RegistryView view in new[] { RegistryView.Registry32, RegistryView.Registry64 })
            {
                using RegistryKey hkcuClasses = OpenHkcuClasses(view);
                Assert(hkcuClasses.OpenSubKey(ClsidKeyPath(registration)) is not null,
                    $"CLSID key missing in {view} view");
            }
        });
    }

    [Test]
    public async Task RegisterLocalServer_IsIdempotent()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcClsidRegistration registration = NewRegistration();
        string exePath = TestExePath();
        try
        {
            WindowsComRegistration.RegisterLocalServer(
                registration,
                exePath,
                RegistryHive.CurrentUser);

            WindowsComRegistration.RegisterLocalServer(
                registration,
                exePath,
                RegistryHive.CurrentUser);

            using RegistryKey hkcuClasses = OpenHkcuClasses(RegistryView.Registry64);
            using RegistryKey? clsidKey = hkcuClasses.OpenSubKey(ClsidKeyPath(registration));
            Assert(clsidKey is not null, "CLSID key missing after double-register");
            object? appIdValue = clsidKey!.GetValue("AppID");
            Assert(string.Equals(
                (string?)appIdValue,
                $"{{{registration.Clsid:D}}}",
                StringComparison.Ordinal),
                "AppID named value should match after double-register");
        }
        finally
        {
            WindowsComRegistration.UnregisterLocalServer(registration, RegistryHive.CurrentUser);
        }

        await Task.CompletedTask;
    }

    // ----- helpers -----

    [SupportedOSPlatform("windows")]
    private static async Task RunWindowsAsync(Action<OpcClsidRegistration, string> verify)
    {
        OpcClsidRegistration registration = NewRegistration();
        string exePath = TestExePath();
        Guid sharedCategory = OpcComponentCategories.OpcDaServer20.CategoryId;

        try
        {
            WindowsComRegistration.RegisterLocalServer(
                registration,
                exePath,
                RegistryHive.CurrentUser,
                views: null,
                implementedCategories:
                [
                    OpcComponentCategories.OpcDaServer20,
                    OpcComponentCategories.OpcDaServer30,
                ]);

            verify(registration, exePath);
        }
        finally
        {
            WindowsComRegistration.UnregisterLocalServer(registration, RegistryHive.CurrentUser);
            CleanupSharedCategoryDescription(sharedCategory);
            CleanupSharedCategoryDescription(OpcComponentCategories.OpcDaServer30.CategoryId);
        }

        await Task.CompletedTask;
    }

    [SupportedOSPlatform("windows")]
    private static void CleanupSharedCategoryDescription(Guid catId)
    {
        foreach (RegistryView view in new[] { RegistryView.Registry32, RegistryView.Registry64 })
        {
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view);
            using RegistryKey? classes = baseKey.OpenSubKey(@"Software\Classes", writable: true);
            using RegistryKey? categories = classes?.OpenSubKey("Component Categories", writable: true);
            categories?.DeleteSubKeyTree($"{{{catId:D}}}", throwOnMissingSubKey: false);
        }
    }

    [SupportedOSPlatform("windows")]
    private static RegistryKey OpenHkcuClasses(RegistryView view)
    {
        RegistryKey baseKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, view);
        return baseKey.OpenSubKey(@"Software\Classes")
            ?? throw new InvalidOperationException("HKCU\\Software\\Classes missing.");
    }

    private static OpcClsidRegistration NewRegistration()
    {
        Guid clsid = Guid.CreateVersion7();
        string suffix = clsid.ToString("N", CultureInfo.InvariantCulture)[..8];
        string progId = $"Test.OpcClassic.{suffix}.1";
        return new OpcClsidRegistration(
            Clsid: clsid,
            ProgId: progId,
            AssemblyName: "Test.OpcClassic",
            TypeName: "Test.OpcClassic.SampleServer",
            FriendlyName: $"Test OPC Server {suffix}");
    }

    private static string TestExePath() =>
        System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "Opc.Classic.Tests.Sample.exe");

    private static string ClsidKeyPath(OpcClsidRegistration registration) =>
        $"CLSID\\{{{registration.Clsid:D}}}";

    private static string VersionIndependent(string progId)
    {
        int dot = progId.LastIndexOf('.');
        return dot > 0 ? progId[..dot] : progId;
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
