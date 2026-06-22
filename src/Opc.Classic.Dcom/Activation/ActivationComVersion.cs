// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using System.Runtime.InteropServices;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// DCOM COMVERSION value.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly record struct ActivationComVersion(ushort Major, ushort Minor)
{
    /// <summary>
    /// DCOM v5.6, used by modern IRemoteSCMActivator activation.
    /// </summary>
    public static ActivationComVersion V5_6 { get; } = new(5, 6);
}
