// SPDX-License-Identifier: MIT

using System;
using System.Security.Cryptography;

namespace Opc.Classic.Dcom.Internal.Ntlm;

public static class NtlmMic
{
    public const int MicLength = 16;

    public static byte[] Compute(byte[] sessionKey, ReadOnlySpan<byte> negotiate, ReadOnlySpan<byte> challenge,
        ReadOnlySpan<byte> authenticate)
    {
        ArgumentNullException.ThrowIfNull(sessionKey);

        var input = new byte[negotiate.Length + challenge.Length + authenticate.Length];
        var offset = 0;
        negotiate.CopyTo(input.AsSpan(offset));
        offset += negotiate.Length;
        challenge.CopyTo(input.AsSpan(offset));
        offset += challenge.Length;
        authenticate.CopyTo(input.AsSpan(offset));

        using var hmac = new HMACMD5(sessionKey);
        return hmac.ComputeHash(input);
    }

    public static bool Verify(byte[] sessionKey, ReadOnlySpan<byte> negotiate, ReadOnlySpan<byte> challenge,
        byte[] authenticate, int micOffset)
    {
        ArgumentNullException.ThrowIfNull(sessionKey);
        ArgumentNullException.ThrowIfNull(authenticate);

        if (micOffset < 0 || micOffset > authenticate.Length - MicLength)
        {
            return false;
        }

        var actualMic = authenticate.AsSpan(micOffset, MicLength).ToArray();
        var zeroedAuthenticate = (byte[])authenticate.Clone();
        zeroedAuthenticate.AsSpan(micOffset, MicLength).Clear();
        var expectedMic = Compute(sessionKey, negotiate, challenge, zeroedAuthenticate);
        return CryptographicOperations.FixedTimeEquals(expectedMic, actualMic);
    }
}
