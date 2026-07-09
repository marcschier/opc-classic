// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Kerberos authentication configuration for a DCOM connection.
/// </summary>
public sealed record KerberosAuthInfo : IKerberosAuthInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KerberosAuthInfo" /> record.
    /// </summary>
    /// <param name="realm">Kerberos realm used to acquire tickets.</param>
    /// <param name="spn">Service principal name, for example <c>RPCSS/server.example.com</c>.</param>
    /// <param name="username">User principal name or account name.</param>
    /// <param name="domain">Optional Windows/NetBIOS domain associated with <paramref name="username" />.</param>
    /// <param name="password">Optional password for password-flow authentication.</param>
    /// <param name="keytabPath">Optional keytab path for keytab-flow authentication.</param>
    public KerberosAuthInfo(
        string realm,
        string spn,
        string username,
        string? domain,
        string? password,
        string? keytabPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(realm);
        ArgumentException.ThrowIfNullOrWhiteSpace(spn);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        Realm = realm;
        Spn = spn;
        Username = username;
        Domain = domain;
        Password = password;
        KeytabPath = keytabPath;
    }

    /// <inheritdoc />
    public string Realm { get; init; }

    /// <inheritdoc />
    public string Spn { get; init; }

    /// <inheritdoc />
    public string Username { get; init; }

    /// <inheritdoc />
    public string? Domain { get; init; }

    /// <summary>
    /// Gets the optional plaintext password for password-flow authentication.
    /// </summary>
    /// <remarks>
    /// This remains a <see cref="string" /> for API compatibility with .NET credential APIs. The
    /// library cannot zeroize GC-managed plaintext strings; prefer short-lived instances and
    /// rotate credentials after suspected exposure.
    /// </remarks>
    public string? Password { get; init; }

    /// <summary>
    /// Gets the optional keytab path for keytab-flow authentication.
    /// </summary>
    public string? KeytabPath { get; init; }
}
