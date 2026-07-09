// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Crypto;

/// <summary>
/// BouncyCastle <c>IDigest</c>-shaped hash interface (transitional).
/// </summary>
public interface IDigest
{
    int GetDigestSize();
    void BlockUpdate(byte[] input, int offset, int count);
    int DoFinal(byte[] output, int offset);
}
