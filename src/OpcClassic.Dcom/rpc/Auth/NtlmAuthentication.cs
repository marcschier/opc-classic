//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc.Auth.ntlm {
    using SharpCifs.Ntlmssp;
    using SharpCifs.Smb;
    using SharpCifs.Util;
    using System;
    using SharpCifs;
    using SharpCifs.Util.Sharpen;
    using System.IO;

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
        public NtlmAuthentication(Properties properties) {
            _properties = properties;
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
                        _keyLength = int.Parse(keyLength);
                    }
                    catch (FormatException) {
                        throw new ArgumentException("Invalid key length: " + keyLength);
                    }
                }

                _useNtlm2sessionsecurity = Convert.ToBoolean(properties.GetProperty("rpc.ntlm.ntlm2"));
                _useNtlmV2 = Convert.ToBoolean(properties.GetProperty("rpc.ntlm.ntlmv2"));
                _useSSO = Convert.ToBoolean(properties.GetProperty("rpc.ntlm.sso"));
                domain = (string)properties.GetProperty("rpc.ntlm.domain");
                user = (string)properties.GetProperty(SharpInterop.Rpc.Security.USERNAME);
                password = (string)properties.GetProperty(SharpInterop.Rpc.Security.PASSWORD);
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
                    "Kerberos/SPNEGO support in OpcClassic.Dcom.Kerberos (Phase 3D).");
            }
            _credentials = new NtlmPasswordAuthentication(domain, user, password);
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
            return new Type1Message(flags, _credentials.GetDomain(), Type1Message.GetDefaultWorkstation());
        }

        /// <summary>
        /// Create type 2 message
        /// </summary>
        /// <param name="type1"></param>
        /// <exception cref="IOException"></exception>
        /// <returns></returns>
        public Type2Message CreateType2(Type1Message type1) {
            int flags;
            if (type1 == null) {
                flags = DefaultFlags;
            }
            else {
                flags = AdjustFlags(type1.GetFlags());
            }
            flags |= 0x00020000; // challenge accept response flag
            var type2Message = new Type2Message(flags, new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
                _credentials.GetDomain()); // generate our own, since SMB will throw exception here
            return type2Message;
        }

        /// <summary>
        /// Create type 3 message
        /// </summary>
        /// <param name="type2"></param>
        /// <exception cref="IOException"></exception>
        /// <returns></returns>
        public Type3Message CreateType3(Type2Message type2) {
            if (_useSSO) {
                // Unreachable — constructor already throws for _useSSO. Defensive guard.
                throw new PlatformNotSupportedException(
                    "NTLM SSO is unsupported on net10; use Kerberos via Phase 3D.");
            }
            else {
                var flags = type2.GetFlags();
                if ((flags & NtlmFlags.NtlmsspNegotiateDatagramStyle) != 0) {
                    flags = AdjustFlags(flags);
                    flags &= ~0x00020000;
                }

                var clientNonce = new byte[8];
                byte[] blob = null;
                string target = null; // getTargetFromTargetInformation(type2.GetTargetInformation());

                if (target == null) {
                    target = _credentials.GetDomain().ToUpper();
                    if (target.Equals("")) {
                        target = GetTargetFromTargetInformation(type2.GetTargetInformation());
                    }
                }

                Type3Message type3;
                if (_useNtlmV2) {
                    kRandomGen.NextBytes(clientNonce);
                    try {
                        var lmv2Response = Responses.GetLMv2Response(target,
                            _credentials.GetUsername(), _credentials.GetPassword(), type2.GetChallenge(), clientNonce);
                        var retval = Responses.GetNTLMv2Response(target,
                            _credentials.GetUsername(), _credentials.GetPassword(), type2.GetTargetInformation(),
                            type2.GetChallenge(), clientNonce);
                        var ntlmv2Response = retval[0];
                        blob = retval[1];
                        type3 = new Type3Message(flags, lmv2Response, ntlmv2Response,
                            target, _credentials.GetUsername(), Type3Message.GetDefaultWorkstation());
                    }
                    catch (Exception e) {
                        throw new Exception("Exception occured while forming NTLMv2 Type3Response", e);
                    }

                }
                else {
                    if ((flags & NtlmFlags.NtlmsspNegotiateNtlm2) != 0) // NTLM2 Session security response
                    {
                        flags = AdjustFlags(flags);
                        flags &= ~0x00020000;
                        // flags =  0xe2888235;
                        var challenge = type2.GetChallenge();
                        // LMReponse is 24 bytes. 8 byte random client nonce and the rest is null padded.
                        var lmResponse = new byte[24];

                        kRandomGen.NextBytes(clientNonce);
                        Array.Copy(clientNonce, 0, lmResponse, 0, clientNonce.Length);
                        byte[] ntResponse;
                        try {
                            ntResponse = Responses.GetNTLM2SessionResponse(_credentials.GetPassword(), challenge, clientNonce);
                        }
                        catch (Exception e) {
                            throw new Exception("Exception occured while forming Session Security Type3Response", e);
                        }

                        type3 = new Type3Message(flags, lmResponse, ntResponse, target,
                            _credentials.GetUsername(), Type3Message.GetDefaultWorkstation());
                    }
                    else // Plain NTLMv1 response
                    {
                        var challenge = type2.GetChallenge();
                        var lmResponse = NtlmPasswordAuthentication
                            .GetPreNtlmResponse(_credentials.GetPassword(), challenge);
                        var ntResponse = NtlmPasswordAuthentication
                            .GetNtlmResponse(_credentials.GetPassword(), challenge);
                        type3 = new Type3Message(flags, lmResponse, ntResponse, target,
                            _credentials.GetUsername(), Type3Message.GetDefaultWorkstation());
                        if ((flags & NtlmFlags.NtlmsspNegotiateKeyExch) != 0) {
                            throw new Exception("Key Exchange not supported by Library !");
                        }
                    }
                }
                // we have to now form lmv2 and ntlmv2 response with regards to the session security
                // the type3message also has to be altered
                if (_useNtlm2sessionsecurity && (flags & NtlmFlags.NtlmsspNegotiateNtlm2) != 0) {
                    var ntlmKeyFactory = new NTLMKeyFactory();
                    byte[] userSessionKey;
                    if (_useNtlmV2) {
                        try {
                            userSessionKey = ntlmKeyFactory
                                .GetNTLMv2UserSessionKey(target, _credentials.GetUsername(), _credentials.GetPassword(), type2.GetChallenge(), blob);
                        }
                        catch (Exception e) {
                            throw new Exception("Exception occured while forming NTLMv2 with NTLM2 Session Security for Type3Response", e);
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
                                .GetNTLM2SessionResponseUserSessionKey(_credentials.GetPassword(), servernonce);
                        }
                        catch (Exception e) {
                            throw new Exception("Exception occured while forming Session Security for Type3Response", e);
                        }

                    }
                    try {
                        // now RC4 encrypt a random 16 byte key
                        var secondayMasterKey = ntlmKeyFactory.SecondarySessionKey;
                        type3.SetSessionKey(ntlmKeyFactory.EncryptSecondarySessionKey(secondayMasterKey, userSessionKey));
                        Security = new Ntlm1(flags, secondayMasterKey, false);
                    }
                    catch (Exception e) {
                        throw new Exception("Exception occured while forming Session Security for Type3Response", e);
                    }
                }

                return type3;
            }
        }

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

        private int DefaultFlags {
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
                    flags |= NtlmFlags.NtlmsspNegotiateKeyExch;
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

        private int AdjustFlags(int flags) {
            if (kUnicodeSupported && ((flags & NtlmFlags.NtlmsspNegotiateUnicode) != 0)) {
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
                switch (Encdec.Dec_uint16le(targetInformation, i)) {
                    case 1: // Server name
                        i++;
                        i++; // advance two bytes
                        int length = Encdec.Dec_uint16le(targetInformation, i);
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
                        length = Encdec.Dec_uint16le(targetInformation, i);
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
        internal void CreateSecurityWhenServer(NtlmMessage type3) {
            var type3Message = (Type3Message)type3;
            // two things here...check for anonymous, in that case the user response key is new byte[16].
            // in case anonymous has not been sent then create the key using credentials.
            var flags = type3Message.GetFlags();
            var ntlmKeyFactory = new NTLMKeyFactory();
            byte[] secondayMasterKey;
            byte[] sessionResponseUserSessionKey = null;
            if (type3Message.GetFlag(0x00000800)) // anonymous flag
            {
                // if it is anonymous the user session key is new byte[16];
                sessionResponseUserSessionKey = new byte[16];
            }
            else if (_useNtlmV2) {
                // TODO this needs to be checked here since the key logic will be totally different
                // and we have to get the key out of Type3 message response (blob of the NTLMv2 response.)
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
                        .GetNTLM2SessionResponseUserSessionKey(_credentials.GetPassword(), servernonce);
                }
                catch (Exception e) {
                    throw new Exception("Exception occured while forming Session Security from Type3 AUTH", e);
                }
            }

            try {
                // now RC4 decrypt the session key
                secondayMasterKey = ntlmKeyFactory
                    .DecryptSecondarySessionKey(type3Message.GetSessionKey(), sessionResponseUserSessionKey);
                Security = new Ntlm1(flags, secondayMasterKey, true);
            }
            catch (Exception e) {
                throw new Exception("Exception occured while forming Session Security Type3Response", e);
            }
        }

        private static readonly bool kUnicodeSupported = Config.GetBoolean("SharpCifs.smb.client.useUnicode", true);
        private static readonly int kBASICFLAGS =
            NtlmFlags.NtlmsspRequestTarget | NtlmFlags.NtlmsspNegotiateNtlm |
            NtlmFlags.NtlmsspNegotiateOem | NtlmFlags.NtlmsspNegotiateAlwaysSign |
            (kUnicodeSupported ? NtlmFlags.NtlmsspNegotiateUnicode : 0);
        private readonly NtlmPasswordAuthentication _credentials;
        //  private AuthenticationSource _authenticationSource;
#pragma warning disable IDE0052 // Remove unread private members
        private readonly Properties _properties;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly bool _lanManagerKey;
        private readonly bool _seal;
        private readonly bool _sign;
        private readonly bool _keyExchange;
        // we always go for 128
        private readonly int _keyLength = 128;
        private readonly bool _useNtlm2sessionsecurity;
        private readonly bool _useNtlmV2;
        private readonly bool _useSSO;
        private static readonly Random kRandomGen = new Random();
    }
}