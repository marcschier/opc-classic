// SPDX-License-Identifier: MIT

using Opc.Classic;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Kerberos;
using Opc.Classic.Security;
using System;
using System.Buffers.Binary;
using SharpCifs;
using SharpCifs.Util.Sharpen;
using System.Globalization;
using System.IO;
using System.Net;
using System.Security;
using System.Security.Cryptography;

namespace Opc.Classic.Dcom.Rpc.Auth.ntlm; 
/// <summary>
/// Ntlm auth
/// </summary>
public class NtlmAuthentication {

    /// <summary>
    /// Type of auth
    /// </summary>
    public const int AUTHENTICATIONSERVICENTLM = 10;


    /// <summary>
    /// Create auth
    /// </summary>
    /// <param name="properties"></param>
    public NtlmAuthentication(PropertyBag properties) {
        _properties = properties;
        _useNtlm2sessionsecurity = true;
        _useNtlmV2 = true;
        string domain = null;
        string user = null;
        string password = null;
        if (properties != null) {
            _lanManagerKey = Convert.ToBoolean(properties.GetProperty("rpc.ntlm.lanManagerKey"));
            _seal = Convert.ToBoolean(properties.GetProperty("rpc.ntlm.seal"));
            _sign = _seal || Convert.ToBoolean(properties.GetProperty("rpc.ntlm.sign"));
            _keyExchange = Convert.ToBoolean(properties.GetProperty("rpc.ntlm.keyExchange"));
            var keyLength = (string)properties.GetProperty("rpc.ntlm.keyLength");
            if (keyLength != null) {
                try {
                    _keyLength = int.Parse(keyLength, CultureInfo.InvariantCulture);
                }
                catch (FormatException) {
                    throw new ArgumentException("Invalid key length: " + keyLength, nameof(properties));
                }
            }

            _useNtlm2sessionsecurity = GetBooleanProperty(properties, "rpc.ntlm.ntlm2", true);
            _useNtlmV2 = GetBooleanProperty(properties, "rpc.ntlm.ntlmv2", true);
            _allowNtlmV1 = Convert.ToBoolean(properties.GetProperty("rpc.ntlm.allowV1"));
            _useSSO = Convert.ToBoolean(properties.GetProperty("rpc.ntlm.sso"));
            _channelBindingsHash = CloneChannelBindingsHash(properties.GetProperty("rpc.ntlm.channelBindingsHash"));
            domain = (string)properties.GetProperty("rpc.ntlm.domain");
            user = (string)properties.GetProperty(Opc.Classic.Dcom.Rpc.Security.USERNAME);
            password = (string)properties.GetProperty(Opc.Classic.Dcom.Rpc.Security.PASSWORD);
        }

        if (!_useNtlmV2 && !_allowNtlmV1) {
            throw new NotSupportedException(
                "NTLMv1 is disabled by default; set rpc.ntlm.allowV1=true to re-enable (not recommended).");
        }

        if (_useSSO) {
            // Phase 2F: the Windows-only SSPIJNIClient (P/Invoke to ntlmauth.dll)
            // is incompatible with the cross-platform .NET 10 target. Single
            // sign-on returns in Phase 3D via Kerberos.NET-backed Kerberos /
            // SPNEGO authentication. Until then, callers must supply explicit
            // domain / user / password credentials.
            throw new PlatformNotSupportedException(
                "NTLM SSO (rpc.ntlm.sso=true) is not supported on this platform. " +
                "Use explicit username/password NTLMv2 credentials, or wait for " +
                "Kerberos/SPNEGO support in Opc.Classic.Dcom.Kerberos (Phase 3D).");
        }
        _credentials = new NetworkCredential(user, password, domain);
    }

    /// <summary>
    /// Creates the managed authentication context selected by <see cref="OpcConnectData.AuthMode" />.
    /// </summary>
    /// <param name="connectData">OPC connection authentication settings.</param>
    /// <returns>The authentication context used by DCOM bind and call PDUs.</returns>
    public static IAuthContext CreateAuthContext(OpcConnectData connectData) {
        ArgumentNullException.ThrowIfNull(connectData);

        return connectData.AuthMode switch {
            OpcAuthMode.Anonymous => NoOpAuthContext.Instance,
            OpcAuthMode.Kerberos => new KerberosAuthContext(
                BuildKerberosAuthInfo(connectData),
                channelBindings: connectData.ChannelBindings,
                protectionLevel: connectData.ProtectionLevel),
            OpcAuthMode.NtlmV2 => new NtlmAuthContext(connectData),
            _ => throw new NotSupportedException($"Auth mode {connectData.AuthMode} not supported")
        };
    }

