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
    using Org.BouncyCastle.Security;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.Linq;

    /// <summary>
    /// Key factory for lan manager
    /// </summary>
    internal class NTLMKeyFactory {

        /// <summary>
        /// Get user session key
        /// </summary>
        /// <param name="target"></param>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <param name="challenge"></param>
        /// <param name="blob"></param>
        /// <returns></returns>
        public byte[] GetNTLMv2UserSessionKey(string target, string user,
            string password, byte[] challenge, byte[] blob) {
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
        /// <exception cref="SecurityUtilityException"> </exception>
        /// <exception cref="UnsupportedEncodingException"> </exception>
        /// <exception cref="SharpCifs.Util.Sharpen.NoSuchAlgorithmException"> </exception>
        public byte[] GetNTLM2SessionResponseUserSessionKey(string password, byte[] servernonce) {
            return Responses.HmacMD5(servernonce, GetNTLMUserSessionKey(password));
        }

        /// <summary>
        /// Randomly generated 16 bytes
        /// </summary>
        public byte[] SecondarySessionKey {
            get {
                var key = new byte[16];
                _random.NextBytes(key);
                return key;
            }
        }

        /// <summary>
        /// Get stream cipher
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IStreamCipher GetARCFOUR(byte[] key) {
            var attrib = new Hashtable();
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


        /// <summary>
        /// NTLMv1 User Session Key. Cases where LMcompatibilitylevel is 0,1,2.
        /// For 3,4,5 the logic is different and based upon the reponses being
        /// sent back (either LMv2 or NTLMv2)
        /// </summary>
        /// <param name="password">
        /// </param>
        /// <exception cref="UnsupportedEncodingException"> </exception>
        /// <exception cref="SecurityUtilityException"> </exception>
        private byte[] GetNTLMUserSessionKey(string password) {
            // look at NTLMPasswordAuthentication in SharpCifs. It supports only
            // the NTLMUserSessionKey and the LMv2UserSessionKey...we need more :(
            //		 byte key[] = new byte[16];
            var ntlmHash = Responses.NtlmHash(password);
            IDigest md4 = new MD4Digest();
            var ret = new byte[md4.GetDigestSize()];
            md4.BlockUpdate(ntlmHash, 0, ntlmHash.Length);
            md4.DoFinal(ret, 0);
            return ret;
        }

        /// <summary>
        /// Decrypt
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] DecryptSecondarySessionKey(byte[] encryptedData, byte[] key) {
            return ApplyARCFOUR(GetARCFOUR(key), encryptedData);
        }

        /// <summary>
        /// Encrypt
        /// </summary>
        /// <param name="plainData"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public byte[] EncryptSecondarySessionKey(byte[] plainData, byte[] key) {
            return ApplyARCFOUR(GetARCFOUR(key), plainData);
        }

        /// <summary>
        /// Generate client signing key
        /// </summary>
        /// <param name="secondarySessionKey"></param>
        /// <returns></returns>
        public byte[] GenerateClientSigningKeyUsingNegotiatedSecondarySessionKey(
            byte[] secondarySessionKey) {
            // TODO this can be moved out of here...
            var dataforhash = new byte[secondarySessionKey.Length + kclientSigningMagicConstant.Length];
            Array.Copy(secondarySessionKey, 0, dataforhash, 0, secondarySessionKey.Length);
            Array.Copy(kclientSigningMagicConstant, 0, dataforhash, secondarySessionKey.Length,
                kclientSigningMagicConstant.Length);
            IDigest md5 = new MD5Digest();
            var ret = new byte[md5.GetDigestSize()];
            md5.BlockUpdate(dataforhash, 0, dataforhash.Length);
            md5.DoFinal(ret, 0);
            return ret;
        }

        /// <summary>
        /// Generate sealing key
        /// </summary>
        /// <param name="secondarySessionKey"></param>
        /// <returns></returns>
        public byte[] GenerateClientSealingKeyUsingNegotiatedSecondarySessionKey(
            byte[] secondarySessionKey) {
            // TODO this can be moved out of here...
            var dataforhash = new byte[secondarySessionKey.Length + kclientSealingMagicConstant.Length];
            Array.Copy(secondarySessionKey, 0, dataforhash, 0, secondarySessionKey.Length);
            Array.Copy(kclientSealingMagicConstant, 0, dataforhash, secondarySessionKey.Length,
                kclientSealingMagicConstant.Length);
            IDigest md5 = new MD5Digest();
            var ret = new byte[md5.GetDigestSize()];
            md5.BlockUpdate(dataforhash, 0, dataforhash.Length);
            md5.DoFinal(ret, 0);
            return ret;
        }

        /// <summary>
        /// Generate server signing key
        /// </summary>
        /// <param name="secondarySessionKey"></param>
        /// <returns></returns>
        public byte[] GenerateServerSigningKeyUsingNegotiatedSecondarySessionKey(
            byte[] secondarySessionKey) {
            // TODO this can be moved out of here...
            var dataforhash = new byte[secondarySessionKey.Length + kserverSigningMagicConstant.Length];
            Array.Copy(secondarySessionKey, 0, dataforhash, 0, secondarySessionKey.Length);
            Array.Copy(kserverSigningMagicConstant, 0, dataforhash, secondarySessionKey.Length,
                kserverSigningMagicConstant.Length);
            IDigest md5 = new MD5Digest();
            var ret = new byte[md5.GetDigestSize()];
            md5.BlockUpdate(dataforhash, 0, dataforhash.Length);
            md5.DoFinal(ret, 0);
            return ret;
        }

        /// <summary>
        /// Generate server sealing key
        /// </summary>
        /// <param name="secondarySessionKey"></param>
        /// <returns></returns>
        public byte[] GenerateServerSealingKeyUsingNegotiatedSecondarySessionKey(
            byte[] secondarySessionKey) {
            //TODO this can be moved out of here...
            var dataforhash = new byte[secondarySessionKey.Length + kserverSealingMagicConstant.Length];
            Array.Copy(secondarySessionKey, 0, dataforhash, 0, secondarySessionKey.Length);
            Array.Copy(kserverSealingMagicConstant, 0, dataforhash, secondarySessionKey.Length,
                kserverSealingMagicConstant.Length);
            IDigest md5 = new MD5Digest();
            var ret = new byte[md5.GetDigestSize()];
            md5.BlockUpdate(dataforhash, 0, dataforhash.Length);
            md5.DoFinal(ret, 0);
            return ret;
        }

        /// <summary>
        /// Signing part 1
        /// </summary>
        /// <param name="sequenceNumber"></param>
        /// <param name="signingKey"></param>
        /// <param name="data"></param>
        /// <param name="lengthOfBuffer"></param>
        /// <exception cref="SharpCifs.Util.Sharpen.NoSuchAlgorithmException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns></returns>
        public byte[] SigningPt1(int sequenceNumber, byte[] signingKey,
            byte[] data, int lengthOfBuffer) {
            // TODO merge the signing routine for both client and server all that
            // they differ by are keys...as expected
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


        /// <summary>
        /// Signing part 2
        /// </summary>
        /// <param name="verifier"></param>
        /// <param name="rc4"></param>
        /// <exception cref="InvalidOperationException"></exception>
        public void SigningPt2(byte[] verifier, IStreamCipher rc4) {
            for (var i = 0; i < 8; i++) {
                //			verifier[i+4] = (byte) (verifier[i+4] ^ rc4.nextByte());
                verifier[i + 4] = (byte)rc4.ReturnByte(verifier[i + 4]);
            }
        }

        /// <summary>
        /// Test signatures
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool CompareSignature(byte[] src, byte[] target) {
            return src.SequenceEqual(target);
        }

        private readonly Random _random = new Random();
        private static readonly byte[] kclientSigningMagicConstant = {
            0x73, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x20,
            0x6b, 0x65, 0x79, 0x20, 0x74, 0x6f, 0x20, 0x63,
            0x6c, 0x69, 0x65, 0x6e, 0x74, 0x2d, 0x74, 0x6f,
            0x2d, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x20,
            0x73, 0x69, 0x67, 0x6e, 0x69, 0x6e, 0x67, 0x20,
            0x6b, 0x65, 0x79, 0x20, 0x6d, 0x61, 0x67, 0x69,
            0x63, 0x20, 0x63, 0x6f, 0x6e, 0x73, 0x74, 0x61,
            0x6e, 0x74, 0x00 };
        private static readonly byte[] kserverSigningMagicConstant = {
            0x73, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x20,
            0x6b, 0x65, 0x79, 0x20, 0x74, 0x6f, 0x20, 0x73,
            0x65, 0x72, 0x76, 0x65, 0x72, 0x2d, 0x74, 0x6f,
            0x2d, 0x63, 0x6c, 0x69, 0x65, 0x6e, 0x74, 0x20,
            0x73, 0x69, 0x67, 0x6e, 0x69, 0x6e, 0x67, 0x20,
            0x6b, 0x65, 0x79, 0x20, 0x6d, 0x61, 0x67, 0x69,
            0x63, 0x20, 0x63, 0x6f, 0x6e, 0x73, 0x74, 0x61,
            0x6e, 0x74, 0x00 };
        private static readonly byte[] kclientSealingMagicConstant = {
            0x73, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x20,
            0x6b, 0x65, 0x79, 0x20, 0x74, 0x6f, 0x20, 0x63,
            0x6c, 0x69, 0x65, 0x6e, 0x74, 0x2d, 0x74, 0x6f,
            0x2d, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x20,
            0x73, 0x65, 0x61, 0x6c, 0x69, 0x6e, 0x67, 0x20,
            0x6b, 0x65, 0x79, 0x20, 0x6d, 0x61, 0x67, 0x69,
            0x63, 0x20, 0x63, 0x6f, 0x6e, 0x73, 0x74, 0x61,
            0x6e, 0x74, 0x00 };
        private static readonly byte[] kserverSealingMagicConstant = {
            0x73, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x20,
            0x6b, 0x65, 0x79, 0x20, 0x74, 0x6f, 0x20, 0x73,
            0x65, 0x72, 0x76, 0x65, 0x72, 0x2d, 0x74, 0x6f,
            0x2d, 0x63, 0x6c, 0x69, 0x65, 0x6e, 0x74, 0x20,
            0x73, 0x65, 0x61, 0x6c, 0x69, 0x6e, 0x67, 0x20,
            0x6b, 0x65, 0x79, 0x20, 0x6d, 0x61, 0x67, 0x69,
            0x63, 0x20, 0x63, 0x6f, 0x6e, 0x73, 0x74, 0x61,
            0x6e, 0x74, 0x00 };
    }
}