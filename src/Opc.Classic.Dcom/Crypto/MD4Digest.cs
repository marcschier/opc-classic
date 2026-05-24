//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Diagnostics.CodeAnalysis;

namespace Opc.Classic.Dcom.Crypto;

/// <summary>MD4Digest backed by the in-tree hand-rolled <see cref="Md4"/> implementation.</summary>
[SuppressMessage("Naming", "CA1709", Justification = "Legacy NTLM API shape preserves BC casing")]
public sealed class MD4Digest : IDigest {
    private Md4State _state;

    public MD4Digest() => _state.Initialize();

    public int GetDigestSize() => Md4.HashSizeInBytes;

    public void BlockUpdate(byte[] input, int offset, int count)
        => _state.AppendData(input.AsSpan(offset, count));

    public int DoFinal(byte[] output, int offset) {
        _state.GetHashAndReset(output.AsSpan(offset, Md4.HashSizeInBytes));
        return Md4.HashSizeInBytes;
    }
}
