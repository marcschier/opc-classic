
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

namespace rpc.security.ntlm {
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Digests;
    using Org.BouncyCastle.Crypto.Engines;
    using Org.BouncyCastle.Crypto.Parameters;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Linq;

    internal class NTLMKeyFactory {

        internal Random random = new Random();

        private static readonly byte[] clientSigningMagicConstant = { 0x73, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x20, 0x6b, 0x65, 0x79, 0x20, 0x74, 0x6f, 0x20, 0x63, 0x6c, 0x69, 0x65, 0x6e, 0x74, 0x2d, 0x74, 0x6f, 0x2d, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x20, 0x73, 0x69, 0x67, 0x6e, 0x69, 0x6e, 0x67, 0x20, 0x6b, 0x65, 0x79, 0x20, 0x6d, 0x61, 0x67, 0x69, 0x63, 0x20, 0x63, 0x6f, 0x6e, 0x73, 0x74, 0x61, 0x6e, 0x74, 0x00 };
        private static readonly byte[] serverSigningMagicConstant = { 0x73, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x20, 0x6b, 0x65, 0x79, 0x20, 0x74, 0x6f, 0x20, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x2d, 0x74, 0x6f, 0x2d, 0x63, 0x6c, 0x69, 0x65, 0x6e, 0x74, 0x20, 0x73, 0x69, 0x67, 0x6e, 0x69, 0x6e, 0x67, 0x20, 0x6b, 0x65, 0x79, 0x20, 0x6d, 0x61, 0x67, 0x69, 0x63, 0x20, 0x63, 0x6f, 0x6e, 0x73, 0x74, 0x61, 0x6e, 0x74, 0x00 };
        private static readonly byte[] clientSealingMagicConstant = { 0x73, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x20, 0x6b, 0x65, 0x79, 0x20, 0x74, 0x6f, 0x20, 0x63, 0x6c, 0x69, 0x65, 0x6e, 0x74, 0x2d, 0x74, 0x6f, 0x2d, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x20, 0x73, 0x65, 0x61, 0x6c, 0x69, 0x6e, 0x67, 0x20, 0x6b, 0x65, 0x79, 0x20, 0x6d, 0x61, 0x67, 0x69, 0x63, 0x20, 0x63, 0x6f, 0x6e, 0x73, 0x74, 0x61, 0x6e, 0x74, 0x00 };
        private static readonly byte[] serverSealingMagicConstant = { 0x73, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x20, 0x6b, 0x65, 0x79, 0x20, 0x74, 0x6f, 0x20, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x2d, 0x74, 0x6f, 0x2d, 0x63, 0x6c, 0x69, 0x65, 0x6e, 0x74, 0x20, 0x73, 0x65, 0x61, 0x6c, 0x69, 0x6e, 0x67, 0x20, 0x6b, 0x65, 0x79, 0x20, 0x6d, 0x61, 0x67, 0x69, 0x63, 0x20, 0x63, 0x6f, 0x6e, 0x73, 0x74, 0x61, 0x6e, 0x74, 0x00 };

        internal NTLMKeyFactory() {

        }


