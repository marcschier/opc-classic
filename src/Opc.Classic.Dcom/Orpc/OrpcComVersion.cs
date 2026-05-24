//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Runtime.InteropServices;

namespace Opc.Classic.Dcom.Orpc;

/// <summary>COMVERSION value carried by ORPC_THIS.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct OrpcComVersion(ushort Major, ushort Minor) {
    /// <summary>Default DCOM COMVERSION for ORPC calls.</summary>
    public static OrpcComVersion Default { get; } = new(5, 7);
}
