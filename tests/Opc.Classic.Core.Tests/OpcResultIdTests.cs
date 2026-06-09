//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic;
using TUnit.Core;

namespace Opc.Classic.Tests;

public sealed class OpcResultIdTests {
    [Test]
    public async Task Ok_IsSuccess() {
        await Assert.That(OpcResultId.Ok.IsSuccess).IsTrue();
        await Assert.That(OpcResultId.Ok.IsFailure).IsFalse();
    }

    [Test]
    public async Task Fail_IsFailure() {
        await Assert.That(OpcResultId.Fail.IsFailure).IsTrue();
        await Assert.That(OpcResultId.Fail.IsSuccess).IsFalse();
    }

    [Test]
    public async Task UnknownItemId_HasFacilityOpc() {
        await Assert.That(OpcResultId.UnknownItemId.Facility)
            .IsEqualTo(OpcResultId.FacilityOpc);
    }

    [Test]
    public async Task UnknownItemId_HasExpectedHResult() {
        await Assert.That(unchecked((uint)OpcResultId.UnknownItemId.Code))
            .IsEqualTo(0xC0040007u);
    }

    [Test]
    public async Task ToString_IncludesHexAndDescription() {
        await Assert.That(OpcResultId.Ok.ToString()).IsEqualTo("0x00000000 (S_OK)");
        await Assert.That(OpcResultId.UnknownItemId.ToString())
            .IsEqualTo("0xC0040007 (OPC_E_UNKNOWNITEMID)");
    }

    [Test]
    public async Task Records_AreValueEquatable() {
        var a = new OpcResultId(unchecked((int)0xC0040007u), "OPC_E_UNKNOWNITEMID");
        var b = OpcResultId.UnknownItemId;
        await Assert.That(a).IsEqualTo(b);
    }

    [Test]
    public async Task SuccessCodes_BelowZero_AreSuccess() {
        // OPC_S_* codes are non-zero but have severity bit clear -> success.
        await Assert.That(OpcResultId.UnsupportedRate.IsSuccess).IsTrue();
        await Assert.That(OpcResultId.Clamp.IsSuccess).IsTrue();
    }

    [Test]
    [Arguments(0x00000000, true, false)]
    [Arguments(0x00000001, true, false)]
    [Arguments(unchecked((int)0x80004005u), false, true)]
    [Arguments(unchecked((int)0xC0040001u), false, true)]
    public async Task SeverityBit_DeterminesSuccessAndFailure(int code, bool expectedSuccess, bool expectedFailure) {
        var resultId = new OpcResultId(code, null);

        await Assert.That(resultId.IsSuccess).IsEqualTo(expectedSuccess);
        await Assert.That(resultId.IsFailure).IsEqualTo(expectedFailure);
    }

    [Test]
    public async Task InvalidHandle_ExtractsOpcFacilityAndCodePart() {
        await Assert.That(OpcResultId.InvalidHandle.Facility).IsEqualTo(OpcResultId.FacilityOpc);
        await Assert.That(OpcResultId.InvalidHandle.CodePart).IsEqualTo(0x0001);
    }

    [Test]
    public async Task InvalidArg_ExtractsWin32Facility() {
        await Assert.That(OpcResultId.InvalidArg.Facility).IsEqualTo(7);
        await Assert.That(OpcResultId.InvalidArg.CodePart).IsEqualTo(0x0057);
    }
}
