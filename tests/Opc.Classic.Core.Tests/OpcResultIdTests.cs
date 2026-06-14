//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Tests;

public sealed class OpcResultIdTests
{
    [Test]
    public async Task Ok_IsSuccess()
    {
        await Assert.That(OpcResultId.Ok.IsSuccess).IsTrue();
        await Assert.That(OpcResultId.Ok.IsFailure).IsFalse();
    }

    [Test]
    public async Task Fail_IsFailure()
    {
        await Assert.That(OpcResultId.Fail.IsFailure).IsTrue();
        await Assert.That(OpcResultId.Fail.IsSuccess).IsFalse();
    }

    [Test]
    public async Task UnknownItemId_HasFacilityOpc()
    {
        await Assert.That(OpcResultId.UnknownItemId.Facility)
            .IsEqualTo(OpcResultId.FacilityOpc);
    }

    [Test]
    public async Task UnknownItemId_HasExpectedHResult()
    {
        await Assert.That(unchecked((uint)OpcResultId.UnknownItemId.Code))
            .IsEqualTo(0xC0040007u);
    }

    [Test]
    public async Task ToString_IncludesHexAndDescription()
    {
        await Assert.That(OpcResultId.Ok.ToString()).IsEqualTo("0x00000000 (S_OK)");
        await Assert.That(OpcResultId.UnknownItemId.ToString())
            .IsEqualTo("0xC0040007 (OPC_E_UNKNOWNITEMID)");
    }

    [Test]
    public async Task Records_AreValueEquatable()
    {
        var a = new OpcResultId(unchecked((int)0xC0040007u), "OPC_E_UNKNOWNITEMID");
        var b = OpcResultId.UnknownItemId;
        await Assert.That(a).IsEqualTo(b);
    }

    [Test]
    public async Task SuccessCodes_BelowZero_AreSuccess()
    {
        // OPC_S_* codes are non-zero but have severity bit clear -> success.
        await Assert.That(OpcResultId.UnsupportedRate.IsSuccess).IsTrue();
        await Assert.That(OpcResultId.Clamp.IsSuccess).IsTrue();
    }

    [Test]
    [Arguments(0x00000000, true, false)]
    [Arguments(0x00000001, true, false)]
    [Arguments(unchecked((int)0x80004005u), false, true)]
    [Arguments(unchecked((int)0xC0040001u), false, true)]
    public async Task SeverityBit_DeterminesSuccessAndFailure(int code, bool expectedSuccess, bool expectedFailure)
    {
        var resultId = new OpcResultId(code, null);

        await Assert.That(resultId.IsSuccess).IsEqualTo(expectedSuccess);
        await Assert.That(resultId.IsFailure).IsEqualTo(expectedFailure);
    }

    [Test]
    public async Task InvalidHandle_ExtractsOpcFacilityAndCodePart()
    {
        await Assert.That(OpcResultId.InvalidHandle.Facility).IsEqualTo(OpcResultId.FacilityOpc);
        await Assert.That(OpcResultId.InvalidHandle.CodePart).IsEqualTo(0x0001);
    }

    [Test]
    public async Task InvalidArg_ExtractsWin32Facility()
    {
        await Assert.That(OpcResultId.InvalidArg.Facility).IsEqualTo(7);
        await Assert.That(OpcResultId.InvalidArg.CodePart).IsEqualTo(0x0057);
    }

    [Test]
    public async Task FromWin32_Zero_ReturnsOk()
    {
        await Assert.That(OpcResultId.FromWin32(0u)).IsEqualTo(OpcResultId.Ok);
    }

    [Test]
    [Arguments(5u, 0x80070005u)]      // ERROR_ACCESS_DENIED
    [Arguments(2u, 0x80070002u)]      // ERROR_FILE_NOT_FOUND
    [Arguments(0x57u, 0x80070057u)]   // ERROR_INVALID_PARAMETER (E_INVALIDARG)
    public async Task FromWin32_WrapsErrorCodeWithFacilityWin32(uint win32, uint expectedHResult)
    {
        var result = OpcResultId.FromWin32(win32);

        await Assert.That(unchecked((uint)result.Code)).IsEqualTo(expectedHResult);
        await Assert.That(result.IsFailure).IsTrue();
        await Assert.That(result.Facility).IsEqualTo(OpcFacility.Win32);
    }

    [Test]
    public async Task FromWin32_AlreadyPromoted_IsIdempotent()
    {
        // If caller passes an already-promoted HRESULT (0x80070005), the
        // result should be the same HRESULT (no double-promotion).
        var first = OpcResultId.FromWin32(5u);
        var second = OpcResultId.FromWin32(unchecked((uint)first.Code));

        await Assert.That(unchecked((uint)second.Code)).IsEqualTo(unchecked((uint)first.Code));
    }

    [Test]
    [Arguments(0xC0000022u)]   // STATUS_ACCESS_DENIED
    [Arguments(0xC000006Du)]   // STATUS_LOGON_FAILURE
    [Arguments(0xC0000017u)]   // STATUS_NO_MEMORY
    public async Task FromNtStatus_SetsNBit(uint ntStatus)
    {
        var result = OpcResultId.FromNtStatus(ntStatus);
        const uint NBit = 0x10000000u;

        await Assert.That(unchecked((uint)result.Code) & NBit).IsEqualTo(NBit);
        // The original NTSTATUS bits below the N bit are preserved.
        await Assert.That(unchecked((uint)result.Code) & 0xCFFFFFFFu).IsEqualTo(ntStatus & 0xCFFFFFFFu);
    }

    [Test]
    public async Task NoInterface_HasCanonicalValue()
    {
        await Assert.That(unchecked((uint)OpcResultId.NoInterface.Code)).IsEqualTo(0x80004002u);
    }

    [Test]
    public async Task Pointer_HasCanonicalValue()
    {
        await Assert.That(unchecked((uint)OpcResultId.Pointer.Code)).IsEqualTo(0x80004003u);
    }

    [Test]
    public async Task Abort_HasCanonicalValue()
    {
        await Assert.That(unchecked((uint)OpcResultId.Abort.Code)).IsEqualTo(0x80004004u);
    }

    [Test]
    public async Task AccessDenied_HasCanonicalValue()
    {
        await Assert.That(unchecked((uint)OpcResultId.AccessDenied.Code)).IsEqualTo(0x80070005u);
    }

    [Test]
    public async Task Facility_Win32_IsSeven()
    {
        var win32 = OpcFacility.Win32;
        await Assert.That(win32).IsEqualTo(7);
    }

    [Test]
    public async Task Facility_Opc_EqualsItf_EqualsFour()
    {
        var opc = OpcFacility.Opc;
        var itf = OpcFacility.Itf;
        var facilityOpc = OpcResultId.FacilityOpc;
        await Assert.That(opc).IsEqualTo(4);
        await Assert.That(itf).IsEqualTo(4);
        await Assert.That(facilityOpc).IsEqualTo(opc);
    }
}
