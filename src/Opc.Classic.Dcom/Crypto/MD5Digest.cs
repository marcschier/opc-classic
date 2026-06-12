//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;

namespace Opc.Classic.Dcom.Crypto;

/// <summary>MD5Digest backed by <see cref="System.Security.Cryptography.MD5"/> (BCL).</summary>
[SuppressMessage("Naming", "CA1709", Justification = "Legacy NTLM API shape preserves BC casing")]
public sealed class MD5Digest : IDigest
{
#pragma warning disable CA5351 // MD5 is required for NTLM protocol compatibility.
    private readonly IncrementalHash _hash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
#pragma warning restore CA5351

    public int GetDigestSize() => 16;

    public void BlockUpdate(byte[] input, int offset, int count)
        => _hash.AppendData(input, offset, count);

    public int DoFinal(byte[] output, int offset)
    {
        Span<byte> tmp = stackalloc byte[16];
        _hash.GetHashAndReset(tmp);
        tmp.CopyTo(output.AsSpan(offset, 16));
        return 16;
    }
}
