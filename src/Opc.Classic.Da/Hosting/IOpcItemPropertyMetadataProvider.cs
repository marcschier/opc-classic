//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Generic;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Optional metadata companion for <see cref="IOpcItemPropertyProvider"/> implementations.
/// </summary>
public interface IOpcItemPropertyMetadataProvider {
    /// <summary>Returns item-specific property descriptors in addition to OPC DA standard properties.</summary>
    IReadOnlyList<OpcStandardProperty> GetAvailableProperties(string itemId);

    /// <summary>Resolves an indirect item ID for a property, if the property exposes one.</summary>
    (string ItemId, int Error) TryGetPropertyItemId(string itemId, int propertyId);
}
