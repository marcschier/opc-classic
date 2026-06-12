//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Formats.Asn1;

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// Encodes RFC 4178 SPNEGO negotiation tokens.
/// </summary>
public static class SpnegoEncoder
{
    /// <summary>
    /// Encodes a NegTokenInit in the RFC 2743 InitialContextToken envelope for SPNEGO.
    /// </summary>
    /// <param name="init">Initial negotiation fields.</param>
    /// <returns>The DER-encoded SPNEGO initial context token.</returns>
    public static byte[] EncodeNegTokenInit(SpnegoNegTokenInit init)
    {
        ArgumentNullException.ThrowIfNull(init);

        var negTokenInit = new AsnWriter(AsnEncodingRules.DER);
        negTokenInit.PushSequence();

        var mechTypesTag = new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true);
        negTokenInit.PushSequence(mechTypesTag);
        negTokenInit.WriteEncodedValue(EncodeMechTypeList(init.MechTypes));
        negTokenInit.PopSequence(mechTypesTag);

        if (!init.MechToken.IsEmpty)
        {
            WriteOctetStringField(negTokenInit, 2, init.MechToken);
        }

        if (init.MechListMic.HasValue && !init.MechListMic.Value.IsEmpty)
        {
            WriteOctetStringField(negTokenInit, 3, init.MechListMic.Value);
        }

        negTokenInit.PopSequence();

        var negotiationToken = new AsnWriter(AsnEncodingRules.DER);
        var negTokenInitTag = new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true);
        negotiationToken.PushSequence(negTokenInitTag);
        negotiationToken.WriteEncodedValue(negTokenInit.Encode());
        negotiationToken.PopSequence(negTokenInitTag);

        var initialContextToken = new AsnWriter(AsnEncodingRules.DER);
        var initialContextTokenTag = new Asn1Tag(TagClass.Application, 0, isConstructed: true);
        initialContextToken.PushSequence(initialContextTokenTag);
        initialContextToken.WriteObjectIdentifier(SpnegoOids.Spnego);
        initialContextToken.WriteEncodedValue(negotiationToken.Encode());
        initialContextToken.PopSequence(initialContextTokenTag);

        return initialContextToken.Encode();
    }

    /// <summary>
    /// Encodes a NegTokenResp continuation token.
    /// </summary>
    /// <param name="response">Response negotiation fields.</param>
    /// <returns>The DER-encoded SPNEGO negTokenResp negotiation token.</returns>
    public static byte[] EncodeNegTokenResp(SpnegoNegTokenResp response)
    {
        ArgumentNullException.ThrowIfNull(response);

        var body = new AsnWriter(AsnEncodingRules.DER);
        body.PushSequence();

        if (response.NegState.HasValue)
        {
            var negStateTag = new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true);
            body.PushSequence(negStateTag);
            body.WriteEnumeratedValue(response.NegState.GetValueOrDefault());
            body.PopSequence(negStateTag);
        }

        if (!string.IsNullOrEmpty(response.SupportedMech))
        {
            var supportedMechTag = new Asn1Tag(TagClass.ContextSpecific, 1, isConstructed: true);
            body.PushSequence(supportedMechTag);
            body.WriteObjectIdentifier(response.SupportedMech);
            body.PopSequence(supportedMechTag);
        }

        if (response.ResponseToken.HasValue)
        {
            WriteOctetStringField(body, 2, response.ResponseToken.Value);
        }

        if (response.MechListMic.HasValue)
        {
            WriteOctetStringField(body, 3, response.MechListMic.Value);
        }

        body.PopSequence();

        var negotiationToken = new AsnWriter(AsnEncodingRules.DER);
        var negTokenRespTag = new Asn1Tag(TagClass.ContextSpecific, 1, isConstructed: true);
        negotiationToken.PushSequence(negTokenRespTag);
        negotiationToken.WriteEncodedValue(body.Encode());
        negotiationToken.PopSequence(negTokenRespTag);
        return negotiationToken.Encode();
    }

    /// <summary>
    /// Encodes a NegTokenResp and computes its mechListMIC over the original MechTypeList bytes.
    /// </summary>
    /// <param name="response">Response negotiation fields without a precomputed mechListMIC.</param>
    /// <param name="mechListBytes">Exact DER bytes of the original MechTypeList SEQUENCE.</param>
    /// <param name="micProvider">The negotiated inner mechanism MIC provider.</param>
    /// <returns>The DER-encoded SPNEGO negTokenResp negotiation token.</returns>
    public static byte[] EncodeNegTokenResp(
        SpnegoNegTokenResp response,
        ReadOnlySpan<byte> mechListBytes,
        IGssMicProvider micProvider)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(micProvider);

        return EncodeNegTokenResp(response with { MechListMic = micProvider.GetMic(mechListBytes) });
    }

    /// <summary>
    /// Encodes the MechTypeList SEQUENCE used as the mechListMIC input.
    /// </summary>
    /// <param name="mechTypes">Mechanism object identifiers in initiator preference order.</param>
    /// <returns>The exact DER-encoded MechTypeList SEQUENCE.</returns>
    public static byte[] EncodeMechTypeList(IEnumerable<string> mechTypes)
    {
        ArgumentNullException.ThrowIfNull(mechTypes);

        var writer = new AsnWriter(AsnEncodingRules.DER);
        writer.PushSequence();
        foreach (var mechType in mechTypes)
        {
            ArgumentNullException.ThrowIfNull(mechType);

            writer.WriteObjectIdentifier(mechType);
        }

        writer.PopSequence();
        return writer.Encode();
    }

    private static void WriteOctetStringField(AsnWriter writer, int tagValue, ReadOnlyMemory<byte> value)
    {
        var tag = new Asn1Tag(TagClass.ContextSpecific, tagValue, isConstructed: true);
        writer.PushSequence(tag);
        writer.WriteOctetString(value.Span);
        writer.PopSequence(tag);
    }
}
