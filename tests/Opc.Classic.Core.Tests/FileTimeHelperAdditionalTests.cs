// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Tests;

public sealed class FileTimeHelperAdditionalTests
{
    [Test]
    public async Task FromFileTime_MaxRepresentableFileTime_ReturnsDateTimeOffsetMaxValue()
    {
        DateTimeOffset value = FileTimeHelper.FromFileTime(2_650_467_743_999_999_999L);

        await Assert.That(value).IsEqualTo(DateTimeOffset.MaxValue);
    }

    [Test]
    public async Task ToFileTime_MaxRepresentableDateTime_ReturnsExpectedRawTicksAndWords()
    {
        long fileTime = FileTimeHelper.ToFileTime(DateTimeOffset.MaxValue);
        (uint low, uint high) = FileTimeHelper.ToFileTimeWords(DateTimeOffset.MaxValue);

        await Assert.That(fileTime).IsEqualTo(2_650_467_743_999_999_999L);
        await Assert.That(low).IsEqualTo(0xD1C03FFFu);
        await Assert.That(high).IsEqualTo(0x24C85A5Eu);
    }

    [Test]
    public async Task FromFileTime_LowWordOnly_UsesUnsignedLowWord()
    {
        DateTimeOffset value = FileTimeHelper.FromFileTime(0xFFFFFFFFu, 0u);

        await Assert.That(value).IsEqualTo(FileTimeHelper.Epoch.AddTicks(4_294_967_295L));
    }

    [Test]
    public async Task FromFileTime_ShortSpan_ThrowsArgumentException()
    {
        await Assert.That(() => FileTimeHelper.FromFileTime(new byte[7]))
            .Throws<ArgumentException>();
    }

    [Test]
    public async Task WriteFileTime_ShortDestination_ThrowsArgumentException()
    {
        await Assert.That(() => FileTimeHelper.WriteFileTime(FileTimeHelper.Epoch, new byte[7]))
            .Throws<ArgumentException>();
    }
}
