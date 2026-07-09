// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.CompilerServices;
using VerifyTUnit;

namespace Opc.Classic.SnapshotTests.Support;

internal static class SnapshotInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        Verifier.UseProjectRelativeDirectory("Snapshots");
    }
}
