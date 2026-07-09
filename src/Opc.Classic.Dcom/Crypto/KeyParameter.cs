// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Crypto;

/// <summary>
/// BouncyCastle <c>KeyParameter</c>: wraps a key byte[].
/// </summary>
public sealed class KeyParameter : ICipherParameters
{
    public KeyParameter(byte[] key)
    {
        Key = (byte[])(key ?? throw new ArgumentNullException(nameof(key))).Clone();
    }

    public byte[] Key { get; }
}
