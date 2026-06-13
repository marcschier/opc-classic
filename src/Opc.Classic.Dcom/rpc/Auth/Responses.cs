//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Extracted from http://davenport.sourceforge.net/ntlm.html
// Copyright (c) 2003, 2006 Eric Glass (eric.glass@gmail.com)
//

using Opc.Classic.Dcom.Crypto;
using System.Security.Cryptography;
using System.Text;

namespace Opc.Classic.Dcom.Rpc.Auth.ntlm;

/// <summary>
/// Calculates the various Type 3 responses.
/// </summary>
public static class Responses
{
    /// <summary>
    /// Calculates the LM Response for the given challenge, using the specified
    /// password.
    /// </summary>
    /// <param name="password"> The user's password. </param>
    /// <param name="challenge"> The Type 2 challenge from the server.
    /// </param>
    /// <returns> The LM Response. </returns>
    public static byte[] GetLMResponse(string password, byte[] challenge)
    {
        var lmHash_Renamed = LmHash(password);
        try
        {
            return LmResponse(lmHash_Renamed, challenge);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(lmHash_Renamed);
        }
    }

    /// <summary>
    /// Calculates the NTLM Response for the given challenge, using the
    /// specified password.
    /// </summary>
    /// <param name="password"> The user's password. </param>
    /// <param name="challenge"> The Type 2 challenge from the server.
    /// </param>
    /// <returns> The NTLM Response. </returns>
    public static byte[] GetNTLMResponse(string password, byte[] challenge)
    {
        var ntlmHash_Renamed = NtlmHash(password);
        try
        {
            return LmResponse(ntlmHash_Renamed, challenge);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(ntlmHash_Renamed);
        }
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
        byte[] targetInformation, byte[] challenge, byte[] clientNonce)
    {
        var retval = new byte[2][];
        var ntlmv2Hash_Renamed = Ntlmv2Hash(target, user, password);
        try
        {
            var blob = CreateBlob(targetInformation, clientNonce);
            retval[1] = blob;
            retval[0] = Lmv2Response(ntlmv2Hash_Renamed, blob, challenge);
            return retval;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(ntlmv2Hash_Renamed);
        }
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
    public static byte[] GetLMv2Response(string target, string user, string password,
        byte[] challenge, byte[] clientNonce)
    {
        var ntlmv2Hash_Renamed = Ntlmv2Hash(target, user, password);
        try
        {
            return Lmv2Response(ntlmv2Hash_Renamed, clientNonce, challenge);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(ntlmv2Hash_Renamed);
        }
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
    /// <exception cref="Opc.Classic.Dcom.Common.Ntlm.NoSuchAlgorithmException">Thrown when the requested NTLM cryptographic algorithm is not available.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the object is not in the state required to perform the operation.</exception>
    /// <exception cref="InvalidKeyException">Thrown when the get ntlm2 session response operation cannot be completed.</exception>
    public static byte[] GetNTLM2SessionResponse(string password,
        byte[] challenge, byte[] clientNonce)
    {
        var hash = NtlmHash(password);
        var digest = Array.Empty<byte>();
        var sessionHash = Array.Empty<byte>();
        try
        {
            var md5 = DigestUtilities.GetDigest("MD5");
            md5.BlockUpdate(challenge, 0, challenge.Length);
            md5.BlockUpdate(clientNonce, 0, clientNonce.Length);
            digest = new byte[md5.GetDigestSize()];
            md5.DoFinal(digest, 0);
            sessionHash = new byte[8];
            Array.Copy(digest, 0, sessionHash, 0, sessionHash.Length);
            return LmResponse(hash, sessionHash);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(hash);
            CryptographicOperations.ZeroMemory(digest);
            CryptographicOperations.ZeroMemory(sessionHash);
        }
    }

    /// <summary>
    /// Creates the LM Hash of the user's password.
    /// </summary>
    /// <param name="password"> The password.
    /// </param>
    /// <returns> The LM Hash of the given password, used in the calculation
    /// of the LM Response. </returns>
    private static byte[] LmHash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        byte[]? oemPassword = null;
        var oemLength = 0;
        Span<byte> keyBytes = stackalloc byte[14];
        byte[]? lowHash = null;
        byte[]? highHash = null;
        try
        {
            oemLength = Math.Min(password.Length, keyBytes.Length);
            oemPassword = SensitiveBufferPool.Rent(oemLength);
            for (var i = 0; i < oemLength; i++)
            {
                var ch = char.ToUpperInvariant(password[i]);
                oemPassword[i] = ch <= 0x7F ? (byte)ch : (byte)'?';
            }
            oemPassword.AsSpan(0, oemLength).CopyTo(keyBytes);

            var lowKey = CreateDESKey(keyBytes, 0);
            var highKey = CreateDESKey(keyBytes, 7);
            var magicConstant = Encoding.ASCII.GetBytes("KGS!@#$%");
            using var des = CipherUtilities.GetCipher("DES/ECB/NoPadding");
            des.Init(true, lowKey);
            CryptographicOperations.ZeroMemory(lowKey.Key);
            lowHash = des.DoFinal(magicConstant);
            des.Init(true, highKey);
            CryptographicOperations.ZeroMemory(highKey.Key);
            highHash = des.DoFinal(magicConstant);
            var lmHash_Renamed = new byte[16];
            Array.Copy(lowHash, 0, lmHash_Renamed, 0, 8);
            Array.Copy(highHash, 0, lmHash_Renamed, 8, 8);
            return lmHash_Renamed;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(keyBytes);
            CryptographicOperations.ZeroMemory(lowHash);
            CryptographicOperations.ZeroMemory(highHash);
            SensitiveBufferPool.Return("lm-password-oem", oemPassword, oemLength);
        }
    }

    /// <summary>
    /// Creates the NTLM Hash of the user's password.
    /// </summary>
    /// <param name="password"> The password.
    /// </param>
    /// <returns> The NTLM Hash of the given password, used in the calculation
    /// of the NTLM Response and the NTLMv2 and LMv2 Hashes. </returns>
    internal static byte[] NtlmHash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        byte[]? unicodePassword = null;
        var bytesWritten = 0;
        try
        {
            var byteCount = Encoding.Unicode.GetByteCount(password);
            unicodePassword = SensitiveBufferPool.Rent(byteCount);
            bytesWritten = Encoding.Unicode.GetBytes(password.AsSpan(), unicodePassword.AsSpan(0, byteCount));
            var md4 = new MD4Digest();
            var ret = new byte[md4.GetDigestSize()];
            md4.BlockUpdate(unicodePassword, 0, bytesWritten);
            md4.DoFinal(ret, 0);
            return ret;
        }
        finally
        {
            SensitiveBufferPool.Return("ntlm-password-unicode", unicodePassword, bytesWritten);
        }
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
    internal static byte[] Ntlmv2Hash(string target, string user, string password)
    {
        var ntlmHash_Renamed = NtlmHash(password);
        var identity = user.ToUpperInvariant() + target;
        byte[]? identityBytes = null;
        var bytesWritten = 0;
        try
        {
            var byteCount = Encoding.Unicode.GetByteCount(identity);
            identityBytes = SensitiveBufferPool.Rent(byteCount);
            bytesWritten = Encoding.Unicode.GetBytes(identity.AsSpan(), identityBytes.AsSpan(0, byteCount));
            return HmacMD5(identityBytes.AsSpan(0, bytesWritten), ntlmHash_Renamed);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(ntlmHash_Renamed);
            SensitiveBufferPool.Return("ntlmv2-identity-unicode", identityBytes, bytesWritten);
        }
    }

    /// <summary>
    /// Creates the LM Response from the given hash and Type 2 challenge.
    /// </summary>
    /// <param name="hash"> The LM or NTLM Hash. </param>
    /// <param name="challenge"> The server challenge from the Type 2 message.
    /// </param>
    /// <returns> The response (either LM or NTLM, depending on the provided
    /// hash). </returns>
    /// <exception cref="Opc.Classic.Dcom.Common.Ntlm.NoSuchAlgorithmException">Thrown when the requested NTLM cryptographic algorithm is not available.</exception>
    /// <exception cref="InvalidKeyException">Thrown when the lm response operation cannot be completed.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the object is not in the state required to perform the operation.</exception>
    private static byte[] LmResponse(byte[] hash, byte[] challenge)
    {
        byte[]? keyBytes = null;
        byte[]? lowResponse = null;
        byte[]? middleResponse = null;
        byte[]? highResponse = null;
        try
        {
            keyBytes = SensitiveBufferPool.Rent(21);
            keyBytes.AsSpan(0, 21).Clear();
            Array.Copy(hash, 0, keyBytes, 0, Math.Min(hash.Length, 16));
            var lowKey = CreateDESKey(keyBytes.AsSpan(0, 21), 0);
            var middleKey = CreateDESKey(keyBytes.AsSpan(0, 21), 7);
            var highKey = CreateDESKey(keyBytes.AsSpan(0, 21), 14);
            using var des = CipherUtilities.GetCipher("DES/ECB/NoPadding");
            des.Init(true, lowKey);
            CryptographicOperations.ZeroMemory(lowKey.Key);
            lowResponse = des.DoFinal(challenge);
            des.Init(true, middleKey);
            CryptographicOperations.ZeroMemory(middleKey.Key);
            middleResponse = des.DoFinal(challenge);
            des.Init(true, highKey);
            CryptographicOperations.ZeroMemory(highKey.Key);
            highResponse = des.DoFinal(challenge);
            var lmResponse_Renamed = new byte[24];
            Array.Copy(lowResponse, 0, lmResponse_Renamed, 0, 8);
            Array.Copy(middleResponse, 0, lmResponse_Renamed, 8, 8);
            Array.Copy(highResponse, 0, lmResponse_Renamed, 16, 8);
            return lmResponse_Renamed;
        }
        finally
        {
            SensitiveBufferPool.Return("lm-response-key-material", keyBytes, 21);
            CryptographicOperations.ZeroMemory(lowResponse);
            CryptographicOperations.ZeroMemory(middleResponse);
            CryptographicOperations.ZeroMemory(highResponse);
        }
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
    private static byte[] Lmv2Response(byte[] hash, byte[] clientData, byte[] challenge)
    {
        var data = new byte[challenge.Length + clientData.Length];
        byte[]? mac = null;
        try
        {
            Array.Copy(challenge, 0, data, 0, challenge.Length);
            Array.Copy(clientData, 0, data, challenge.Length, clientData.Length);
            mac = HmacMD5(data, hash);
            var lmv2Response_Renamed = new byte[mac.Length + clientData.Length];
            Array.Copy(mac, 0, lmv2Response_Renamed, 0, mac.Length);
            Array.Copy(clientData, 0, lmv2Response_Renamed, mac.Length, clientData.Length);
            return lmv2Response_Renamed;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(data);
            CryptographicOperations.ZeroMemory(mac);
        }
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
    internal static byte[] CreateBlob(byte[] targetInformation, byte[] clientNonce)
    {
        byte[] blobSignature = { 0x01, 0x01, 0x00, 0x00 };
        byte[] reserved = { 0x00, 0x00, 0x00, 0x00 };
        byte[] unknown1 = { 0x00, 0x00, 0x00, 0x00 };
        byte[] unknown2 = { 0x00, 0x00, 0x00, 0x00 };
        var time = DateTime.UtcNow.ToFileTimeUtc();
        //var time = DateTimeHelperClass.CurrentUnixTimeMillis();
        //time += 11644473600000L; // milliseconds from January 1, 1601 -> epoch.
        //time *= 10000; // tenths of a microsecond.
        //               // convert to little-endian byte array.
        var timestamp = new byte[8];
        for (var i = 0; i < 8; i++)
        {
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
    /// <exception cref="Opc.Classic.Dcom.Common.Ntlm.NoSuchAlgorithmException">Thrown when the requested NTLM cryptographic algorithm is not available.</exception>
    /// <returns> The HMAC-MD5 hash of the given data. </returns>
#pragma warning disable CA5351 // NTLM requires HMAC-MD5 per [MS-NLMP].
    internal static byte[] HmacMD5(byte[] data, byte[] key) => HmacMD5(data.AsSpan(), key.AsSpan());
    internal static byte[] HmacMD5(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key) => HMACMD5.HashData(key, data);
#pragma warning restore CA5351

    /// <summary>
    /// Creates a DES encryption key from the given key material.
    /// </summary>
    /// <param name="bytes">A span containing the DES key material.</param>
    /// <param name="offset"> The offset in the given span at which
    /// the 7-byte key material starts.
    /// </param>
    /// <returns> A DES encryption key created from the key material
    /// starting at the specified offset in the given span. </returns>
    private static KeyParameter CreateDESKey(ReadOnlySpan<byte> bytes, int offset)
    {
        var material = new byte[8];
        try
        {
            material[0] = bytes[offset];
            material[1] = (byte)((bytes[offset] << 7) | (int)((uint)(bytes[offset + 1] & 0xff) >> 1));
            material[2] = (byte)((bytes[offset + 1] << 6) | (int)((uint)(bytes[offset + 2] & 0xff) >> 2));
            material[3] = (byte)((bytes[offset + 2] << 5) | (int)((uint)(bytes[offset + 3] & 0xff) >> 3));
            material[4] = (byte)((bytes[offset + 3] << 4) | (int)((uint)(bytes[offset + 4] & 0xff) >> 4));
            material[5] = (byte)((bytes[offset + 4] << 3) | (int)((uint)(bytes[offset + 5] & 0xff) >> 5));
            material[6] = (byte)((bytes[offset + 5] << 2) | (int)((uint)(bytes[offset + 6] & 0xff) >> 6));
            material[7] = (byte)(bytes[offset + 6] << 1);
            OddParity(material);
            return new KeyParameter(material);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(material);
        }
    }

    /// <summary>
    /// Applies odd parity to the given byte array.
    /// </summary>
    /// <param name="bytes"> The data whose parity bits are to be adjusted for
    /// odd parity. </param>
    private static void OddParity(Span<byte> bytes)
    {
        for (var i = 0; i < bytes.Length; i++)
        {
            var b = bytes[i];
            var needsParity =
                ((((int)((uint)b >> 7)) ^
                ((int)((uint)b >> 6)) ^
                ((int)((uint)b >> 5)) ^
                ((int)((uint)b >> 4)) ^
                ((int)((uint)b >> 3)) ^
                ((int)((uint)b >> 2)) ^
                ((int)((uint)b >> 1))) & 0x01) == 0;
            if (needsParity)
            {
                bytes[i] |= 0x01;
            }
            else
            {
                bytes[i] &= unchecked(0xfe);
            }
        }
    }
}
