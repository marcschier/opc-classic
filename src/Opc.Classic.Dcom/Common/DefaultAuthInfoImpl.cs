// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Common;

/// <summary>
/// Default implementation of <code>IAuthInfo</code>.
/// </summary>
public sealed class DefaultAuthInfoImpl : IAuthInfo
{
    /// <summary>
    ///Creates the AuthInfo Object.
    /// </summary>
    /// <param name="domain">Authentication domain used for the NTLM or Kerberos handshake.</param>
    /// <param name="username">User name used for the NTLM or Kerberos handshake.</param>
    /// <param name="password">Password used for the NTLM or Kerberos handshake.</param>
    public DefaultAuthInfoImpl(string domain, string username,
        string password)
    {
        UserName = username;
        Password = password;
        Domain = domain;
    }

    /// <summary>
    /// User name
    /// </summary>
    public string UserName { get; }

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// Domain
    /// </summary>
    public string Domain { get; }
}
