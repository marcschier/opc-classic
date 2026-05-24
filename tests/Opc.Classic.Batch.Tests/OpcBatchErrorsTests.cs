//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using TUnit.Core;

namespace Opc.Classic.Batch.Tests;

public sealed class OpcBatchErrorsTests
{
    [Test]
    public async Task NotMeaningful_hresult_matches_spec()
    {
        await Assert.That(ReadNotMeaningful()).IsEqualTo(ReadExpectedNotMeaningful());
    }

    private static int ReadNotMeaningful() => OpcBatchErrors.OPCB_E_NOT_MEANINGFUL;

    private static int ReadExpectedNotMeaningful() => unchecked((int)0xC0040300u);
}
