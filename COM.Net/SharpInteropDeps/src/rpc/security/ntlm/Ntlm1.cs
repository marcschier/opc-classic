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

	//import gnu.crypto.prng.IRandom;
	//import gnu.crypto.util.Util;


	using NtlmFlags = jcifs.ntlmssp.NtlmFlags;
	using NdrBuffer = ndr.NdrBuffer;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using StreamCipher = org.bouncycastle.crypto.StreamCipher;


	public class Ntlm1 : NtlmFlags, Security
	{

		private const int NTLM1_VERIFIER_LENGTH = 16;

	//    private IRandom clientCipher = null;
	//    private IRandom serverCipher = null;
		private readonly StreamCipher clientCipher;
		private readonly StreamCipher serverCipher;
		private readonly sbyte[] clientSigningKey;
		private readonly sbyte[] serverSigningKey;
		private NTLMKeyFactory keyFactory;
		private readonly bool isServer;
		private readonly int protectionLevel;

		private int requestCounter;
		private int responseCounter;

		private static readonly Logger logger = Logger.getLogger("org.jinterop");

		public Ntlm1(int flags, sbyte[] sessionKey, bool isServer)
		{

			protectionLevel = ((flags & NTLMSSP_NEGOTIATE_SEAL) != 0) ? Security_Fields.PROTECTION_LEVEL_PRIVACY : Security_Fields.PROTECTION_LEVEL_INTEGRITY;

			this.isServer = isServer;
			keyFactory = new NTLMKeyFactory();
			clientSigningKey = keyFactory.generateClientSigningKeyUsingNegotiatedSecondarySessionKey(sessionKey);
			var clientSealingKey = keyFactory.generateClientSealingKeyUsingNegotiatedSecondarySessionKey(sessionKey);

			serverSigningKey = keyFactory.generateServerSigningKeyUsingNegotiatedSecondarySessionKey(sessionKey);
			var serverSealingKey = keyFactory.generateServerSealingKeyUsingNegotiatedSecondarySessionKey(sessionKey);


			//Used by the server to decrypt client messages
			 clientCipher = keyFactory.getARCFOUR(clientSealingKey);

			//Used by the client to decrypt server messages
			 serverCipher = keyFactory.getARCFOUR(serverSealingKey);

	//		 if (logger.isLoggable(Level.FINEST))
	// 	    {
	//			 logger.finest("Client Signing Key derieved from the session key: [" + Util.dumpString(clientSigningKey) + "]");
	//			 logger.finest("Client Sealing Key derieved from the session key: [" + Util.dumpString(clientSealingKey) + "]");
	//			 logger.finest("Server Signing Key derieved from the session key: [" + Util.dumpString(serverSigningKey) + "]");
	//			 logger.finest("Server Sealing Key derieved from the session key: [" + Util.dumpString(serverSealingKey) + "]");
	// 	    }
		}

        public virtual int VerifierLength => NTLM1_VERIFIER_LENGTH;

        public virtual int AuthenticationService => NtlmAuthentication.AUTHENTICATION_SERVICE_NTLM;

        public virtual int ProtectionLevel => protectionLevel;

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void processIncoming(ndr.NetworkDataRepresentation ndr, int index, int length, int verifierIndex, bool isFragmented) throws java.io.IOException
        public virtual void processIncoming(NetworkDataRepresentation ndr, int index, int length, int verifierIndex, bool isFragmented)
		{
			try
			{
				var buffer = ndr.Buffer;

				sbyte[] signingKey = null;
	//            IRandom cipher = null;
				StreamCipher cipher = null;

				//reverse of what it is
				if (!isServer)
				{
					signingKey = serverSigningKey;
					cipher = serverCipher;
				}
				else
				{
					signingKey = clientSigningKey;
					cipher = clientCipher;
				}

				var data = new sbyte[length];
				Array.Copy(ndr.Buffer.Buffer,index,data, 0, data.Length);

				if (ProtectionLevel == Security_Fields.PROTECTION_LEVEL_PRIVACY)
				{
					data = keyFactory.applyARCFOUR(cipher, data);
					Array.Copy(data, 0, ndr.Buffer.buf, index, data.Length);
				}


				if (logger.isLoggable(Level.FINEST))
				{
					logger.finest("\n AFTER Decryption");
					var byteArrayOutputStream = new ByteArrayOutputStream();
					jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), data, 0, data.Length);
					logger.finest("\n" + byteArrayOutputStream.ToString());
					logger.finest("\nLength is: " + data.Length);
				}



				var verifier = keyFactory.signingPt1(responseCounter, signingKey, buffer.Buffer,verifierIndex);
				keyFactory.signingPt2(verifier, cipher);

				buffer.Index = verifierIndex;
				//now read the next 16 bytes and pass compare them
				var signing = new sbyte[16];
				ndr.readOctetArray(signing, 0, signing.Length);

				//this should result in an access denied fault
				if (!keyFactory.compareSignature(verifier, signing))
				{
					throw new IntegrityException("Message out of sequence. Perhaps the user being used to run this application is different from the one under which the COM server is running !.");
				}

				//only clients increment, servers just respond to the clients seq id.
	//            if (!isServer || isFragmented)
	//            {
	//            	responseCounter++;
	//            }

				responseCounter++;


			}
			catch (IOException ex)
			{
				logger.log(Level.SEVERE, "", ex);
				throw ex;
			}
			catch (Exception ex)
			{
				logger.log(Level.SEVERE, "", ex);
				throw new IntegrityException("General error: " + ex.Message);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void processOutgoing(ndr.NetworkDataRepresentation ndr, int index, int length, int verifierIndex, bool isFragmented) throws java.io.IOException
		public virtual void processOutgoing(NetworkDataRepresentation ndr, int index, int length, int verifierIndex, bool isFragmented)
		{
			try
			{
				var buffer = ndr.Buffer;

				sbyte[] signingKey = null;
	//            IRandom cipher = null;
				StreamCipher cipher = null;

				if (isServer)
				{
					signingKey = serverSigningKey;
					cipher = serverCipher;
				}
				else
				{
					signingKey = clientSigningKey;
					cipher = clientCipher;
				}

				var verifier = keyFactory.signingPt1(requestCounter, signingKey, buffer.Buffer,verifierIndex);
				var data = new sbyte[length];
				Array.Copy(ndr.Buffer.Buffer,index,data, 0, data.Length);
				if (logger.isLoggable(Level.FINEST))
				{
					logger.finest("\n BEFORE Encryption");
					var byteArrayOutputStream = new ByteArrayOutputStream();
					jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), data, 0, data.Length);
					logger.finest("\n" + byteArrayOutputStream.ToString());
					logger.finest("\n Length is: " + data.Length);
				}


				if (ProtectionLevel == Security_Fields.PROTECTION_LEVEL_PRIVACY)
				{
					var data2 = keyFactory.applyARCFOUR(cipher, data);
					Array.Copy(data2, 0, ndr.Buffer.buf, index, data2.Length);
				}
				keyFactory.signingPt2(verifier, cipher);
				buffer.Index = verifierIndex;
				buffer.writeOctetArray(verifier, 0, verifier.Length);


	//            if (isServer && !isFragmented)
	//            {
	//            	responseCounter++;
	//            }

				requestCounter++;


			}
			catch (Exception ex)
			{
				throw new IntegrityException("General error: " + ex.Message);
			}
		}

	}

}