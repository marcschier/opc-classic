//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Runtime.InteropServices;

namespace SharpInterop.Core;

/// <summary>DCOM COMVERSION value.</summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct ActivationComVersion(ushort Major, ushort Minor) {
    /// <summary>DCOM v5.6, used by modern IRemoteSCMActivator activation.</summary>
    public static ActivationComVersion V5_6 { get; } = new(5, 6);
}
