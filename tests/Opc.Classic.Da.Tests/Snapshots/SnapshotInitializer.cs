// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Runtime.CompilerServices;
using VerifyTUnit;

namespace Opc.Classic.Da.Tests.Snapshots;

internal static class SnapshotInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        Verifier.UseProjectRelativeDirectory("Snapshots");
    }
}
