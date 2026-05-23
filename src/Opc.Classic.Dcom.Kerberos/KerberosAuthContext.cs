//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using Opc.Classic.Dcom.Kerberos.Spnego;
using Opc.Classic.Security;

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Kerberos / SPNEGO implementation of the DCOM authentication context abstraction.
/// </summary>
public sealed class KerberosAuthContext : IAuthContext
{
    private readonly IKerberosConnectionContext _kerberosCtx;
    private readonly ChannelBindings? _channelBindings;
    private readonly IGssMicProvider _micProvider;
    private byte[]? _mechListBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="KerberosAuthContext" /> class.
    /// </summary>
    /// <param name="authInfo">Kerberos credentials and service principal information.</param>
    /// <param name="channelBindings">Optional channel bindings for extended protection.</param>
    /// <param name="protectionLevel">DCE/RPC packet-protection level.</param>
    /// <param name="micProvider">Optional Kerberos MIC provider for SPNEGO mechListMIC verification.</param>
    public KerberosAuthContext(
        KerberosAuthInfo authInfo,
        ChannelBindings? channelBindings = null,
        OpcProtectionLevel protectionLevel = OpcProtectionLevel.Integrity,
        IGssMicProvider? micProvider = null)
        : this(new KerberosConnectionContext(authInfo), channelBindings, protectionLevel, micProvider)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KerberosAuthContext" /> class.
    /// </summary>
    /// <param name="kerberosContext">Kerberos handshake context.</param>
    /// <param name="channelBindings">Optional channel bindings for extended protection.</param>
    /// <param name="protectionLevel">DCE/RPC packet-protection level.</param>
    /// <param name="micProvider">Optional Kerberos MIC provider for SPNEGO mechListMIC verification.</param>
    public KerberosAuthContext(
        IKerberosConnectionContext kerberosContext,
        ChannelBindings? channelBindings = null,
        OpcProtectionLevel protectionLevel = OpcProtectionLevel.Integrity,
        IGssMicProvider? micProvider = null)
    {
        ArgumentNullException.ThrowIfNull(kerberosContext);

        _kerberosCtx = kerberosContext;
        _channelBindings = channelBindings;
        _micProvider = micProvider ?? new KerberosMicProvider();
        ProtectionLevel = protectionLevel;
    }

    /// <inheritdoc />
    public OpcProtectionLevel ProtectionLevel { get; }

    /// <inheritdoc />
    public byte[] BuildInitialToken()
    {
        ReadOnlyMemory<byte>? channelBindingsHash = null;
        if (_channelBindings is not null)
        {
            channelBindingsHash = ChannelBindingsHash.Compute(_channelBindings);
        }

#pragma warning disable VSTHRD002 // IAuthContext is synchronous; Kerberos.NET ticket acquisition is async.
        var apReq = _kerberosCtx.AcquireApRequestAsync(channelBindingsHash, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
        return SpnegoTokenBuilder.BuildInitToken(apReq, out _mechListBytes);
    }

    /// <inheritdoc />
    public byte[] ProcessChallengeToken(ReadOnlyMemory<byte> serverToken)
    {
        var resp = SpnegoDecoder.DecodeNegTokenResp(serverToken);
        if (resp.ResponseToken is { } responseToken)
        {
#pragma warning disable VSTHRD002 // IAuthContext is synchronous; Kerberos.NET AP-REP processing is async.
            _ = _kerberosCtx.ProcessApResponseAsync(responseToken, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
        }

        if (resp.MechListMic.HasValue)
        {
            VerifyMechListMic(resp);
        }

        return [];
    }

    private void VerifyMechListMic(SpnegoNegTokenResp response)
    {
        if (_mechListBytes is null || _mechListBytes.Length == 0)
        {
            throw new InvalidOperationException("SPNEGO mechListMIC verification requires the original NegTokenInit mechType list.");
        }

        if (!response.VerifyMechListMic(_mechListBytes, _micProvider))
        {
            throw new InvalidOperationException("SPNEGO mechListMIC verification failed.");
        }
    }

    /// <inheritdoc />
    public void SignAndSeal(Span<byte> pduBody, out byte[] signature)
    {
        _ = pduBody;
        signature = [];
#pragma warning disable MA0025 // N8 scaffold intentionally documents deferred Kerberos signing.
        throw new NotImplementedException("Phase 3F follow-up: Kerberos gss_get_mic / gss_wrap signing");
#pragma warning restore MA0025
    }

    /// <inheritdoc />
    public bool VerifyAndUnseal(Span<byte> pduBody, ReadOnlyMemory<byte> signature)
    {
        _ = pduBody;
        _ = signature;
#pragma warning disable MA0025 // N8 scaffold intentionally documents deferred Kerberos verification.
        throw new NotImplementedException("Phase 3F follow-up: Kerberos gss_verify_mic / gss_unwrap");
#pragma warning restore MA0025
    }
}
