//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Globalization;
using System.Linq;

namespace Opc.Classic.Dcom.Kerberos.Tests;

internal static class KerberosTestHex {
    public static byte[] FromHex(string hex) {
        string compact = new(hex.Where(Uri.IsHexDigit).ToArray());
        if (compact.Length % 2 != 0) {
            throw new ArgumentException("Hex string must contain complete bytes.", nameof(hex));
        }

        var bytes = new byte[compact.Length / 2];
        for (int i = 0; i < bytes.Length; i++) {
            bytes[i] = byte.Parse(compact.AsSpan(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        return bytes;
    }
}
