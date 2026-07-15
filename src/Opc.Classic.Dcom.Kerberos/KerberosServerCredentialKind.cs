// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Identifies the secret material carried by a Kerberos server credential.
/// </summary>
public enum KerberosServerCredentialKind
{
    /// <summary>
    /// A MIT keytab byte sequence.
    /// </summary>
    Keytab,

    /// <summary>
    /// A password used to derive service keys.
    /// </summary>
    Password,
}
