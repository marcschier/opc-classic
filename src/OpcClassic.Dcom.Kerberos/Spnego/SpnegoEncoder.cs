//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Formats.Asn1;

namespace OpcClassic.Dcom.Kerberos.Spnego;

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
        negTokenInit.PushSequence();
        foreach (var mech in init.MechTypes)
        {
            negTokenInit.WriteObjectIdentifier(mech);
        }

        negTokenInit.PopSequence();
        negTokenInit.PopSequence(mechTypesTag);

        if (!init.MechToken.IsEmpty)
        {
            var mechTokenTag = new Asn1Tag(TagClass.ContextSpecific, 2, isConstructed: true);
            negTokenInit.PushSequence(mechTokenTag);
            negTokenInit.WriteOctetString(init.MechToken.Span);
            negTokenInit.PopSequence(mechTokenTag);
        }

        if (init.MechListMic.HasValue && !init.MechListMic.Value.IsEmpty)
        {
            var mechListMicTag = new Asn1Tag(TagClass.ContextSpecific, 3, isConstructed: true);
            negTokenInit.PushSequence(mechListMicTag);
            negTokenInit.WriteOctetString(init.MechListMic.Value.Span);
            negTokenInit.PopSequence(mechListMicTag);
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
}
