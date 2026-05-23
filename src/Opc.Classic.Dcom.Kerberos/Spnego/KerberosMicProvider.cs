//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// Placeholder GSS-API MIC provider for the Kerberos inner mechanism.
/// </summary>
/// <remarks>
/// Gap-6 will replace the default throwing path with Kerberos gss_get_mic / gss_verify_mic support.
/// Tests and future implementations can inject an inner provider today.
/// </remarks>
public sealed class KerberosMicProvider : IGssMicProvider
{
    private readonly IGssMicProvider? _inner;

    /// <summary>
    /// Initializes a provider that fails closed until Kerberos MIC support is implemented.
    /// </summary>
    public KerberosMicProvider()
    {
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
        if (_inner is not null)
        {
            return _inner.GetMic(data);
        }

#pragma warning disable MA0025 // Gap-6 will add Kerberos gss_get_mic support.
        throw new NotImplementedException("Kerberos SPNEGO mechListMIC generation requires gap-6 Kerberos gss_get_mic support.");
#pragma warning restore MA0025
    }

    /// <inheritdoc />
    public bool VerifyMic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mic)
    {
        if (_inner is not null)
        {
            return _inner.VerifyMic(data, mic);
        }

#pragma warning disable MA0025 // Gap-6 will add Kerberos gss_verify_mic support.
        throw new NotImplementedException("Kerberos SPNEGO mechListMIC verification requires gap-6 Kerberos gss_verify_mic support.");
#pragma warning restore MA0025
    }
}
