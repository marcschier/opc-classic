//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic.Tests.Integration.Native;

internal static class NativeServerProbe
{
    /// <summary>True if a server with the given ProgID is registered (Windows only).</summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static bool IsRegistered(string progId)
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        using var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey($@"{progId}\CLSID");
        return key is not null;
    }

    public static bool ShouldSkip(string progId, out string reason)
    {
        if (!OperatingSystem.IsWindows())
        {
            reason = "Native COM tests require Windows";
            return true;
        }

        if (!IsRegistered(progId))
        {
            reason = $"Native server {progId} is not registered (Phase 14A workflow must install OpcCoreComponents.exe + run COM/regserver.cmd)";
            return true;
        }

        reason = string.Empty;
        return false;
    }
}