    private static KerberosAuthInfo BuildKerberosAuthInfo(OpcConnectData connectData) {
        var credentials = connectData.Credentials ?? throw new InvalidOperationException(
            "Kerberos authentication requires credentials.");
        var host = string.IsNullOrWhiteSpace(connectData.Url.Host) ? "localhost" : connectData.Url.Host;
        var realm = !string.IsNullOrWhiteSpace(credentials.Domain)
            ? credentials.Domain.ToUpperInvariant()
            : ExtractRealm(credentials.UserName) ?? host.ToUpperInvariant();

        return new KerberosAuthInfo(
            realm,
            "RPCSS/" + host,
            credentials.UserName,
            string.IsNullOrWhiteSpace(credentials.Domain) ? null : credentials.Domain,
            credentials.Password,
            keytabPath: null);
    }

    private static string ExtractRealm(string userName) {
        var separator = userName.LastIndexOf('@');
        return separator > 0 && separator < userName.Length - 1
            ? userName[(separator + 1)..].ToUpperInvariant()
            : null;
    }

    private static PropertyBag CreateNtlmProperties(OpcConnectData connectData) {
        var credentials = connectData.Credentials ?? throw new InvalidOperationException(
            "NTLM authentication requires credentials.");
        var properties = new PropertyBag();
        var sign = connectData.ProtectionLevel >= OpcProtectionLevel.Integrity;
        var seal = connectData.ProtectionLevel >= OpcProtectionLevel.Privacy;

        properties.SetProperty("rpc.ntlm.lanManagerKey", "false");
        properties.SetProperty("rpc.ntlm.sign", sign.ToString());
        properties.SetProperty("rpc.ntlm.seal", seal.ToString());
        properties.SetProperty("rpc.ntlm.keyExchange", sign.ToString());
        properties.SetProperty("rpc.ntlm.keyLength", "128");
        properties.SetProperty("rpc.ntlm.ntlm2", "true");
        properties.SetProperty("rpc.ntlm.ntlmv2", "true");
        properties.SetProperty("rpc.ntlm.allowV1", "false");
        properties.SetProperty("rpc.ntlm.sso", "false");
        properties.SetProperty("rpc.ntlm.domain", credentials.Domain);
        if (connectData.ChannelBindings is not null) {
            properties.SetProperty("rpc.ntlm.channelBindingsHash", ChannelBindingsHash.Compute(connectData.ChannelBindings));
        }
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.USERNAME, credentials.UserName);
        properties.SetProperty(Opc.Classic.Dcom.Rpc.Security.PASSWORD, credentials.Password);
        return properties;
    }

    private sealed class NtlmAuthContext : IAuthContext {
        private readonly NtlmAuthentication _authentication;

        public NtlmAuthContext(OpcConnectData connectData) {
            ArgumentNullException.ThrowIfNull(connectData);
            _authentication = new NtlmAuthentication(CreateNtlmProperties(connectData));
            ProtectionLevel = connectData.ProtectionLevel;
        }

        public OpcProtectionLevel ProtectionLevel { get; }

        public byte[] BuildInitialToken() {
            var type1 = _authentication.CreateType1();
            var token = type1.ToByteArray();
            _authentication.SetNegotiateMessage(token);
            return token;
        }

        public byte[] ProcessChallengeToken(ReadOnlyMemory<byte> serverToken) {
            var challengeToken = serverToken.ToArray();
            _authentication.SetChallengeMessage(challengeToken);
            var type2 = new Type2Message(challengeToken);
            return _authentication.CreateType3(type2).ToByteArray();
        }

        public void SignAndSeal(Span<byte> pduBody, out byte[] signature) {
            if (ProtectionLevel < OpcProtectionLevel.Integrity) {
                signature = [];
                return;
            }

            var security = EstablishedSecurity;
            var buffer = new byte[pduBody.Length + security.VerifierLength];
            pduBody.CopyTo(buffer.AsSpan());
            var ndr = CreateNdrCodec(buffer);
            security.ProcessOutgoing(ndr, 0, pduBody.Length, pduBody.Length, isFragmented: false);
            buffer.AsSpan(0, pduBody.Length).CopyTo(pduBody);
            signature = buffer.AsSpan(pduBody.Length, security.VerifierLength).ToArray();
        }

        public bool VerifyAndUnseal(Span<byte> pduBody, ReadOnlyMemory<byte> signature) {
            if (ProtectionLevel < OpcProtectionLevel.Integrity) {
                return signature.IsEmpty;
            }

            var security = EstablishedSecurity;
            if (signature.Length != security.VerifierLength) {
                return false;
            }

            var buffer = new byte[pduBody.Length + security.VerifierLength];
            pduBody.CopyTo(buffer.AsSpan());
            signature.Span.CopyTo(buffer.AsSpan(pduBody.Length));
            var ndr = CreateNdrCodec(buffer);
            try {
                security.ProcessIncoming(ndr, 0, pduBody.Length, pduBody.Length, isFragmented: false);
            }
            catch (IntegrityException) {
                return false;
            }

            buffer.AsSpan(0, pduBody.Length).CopyTo(pduBody);
            return true;
        }

        private ISecurity EstablishedSecurity => _authentication.Security ?? throw new InvalidOperationException(
            "NTLM session security is not established until ProcessChallengeToken completes.");

        private static NdrCodec CreateNdrCodec(byte[] buffer) {
            var ndrBuffer = new NdrBuffer(buffer, 0);
            ndrBuffer.SetLength(buffer.Length);
            return new NdrCodec { Buffer = ndrBuffer, Format = NdrFormat.DEFAULT_FORMAT };
        }
    }

    /// <summary>
    /// Get security object
    /// </summary>
    public ISecurity Security { get; private set; }

    /// <summary>
    /// Create type 1 message
    /// </summary>
    /// <exception cref="IOException"></exception>
    /// <returns></returns>
    public Type1Message CreateType1() {
        if (_useSSO) {
            // Unreachable — constructor already throws for _useSSO. Defensive guard.
            throw new PlatformNotSupportedException(
                "NTLM SSO is unsupported on net10; use Kerberos via Phase 3D.");
        }
        var flags = DefaultFlags;
        var type1 = new Type1Message(flags, _credentials.Domain, Type1Message.GetDefaultWorkstation());
        _negotiateMessage = type1.ToByteArray();
        return type1;
    }

    /// <summary>
    /// Create type 2 message
    /// </summary>
    /// <param name="type1"></param>
    /// <exception cref="IOException"></exception>
    /// <returns></returns>
    public Type2Message CreateType2(Type1Message type1) {
        NtlmFlags flags;
        if (type1 == null) {
            flags = DefaultFlags;
        }
        else {
            flags = AdjustFlags(type1.GetFlags());
        }
        flags |= NtlmFlags.NtlmsspTargetTypeServer; // challenge accept response flag
        var challenge = (byte[])kDefaultServerChallenge.Clone();
        _serverChallenge = challenge;
        var type2Message = new Type2Message(flags, challenge,
            _credentials.Domain); // generate our own, since SMB will throw exception here
        if (ShouldRequestMic(flags)) {
            type2Message.SetTargetInformation(NtlmAvPairs.AddMicFlag(type2Message.GetTargetInformation()));
        }

        _challengeFlags = flags;
        _challengeTargetInformation = type2Message.GetTargetInformation();
        _challengeMessage = type2Message.ToByteArray();
        if (type1 != null && _negotiateMessage == null) {
            _negotiateMessage = type1.ToByteArray();
        }

        return type2Message;
    }

    /// <summary>
    /// Create type 3 message
    /// </summary>
    /// <param name="type2"></param>
    /// <exception cref="IOException"></exception>
    /// <returns></returns>
