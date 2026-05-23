//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Kerberos.NET;
using Kerberos.NET.Client;
using Kerberos.NET.Configuration;
using Kerberos.NET.Credentials;
using Kerberos.NET.Crypto;
using Kerberos.NET.Entities;

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Owns the per-connection Kerberos authentication handshake state.
/// </summary>
public sealed class KerberosConnectionContext : IKerberosConnectionContext
{
    private const byte GssInitialContextTokenTag = 0x60;
    private const byte GssApRepTokenId0 = 0x02;
    private const byte GssApRepTokenId1 = 0x00;
    private const byte KerberosApRepApplicationTag = 0x6f;
    private const int GssKerberosTokenHeaderLength = 2;

    private readonly KerberosAuthInfo _info;
    private ApplicationSessionContext? _sessionContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="KerberosConnectionContext" /> class.
    /// </summary>
    /// <param name="info">Kerberos authentication configuration.</param>
    public KerberosConnectionContext(KerberosAuthInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        _info = info;
    }

    /// <summary>
    /// Acquires an AP-REQ token for the configured SPN. Returns the GSS-API token bytes
    /// suitable for embedding in a DCOM bind PDU (after wrapping in SPNEGO if SPNEGO
    /// negotiation is enabled - see Phase 3E).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the future KDC request flow.</param>
    /// <returns>The AP-REQ token bytes.</returns>
    public Task<byte[]> AcquireApRequestAsync(CancellationToken cancellationToken = default) =>
        AcquireApRequestAsync(channelBindingsHash: null, cancellationToken);

    /// <summary>
    /// Acquires an AP-REQ token for the configured SPN with an optional Phase 3F channel-bindings hash.
    /// </summary>
    /// <param name="channelBindingsHash">
    /// Optional Phase 3F channel-bindings hash. Kerberos.NET does not currently expose
    /// the AP-REQ authorization-data hook needed to embed this value as
    /// KERB_AD_RESTRICTION_ENTRY, so this is accepted for the integration point and
    /// deferred until that API surface is available.
    /// </param>
    /// <param name="cancellationToken">Cancellation token for the future KDC request flow.</param>
    /// <returns>The AP-REQ token bytes.</returns>
    public async Task<byte[]> AcquireApRequestAsync(
        ReadOnlyMemory<byte>? channelBindingsHash,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (channelBindingsHash.HasValue && !channelBindingsHash.Value.IsEmpty)
        {
            _ = channelBindingsHash.Value;
        }

        var credential = CreateCredential();
        using var client = CreateKerberosClient();

        await client.Authenticate(credential).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        var sessionContext = await client.GetServiceTicket(
            new RequestServiceTicket
            {
                ServicePrincipalName = _info.Spn,
                Realm = _info.Realm,
                ApOptions = ApOptions.MutualRequired,
                GssContextFlags = GssContextEstablishmentFlag.GSS_C_MUTUAL_FLAG,
                IncludeSequenceNumber = true,
            },
            cancellationToken).ConfigureAwait(false);

        _sessionContext = sessionContext;
        return sessionContext.ApReq.EncodeGssApi().ToArray();
    }

    /// <summary>
    /// Processes the server's AP-REP token to complete the mutual-auth handshake and
    /// derive the session key.
    /// </summary>
    /// <param name="apReply">AP-REP token bytes returned by the server.</param>
    /// <param name="cancellationToken">Cancellation token for the future AP-REP processing flow.</param>
    /// <returns>The derived session key bytes.</returns>
    public Task<byte[]> ProcessApResponseAsync(ReadOnlyMemory<byte> apReply, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var applicationToken = ExtractApReplyApplicationToken(apReply);
        var sessionContext = _sessionContext ?? throw new InvalidOperationException(
            "AcquireApRequestAsync must complete before processing an AP-REP token.");

        var sessionKey = sessionContext.AuthenticateServiceResponse(applicationToken);
        return Task.FromResult(sessionKey.KeyValue.ToArray());
    }

    private KerberosClient CreateKerberosClient()
    {
        var configuration = Krb5Config.CurrentUser();
        configuration.Defaults.DefaultRealm = _info.Realm;

        return new KerberosClient(configuration, logger: null);
    }

    private KerberosCredential CreateCredential()
    {
        if (_info.Password is not null)
        {
            return new KerberosPasswordCredential(_info.Username, _info.Password, _info.Realm);
        }

        if (_info.KeytabPath is not null)
        {
            return new KeytabCredential(_info.Username, ReadKeytab(_info.KeytabPath), _info.Realm);
        }

        throw new InvalidOperationException("KerberosAuthInfo must carry either Password or KeytabPath.");
    }

    private static KeyTable ReadKeytab(string path)
    {
        using var stream = File.OpenRead(path);
        return new KeyTable(stream);
    }

    private static ReadOnlyMemory<byte> ExtractApReplyApplicationToken(ReadOnlyMemory<byte> apReply)
    {
        if (apReply.IsEmpty)
        {
            throw new KerberosProtocolException("AP-REP token is empty.");
        }

        var span = apReply.Span;
        if (span[0] == KerberosApRepApplicationTag)
        {
            return apReply;
        }

        if (HasGssApRepTokenId(span))
        {
            return apReply[GssKerberosTokenHeaderLength..];
        }

        if (span[0] == GssInitialContextTokenTag)
        {
            var token = GssApiToken.Decode(apReply).Token;
            var tokenSpan = token.Span;

            if (!token.IsEmpty && tokenSpan[0] == KerberosApRepApplicationTag)
            {
                return token;
            }

            if (HasGssApRepTokenId(tokenSpan))
            {
                return token[GssKerberosTokenHeaderLength..];
            }
        }

        throw new KerberosProtocolException("AP-REP token was not a raw AP-REP or recognized GSS-API KRB_AP_REP frame.");
    }

    private static bool HasGssApRepTokenId(ReadOnlySpan<byte> token)
    {
        return token.Length > GssKerberosTokenHeaderLength &&
            token[0] == GssApRepTokenId0 &&
            token[1] == GssApRepTokenId1 &&
            token[GssKerberosTokenHeaderLength] == KerberosApRepApplicationTag;
    }
}
