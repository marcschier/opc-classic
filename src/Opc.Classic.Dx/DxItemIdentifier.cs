// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dx;

/// <summary>
/// OPC DX <c>OpcDxItemIdentifier</c> — item path, item name, and configuration version.
/// </summary>
public sealed record DxItemIdentifier(
    string? ItemPath = null,
    string? ItemName = null,
    string? Version = null,
    int Reserved = 0)
{
    /// <summary>
    /// Creates an item identifier for a branch-local item name.
    /// </summary>
    public static DxItemIdentifier FromName(string itemName, string? version = null) =>
        new(null, itemName, version);
}
