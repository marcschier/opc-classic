//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Formats.Asn1;
using System.Security.Claims;
using Kerberos.NET;
using Kerberos.NET.Crypto;
using Opc.Classic.Dcom.Kerberos.Spnego;
using Opc.Classic.Security;

namespace Opc.Classic.Dcom.Kerberos.Tests;

[ClassDataSource<KdcFixture>(Shared = SharedType.PerAssembly)]
[Category("Kerberos")]
[NotInParallel]
public sealed class KerberosKdcIntegrationTests
{
    private static readonly ChannelBindings MatchingChannelBindings = new(
        InitiatorAddrType: 0,
        InitiatorAddress: ReadOnlyMemory<byte>.Empty,
        AcceptorAddrType: 0,
        AcceptorAddress: ReadOnlyMemory<byte>.Empty,
        ApplicationData: "tls-server-end-point:opc-classic-kdc"u8.ToArray());

    private readonly KdcFixture _kdc;

    public KerberosKdcIntegrationTests(KdcFixture kdc)
    {
        _kdc = kdc;
    }

    [Test, Category("Kerberos")]
    public async Task KerberosAuthContext_requests_live_kdc_ticket_and_wraps_ap_req_in_spnego()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        _ = _kdc.TestUserKeyTable;
        var context = new KerberosAuthContext(_kdc.CreateUserKeytabAuthInfo());

        byte[] token = context.BuildInitialToken();
        ReadOnlyMemory<byte> optimisticToken = ExtractOptimisticMechanismToken(token);
        KerberosIdentity identity = await AuthenticateAsServerAsync(optimisticToken).ConfigureAwait(false);

