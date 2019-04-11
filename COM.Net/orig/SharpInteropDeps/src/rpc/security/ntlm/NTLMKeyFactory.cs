using System;
using System.Collections;

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


    //import gnu.crypto.hash.MD4;
    //import gnu.crypto.hash.MD5;
    //import gnu.crypto.prng.ARCFour;
    //import gnu.crypto.prng.IRandom;
    //import gnu.crypto.prng.LimitReachedException;


    using CipherParameters = org.bouncycastle.crypto.CipherParameters;
    using Digest = org.bouncycastle.crypto.Digest;
    using StreamCipher = org.bouncycastle.crypto.StreamCipher;
    using MD4Digest = org.bouncycastle.crypto.digests.MD4Digest;
    using MD5Digest = org.bouncycastle.crypto.digests.MD5Digest;
    using RC4Engine = org.bouncycastle.crypto.engines.RC4Engine;
    using KeyParameter = org.bouncycastle.crypto.@params.KeyParameter;


    internal class NTLMKeyFactory {

        internal Random Random = new Random();

        private static readonly sbyte[] ClientSigningMagicConstant = new sbyte[]{ 0x73,0x65,0x73,0x73,0x69,0x6f,0x6e,0x20,0x6b,0x65,0x79,0x20,0x74,0x6f,0x20,0x63,0x6c,0x69,0x65,0x6e,0x74,0x2d,0x74,0x6f,0x2d,0x73,0x65,0x72,0x76,0x65,0x72,0x20,0x73,0x69,0x67,0x6e,0x69,0x6e,0x67,0x20,0x6b,0x65,0x79,0x20,0x6d,0x61,0x67,0x69,0x63,0x20,0x63,0x6f,0x6e,0x73,0x74,0x61,0x6e,0x74,0x00 };
        private static readonly sbyte[] ServerSigningMagicConstant = new sbyte[]{ 0x73,0x65,0x73,0x73,0x69,0x6f,0x6e,0x20,0x6b,0x65,0x79,0x20,0x74,0x6f,0x20,0x73,0x65,0x72,0x76,0x65,0x72,0x2d,0x74,0x6f,0x2d,0x63,0x6c,0x69,0x65,0x6e,0x74,0x20,0x73,0x69,0x67,0x6e,0x69,0x6e,0x67,0x20,0x6b,0x65,0x79,0x20,0x6d,0x61,0x67,0x69,0x63,0x20,0x63,0x6f,0x6e,0x73,0x74,0x61,0x6e,0x74,0x00 };
        private static readonly sbyte[] ClientSealingMagicConstant = new sbyte[]{ 0x73,0x65,0x73,0x73,0x69,0x6f,0x6e,0x20,0x6b,0x65,0x79,0x20,0x74,0x6f,0x20,0x63,0x6c,0x69,0x65,0x6e,0x74,0x2d,0x74,0x6f,0x2d,0x73,0x65,0x72,0x76,0x65,0x72,0x20,0x73,0x65,0x61,0x6c,0x69,0x6e,0x67,0x20,0x6b,0x65,0x79,0x20,0x6d,0x61,0x67,0x69,0x63,0x20,0x63,0x6f,0x6e,0x73,0x74,0x61,0x6e,0x74,0x00 };
        private static readonly sbyte[] ServerSealingMagicConstant = new sbyte[]{ 0x73,0x65,0x73,0x73,0x69,0x6f,0x6e,0x20,0x6b,0x65,0x79,0x20,0x74,0x6f,0x20,0x73,0x65,0x72,0x76,0x65,0x72,0x2d,0x74,0x6f,0x2d,0x63,0x6c,0x69,0x65,0x6e,0x74,0x20,0x73,0x65,0x61,0x6c,0x69,0x6e,0x67,0x20,0x6b,0x65,0x79,0x20,0x6d,0x61,0x67,0x69,0x63,0x20,0x63,0x6f,0x6e,0x73,0x74,0x61,0x6e,0x74,0x00 };

        public NTLMKeyFactory() {

        }


        /// <summary>
        /// NTLMv1 User Session Key. Cases where LMcompatibilitylevel is 0,1,2. For 3,4,5 the logic is different
        /// and based upon the reponses being sent back (either LMv2 or NTLMv2)
        /// </summary>
        /// <param name="password">
        /// @return </param>
        /// <exception cref="UnsupportedEncodingException"> </exception>
        /// <exception cref="DigestException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: byte[] getNTLMUserSessionKey(String password) throws java.io.UnsupportedEncodingException, java.security.DigestException
        public virtual sbyte[] GetNTLMUserSessionKey(string password) {
            //look at NTLMPasswordAuthentication in jcifs. It supports only the NTLMUserSessionKey and the LMv2UserSessionKey...we need more :(
    //         byte key[] = new byte[16];
             sbyte[] ntlmHash = Responses.NtlmHash(password);
    //         MD4 md4 = new MD4();
    //         md4.update(ntlmHash,0,ntlmHash.length);
    //         key = md4.digest();
    //         return key;
             Digest md4 = new MD4Digest();
             sbyte[] ret = new sbyte[md4.DigestSize];
             md4.Update(ntlmHash,0,ntlmHash.Length);
             md4.DoFinal(ret, 0);
             return ret;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: byte[] getNTLMv2UserSessionKey(String target, String user, String password, byte[] challenge,byte[] blob) throws Exception
        public virtual sbyte[] GetNTLMv2UserSessionKey(string target, string user, string password, sbyte[] challenge, sbyte[] blob) {
            sbyte[] key = new sbyte[16];
            sbyte[] ntlm2Hash = Responses.Ntlmv2Hash(target, user, password);
            sbyte[] data = new sbyte[challenge.Length + blob.Length];
            Array.Copy(challenge, 0, data, 0, challenge.Length);
            Array.Copy(blob, 0, data, challenge.Length, blob.Length);
            sbyte[] mac = Responses.HmacMD5(data, ntlm2Hash);
            key = Responses.HmacMD5(mac, ntlm2Hash);
            return key;
        }
        /// <summary>
        /// Password of the user
        /// </summary>
        /// <param name="password"> </param>
        /// <param name="servernonce"> challenge + nonce from NTLM2 Session Response
        /// @return </param>
        /// <exception cref="DigestException"> </exception>
        /// <exception cref="UnsupportedEncodingException"> </exception>
        /// <exception cref="NoSuchAlgorithmException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: byte[] getNTLM2SessionResponseUserSessionKey(String password, byte[] servernonce) throws java.security.NoSuchAlgorithmException, java.io.UnsupportedEncodingException, java.security.DigestException
        public virtual sbyte[] GetNTLM2SessionResponseUserSessionKey(string password, sbyte[] servernonce) {
            return Responses.HmacMD5(servernonce, GetNTLMUserSessionKey(password));
        }

        /// <summary>
        /// Randomly generated 16 bytes
        /// 
        /// @return
        /// </summary>
        public virtual sbyte[] SecondarySessionKey {
            get {
                sbyte[] key = new sbyte[16];
                Random.NextBytes(key);
                return key;
            }
        }

        public virtual StreamCipher GetARCFOUR(sbyte[] key) {
            Hashtable attrib = new Hashtable();
    //        IRandom keystream = new ARCFour();
    //        attrib.put(ARCFour.ARCFOUR_KEY_MATERIAL, key);
    //        keystream.init(attrib);
            StreamCipher keystream = new RC4Engine();
            CipherParameters @params = new KeyParameter(key);
            keystream.Init(true, @params);
            return keystream;
        }

        public virtual sbyte[] ApplyARCFOUR(StreamCipher keystream, sbyte[] data) {
            sbyte[] retData = new sbyte[data.Length];

            keystream.ProcessBytes(data, 0, data.Length, retData, 0);

    //        for (int i = 0; i < data.length; i++) {
    //           retData[i] = (byte) (data[i] ^ keystream.nextByte());
    //        }

            return retData;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: byte[] decryptSecondarySessionKey(byte[] encryptedData, byte[] key) throws IllegalStateException
        public virtual sbyte[] DecryptSecondarySessionKey(sbyte[] encryptedData, sbyte[] key) {
            return ApplyARCFOUR(GetARCFOUR(key),encryptedData);
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: byte[] encryptSecondarySessionKey(byte[] plainData, byte[] key) throws IllegalStateException
        public virtual sbyte[] EncryptSecondarySessionKey(sbyte[] plainData, sbyte[] key) {
            return ApplyARCFOUR(GetARCFOUR(key),plainData);
        }

        public virtual sbyte[] GenerateClientSigningKeyUsingNegotiatedSecondarySessionKey(sbyte[] secondarySessionKey) {
            //TODO this can be moved out of here...
            sbyte[] dataforhash = new sbyte[secondarySessionKey.Length + ClientSigningMagicConstant.Length];
            Array.Copy(secondarySessionKey, 0, dataforhash, 0, secondarySessionKey.Length);
            Array.Copy(ClientSigningMagicConstant, 0, dataforhash, secondarySessionKey.Length, ClientSigningMagicConstant.Length);
    //        MD5 md5 = new MD5();
    //        md5.update(dataforhash, 0, dataforhash.length);
    //        return md5.digest();
            Digest md5 = new MD5Digest();
            sbyte[] ret = new sbyte[md5.DigestSize];
            md5.Update(dataforhash, 0, dataforhash.Length);
            md5.DoFinal(ret, 0);
            return ret;
        }

        public virtual sbyte[] GenerateClientSealingKeyUsingNegotiatedSecondarySessionKey(sbyte[] secondarySessionKey) {
            //TODO this can be moved out of here...
            sbyte[] dataforhash = new sbyte[secondarySessionKey.Length + ClientSealingMagicConstant.Length];
            Array.Copy(secondarySessionKey, 0, dataforhash, 0, secondarySessionKey.Length);
            Array.Copy(ClientSealingMagicConstant, 0, dataforhash, secondarySessionKey.Length, ClientSealingMagicConstant.Length);
    //        MD5 md5 = new MD5();
    //        md5.update(dataforhash, 0, dataforhash.length);
    //        return md5.digest();
            Digest md5 = new MD5Digest();
            sbyte[] ret = new sbyte[md5.DigestSize];
            md5.Update(dataforhash, 0, dataforhash.Length);
            md5.DoFinal(ret, 0);
            return ret;
        }

        public virtual sbyte[] GenerateServerSigningKeyUsingNegotiatedSecondarySessionKey(sbyte[] secondarySessionKey) {
            //TODO this can be moved out of here...
            sbyte[] dataforhash = new sbyte[secondarySessionKey.Length + ServerSigningMagicConstant.Length];
            Array.Copy(secondarySessionKey, 0, dataforhash, 0, secondarySessionKey.Length);
            Array.Copy(ServerSigningMagicConstant, 0, dataforhash, secondarySessionKey.Length, ServerSigningMagicConstant.Length);
    //        MD5 md5 = new MD5();
    //        md5.update(dataforhash, 0, dataforhash.length);
    //        return md5.digest();
            Digest md5 = new MD5Digest();
            sbyte[] ret = new sbyte[md5.DigestSize];
            md5.Update(dataforhash, 0, dataforhash.Length);
            md5.DoFinal(ret, 0);
            return ret;
        }

        public virtual sbyte[] GenerateServerSealingKeyUsingNegotiatedSecondarySessionKey(sbyte[] secondarySessionKey) {
            //TODO this can be moved out of here...
            sbyte[] dataforhash = new sbyte[secondarySessionKey.Length + ServerSealingMagicConstant.Length];
            Array.Copy(secondarySessionKey, 0, dataforhash, 0, secondarySessionKey.Length);
            Array.Copy(ServerSealingMagicConstant, 0, dataforhash, secondarySessionKey.Length, ServerSealingMagicConstant.Length);
    //        MD5 md5 = new MD5();
    //        md5.update(dataforhash, 0, dataforhash.length);
    //        return md5.digest();
            Digest md5 = new MD5Digest();
            sbyte[] ret = new sbyte[md5.DigestSize];
            md5.Update(dataforhash, 0, dataforhash.Length);
            md5.DoFinal(ret, 0);
            return ret;
        }

        //TODO merge the signing routine for both client and server all that they differ by are keys...as expected
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: byte[] signingPt1(int sequenceNumber, byte[] signingKey, byte[] data, int lengthOfBuffer) throws java.security.NoSuchAlgorithmException, IllegalStateException
        public virtual sbyte[] SigningPt1(int sequenceNumber, sbyte[] signingKey, sbyte[] data, int lengthOfBuffer) {
            sbyte[] seqNumPlusData = new sbyte[4 + lengthOfBuffer];

            seqNumPlusData[0] = unchecked((sbyte)(sequenceNumber & 0xFF));
            seqNumPlusData[1] = unchecked((sbyte)((sequenceNumber >> 8) & 0xFF));
            seqNumPlusData[2] = unchecked((sbyte)((sequenceNumber >> 16) & 0xFF));
            seqNumPlusData[3] = unchecked((sbyte)((sequenceNumber >> 24) & 0xFF));

            Array.Copy(data, 0, seqNumPlusData, 4, lengthOfBuffer);

            sbyte[] retval = new sbyte[16];
            retval[0] = 0x01; //Version number LE 1.

            sbyte[] sign = Responses.HmacMD5(seqNumPlusData, signingKey);

            for (int i = 0; i < 8; i++) {
                retval[i + 4] = sign[i];
            }

            retval[12] = unchecked((sbyte)(sequenceNumber & 0xFF));
            retval[13] = unchecked((sbyte)((sequenceNumber >> 8) & 0xFF));
            retval[14] = unchecked((sbyte)((sequenceNumber >> 16) & 0xFF));
            retval[15] = unchecked((sbyte)((sequenceNumber >> 24) & 0xFF));

            return retval;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void signingPt2(byte[] verifier, org.bouncycastle.crypto.StreamCipher rc4) throws IllegalStateException
        public virtual void SigningPt2(sbyte[] verifier, StreamCipher rc4) {
            for (int i = 0; i < 8; i++) {
    //            verifier[i+4] = (byte) (verifier[i+4] ^ rc4.nextByte());
                verifier[i + 4] = (sbyte)(rc4.ReturnByte(verifier[i + 4]));
            }
        }

        public virtual bool CompareSignature(sbyte[] src, sbyte[] target) {
            return Arrays.Equals(src, target);
        }

        //TODO merge the signing routine for both client and server all that they differ by are keys...as expected
    //    byte[] serverSigning(int sequenceNumber, byte[] serverSigningKey, byte[] data, IRandom rc4) throws NoSuchAlgorithmException, IllegalStateException, LimitReachedException
    //    {
    //        byte[] seqNumPlusData = new byte[4 + data.length];
    //
    //        seqNumPlusData[0] = (byte)(sequenceNumber & 0xFF);
    //        seqNumPlusData[1] = (byte)((sequenceNumber >> 8) & 0xFF);
    //        seqNumPlusData[2] = (byte)((sequenceNumber >> 16) & 0xFF);
    //        seqNumPlusData[3] = (byte)((sequenceNumber >> 24) & 0xFF);
    //
    //        System.arraycopy(data, 0, seqNumPlusData, 4, data.length);
    //
    //        byte[] retval = new byte[16];
    //        retval[0] = 0x01; //Version number LE 1.
    //
    //        byte[] sign = Responses.hmacMD5(seqNumPlusData, serverSigningKey);
    //
    //        for (int i = 0; i < 8; i++) {
    //            retval[i+4] = (byte) (sign[i] ^ rc4.nextByte());
    //        }
    //
    //        retval[12] = (byte)(sequenceNumber & 0xFF);
    //        retval[13] = (byte)((sequenceNumber >> 8) & 0xFF);
    //        retval[14] = (byte)((sequenceNumber >> 16) & 0xFF);
    //        retval[15] = (byte)((sequenceNumber >> 24) & 0xFF);
    //
    //        return retval;
    //    }

    //    byte[] clientSealing(int sequenceNumber, byte[] clientSealingKey, byte[] clientSigningKey, byte[] data,IRandom rc4) throws IllegalStateException, LimitReachedException, NoSuchAlgorithmException
    //    {
    //        //TODO..Imp... this implementation is not correct and should work for sequence 0, for the rest of the
    //        // sequences the arcfour state has to be maintained and not a new one used everytime...
    //        byte[] cipheredData = applyARCFOUR(rc4, data);
    //        byte[] signature = clientSigning(sequenceNumber, clientSigningKey, data, rc4);
    //        byte[] retval = new byte[cipheredData.length + signature.length];
    //        System.arraycopy(cipheredData, 0, retval, 0, cipheredData.length);
    //        System.arraycopy(signature, 0, retval, cipheredData.length,signature.length);
    //        return retval;
    //    }
    //
    //    byte[] serverSealing(int sequenceNumber, byte[] serverSealingKey, byte[] serverSigningKey, byte[] data, IRandom rc4) throws IllegalStateException, LimitReachedException, NoSuchAlgorithmException
    //    {
    //        //TODO..Imp... this implementation is not correct and should work for sequence 0, for the rest of the
    //        // sequences the arcfour state has to be maintained and not a new one used everytime...
    //        byte[] cipheredData = applyARCFOUR(rc4, data);
    //        byte[] signature = clientSigning(sequenceNumber, serverSigningKey, data, rc4);
    //        byte[] retval = new byte[cipheredData.length + signature.length];
    //        System.arraycopy(cipheredData, 0, retval, 0, cipheredData.length);
    //        System.arraycopy(signature, 0, retval, cipheredData.length,signature.length);
    //        return retval;
    //    }

    //    static void testFromDavenportPaper()
    //    {
    //        try
    //        {
    //
    //            NTLMKeyFactory keyFactory = new NTLMKeyFactory();
    //            byte[] challengePlusclientNonce = Util.toBytesFromString("677f1c557a5ee96c404d1b6f69152580");
    //            byte [] ntlm2UserSessionReponseKey = keyFactory.getNTLM2SessionResponseUserSessionKey("test1234", challengePlusclientNonce);
    //
    //            System.out.println(Util.dumpString(ntlm2UserSessionReponseKey));
    //
    //            byte[] secondaryEncryptedKey = Util.toBytesFromString("727a5240822ec7af4e9100c43e6fee7f");
    //
    //            byte[] decryptedSecondaryKey = keyFactory.decryptSecondarySessionKey(secondaryEncryptedKey, ntlm2UserSessionReponseKey);
    //            System.out.println(Util.dumpString(decryptedSecondaryKey));
    //
    //            //now lets try signature from server
    //            byte[] data = new byte[]{0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08};
    //            byte[] serverSigningKey = keyFactory.generateServerSigningKeyUsingNegotiatedSecondarySessionKey(decryptedSecondaryKey);
    //            System.out.println(Util.dumpString(serverSigningKey));
    //            byte[] serverSealingKey = keyFactory.generateServerSealingKeyUsingNegotiatedSecondarySessionKey(decryptedSecondaryKey);
    //            System.out.println(Util.dumpString(serverSealingKey));
    //            IRandom rc4 = keyFactory.getARCFOUR(serverSealingKey);
    //            System.out.println(Util.dumpString(keyFactory.serverSigning(0, serverSigningKey, data, rc4)));
    //            byte[] cipheredPack = keyFactory.serverSealing(1, serverSealingKey, serverSigningKey,data, rc4);
    //            System.out.println(Util.dumpString(cipheredPack));
    //
    //            IRandom rc4fordecipher = keyFactory.getARCFOUR(serverSealingKey);
    //            keyFactory.serverSigning(0, serverSigningKey, data, rc4fordecipher);//just like that for increasing rc4fordecipher state...will not be like this
    //            //in the actual implementation...
    //            byte[] cipheredData = new byte[8];
    //            System.arraycopy(cipheredPack, 0, cipheredData, 0, 8);
    //
    //            System.out.println(Util.dumpString(keyFactory.applyARCFOUR(rc4fordecipher, cipheredData)));
    //            int i = 0;
    //        }catch(Exception e)
    //        {
    //            e.printStackTrace();
    //        }
    //
    //    }
    //
    //    /**
    //     * @param args
    //     */
    //    static void main(String[] args) {
    //
    //        try
    //        {
    //
    //            NTLMKeyFactory keyFactory = new NTLMKeyFactory();
    //            byte[] challengePlusclientNonce = Util.toBytesFromString("38c2c82866a284b6a2d45d0f58feb085");
    //            byte [] ntlm2UserSessionReponseKey = keyFactory.getNTLM2SessionResponseUserSessionKey("enterprise", challengePlusclientNonce);
    //
    //            System.out.println(Util.dumpString(ntlm2UserSessionReponseKey));
    //
    //            byte[] secondaryEncryptedKey = Util.toBytesFromString("fa650f59feb62161fc08defeb9e5f5d2");
    //
    //            byte[] decryptedSecondaryKey = keyFactory.decryptSecondarySessionKey(secondaryEncryptedKey, ntlm2UserSessionReponseKey);
    //            System.out.println(Util.dumpString(decryptedSecondaryKey));
    //
    //            //now lets try signature from server
    //            byte[] data = new byte[]{0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08};
    //            byte[] clientSigningKey = keyFactory.generateClientSigningKeyUsingNegotiatedSecondarySessionKey(decryptedSecondaryKey);
    //            System.out.println(Util.dumpString(clientSigningKey));
    //            byte[] clientSealingKey = keyFactory.generateClientSealingKeyUsingNegotiatedSecondarySessionKey(decryptedSecondaryKey);
    //            System.out.println(Util.dumpString(clientSealingKey));
    ////            IRandom rc4 = keyFactory.getARCFOUR(serverSealingKey);
    //
    ////            byte[] cipheredPack = keyFactory.serverSealing(0, serverSealingKey, serverSigningKey,data, rc4);
    ////            System.out.println(Util.dumpString(cipheredPack));
    //
    //            IRandom rc4fordecipher = keyFactory.getARCFOUR(clientSealingKey);
    ////            keyFactory.serverSigning(0, serverSigningKey, data, rc4fordecipher);//just like that for increasing rc4fordecipher state...will not be like this
    //            //in the actual implementation...
    //            byte[] cipheredData = new byte[496];
    //            FileInputStream stream = new FileInputStream("c:/temp/encrypted");
    //            stream.read(cipheredData, 0, 496);
    ////            System.arraycopy(cipheredPack, 0, cipheredData, 0, 8);
    //            cipheredData = keyFactory.applyARCFOUR(rc4fordecipher, cipheredData);
    //            Hexdump.hexdump(System.out, cipheredData, 0, cipheredData.length);
    //            int i = 0;
    //        }catch(Exception e)
    //        {
    //            e.printStackTrace();
    //        }
    //
    //    }

    }

}