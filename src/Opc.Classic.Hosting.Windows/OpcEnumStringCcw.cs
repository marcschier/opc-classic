// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Runtime.Versioning;

namespace Opc.Classic.Hosting.Windows;

/// <summary>
/// Shared Windows factory for an AOT-safe <c>IEnumString</c> CCW over an immutable snapshot.
/// </summary>
[SupportedOSPlatform("windows")]
public static class OpcEnumStringCcw
{
    /// <summary>Creates a caller-owned enumerator over a copied string snapshot.</summary>
    public static IntPtr Create(IReadOnlyList<string> values) =>
        global::Opc.Classic.Ae.Hosting.Windows.OpcEnumStringCcw.Create(values);

    /// <summary>Returns the current CCW reference count, or -1 after disposal.</summary>
    public static long GetReferenceCount(IntPtr instance) =>
        global::Opc.Classic.Ae.Hosting.Windows.OpcEnumStringCcw.GetReferenceCount(instance);
}