        await Assert.That(token[0]).IsEqualTo((byte)0x60);
        await Assert.That(optimisticToken.Length > 0).IsTrue();
        await Assert.That(optimisticToken.Span[0]).IsEqualTo((byte)0x60);
        await Assert.That(identity.FindFirst(ClaimTypes.NameIdentifier)?.Value).Contains(KdcFixture.TestUserName);
    }

    [Test, Category("Kerberos")]
    public async Task Mutual_auth_round_trips_ap_req_to_ap_rep_against_server_keytab()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        var context = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());

        byte[] apReq = await context.AcquireApRequestAsync().ConfigureAwait(false);
        KerberosIdentity identity = await AuthenticateAsServerAsync(apReq).ConfigureAwait(false);
        byte[] apRep = Convert.FromBase64String(identity.ApRep);

        byte[] sessionKey = await context.ProcessApResponseAsync(apRep).ConfigureAwait(false);

        await Assert.That(apReq[0]).IsEqualTo((byte)0x60);
        await Assert.That(apRep[0]).IsEqualTo((byte)0x6f);
        await Assert.That(sessionKey.SequenceEqual(identity.SessionKey.ToArray())).IsTrue();
    }

    [Test, Category("Kerberos")]
    public async Task Replay_protection_rejects_second_use_of_same_ap_req()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        var context = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());
        byte[] apReq = await context.AcquireApRequestAsync().ConfigureAwait(false);
        var authenticator = new KerberosAuthenticator(CreateValidator(_kdc.ServerKeyTable));

        _ = await authenticator.Authenticate(apReq).ConfigureAwait(false);
        Exception? thrown = await CaptureExceptionAsync(() => authenticator.Authenticate(apReq)).ConfigureAwait(false);

        await Assert.That(ContainsException<ReplayException>(thrown)).IsTrue();
    }

    [Test, Category("Kerberos")]
    public async Task Channel_binding_hash_is_embedded_and_tamper_is_detected()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        byte[] expectedHash = ChannelBindingsHash.Compute(MatchingChannelBindings);
        var context = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo());

        byte[] apReq = await context.AcquireApRequestAsync(expectedHash).ConfigureAwait(false);
        DecryptedKrbApReq decrypted = await CreateValidator(_kdc.ServerKeyTable).Validate(apReq).ConfigureAwait(false);
        ReadOnlyMemory<byte> actualHash = ExtractChannelBindingHash(decrypted);
        Exception? tamperFailure = CaptureException(() => ValidateChannelBinding(decrypted, TamperedChannelBindings()));

        await Assert.That(actualHash.Span.SequenceEqual(expectedHash)).IsTrue();
        await Assert.That(tamperFailure is KerberosValidationException).IsTrue();
        await Assert.That(tamperFailure!.Message).Contains("channel bindings");
    }

    [Test, Category("Kerberos")]
    public async Task Expired_service_ticket_is_rejected_with_ticket_expired_semantics()
    {
        SkipWhenKdcUnavailable();
        using var krb5Config = _kdc.UseKrb5Config();
        var context = new KerberosConnectionContext(_kdc.CreatePasswordAuthInfo(KdcFixture.ShortLivedServerSpn));
        byte[] apReq = await context.AcquireApRequestAsync().ConfigureAwait(false);
        DecryptedKrbApReq decrypted = await CreateValidator(_kdc.ShortLivedServerKeyTable).Validate(apReq).ConfigureAwait(false);
        var expiredValidator = CreateValidator(_kdc.ShortLivedServerKeyTable);
        expiredValidator.Now = () => decrypted.Ticket.EndTime.AddMinutes(6);

        Exception? thrown = await CaptureExceptionAsync(() => expiredValidator.Validate(apReq)).ConfigureAwait(false);

        await Assert.That(decrypted.Ticket.EndTime - DateTimeOffset.UtcNow < TimeSpan.FromMinutes(2)).IsTrue();
        await Assert.That(thrown is KerberosValidationException).IsTrue();
        await Assert.That(thrown!.Message).Contains("expired");
    }

    private void SkipWhenKdcUnavailable()
    {
        if (!_kdc.IsAvailable)
        {
            Skip.Test(_kdc.SkipReason ?? $"Requires Docker — set {KdcFixture.RunEnvironmentVariable}=1 to enable.");
        }
    }

    private Task<KerberosIdentity> AuthenticateAsServerAsync(ReadOnlyMemory<byte> apReq)
    {
        var authenticator = new KerberosAuthenticator(CreateValidator(_kdc.ServerKeyTable));
        return AuthenticateAsKerberosIdentityAsync(authenticator, apReq);
    }

    private static KerberosValidator CreateValidator(KeyTable keyTable) => new(keyTable)
    {
        ValidateAfterDecrypt = ValidationActions.All,
    };

    private static async Task<KerberosIdentity> AuthenticateAsKerberosIdentityAsync(
        KerberosAuthenticator authenticator,
        ReadOnlyMemory<byte> apReq)
    {
        var identity = await authenticator.Authenticate(apReq).ConfigureAwait(false);
        return identity as KerberosIdentity ?? throw new InvalidOperationException("Kerberos authenticator did not return a KerberosIdentity.");
    }

    private static ReadOnlyMemory<byte> ExtractOptimisticMechanismToken(ReadOnlyMemory<byte> token)
    {
        var reader = new AsnReader(token, AsnEncodingRules.DER);
        var initialContextTokenTag = new Asn1Tag(TagClass.Application, 0, isConstructed: true);
        var initialContextToken = reader.ReadSequence(initialContextTokenTag);
        string oid = initialContextToken.ReadObjectIdentifier();
        if (!StringComparer.Ordinal.Equals(oid, SpnegoOids.Spnego))
        {
            throw new AsnContentException();
        }

        var negTokenInitTag = new Asn1Tag(TagClass.ContextSpecific, 0, isConstructed: true);
        var negotiationToken = initialContextToken.ReadSequence(negTokenInitTag);
        var body = negotiationToken.ReadSequence();
        while (body.HasData)
        {
            var tag = body.PeekTag();
            if (tag.TagClass == TagClass.ContextSpecific && tag.TagValue == 2)
            {
                var mechToken = body.ReadSequence(tag);
                return mechToken.ReadOctetString();
            }

            _ = body.ReadEncodedValue();
        }

        throw new AsnContentException();
    }

    private static ReadOnlyMemory<byte> ExtractChannelBindingHash(DecryptedKrbApReq decrypted)
    {
        var checksum = decrypted.Authenticator.Checksum ?? throw new KerberosValidationException("AP-REQ authenticator does not contain channel bindings.");
        var delegationInfo = checksum.DecodeDelegation() ?? throw new KerberosValidationException("AP-REQ authenticator checksum is not a GSS checksum.");
        return delegationInfo.ChannelBinding;
    }

    private static void ValidateChannelBinding(DecryptedKrbApReq decrypted, ChannelBindings expectedBindings)
    {
        ReadOnlyMemory<byte> actual = ExtractChannelBindingHash(decrypted);
        byte[] expected = ChannelBindingsHash.Compute(expectedBindings);
        if (actual.IsEmpty || !actual.Span.SequenceEqual(expected))
        {
            throw new KerberosValidationException("KRB_AP_ERR_BAD_BINDINGS: AP-REQ channel bindings do not match expected channel bindings.");
        }
    }

    private static ChannelBindings TamperedChannelBindings() => new(
        InitiatorAddrType: 0,
        InitiatorAddress: ReadOnlyMemory<byte>.Empty,
        AcceptorAddrType: 0,
        AcceptorAddress: ReadOnlyMemory<byte>.Empty,
        ApplicationData: "tls-server-end-point:tampered"u8.ToArray());

    private static async Task<Exception?> CaptureExceptionAsync(Func<Task> action)
    {
        try
        {
            await action().ConfigureAwait(false);
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

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

    private static bool ContainsException<TException>(Exception? exception)
        where TException : Exception =>
        exception is TException || exception?.InnerException is not null && ContainsException<TException>(exception.InnerException);
}
