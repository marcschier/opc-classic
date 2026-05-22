//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpcClassic.Integration.Tests.CompatMatrix;

internal static class CompatMatrixProbe
{
    public static bool ShouldSkipNet10ServerToNativeClient(out string reason)
    {
        if (!OperatingSystem.IsWindows())
        {
            reason = "Compat matrix tests require Windows to run native COM clients";
            return true;
        }

        var candidates = GetNativeSampleClientCandidates().ToArray();
        if (!candidates.Any(File.Exists))
        {
            reason = "Native sample client not found at any expected path: "
                + string.Join(", ", candidates)
                + " (Phase 14A workflow must build the COM sample client first)";
            return true;
        }

        reason = string.Empty;
        return false;
    }

    internal static string NativeSampleClientPath =>
        GetNativeSampleClientCandidates().FirstOrDefault(File.Exists)
        ?? GetNativeSampleClientCandidates().First();

    private static IEnumerable<string> GetNativeSampleClientCandidates()
    {
        var repoRoot = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            ".."));

        yield return Path.Combine(repoRoot, "COM", "BuildOutput", "bin", "clients", "Win32", "Release", "OpcDaSimpleClient.exe");
        yield return Path.Combine(repoRoot, "COM", "BuildOutput", "bin", "clients", "Win32", "Debug", "OpcDaSimpleClient.exe");
        yield return Path.Combine(repoRoot, "BuildOutput", "bin", "clients", "Win32", "Release", "OpcDaSimpleClient.exe");
        yield return Path.Combine(repoRoot, "BuildOutput", "bin", "clients", "Win32", "Debug", "OpcDaSimpleClient.exe");
        yield return Path.Combine(repoRoot, "COM", "Bin", "Da", "Simple Client", "OpcDaSimpleClient.exe");
    }
}
