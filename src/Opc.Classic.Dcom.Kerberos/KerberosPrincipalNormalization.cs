// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Selects the explicit normalization applied before principal lookup.
/// </summary>
public enum KerberosPrincipalNormalization
{
    /// <summary>
    /// Preserve name and realm casing.
    /// </summary>
    None,

    /// <summary>
    /// Preserve the name and uppercase the realm.
    /// </summary>
    CanonicalRealm,

    /// <summary>
    /// Lowercase the name and uppercase the realm.
    /// </summary>
    LowercaseNameAndCanonicalRealm,
}
