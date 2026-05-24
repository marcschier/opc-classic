//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using Opc.Classic.Dcom.Internal.Ntlm;

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// GSS-API MIC provider for SPNEGO sessions that negotiate NTLMSSP.
/// </summary>
public sealed class NtlmMicProvider : IGssMicProvider {
    private readonly byte[] _signingKey;

    /// <summary>
    /// Initializes a new NTLM MIC provider.
    /// </summary>
    /// <param name="signingKey">The negotiated NTLM signing key.</param>
    /// <param name="sequenceNumber">The NTLM sequence number. SPNEGO mechListMIC uses zero.</param>
    public NtlmMicProvider(ReadOnlySpan<byte> signingKey, uint sequenceNumber = 0) {
        if (signingKey.IsEmpty) {
            throw new ArgumentException("The NTLM signing key must not be empty.", nameof(signingKey));
        }

        _signingKey = signingKey.ToArray();
        SequenceNumber = sequenceNumber;
    }

    /// <summary>
    /// Gets the NTLM sequence number used for MIC operations.
    /// </summary>
    public uint SequenceNumber { get; }

    /// <inheritdoc />
    public byte[] GetMic(ReadOnlySpan<byte> data) =>
        NtlmMessageSignature.Sign(_signingKey, data, SequenceNumber);

    /// <inheritdoc />
    public bool VerifyMic(ReadOnlySpan<byte> data, ReadOnlySpan<byte> mic) =>
        NtlmMessageSignature.Verify(_signingKey, data, mic, SequenceNumber);
}
