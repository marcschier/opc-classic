// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Common.Ntlm;

/// <summary>
/// Stores NTLM credentials for legacy Opc.Classic.Dcom.Common.Ntlm-compatible call sites.
/// </summary>
/// <remarks>
/// The public API preserves <see cref="string" /> credentials for source compatibility. Plaintext
/// password strings are managed by the GC and cannot be zeroized by this library; callers should
/// avoid long-lived instances and rotate credentials after suspected exposure.
/// </remarks>
public sealed class NtlmPasswordAuthentication
{
    public NtlmPasswordAuthentication(string domain, string username, string password)
    {
        Domain = domain;
        Username = username;
        Password = password;
    }

    public string Domain { get; }

    public string Username { get; }

    /// <summary>
    /// Gets the plaintext password retained for legacy API compatibility.
    /// </summary>
    public string Password { get; }
}
