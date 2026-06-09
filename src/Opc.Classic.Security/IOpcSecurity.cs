//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Security;

/// <summary>
/// The managed async-first OPC Security contract — exposes
/// <c>IOPCSecurityNT</c> (Windows-integrated auth) and
/// <c>IOPCSecurityPrivate</c> (server-private credentials) operations.
/// </summary>
/// <remarks>
/// <para>
/// OPC Security is OPTIONAL — most OPC DA/AE/HDA servers do not implement it
/// (they rely on DCOM-level authentication instead). When implemented, it lets
/// a single client connection switch between identities without re-activating
/// the server object.
/// </para>
/// <para>
/// Two authentication models:
/// </para>
/// <list type="bullet">
///   <item><description><strong>NT</strong> — Windows-integrated. The current process's identity is used.
///     In a cross-platform deployment, this requires Kerberos (Phase 3D).</description></item>
///   <item><description><strong>Private</strong> — the server validates a username + password it manages
///     directly. Stored in the server, not Windows.</description></item>
/// </list>
/// </remarks>
public interface IOpcSecurity {
    /// <summary>True if the server implements <c>IOPCSecurityNT</c>.</summary>
    bool SupportsWindowsAuthentication { get; }

    /// <summary>True if the server implements <c>IOPCSecurityPrivate</c>.</summary>
    bool SupportsPrivateAuthentication { get; }

    /// <summary>
    /// Switch to Windows-integrated authentication using the current process's
    /// identity. Returns true on success; false if the server doesn't accept
    /// the identity or the platform doesn't support it.
    /// </summary>
    Task<bool> LoginAsCurrentUserAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Switch to private authentication using server-validated credentials.
    /// </summary>
    /// <param name="username">The private-auth username.</param>
    /// <param name="password">The private-auth password (transmitted to the server in cleartext at the OPC protocol level — DCOM authentication-level encryption is the only protection).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<bool> LoginPrivateAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>Switch back to the connection's anonymous / default identity.</summary>
    Task LogoutAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// True when the consumer is currently authenticated (either Windows or
    /// private). False after <see cref="LogoutAsync"/>.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>The currently active identity ("CORP\\alice", "private:operator", or empty when anonymous).</summary>
    string CurrentIdentity { get; }
}
