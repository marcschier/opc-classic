//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Supplies per-item property values for <see cref="DefaultItemProperties"/>.
/// </summary>
public interface IOpcItemPropertyProvider
{
    /// <summary>
    /// Resolves a property value for an item. Returns
    /// (<c>OpcVariant.Empty</c>, <c>OPC_E_INVALID_PID</c>) for unknown
    /// item/property combinations.
    /// </summary>
    (OpcVariant Value, int Error) TryGetPropertyValue(string itemId, int propertyId);
}
