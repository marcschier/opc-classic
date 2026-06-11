//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic;
using TUnit.Core;

namespace Opc.Classic.Tests;

public sealed class OpcDa30ResultIdTests
{
    [Test]
    public async Task InvalidPid_HasExpectedCode()
    {
        await Assert.That(OpcResultId.InvalidPid.Code).IsEqualTo(unchecked((int)0xC0040203u));
        await Assert.That(OpcResultId.InvalidPid.IsFailure).IsTrue();
    }

    [Test]
    public async Task DeadbandNotSet_HasExpectedCode()
    {
        await Assert.That(OpcResultId.DeadbandNotSet.Code).IsEqualTo(unchecked((int)0xC0040400u));
        await Assert.That(OpcResultId.DeadbandNotSet.IsFailure).IsTrue();
    }

    [Test]
    public async Task DataQueueOverflow_IsSuccessSeverity()
    {
        await Assert.That(OpcResultId.DataQueueOverflow.Code).IsEqualTo(0x00040404);
        await Assert.That(OpcResultId.DataQueueOverflow.IsSuccess).IsTrue();
    }

    [Test]
    public async Task RateNotSet_HasExpectedCode()
    {
        await Assert.That(OpcResultId.RateNotSet.Code).IsEqualTo(unchecked((int)0xC0040405u));
    }

    [Test]
    public async Task NotSupported_HasExpectedCode()
    {
        await Assert.That(OpcResultId.NotSupported.Code).IsEqualTo(unchecked((int)0xC0040406u));
    }

    [Test]
    public async Task NoBuffering_DeadbandNotSupported_InvalidContinuationPoint_HaveExpectedCodes()
    {
        await Assert.That(OpcResultId.NoBuffering.Code).IsEqualTo(unchecked((int)0xC0040402u));
        await Assert.That(OpcResultId.DeadbandNotSupported.Code).IsEqualTo(unchecked((int)0xC0040401u));
        await Assert.That(OpcResultId.InvalidContinuationPoint.Code).IsEqualTo(unchecked((int)0xC0040403u));
    }
}
