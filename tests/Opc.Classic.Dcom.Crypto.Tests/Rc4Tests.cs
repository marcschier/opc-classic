//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Conformance tests for the hand-rolled RC4 in Opc.Classic.Dcom.Crypto.Rc4.
// Test vectors come from RFC 6229 §2.1 — the IETF reference RC4 test vectors.
//

using System;
using Opc.Classic.Dcom.Crypto;
using TUnit.Core;

namespace Opc.Classic.Dcom.Crypto.Tests;

public sealed class Rc4Tests {
    // RFC 6229 §2.1 — RC4 test vectors. Inputs are all-zero plaintext;
    // outputs are the keystream bytes at offset 0..15.

    [Test]
    [Arguments("0102030405", "b2396305f03dc027ccc3524a0a1118a8")]                            // 40-bit key
    [Arguments("01020304050607", "293f02d47f37c9b633f2af5285feb46b")]                        // 56-bit key
    [Arguments("0102030405060708", "97ab8a1bf0afb96132f2f67258da15a8")]                      // 64-bit key
    [Arguments("0102030405060708090a", "ede3b04643e586cc907dc21851709902")]                  // 80-bit key
    [Arguments("0102030405060708090a0b0c0d0e0f10", "9ac7cc9a609d1ef7b2932899cde41b97")]      // 128-bit key
    public async Task FirstSixteenBytes_MatchRfc6229Vector(string keyHex, string expectedHex) {
        var key = Convert.FromHexString(keyHex);
        var input = new byte[16]; // all zeros — RC4(0) == keystream
        var output = new Rc4(key).Process(input);
        await Assert.That(Convert.ToHexString(output).ToLowerInvariant())
            .IsEqualTo(expectedHex.ToLowerInvariant());
    }

    [Test]
    public async Task Rc4_IsInverse_OfItself() {
        // RC4 is symmetric: encrypt(encrypt(x)) == x with the same key.
        var key = "TheBestSecretKey"u8.ToArray();
        var plaintext = "Attack at dawn"u8.ToArray();

        var ciphertext = new Rc4(key).Process(plaintext);
        var decrypted = new Rc4(key).Process(ciphertext);

        await Assert.That(Convert.ToHexString(decrypted))
            .IsEqualTo(Convert.ToHexString(plaintext));
    }

    [Test]
    public async Task XorInPlace_MatchesProcess() {
        var key = Convert.FromHexString("0102030405");
        var data1 = new byte[32];
        var data2 = new byte[32];
        new Random(7).NextBytes(data1);
        Array.Copy(data1, data2, 32);

        var processOutput = new Rc4(key).Process(data1);
        new Rc4(key).XorInPlace(data2);

        await Assert.That(Convert.ToHexString(data2))
            .IsEqualTo(Convert.ToHexString(processOutput));
    }

    [Test]
    public async Task ProcessOnZeros_StreamRemainsConsistent_Across_Boundary() {
        // RFC 6229 confirms bytes [0..15] for the 40-bit key (already tested).
        // This complementary test validates that the keystream is continuous
        // across the 16-byte boundary by checking that processing two 16-byte
        // halves separately yields the same result as one 32-byte process.
        var key = Convert.FromHexString("0102030405");

        var oneShot = new Rc4(key).Process(new byte[32]);

        var split = new Rc4(key);
        var first = split.Process(new byte[16]);
        var second = split.Process(new byte[16]);

        var combined = new byte[32];
        first.CopyTo(combined, 0);
        second.CopyTo(combined, 16);

        await Assert.That(Convert.ToHexString(combined))
            .IsEqualTo(Convert.ToHexString(oneShot));
    }

    [Test]
    public async Task BcCompat_RC4Engine_MatchesStaticRc4() {
        // The BC-shape RC4Engine wrapper must produce identical output to the
        // direct Rc4 class for the same key + input.
        var key = Convert.FromHexString("0102030405");
        var input = "Hello, OPC Classic!"u8.ToArray();

        var direct = new Rc4(key).Process(input);

        var engine = new RC4Engine();
        engine.Init(forEncryption: true, new KeyParameter(key));
        var wrapped = new byte[input.Length];
        engine.ProcessBytes(input, 0, input.Length, wrapped, 0);

        await Assert.That(Convert.ToHexString(wrapped))
            .IsEqualTo(Convert.ToHexString(direct));
    }

    [Test]
    public async Task BcCompat_ReturnByte_IsByteWiseProcess() {
        var key = Convert.FromHexString("0102030405060708");
        var input = "Bytewise stream"u8.ToArray();

        var direct = new Rc4(key).Process(input);

        var engine = new RC4Engine();
        engine.Init(forEncryption: true, new KeyParameter(key));
        var bytewise = new byte[input.Length];
        for (var i = 0; i < input.Length; i++) {
            bytewise[i] = engine.ReturnByte(input[i]);
        }

        await Assert.That(Convert.ToHexString(bytewise))
            .IsEqualTo(Convert.ToHexString(direct));
    }
}