#pragma warning disable MA0051 // Legacy NTLM type-3 negotiation flow; refactor would risk authentication behavior.
    public Type3Message CreateType3(Type2Message type2) {
        if (_useSSO) {
            // Unreachable — constructor already throws for _useSSO. Defensive guard.
            throw new PlatformNotSupportedException(
                "NTLM SSO is unsupported on net10; use Kerberos via Phase 3D.");
        }
        else {
            var flags = type2.GetFlags();
            if ((flags & NtlmFlags.NtlmsspNegotiateDatagramStyle) != NtlmFlags.None) {
                flags = AdjustFlags(flags);
                flags &= ~NtlmFlags.NtlmsspTargetTypeServer;
            }

            var clientNonce = new byte[8];
            byte[] blob = null;
            string target = null; // getTargetFromTargetInformation(type2.GetTargetInformation());

            if (target == null) {
                target = _credentials.Domain.ToUpperInvariant();
                if (target.Equals("")) {
                    target = GetTargetFromTargetInformation(type2.GetTargetInformation());
                }
            }

            Type3Message type3;
            byte[] exportedSessionKey = null;
            if (_useNtlmV2) {
                kRandomGen.NextBytes(clientNonce);
                try {
                    var lmv2Response = Responses.GetLMv2Response(target,
                        _credentials.UserName, _credentials.Password, type2.GetChallenge(), clientNonce);
                    var targetInformation = ApplyChannelBindings(type2.GetTargetInformation());
                    var retval = Responses.GetNTLMv2Response(target,
                        _credentials.UserName, _credentials.Password, targetInformation,
                        type2.GetChallenge(), clientNonce);
                    var ntlmv2Response = retval[0];
                    blob = retval[1];
                    type3 = new Type3Message(flags, lmv2Response, ntlmv2Response,
                        target, _credentials.UserName, Type3Message.GetDefaultWorkstation());
                }
                catch (Exception e) {
                    throw new InvalidOperationException("Exception occurred while forming NTLMv2 Type3Response", e);
                }

            }
            else {
                if ((flags & NtlmFlags.NtlmsspNegotiateNtlm2) != NtlmFlags.None) // NTLM2 Session security response
                {
                    flags = AdjustFlags(flags);
                    flags &= ~NtlmFlags.NtlmsspTargetTypeServer;
                    // flags =  0xe2888235;
                    var challenge = type2.GetChallenge();
                    // LMReponse is 24 bytes. 8 byte random client nonce and the rest is null padded.
                    var lmResponse = new byte[24];

                    kRandomGen.NextBytes(clientNonce);
                    Array.Copy(clientNonce, 0, lmResponse, 0, clientNonce.Length);
                    byte[] ntResponse;
                    try {
                        ntResponse = Responses.GetNTLM2SessionResponse(_credentials.Password, challenge, clientNonce);
                    }
                    catch (Exception e) {
                        throw new InvalidOperationException("Exception occurred while forming Session Security Type3Response", e);
                    }

                    type3 = new Type3Message(flags, lmResponse, ntResponse, target,
                        _credentials.UserName, Type3Message.GetDefaultWorkstation());
                }
                else // Plain NTLMv1 response
                {
                    var challenge = type2.GetChallenge();
                    var lmResponse = Responses.GetLMResponse(_credentials.Password, challenge);
                    var ntResponse = Responses.GetNTLMResponse(_credentials.Password, challenge);
                    type3 = new Type3Message(flags, lmResponse, ntResponse, target,
                        _credentials.UserName, Type3Message.GetDefaultWorkstation());
                    if ((flags & NtlmFlags.NtlmsspNegotiateKeyExch) != NtlmFlags.None) {
                        throw new NotSupportedException("Key Exchange not supported by Library.");
                    }
                }
            }
            // we have to now form lmv2 and ntlmv2 response with regards to the session security
            // the type3message also has to be altered
            if (_useNtlm2sessionsecurity && (flags & NtlmFlags.NtlmsspNegotiateNtlm2) != NtlmFlags.None) {
                var ntlmKeyFactory = new NTLMKeyFactory();
                byte[] userSessionKey;
                if (_useNtlmV2) {
                    try {
                        userSessionKey = ntlmKeyFactory
                            .GetNTLMv2UserSessionKey(target, _credentials.UserName, _credentials.Password, type2.GetChallenge(), blob);
                    }
                    catch (Exception e) {
                        throw new InvalidOperationException("Exception occurred while forming NTLMv2 with NTLM2 Session Security for Type3Response", e);
                    }
                }
                else {
                    // now create the key for the session
                    // this key will be used to RC4 a 16 byte random key and set to the type3 message
                    var servernonce = new byte[16];
                    Array.Copy(type2.GetChallenge(), 0, servernonce, 0, type2.GetChallenge().Length);
                    Array.Copy(clientNonce, 0, servernonce, 8, clientNonce.Length);
                    try {
                        userSessionKey = ntlmKeyFactory
                            .GetNTLM2SessionResponseUserSessionKey(_credentials.Password, servernonce);
                    }
                    catch (Exception e) {
                        throw new InvalidOperationException("Exception occurred while forming Session Security for Type3Response", e);
                    }

                }
                try {
                    if ((flags & NtlmFlags.NtlmsspNegotiateKeyExch) != NtlmFlags.None) {
                        // now RC4 encrypt a random 16 byte key
                        exportedSessionKey = ntlmKeyFactory.SecondarySessionKey;
                        type3.SetSessionKey(ntlmKeyFactory.EncryptSecondarySessionKey(exportedSessionKey, userSessionKey));
                    }
                    else {
                        exportedSessionKey = userSessionKey;
                    }
#pragma warning disable CS0618 // NTLMv1 fallback - explicit opt-in via rpc.ntlm.allowV1
                    Security = new Ntlm1(flags, exportedSessionKey, false);
#pragma warning restore CS0618
                }
                catch (Exception e) {
                    throw new InvalidOperationException("Exception occurred while forming Session Security for Type3Response", e);
                }
            }

            AddMicIfRequired(type2, type3, exportedSessionKey);
            return type3;
        }
    }
