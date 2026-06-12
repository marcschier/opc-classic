//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// Provides GSS-API MIC services for the negotiated SPNEGO inner mechanism.
/// </summary>
public interface IGssMicProvider
{
    /// <summary>
    /// Computes a GSS-API MIC over the supplied data.
    /// </summary>
    /// <param name="data">The exact bytes to protect.</param>
    /// <returns>The mechanism-specific MIC bytes.</returns>
    byte[] GetMic(ReadOnlySpan<byte> data);

    /// <summary>
    /// Verifies a GSS-API MIC over the supplied data.
    /// </summary>
    /// <param name="data">The exact bytes that were protected.</param>
    /// <param name="mic">The mechanism-specific MIC bytes.</param>
    /// <returns><see langword="true" /> when the MIC is valid.</returns>
    bool VerifyMic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mic);
}
