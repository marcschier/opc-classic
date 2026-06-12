//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Crypto;

/// <summary>BouncyCastle <c>KeyParameter</c>: wraps a key byte[].</summary>
public sealed class KeyParameter : ICipherParameters
{
    public KeyParameter(byte[] key)
    {
        Key = (byte[])(key ?? throw new ArgumentNullException(nameof(key))).Clone();
    }

    public byte[] Key { get; }
}
