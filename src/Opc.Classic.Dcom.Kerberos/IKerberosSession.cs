//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Kerberos GSS-API per-message protection context.
/// </summary>
public interface IKerberosSession
{
    /// <summary>
    /// Protects a plaintext message as an RFC 4121 Wrap token.
    /// </summary>
    /// <param name="plaintext">The application bytes to protect.</param>
    /// <param name="confidential">Whether to encrypt the plaintext in addition to signing it.</param>
    /// <returns>The RFC 4121 Wrap token.</returns>
    byte[] WrapMessage(ReadOnlySpan<byte> plaintext, bool confidential);

    /// <summary>
    /// Verifies and unwraps an RFC 4121 Wrap token.
    /// </summary>
    /// <param name="wrappedToken">The received Wrap token.</param>
    /// <param name="wasConfidential">Set to <see langword="true" /> when the token provided confidentiality.</param>
    /// <returns>The unwrapped plaintext.</returns>
    byte[] UnwrapMessage(ReadOnlySpan<byte> wrappedToken, out bool wasConfidential);

    /// <summary>
    /// Computes an RFC 4121 MIC token over the supplied data.
    /// </summary>
    /// <param name="data">The exact bytes to sign.</param>
    /// <returns>The RFC 4121 MIC token.</returns>
    byte[] GetMic(ReadOnlySpan<byte> data);

    /// <summary>
    /// Verifies an RFC 4121 MIC token over the supplied data.
    /// </summary>
    /// <param name="data">The exact bytes that were signed.</param>
    /// <param name="mic">The received MIC token.</param>
    /// <returns><see langword="true" /> when the MIC is valid and in sequence.</returns>
    bool VerifyMic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mic);

    /// <summary>
    /// Gets the next outbound sequence number.
    /// </summary>
    int SequenceNumber { get; }
}
