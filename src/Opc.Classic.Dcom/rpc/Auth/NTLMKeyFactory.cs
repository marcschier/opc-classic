// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Crypto;
using Opc.Classic.Dcom.Internal.Ntlm;
using Opc.Classic.Dcom.Common.Ntlm;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Opc.Classic.Dcom.Rpc.Auth.ntlm;

/// <summary>
/// Key factory for lan manager
/// </summary>
internal sealed class NTLMKeyFactory
{

    /// <summary>
    /// Get user session key
    /// </summary>
    /// <param name="target">Target object or buffer that receives the operation result.</param>
    /// <param name="user">User name or account principal used for authentication.</param>
    /// <param name="password">Password used for the NTLM or Kerberos handshake.</param>
    /// <param name="challenge">Wire-format bytes consumed or produced by the operation.</param>
    /// <param name="blob">Wire-format bytes consumed or produced by the operation.</param>
    /// <returns>The sequence of ntlmv2 user session key values produced by the operation.</returns>
    public byte[] GetNTLMv2UserSessionKey(string target, string user,
        string password, byte[] challenge, byte[] blob)
    {
        var ntlm2Hash = Responses.Ntlmv2Hash(target, user, password);
        byte[]? data = null;
        byte[]? mac = null;
        try
        {
            data = new byte[challenge.Length + blob.Length];
            Array.Copy(challenge, 0, data, 0, challenge.Length);
            Array.Copy(blob, 0, data, challenge.Length, blob.Length);
            mac = Responses.HmacMD5(data, ntlm2Hash);
            return Responses.HmacMD5(mac, ntlm2Hash);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(ntlm2Hash);
            CryptographicOperations.ZeroMemory(data);
            CryptographicOperations.ZeroMemory(mac);
        }
    }

    /// <summary>
    /// Password of the user
    /// </summary>
    /// <param name="password">Password used for the NTLM or Kerberos handshake.</param>
    /// <param name="servernonce"> challenge + nonce from NTLM2 Session Response
    /// </param>
    /// <exception cref="SecurityUtilityException">Thrown when the get ntlm2 session response user session key operation cannot be completed.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="password"/> is not valid for this operation.</exception>
    /// <exception cref="Opc.Classic.Dcom.Common.Ntlm.NoSuchAlgorithmException">Thrown when the requested NTLM cryptographic algorithm is not available.</exception>
    public byte[] GetNTLM2SessionResponseUserSessionKey(string password, byte[] servernonce)
    {
        var userSessionKey = GetNTLMUserSessionKey(password);
        try
        {
            return Responses.HmacMD5(servernonce, userSessionKey);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(userSessionKey);
        }
    }

    /// <summary>
    /// Randomly generated 16 bytes
    /// </summary>
    public byte[] SecondarySessionKey
    {
        get
        {
            var key = new byte[16];
            _random.NextBytes(key);
            return key;
        }
    }

    /// <summary>
    /// Get stream cipher
    /// </summary>
    /// <param name="key">Lookup key used to select the value from the collection.</param>
    /// <returns>The requested arcfour value.</returns>
    public IStreamCipher GetARCFOUR(byte[] key)
    {
        var keystream = new RC4Engine();
        var parameters = new KeyParameter(key);
        keystream.Init(true, parameters);
        return keystream;
    }

    /// <summary>
    /// Apply stream cypher
    /// </summary>
    /// <param name="keystream">Stream used to read or write the wire-format data.</param>
    /// <param name="data">Wire-format payload bytes to process.</param>
    /// <returns>The sequence of apply arcfour values produced by the operation.</returns>
    internal byte[] ApplyARCFOUR(IStreamCipher keystream, byte[] data)
    {
        var retData = new byte[data.Length];
        keystream.ProcessBytes(data, 0, data.Length, retData, 0);
        return retData;
    }

    /// <summary>
    /// NTLMv1 User Session Key. Cases where LMcompatibilitylevel is 0,1,2.
    /// For 3,4,5 the logic is different and based upon the reponses being
    /// sent back (either LMv2 or NTLMv2)
    /// </summary>
    /// <param name="password">
    /// </param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="password"/> is not valid for this operation.</exception>
    /// <exception cref="SecurityUtilityException">Thrown when the get ntlmuser session key operation cannot be completed.</exception>
    private byte[] GetNTLMUserSessionKey(string password)
    {
        // The old Opc.Classic.Dcom.Common.Ntlm credential helper supported only
        // the NTLMUserSessionKey and the LMv2UserSessionKey...we need more :(
        //         byte key[] = new byte[16];
        var ntlmHash = Responses.NtlmHash(password);
        try
        {
            var md4 = new MD4Digest();
            var ret = new byte[md4.GetDigestSize()];
            md4.BlockUpdate(ntlmHash, 0, ntlmHash.Length);
            md4.DoFinal(ret, 0);
            return ret;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(ntlmHash);
        }
    }

