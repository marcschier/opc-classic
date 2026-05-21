//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// Pure-managed RC4 (Rivest Cipher 4) — symmetric stream cipher.
// In-tree replacement for BouncyCastle's RC4Engine. BCL removed RC4 in net6.
//
// RC4 is cryptographically broken. Used here only for protocol compatibility
// with NTLM packet sealing (RC4-128 over RPC integrity-mode signed traffic
// per MS-NLMP §3.4.5). Do NOT use for new security primitives.
//

namespace SharpInterop.Crypto;

using System;

/// <summary>
/// Pure-managed RC4 stream cipher (KSA + PRGA).
/// Each instance carries its own keystream state — create one per direction
/// (client→server and server→client need separate states with the same key).
/// </summary>
public sealed class Rc4
{
    private readonly byte[] _s = new byte[256];
    private byte _i;
    private byte _j;

    /// <summary>Construct + key-schedule (KSA).</summary>
    public Rc4(ReadOnlySpan<byte> key)
    {
        if (key.IsEmpty)
        {
            throw new ArgumentException("Key cannot be empty.", nameof(key));
        }

        for (var n = 0; n < 256; n++)
        {
            _s[n] = (byte)n;
        }
        byte j = 0;
        for (var n = 0; n < 256; n++)
        {
            j = (byte)(j + _s[n] + key[n % key.Length]);
            (_s[n], _s[j]) = (_s[j], _s[n]);
        }
    }

    /// <summary>PRGA: produce <paramref name="output"/> = XOR(input, keystream).</summary>
    public void Process(ReadOnlySpan<byte> input, Span<byte> output)
    {
        if (output.Length < input.Length)
        {
            throw new ArgumentException("Output is shorter than input.", nameof(output));
        }
        for (var k = 0; k < input.Length; k++)
        {
            _i = (byte)(_i + 1);
            _j = (byte)(_j + _s[_i]);
            (_s[_i], _s[_j]) = (_s[_j], _s[_i]);
            var ks = _s[(byte)(_s[_i] + _s[_j])];
            output[k] = (byte)(input[k] ^ ks);
        }
    }

    /// <summary>Convenience: XOR <paramref name="data"/> in place against the keystream.</summary>
    public void XorInPlace(Span<byte> data) => Process(data, data);

    /// <summary>Convenience: returns a fresh <paramref name="input"/>.Length-sized byte[] of XOR output.</summary>
    public byte[] Process(ReadOnlySpan<byte> input)
    {
        var output = new byte[input.Length];
        Process(input, output);
        return output;
    }
}
