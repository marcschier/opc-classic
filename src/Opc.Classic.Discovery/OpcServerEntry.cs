//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Discovery;

/// <summary>
/// A single OPC Classic server registration discovered from configuration, registry, or OpcEnum.
/// </summary>
public sealed record OpcServerEntry(
    Guid Clsid,
    string ProgId,
    string FriendlyName,
    string Host,
    IReadOnlyList<Guid> SupportedCategories);