    /// <summary>
    /// Decrypt
    /// </summary>
    /// <param name="encryptedData">Wire-format bytes consumed or produced by the operation.</param>
    /// <param name="key">Lookup key used to select the value from the collection.</param>
    /// <returns>The sequence of decrypt secondary session key values produced by the operation.</returns>
    public byte[] DecryptSecondarySessionKey(byte[] encryptedData, byte[] key) =>
        ApplyARCFOUR(GetARCFOUR(key), encryptedData);

    /// <summary>
    /// Encrypt
    /// </summary>
    /// <param name="plainData">Wire-format bytes consumed or produced by the operation.</param>
    /// <param name="key">Lookup key used to select the value from the collection.</param>
    /// <returns>The sequence of encrypt secondary session key values produced by the operation.</returns>
    public byte[] EncryptSecondarySessionKey(byte[] plainData, byte[] key) =>
        ApplyARCFOUR(GetARCFOUR(key), plainData);

    /// <summary>
    /// Generate client signing key
    /// </summary>
    /// <param name="secondarySessionKey">Secondary session key used to sign or seal NTLM messages.</param>
    /// <returns>The sequence of generate client signing key using negotiated secondary session key values produced by the operation.</returns>
    public byte[] GenerateClientSigningKeyUsingNegotiatedSecondarySessionKey(
        byte[] secondarySessionKey) => GenerateExtendedSessionSecurityKey(secondarySessionKey, kClientSigningMagicConstant);

    public byte[] GenerateClientSigningKey(NtlmFlags flags, byte[] exportedSessionKey) =>
        GenerateSigningKey(flags, exportedSessionKey, kClientSigningMagicConstant);

    /// <summary>
    /// Generate sealing key
    /// </summary>
    /// <param name="secondarySessionKey">Secondary session key used to sign or seal NTLM messages.</param>
    /// <returns>The sequence of generate client sealing key using negotiated secondary session key values produced by the operation.</returns>
    public byte[] GenerateClientSealingKeyUsingNegotiatedSecondarySessionKey(
        byte[] secondarySessionKey) => GenerateExtendedSessionSecurityKey(secondarySessionKey, kClientSealingMagicConstant);

    public byte[] GenerateClientSealingKey(NtlmFlags flags, byte[] exportedSessionKey) =>
        GenerateSealingKey(flags, exportedSessionKey, kClientSealingMagicConstant);

    /// <summary>
    /// Generate server signing key
    /// </summary>
    /// <param name="secondarySessionKey">Secondary session key used to sign or seal NTLM messages.</param>
    /// <returns>The sequence of generate server signing key using negotiated secondary session key values produced by the operation.</returns>
    public byte[] GenerateServerSigningKeyUsingNegotiatedSecondarySessionKey(
        byte[] secondarySessionKey) => GenerateExtendedSessionSecurityKey(secondarySessionKey, kServerSigningMagicConstant);

    public byte[] GenerateServerSigningKey(NtlmFlags flags, byte[] exportedSessionKey) =>
        GenerateSigningKey(flags, exportedSessionKey, kServerSigningMagicConstant);

    /// <summary>
    /// Generate server sealing key
    /// </summary>
    /// <param name="secondarySessionKey">Secondary session key used to sign or seal NTLM messages.</param>
    /// <returns>The sequence of generate server sealing key using negotiated secondary session key values produced by the operation.</returns>
    public byte[] GenerateServerSealingKeyUsingNegotiatedSecondarySessionKey(
        byte[] secondarySessionKey) => GenerateExtendedSessionSecurityKey(secondarySessionKey, kServerSealingMagicConstant);

    public byte[] GenerateServerSealingKey(NtlmFlags flags, byte[] exportedSessionKey) =>
        GenerateSealingKey(flags, exportedSessionKey, kServerSealingMagicConstant);