        /// <summary>
        /// NTLMv1 User Session Key. Cases where LMcompatibilitylevel is 0,1,2. For 3,4,5 the logic is different
        /// and based upon the reponses being sent back (either LMv2 or NTLMv2)
        /// </summary>
        /// <param name="password">
        /// </param>
        /// <exception cref="UnsupportedEncodingException"> </exception>
        /// <exception cref="DigestException"> </exception>
        internal virtual byte[] GetNTLMUserSessionKey(string password) {
            //look at NTLMPasswordAuthentication in SharpCifs. It supports only the NTLMUserSessionKey and the LMv2UserSessionKey...we need more :(
            //		 byte key[] = new byte[16];
            var ntlmHash = Responses.NtlmHash(password);
            //		 MD4 md4 = new MD4();
            //	     md4.update(ntlmHash,0,ntlmHash.length);
            //	     key = md4.digest();
            //		 return key;
            IDigest md4 = new MD4Digest();
            var ret = new byte[md4.GetDigestSize()];
            md4.BlockUpdate(ntlmHash, 0, ntlmHash.Length);
            md4.DoFinal(ret, 0);
            return ret;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: byte[] getNTLMv2UserSessionKey(String target, String user, String password, byte[] challenge,byte[] blob) throws Exception
        internal virtual byte[] GetNTLMv2UserSessionKey(string target, string user, string password, byte[] challenge, byte[] blob) {
            var key = new byte[16];
            var ntlm2Hash = Responses.Ntlmv2Hash(target, user, password);
            var data = new byte[challenge.Length + blob.Length];
            Array.Copy(challenge, 0, data, 0, challenge.Length);
            Array.Copy(blob, 0, data, challenge.Length, blob.Length);
            var mac = Responses.HmacMD5(data, ntlm2Hash);
            key = Responses.HmacMD5(mac, ntlm2Hash);
            return key;
        }
        /// <summary>
        /// Password of the user
        /// </summary>
        /// <param name="password"> </param>
        /// <param name="servernonce"> challenge + nonce from NTLM2 Session Response
        /// </param>
        /// <exception cref="DigestException"> </exception>
        /// <exception cref="UnsupportedEncodingException"> </exception>
        /// <exception cref="NoSuchAlgorithmException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: byte[] getNTLM2SessionResponseUserSessionKey(String password, byte[] servernonce) throws java.security.NoSuchAlgorithmException, java.io.UnsupportedEncodingException, java.security.DigestException
        internal virtual byte[] GetNTLM2SessionResponseUserSessionKey(string password, byte[] servernonce) {
            return Responses.HmacMD5(servernonce, GetNTLMUserSessionKey(password));
        }

        /// <summary>
        /// Randomly generated 16 bytes
        /// 
        /// @return
        /// </summary>
        internal virtual byte[] SecondarySessionKey {
            get {
                var key = new byte[16];
                random.NextBytes(key);
                return key;
            }
        }

        internal virtual IStreamCipher GetARCFOUR(byte[] key) {
            var attrib = new Hashtable();
            //		IRandom keystream = new ARCFour();
            //		attrib.put(ARCFour.ARCFOUR_KEY_MATERIAL, key);
            //		keystream.init(attrib);
            IStreamCipher keystream = new RC4Engine();
            ICipherParameters @params = new KeyParameter(key);
            keystream.Init(true, @params);
            return keystream;
        }

        internal virtual byte[] ApplyARCFOUR(IStreamCipher keystream, byte[] data) {
            var retData = new byte[data.Length];

            keystream.ProcessBytes(data, 0, data.Length, retData, 0);

            //		for (int i = 0; i < data.length; i++) {
            //		   retData[i] = (byte) (data[i] ^ keystream.nextByte());
            //		}

            return retData;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: byte[] decryptSecondarySessionKey(byte[] encryptedData, byte[] key) throws System.InvalidOperationException
        internal virtual byte[] DecryptSecondarySessionKey(byte[] encryptedData, byte[] key) {
            return ApplyARCFOUR(GetARCFOUR(key), encryptedData);
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: byte[] encryptSecondarySessionKey(byte[] plainData, byte[] key) throws System.InvalidOperationException
        internal virtual byte[] EncryptSecondarySessionKey(byte[] plainData, byte[] key) {
            return ApplyARCFOUR(GetARCFOUR(key), plainData);
        }

        internal virtual byte[] GenerateClientSigningKeyUsingNegotiatedSecondarySessionKey(byte[] secondarySessionKey) {
            //TODO this can be moved out of here...
            var dataforhash = new byte[secondarySessionKey.Length + clientSigningMagicConstant.Length];
            Array.Copy(secondarySessionKey, 0, dataforhash, 0, secondarySessionKey.Length);
            Array.Copy(clientSigningMagicConstant, 0, dataforhash, secondarySessionKey.Length, clientSigningMagicConstant.Length);
            //		MD5 md5 = new MD5();
            //		md5.update(dataforhash, 0, dataforhash.length);
            //		return md5.digest();
            IDigest md5 = new MD5Digest();
            var ret = new byte[md5.GetDigestSize()];
            md5.BlockUpdate(dataforhash, 0, dataforhash.Length);
            md5.DoFinal(ret, 0);
            return ret;
        }

        internal virtual byte[] GenerateClientSealingKeyUsingNegotiatedSecondarySessionKey(byte[] secondarySessionKey) {
            //TODO this can be moved out of here...
            var dataforhash = new byte[secondarySessionKey.Length + clientSealingMagicConstant.Length];
            Array.Copy(secondarySessionKey, 0, dataforhash, 0, secondarySessionKey.Length);
            Array.Copy(clientSealingMagicConstant, 0, dataforhash, secondarySessionKey.Length, clientSealingMagicConstant.Length);
            //		MD5 md5 = new MD5();
            //		md5.update(dataforhash, 0, dataforhash.length);
            //		return md5.digest();
            IDigest md5 = new MD5Digest();
            var ret = new byte[md5.GetDigestSize()];
            md5.BlockUpdate(dataforhash, 0, dataforhash.Length);
            md5.DoFinal(ret, 0);
            return ret;
        }

        internal virtual byte[] GenerateServerSigningKeyUsingNegotiatedSecondarySessionKey(byte[] secondarySessionKey) {
            //TODO this can be moved out of here...
            var dataforhash = new byte[secondarySessionKey.Length + serverSigningMagicConstant.Length];
            Array.Copy(secondarySessionKey, 0, dataforhash, 0, secondarySessionKey.Length);
            Array.Copy(serverSigningMagicConstant, 0, dataforhash, secondarySessionKey.Length, serverSigningMagicConstant.Length);
            //		MD5 md5 = new MD5();
            //		md5.update(dataforhash, 0, dataforhash.length);
            //		return md5.digest();
            IDigest md5 = new MD5Digest();
            var ret = new byte[md5.GetDigestSize()];
            md5.BlockUpdate(dataforhash, 0, dataforhash.Length);
            md5.DoFinal(ret, 0);
            return ret;
        }

        internal virtual byte[] GenerateServerSealingKeyUsingNegotiatedSecondarySessionKey(byte[] secondarySessionKey) {
            //TODO this can be moved out of here...
            var dataforhash = new byte[secondarySessionKey.Length + serverSealingMagicConstant.Length];
            Array.Copy(secondarySessionKey, 0, dataforhash, 0, secondarySessionKey.Length);
            Array.Copy(serverSealingMagicConstant, 0, dataforhash, secondarySessionKey.Length, serverSealingMagicConstant.Length);
            //		MD5 md5 = new MD5();
            //		md5.update(dataforhash, 0, dataforhash.length);
            //		return md5.digest();
            IDigest md5 = new MD5Digest();
            var ret = new byte[md5.GetDigestSize()];
            md5.BlockUpdate(dataforhash, 0, dataforhash.Length);
            md5.DoFinal(ret, 0);
            return ret;
        }

        //TODO merge the signing routine for both client and server all that they differ by are keys...as expected
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: byte[] signingPt1(int sequenceNumber, byte[] signingKey, byte[] data, int lengthOfBuffer) throws java.security.NoSuchAlgorithmException, System.InvalidOperationException
        internal virtual byte[] SigningPt1(int sequenceNumber, byte[] signingKey, byte[] data, int lengthOfBuffer) {
            var seqNumPlusData = new byte[4 + lengthOfBuffer];

            seqNumPlusData[0] = unchecked((byte)(sequenceNumber & 0xFF));
            seqNumPlusData[1] = unchecked((byte)((sequenceNumber >> 8) & 0xFF));
            seqNumPlusData[2] = unchecked((byte)((sequenceNumber >> 16) & 0xFF));
            seqNumPlusData[3] = unchecked((byte)((sequenceNumber >> 24) & 0xFF));

            Array.Copy(data, 0, seqNumPlusData, 4, lengthOfBuffer);

            var retval = new byte[16];
            retval[0] = 0x01; //Version number LE 1.

            var sign = Responses.HmacMD5(seqNumPlusData, signingKey);

            for (var i = 0; i < 8; i++) {
                retval[i + 4] = sign[i];
            }

            retval[12] = unchecked((byte)(sequenceNumber & 0xFF));
            retval[13] = unchecked((byte)((sequenceNumber >> 8) & 0xFF));
            retval[14] = unchecked((byte)((sequenceNumber >> 16) & 0xFF));
            retval[15] = unchecked((byte)((sequenceNumber >> 24) & 0xFF));

            return retval;
        }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: void signingPt2(byte[] verifier, org.bouncycastle.crypto.StreamCipher rc4) throws System.InvalidOperationException
        internal virtual void SigningPt2(byte[] verifier, IStreamCipher rc4) {
            for (var i = 0; i < 8; i++) {
                //			verifier[i+4] = (byte) (verifier[i+4] ^ rc4.nextByte());
                verifier[i + 4] = (byte)rc4.ReturnByte(verifier[i + 4]);
            }
        }

        internal virtual bool CompareSignature(byte[] src, byte[] target) {
            return src.SequenceEqual(target);
        }

        //TODO merge the signing routine for both client and server all that they differ by are keys...as expected
        //	byte[] serverSigning(int sequenceNumber, byte[] serverSigningKey, byte[] data, IRandom rc4) throws NoSuchAlgorithmException, System.InvalidOperationException, LimitReachedException
        //	{
        //		byte[] seqNumPlusData = new byte[4 + data.length];
        //
        //		seqNumPlusData[0] = (byte)(sequenceNumber & 0xFF);
        //		seqNumPlusData[1] = (byte)((sequenceNumber >> 8) & 0xFF);
        //		seqNumPlusData[2] = (byte)((sequenceNumber >> 16) & 0xFF);
        //		seqNumPlusData[3] = (byte)((sequenceNumber >> 24) & 0xFF);
        //
        //		System.arraycopy(data, 0, seqNumPlusData, 4, data.length);
        //
        //		byte[] retval = new byte[16];
        //		retval[0] = 0x01; //Version number LE 1.
        //
        //		byte[] sign = Responses.hmacMD5(seqNumPlusData, serverSigningKey);
        //
        //		for (int i = 0; i < 8; i++) {
        //			retval[i+4] = (byte) (sign[i] ^ rc4.nextByte());
        //		}
        //
        //		retval[12] = (byte)(sequenceNumber & 0xFF);
        //		retval[13] = (byte)((sequenceNumber >> 8) & 0xFF);
        //		retval[14] = (byte)((sequenceNumber >> 16) & 0xFF);
        //		retval[15] = (byte)((sequenceNumber >> 24) & 0xFF);
        //
        //		return retval;
        //	}

        //	byte[] clientSealing(int sequenceNumber, byte[] clientSealingKey, byte[] clientSigningKey, byte[] data,IRandom rc4) throws System.InvalidOperationException, LimitReachedException, NoSuchAlgorithmException
        //	{
        //		//TODO..Imp... this implementation is not correct and should work for sequence 0, for the rest of the
        //		// sequences the arcfour state has to be maintained and not a new one used everytime...
        //		byte[] cipheredData = applyARCFOUR(rc4, data);
        //		byte[] signature = clientSigning(sequenceNumber, clientSigningKey, data, rc4);
        //		byte[] retval = new byte[cipheredData.length + signature.length];
        //		System.arraycopy(cipheredData, 0, retval, 0, cipheredData.length);
        //		System.arraycopy(signature, 0, retval, cipheredData.length,signature.length);
        //		return retval;
        //	}
        //
        //	byte[] serverSealing(int sequenceNumber, byte[] serverSealingKey, byte[] serverSigningKey, byte[] data, IRandom rc4) throws System.InvalidOperationException, LimitReachedException, NoSuchAlgorithmException
        //	{
        //		//TODO..Imp... this implementation is not correct and should work for sequence 0, for the rest of the
        //		// sequences the arcfour state has to be maintained and not a new one used everytime...
        //		byte[] cipheredData = applyARCFOUR(rc4, data);
        //		byte[] signature = clientSigning(sequenceNumber, serverSigningKey, data, rc4);
        //		byte[] retval = new byte[cipheredData.length + signature.length];
        //		System.arraycopy(cipheredData, 0, retval, 0, cipheredData.length);
        //		System.arraycopy(signature, 0, retval, cipheredData.length,signature.length);
        //		return retval;
        //	}

        //	static void testFromDavenportPaper()
        //	{
        //		try
        //		{
        //
        //			NTLMKeyFactory keyFactory = new NTLMKeyFactory();
        //			byte[] challengePlusclientNonce = Util.toBytesFromString("677f1c557a5ee96c404d1b6f69152580");
        //			byte [] ntlm2UserSessionReponseKey = keyFactory.getNTLM2SessionResponseUserSessionKey("test1234", challengePlusclientNonce);
        //
        //			System.out.println(Util.dumpString(ntlm2UserSessionReponseKey));
        //
        //			byte[] secondaryEncryptedKey = Util.toBytesFromString("727a5240822ec7af4e9100c43e6fee7f");
        //
        //			byte[] decryptedSecondaryKey = keyFactory.decryptSecondarySessionKey(secondaryEncryptedKey, ntlm2UserSessionReponseKey);
        //			System.out.println(Util.dumpString(decryptedSecondaryKey));
        //
        //			//now lets try signature from server
        //			byte[] data = new byte[]{0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08};
        //			byte[] serverSigningKey = keyFactory.generateServerSigningKeyUsingNegotiatedSecondarySessionKey(decryptedSecondaryKey);
        //			System.out.println(Util.dumpString(serverSigningKey));
        //			byte[] serverSealingKey = keyFactory.generateServerSealingKeyUsingNegotiatedSecondarySessionKey(decryptedSecondaryKey);
        //			System.out.println(Util.dumpString(serverSealingKey));
        //			IRandom rc4 = keyFactory.getARCFOUR(serverSealingKey);
        //			System.out.println(Util.dumpString(keyFactory.serverSigning(0, serverSigningKey, data, rc4)));
        //			byte[] cipheredPack = keyFactory.serverSealing(1, serverSealingKey, serverSigningKey,data, rc4);
        //			System.out.println(Util.dumpString(cipheredPack));
        //
        //			IRandom rc4fordecipher = keyFactory.getARCFOUR(serverSealingKey);
        //			keyFactory.serverSigning(0, serverSigningKey, data, rc4fordecipher);//just like that for increasing rc4fordecipher state...will not be like this
        //			//in the actual implementation...
        //			byte[] cipheredData = new byte[8];
        //			System.arraycopy(cipheredPack, 0, cipheredData, 0, 8);
        //
        //			System.out.println(Util.dumpString(keyFactory.applyARCFOUR(rc4fordecipher, cipheredData)));
        //			int i = 0;
        //		}catch(Exception e)
        //		{
        //			e.printStackTrace();
        //		}
        //
        //	}
        //
        //	/**
        //	 * @param args
        //	 */
        //	static void main(String[] args) {
        //
        //		try
        //		{
        //
        //			NTLMKeyFactory keyFactory = new NTLMKeyFactory();
        //			byte[] challengePlusclientNonce = Util.toBytesFromString("38c2c82866a284b6a2d45d0f58feb085");
        //			byte [] ntlm2UserSessionReponseKey = keyFactory.getNTLM2SessionResponseUserSessionKey("enterprise", challengePlusclientNonce);
        //
        //			System.out.println(Util.dumpString(ntlm2UserSessionReponseKey));
        //
        //			byte[] secondaryEncryptedKey = Util.toBytesFromString("fa650f59feb62161fc08defeb9e5f5d2");
        //
        //			byte[] decryptedSecondaryKey = keyFactory.decryptSecondarySessionKey(secondaryEncryptedKey, ntlm2UserSessionReponseKey);
        //			System.out.println(Util.dumpString(decryptedSecondaryKey));
        //
        //			//now lets try signature from server
        //			byte[] data = new byte[]{0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08};
        //			byte[] clientSigningKey = keyFactory.generateClientSigningKeyUsingNegotiatedSecondarySessionKey(decryptedSecondaryKey);
        //			System.out.println(Util.dumpString(clientSigningKey));
        //			byte[] clientSealingKey = keyFactory.generateClientSealingKeyUsingNegotiatedSecondarySessionKey(decryptedSecondaryKey);
        //			System.out.println(Util.dumpString(clientSealingKey));
        ////			IRandom rc4 = keyFactory.getARCFOUR(serverSealingKey);
        //
        ////			byte[] cipheredPack = keyFactory.serverSealing(0, serverSealingKey, serverSigningKey,data, rc4);
        ////			System.out.println(Util.dumpString(cipheredPack));
        //
        //			IRandom rc4fordecipher = keyFactory.getARCFOUR(clientSealingKey);
        ////			keyFactory.serverSigning(0, serverSigningKey, data, rc4fordecipher);//just like that for increasing rc4fordecipher state...will not be like this
        //			//in the actual implementation...
        //			byte[] cipheredData = new byte[496];
        //			FileInputStream stream = new FileInputStream("c:/temp/encrypted");
        //			stream.read(cipheredData, 0, 496);
        ////			System.arraycopy(cipheredPack, 0, cipheredData, 0, 8);
        //			cipheredData = keyFactory.applyARCFOUR(rc4fordecipher, cipheredData);
        //        	Hexdump.hexdump(System.out, cipheredData, 0, cipheredData.length);
        //			int i = 0;
        //		}catch(Exception e)
        //		{
        //			e.printStackTrace();
        //		}
        //
        //	}

    }

}