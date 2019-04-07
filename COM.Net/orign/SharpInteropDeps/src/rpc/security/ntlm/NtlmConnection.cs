using System;

/// <summary>
/// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
/// 
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v1.
/// 
/// </summary>

namespace rpc.security.ntlm {


	using NtlmFlags = jcifs.ntlmssp.NtlmFlags;
	using NtlmMessage = jcifs.ntlmssp.NtlmMessage;
	using Type1Message = jcifs.ntlmssp.Type1Message;
	using Type2Message = jcifs.ntlmssp.Type2Message;
	using Type3Message = jcifs.ntlmssp.Type3Message;
	using NdrBuffer = ndr.NdrBuffer;
	using AuthenticationVerifier = rpc.core.AuthenticationVerifier;

	public class NtlmConnection : DefaultConnection {

		private static int ContextSerial;

		private NtlmAuthentication Authentication;

		protected internal Properties Properties;

		private NtlmMessage Ntlm;

		public NtlmConnection(Properties properties) {
			this.Authentication = new NtlmAuthentication(properties);
			this.Properties = properties;
		}

		public virtual int TransmitLength {
			set {
				TransmitBuffer = new NdrBuffer(new sbyte[value], 0);
			}
		}

		public virtual int ReceiveLength {
			set {
				ReceiveBuffer = new NdrBuffer(new sbyte[value], 0);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void incomingRebind(rpc.core.AuthenticationVerifier verifier) throws java.io.IOException
		public override void IncomingRebind(AuthenticationVerifier verifier) {
			switch (verifier.Body[8]) {
			case 1:
				// server gets negotiate from client
				//setSecurity(null);
				ContextId = verifier.ContextId;
				Ntlm = new Type1Message(verifier.Body);
				break;
			case 2:
				// client gets challenge from server
				Ntlm = new Type2Message(verifier.Body);
				break;
			case 3:
				// server gets authenticate from client
				Type2Message type2 = (Type2Message) Ntlm;
				Ntlm = new Type3Message(verifier.Body);
				bool usentlmv2 = (bool)Convert.ToBoolean(Properties.getProperty("rpc.ntlm.ntlm2"));
				if (usentlmv2) {
					Authentication.CreateSecurityWhenServer(Ntlm);
					Security = Authentication.Security;
				}
				break;
			default:
				throw new IOException("Invalid NTLM message type.");
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected rpc.core.AuthenticationVerifier outgoingRebind() throws java.io.IOException
		public override AuthenticationVerifier OutgoingRebind() {
			if (Ntlm == null) {
				// client sends negotiate to server
			  //  setSecurity(null);
				lock (typeof(NtlmConnection)) {
					ContextId = ++ContextSerial;
				}
				Ntlm = Authentication.CreateType1();
			}
			else if (Ntlm is Type1Message) {
				// server sends challenge to client
				Ntlm = Authentication.CreateType2((Type1Message) Ntlm);
			}
			else if (Ntlm is Type2Message) {
				// client sends authenticate to server
				Type2Message type2 = (Type2Message) Ntlm;
				Ntlm = Authentication.CreateType3(type2);
				bool usentlmv2 = (bool)Convert.ToBoolean(Properties.getProperty("rpc.ntlm.ntlm2"));
				if (usentlmv2) {
					Security = Authentication.Security;
				}
			}
			else if (Ntlm is Type3Message) //this simply means that we have sent the response to the challenge
			{ //now is the time to send the Auth Context only
	//        	 return new AuthenticationVerifier(
	//                     NtlmAuthentication.AUTHENTICATION_SERVICE_NTLM,Security.PROTECTION_LEVEL_CONNECT,
	//                             contextId, new byte[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0});
				return null;
			}
			else {
				throw new IOException("Unrecognized NTLM message.");
			}
			int protectionLevel = Ntlm.getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_SEAL) ? rpc.Security_Fields.PROTECTION_LEVEL_PRIVACY : Ntlm.getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_SIGN) ? rpc.Security_Fields.PROTECTION_LEVEL_INTEGRITY : rpc.Security_Fields.PROTECTION_LEVEL_CONNECT;
				   return new AuthenticationVerifier(NtlmAuthentication.AUTHENTICATION_SERVICE_NTLM, protectionLevel, ContextId, Ntlm.toByteArray());
		}

	}

}