#pragma warning restore MA0051

    //      /// <summary>
    //      /// Get authentication sources
    //      /// </summary>
    //      protected internal virtual AuthenticationSource AuthenticationSource {
    //          get {
    //              if (_authenticationSource != null) {
    //                  return _authenticationSource;
    //              }
    //              var sourceClass = (_properties != null) ? (string)_properties.GetProperty("rpc.ntlm.authenticationSource") : null;
    //              if (sourceClass == null) {
    //                  return _authenticationSource = AuthenticationSource.DefaultInstance;
    //              }
    //              try {
    //                  return _authenticationSource = (AuthenticationSource)Type.GetType(sourceClass).newInstance();
    //              }
    //              catch (Exception ex) {
    //                  throw new ArgumentException("Invalid authentication source: " + ex);
    //              }
    //          }
    //      }

    private NtlmFlags DefaultFlags {
        get {
            var flags = kBASICFLAGS;
            if (_lanManagerKey) {
                flags |= NtlmFlags.NtlmsspNegotiateLmKey;
            }
            if (_sign) {
                flags |= NtlmFlags.NtlmsspNegotiateSign;
            }
            if (_seal) {
                flags |= NtlmFlags.NtlmsspNegotiateSeal;
            }
            if (_keyExchange) {
                flags |= NtlmFlags.NtlmsspNegotiateKeyExch | NtlmFlags.NtlmsspNegotiateVersion;
            }
            if (_keyLength >= 56) {
                flags |= NtlmFlags.NtlmsspNegotiate56;
            }
            if (_keyLength >= 128) {
                flags |= NtlmFlags.NtlmsspNegotiate128;
            }
            // We always negotiate for NTLM2 session security
            //        if (useNtlm2sessionsecurity)
            {
                flags |= NtlmFlags.NtlmsspNegotiateNtlm2;
            }

            return flags;
        }
    }

    private NtlmFlags AdjustFlags(NtlmFlags flags) {
        if (kUnicodeSupported && ((flags & NtlmFlags.NtlmsspNegotiateUnicode) != NtlmFlags.None)) {
            flags &= ~NtlmFlags.NtlmsspNegotiateOem;
            flags |= NtlmFlags.NtlmsspNegotiateUnicode;
        }
        else {
            flags &= ~NtlmFlags.NtlmsspNegotiateUnicode;
            flags |= NtlmFlags.NtlmsspNegotiateOem;
        }
        if (!_lanManagerKey) {
            flags &= ~NtlmFlags.NtlmsspNegotiateLmKey;
        }
        if (!(_sign || _seal)) {
            flags &= ~NtlmFlags.NtlmsspNegotiateSign;
        }
        if (!_seal) {
            flags &= ~NtlmFlags.NtlmsspNegotiateSeal;
        }
        if (!_keyExchange) {
            flags &= ~NtlmFlags.NtlmsspNegotiateKeyExch;
        }
        if (_keyLength < 128) {
            flags &= ~NtlmFlags.NtlmsspNegotiate128;
        }
        if (_keyLength < 56) {
            flags &= ~NtlmFlags.NtlmsspNegotiate56;
        }
        //   if (!useNtlm2sessionsecurity)
        //   {
        //       flags &= ~NtlmFlags.NtlmsspNegotiateNtlm2;
        //   }
        return flags;
    }

    /// <summary>
    /// Get target
    /// </summary>
    /// <param name="targetInformation"></param>
    /// <returns></returns>
    private string GetTargetFromTargetInformation(byte[] targetInformation) {
        string target = null;

        var i = 0;
        while (i < targetInformation.Length) {
            switch (BinaryPrimitives.ReadUInt16LittleEndian(targetInformation.AsSpan(i, sizeof(ushort)))) {
                case 1: // Server name
                    i++;
                    i++; // advance two bytes
                    int length = BinaryPrimitives.ReadUInt16LittleEndian(targetInformation.AsSpan(i, sizeof(ushort)));
                    i++;
                    i++; // advance two bytes
                    var domainb = new byte[length];
                    Array.Copy(targetInformation, i, domainb, 0, length);
                    try {
                        target = StringHelperClass.NewString(domainb, "UTF-16LE");
                    }
                    catch (ArgumentException) {
                        return null;
                    }

                    i = targetInformation.Length;
                    break;
                default: // skip bytes
                    i++;
                    i++; // advance two bytes
                    length = BinaryPrimitives.ReadUInt16LittleEndian(targetInformation.AsSpan(i, sizeof(ushort)));
                    i++;
                    i++; // advance two bytes
                    i += length;
                    break;
            }
        }
        return target;
    }

    /// <summary>
    /// Create security
    /// </summary>
    /// <param name="type3"></param>
    internal void CreateSecurityWhenServer(object type3) => CreateSecurityWhenServerCore(type3, null);

    internal void CreateSecurityWhenServerWithMic(object type3, byte[] authenticateMessage) =>
        CreateSecurityWhenServerCore(type3, authenticateMessage);

    private void CreateSecurityWhenServerCore(object type3, byte[]? authenticateMessage) {
        var type3Message = Type3Message.FromObject(type3);
        // two things here...check for anonymous, in that case the user response key is new byte[16].
        // in case anonymous has not been sent then create the key using credentials.
        var flags = type3Message.GetFlags();
        var ntlmKeyFactory = new NTLMKeyFactory();
        byte[] secondayMasterKey;
        byte[] sessionResponseUserSessionKey = null;
        var sessionResponseUserSessionKeyIsSecondaryMasterKey = false;
        if (type3Message.GetFlag(NtlmFlags.NtlmsspNegotiateAnonymous)) // anonymous flag
        {
            // if it is anonymous the user session key is new byte[16];
            sessionResponseUserSessionKey = new byte[16];
        }
        else if (_useNtlmV2) {
            sessionResponseUserSessionKey = CreateNtlmV2ServerSessionKey(type3Message, ntlmKeyFactory);
            sessionResponseUserSessionKeyIsSecondaryMasterKey = true;
        }
        else {
            // now create the key for the session
            // this key will be used to RC4 a 16 byte random key and set to the type3 message
            var servernonce = new byte[16];
            byte[] challenge = { 1, 2, 3, 4, 5, 6, 7, 8 }; // challenge is fixed
            Array.Copy(challenge, 0, servernonce, 0, challenge.Length);
            // first 8 bytes only, the rest are all 0x00 and not required.
            Array.Copy(type3Message.GetLMResponse(), 0, servernonce, 8, 8);
            try {
                sessionResponseUserSessionKey = ntlmKeyFactory
                    .GetNTLM2SessionResponseUserSessionKey(_credentials.Password, servernonce);
            }
            catch (Exception e) {
                throw new InvalidOperationException("Exception occurred while forming Session Security from Type3 AUTH", e);
            }
        }

        try {
            secondayMasterKey = sessionResponseUserSessionKeyIsSecondaryMasterKey ||
                (flags & NtlmFlags.NtlmsspNegotiateKeyExch) == NtlmFlags.None
                ? sessionResponseUserSessionKey
                : ntlmKeyFactory.DecryptSecondarySessionKey(type3Message.GetSessionKey(), sessionResponseUserSessionKey);
            VerifyMicIfRequired(type3Message, secondayMasterKey, authenticateMessage);
#pragma warning disable CS0618 // NTLMv1 fallback - explicit opt-in via rpc.ntlm.allowV1
            Security = new Ntlm1(flags, secondayMasterKey, true);
#pragma warning restore CS0618
        }
        catch (SecurityException) {
            throw;
        }
        catch (Exception e) {
            throw new InvalidOperationException("Exception occurred while forming Session Security Type3Response", e);
        }
    }

    private byte[] CreateNtlmV2ServerSessionKey(Type3Message type3Message, NTLMKeyFactory ntlmKeyFactory) {
        var ntResponse = type3Message.GetNTResponse();
        if (ntResponse == null || ntResponse.Length < 16) {
            throw new SecurityException("Invalid NTLMv2 NT challenge response.");
        }
        if (_serverChallenge == null || _serverChallenge.Length != 8) {
            throw new SecurityException("The NTLMv2 server challenge was not saved before authentication.");
        }

        var ntProofStr = new byte[16];
        Array.Copy(ntResponse, 0, ntProofStr, 0, ntProofStr.Length);
        var temp = new byte[ntResponse.Length - ntProofStr.Length];
        Array.Copy(ntResponse, ntProofStr.Length, temp, 0, temp.Length);
        ValidateChannelBindingsInNtChallengeResponse(temp);

        var target = type3Message.GetDomain() ?? _credentials.Domain;
        var user = type3Message.GetUser() ?? _credentials.UserName;
        var ntowfv2 = Responses.Ntlmv2Hash(target, user, _credentials.Password);
        var challengeAndTemp = new byte[_serverChallenge.Length + temp.Length];
        Array.Copy(_serverChallenge, 0, challengeAndTemp, 0, _serverChallenge.Length);
        Array.Copy(temp, 0, challengeAndTemp, _serverChallenge.Length, temp.Length);
        var expectedNtProofStr = Responses.HmacMD5(challengeAndTemp, ntowfv2);
        if (!CryptographicOperations.FixedTimeEquals(expectedNtProofStr, ntProofStr)) {
            throw new SecurityException("Invalid NTLMv2 NT proof string.");
        }

        var sessionBaseKey = Responses.HmacMD5(ntProofStr, ntowfv2);
        if (!type3Message.GetFlag(NtlmFlags.NtlmsspNegotiateKeyExch)) {
            return sessionBaseKey;
        }

        var encryptedRandomSessionKey = type3Message.GetSessionKey();
        if (encryptedRandomSessionKey == null || encryptedRandomSessionKey.Length == 0) {
            throw new SecurityException("NTLMv2 key exchange was negotiated without an encrypted random session key.");
        }
        return ntlmKeyFactory.DecryptSecondarySessionKey(encryptedRandomSessionKey, sessionBaseKey);
    }

    internal void SetNegotiateMessage(ReadOnlySpan<byte> negotiateMessage) =>
        _negotiateMessage = negotiateMessage.ToArray();

    internal void SetChallengeMessage(ReadOnlySpan<byte> challengeMessage) =>
        _challengeMessage = challengeMessage.ToArray();

    private void AddMicIfRequired(Type2Message type2, Type3Message type3, byte[]? exportedSessionKey) {
        var targetInformation = type2.GetTargetInformation();
        if (!RequiresMic(type2.GetFlags(), targetInformation)) {
            return;
        }
        if (exportedSessionKey == null || exportedSessionKey.Length == 0) {
            throw new SecurityException("NTLMv2 MIC requires an exported session key.");
        }
        if (_negotiateMessage == null || _negotiateMessage.Length == 0) {
            throw new SecurityException("NTLMv2 MIC requires the original NEGOTIATE message.");
        }

        var challengeMessage = _challengeMessage ?? type2.ToByteArray();
        type3.ToByteArrayWithMic(exportedSessionKey, _negotiateMessage, challengeMessage);
    }

    private void VerifyMicIfRequired(Type3Message type3Message, byte[] exportedSessionKey, byte[]? authenticateMessage) {
        if (!RequiresMic(_challengeFlags, _challengeTargetInformation ?? Array.Empty<byte>())) {
            return;
        }
        if (exportedSessionKey == null || exportedSessionKey.Length == 0) {
            throw new SecurityException("NTLMv2 MIC verification requires an exported session key.");
        }
        if (_negotiateMessage == null || _negotiateMessage.Length == 0 ||
            _challengeMessage == null || _challengeMessage.Length == 0) {
            throw new SecurityException("NTLMv2 MIC verification requires the original NEGOTIATE and CHALLENGE messages.");
        }

        var authenticate = authenticateMessage ?? type3Message.ToByteArray();
        if (!type3Message.HasMic ||
            !NtlmMic.Verify(exportedSessionKey, _negotiateMessage, _challengeMessage, authenticate, Type3Message.MicOffset)) {
            throw new SecurityException("Invalid NTLMv2 MIC.");
        }
    }

    private static bool RequiresMic(NtlmFlags flags, ReadOnlySpan<byte> targetInformation) =>
        ShouldRequestMic(flags) && NtlmAvPairs.HasMicFlag(targetInformation);

    private static bool ShouldRequestMic(NtlmFlags flags) =>
        (flags & (NtlmFlags.NtlmsspNegotiateKeyExch | NtlmFlags.NtlmsspNegotiateVersion)) ==
        (NtlmFlags.NtlmsspNegotiateKeyExch | NtlmFlags.NtlmsspNegotiateVersion);

    private static readonly byte[] kDefaultServerChallenge = { 1, 2, 3, 4, 5, 6, 7, 8 };
    private static readonly bool kUnicodeSupported = Config.GetBoolean("SharpCifs.smb.client.useUnicode", true);
    private static readonly NtlmFlags kBASICFLAGS =
        NtlmFlags.NtlmsspRequestTarget | NtlmFlags.NtlmsspNegotiateNtlm |
        NtlmFlags.NtlmsspNegotiateOem | NtlmFlags.NtlmsspNegotiateAlwaysSign |
        (kUnicodeSupported ? NtlmFlags.NtlmsspNegotiateUnicode : NtlmFlags.None);

    private byte[] ApplyChannelBindings(byte[] targetInformation) {
        if (_channelBindingsHash == null || _channelBindingsHash.Length == 0) {
            return targetInformation;
        }

        return AddOrReplaceAvPair(targetInformation, MsvAvChannelBindings, _channelBindingsHash);
    }

    private void ValidateChannelBindingsInNtChallengeResponse(byte[] temp) {
        if (_channelBindingsHash == null || _channelBindingsHash.Length == 0) {
            return;
        }

        const int avPairsOffset = 28;
        if (temp.Length < avPairsOffset ||
            !TryGetAvPair(temp.AsSpan(avPairsOffset), MsvAvChannelBindings, out var actualChannelBindings) ||
            actualChannelBindings.Length != _channelBindingsHash.Length ||
            !CryptographicOperations.FixedTimeEquals(actualChannelBindings, _channelBindingsHash)) {
            throw new SecurityException("NTLMv2 channel bindings did not match the TLS endpoint.");
        }
    }

    private static byte[] CloneChannelBindingsHash(object value) {
        if (value == null) {
            return null;
        }

        if (value is byte[] bytes) {
            if (bytes.Length != 16) {
                throw new ArgumentException("NTLM channel bindings hash must be exactly 16 bytes.", nameof(value));
            }

            return (byte[])bytes.Clone();
        }

        throw new ArgumentException("NTLM channel bindings hash must be a byte array.", nameof(value));
    }

    private static byte[] AddOrReplaceAvPair(byte[] targetInformation, ushort avId, byte[] value) {
        var source = targetInformation ?? Array.Empty<byte>();
        using var output = new MemoryStream(source.Length + 4 + value.Length);
        var offset = 0;
        var wroteChannelBindings = false;

        while (offset + 4 <= source.Length) {
            var currentAvId = BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(offset, sizeof(ushort)));
            var length = BinaryPrimitives.ReadUInt16LittleEndian(source.AsSpan(offset + sizeof(ushort), sizeof(ushort)));
            offset += 4;
            if (length > source.Length - offset) {
                throw new ArgumentException("NTLM target information AV_PAIR length is invalid.", nameof(targetInformation));
            }

            if (currentAvId == MsvAvEol) {
                if (!wroteChannelBindings) {
                    WriteAvPair(output, avId, value);
                }

                WriteAvPair(output, MsvAvEol, Array.Empty<byte>());
                return output.ToArray();
            }

            if (currentAvId == avId) {
                WriteAvPair(output, avId, value);
                wroteChannelBindings = true;
            }
            else {
                WriteAvPair(output, currentAvId, source.AsSpan(offset, length));
            }

            offset += length;
        }

        if (!wroteChannelBindings) {
            WriteAvPair(output, avId, value);
        }
        WriteAvPair(output, MsvAvEol, Array.Empty<byte>());
        return output.ToArray();
    }

    private static bool TryGetAvPair(ReadOnlySpan<byte> targetInformation, ushort avId, out ReadOnlySpan<byte> value) {
        var offset = 0;
        while (offset + 4 <= targetInformation.Length) {
            var currentAvId = BinaryPrimitives.ReadUInt16LittleEndian(targetInformation.Slice(offset, sizeof(ushort)));
            var length = BinaryPrimitives.ReadUInt16LittleEndian(targetInformation.Slice(offset + sizeof(ushort), sizeof(ushort)));
            offset += 4;
            if (length > targetInformation.Length - offset) {
                break;
            }

            if (currentAvId == avId) {
                value = targetInformation.Slice(offset, length);
                return true;
            }

            if (currentAvId == MsvAvEol) {
                break;
            }

            offset += length;
        }

        value = ReadOnlySpan<byte>.Empty;
        return false;
    }

    private static void WriteAvPair(Stream output, ushort avId, ReadOnlySpan<byte> value) {
        Span<byte> header = stackalloc byte[4];
        BinaryPrimitives.WriteUInt16LittleEndian(header, avId);
        BinaryPrimitives.WriteUInt16LittleEndian(header[sizeof(ushort)..], checked((ushort)value.Length));
        output.Write(header);
        output.Write(value);
    }

    private static bool GetBooleanProperty(PropertyBag properties, string name, bool defaultValue) {
        var value = properties.GetProperty(name);
        return value == null ? defaultValue : Convert.ToBoolean(value);
    }

    private readonly NetworkCredential _credentials;
    //  private AuthenticationSource _authenticationSource;
#pragma warning disable IDE0052 // Remove unread private members
    private readonly PropertyBag _properties;
#pragma warning restore IDE0052 // Remove unread private members
    private readonly bool _lanManagerKey;
    private readonly bool _seal;
    private readonly bool _sign;
    private readonly bool _keyExchange;
    // we always go for 128
    private readonly int _keyLength = 128;
    private readonly bool _useNtlm2sessionsecurity;
    private readonly bool _useNtlmV2;
    private readonly bool _allowNtlmV1;
    private readonly bool _useSSO;
    private readonly byte[] _channelBindingsHash;
    private byte[] _serverChallenge;
    private NtlmFlags _challengeFlags;
    private byte[]? _challengeTargetInformation;
    private byte[]? _negotiateMessage;
    private byte[]? _challengeMessage;
    private const ushort MsvAvEol = 0x0000;
    private const ushort MsvAvChannelBindings = 0x000A;
    private static readonly Random kRandomGen = new Random();
}
