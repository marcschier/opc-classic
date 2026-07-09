// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Crypto;

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
