//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace OpcClassic.Discovery;

/// <summary>
/// A single OPC Classic server registration discovered from configuration, registry, or OpcEnum.
/// </summary>
public sealed record OpcServerEntry(
    Guid Clsid,
    string ProgId,
    string FriendlyName,
    string Host,
    IReadOnlyList<Guid> SupportedCategories);
