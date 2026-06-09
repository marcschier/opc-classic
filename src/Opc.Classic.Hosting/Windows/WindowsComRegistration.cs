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
/// Writes and removes Windows COM registration entries (HKCR-rooted) for an
/// out-of-process OPC Classic server.
/// </summary>
/// <remarks>
/// <para>
/// OPC Classic clients (CTT, OPCEnum, OpcDaAutomation) discover servers through the
/// standard Windows COM registry layout under <c>HKCR\CLSID</c>, <c>HKCR\AppID</c>,
/// the ProgID alias keys, and <c>HKCR\Component Categories</c>. <c>HKCR</c> itself is
/// a merged view of <c>HKLM\Software\Classes</c> (system-wide) and
/// <c>HKCU\Software\Classes</c> (per-user). This type writes to the underlying physical
/// hive so that registration can run without administrative privileges when targeting
/// the per-user hive (e.g. tests via <see cref="RegistryHive.CurrentUser"/>).
/// </para>
/// <para>
/// The OPC CTT v2.0.15 (and most published OPC Classic clients) ship as 32-bit binaries.
/// On a 64-bit OS, Windows redirects their registry reads to the WoW6432Node view.
/// To make a single managed publish discoverable by both 32-bit and 64-bit clients the
/// default behaviour writes to both <see cref="RegistryView.Registry32"/> and
/// <see cref="RegistryView.Registry64"/>.
/// </para>
/// <para>
/// On a service-hosted activation (COM SCM launching the server as <c>LocalSystem</c>
/// or another principal) the per-user HKCU registration is invisible to the calling
/// principal; production registrations must use <see cref="RegistryHive.LocalMachine"/>
/// and therefore require administrative privileges.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public static class WindowsComRegistration {
    private const string ClassesSubkey = @"Software\Classes";

    /// <summary>
    /// Registers an OPC server as an out-of-process (<c>LocalServer32</c>) COM class.
    /// </summary>
    /// <param name="registration">CLSID/ProgID metadata for the server.</param>
    /// <param name="executablePath">Full path to the server EXE. Will be written quoted under <c>LocalServer32</c>.</param>
    /// <param name="hive">
    /// <see cref="RegistryHive.LocalMachine"/> for system-wide (requires admin) or
    /// <see cref="RegistryHive.CurrentUser"/> for per-user (no admin needed; tests use this).
    /// </param>
    /// <param name="views">
    /// Registry views to write. Passing <see langword="null"/> writes to both
    /// <see cref="RegistryView.Registry32"/> and <see cref="RegistryView.Registry64"/>,
    /// which is the recommended setting for OPC Classic servers (the OPC CTT and most
    /// published OPC Classic clients are 32-bit binaries that read from the 32-bit view).
    /// </param>
    /// <param name="implementedCategories">
    /// The OPC component categories implemented by this server (e.g.
    /// <see cref="OpcComponentCategories.OpcDaServer20"/>,
    /// <see cref="OpcComponentCategories.OpcDaServer30"/>). May be <see langword="null"/>
    /// to register only the bare CLSID without any category membership; the per-server
    /// <c>Implemented Categories</c> subkey is then omitted entirely. The system-wide
    /// <c>Component Categories</c> description (LCID 409) entry is also written so that
    /// clients enumerating via <c>ICatInformation</c> can resolve the human-readable name.
    /// </param>
    public static void RegisterLocalServer(
        OpcClsidRegistration registration,
        string executablePath,
        RegistryHive hive = RegistryHive.LocalMachine,
        IReadOnlyList<RegistryView>? views = null,
        IReadOnlyList<OpcComponentCategory>? implementedCategories = null) {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentException.ThrowIfNullOrEmpty(executablePath);

        foreach (RegistryView v in ResolveViews(views)) {
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, v);
            using RegistryKey classes = baseKey.CreateSubKey(ClassesSubkey, writable: true)
                ?? throw new UnauthorizedAccessException(
                    $"Cannot open {hive}\\{ClassesSubkey} ({v}) for write.");
            WriteClsidTree(classes, registration, executablePath, implementedCategories);
            WriteAppIdTree(classes, registration);
            WriteProgIdAliases(classes, registration);
            if (implementedCategories is { Count: > 0 }) {
                WriteComponentCategoryDescriptions(classes, implementedCategories);
            }
        }
    }

    /// <summary>
    /// Removes the per-server CLSID/AppID/ProgID registrations written by
    /// <see cref="RegisterLocalServer"/>. The shared <c>Component Categories</c>
    /// description subtree is intentionally NOT removed so that other servers
    /// sharing the same CATID remain discoverable.
    /// </summary>
    public static void UnregisterLocalServer(
        OpcClsidRegistration registration,
        RegistryHive hive = RegistryHive.LocalMachine,
        IReadOnlyList<RegistryView>? views = null) {
        ArgumentNullException.ThrowIfNull(registration);

        foreach (RegistryView v in ResolveViews(views)) {
            using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, v);
            using RegistryKey? classes = baseKey.OpenSubKey(ClassesSubkey, writable: true);
            if (classes is null) {
                continue;
            }

            DeleteIfExists(classes, $@"CLSID\{{{registration.Clsid:D}}}");
            DeleteIfExists(classes, $@"AppID\{{{registration.Clsid:D}}}");
            DeleteIfExists(classes, registration.ProgId);

            string versionIndependent = StripVersionSuffix(registration.ProgId);
            if (!string.Equals(versionIndependent, registration.ProgId, StringComparison.Ordinal)) {
                DeleteIfExists(classes, versionIndependent);
            }
        }
    }

    private static void DeleteIfExists(RegistryKey parent, string subkeyPath) {
        using RegistryKey? probe = parent.OpenSubKey(subkeyPath);
        if (probe is null) {
            return;
        }
        probe.Dispose();
        parent.DeleteSubKeyTree(subkeyPath, throwOnMissingSubKey: false);
    }

    private static void WriteClsidTree(
        RegistryKey classes,
        OpcClsidRegistration registration,
        string executablePath,
        IReadOnlyList<OpcComponentCategory>? implementedCategories) {
        string friendly = registration.FriendlyName ?? registration.ProgId;
        string versionIndependent = StripVersionSuffix(registration.ProgId);

        using RegistryKey clsidKey = classes.CreateSubKey(
            $@"CLSID\{{{registration.Clsid:D}}}",
            writable: true)
            ?? throw new UnauthorizedAccessException("Cannot create CLSID subkey.");

        clsidKey.SetValue(null, friendly);
        // AppID is a NAMED VALUE on the CLSID, NOT a subkey. COM uses this to resolve
        // the per-application activation policy (RunAs, endpoints, etc.).
        clsidKey.SetValue("AppID", $"{{{registration.Clsid:D}}}");

        using (RegistryKey local = clsidKey.CreateSubKey("LocalServer32", writable: true)
            ?? throw new UnauthorizedAccessException("Cannot create LocalServer32 subkey.")) {
            local.SetValue(null, $"\"{executablePath}\"");
        }

        using (RegistryKey progIdKey = clsidKey.CreateSubKey("ProgID", writable: true)
            ?? throw new UnauthorizedAccessException("Cannot create ProgID subkey.")) {
            progIdKey.SetValue(null, registration.ProgId);
        }

        if (!string.Equals(versionIndependent, registration.ProgId, StringComparison.Ordinal)) {
            using RegistryKey viKey = clsidKey.CreateSubKey("VersionIndependentProgID", writable: true)
                ?? throw new UnauthorizedAccessException("Cannot create VersionIndependentProgID subkey.");
            viKey.SetValue(null, versionIndependent);
        }

        if (implementedCategories is { Count: > 0 }) {
            using RegistryKey impl = clsidKey.CreateSubKey("Implemented Categories", writable: true)
                ?? throw new UnauthorizedAccessException("Cannot create Implemented Categories subkey.");
            foreach (OpcComponentCategory cat in implementedCategories) {
                using RegistryKey? _ = impl.CreateSubKey($"{{{cat.CategoryId:D}}}", writable: true);
            }
        }
    }

    private static void WriteAppIdTree(RegistryKey classes, OpcClsidRegistration registration) {
        using RegistryKey appIdKey = classes.CreateSubKey(
            $@"AppID\{{{registration.Clsid:D}}}",
            writable: true)
            ?? throw new UnauthorizedAccessException("Cannot create AppID subkey.");
        appIdKey.SetValue(null, registration.FriendlyName ?? registration.ProgId);
    }

    private static void WriteProgIdAliases(RegistryKey classes, OpcClsidRegistration registration) {
        string friendly = registration.FriendlyName ?? registration.ProgId;
        string versionIndependent = StripVersionSuffix(registration.ProgId);

        using (RegistryKey progIdKey = classes.CreateSubKey(registration.ProgId, writable: true)
            ?? throw new UnauthorizedAccessException("Cannot create ProgID alias subkey.")) {
            progIdKey.SetValue(null, friendly);
            using RegistryKey clsidValue = progIdKey.CreateSubKey("CLSID", writable: true)
                ?? throw new UnauthorizedAccessException("Cannot create ProgID alias\\CLSID subkey.");
            clsidValue.SetValue(null, $"{{{registration.Clsid:D}}}");
        }

        if (!string.Equals(versionIndependent, registration.ProgId, StringComparison.Ordinal)) {
            using RegistryKey viKey = classes.CreateSubKey(versionIndependent, writable: true)
                ?? throw new UnauthorizedAccessException("Cannot create VersionIndependentProgID alias subkey.");
            viKey.SetValue(null, friendly);

            using (RegistryKey viClsid = viKey.CreateSubKey("CLSID", writable: true)
                ?? throw new UnauthorizedAccessException("Cannot create version-independent CLSID subkey.")) {
                viClsid.SetValue(null, $"{{{registration.Clsid:D}}}");
            }

            using RegistryKey curVer = viKey.CreateSubKey("CurVer", writable: true)
                ?? throw new UnauthorizedAccessException("Cannot create CurVer subkey.");
            curVer.SetValue(null, registration.ProgId);
        }
    }

    private static void WriteComponentCategoryDescriptions(
        RegistryKey classes,
        IReadOnlyList<OpcComponentCategory> categories) {
        using RegistryKey categoriesRoot = classes.CreateSubKey("Component Categories", writable: true)
            ?? throw new UnauthorizedAccessException("Cannot create Component Categories subkey.");

        foreach (OpcComponentCategory cat in categories) {
            using RegistryKey catKey = categoriesRoot.CreateSubKey($"{{{cat.CategoryId:D}}}", writable: true)
                ?? throw new UnauthorizedAccessException("Cannot create category description subkey.");
            // LCID 409 = en-US. ICatInformation::GetCategoryDesc resolves the locale-specific value.
            using RegistryKey lcidKey = catKey.CreateSubKey(
                LcidEnUs.ToString("X", CultureInfo.InvariantCulture),
                writable: true)
                ?? throw new UnauthorizedAccessException("Cannot create LCID 409 subkey.");
            lcidKey.SetValue(null, cat.Description);
        }
    }

    private const int LcidEnUs = 0x409;

    private static IEnumerable<RegistryView> ResolveViews(IReadOnlyList<RegistryView>? views) {
        if (views is null || views.Count == 0) {
            yield return RegistryView.Registry32;
            yield return RegistryView.Registry64;
            yield break;
        }

        foreach (RegistryView v in views) {
            yield return v;
        }
    }

    private static string StripVersionSuffix(string progId) {
        // OPC ProgIDs use the convention "Vendor.Server.N" where N is an integer version.
        // The version-independent form is "Vendor.Server" (everything before the final ".N").
        int dot = progId.LastIndexOf('.');
        if (dot <= 0 || dot == progId.Length - 1) {
            return progId;
        }

        for (int i = dot + 1; i < progId.Length; i++) {
            if (!char.IsAsciiDigit(progId[i])) {
                return progId;
            }
        }

        return progId[..dot];
    }
}
