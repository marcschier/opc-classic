//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace SharpInterop.Crypto;

/// <summary>BouncyCastle <c>IBufferedCipher</c>-shaped buffered cipher (transitional).</summary>
public interface IBufferedCipher : IDisposable {
    void Init(bool forEncryption, ICipherParameters parameters);
    byte[] DoFinal(byte[] input);
}
