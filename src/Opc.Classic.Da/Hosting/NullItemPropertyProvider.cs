//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// No-op property provider that returns <c>OPC_E_INVALID_PID</c> for every
/// query. Used by <see cref="DefaultItemProperties"/> when the user hasn't
/// supplied a real provider.
/// </summary>
public sealed class NullItemPropertyProvider : IOpcItemPropertyProvider
{
    /// <summary>Singleton instance.</summary>
    public static NullItemPropertyProvider Instance { get; } = new();

    private NullItemPropertyProvider()
    {
    }

    /// <inheritdoc />
    public (OpcVariant Value, int Error) TryGetPropertyValue(string itemId, int propertyId)
    {
        _ = itemId; _ = propertyId;
        return (OpcVariant.Empty, OpcResultId.InvalidPid.Code);
    }
}
