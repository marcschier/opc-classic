//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Tests;

public sealed class OpcExceptionTests
{
    [Test]
    public async Task Default_HasFailResultId()
    {
        var ex = new OpcException();
        await Assert.That(ex.ResultId).IsEqualTo(OpcResultId.Fail);
    }

    [Test]
    public async Task WithResultId_PreservesId()
    {
        var ex = new OpcException(OpcResultId.UnknownItemId);
        await Assert.That(ex.ResultId).IsEqualTo(OpcResultId.UnknownItemId);
    }

    [Test]
    public async Task ThrowIfFailed_OnSuccess_ReturnsId()
    {
        var returned = OpcException.ThrowIfFailed(OpcResultId.Ok);
        await Assert.That(returned).IsEqualTo(OpcResultId.Ok);
    }

    [Test]
    public async Task ThrowIfFailed_OnFailure_ThrowsAndCarriesId()
    {
        var thrown = await Assert.That(() => { OpcException.ThrowIfFailed(OpcResultId.UnknownItemId, "Read"); })
            .Throws<OpcException>();
        await Assert.That(thrown!.ResultId).IsEqualTo(OpcResultId.UnknownItemId);
        await Assert.That(thrown.Message).Contains("Read");
        await Assert.That(thrown.Message).Contains("OPC_E_UNKNOWNITEMID");
    }

    [Test]
    public async Task ThrowIfFailed_OnOpcSuccessCode_DoesNotThrow()
    {
        // OPC_S_UNSUPPORTEDRATE is a success code (severity bit clear).
        var returned = OpcException.ThrowIfFailed(OpcResultId.UnsupportedRate);
        await Assert.That(returned).IsEqualTo(OpcResultId.UnsupportedRate);
    }

    [Test]
    public async Task SpecHierarchy_IsOpcException()
    {
        var ae = new OpcAeException(OpcResultId.Fail);
        await Assert.That(ae is OpcException).IsTrue();
    }
}
