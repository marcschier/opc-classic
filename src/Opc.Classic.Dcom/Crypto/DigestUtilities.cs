//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Crypto;

/// <summary>
/// DigestUtilities shim — supports "MD4" and "MD5" (the two hashes the legacy
/// NTLM code requests via string name).
/// </summary>
public static class DigestUtilities {
    public static IDigest GetDigest(string algorithm) {
        return algorithm switch {
            "MD4" => new MD4Digest(),
            "MD5" => new MD5Digest(),
            _ => throw new NotSupportedException(
                $"Digest '{algorithm}' is not supported by the transitional crypto shim. " +
                $"Add support or refactor the caller to use BCL/in-tree primitives directly."),
        };
    }
}
