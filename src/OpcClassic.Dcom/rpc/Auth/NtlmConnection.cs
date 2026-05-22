//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc.Auth.ntlm {
    using SharpInterop.Rpc.Core;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Ntlmssp;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.IO;

    /// <summary>
    /// Connection
    /// </summary>
    public class NtlmConnection : DefaultConnection {

        /// <summary>
        /// Create connection
        /// </summary>
        /// <param name="properties"></param>
        public NtlmConnection(Properties properties) {
            _authentication = new NtlmAuthentication(properties);
            _properties = properties;
        }

        /// <summary>
        /// Set transmit length
        /// </summary>
        public int TransmitLength {
            set => _transmitBuffer = new NdrBuffer(new byte[value], 0);
            get => _transmitBuffer.Length;
        }

        /// <summary>
        /// Set receive length
        /// </summary>
        public int ReceiveLength {
            set => _receiveBuffer = new NdrBuffer(new byte[value], 0);
            get => _receiveBuffer.Length;
        }

        /// <inheritdoc/>
        protected internal override void IncomingRebind(AuthenticationVerifier verifier) {
            switch (verifier.Body[8]) {
                case 1:
                    // server gets negotiate from client
                    // setSecurity(null);
                    _contextId = verifier.ContextId;
                    _ntlm = new Type1Message(verifier.Body);
                    break;
                case 2:
                    // client gets challenge from server
                    _ntlm = new Type2Message(verifier.Body);
                    break;
                case 3:
                    // server gets authenticate from client
                    _ntlm = new Type3Message(verifier.Body);
                    if (UseNtlm2SessionSecurity()) {
                        _authentication.CreateSecurityWhenServer(_ntlm);
                        _security = _authentication.Security;
                    }
                    break;
                default:
                    throw new IOException("Invalid NTLM message type.");
            }
        }

        /// <inheritdoc/>
        protected internal override AuthenticationVerifier OutgoingRebind() {
            if (_ntlm == null) {
                // client sends negotiate to server
                //  setSecurity(null);
                lock (typeof(NtlmConnection)) {
                    _contextId = ++_contextSerial;
                }
                _ntlm = _authentication.CreateType1();
            }
            else if (_ntlm is Type1Message) {
                // server sends challenge to client
                _ntlm = _authentication.CreateType2((Type1Message)_ntlm);
            }
            else if (_ntlm is Type2Message type2) // client sends authenticate to server
{
                _ntlm = _authentication.CreateType3(type2);
                if (UseNtlm2SessionSecurity()) {
                    _security = _authentication.Security;
                }
            }
            else if (_ntlm is Type3Message) {
                // this simply means that we have sent the response to the challenge
                // now is the time to send the Auth Context only
                //             return new AuthenticationVerifier(
                //                     NtlmAuthentication.AUTHENTICATION_SERVICE_NTLM,Security.PROTECTION_LEVEL_CONNECT,
                //                             contextId, new byte[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0});
                return null;
            }
            else {
                throw new IOException("Unrecognized NTLM message.");
            }
            var protectionLevel = _ntlm.GetFlag(NtlmFlags.NtlmsspNegotiateSeal) ?
                ProtectionLevel.PROTECTION_LEVEL_PRIVACY :
                    _ntlm.GetFlag(NtlmFlags.NtlmsspNegotiateSign) ?
                        ProtectionLevel.PROTECTION_LEVEL_INTEGRITY :
                        ProtectionLevel.PROTECTION_LEVEL_CONNECT;
            return new AuthenticationVerifier(NtlmAuthentication.AUTHENTICATIONSERVICENTLM,
                protectionLevel, _contextId, _ntlm.ToByteArray());
        }

        private bool UseNtlm2SessionSecurity() {
            var value = _properties.GetProperty("rpc.ntlm.ntlm2");
            return value == null || Convert.ToBoolean(value);
        }

        private static int _contextSerial;
        private readonly Properties _properties;
        private readonly NtlmAuthentication _authentication;
        private NtlmMessage _ntlm;
    }

}