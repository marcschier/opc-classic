// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Hda.Tests;

public sealed class OpcHdaResultIdTests
{
    [Test]
    public async Task MaxExceeded_HasExpectedCode()
    {
        await Assert.That(OpcHdaResultId.MaxExceeded.Code).IsEqualTo(unchecked((int)0xC0041001u));
        await Assert.That(OpcHdaResultId.MaxExceeded.IsFailure).IsTrue();
    }

    [Test]
    public async Task NoData_HasSuccessSeverity()
    {
        // 0x40041002: severity=0 (success), customer-bit=1, facility=ITF.
        await Assert.That(OpcHdaResultId.NoData.Code).IsEqualTo(0x40041002);
        await Assert.That(OpcHdaResultId.NoData.IsSuccess).IsTrue();
    }

    [Test]
    public async Task MoreData_CurrentValue_ExtraData_AreSuccess()
    {
        await Assert.That(OpcHdaResultId.MoreData.IsSuccess).IsTrue();
        await Assert.That(OpcHdaResultId.CurrentValue.IsSuccess).IsTrue();
        await Assert.That(OpcHdaResultId.ExtraData.IsSuccess).IsTrue();
    }

    [Test]
    public async Task InvalidAggregate_UnknownAttrId_NotAvail_AreFailure()
    {
        await Assert.That(OpcHdaResultId.InvalidAggregate.IsFailure).IsTrue();
        await Assert.That(OpcHdaResultId.UnknownAttrId.IsFailure).IsTrue();
        await Assert.That(OpcHdaResultId.NotAvail.IsFailure).IsTrue();
    }

    [Test]
    public async Task InvalidDataType_DataExists_InvalidAttrId_NoDataExists_HaveExpectedCodes()
    {
        await Assert.That(OpcHdaResultId.InvalidDataType.Code).IsEqualTo(unchecked((int)0xC004100Au));
        await Assert.That(OpcHdaResultId.DataExists.Code).IsEqualTo(unchecked((int)0xC004100Bu));
        await Assert.That(OpcHdaResultId.InvalidAttrId.Code).IsEqualTo(unchecked((int)0xC004100Cu));
        await Assert.That(OpcHdaResultId.NoDataExists.Code).IsEqualTo(unchecked((int)0xC004100Du));
    }

    [Test]
    public async Task Inserted_Replaced_AreSuccess()
    {
        await Assert.That(OpcHdaResultId.Inserted.Code).IsEqualTo(0x4004100E);
        await Assert.That(OpcHdaResultId.Inserted.IsSuccess).IsTrue();
        await Assert.That(OpcHdaResultId.Replaced.Code).IsEqualTo(0x4004100F);
        await Assert.That(OpcHdaResultId.Replaced.IsSuccess).IsTrue();
    }
}
