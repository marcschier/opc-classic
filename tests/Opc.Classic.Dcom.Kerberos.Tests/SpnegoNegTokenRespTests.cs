// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom.Kerberos.Spnego;

namespace Opc.Classic.Dcom.Kerberos.Tests;

public sealed class SpnegoNegTokenRespTests
{
    private static readonly byte[] SigningKey =
    [
        0x10, 0x11, 0x12, 0x13,
        0x14, 0x15, 0x16, 0x17,
        0x18, 0x19, 0x1A, 0x1B,
        0x1C, 0x1D, 0x1E, 0x1F,
    ];

    [Test]
    public async Task EncodeNegTokenResp_with_all_fields_round_trips()
    {
        byte[] responseToken = [0x01, 0x02, 0x03];
        byte[] mechListMic = [0x04, 0x05, 0x06, 0x07];
        var response = new SpnegoNegTokenResp(
            SpnegoNegState.AcceptIncomplete,
            SpnegoOids.KerberosV5,
            responseToken,
            mechListMic);

        var encoded = SpnegoEncoder.EncodeNegTokenResp(response);
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
    public async Task EncodeNegTokenResp_with_only_negState_omits_other_fields()
    {
        var response = new SpnegoNegTokenResp(SpnegoNegState.RequestMic, null, null, null);

        var encoded = SpnegoEncoder.EncodeNegTokenResp(response);
        var decoded = SpnegoDecoder.DecodeNegTokenResp(encoded);

        await Assert.That(decoded.NegState.HasValue).IsTrue();
        await Assert.That(decoded.NegState.GetValueOrDefault()).IsEqualTo(SpnegoNegState.RequestMic);
        await Assert.That(decoded.SupportedMech).IsNull();
        await Assert.That(decoded.ResponseToken).IsNull();
        await Assert.That(decoded.MechListMic).IsNull();
    }

    [Test]
    public async Task EncodeNegTokenResp_with_mechListMIC_verifies_with_matching_provider()
    {
        var mechListBytes = SpnegoEncoder.EncodeMechTypeList([SpnegoOids.KerberosV5, SpnegoOids.Ntlmssp]);
        var micProvider = new NtlmMicProvider(SigningKey);
        var response = new SpnegoNegTokenResp(SpnegoNegState.AcceptCompleted, SpnegoOids.Ntlmssp, null, null);

        var encoded = SpnegoEncoder.EncodeNegTokenResp(response, mechListBytes, micProvider);
        var decoded = SpnegoDecoder.DecodeNegTokenResp(encoded);

        await Assert.That(decoded.MechListMic.HasValue).IsTrue();
        await Assert.That(decoded.VerifyMechListMic(mechListBytes, micProvider)).IsTrue();
    }

    [Test]
    public async Task VerifyMechListMic_with_tampered_mic_fails()
    {
        var mechListBytes = SpnegoEncoder.EncodeMechTypeList([SpnegoOids.KerberosV5, SpnegoOids.Ntlmssp]);
        var micProvider = new NtlmMicProvider(SigningKey);
        var encoded = SpnegoEncoder.EncodeNegTokenResp(
            new SpnegoNegTokenResp(SpnegoNegState.AcceptCompleted, SpnegoOids.Ntlmssp, null, null),
            mechListBytes,
            micProvider);
        var decoded = SpnegoDecoder.DecodeNegTokenResp(encoded);
        var tamperedMic = decoded.MechListMic.GetValueOrDefault().ToArray();
        tamperedMic[4] ^= 0xFF;
        var tamperedResponse = decoded with { MechListMic = tamperedMic };

        await Assert.That(tamperedResponse.VerifyMechListMic(mechListBytes, micProvider)).IsFalse();
    }

    [Test]
    public async Task VerifyMechListMic_with_tampered_mechList_fails()
    {
        var mechListBytes = SpnegoEncoder.EncodeMechTypeList([SpnegoOids.KerberosV5, SpnegoOids.Ntlmssp]);
        var micProvider = new NtlmMicProvider(SigningKey);
        var encoded = SpnegoEncoder.EncodeNegTokenResp(
            new SpnegoNegTokenResp(SpnegoNegState.AcceptCompleted, SpnegoOids.Ntlmssp, null, null),
            mechListBytes,
            micProvider);
        var decoded = SpnegoDecoder.DecodeNegTokenResp(encoded);
        var tamperedMechList = (byte[])mechListBytes.Clone();
        tamperedMechList[^1] ^= 0x01;

        await Assert.That(decoded.VerifyMechListMic(tamperedMechList, micProvider)).IsFalse();
    }

    [Test]
    public async Task EncodeNegTokenResp_known_answer_round_trips_byte_exactly()
    {
        byte[] knownGood =
        [
            0xA1, 0x1A,
            0x30, 0x18,
            0xA0, 0x03, 0x0A, 0x01, 0x00,
            0xA1, 0x0B, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x12, 0x01, 0x02, 0x02,
            0xA2, 0x04, 0x04, 0x02, 0x01, 0x02,
        ];
        var response = new SpnegoNegTokenResp(
            SpnegoNegState.AcceptCompleted,
            SpnegoOids.KerberosV5,
            new byte[] { 0x01, 0x02 },
            null);

        var encoded = SpnegoEncoder.EncodeNegTokenResp(response);
        var reencoded = SpnegoEncoder.EncodeNegTokenResp(SpnegoDecoder.DecodeNegTokenResp(knownGood));

        await Assert.That(encoded.AsSpan().SequenceEqual(knownGood)).IsTrue();
        await Assert.That(reencoded.AsSpan().SequenceEqual(knownGood)).IsTrue();
    }

    [Test]
    public async Task DecodeNegTokenInit_captures_exact_mechList_sequence_bytes()
    {
        byte[] apReq = [0x60, 0x61, 0x62];
        var init = SpnegoTokenBuilder.CreateKerberosPreferredInit(apReq);
        var encoded = SpnegoEncoder.EncodeNegTokenInit(init);
        var expectedMechListBytes = SpnegoEncoder.EncodeMechTypeList(init.MechTypes);

        var decoded = SpnegoDecoder.DecodeNegTokenInit(encoded);

        await Assert.That(decoded.MechTypes.Count).IsEqualTo(2);
        await Assert.That(decoded.MechTypes[0]).IsEqualTo(SpnegoOids.KerberosV5);
        await Assert.That(decoded.MechTypes[1]).IsEqualTo(SpnegoOids.Ntlmssp);
        await Assert.That(decoded.MechToken.Span.SequenceEqual(apReq)).IsTrue();
        await Assert.That(decoded.MechListBytes.Span.SequenceEqual(expectedMechListBytes)).IsTrue();
    }
}
