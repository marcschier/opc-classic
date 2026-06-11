// SPDX-License-Identifier: MIT

using System;

namespace Opc.Classic.Dcom.Common.Ntlm;

public static class Arrays
{
    public static bool Equals(byte[]? left, byte[]? right) =>
        left is null ? right is null : right is not null && left.AsSpan().SequenceEqual(right);
}
