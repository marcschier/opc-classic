// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Windows;

/// <summary>
/// Marker for the <c>Opc.Classic.Windows</c> add-on package. This package delivers the
/// Windows DCOM server-hosting assembly (<c>Opc.Classic.Hosting.Windows</c>) on top of
/// the <c>Opc.Classic</c> SDK meta-package, letting a managed Opc.Classic server be
/// exposed as a real Windows DCOM server. The functional API lives in the bundled
/// <c>Opc.Classic.Hosting.Windows</c> assembly; this type exists only so the add-on
/// ships a non-empty, discoverable assembly.
/// </summary>
public static class OpcClassicWindows
{
    /// <summary>The published NuGet package id for this add-on.</summary>
    public const string PackageId = "Opc.Classic.Windows";
}
