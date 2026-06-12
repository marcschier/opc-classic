//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Globalization;
using System.Runtime.Versioning;
using Microsoft.Win32;
using Opc.Classic.Hosting.Windows;
using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Hosting.Tests.Windows;

/// <summary>
/// Tests for <see cref="SampleServerRegistrationCommand"/>. Verifies the
/// command-line contract used by all DA / AE / HDA sample servers: which
/// flag combinations short-circuit startup, which exit codes are returned,
/// and that the underlying <see cref="WindowsComRegistration"/> is invoked
/// with the right hive / view selection.
/// </summary>
public sealed class SampleServerRegistrationCommandTests
{
    [Test]
    public async Task TryHandle_ReturnsFalse_WhenNoFlag()
    {
        OpcClsidRegistration registration = NewRegistration();
        bool handled = SampleServerRegistrationCommand.TryHandle(
            ["--something-else", "--unrelated"],
            registration,
            implementedCategories:
            [
                OpcComponentCategories.OpcDaServer20,
            ],
            out int exitCode);

        await Assert.That(handled).IsFalse();
        await Assert.That(exitCode).IsEqualTo(0);
    }

    [Test]
    public async Task TryHandle_ReturnsExitCode2_WhenBothFlagsPresent()
    {
        OpcClsidRegistration registration = NewRegistration();
        bool handled = SampleServerRegistrationCommand.TryHandle(
            ["--register", "--unregister"],
            registration,
            implementedCategories:
            [
                OpcComponentCategories.OpcDaServer20,
            ],
            out int exitCode);

        await Assert.That(handled).IsTrue();
        await Assert.That(exitCode).IsEqualTo(2);
    }

    [Test]
    [Arguments("-Embedding")]
    [Arguments("/Embedding")]
    [Arguments("-embedding")]
    [Arguments("/EMBEDDING")]
    public async Task HasEmbeddingFlag_AcceptsAllVariants(string flag)
    {
        bool result = SampleServerRegistrationCommand.HasEmbeddingFlag([flag]);
        await Assert.That(result).IsTrue();
    }

    [Test]
    public async Task HasEmbeddingFlag_ReturnsFalse_WhenAbsent()
    {
        bool result = SampleServerRegistrationCommand.HasEmbeddingFlag(["--unrelated"]);
        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task TryHandle_NullArgs_Throws()
    {
        OpcClsidRegistration registration = NewRegistration();
        await Assert.That(() =>
        {
            _ = SampleServerRegistrationCommand.TryHandle(
                null!,
                registration,
                [OpcComponentCategories.OpcDaServer20],
                out _);
        }).Throws<ArgumentNullException>();
    }

    [Test]
    [NotInParallel]
    [SupportedOSPlatform("windows")]
    public async Task TryHandle_Register_WritesRegistryEntries_HkcuView()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcClsidRegistration registration = NewRegistration();
        try
        {
            bool handled = SampleServerRegistrationCommand.TryHandle(
                ["--register", "--registry-hive=hkcu"],
                registration,
                [OpcComponentCategories.OpcDaServer20, OpcComponentCategories.OpcDaServer30],
                out int exitCode);

            await Assert.That(handled).IsTrue();
            await Assert.That(exitCode).IsEqualTo(0);

            using RegistryKey baseKey = RegistryKey.OpenBaseKey(
                RegistryHive.CurrentUser,
                RegistryView.Registry64);
            using RegistryKey? classes = baseKey.OpenSubKey(@"Software\Classes");
            await Assert.That(classes).IsNotNull();

            using RegistryKey? clsidKey = classes!.OpenSubKey($"CLSID\\{{{registration.Clsid:D}}}");
            await Assert.That(clsidKey).IsNotNull();

            using RegistryKey? localServer = clsidKey!.OpenSubKey("LocalServer32");
            await Assert.That(localServer).IsNotNull();
        }
        finally
        {
            WindowsComRegistration.UnregisterLocalServer(registration, RegistryHive.CurrentUser);
            CleanupSharedCategoryDescription(OpcComponentCategories.OpcDaServer20.CategoryId);
            CleanupSharedCategoryDescription(OpcComponentCategories.OpcDaServer30.CategoryId);
        }
    }

    [Test]
    [NotInParallel]
    [SupportedOSPlatform("windows")]
    public async Task TryHandle_Unregister_RemovesRegistryEntries_HkcuView()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcClsidRegistration registration = NewRegistration();
        IReadOnlyList<OpcComponentCategory> cats =
        [
            OpcComponentCategories.OpcDaServer20,
        ];

        try
        {
            _ = SampleServerRegistrationCommand.TryHandle(
                ["--register", "--registry-hive=hkcu"],
                registration,
                cats,
                out _);

            bool handled = SampleServerRegistrationCommand.TryHandle(
                ["--unregister", "--registry-hive=hkcu"],
                registration,
                cats,
                out int exitCode);

            await Assert.That(handled).IsTrue();
            await Assert.That(exitCode).IsEqualTo(0);

            using RegistryKey baseKey = RegistryKey.OpenBaseKey(
                RegistryHive.CurrentUser,
                RegistryView.Registry64);
            using RegistryKey? classes = baseKey.OpenSubKey(@"Software\Classes");
            using RegistryKey? clsidKey = classes?.OpenSubKey($"CLSID\\{{{registration.Clsid:D}}}");
            await Assert.That(clsidKey).IsNull();
        }
        finally
        {
            WindowsComRegistration.UnregisterLocalServer(registration, RegistryHive.CurrentUser);
            CleanupSharedCategoryDescription(OpcComponentCategories.OpcDaServer20.CategoryId);
        }
    }

    [Test]
    [SupportedOSPlatform("windows")]
    public async Task TryHandle_Register_ReturnsExitCode4_WhenHiveValueIsInvalid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcClsidRegistration registration = NewRegistration();
        bool handled = SampleServerRegistrationCommand.TryHandle(
            ["--register", "--registry-hive=nonsense"],
            registration,
            [OpcComponentCategories.OpcDaServer20],
            out int exitCode);

        await Assert.That(handled).IsTrue();
        await Assert.That(exitCode).IsEqualTo(4);
    }

    [Test]
    [SupportedOSPlatform("windows")]
    public async Task TryHandle_Register_ReturnsExitCode4_WhenViewValueIsInvalid()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        OpcClsidRegistration registration = NewRegistration();
        bool handled = SampleServerRegistrationCommand.TryHandle(
            ["--register", "--registry-hive=hkcu", "--registry-view=nonsense"],
            registration,
            [OpcComponentCategories.OpcDaServer20],
            out int exitCode);

        await Assert.That(handled).IsTrue();
        await Assert.That(exitCode).IsEqualTo(4);
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

    private static OpcClsidRegistration NewRegistration()
    {
        Guid clsid = Guid.CreateVersion7();
        string suffix = clsid.ToString("N", CultureInfo.InvariantCulture)[..8];
        return new OpcClsidRegistration(
            Clsid: clsid,
            ProgId: $"Test.OpcClassic.SampleCmd.{suffix}.1",
            AssemblyName: "Test.OpcClassic.SampleCmd",
            TypeName: "Test.OpcClassic.SampleCmd.SampleServer",
            FriendlyName: $"Test OPC Sample {suffix}");
    }
}
