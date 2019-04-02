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


	using Config = jcifs.Config;
	using NtlmFlags = jcifs.ntlmssp.NtlmFlags;
	using NtlmMessage = jcifs.ntlmssp.NtlmMessage;
	using Type1Message = jcifs.ntlmssp.Type1Message;
	using Type2Message = jcifs.ntlmssp.Type2Message;
	using Type3Message = jcifs.ntlmssp.Type3Message;
	using NtlmPasswordAuthentication = jcifs.smb.NtlmPasswordAuthentication;
	using Encdec = jcifs.util.Encdec;
	using SSPIJNIClient = net.sourceforge.jtds.util.SSPIJNIClient;

	public class NtlmAuthentication
	{

		public const int AUTHENTICATION_SERVICE_NTLM = 10;



		private static readonly bool UNICODE_SUPPORTED = Config.getBoolean("jcifs.smb.client.useUnicode", true);


		private static readonly int BASIC_FLAGS = NtlmFlags.NTLMSSP_REQUEST_TARGET | NtlmFlags.NTLMSSP_NEGOTIATE_NTLM | NtlmFlags.NTLMSSP_NEGOTIATE_OEM | NtlmFlags.NTLMSSP_NEGOTIATE_ALWAYS_SIGN | (UNICODE_SUPPORTED ? NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE : 0);


		private Security security;

		protected internal Properties properties;

		private NtlmPasswordAuthentication credentials;

		private AuthenticationSource authenticationSource;

		private readonly bool lanManagerKey;

		private readonly bool seal;

		private readonly bool sign;

		private readonly bool keyExchange;

		//we always go for 128
		private readonly int keyLength = 128;

		private readonly bool useNtlm2sessionsecurity;

		private readonly bool useNtlmV2;

		private readonly bool useSSO;

		private static readonly Random RANDOM = new Random();

		private readonly SSPIJNIClient jniClient;

		public NtlmAuthentication(Properties properties)
		{
			this.properties = properties;
			string domain = null;
			string user = null;
			string password = null;
			if (properties != null)
			{
				lanManagerKey = (bool)Convert.ToBoolean(properties.getProperty("rpc.ntlm.lanManagerKey"));
				seal = (bool)Convert.ToBoolean(properties.getProperty("rpc.ntlm.seal"));
				sign = seal ? true : (bool)Convert.ToBoolean(properties.getProperty("rpc.ntlm.sign"));
				keyExchange = (bool)Convert.ToBoolean(properties.getProperty("rpc.ntlm.keyExchange"));
				string keyLength = properties.getProperty("rpc.ntlm.keyLength");
				if (keyLength != null)
				{
					try
					{
						this.keyLength = int.Parse(keyLength);
					}
					catch (FormatException)
					{
						throw new ArgumentException("Invalid key length: " + keyLength);
					}
				}

				useNtlm2sessionsecurity = (bool)Convert.ToBoolean(properties.getProperty("rpc.ntlm.ntlm2"));
				useNtlmV2 = (bool)Convert.ToBoolean(properties.getProperty("rpc.ntlm.ntlmv2"));
				useSSO = (bool)Convert.ToBoolean(properties.getProperty("rpc.ntlm.sso"));
				domain = properties.getProperty("rpc.ntlm.domain");
				user = properties.getProperty(Security_Fields.USERNAME);
				password = properties.getProperty(Security_Fields.PASSWORD);
			}

			if (useSSO)
			{
				jniClient = SSPIJNIClient.Instance;
			}
			else
			{
				jniClient = null;
				credentials = new NtlmPasswordAuthentication(domain, user, password);
			}


		}

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public rpc.Security getSecurity() throws java.io.IOException
        public virtual Security Security => security;

        protected internal virtual AuthenticationSource AuthenticationSource
		{
			get
			{
				if (authenticationSource != null)
				{
					return authenticationSource;
				}
				string sourceClass = (properties != null) ? properties.getProperty("rpc.ntlm.authenticationSource") : null;
				if (sourceClass == null)
				{
					return authenticationSource = AuthenticationSource.DefaultInstance;
				}
				try
				{
					return authenticationSource = (AuthenticationSource) Type.GetType(sourceClass).newInstance();
				}
				catch (Exception ex)
				{
					throw new ArgumentException("Invalid authentication source: " + ex);
				}
			}
		}

		private int DefaultFlags
		{
			get
			{
				var flags = BASIC_FLAGS;
				if (lanManagerKey)
				{
					flags |= NtlmFlags.NTLMSSP_NEGOTIATE_LM_KEY;
				}
				if (sign)
				{
					flags |= NtlmFlags.NTLMSSP_NEGOTIATE_SIGN;
				}
				if (seal)
				{
					flags |= NtlmFlags.NTLMSSP_NEGOTIATE_SEAL;
				}
				if (keyExchange)
				{
					flags |= NtlmFlags.NTLMSSP_NEGOTIATE_KEY_EXCH;
				}
				if (keyLength >= 56)
				{
					flags |= NtlmFlags.NTLMSSP_NEGOTIATE_56;
				}
				if (keyLength >= 128)
				{
					flags |= NtlmFlags.NTLMSSP_NEGOTIATE_128;
				}
				//We always negotiate for NTLM2 session security
		//        if (useNtlm2sessionsecurity)
				{
					flags |= NtlmFlags.NTLMSSP_NEGOTIATE_NTLM2;
				}
    
				return flags;
			}
		}

		private int adjustFlags(int flags)
		{
			if (UNICODE_SUPPORTED && ((flags & NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE) != 0))
			{
				flags &= ~NtlmFlags.NTLMSSP_NEGOTIATE_OEM;
				flags |= NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE;
			}
			else
			{
				flags &= ~NtlmFlags.NTLMSSP_NEGOTIATE_UNICODE;
				flags |= NtlmFlags.NTLMSSP_NEGOTIATE_OEM;
			}
			if (!lanManagerKey)
			{
				flags &= ~NtlmFlags.NTLMSSP_NEGOTIATE_LM_KEY;
			}
			if (!(sign || seal))
			{
				flags &= ~NtlmFlags.NTLMSSP_NEGOTIATE_SIGN;
			}
			if (!seal)
			{
				flags &= ~NtlmFlags.NTLMSSP_NEGOTIATE_SEAL;
			}
			if (!keyExchange)
			{
				flags &= ~NtlmFlags.NTLMSSP_NEGOTIATE_KEY_EXCH;
			}
			if (keyLength < 128)
			{
				flags &= ~NtlmFlags.NTLMSSP_NEGOTIATE_128;
			}
			if (keyLength < 56)
			{
				flags &= ~NtlmFlags.NTLMSSP_NEGOTIATE_56;
			}
	//        if (!useNtlm2sessionsecurity)
	//        {
	//        	flags &= ~NtlmFlags.NTLMSSP_NEGOTIATE_NTLM2;
	//        }
			return flags;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public jcifs.ntlmssp.Type1Message createType1() throws java.io.IOException
		public virtual Type1Message createType1()
		{

            if (useSSO) {
                var ntlmMessage = jniClient.invokePrepareSSORequest();
                var type1Message = new Type1Message(ntlmMessage) {
                    Flags = DefaultFlags
                };
                return type1Message;
            }
            var flags = DefaultFlags;
            return new Type1Message(flags, credentials.Domain, Type1Message.DefaultWorkstation);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public jcifs.ntlmssp.Type2Message createType2(jcifs.ntlmssp.Type1Message type1) throws java.io.IOException
		public virtual Type2Message createType2(Type1Message type1)
		{
			int flags;
			if (type1 == null)
			{
				flags = DefaultFlags;
			}
			else
			{
				flags = adjustFlags(type1.Flags);
			}
			flags |= 0x00020000; //challenge accept response flag

			var type2Message = new Type2Message(flags, new sbyte[]{1,2,3,4,5,6,7,8}, credentials.Domain); //generate our own, since SMB will throw exception here

			return type2Message;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public jcifs.ntlmssp.Type3Message createType3(jcifs.ntlmssp.Type2Message type2) throws java.io.IOException
		public virtual Type3Message createType3(Type2Message type2)
		{
			if (useSSO)
			{
				sbyte[] ntlmMessage = type2.toByteArray();
				var ret = jniClient.invokePrepareSSOSubmit(ntlmMessage);
				var message = new Type3Message(ret);
				int flags = type2.Flags;
				if ((flags & NtlmFlags.NTLMSSP_NEGOTIATE_DATAGRAM_STYLE) != 0)
				{
					flags = adjustFlags(flags);
					flags &= ~0x00020000;
				}
				message.Flags = flags;
				return message;
			}
			else
			{
				int flags = type2.Flags;
				if ((flags & NtlmFlags.NTLMSSP_NEGOTIATE_DATAGRAM_STYLE) != 0)
				{
					flags = adjustFlags(flags);
					flags &= ~0x00020000;
				}

				Type3Message type3 = null;

				var clientNonce = new sbyte[8];
				sbyte[] blob = null;

				string target = null; //getTargetFromTargetInformation(type2.getTargetInformation());

				if (target == null)
				{
					target = credentials.Domain.ToUpper();
					if (target.Equals(""))
					{
						target = getTargetFromTargetInformation(type2.TargetInformation);
					}
				}

				if (useNtlmV2)
				{
					RANDOM.NextBytes(clientNonce);
					try
					{
						var lmv2Response = Responses.getLMv2Response(target, credentials.Username, credentials.Password, type2.Challenge, clientNonce);
						var retval = Responses.getNTLMv2Response(target, credentials.Username, credentials.Password, type2.TargetInformation, type2.Challenge, clientNonce);
						var ntlmv2Response = retval[0];
						blob = retval[1];
						type3 = new Type3Message(flags, lmv2Response, ntlmv2Response, target, credentials.Username, Type3Message.DefaultWorkstation);
					}
					catch (Exception e)
					{
						throw new Exception("Exception occured while forming NTLMv2 Type3Response",e);
					}

				}
				else
				{
				if ((flags & NtlmFlags.NTLMSSP_NEGOTIATE_NTLM2) != 0) //NTLM2 Session security response
				{
					flags = adjustFlags(flags);
					flags &= ~0x00020000;
					//flags =  0xe2888235;
					sbyte[] challenge = type2.Challenge;
					//LMReponse is 24 bytes. 8 byte random client nonce and the rest is null padded.
					var lmResponse = new sbyte[24];

					RANDOM.NextBytes(clientNonce);
					Array.Copy(clientNonce, 0, lmResponse, 0, clientNonce.Length);
					sbyte[] ntResponse;
					try
					{
						ntResponse = Responses.getNTLM2SessionResponse(credentials.Password, challenge, clientNonce);
					}
					catch (Exception e)
					{
						throw new Exception("Exception occured while forming Session Security Type3Response",e);
					}

					type3 = new Type3Message(flags, lmResponse, ntResponse, target, credentials.Username, Type3Message.DefaultWorkstation);
				}
				else //Plain NTLMv1 response
				{
					sbyte[] challenge = type2.Challenge;
					sbyte[] lmResponse = NtlmPasswordAuthentication.getPreNTLMResponse(credentials.Password, challenge);
					sbyte[] ntResponse = NtlmPasswordAuthentication.getNTLMResponse(credentials.Password, challenge);
					type3 = new Type3Message(flags, lmResponse, ntResponse, target, credentials.Username, Type3Message.DefaultWorkstation);
					if ((flags & NtlmFlags.NTLMSSP_NEGOTIATE_KEY_EXCH) != 0)
					{
						throw new Exception("Key Exchange not supported by Library !");
					}
				}
				}
				//we have to now form lmv2 and ntlmv2 response with regards to the session security
				//the type3message also has to be altered
				if (useNtlm2sessionsecurity && (flags & NtlmFlags.NTLMSSP_NEGOTIATE_NTLM2) != 0)
				{
					var ntlmKeyFactory = new NTLMKeyFactory();
					sbyte[] userSessionKey;
					if (useNtlmV2)
					{
						try
						{
							userSessionKey = ntlmKeyFactory.getNTLMv2UserSessionKey(target, credentials.Username, credentials.Password, type2.Challenge, blob);
						}
						catch (Exception e)
						{
							throw new Exception("Exception occured while forming NTLMv2 with NTLM2 Session Security for Type3Response",e);
						}
					}
					else
					{
						//now create the key for the session
						//this key will be used to RC4 a 16 byte random key and set to the type3 message
						var servernonce = new sbyte[16];
						Array.Copy(type2.Challenge, 0, servernonce, 0, type2.Challenge.length);
						Array.Copy(clientNonce, 0, servernonce, 8, clientNonce.Length);
						try
						{
							userSessionKey = ntlmKeyFactory.getNTLM2SessionResponseUserSessionKey(credentials.Password, servernonce);
						}
						catch (Exception e)
						{
							throw new Exception("Exception occured while forming Session Security for Type3Response",e);
						}

					}

					try
					{
						//now RC4 encrypt a random 16 byte key
						var secondayMasterKey = ntlmKeyFactory.SecondarySessionKey;
						type3.SessionKey = ntlmKeyFactory.encryptSecondarySessionKey(secondayMasterKey, userSessionKey);
						security = (Security) new Ntlm1(flags, secondayMasterKey,false);
					}
					catch (Exception e)
					{
						throw new Exception("Exception occured while forming Session Security for Type3Response",e);
					}
				}

				return type3;
			}
		}

		private string getTargetFromTargetInformation(sbyte[] targetInformation)
		{
			 string target = null;

			 var i = 0;
			 while (i < targetInformation.Length)
			 {
				 switch (Encdec.dec_uint16le(targetInformation, i))
				 {
					 case 1: //Server name
						 i++;
						 i++; //advance two bytes
						 int length = Encdec.dec_uint16le(targetInformation, i);
						 i++;
						 i++; //advance two bytes
						 var domainb = new sbyte[length];
						 Array.Copy(targetInformation, i, domainb, 0, length);
						 try
						 {
							  target = StringHelperClass.NewString(domainb, "UTF-16LE");
						 }
						 catch (UnsupportedEncodingException)
						 {
							 return null;
						 }
						 i = i + length;
						 i = targetInformation.Length;
						 break;
					 default: //skip bytes
						 i++;
						 i++; //advance two bytes
						 length = Encdec.dec_uint16le(targetInformation, i);
						 i++;
						 i++; //advance two bytes
						 i = i + length;
					 break;
				 }
			 }

			 return target;
		}
		internal virtual void createSecurityWhenServer(NtlmMessage type3)
		{
			var type3Message = (Type3Message)type3;
			//two things here...check for anonymous , in that case the user response key is new byte[16].
			//in case anonymous has not been sent then create the key using credentials.
			int flags = type3Message.Flags;
			var ntlmKeyFactory = new NTLMKeyFactory();
			sbyte[] secondayMasterKey;
			sbyte[] sessionResponseUserSessionKey = null;
			if (type3Message.getFlag(0x00000800)) //anonymous flag
			{
				//if it is anonymous the user session key is new byte[16];
				sessionResponseUserSessionKey = new sbyte[16];
			}
			else if (useNtlmV2)
			{
				//TODO this needs to be checked here since the key logic will be totally different 
				//and we have to get the key out of Type3 message response (blob of the NTLMv2 response.)
				var h = 0;
			}
			else
			{
				 //now create the key for the session
				//this key will be used to RC4 a 16 byte random key and set to the type3 message
				var servernonce = new sbyte[16];
				sbyte[] challenge = {1,2,3,4,5,6,7,8}; //challenge is fixed
				Array.Copy(challenge, 0, servernonce, 0, challenge.Length);
				Array.Copy(type3Message.LMResponse, 0, servernonce, 8, 8); //first 8 bytes only , the rest are all 0x00 and not required.
				try
				{
					sessionResponseUserSessionKey = ntlmKeyFactory.getNTLM2SessionResponseUserSessionKey(credentials.Password, servernonce);
				}
				catch (Exception e)
				{
					throw new Exception("Exception occured while forming Session Security from Type3 AUTH",e);
				}
			}

			try
			{
				//now RC4 decrypt the session key
				secondayMasterKey = ntlmKeyFactory.decryptSecondarySessionKey(type3Message.SessionKey, sessionResponseUserSessionKey);
				security = (Security) new Ntlm1(flags, secondayMasterKey,true);
			}
			catch (Exception e)
			{
				throw new Exception("Exception occured while forming Session Security Type3Response",e);
			}
		}

	}

}