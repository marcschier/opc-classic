//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Result of <see cref="IOpcAddressSpace.BrowseAsync"/>: lists of branch
/// names + item IDs at the requested level.
/// </summary>
public sealed record OpcBrowseResult(
    IReadOnlyList<string> Branches,
    IReadOnlyList<string> Items)
{
    /// <summary>Empty result.</summary>
    public static OpcBrowseResult Empty { get; } = new(Array.Empty<string>(), Array.Empty<string>());
}
