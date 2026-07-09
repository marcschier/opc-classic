// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic;

/// <summary>
/// Marker for the <c>Opc.Classic</c> SDK meta-package. This package bundles the
/// cross-platform Opc.Classic runtime assemblies (DA, AE, HDA, Batch, Commands,
/// Complex Data, DX, Security, Discovery, XML-DA, managed DCOM/MSRPC, and auth) so a
/// single package reference is enough to build an OPC Classic client or a managed,
/// cross-platform server. Reference <c>Opc.Classic.Windows</c> in addition to host a
/// Windows DCOM server. The functional API lives in the bundled assemblies; this type
/// exists only so the meta-package ships a non-empty, discoverable assembly.
/// </summary>
public static class OpcClassicSdk
{
    /// <summary>The published NuGet package id for this SDK meta-package.</summary>
    public const string PackageId = "Opc.Classic";
}
