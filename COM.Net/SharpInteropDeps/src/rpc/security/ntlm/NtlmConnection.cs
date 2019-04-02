using System;

// 
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
// 
// j-Interop (Pure Java implementation of DCOM protocol)
// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace rpc.security.ntlm
{


	using NtlmFlags = jcifs.ntlmssp.NtlmFlags;
	using NtlmMessage = jcifs.ntlmssp.NtlmMessage;
	using Type1Message = jcifs.ntlmssp.Type1Message;
	using Type2Message = jcifs.ntlmssp.Type2Message;
	using Type3Message = jcifs.ntlmssp.Type3Message;
	using NdrBuffer = ndr.NdrBuffer;
	using AuthenticationVerifier = core.AuthenticationVerifier;

	public class NtlmConnection : DefaultConnection
	{

		private static int contextSerial;

		private NtlmAuthentication authentication;

		protected internal Properties properties;

		private NtlmMessage ntlm;

		public NtlmConnection(Properties properties)
		{
			authentication = new NtlmAuthentication(properties);
			this.properties = properties;
		}

		public virtual int TransmitLength {
            set => transmitBuffer = new NdrBuffer(new sbyte[value], 0);
        }

        public virtual int ReceiveLength {
            set => receiveBuffer = new NdrBuffer(new sbyte[value], 0);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: protected void incomingRebind(rpc.core.AuthenticationVerifier verifier) throws java.io.IOException
        protected internal override void incomingRebind(AuthenticationVerifier verifier)
		{
			switch (verifier.body[8])
			{
			case 1:
				// server gets negotiate from client
				//setSecurity(null);
				contextId = verifier.contextId;
				ntlm = new Type1Message(verifier.body);
				break;
			case 2:
				// client gets challenge from server
				ntlm = new Type2Message(verifier.body);
				break;
			case 3:
				// server gets authenticate from client
				var type2 = (Type2Message) ntlm;
				ntlm = new Type3Message(verifier.body);
				var usentlmv2 = (bool)Convert.ToBoolean(properties.getProperty("rpc.ntlm.ntlm2"));
				if (usentlmv2)
				{
					authentication.createSecurityWhenServer(ntlm);
					Security = authentication.Security;
				}
				break;
			default:
				throw new IOException("Invalid NTLM message type.");
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected rpc.core.AuthenticationVerifier outgoingRebind() throws java.io.IOException
		protected internal override AuthenticationVerifier outgoingRebind()
		{
			if (ntlm == null)
			{
				// client sends negotiate to server
			  //  setSecurity(null);
				lock (typeof(NtlmConnection))
				{
					contextId = ++contextSerial;
				}
				ntlm = authentication.createType1();
			}
			else if (ntlm is Type1Message)
			{
				// server sends challenge to client
				ntlm = authentication.createType2((Type1Message) ntlm);
			}
			else if (ntlm is Type2Message)
			{
				// client sends authenticate to server
				var type2 = (Type2Message) ntlm;
				ntlm = authentication.createType3(type2);
				var usentlmv2 = (bool)Convert.ToBoolean(properties.getProperty("rpc.ntlm.ntlm2"));
				if (usentlmv2)
				{
					Security = authentication.Security;
				}
			}
			else if (ntlm is Type3Message) //this simply means that we have sent the response to the challenge
			{ //now is the time to send the Auth Context only
	//        	 return new AuthenticationVerifier(
	//                     NtlmAuthentication.AUTHENTICATION_SERVICE_NTLM,Security.PROTECTION_LEVEL_CONNECT,
	//                             contextId, new byte[]{1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0});
				return null;
			}
			else
			{
				throw new IOException("Unrecognized NTLM message.");
			}
			var protectionLevel = ntlm.getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_SEAL) ? Security_Fields.PROTECTION_LEVEL_PRIVACY : ntlm.getFlag(NtlmFlags.NTLMSSP_NEGOTIATE_SIGN) ? Security_Fields.PROTECTION_LEVEL_INTEGRITY : Security_Fields.PROTECTION_LEVEL_CONNECT;
				   return new AuthenticationVerifier(NtlmAuthentication.AUTHENTICATION_SERVICE_NTLM, protectionLevel, contextId, ntlm.toByteArray());
		}

	}

}