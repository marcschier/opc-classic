//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

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
