using System;

/// <summary>
/// Extracted from http://davenport.sourceforge.net/ntlm.html
/// Copyright � 2003, 2006 Eric Glass (eric.glass@gmail.com) 
/// 
/// </summary>
namespace rpc.security.ntlm {



	//import gnu.crypto.hash.MD4;



	using Digest = org.bouncycastle.crypto.Digest;
	using MD4Digest = org.bouncycastle.crypto.digests.MD4Digest;

	/// <summary>
	/// Calculates the various Type 3 responses.
	/// </summary>
	public class Responses {

		/// <summary>
		/// Calculates the LM Response for the given challenge, using the specified
		/// password.
		/// </summary>
		/// <param name="password"> The user's password. </param>
		/// <param name="challenge"> The Type 2 challenge from the server.
		/// </param>
		/// <returns> The LM Response. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static byte[] getLMResponse(String password, byte[] challenge) throws Exception
		public static sbyte[] GetLMResponse(string password, sbyte[] challenge) {
			sbyte[] lmHash = LmHash(password);
			return LmResponse(lmHash, challenge);
		}

		/// <summary>
		/// Calculates the NTLM Response for the given challenge, using the
		/// specified password.
		/// </summary>
		/// <param name="password"> The user's password. </param>
		/// <param name="challenge"> The Type 2 challenge from the server.
		/// </param>
		/// <returns> The NTLM Response. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static byte[] getNTLMResponse(String password, byte[] challenge) throws Exception
		public static sbyte[] GetNTLMResponse(string password, sbyte[] challenge) {
			sbyte[] ntlmHash = NtlmHash(password);
			return LmResponse(ntlmHash, challenge);
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static byte[][] getNTLMv2Response(String target, String user, String password, byte[] targetInformation, byte[] challenge, byte[] clientNonce) throws Exception
		public static sbyte[][] GetNTLMv2Response(string target, string user, string password, sbyte[] targetInformation, sbyte[] challenge, sbyte[] clientNonce) {
			sbyte[][] retval = new sbyte[2][];
			sbyte[] ntlmv2Hash = Ntlmv2Hash(target, user, password);
			sbyte[] blob = CreateBlob(targetInformation, clientNonce);
			retval[1] = blob;
			retval[0] = Lmv2Response(ntlmv2Hash, blob, challenge);
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static byte[] getLMv2Response(String target, String user, String password, byte[] challenge, byte[] clientNonce) throws Exception
		public static sbyte[] GetLMv2Response(string target, string user, string password, sbyte[] challenge, sbyte[] clientNonce) {
			sbyte[] ntlmv2Hash = Ntlmv2Hash(target, user, password);
			return Lmv2Response(ntlmv2Hash, clientNonce, challenge);
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
		/// <exception cref="UnsupportedEncodingException"> </exception>
		/// <exception cref="NoSuchAlgorithmException"> </exception>
		/// <exception cref="BadPaddingException"> </exception>
		/// <exception cref="IllegalBlockSizeException"> </exception>
		/// <exception cref="IllegalStateException"> </exception>
		/// <exception cref="NoSuchPaddingException"> </exception>
		/// <exception cref="InvalidKeyException">  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static byte[] getNTLM2SessionResponse(String password, byte[] challenge, byte[] clientNonce) throws java.io.UnsupportedEncodingException, java.security.NoSuchAlgorithmException, java.security.InvalidKeyException, javax.crypto.NoSuchPaddingException, IllegalStateException, javax.crypto.IllegalBlockSizeException, javax.crypto.BadPaddingException
		public static sbyte[] GetNTLM2SessionResponse(string password, sbyte[] challenge, sbyte[] clientNonce) {
			sbyte[] ntlmHash = NtlmHash(password);
			MessageDigest md5 = MessageDigest.getInstance("MD5");
			md5.update(challenge);
			md5.update(clientNonce);
			sbyte[] sessionHash = new sbyte[8];
			Array.Copy(md5.digest(), 0, sessionHash, 0, 8);
			return LmResponse(ntlmHash, sessionHash);
		}

		/// <summary>
		/// Creates the LM Hash of the user's password.
		/// </summary>
		/// <param name="password"> The password.
		/// </param>
		/// <returns> The LM Hash of the given password, used in the calculation
		/// of the LM Response. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static byte[] lmHash(String password) throws Exception
		private static sbyte[] LmHash(string password) {
			sbyte[] oemPassword = password.ToUpper().GetBytes("US-ASCII");
			int length = Math.Min(oemPassword.Length, 14);
			sbyte[] keyBytes = new sbyte[14];
			Array.Copy(oemPassword, 0, keyBytes, 0, length);
			Key lowKey = CreateDESKey(keyBytes, 0);
			Key highKey = CreateDESKey(keyBytes, 7);
			sbyte[] magicConstant = "KGS!@#$%".GetBytes("US-ASCII");
			Cipher des = Cipher.getInstance("DES/ECB/NoPadding");
			des.init(Cipher.ENCRYPT_MODE, lowKey);
			sbyte[] lowHash = des.doFinal(magicConstant);
			des.init(Cipher.ENCRYPT_MODE, highKey);
			sbyte[] highHash = des.doFinal(magicConstant);
			sbyte[] lmHash = new sbyte[16];
			Array.Copy(lowHash, 0, lmHash, 0, 8);
			Array.Copy(highHash, 0, lmHash, 8, 8);
			return lmHash;
		}

		/// <summary>
		/// Creates the NTLM Hash of the user's password.
		/// </summary>
		/// <param name="password"> The password.
		/// </param>
		/// <returns> The NTLM Hash of the given password, used in the calculation
		/// of the NTLM Response and the NTLMv2 and LMv2 Hashes. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static byte[] ntlmHash(String password) throws java.io.UnsupportedEncodingException
		internal static sbyte[] NtlmHash(string password) {
			sbyte[] unicodePassword = password.GetBytes("UnicodeLittleUnmarked");
	//        MD4 md4 = new MD4();
	//        md4.update(unicodePassword,0,unicodePassword.length);
	//        return md4.digest();
			  Digest md4 = new MD4Digest();
			  sbyte[] ret = new sbyte[md4.DigestSize];
			  md4.Update(unicodePassword,0,unicodePassword.Length);
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static byte[] ntlmv2Hash(String target, String user, String password) throws Exception
		 internal static sbyte[] Ntlmv2Hash(string target, string user, string password) {
			sbyte[] ntlmHash = NtlmHash(password);
			string identity = user.ToUpper() + target;
			return HmacMD5(identity.GetBytes("UnicodeLittleUnmarked"), ntlmHash);
		 }



		/// <summary>
		/// Creates the LM Response from the given hash and Type 2 challenge.
		/// </summary>
		/// <param name="hash"> The LM or NTLM Hash. </param>
		/// <param name="challenge"> The server challenge from the Type 2 message.
		/// </param>
		/// <returns> The response (either LM or NTLM, depending on the provided
		/// hash). </returns>
		/// <exception cref="NoSuchPaddingException"> </exception>
		/// <exception cref="NoSuchAlgorithmException"> </exception>
		/// <exception cref="InvalidKeyException"> </exception>
		/// <exception cref="BadPaddingException"> </exception>
		/// <exception cref="IllegalBlockSizeException"> </exception>
		/// <exception cref="IllegalStateException">  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static byte[] lmResponse(byte[] hash, byte[] challenge) throws java.security.NoSuchAlgorithmException, javax.crypto.NoSuchPaddingException, java.security.InvalidKeyException, IllegalStateException, javax.crypto.IllegalBlockSizeException, javax.crypto.BadPaddingException
		private static sbyte[] LmResponse(sbyte[] hash, sbyte[] challenge) {
			sbyte[] keyBytes = new sbyte[21];
			Array.Copy(hash, 0, keyBytes, 0, 16);
			Key lowKey = CreateDESKey(keyBytes, 0);
			Key middleKey = CreateDESKey(keyBytes, 7);
			Key highKey = CreateDESKey(keyBytes, 14);
			Cipher des = Cipher.getInstance("DES/ECB/NoPadding");
			des.init(Cipher.ENCRYPT_MODE, lowKey);
			sbyte[] lowResponse = des.doFinal(challenge);
			des.init(Cipher.ENCRYPT_MODE, middleKey);
			sbyte[] middleResponse = des.doFinal(challenge);
			des.init(Cipher.ENCRYPT_MODE, highKey);
			sbyte[] highResponse = des.doFinal(challenge);
			sbyte[] lmResponse = new sbyte[24];
			Array.Copy(lowResponse, 0, lmResponse, 0, 8);
			Array.Copy(middleResponse, 0, lmResponse, 8, 8);
			Array.Copy(highResponse, 0, lmResponse, 16, 8);
			return lmResponse;
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
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static byte[] lmv2Response(byte[] hash, byte[] clientData, byte[] challenge) throws Exception
		private static sbyte[] Lmv2Response(sbyte[] hash, sbyte[] clientData, sbyte[] challenge) {
			sbyte[] data = new sbyte[challenge.Length + clientData.Length];
			Array.Copy(challenge, 0, data, 0, challenge.Length);
			Array.Copy(clientData, 0, data, challenge.Length, clientData.Length);
			sbyte[] mac = HmacMD5(data, hash);
			sbyte[] lmv2Response = new sbyte[mac.Length + clientData.Length];
			Array.Copy(mac, 0, lmv2Response, 0, mac.Length);
			Array.Copy(clientData, 0, lmv2Response, mac.Length, clientData.Length);
			return lmv2Response;
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
		internal static sbyte[] CreateBlob(sbyte[] targetInformation, sbyte[] clientNonce) {
			sbyte[] blobSignature = new sbyte[] { (sbyte) 0x01, (sbyte) 0x01, (sbyte) 0x00, (sbyte) 0x00 };
			sbyte[] reserved = new sbyte[] { (sbyte) 0x00, (sbyte) 0x00, (sbyte) 0x00, (sbyte) 0x00 };
			sbyte[] unknown1 = new sbyte[] { (sbyte) 0x00, (sbyte) 0x00, (sbyte) 0x00, (sbyte) 0x00 };
			sbyte[] unknown2 = new sbyte[] { (sbyte) 0x00, (sbyte) 0x00, (sbyte) 0x00, (sbyte) 0x00 };
			long time = DateTimeHelperClass.CurrentUnixTimeMillis();
			time += 11644473600000l; // milliseconds from January 1, 1601 -> epoch.
			time *= 10000; // tenths of a microsecond.
			// convert to little-endian byte array.
			sbyte[] timestamp = new sbyte[8];
			for (int i = 0; i < 8; i++) {
				timestamp[i] = (sbyte) time;
				time = (long)((ulong)time >> 8);
			}
			sbyte[] blob = new sbyte[blobSignature.Length + reserved.Length + timestamp.Length + clientNonce.Length + unknown1.Length + targetInformation.Length + unknown2.Length];
			int offset = 0;
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
		/// <returns> The HMAC-MD5 hash of the given data. </returns>
		/// <exception cref="NoSuchAlgorithmException">  </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: static byte[] hmacMD5(byte[] data, byte[] key) throws java.security.NoSuchAlgorithmException
		internal static sbyte[] HmacMD5(sbyte[] data, sbyte[] key) {
			sbyte[] ipad = new sbyte[64];
			sbyte[] opad = new sbyte[64];
			for (int i = 0; i < 64; i++) {
				ipad[i] = (sbyte) 0x36;
				opad[i] = (sbyte) 0x5c;
			}
			for (int i = key.Length - 1; i >= 0; i--) {
				ipad[i] ^= key[i];
				opad[i] ^= key[i];
			}
			sbyte[] content = new sbyte[data.Length + 64];
			Array.Copy(ipad, 0, content, 0, 64);
			Array.Copy(data, 0, content, 64, data.Length);
			MessageDigest md5 = MessageDigest.getInstance("MD5");
			data = md5.digest(content);
			content = new sbyte[data.Length + 64];
			Array.Copy(opad, 0, content, 0, 64);
			Array.Copy(data, 0, content, 64, data.Length);
			return md5.digest(content);
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
		private static Key CreateDESKey(sbyte[] bytes, int offset) {
			sbyte[] keyBytes = new sbyte[7];
			Array.Copy(bytes, offset, keyBytes, 0, 7);
			sbyte[] material = new sbyte[8];
			material[0] = keyBytes[0];
			material[1] = (sbyte)(keyBytes[0] << 7 | (int)((uint)(keyBytes[1] & 0xff) >> 1));
			material[2] = (sbyte)(keyBytes[1] << 6 | (int)((uint)(keyBytes[2] & 0xff) >> 2));
			material[3] = (sbyte)(keyBytes[2] << 5 | (int)((uint)(keyBytes[3] & 0xff) >> 3));
			material[4] = (sbyte)(keyBytes[3] << 4 | (int)((uint)(keyBytes[4] & 0xff) >> 4));
			material[5] = (sbyte)(keyBytes[4] << 3 | (int)((uint)(keyBytes[5] & 0xff) >> 5));
			material[6] = (sbyte)(keyBytes[5] << 2 | (int)((uint)(keyBytes[6] & 0xff) >> 6));
			material[7] = (sbyte)(keyBytes[6] << 1);
			OddParity(material);
			return new SecretKeySpec(material, "DES");
		}

		/// <summary>
		/// Applies odd parity to the given byte array.
		/// </summary>
		/// <param name="bytes"> The data whose parity bits are to be adjusted for
		/// odd parity. </param>
		private static void OddParity(sbyte[] bytes) {
			for (int i = 0; i < bytes.Length; i++) {
				sbyte b = bytes[i];
				bool needsParity = ((((int)((uint)b >> 7)) ^ ((int)((uint)b >> 6)) ^ ((int)((uint)b >> 5)) ^ ((int)((uint)b >> 4)) ^ ((int)((uint)b >> 3)) ^ ((int)((uint)b >> 2)) ^ ((int)((uint)b >> 1))) & 0x01) == 0;
				if (needsParity) {
					bytes[i] |= (sbyte) 0x01;
				}
				else {
					bytes[i] &= unchecked((sbyte) 0xfe);
				}
			}
		}

	}
}