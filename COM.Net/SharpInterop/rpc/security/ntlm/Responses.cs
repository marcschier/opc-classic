// Extracted from http://davenport.sourceforge.net/ntlm.html
// Copyright � 2003, 2006 Eric Glass (eric.glass@gmail.com) 

namespace rpc.security.ntlm {
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Digests;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.Security;
    using SharpCifs.Util;
    using SharpCifs.Util.Sharpen;
    using System;

    /// <summary>
    /// Calculates the various Type 3 responses.
    /// </summary>
    public static class Responses {

        /// <summary>
        /// Calculates the LM Response for the given challenge, using the specified
        /// password.
        /// </summary>
        /// <param name="password"> The user's password. </param>
        /// <param name="challenge"> The Type 2 challenge from the server.
        /// </param>
        /// <returns> The LM Response. </returns>
        public static byte[] GetLMResponse(string password, byte[] challenge) {
            var lmHash_Renamed = LmHash(password);
            return LmResponse(lmHash_Renamed, challenge);
        }

        /// <summary>
        /// Calculates the NTLM Response for the given challenge, using the
        /// specified password.
        /// </summary>
        /// <param name="password"> The user's password. </param>
        /// <param name="challenge"> The Type 2 challenge from the server.
        /// </param>
        /// <returns> The NTLM Response. </returns>
        public static byte[] GetNTLMResponse(string password, byte[] challenge) {
            var ntlmHash_Renamed = NtlmHash(password);
            return LmResponse(ntlmHash_Renamed, challenge);
        }

        /// <summary>
        /// Calculates the NTLMv2 Response for the given challenge, using the
        /// specified authentication target, username, password, target information
        /// block, and client nonce.
        /// </summary>
        /// <param name="target"> The authentication target (i.e., domain). </param>
        /// <param name="user"> The username. </param>
        /// <param name="password"> The user's password. </param>
        /// <param name="targetInformation"> The target information block from the Type 2
        /// message. </param>
        /// <param name="challenge"> The Type 2 challenge from the server. </param>
        /// <param name="clientNonce"> The random 8-byte client nonce. 
        /// </param>
        /// <returns> The NTLMv2 Response. </returns>
        public static byte[][] GetNTLMv2Response(string target, string user, string password, 
            byte[] targetInformation, byte[] challenge, byte[] clientNonce) {
            var retval = new byte[2][];
            var ntlmv2Hash_Renamed = Ntlmv2Hash(target, user, password);
            var blob = CreateBlob(targetInformation, clientNonce);
            retval[1] = blob;
            retval[0] = Lmv2Response(ntlmv2Hash_Renamed, blob, challenge);
            return retval;
        }

        /// <summary>
        /// Calculates the LMv2 Response for the given challenge, using the
        /// specified authentication target, username, password, and client
        /// challenge.
        /// </summary>
        /// <param name="target"> The authentication target (i.e., domain). </param>
        /// <param name="user"> The username. </param>
        /// <param name="password"> The user's password. </param>
        /// <param name="challenge"> The Type 2 challenge from the server. </param>
        /// <param name="clientNonce"> The random 8-byte client nonce.
        /// </param>
        /// <returns> The LMv2 Response.  </returns>
        public static byte[] GetLMv2Response(string target, string user, string password, byte[] challenge, byte[] clientNonce) {
            var ntlmv2Hash_Renamed = Ntlmv2Hash(target, user, password);
            return Lmv2Response(ntlmv2Hash_Renamed, clientNonce, challenge);
        }

