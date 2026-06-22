// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Crypto;

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
