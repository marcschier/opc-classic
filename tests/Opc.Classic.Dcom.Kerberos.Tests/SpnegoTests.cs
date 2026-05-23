//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Formats.Asn1;
using Opc.Classic.Dcom.Kerberos.Spnego;
using TUnit.Core;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class SpnegoTests
{
    [Test]
    public async Task DecodeNegTokenResp_round_trips_response_constructed_with_AsnWriter()
    {
        byte[] responseToken = [0x01, 0x02, 0x03];
        byte[] mechListMic = [0x04, 0x05];
        var encoded = EncodeNegTokenResp(
            SpnegoNegState.AcceptIncomplete,
            SpnegoOids.KerberosV5,
            responseToken,
            mechListMic);

        var decoded = SpnegoDecoder.DecodeNegTokenResp(encoded);

        await Assert.That(decoded.NegState.HasValue).IsTrue();
        await Assert.That(decoded.NegState.GetValueOrDefault()).IsEqualTo(SpnegoNegState.AcceptIncomplete);
        await Assert.That(decoded.SupportedMech).IsEqualTo(SpnegoOids.KerberosV5);
        await Assert.That(decoded.ResponseToken.HasValue).IsTrue();
        await Assert.That(decoded.ResponseToken.GetValueOrDefault().Span.SequenceEqual(responseToken)).IsTrue();
        await Assert.That(decoded.MechListMic.HasValue).IsTrue();
        await Assert.That(decoded.MechListMic.GetValueOrDefault().Span.SequenceEqual(mechListMic)).IsTrue();
    }

    [Test]
    public async Task DecodeNegTokenResp_decodes_known_der_continuation_token()
    {
        byte[] knownGood =
        [
            0xA1, 0x1A,
            0x30, 0x18,
            0xA0, 0x03, 0x0A, 0x01, 0x00,
            0xA1, 0x0B, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x12, 0x01, 0x02, 0x02,
            0xA2, 0x04, 0x04, 0x02, 0x01, 0x02,
        ];

        var decoded = SpnegoDecoder.DecodeNegTokenResp(knownGood);

        await Assert.That(decoded.NegState.HasValue).IsTrue();
        await Assert.That(decoded.NegState.GetValueOrDefault()).IsEqualTo(SpnegoNegState.AcceptCompleted);
        await Assert.That(decoded.SupportedMech).IsEqualTo(SpnegoOids.KerberosV5);
        await Assert.That(decoded.ResponseToken.HasValue).IsTrue();
        await Assert.That(decoded.ResponseToken.GetValueOrDefault().Span.SequenceEqual(new byte[] { 0x01, 0x02 })).IsTrue();
        await Assert.That(decoded.MechListMic).IsNull();
    }

    [Test]
    public async Task BuildInitToken_starts_with_initial_context_token_and_contains_spnego_oid()
    {
        var token = SpnegoTokenBuilder.BuildInitToken(new byte[] { 0x60, 0x61, 0x62 });
        byte[] spnegoOid = [0x06, 0x06, 0x2B, 0x06, 0x01, 0x05, 0x05, 0x02];

        await Assert.That(token[0]).IsEqualTo((byte)0x60);
        await Assert.That(ContainsSubsequence(token, spnegoOid)).IsTrue();
    }

    [Test]
    public async Task Mechanism_oid_constants_match_rfc_values()
    {
        await Assert.That(ReadSpnegoOid()).IsEqualTo("1.3.6.1.5.5.2");
        await Assert.That(ReadKerberosV5Oid()).IsEqualTo("1.2.840.113554.1.2.2");
        await Assert.That(ReadNtlmsspOid()).IsEqualTo("1.3.6.1.4.1.311.2.2.10");
    }

    // TUnitAssertions0005 workaround: Assert.That(const) is rejected by the analyzer.
    // Pass the constants through a non-const indirection so the analyzer sees a method call.
    private static string ReadSpnegoOid() => SpnegoOids.Spnego;
    private static string ReadKerberosV5Oid() => SpnegoOids.KerberosV5;
    private static string ReadNtlmsspOid() => SpnegoOids.Ntlmssp;

    private static byte[] EncodeNegTokenResp(
        SpnegoNegState negState,
        string supportedMech,
        ReadOnlySpan<byte> responseToken,
        ReadOnlySpan<byte> mechListMic)
    {
        var body = new AsnWriter(AsnEncodingRules.DER);
        body.PushSequence();

        var negStateTag = new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true);
        body.PushSequence(negStateTag);
        body.WriteEnumeratedValue(negState);
        body.PopSequence(negStateTag);

        var supportedMechTag = new Asn1Tag(TagClass.ContextSpecific, 1, isConstructed: true);
        body.PushSequence(supportedMechTag);
        body.WriteObjectIdentifier(supportedMech);
        body.PopSequence(supportedMechTag);

        var responseTokenTag = new Asn1Tag(TagClass.ContextSpecific, 2, isConstructed: true);
        body.PushSequence(responseTokenTag);
        body.WriteOctetString(responseToken);
        body.PopSequence(responseTokenTag);

        var mechListMicTag = new Asn1Tag(TagClass.ContextSpecific, 3, isConstructed: true);
        body.PushSequence(mechListMicTag);
        body.WriteOctetString(mechListMic);
        body.PopSequence(mechListMicTag);

        body.PopSequence();

        var negotiationToken = new AsnWriter(AsnEncodingRules.DER);
        var negTokenRespTag = new Asn1Tag(TagClass.ContextSpecific, 1, isConstructed: true);
        negotiationToken.PushSequence(negTokenRespTag);
        negotiationToken.WriteEncodedValue(body.Encode());
        negotiationToken.PopSequence(negTokenRespTag);
        return negotiationToken.Encode();
    }

    private static bool ContainsSubsequence(ReadOnlySpan<byte> haystack, ReadOnlySpan<byte> needle)
    {
        return haystack.IndexOf(needle) >= 0;
    }
}
