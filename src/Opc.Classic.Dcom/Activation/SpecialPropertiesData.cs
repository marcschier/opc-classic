//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Managed shadow of SPECIAL_PROPERTIES_DATA.
/// </summary>
public sealed record SpecialPropertiesData(
    ActivationComVersion ClientVersion,
    int Mode,
    int ClassContext,
    Guid RequestedIid,
    IReadOnlyList<int> SpecialProperties)
{
    /// <summary>
    /// An empty v5.6 special-properties set.
    /// </summary>
    public static SpecialPropertiesData Empty { get; } = new(
        ActivationComVersion.V5_6,
        0,
        0,
        Guid.Empty,
        Array.Empty<int>());
}
