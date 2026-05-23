//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// GSS-API MIC provider for the Kerberos inner mechanism.
/// </summary>
public sealed class KerberosMicProvider : IGssMicProvider
{
    private readonly IKerberosSession? _session;
    private readonly IGssMicProvider? _inner;

    /// <summary>
    /// Initializes a provider that fails closed until Kerberos MIC support is implemented.
    /// </summary>
    public KerberosMicProvider()
    {
    }

    /// <summary>
    /// Initializes a provider backed by an established Kerberos session.
    /// </summary>
    /// <param name="session">The established Kerberos packet-protection session.</param>
    public KerberosMicProvider(IKerberosSession session)
    {
        ArgumentNullException.ThrowIfNull(session);

        _session = session;
    }

    /// <summary>
    /// Initializes a provider that delegates to an already-established Kerberos MIC implementation.
    /// </summary>
    /// <param name="inner">The concrete Kerberos MIC implementation.</param>
    public KerberosMicProvider(IGssMicProvider inner)
    {
        ArgumentNullException.ThrowIfNull(inner);

        _inner = inner;
    }

    /// <inheritdoc />
    public byte[] GetMic(ReadOnlySpan<byte> data)
    {
        if (_session is not null)
        {
            return _session.GetMic(data);
        }

        if (_inner is not null)
        {
            return _inner.GetMic(data);
        }

        throw new InvalidOperationException("Kerberos SPNEGO mechListMIC generation requires an established Kerberos session.");
    }

    /// <inheritdoc />
    public bool VerifyMic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mic)
    {
        if (_session is not null)
        {
            return _session.VerifyMic(data, mic);
        }

        if (_inner is not null)
        {
            return _inner.VerifyMic(data, mic);
        }

        throw new InvalidOperationException("Kerberos SPNEGO mechListMIC verification requires an established Kerberos session.");
    }
}