    /// <summary>
    /// Signing part 1
    /// </summary>
    /// <param name="sequenceNumber">Sequence number used when signing or verifying the message.</param>
    /// <param name="signingKey">Lookup key used to identify the cached or serialized value.</param>
    /// <param name="data">Wire-format payload bytes to process.</param>
    /// <param name="lengthOfBuffer">Length in bytes of the buffer that contains the verifier or payload.</param>
    /// <exception cref="Opc.Classic.Dcom.Common.Ntlm.NoSuchAlgorithmException">Thrown when the requested NTLM cryptographic algorithm is not available.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the object is not in the state required to perform the operation.</exception>
    /// <returns>The sequence of signing pt1 values produced by the operation.</returns>
    public byte[] SigningPt1(int sequenceNumber, byte[] signingKey,
        byte[] data, int lengthOfBuffer)
    {
        // TODO merge the signing routine for both client and server all that
        // they differ by are keys...as expected
        var seqNumPlusData = new byte[4 + lengthOfBuffer];

        seqNumPlusData[0] = unchecked((byte)(sequenceNumber & 0xFF));
        seqNumPlusData[1] = unchecked((byte)((sequenceNumber >> 8) & 0xFF));
        seqNumPlusData[2] = unchecked((byte)((sequenceNumber >> 16) & 0xFF));
        seqNumPlusData[3] = unchecked((byte)((sequenceNumber >> 24) & 0xFF));

        Array.Copy(data, 0, seqNumPlusData, 4, lengthOfBuffer);

        var retval = new byte[16];
        retval[0] = 0x01; // Version number LE 1.

        byte[]? sign = null;
        try
        {
            sign = Responses.HmacMD5(seqNumPlusData, signingKey);

            for (var i = 0; i < 8; i++)
            {
                retval[i + 4] = sign[i];
            }

            retval[12] = unchecked((byte)(sequenceNumber & 0xFF));
            retval[13] = unchecked((byte)((sequenceNumber >> 8) & 0xFF));
            retval[14] = unchecked((byte)((sequenceNumber >> 16) & 0xFF));
            retval[15] = unchecked((byte)((sequenceNumber >> 24) & 0xFF));
            return retval;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(seqNumPlusData);
            CryptographicOperations.ZeroMemory(sign);
        }
    }


    /// <summary>
    /// Signing part 2
    /// </summary>
    /// <param name="verifier">Authentication verifier attached to the RPC PDU.</param>
    /// <param name="rc4">RC4 stream cipher used to seal or unseal the NTLM message payload.</param>
    /// <exception cref="InvalidOperationException">Thrown when the object is not in the state required to perform the operation.</exception>
    public void SigningPt2(byte[] verifier, IStreamCipher rc4)
    {
        for (var i = 0; i < 8; i++)
        {
            //            verifier[i+4] = (byte) (verifier[i+4] ^ rc4.nextByte());
            verifier[i + 4] = rc4.ReturnByte(verifier[i + 4]);
        }
    }

    /// <summary>
    /// Test signatures
    /// </summary>
    /// <param name="src">Source NDR buffer that supplies the field data to decode.</param>
    /// <param name="target">Target object or buffer that receives the operation result.</param>
    /// <returns><c>true</c> when compare signature is satisfied; otherwise <c>false</c>.</returns>
    public bool CompareSignature(byte[] src, byte[] target) => src.SequenceEqual(target);

    private static byte[] GenerateSigningKey(NtlmFlags flags, byte[] exportedSessionKey, byte[] magicConstant)
    {
        if ((flags & NtlmFlags.NtlmsspNegotiateExtendedSessionSecurity) == NtlmFlags.None)
        {
            return Array.Empty<byte>();
        }

        return GenerateExtendedSessionSecurityKey(exportedSessionKey, magicConstant);
    }

    private static byte[] GenerateSealingKey(NtlmFlags flags, byte[] exportedSessionKey, byte[] magicConstant)
    {
        if ((flags & NtlmFlags.NtlmsspNegotiateExtendedSessionSecurity) != NtlmFlags.None)
        {
            return GenerateExtendedSessionSecurityKey(GetExtendedSessionSecuritySealKeyMaterial(flags, exportedSessionKey), magicConstant);
        }

        if ((flags & (NtlmFlags.NtlmsspNegotiateLmKey | NtlmFlags.NtlmsspNegotiateDatagram)) != NtlmFlags.None)
        {
            if ((flags & NtlmFlags.NtlmsspNegotiate56) != NtlmFlags.None)
            {
                return Concatenate(Left(exportedSessionKey, 7), [0xA0]);
            }

            return Concatenate(Left(exportedSessionKey, 5), [0xE5, 0x38, 0xB0]);
        }

        return (byte[])exportedSessionKey.Clone();
    }

    private static byte[] GetExtendedSessionSecuritySealKeyMaterial(NtlmFlags flags, byte[] exportedSessionKey)
    {
        if ((flags & NtlmFlags.NtlmsspNegotiate128) != NtlmFlags.None)
        {
            return exportedSessionKey;
        }

        return (flags & NtlmFlags.NtlmsspNegotiate56) != NtlmFlags.None
            ? Left(exportedSessionKey, 7)
            : Left(exportedSessionKey, 5);
    }

