// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Crypto;

/// <summary>
/// BouncyCastle <c>IBufferedCipher</c>-shaped buffered cipher (transitional).
/// </summary>
public interface IBufferedCipher : IDisposable
{
    void Init(bool forEncryption, ICipherParameters parameters);
    byte[] DoFinal(byte[] input);
}