        /// <summary>
        /// Calculates the NTLM2 Session Response for the given challenge, using the
        /// specified password and client nonce.
        /// </summary>
        /// <param name="password"> The user's password. </param>
        /// <param name="challenge"> The Type 2 challenge from the server. </param>
        /// <param name="clientNonce"> The random 8-byte client nonce.
        /// </param>
        /// <returns> The NTLM2 Session Response.  This is placed in the NTLM
        /// response field of the Type 3 message; the LM response field contains
        /// the client nonce, null-padded to 24 bytes. </returns>
        /// <exception cref="SharpCifs.Util.Sharpen.NoSuchAlgorithmException"> </exception>
        /// <exception cref="InvalidOperationException"> </exception>
        /// <exception cref="InvalidKeyException">  </exception>
        public static byte[] GetNTLM2SessionResponse(string password, byte[] challenge, byte[] clientNonce) {
            var hash = NtlmHash(password);
            var md5 = DigestUtilities.GetDigest("MD5");
            md5.BlockUpdate(challenge, 0, challenge.Length);
            md5.BlockUpdate(clientNonce, 0, clientNonce.Length);
            var sessionHash = new byte[8];
            md5.DoFinal(sessionHash, 0);
            // was: Array.Copy(md5.digest(), 0, sessionHash, 0, 8);
            return LmResponse(hash, sessionHash);
        }

        /// <summary>
        /// Creates the LM Hash of the user's password.
        /// </summary>
        /// <param name="password"> The password.
        /// </param>
        /// <returns> The LM Hash of the given password, used in the calculation
        /// of the LM Response. </returns>
        private static byte[] LmHash(string password) {
            var oemPassword = password.ToUpper().GetBytes("US-ASCII");
            var length = Math.Min(oemPassword.Length, 14);
            var keyBytes = new byte[14];
            Array.Copy(oemPassword, 0, keyBytes, 0, length);
            var lowKey = CreateDESKey(keyBytes, 0);
            var highKey = CreateDESKey(keyBytes, 7);
            var magicConstant = "KGS!@#$%".GetBytes("US-ASCII");
            var des = CipherUtilities.GetCipher("DES/ECB/NoPadding");
            des.Init(true, lowKey);
            var lowHash = des.DoFinal(magicConstant);
            des.Init(true, highKey);
            var highHash = des.DoFinal(magicConstant);
            var lmHash_Renamed = new byte[16];
            Array.Copy(lowHash, 0, lmHash_Renamed, 0, 8);
            Array.Copy(highHash, 0, lmHash_Renamed, 8, 8);
            return lmHash_Renamed;
        }

        /// <summary>
        /// Creates the NTLM Hash of the user's password.
        /// </summary>
        /// <param name="password"> The password.
        /// </param>
        /// <returns> The NTLM Hash of the given password, used in the calculation
        /// of the NTLM Response and the NTLMv2 and LMv2 Hashes. </returns>
        internal static byte[] NtlmHash(string password) {
            var unicodePassword = password.GetBytes("UnicodeLittleUnmarked");
            IDigest md4 = new MD4Digest();
            var ret = new byte[md4.GetDigestSize()];
            md4.BlockUpdate(unicodePassword, 0, unicodePassword.Length);
            md4.DoFinal(ret, 0);
            return ret;
        }

        /// <summary>
        /// Creates the NTLMv2 Hash of the user's password.
        /// </summary>
        /// <param name="target"> The authentication target (i.e., domain). </param>
        /// <param name="user"> The username. </param>
        /// <param name="password"> The password.
        /// </param>
        /// <returns> The NTLMv2 Hash, used in the calculation of the NTLMv2
        /// and LMv2 Responses.  </returns>
        internal static byte[] Ntlmv2Hash(string target, string user, string password) {
            var ntlmHash_Renamed = NtlmHash(password);
            var identity = user.ToUpper() + target;
            return HmacMD5(identity.GetBytes("UnicodeLittleUnmarked"), ntlmHash_Renamed);
        }

