//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Tests.Integration.Native;

internal static class NativeServerProbe
{
    /// <summary>True if a server with the given ProgID is registered (Windows only).</summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static bool IsRegistered(string progId)
    {
        return TryGetRegisteredClsid(progId, out _);
    }

    /// <summary>True if a server with the given ProgID is registered with the expected CLSID (Windows only).</summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    public static bool IsRegistered(string progId, Guid expectedClsid)
    {
        return TryGetRegisteredClsid(progId, out var actualClsid) && actualClsid == expectedClsid;
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
            reason = $"Native server {progId} is not registered (Phase 14A workflow must install OpcCoreComponents.exe + run ext/samples/regserver.cmd)";
            return true;
        }

        reason = string.Empty;
        return false;
    }

    public static bool ShouldSkip(string progId, Guid expectedClsid, out string reason)
    {
        if (!OperatingSystem.IsWindows())
        {
            reason = "Native COM tests require Windows";
            return true;
        }

        if (!TryGetRegisteredClsid(progId, out var actualClsid))
        {
            reason = $"Native server {progId} is not registered (Phase 14A workflow must install OpcCoreComponents.exe + run ext/samples/regserver.cmd)";
            return true;
        }

        if (actualClsid != expectedClsid)
        {
            reason = $"Native server {progId} registered CLSID {actualClsid:B} does not match expected {expectedClsid:B}";
            return true;
        }

        reason = string.Empty;
        return false;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static bool TryGetRegisteredClsid(string progId, out Guid clsid)
    {
        using var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey($@"{progId}\CLSID");
        var value = key?.GetValue(null) as string;
        return Guid.TryParse(value, out clsid);
    }
}
