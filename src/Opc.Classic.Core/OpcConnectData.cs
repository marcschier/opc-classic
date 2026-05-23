//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Net;

namespace Opc.Classic;

/// <summary>
/// Connection-time configuration for an OPC Classic client: where to connect,
/// which credentials to use, and which authentication / protection levels
/// to negotiate.
/// </summary>
/// <remarks>
/// Replaces the legacy <c>Opc.ConnectData</c> from the .NET Framework 4.6.2
/// surface. Immutable and AOT-clean.
/// </remarks>
public sealed class OpcConnectData
{
    /// <summary>Construct connection data.</summary>
    /// <param name="url">Target OPC URL (scheme + host + port + ProgID/CLSID).</param>
    /// <param name="credentials">
    /// User credentials. Pass <see langword="null"/> for anonymous (rare —
    /// hardened Windows servers reject anonymous activation).
    /// </param>
    /// <param name="authMode">
    /// Authentication mechanism. Default <see cref="OpcAuthMode.NtlmV2"/> with
    /// integrity-mode session security — required for compatibility with
    /// Microsoft DCOM hardening (KB5004442).
    /// </param>
    /// <param name="protectionLevel">
    /// DCE 1.1 packet-protection level. Default <see cref="OpcProtectionLevel.Integrity"/>.
    /// </param>
    /// <param name="operationTimeout">
    /// Per-operation timeout. <see langword="null"/> means use the global default
    /// (typically 30 seconds).
    /// </param>
    public OpcConnectData(
        OpcUrl url,
        NetworkCredential? credentials = null,
        OpcAuthMode authMode = OpcAuthMode.NtlmV2,
        OpcProtectionLevel protectionLevel = OpcProtectionLevel.Integrity,
        TimeSpan? operationTimeout = null)
    {
        ArgumentNullException.ThrowIfNull(url);
        if (authMode == OpcAuthMode.Anonymous && credentials is not null)
        {
            throw new ArgumentException(
                "OpcAuthMode.Anonymous is incompatible with non-null credentials.",
                nameof(authMode));
        }
        if (authMode != OpcAuthMode.Anonymous && credentials is null)
        {
            throw new ArgumentException(
                $"OpcAuthMode.{authMode} requires non-null credentials.",
                nameof(credentials));
        }
        if (operationTimeout is { } t && t <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(operationTimeout), t,
                "Operation timeout must be positive.");
        }

        Url = url;
        Credentials = credentials;
        AuthMode = authMode;
        ProtectionLevel = protectionLevel == OpcProtectionLevel.Default
            ? OpcProtectionLevel.Integrity
            : protectionLevel;
        OperationTimeout = operationTimeout;
    }

    /// <summary>Target OPC URL.</summary>
    public OpcUrl Url { get; }

    /// <summary>Caller-supplied credentials (<see langword="null"/> for anonymous).</summary>
    public NetworkCredential? Credentials { get; }

    /// <summary>Negotiated authentication mechanism.</summary>
    public OpcAuthMode AuthMode { get; }

    /// <summary>Negotiated packet-protection level (defaults expanded from <see cref="OpcProtectionLevel.Default"/>).</summary>
    public OpcProtectionLevel ProtectionLevel { get; }

    /// <summary>Per-operation timeout, or <see langword="null"/> for global default.</summary>
    public TimeSpan? OperationTimeout { get; }

    /// <summary>Construct anonymous connection data (no credentials, no NTLM).</summary>
    public static OpcConnectData Anonymous(OpcUrl url, TimeSpan? operationTimeout = null)
        => new(url, credentials: null, authMode: OpcAuthMode.Anonymous, operationTimeout: operationTimeout);

    /// <summary>Construct NTLMv2 connection data (the recommended default).</summary>
    public static OpcConnectData WithNtlmV2(
        OpcUrl url,
        NetworkCredential credentials,
        OpcProtectionLevel protectionLevel = OpcProtectionLevel.Integrity,
        TimeSpan? operationTimeout = null)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        return new OpcConnectData(url, credentials, OpcAuthMode.NtlmV2, protectionLevel, operationTimeout);
    }

    /// <summary>Construct Kerberos / SPNEGO connection data (requires Phase 3D).</summary>
    public static OpcConnectData WithKerberos(
        OpcUrl url,
        NetworkCredential credentials,
        OpcProtectionLevel protectionLevel = OpcProtectionLevel.Integrity,
        TimeSpan? operationTimeout = null)
    {
        ArgumentNullException.ThrowIfNull(credentials);
        return new OpcConnectData(url, credentials, OpcAuthMode.Kerberos, protectionLevel, operationTimeout);
    }
}