        /// <summary>
        /// Creates the LM Response from the given hash and Type 2 challenge.
        /// </summary>
        /// <param name="hash"> The LM or NTLM Hash. </param>
        /// <param name="challenge"> The server challenge from the Type 2 message.
        /// </param>
        /// <returns> The response (either LM or NTLM, depending on the provided
        /// hash). </returns>
        /// <exception cref="SharpCifs.Util.Sharpen.NoSuchAlgorithmException"> </exception>
        /// <exception cref="InvalidKeyException"> </exception>
        /// <exception cref="InvalidOperationException">  </exception>
        private static byte[] LmResponse(byte[] hash, byte[] challenge) {
            var keyBytes = new byte[21];
            Array.Copy(hash, 0, keyBytes, 0, 16);
            var lowKey = CreateDESKey(keyBytes, 0);
            var middleKey = CreateDESKey(keyBytes, 7);
            var highKey = CreateDESKey(keyBytes, 14);
            var des = CipherUtilities.GetCipher("DES/ECB/NoPadding");
            des.Init(true, lowKey);
            var lowResponse = des.DoFinal(challenge);
            des.Init(true, middleKey);
            var middleResponse = des.DoFinal(challenge);
            des.Init(true, highKey);
            var highResponse = des.DoFinal(challenge);
            var lmResponse_Renamed = new byte[24];
            Array.Copy(lowResponse, 0, lmResponse_Renamed, 0, 8);
            Array.Copy(middleResponse, 0, lmResponse_Renamed, 8, 8);
            Array.Copy(highResponse, 0, lmResponse_Renamed, 16, 8);
            return lmResponse_Renamed;
        }

        /// <summary>
        /// Creates the LMv2 Response from the given hash, client data, and
        /// Type 2 challenge.
        /// </summary>
        /// <param name="hash"> The NTLMv2 Hash. </param>
        /// <param name="clientData"> The client data (blob or client nonce). </param>
        /// <param name="challenge"> The server challenge from the Type 2 message.
        /// </param>
        /// <returns> The response (either NTLMv2 or LMv2, depending on the
        /// client data). </returns>
        private static byte[] Lmv2Response(byte[] hash, byte[] clientData, byte[] challenge) {
            var data = new byte[challenge.Length + clientData.Length];
            Array.Copy(challenge, 0, data, 0, challenge.Length);
            Array.Copy(clientData, 0, data, challenge.Length, clientData.Length);
            var mac = HmacMD5(data, hash);
            var lmv2Response_Renamed = new byte[mac.Length + clientData.Length];
            Array.Copy(mac, 0, lmv2Response_Renamed, 0, mac.Length);
            Array.Copy(clientData, 0, lmv2Response_Renamed, mac.Length, clientData.Length);
            return lmv2Response_Renamed;
        }

        /// <summary>
        /// Creates the NTLMv2 blob from the given target information block and
        /// client nonce.
        /// </summary>
        /// <param name="targetInformation"> The target information block from the Type 2
        /// message. </param>
        /// <param name="clientNonce"> The random 8-byte client nonce.
        /// </param>
        /// <returns> The blob, used in the calculation of the NTLMv2 Response. </returns>
        internal static byte[] CreateBlob(byte[] targetInformation, byte[] clientNonce) {
            byte[] blobSignature = { 0x01, 0x01, 0x00, 0x00 };
            byte[] reserved = { 0x00, 0x00, 0x00, 0x00 };
            byte[] unknown1 = { 0x00, 0x00, 0x00, 0x00 };
            byte[] unknown2 = { 0x00, 0x00, 0x00, 0x00 };
            var time = DateTimeHelperClass.CurrentUnixTimeMillis();
            time += 11644473600000L; // milliseconds from January 1, 1601 -> epoch.
            time *= 10000; // tenths of a microsecond.
                           // convert to little-endian byte array.
            var timestamp = new byte[8];
            for (var i = 0; i < 8; i++) {
                timestamp[i] = (byte)time;
                time = (long)((ulong)time >> 8);
            }
            var blob = new byte[blobSignature.Length + reserved.Length + timestamp.Length +
                clientNonce.Length + unknown1.Length + targetInformation.Length + unknown2.Length];
            var offset = 0;
            Array.Copy(blobSignature, 0, blob, offset, blobSignature.Length);
            offset += blobSignature.Length;
            Array.Copy(reserved, 0, blob, offset, reserved.Length);
            offset += reserved.Length;
            Array.Copy(timestamp, 0, blob, offset, timestamp.Length);
            offset += timestamp.Length;
            Array.Copy(clientNonce, 0, blob, offset, clientNonce.Length);
            offset += clientNonce.Length;
            Array.Copy(unknown1, 0, blob, offset, unknown1.Length);
            offset += unknown1.Length;
            Array.Copy(targetInformation, 0, blob, offset, targetInformation.Length);
            offset += targetInformation.Length;
            Array.Copy(unknown2, 0, blob, offset, unknown2.Length);
            return blob;
        }

