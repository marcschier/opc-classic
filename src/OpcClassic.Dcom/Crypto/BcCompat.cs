//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//
// TRANSITIONAL — a thin compatibility layer that mimics the BouncyCastle
// surface (IDigest, IStreamCipher, ICipherParameters, KeyParameter,
// CipherUtilities) so the legacy SharpInterop NTLM code under
// src/OpcClassic.Dcom/rpc/Auth/ keeps compiling with one-line `using`
// changes, but is backed by:
//   - Hand-rolled MD4 (Crypto/Md4.cs)
//   - Hand-rolled RC4 (Crypto/Rc4.cs)
//   - BCL System.Security.Cryptography for MD5, HMAC-MD5, DES
//
// Phase 2B / 2C will eliminate this shim by rewriting the call sites to use
// the hand-rolled / BCL types directly. Don't add NEW code against this
// shim — it exists only to bridge the legacy code through Phase 2 until
// the proper rewrite happens.
//

namespace SharpInterop.Crypto;

using System;
using System.Security.Cryptography;

/// <summary>
/// BouncyCastle <c>IDigest</c>-shaped hash interface (transitional).
/// </summary>
public interface IDigest
{
    int GetDigestSize();
    void BlockUpdate(byte[] input, int offset, int count);
    int DoFinal(byte[] output, int offset);
}

/// <summary>
/// BouncyCastle <c>IStreamCipher</c>-shaped stream-cipher interface (transitional).
/// </summary>
public interface IStreamCipher
{
    void Init(bool forEncryption, ICipherParameters parameters);
    int ProcessBytes(byte[] input, int inOff, int len, byte[] output, int outOff);
    byte ProcessByte(byte b);
    byte ReturnByte(byte b);
    string AlgorithmName { get; }
    void Reset();
}

/// <summary>BouncyCastle <c>ICipherParameters</c> marker.</summary>
public interface ICipherParameters { }

/// <summary>BouncyCastle <c>KeyParameter</c>: wraps a key byte[].</summary>
public sealed class KeyParameter : ICipherParameters
{
    public KeyParameter(byte[] key)
    {
        Key = (byte[])(key ?? throw new ArgumentNullException(nameof(key))).Clone();
    }

    public byte[] Key { get; }
}

/// <summary>BouncyCastle <c>IBufferedCipher</c>-shaped buffered cipher (transitional).</summary>
public interface IBufferedCipher : IDisposable
{
    void Init(bool forEncryption, ICipherParameters parameters);
    byte[] DoFinal(byte[] input);
}

/// <summary>
/// MD4Digest backed by the in-tree hand-rolled <see cref="Md4"/> implementation.
/// </summary>
#pragma warning disable CA1707 // Identifiers should not contain underscores — legacy NTLM API shape
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1709", Justification = "Legacy NTLM API shape preserves BC casing")]
public sealed class MD4Digest : IDigest
{
    private Md4State _state;

    public MD4Digest() => _state.Initialize();

    public int GetDigestSize() => Md4.HashSizeInBytes;

    public void BlockUpdate(byte[] input, int offset, int count)
        => _state.AppendData(input.AsSpan(offset, count));

    public int DoFinal(byte[] output, int offset)
    {
        _state.GetHashAndReset(output.AsSpan(offset, Md4.HashSizeInBytes));
        return Md4.HashSizeInBytes;
    }
}

/// <summary>
/// MD5Digest backed by <see cref="System.Security.Cryptography.MD5"/> (BCL).
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1709", Justification = "Legacy NTLM API shape preserves BC casing")]
public sealed class MD5Digest : IDigest
{
    private readonly IncrementalHash _hash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);

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

/// <summary>
/// RC4Engine backed by the in-tree hand-rolled <see cref="Rc4"/> implementation.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "CA1709", Justification = "Legacy NTLM API shape preserves BC casing")]
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
        Require();
        _cipher!.Process(input.AsSpan(inOff, len), output.AsSpan(outOff, len));
        return len;
    }

    public byte ProcessByte(byte b)
    {
        Require();
        Span<byte> single = stackalloc byte[1] { b };
        _cipher!.XorInPlace(single);
        return single[0];
    }

    public byte ReturnByte(byte b) => ProcessByte(b);

    public void Reset() => _cipher = null;

    private void Require()
    {
        if (_cipher is null)
        {
            throw new InvalidOperationException("Init must be called before Process.");
        }
    }
}
#pragma warning restore CA1707

/// <summary>
/// CipherUtilities shim — supports only "DES/ECB/NoPadding" (the one algorithm
/// the legacy NTLM code requests).
/// </summary>
public static class CipherUtilities
{
    public static IBufferedCipher GetCipher(string algorithm)
    {
        return algorithm switch
        {
            "DES/ECB/NoPadding" => new DesEcbNoPaddingCipher(),
            _ => throw new NotSupportedException(
                $"Algorithm '{algorithm}' is not supported by the transitional crypto shim. " +
                $"Add support or refactor the caller to use BCL/in-tree primitives directly."),
        };
    }
}

/// <summary>
/// DigestUtilities shim — supports "MD4" and "MD5" (the two hashes the legacy
/// NTLM code requests via string name).
/// </summary>
public static class DigestUtilities
{
    public static IDigest GetDigest(string algorithm)
    {
        return algorithm switch
        {
            "MD4" => new MD4Digest(),
            "MD5" => new MD5Digest(),
            _ => throw new NotSupportedException(
                $"Digest '{algorithm}' is not supported by the transitional crypto shim. " +
                $"Add support or refactor the caller to use BCL/in-tree primitives directly."),
        };
    }
}

internal sealed class DesEcbNoPaddingCipher : IBufferedCipher
{
#pragma warning disable CA5351, SYSLIB0021 // DES required for NTLMv1 LM hash compat
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
