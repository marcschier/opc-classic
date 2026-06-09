//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Buffers.Binary;
using Opc.Classic.Dcom.Smb;
using TUnit.Core;

namespace Opc.Classic.Dcom.Smb.Tests;

public sealed class Smb2SignerTests {
    [Test]
    public async Task HmacSha256_Smb202KnownAnswer_TruncatesTo16Bytes() {
        byte[] sessionKey = Convert.FromHexString("000102030405060708090A0B0C0D0E0F");
        byte[] message = CreateSignedTreeConnectSkeleton();
        byte[] expected = Convert.FromHexString("D5ED1480CCC1569BD4A4FDE120192EF7");

        var signer = new Smb2Signer(sessionKey, Smb2SigningAlgorithm.HmacSha256);
        byte[] actual = new byte[Smb2Signer.SignatureLength];
        signer.ComputeSignature(message, actual);

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task AesCmac_NistSp80038BKnownAnswer_Matches16ByteTag() {
        byte[] key = Convert.FromHexString("2B7E151628AED2A6ABF7158809CF4F3C");
        byte[] message = Convert.FromHexString("6BC1BEE22E409F96E93D7E117393172A");
        byte[] expected = Convert.FromHexString("070A16B46B4D4144F79BDD9DD04A287C");

        byte[] actual = new byte[Smb2Signer.SignatureLength];
        Smb2Signer.ComputeAesCmac(key, message, actual);

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task Smb3Kdf_Smb300KnownAnswer_DerivesSigningKey() {
        byte[] sessionKey = Convert.FromHexString("000102030405060708090A0B0C0D0E0F");
        byte[] expected = Convert.FromHexString("D3AE02925B058C68B16D609099D64D16");

        byte[] actual = Smb2Signer.DeriveSmb3SigningKey(Smb2Dialect.Smb300, sessionKey);

        await Assert.That(actual).IsEquivalentTo(expected);
    }

    [Test]
    public async Task SignThenVerify_RoundTrips() {
        byte[] sessionKey = Convert.FromHexString("101112131415161718191A1B1C1D1E1F");
        byte[] message = CreateSignedTreeConnectSkeleton();
        var signer = new Smb2Signer(sessionKey, Smb2SigningAlgorithm.HmacSha256);

        signer.Sign(message);

        await Assert.That(message.AsSpan(48, Smb2Signer.SignatureLength).ToArray().Any(static value => value != 0)).IsTrue();
        await Assert.That(signer.VerifySignature(message)).IsTrue();
    }

    [Test]
    public async Task Verify_RejectsTamperedMessage() {
        byte[] sessionKey = Convert.FromHexString("202122232425262728292A2B2C2D2E2F");
        byte[] message = CreateSignedTreeConnectSkeleton();
        var signer = new Smb2Signer(sessionKey, Smb2SigningAlgorithm.HmacSha256);
        signer.Sign(message);

        message[^1] ^= 0x01;

        await Assert.That(signer.VerifySignature(message)).IsFalse();
    }

    [Test]
    public async Task Verify_RejectsWrongKey() {
        byte[] signingKey = Convert.FromHexString("303132333435363738393A3B3C3D3E3F");
        byte[] verifyingKey = Convert.FromHexString("404142434445464748494A4B4C4D4E4F");
        byte[] message = CreateSignedTreeConnectSkeleton();
        var signer = new Smb2Signer(signingKey, Smb2SigningAlgorithm.HmacSha256);
        var verifier = new Smb2Signer(verifyingKey, Smb2SigningAlgorithm.HmacSha256);
        signer.Sign(message);

        await Assert.That(verifier.VerifySignature(message)).IsFalse();
    }

    private static byte[] CreateSignedTreeConnectSkeleton() {
        byte[] message = new byte[64 + 8];
        var header = new Smb2PacketHeader(
            CreditCharge: 1,
            Status: 0,
            Command: Smb2Command.TreeConnect,
            CreditRequestResponse: 1,
            Flags: 0x00000008,
            NextCommand: 0,
            MessageId: 7,
            ProcessId: 0,
            TreeId: 0x11223344,
            SessionId: 0x0102030405060708,
            Signature: ReadOnlyMemory<byte>.Empty);
        header.Write(message);
        BinaryPrimitives.WriteUInt16LittleEndian(message.AsSpan(64), 9);
        BinaryPrimitives.WriteUInt16LittleEndian(message.AsSpan(68), 72);
        return message;
    }
}