        /// <summary>
        /// Calculates the HMAC-MD5 hash of the given data using the specified
        /// hashing key.
        /// </summary>
        /// <param name="data"> The data for which the hash will be calculated. </param>
        /// <param name="key"> The hashing key.
        /// </param>
        /// <exception cref="SharpCifs.Util.Sharpen.NoSuchAlgorithmException"> </exception>
        /// <returns> The HMAC-MD5 hash of the given data. </returns>
        internal static byte[] HmacMD5(byte[] data, byte[] key) {

            var hmac = new Hmact64(key);
            return hmac.Digest(data);

           //  var ipad = new byte[64];
           //  var opad = new byte[64];
           //  for (var i = 0; i < 64; i++) {
           //      ipad[i] = 0x36;
           //      opad[i] = 0x5c;
           //  }
           //  for (var i = key.Length - 1; i >= 0; i--) {
           //      ipad[i] ^= key[i];
           //      opad[i] ^= key[i];
           //  }
           //  var content = new byte[data.Length + 64];
           //  Array.Copy(ipad, 0, content, 0, 64);
           //  Array.Copy(data, 0, content, 64, data.Length);
           //  
           //  var md5 = DigestUtilities.GetDigest("MD5");
           //  data = md5.digest(content);
           //  content = new byte[data.Length + 64];
           //  Array.Copy(opad, 0, content, 0, 64);
           //  Array.Copy(data, 0, content, 64, data.Length);
           //  return md5.digest(content);
        }

        /// <summary>
        /// Creates a DES encryption key from the given key material.
        /// </summary>
        /// <param name="bytes"> A byte array containing the DES key material. </param>
        /// <param name="offset"> The offset in the given byte array at which
        /// the 7-byte key material starts.
        /// </param>
        /// <returns> A DES encryption key created from the key material
        /// starting at the specified offset in the given byte array. </returns>
        private static ICipherParameters CreateDESKey(byte[] bytes, int offset) {
            var keyBytes = new byte[7];
            Array.Copy(bytes, offset, keyBytes, 0, 7);
            var material = new byte[8];
            material[0] = keyBytes[0];
            material[1] = (byte)((keyBytes[0] << 7) | (int)((uint)(keyBytes[1] & 0xff) >> 1));
            material[2] = (byte)((keyBytes[1] << 6) | (int)((uint)(keyBytes[2] & 0xff) >> 2));
            material[3] = (byte)((keyBytes[2] << 5) | (int)((uint)(keyBytes[3] & 0xff) >> 3));
            material[4] = (byte)((keyBytes[3] << 4) | (int)((uint)(keyBytes[4] & 0xff) >> 4));
            material[5] = (byte)((keyBytes[4] << 3) | (int)((uint)(keyBytes[5] & 0xff) >> 5));
            material[6] = (byte)((keyBytes[5] << 2) | (int)((uint)(keyBytes[6] & 0xff) >> 6));
            material[7] = (byte)(keyBytes[6] << 1);
            OddParity(material);
            return new KeyParameter(material); //, "DES");
        }

        /// <summary>
        /// Applies odd parity to the given byte array.
        /// </summary>
        /// <param name="bytes"> The data whose parity bits are to be adjusted for
        /// odd parity. </param>
        private static void OddParity(byte[] bytes) {
            for (var i = 0; i < bytes.Length; i++) {
                var b = bytes[i];
                var needsParity = 
                    ((((int)((uint)b >> 7)) ^
                    ((int)((uint)b >> 6)) ^
                    ((int)((uint)b >> 5)) ^ 
                    ((int)((uint)b >> 4)) ^
                    ((int)((uint)b >> 3)) ^
                    ((int)((uint)b >> 2)) ^
                    ((int)((uint)b >> 1))) & 0x01) == 0;
                if (needsParity) {
                    bytes[i] |= 0x01;
                }
                else {
                    bytes[i] &= unchecked(0xfe);
                }
            }
        }
    }
}