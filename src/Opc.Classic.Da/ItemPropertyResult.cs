//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace Opc.Classic.Da;

/// <summary>
/// The properties returned for a single item by
/// <c>IDaServer.GetPropertiesAsync</c>. One <see cref="ItemPropertyResult"/>
/// per input item.
/// </summary>
public sealed class ItemPropertyResult
{
    /// <summary>The item these properties belong to.</summary>
    public string ItemName { get; init; } = string.Empty;

    /// <summary>Optional access path of the originating item.</summary>
    public string? ItemPath { get; init; }

    /// <summary>Per-item HRESULT (success/failure of resolving this item).</summary>
    public OpcResultId ResultId { get; init; } = OpcResultId.Ok;

    /// <summary>The property values returned for this item.</summary>
    public IReadOnlyList<ItemProperty> Properties { get; init; } = Array.Empty<ItemProperty>();
}
