// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Ae.Tests;

public sealed class OpcAeResultIdTests
{
    [Test]
    public async Task AlreadyAcked_HasExpectedCode()
    {
        await Assert.That(OpcAeResultId.AlreadyAcked.Code).IsEqualTo(0x00040200);
        await Assert.That(OpcAeResultId.AlreadyAcked.IsSuccess).IsTrue();
    }

    [Test]
    public async Task InvalidBufferTime_InvalidMaxSize_InvalidKeepAliveTime_AreSuccess()
    {
        await Assert.That(OpcAeResultId.InvalidBufferTime.IsSuccess).IsTrue();
        await Assert.That(OpcAeResultId.InvalidMaxSize.IsSuccess).IsTrue();
        await Assert.That(OpcAeResultId.InvalidKeepAliveTime.IsSuccess).IsTrue();
    }

    [Test]
    public async Task InvalidBranchName_HasExpectedCode()
    {
        await Assert.That(OpcAeResultId.InvalidBranchName.Code).IsEqualTo(unchecked((int)0xC0040203u));
        await Assert.That(OpcAeResultId.InvalidBranchName.IsFailure).IsTrue();
    }

    [Test]
    public async Task InvalidBranchName_NumericallyCollidesWithDaInvalidPid()
    {
        // AE's OPC_E_INVALIDBRANCHNAME and DA's OPC_E_INVALID_PID share the same
        // 32-bit HRESULT value (0xC0040203) — disambiguation is by spec context.
        await Assert.That(OpcAeResultId.InvalidBranchName.Code).IsEqualTo(OpcResultId.InvalidPid.Code);
        // The descriptions differ — that's the spec-context tag.
        await Assert.That(OpcAeResultId.InvalidBranchName.Description).IsNotEqualTo(OpcResultId.InvalidPid.Description);
    }

    [Test]
    public async Task InvalidTime_Busy_NoInfo_HaveExpectedCodes()
    {
        await Assert.That(OpcAeResultId.InvalidTime.Code).IsEqualTo(unchecked((int)0xC0040204u));
        await Assert.That(OpcAeResultId.Busy.Code).IsEqualTo(unchecked((int)0xC0040205u));
        await Assert.That(OpcAeResultId.NoInfo.Code).IsEqualTo(unchecked((int)0xC0040206u));
    }
}
