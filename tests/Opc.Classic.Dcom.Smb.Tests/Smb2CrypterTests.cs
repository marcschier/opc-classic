//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using Opc.Classic.Dcom.Smb;
using TUnit.Core;

namespace Opc.Classic.Dcom.Smb.Tests;

public sealed class Smb2CrypterTests
{
    private const ulong SessionId = 0x0102030405060708UL;

    [Test]
    public async Task AesCcm_KnownAnswer_ProducesTransformHeaderAndCiphertext()
    {
        byte[] key = Convert.FromHexString("000102030405060708090A0B0C0D0E0F");
        byte[] nonce = Convert.FromHexString("101112131415161718191A");
        byte[] expected = Convert.FromHexString(
            "FD534D42FE63B7725B85A3665E0D3B6992F00CE9101112131415161718191A000000000048000000000001000807060504030201" +
            "B23C309980BD50AB8E0CE96321639FAE2168D8594FDD7AEFED4086039DF8D2EA09CA74BA07714D719074C786E348586674FE7B5208F758DD4D3C36425C19B241C047AC44CDD9B18C");

        var crypter = new Smb2Crypter(key, Smb2EncryptionAlgorithm.AesCcm);
        byte[] actual = crypter.EncryptMessage(CreatePlaintextMessage(), nonce, SessionId);

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task AesGcm_KnownAnswer_ProducesTransformHeaderAndCiphertext()
    {
        byte[] key = Convert.FromHexString("202122232425262728292A2B2C2D2E2F");
        byte[] nonce = Convert.FromHexString("303132333435363738393A3B");
        byte[] expected = Convert.FromHexString(
            "FD534D423E58662DC54326B03F8A9C5EBF06AD60303132333435363738393A3B0000000048000000000001000807060504030201" +
            "72BA205D0C28FBCA8A6D2D1BAECE573E5521516229713952196F809ADEE98B09DEE3F72C6AFE28D7B0B2901717A4725E43BBB247E0E0847A1C84C7256BD05A7597E493298786435A");

        var crypter = new Smb2Crypter(key, Smb2EncryptionAlgorithm.AesGcm);
        byte[] actual = crypter.EncryptMessage(CreatePlaintextMessage(), nonce, SessionId);

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task Smb3Kdf_Smb300KnownAnswer_DerivesEncryptionAndDecryptionKeys()
    {
        byte[] sessionKey = Convert.FromHexString("000102030405060708090A0B0C0D0E0F");
        byte[] expectedEncryptionKey = Convert.FromHexString("86EFCAD258778BC261FB4584AC60F5C1");
        byte[] expectedDecryptionKey = Convert.FromHexString("7471AF66CBAC2334799A8F81BBA69B84");

        byte[] encryptionKey = Smb2Crypter.DeriveSmb3ClientEncryptionKey(Smb2Dialect.Smb300, sessionKey);
        byte[] decryptionKey = Smb2Crypter.DeriveSmb3ClientDecryptionKey(Smb2Dialect.Smb300, sessionKey);

        await Assert.That(encryptionKey).IsEquivalentTo(expectedEncryptionKey);
        await Assert.That(decryptionKey).IsEquivalentTo(expectedDecryptionKey);
    }

    [Test]
    public async Task Smb3Kdf_Smb311KnownAnswer_DerivesDirectionalCipherKeys()
    {
        byte[] sessionKey = Convert.FromHexString("000102030405060708090A0B0C0D0E0F");
        byte[] preauthHash = new byte[64];
        for (int i = 0; i < preauthHash.Length; i++)
        {
            preauthHash[i] = (byte)i;
        }
        byte[] expectedEncryptionKey = Convert.FromHexString("421F424EB573336F616F4385D774587F");
        byte[] expectedDecryptionKey = Convert.FromHexString("E10E8988123DD9A9CA35CECE19DFFCFB");

        byte[] encryptionKey = Smb2Crypter.DeriveSmb3ClientEncryptionKey(Smb2Dialect.Smb311, sessionKey, preauthHash);
        byte[] decryptionKey = Smb2Crypter.DeriveSmb3ClientDecryptionKey(Smb2Dialect.Smb311, sessionKey, preauthHash);

        await Assert.That(encryptionKey).IsEquivalentTo(expectedEncryptionKey);
        await Assert.That(decryptionKey).IsEquivalentTo(expectedDecryptionKey);
    }

    [Test]
    public async Task EncryptThenDecrypt_RoundTrips()
    {
        byte[] key = Convert.FromHexString("404142434445464748494A4B4C4D4E4F");
        byte[] nonce = Convert.FromHexString("505152535455565758595A5B");
        byte[] plaintext = CreatePlaintextMessage();
        var crypter = new Smb2Crypter(key, Smb2EncryptionAlgorithm.AesGcm);

        byte[] encrypted = crypter.EncryptMessage(plaintext, nonce, SessionId);
        byte[] decrypted = crypter.DecryptMessage(encrypted, SessionId);

        await Assert.That(decrypted).IsEquivalentTo(plaintext);
    }

    [Test]
    public async Task Decrypt_RejectsTamperedCiphertext()
    {
        var crypter = CreateCcmCrypter(out byte[] encrypted);
        encrypted[^1] ^= 0x01;

        await AssertDecryptRejectedAsync(crypter, encrypted);
    }

    [Test]
    public async Task Decrypt_RejectsTamperedNonce()
    {
        var crypter = CreateCcmCrypter(out byte[] encrypted);
        encrypted[20] ^= 0x01;

        await AssertDecryptRejectedAsync(crypter, encrypted);
    }

    [Test]
    public async Task Decrypt_RejectsWrongKey()
    {
        _ = CreateCcmCrypter(out byte[] encrypted);
        var wrongKeyCrypter = new Smb2Crypter(
            Convert.FromHexString("606162636465666768696A6B6C6D6E6F"),
            Smb2EncryptionAlgorithm.AesCcm);

        await AssertDecryptRejectedAsync(wrongKeyCrypter, encrypted);
    }

    [Test]
    public async Task Decrypt_RejectsTamperedTransformHeaderSessionId()
    {
        var crypter = CreateCcmCrypter(out byte[] encrypted);
        encrypted[44] ^= 0x01;

        await AssertDecryptRejectedAsync(crypter, encrypted);
    }

    private static Smb2Crypter CreateCcmCrypter(out byte[] encrypted)
    {
        byte[] key = Convert.FromHexString("000102030405060708090A0B0C0D0E0F");
        byte[] nonce = Convert.FromHexString("101112131415161718191A");
        var crypter = new Smb2Crypter(key, Smb2EncryptionAlgorithm.AesCcm);
        encrypted = crypter.EncryptMessage(CreatePlaintextMessage(), nonce, SessionId);
        return crypter;
    }

    private static async Task AssertDecryptRejectedAsync(Smb2Crypter crypter, byte[] encrypted)
    {
        bool threw = false;
        try
        {
            _ = crypter.DecryptMessage(encrypted, SessionId);
        }
        catch (Smb2ProtocolException)
        {
            threw = true;
        }

        await Assert.That(threw).IsTrue();
    }

    private static byte[] CreatePlaintextMessage()
    {
        byte[] message = new byte[64 + 8];
        var header = new Smb2PacketHeader(
            CreditCharge: 1,
            Status: 0,
            Command: Smb2Command.TreeConnect,
            CreditRequestResponse: 1,
            Flags: 0,
            NextCommand: 0,
            MessageId: 7,
            ProcessId: 0,
            TreeId: 0x11223344,
            SessionId: SessionId,
            Signature: ReadOnlyMemory<byte>.Empty);
        header.Write(message);
        BinaryPrimitives.WriteUInt16LittleEndian(message.AsSpan(64), 9);
        BinaryPrimitives.WriteUInt16LittleEndian(message.AsSpan(68), 72);
        return message;
    }
}
