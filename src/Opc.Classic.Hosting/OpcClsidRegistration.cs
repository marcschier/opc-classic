// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Hosting;

/// <summary>
/// Registration metadata for an OPC Classic COM server class.
/// </summary>
/// <param name="Clsid">The COM class identifier.</param>
/// <param name="ProgId">The ProgID alias used by OPC clients.</param>
/// <param name="AssemblyName">The managed assembly containing the server implementation.</param>
/// <param name="TypeName">The managed type implementing the server class.</param>
/// <param name="FriendlyName">Optional display name for human-readable discovery/registry views.</param>
/// <param name="ImplementedCategories">Optional OPC category identifiers implemented by the class.</param>
public sealed record OpcClsidRegistration(
    Guid Clsid,
    string ProgId,
    string AssemblyName,
    string TypeName,
    string? FriendlyName = null,
    IReadOnlyList<Guid>? ImplementedCategories = null);
