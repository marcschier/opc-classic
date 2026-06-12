//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Diagnostics.CodeAnalysis;

namespace Opc.Classic.Dcom.Crypto;

/// <summary>RC4Engine backed by the in-tree hand-rolled <see cref="Rc4"/> implementation.</summary>
[SuppressMessage("Naming", "CA1709", Justification = "Legacy NTLM API shape preserves BC casing")]
public sealed class RC4Engine : IStreamCipher
{
    private Rc4? _cipher;

    public string AlgorithmName => "RC4";

    public void Init(bool forEncryption, ICipherParameters parameters)
    {
        var kp = parameters as KeyParameter
            ?? throw new ArgumentException("RC4 requires a KeyParameter.", nameof(parameters));
        _cipher = new Rc4(kp.Key);
    }

    public int ProcessBytes(byte[] input, int inOff, int len, byte[] output, int outOff)
    {
        Require().Process(input.AsSpan(inOff, len), output.AsSpan(outOff, len));
        return len;
    }

    public byte ProcessByte(byte b)
    {
        Span<byte> single = stackalloc byte[1] { b };
        Require().XorInPlace(single);
        return single[0];
    }

    public byte ReturnByte(byte b) => ProcessByte(b);

    public void Reset() => _cipher = null;

    private Rc4 Require()
    {
        if (_cipher is null)
        {
            throw new InvalidOperationException("Init must be called before Process.");
        }

        return _cipher;
    }
}
