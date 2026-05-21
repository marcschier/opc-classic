//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic.Ae;

/// <summary>
/// A node in an OPC AE event-area browse tree (returned by
/// <see cref="IAeServer.BrowseAreasAsync"/>).
/// </summary>
public sealed class AreaBrowseElement
{
    /// <summary>Short display name of this area or source.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Fully-qualified path (server-canonical).</summary>
    public string QualifiedName { get; init; } = string.Empty;

    /// <summary>True if this is a sub-area (has child areas).</summary>
    public bool IsArea { get; init; }

    /// <summary>True if this is an event source (leaf).</summary>
    public bool IsSource { get; init; }
}
