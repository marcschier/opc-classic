// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Security.Cryptography;

namespace Opc.Classic.Dcom.Crypto;

internal sealed class DesEcbNoPaddingCipher : IBufferedCipher
{
#pragma warning disable CA5351, SYSLIB0021 // DES required for NTLMv1 LM hash compat.
    private readonly DES _alg = DES.Create();
#pragma warning restore CA5351, SYSLIB0021
    private ICryptoTransform? _transform;
    private bool _disposed;

    public DesEcbNoPaddingCipher()
    {
        _alg.Mode = CipherMode.ECB;
        _alg.Padding = PaddingMode.None;
    }

    public void Init(bool forEncryption, ICipherParameters parameters)
    {
        var kp = parameters as KeyParameter
            ?? throw new ArgumentException("DES requires a KeyParameter.", nameof(parameters));
        _alg.Key = kp.Key;
        _transform?.Dispose();
        _transform = forEncryption ? _alg.CreateEncryptor() : _alg.CreateDecryptor();
    }

    public byte[] DoFinal(byte[] input)
    {
        if (_transform is null)
        {
            throw new InvalidOperationException("Init must be called before DoFinal.");
        }

        return _transform.TransformFinalBlock(input, 0, input.Length);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _transform?.Dispose();
        _alg.Dispose();
        _disposed = true;
    }
}
