// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Tests.Integration.Matrikon;

internal static class MatrikonServerProbe
{
    public const string MatrikonProgId = "Matrikon.OPC.Simulation.1";

    public static bool ShouldSkip(out string reason)
    {
        if (!System.OperatingSystem.IsWindows())
        {
            reason = "Matrikon conformance tests require Windows";
            return true;
        }

        if (!IsRegistered(MatrikonProgId))
        {
            reason = "Matrikon.OPC.Simulation.1 is not installed";
            return true;
        }

        reason = string.Empty;
        return false;
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static bool IsRegistered(string progId)
    {
        if (!System.OperatingSystem.IsWindows())
        {
            return false;
        }

        using var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey($@"{progId}\CLSID");
        return key is not null;
    }
}
