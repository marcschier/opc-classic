//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System.Runtime.CompilerServices;
using VerifyTUnit;

namespace OpcClassic.Da.Tests.Snapshots;

internal static class SnapshotInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        Verifier.UseProjectRelativeDirectory("Snapshots");
    }
}
