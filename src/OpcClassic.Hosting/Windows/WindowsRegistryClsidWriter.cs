//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace OpcClassic.Hosting.Windows;

/// <summary>
/// Writes OPC Classic CLSID registrations into the Windows COM registry.
/// </summary>
[SupportedOSPlatform("windows")]
public static class WindowsRegistryClsidWriter
{
    /// <summary>
    /// Writes the CLSID under HKLM\SOFTWARE\Classes\CLSID for native COM client
    /// activation compatibility. No-op on non-Windows; the OS guard at the
    /// attribute-level prevents non-Windows callers from invoking this method.
    /// </summary>
    public static void Write(OpcClsidRegistration registration)
    {
        ArgumentNullException.ThrowIfNull(registration);
        var clsidKeyPath = $@"SOFTWARE\Classes\CLSID\{{{registration.Clsid:D}}}";
        using var clsidKey = Registry.LocalMachine.CreateSubKey(clsidKeyPath, writable: true);
        if (clsidKey is null)
        {
            throw new UnauthorizedAccessException(
                $"Cannot open HKLM\\{clsidKeyPath} for write (admin required).");
        }

        clsidKey.SetValue(null, registration.FriendlyName ?? registration.ProgId);
        using var inprocServer32 = clsidKey.CreateSubKey("InprocServer32", writable: true);
        inprocServer32?.SetValue(null, registration.AssemblyName);
        inprocServer32?.SetValue("ThreadingModel", "Both");
    }

    /// <summary>
    /// Removes the CLSID registration from HKLM\SOFTWARE\Classes\CLSID.
    /// </summary>
    public static void Remove(Guid clsid)
    {
        var clsidKeyPath = $@"SOFTWARE\Classes\CLSID\{{{clsid:D}}}";
        Registry.LocalMachine.DeleteSubKeyTree(clsidKeyPath, throwOnMissingSubKey: false);
    }
}