    private static byte[] GenerateExtendedSessionSecurityKey(byte[] keyMaterial, byte[] magicConstant)
    {
        var dataforhash = new byte[keyMaterial.Length + magicConstant.Length];
        try
        {
            Array.Copy(keyMaterial, 0, dataforhash, 0, keyMaterial.Length);
            Array.Copy(magicConstant, 0, dataforhash, keyMaterial.Length, magicConstant.Length);
            var md5 = new MD5Digest();
            var ret = new byte[md5.GetDigestSize()];
            md5.BlockUpdate(dataforhash, 0, dataforhash.Length);
            md5.DoFinal(ret, 0);
            return ret;
        }
        finally
        {
            CryptographicOperations.ZeroMemory(dataforhash);
        }
    }

    private static byte[] Left(byte[] value, int length)
    {
        var copy = new byte[length];
        Array.Copy(value, 0, copy, 0, Math.Min(value.Length, length));
        return copy;
    }

    private static byte[] Concatenate(byte[] first, byte[] second)
    {
        var result = new byte[first.Length + second.Length];
        Array.Copy(first, 0, result, 0, first.Length);
        Array.Copy(second, 0, result, first.Length, second.Length);
        return result;
    }

    private readonly Random _random = new Random();
    private static readonly byte[] kClientSigningMagicConstant = {
        0x73, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x20,
        0x6b, 0x65, 0x79, 0x20, 0x74, 0x6f, 0x20, 0x63,
        0x6c, 0x69, 0x65, 0x6e, 0x74, 0x2d, 0x74, 0x6f,
        0x2d, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x20,
        0x73, 0x69, 0x67, 0x6e, 0x69, 0x6e, 0x67, 0x20,
        0x6b, 0x65, 0x79, 0x20, 0x6d, 0x61, 0x67, 0x69,
        0x63, 0x20, 0x63, 0x6f, 0x6e, 0x73, 0x74, 0x61,
        0x6e, 0x74, 0x00 };
    private static readonly byte[] kServerSigningMagicConstant = {
        0x73, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x20,
        0x6b, 0x65, 0x79, 0x20, 0x74, 0x6f, 0x20, 0x73,
        0x65, 0x72, 0x76, 0x65, 0x72, 0x2d, 0x74, 0x6f,
        0x2d, 0x63, 0x6c, 0x69, 0x65, 0x6e, 0x74, 0x20,
        0x73, 0x69, 0x67, 0x6e, 0x69, 0x6e, 0x67, 0x20,
        0x6b, 0x65, 0x79, 0x20, 0x6d, 0x61, 0x67, 0x69,
        0x63, 0x20, 0x63, 0x6f, 0x6e, 0x73, 0x74, 0x61,
        0x6e, 0x74, 0x00 };
    private static readonly byte[] kClientSealingMagicConstant = {
        0x73, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x20,
        0x6b, 0x65, 0x79, 0x20, 0x74, 0x6f, 0x20, 0x63,
        0x6c, 0x69, 0x65, 0x6e, 0x74, 0x2d, 0x74, 0x6f,
        0x2d, 0x73, 0x65, 0x72, 0x76, 0x65, 0x72, 0x20,
        0x73, 0x65, 0x61, 0x6c, 0x69, 0x6e, 0x67, 0x20,
        0x6b, 0x65, 0x79, 0x20, 0x6d, 0x61, 0x67, 0x69,
        0x63, 0x20, 0x63, 0x6f, 0x6e, 0x73, 0x74, 0x61,
        0x6e, 0x74, 0x00 };
    private static readonly byte[] kServerSealingMagicConstant = {
        0x73, 0x65, 0x73, 0x73, 0x69, 0x6f, 0x6e, 0x20,
        0x6b, 0x65, 0x79, 0x20, 0x74, 0x6f, 0x20, 0x73,
        0x65, 0x72, 0x76, 0x65, 0x72, 0x2d, 0x74, 0x6f,
        0x2d, 0x63, 0x6c, 0x69, 0x65, 0x6e, 0x74, 0x20,
        0x73, 0x65, 0x61, 0x6c, 0x69, 0x6e, 0x67, 0x20,
        0x6b, 0x65, 0x79, 0x20, 0x6d, 0x61, 0x67, 0x69,
        0x63, 0x20, 0x63, 0x6f, 0x6e, 0x73, 0x74, 0x61,
        0x6e, 0x74, 0x00 };
}
