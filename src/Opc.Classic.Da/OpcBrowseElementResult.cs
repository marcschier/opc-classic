// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Da;

/// <summary>
/// OPC DA's <c>OPCBROWSEELEMENT</c> result returned by <c>IOPCBrowse::Browse</c>.
/// </summary>
/// <param name="Name">Display name of the browse element.</param>
/// <param name="ItemId">Fully-qualified server item ID for the browse element.</param>
/// <param name="FlagValue">Bitmask classifying the element; bit 0 = branch, bit 1 = item.</param>
/// <param name="Properties">Inline property results returned for the element.</param>
public sealed record OpcBrowseElementResult(
    string? Name,
    string? ItemId,
    int FlagValue,
    OpcItemProperties Properties)
{
    private OpcItemProperties _properties = Properties ?? throw new ArgumentNullException(nameof(Properties));

    /// <summary>
    /// Inline property results returned for the element.
    /// </summary>
    public OpcItemProperties Properties
    {
        get => _properties;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            _properties = value;
        }
    }

    /// <summary>
    /// True when <see cref="FlagValue"/> marks the browse element as a branch.
    /// </summary>
    public bool IsBranch => (FlagValue & 1) != 0;

    /// <summary>
    /// True when <see cref="FlagValue"/> marks the browse element as an item.
    /// </summary>
    public bool IsItem => (FlagValue & 2) != 0;
}
