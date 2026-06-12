//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using VerifyTUnit;

namespace Opc.Classic.SnapshotTests.Support;

internal static class SnapshotVerifier
{
    public static async Task VerifyBytes(string codecName, string sampleDescription, byte[] bytes)
    {
        await Verifier.Verify(HexDumpFormatter.Format(codecName, sampleDescription, bytes));
    }
}
