// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// RFC 4178 NegTokenResp fields returned by the acceptor.
/// </summary>
/// <param name="NegState">Optional negotiation state.</param>
/// <param name="SupportedMech">Optional selected mechanism object identifier.</param>
/// <param name="ResponseToken">Optional mechanism response token.</param>
/// <param name="MechListMic">Optional MIC over the mechanism list.</param>
public sealed record SpnegoNegTokenResp(
    SpnegoNegState? NegState,
    string? SupportedMech,
    ReadOnlyMemory<byte>? ResponseToken,
    ReadOnlyMemory<byte>? MechListMic)
{
    /// <summary>
    /// Verifies this response's mechListMIC over the exact encoded MechTypeList bytes.
    /// </summary>
    /// <param name="mechListBytes">Exact DER bytes of the original MechTypeList SEQUENCE.</param>
    /// <param name="micProvider">The negotiated inner mechanism MIC provider.</param>
    /// <returns><see langword="true" /> when the mechListMIC is present and valid.</returns>
    public bool VerifyMechListMic(ReadOnlySpan<byte> mechListBytes, IGssMicProvider micProvider)
    {
        ArgumentNullException.ThrowIfNull(micProvider);

        return MechListMic.HasValue && micProvider.VerifyMic(mechListBytes, MechListMic.Value.Span);
    }
}
