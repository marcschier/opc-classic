//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;
using System.Formats.Asn1;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpcClassic.Dcom.Kerberos.Spnego;
using OpcClassic.Security;
using TUnit.Core;

namespace OpcClassic.Dcom.Kerberos.Tests;

public sealed class KerberosAuthContextTests
{
    [Test]
    public async Task BuildInitialToken_returns_SPNEGO_wrapped_AP_REQ_bytes()
    {
        byte[] apReq = [0x60, 0x61, 0x62];
        var kerberos = new FakeKerberosConnectionContext(apReq);
        var context = new KerberosAuthContext(kerberos);

        byte[] token = context.BuildInitialToken();

        byte[] spnegoOid = [0x06, 0x06, 0x2B, 0x06, 0x01, 0x05, 0x05, 0x02];
        await Assert.That(kerberos.AcquireCallCount).IsEqualTo(1);
        await Assert.That(token[0]).IsEqualTo((byte)0x60);
        await Assert.That(ContainsSubsequence(token, spnegoOid)).IsTrue();
        await Assert.That(ContainsSubsequence(token, apReq)).IsTrue();
    }

    [Test]
    public async Task ProcessChallengeToken_parses_NegTokenResp_and_extracts_AP_REP()
    {
        byte[] apRep = [0x6F, 0x01, 0x00];
        byte[] encoded = EncodeNegTokenResp(apRep);
        var kerberos = new FakeKerberosConnectionContext([0x60, 0x61, 0x62]);
        var context = new KerberosAuthContext(kerberos);

        byte[] nextToken = context.ProcessChallengeToken(encoded);

        await Assert.That(nextToken.Length).IsEqualTo(0);
        await Assert.That(kerberos.ProcessCallCount).IsEqualTo(1);
        await Assert.That(kerberos.LastApReply.ToArray().SequenceEqual(apRep)).IsTrue();
    }

    [Test]
    public async Task SignAndSeal_throws_NotImplementedException_today()
    {
        var context = new KerberosAuthContext(new FakeKerberosConnectionContext([0x60, 0x61, 0x62]));
        byte[] pduBody = [0x01, 0x02, 0x03];

        var thrown = CaptureException(() => context.SignAndSeal(pduBody, out _));

        await Assert.That(thrown is NotImplementedException).IsTrue();
        await Assert.That(thrown!.Message).Contains("Phase 3F follow-up");
    }

    [Test]
    public async Task KerberosAuthContext_with_channel_bindings_documents_phase3f_integration_point()
    {
        var bindings = new ChannelBindings(
            InitiatorAddrType: 0,
            InitiatorAddress: ReadOnlyMemory<byte>.Empty,
            AcceptorAddrType: 0,
            AcceptorAddress: ReadOnlyMemory<byte>.Empty,
            ApplicationData: new byte[] { 0x10, 0x20, 0x30 });
        byte[] expectedHash = ChannelBindingsHash.Compute(bindings);
        var kerberos = new FakeKerberosConnectionContext([0x60, 0x61, 0x62]);
        var context = new KerberosAuthContext(kerberos, bindings);

        byte[] token = context.BuildInitialToken();

        await Assert.That(token.Length > 0).IsTrue();
        await Assert.That(kerberos.LastChannelBindingsHash.HasValue).IsTrue();
        await Assert.That(kerberos.LastChannelBindingsHash.GetValueOrDefault().ToArray().SequenceEqual(expectedHash)).IsTrue();
    }

    private static byte[] EncodeNegTokenResp(ReadOnlySpan<byte> responseToken)
    {
        var body = new AsnWriter(AsnEncodingRules.DER);
        body.PushSequence();

        var negStateTag = new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true);
        body.PushSequence(negStateTag);
        body.WriteEnumeratedValue(SpnegoNegState.AcceptCompleted);
        body.PopSequence(negStateTag);

        var supportedMechTag = new Asn1Tag(TagClass.ContextSpecific, 1, isConstructed: true);
        body.PushSequence(supportedMechTag);
        body.WriteObjectIdentifier(SpnegoOids.KerberosV5);
        body.PopSequence(supportedMechTag);

        var responseTokenTag = new Asn1Tag(TagClass.ContextSpecific, 2, isConstructed: true);
        body.PushSequence(responseTokenTag);
        body.WriteOctetString(responseToken);
        body.PopSequence(responseTokenTag);

        body.PopSequence();

        var negotiationToken = new AsnWriter(AsnEncodingRules.DER);
        var negTokenRespTag = new Asn1Tag(TagClass.ContextSpecific, 1, isConstructed: true);
        negotiationToken.PushSequence(negTokenRespTag);
        negotiationToken.WriteEncodedValue(body.Encode());
        negotiationToken.PopSequence(negTokenRespTag);
        return negotiationToken.Encode();
    }

    private static bool ContainsSubsequence(ReadOnlySpan<byte> haystack, ReadOnlySpan<byte> needle) =>
        haystack.IndexOf(needle) >= 0;

    private static Exception? CaptureException(Action action)
    {
        try
        {
            action();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    private sealed class FakeKerberosConnectionContext : IKerberosConnectionContext
    {
        private readonly byte[] _apRequest;

        public FakeKerberosConnectionContext(byte[] apRequest)
        {
            _apRequest = apRequest;
        }

        public int AcquireCallCount { get; private set; }

        public int ProcessCallCount { get; private set; }

        public ReadOnlyMemory<byte>? LastChannelBindingsHash { get; private set; }

        public ReadOnlyMemory<byte> LastApReply { get; private set; }

        public Task<byte[]> AcquireApRequestAsync(
            ReadOnlyMemory<byte>? channelBindingsHash,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AcquireCallCount++;
            LastChannelBindingsHash = channelBindingsHash;
            return Task.FromResult(_apRequest);
        }

        public Task<byte[]> ProcessApResponseAsync(ReadOnlyMemory<byte> apReply, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ProcessCallCount++;
            LastApReply = apReply.ToArray();
            return Task.FromResult(Array.Empty<byte>());
        }
    }
}
