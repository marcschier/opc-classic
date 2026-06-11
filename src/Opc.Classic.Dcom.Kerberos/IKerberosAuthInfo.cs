//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Describes the Kerberos identity and service principal used for DCOM authentication.
/// </summary>
public interface IKerberosAuthInfo
{
    /// <summary>
    /// Gets the Kerberos realm used for ticket acquisition.
    /// </summary>
    string Realm { get; }

    /// <summary>
    /// Gets the service principal name, for example <c>RPCSS/server.example.com</c>.
    /// </summary>
    string Spn { get; }

    /// <summary>
    /// Gets the user principal name or account name used for client authentication.
    /// </summary>
    string Username { get; }

    /// <summary>
    /// Gets the optional Windows/NetBIOS domain associated with <see cref="Username" />.
    /// </summary>
    string? Domain { get; }
}
