// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Kerberos.Spnego;
using Opc.Classic.Security;

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Kerberos / SPNEGO implementation of the DCOM authentication context abstraction.
/// </summary>
public sealed class KerberosAuthContext : IAuthContext, IAuthSessionKeyProvider
{
    private readonly IKerberosConnectionContext _kerberosCtx;
    private readonly ChannelBindings? _channelBindings;
    private readonly IGssMicProvider? _micProvider;
    private IKerberosSession? _session;
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
        _micProvider = micProvider;
        ProtectionLevel = protectionLevel;
    }

    /// <inheritdoc />
    public OpcProtectionLevel ProtectionLevel { get; }

    /// <summary>
    /// SPNEGO auth-service code (MS-RPCE §2.2.1.1.7).
    /// </summary>
    public byte AuthenticationServiceCode => 0x09;

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
        UpdateSessionFromEstablishedKey();
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
            UpdateSessionFromEstablishedKey();
        }

        if (resp.MechListMic.HasValue)
        {
            VerifyMechListMic(resp);
        }

        return [];
    }

    /// <inheritdoc />
    public int GetVerifierLength(
        int signedRegionLength,
        int confidentialLength)
    {
        _ = signedRegionLength;
        _ = confidentialLength;
        return EstablishedSession.GetRpcVerifierLength(
            ProtectionLevel >= OpcProtectionLevel.Privacy);
    }

    private void VerifyMechListMic(SpnegoNegTokenResp response)
    {
        if (_mechListBytes is null || _mechListBytes.Length == 0)
        {
            throw new InvalidOperationException("SPNEGO mechListMIC verification requires the original NegTokenInit mechType list.");
        }

        if (!response.VerifyMechListMic(_mechListBytes, GetMicProvider()))
        {
            throw new InvalidOperationException("SPNEGO mechListMIC verification failed.");
        }
    }

    private IGssMicProvider GetMicProvider()
    {
        if (_micProvider is not null)
        {
            return _micProvider;
        }

        return new KerberosMicProvider(EstablishedSession);
    }

    private void UpdateSessionFromEstablishedKey()
    {
        if (_kerberosCtx.EstablishedSessionKey is not { } sessionKey)
        {
            return;
        }

        _session = new KerberosSession(
            sessionKey.Key.Span,
            sessionKey.EncryptionType,
            sessionKey.SendSequenceNumber,
            sessionKey.ReceiveSequenceNumber,
            isAcceptor: false,
            usesAcceptorSubkey: sessionKey.UsesAcceptorSubkey);
    }

    private IKerberosSession EstablishedSession => _session ?? throw new InvalidOperationException(
        "Kerberos packet protection is not available until the AP-REQ/AP-REP context is established.");

    /// <inheritdoc />
    public ReadOnlyMemory<byte>? GetSessionKey() => _kerberosCtx.EstablishedSessionKey?.Key;

    /// <inheritdoc />
    public void SignAndSeal(Span<byte> signedRegion, int confidentialOffset, int confidentialLength, out byte[] signature)
    {
        if (ProtectionLevel < OpcProtectionLevel.Integrity)
        {
            signature = [];
            return;
        }

        signature = EstablishedSession.ProtectRpcMessage(
            signedRegion,
            confidentialOffset,
            confidentialLength,
            ProtectionLevel >= OpcProtectionLevel.Privacy);
    }

    /// <inheritdoc />
    public bool VerifyAndUnseal(Span<byte> signedRegion, int confidentialOffset, int confidentialLength, ReadOnlyMemory<byte> signature)
    {
        if (ProtectionLevel < OpcProtectionLevel.Integrity)
        {
            return signature.IsEmpty;
        }

        try
        {
            EstablishedSession.UnprotectRpcMessage(
                signedRegion,
                confidentialOffset,
                confidentialLength,
                signature.Span,
                ProtectionLevel >= OpcProtectionLevel.Privacy);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
        catch (System.Security.SecurityException)
        {
            return false;
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            return false;
        }
    }
}
