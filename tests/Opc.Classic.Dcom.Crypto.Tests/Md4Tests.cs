//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//
// Conformance tests for the hand-rolled MD4 in Opc.Classic.Dcom.Crypto.Md4.
// Test vectors come from RFC 1320 Appendix A.5 (the canonical MD4 reference).
//

using System.Text;

namespace Opc.Classic.Dcom.Crypto.Tests;

public sealed class Md4Tests
{
    // RFC 1320 Appendix A.5 — MD4 Test Suite. ASCII inputs only.
    [Test]
    [Arguments("", "31d6cfe0d16ae931b73c59d7e0c089c0")]
    [Arguments("a", "bde52cb31de33e46245e05fbdbd6fb24")]
    [Arguments("abc", "a448017aaf21d8525fc10ae87aa6729d")]
    [Arguments("message digest", "d9130a8164549fe818874806e1c7014b")]
    [Arguments("abcdefghijklmnopqrstuvwxyz", "d79e1c308aa5bbcdeea8ed63df412da9")]
    [Arguments("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789", "043f8582f241db351ce627e153e7f0e4")]
    [Arguments("12345678901234567890123456789012345678901234567890123456789012345678901234567890", "e33b4ddc9c38f2199c3e7b164fcc0536")]
    public async Task HashData_MatchesRfc1320Vector(string input, string expectedHex)
    {
        var bytes = Encoding.ASCII.GetBytes(input);
        var hash = Md4.HashData(bytes);
        var actual = Convert.ToHexString(hash).ToLowerInvariant();
        await Assert.That(actual).IsEqualTo(expectedHex);
    }

    [Test]
    public async Task HashData_NtlmNtHash_FromMicrosoftKnownExample()
    {
        // NT-Hash("password") — the well-known Windows NT password-hash example.
        // UTF-16LE encoded, MD4-hashed.
        var ntHash = Md4.HashData(Encoding.Unicode.GetBytes("password"));
        var actual = Convert.ToHexString(ntHash).ToLowerInvariant();
        await Assert.That(actual).IsEqualTo("8846f7eaee8fb117ad06bdd830b7586c");
    }

    [Test]
    public async Task Incremental_MatchesOneShot_LargeInput()
    {
        // 200 bytes — spans multiple 64-byte blocks and exercises both the
        // direct-block path and the buffered-tail path.
        var data = new byte[200];
        new Random(42).NextBytes(data);

        var oneShot = Md4.HashData(data);

        var state = new Md4State();
        state.Initialize();
        state.AppendData(data.AsSpan(0, 1));
        state.AppendData(data.AsSpan(1, 63));
        state.AppendData(data.AsSpan(64, 128));
        state.AppendData(data.AsSpan(192, 8));
        var incremental = new byte[16];
        state.GetHashAndReset(incremental);

        await Assert.That(Convert.ToHexString(incremental))
            .IsEqualTo(Convert.ToHexString(oneShot));
    }

    [Test]
    public async Task Incremental_ResetAllowsReuse()
    {
        var state = new Md4State();
        state.Initialize();
        state.AppendData(Encoding.ASCII.GetBytes("abc"));
        var first = new byte[16];
        state.GetHashAndReset(first);

        state.AppendData(Encoding.ASCII.GetBytes("message digest"));
        var second = new byte[16];
        state.GetHashAndReset(second);

        await Assert.That(Convert.ToHexString(first).ToLowerInvariant())
            .IsEqualTo("a448017aaf21d8525fc10ae87aa6729d");
        await Assert.That(Convert.ToHexString(second).ToLowerInvariant())
            .IsEqualTo("d9130a8164549fe818874806e1c7014b");
    }

    [Test]
    public async Task BcCompat_MD4Digest_MatchesStaticHashData()
    {
        // The BC-shape MD4Digest wrapper must produce identical bytes to the
        // static Md4.HashData API for the same input.
        var input = Encoding.ASCII.GetBytes("The quick brown fox jumps over the lazy dog");

        var staticHash = Md4.HashData(input);

        var wrapper = new MD4Digest();
        wrapper.BlockUpdate(input, 0, input.Length);
        var wrapperHash = new byte[wrapper.GetDigestSize()];
        wrapper.DoFinal(wrapperHash, 0);

        await Assert.That(Convert.ToHexString(wrapperHash))
            .IsEqualTo(Convert.ToHexString(staticHash));
    }
}
