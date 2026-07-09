// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Security;

/// <summary>
/// Private OPC Security logon credentials passed to <c>IOPCSecurityPrivate::Logon</c>.
/// </summary>
public sealed record OpcLogonRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpcLogonRequest" /> record.
    /// </summary>
    /// <param name="userId">Server-private user identifier.</param>
    /// <param name="password">Server-private password. May be empty, but not <see langword="null" />.</param>
    public OpcLogonRequest(string userId, string password)
    {
        System.ArgumentException.ThrowIfNullOrEmpty(userId);
        System.ArgumentNullException.ThrowIfNull(password);

        UserId = userId;
        Password = password;
    }

    /// <summary>
    /// Server-private user identifier.
    /// </summary>
    public string UserId { get; init; }

    /// <summary>
    /// Server-private password. May be empty, but not <see langword="null" />.
    /// </summary>
    public string Password { get; init; }
}
