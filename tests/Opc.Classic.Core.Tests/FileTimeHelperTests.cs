// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using TUnit.Assertions.AssertConditions.Throws;

namespace Opc.Classic.Tests;

public sealed class FileTimeHelperTests
{
    [Test]
    public async Task Epoch_IsWindowsFileTimeOrigin()
    {
        await Assert.That(FileTimeHelper.Epoch)
            .IsEqualTo(new DateTimeOffset(1601, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }

    [Test]
    public async Task FromFileTime_Zero_IsEpoch()
    {
        var dt = FileTimeHelper.FromFileTime(0L);
        await Assert.That(dt).IsEqualTo(FileTimeHelper.Epoch);
    }

    [Test]
    public async Task FromFileTime_OneSecond_AdvancesByOneSecond()
    {
        var dt = FileTimeHelper.FromFileTime(FileTimeHelper.TicksPerSecond);
        var expected = FileTimeHelper.Epoch.AddSeconds(1);
        await Assert.That(dt).IsEqualTo(expected);
    }

    [Test]
    public async Task FromFileTime_LowHighWords_RecombineCorrectly()
    {
        // 1 January 2000 UTC == FILETIME = 125911584000000000 ticks
        const long jan1_2000 = 125_911_584_000_000_000L;
        var low = (uint)(jan1_2000 & 0xFFFFFFFFL);
        var high = (uint)((jan1_2000 >> 32) & 0xFFFFFFFFL);

        var dt = FileTimeHelper.FromFileTime(low, high);
        await Assert.That(dt).IsEqualTo(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero));
    }

    [Test]
    public async Task ToFileTime_Epoch_IsZero()
    {
        await Assert.That(FileTimeHelper.ToFileTime(FileTimeHelper.Epoch)).IsEqualTo(0L);
    }

    [Test]
    public async Task ToFileTime_AndBack_RoundTrip()
    {
        var original = new DateTimeOffset(2026, 5, 21, 12, 34, 56, 789, TimeSpan.Zero);
        var ticks = FileTimeHelper.ToFileTime(original);
        var roundTripped = FileTimeHelper.FromFileTime(ticks);
        await Assert.That(roundTripped).IsEqualTo(original);
    }

    [Test]
    public async Task ToFileTime_OffsetTime_NormalizedToUtc()
    {
        // Same instant, different offsets — both must produce the same FILETIME.
        var utc = new DateTimeOffset(2026, 5, 21, 12, 0, 0, TimeSpan.Zero);
        var plus2 = new DateTimeOffset(2026, 5, 21, 14, 0, 0, TimeSpan.FromHours(2));

        await Assert.That(FileTimeHelper.ToFileTime(utc))
            .IsEqualTo(FileTimeHelper.ToFileTime(plus2));
    }

    [Test]
    public async Task ToFileTime_BeforeEpoch_Throws()
    {
        var preEpoch = new DateTimeOffset(1600, 12, 31, 23, 59, 59, TimeSpan.Zero);
        await Assert.That(() => { FileTimeHelper.ToFileTime(preEpoch); })
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task FromFileTime_Negative_Throws()
    {
        await Assert.That(() => { FileTimeHelper.FromFileTime(-1L); })
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task FromFileTime_Span_DecodesLittleEndian()
    {
        // FILETIME for 1601-01-01 00:00:01 UTC = TicksPerSecond.
        Span<byte> bytes = stackalloc byte[8];
        System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(bytes, FileTimeHelper.TicksPerSecond);

        var dt = FileTimeHelper.FromFileTime(bytes);
        await Assert.That(dt).IsEqualTo(FileTimeHelper.Epoch.AddSeconds(1));
    }

    [Test]
    public async Task WriteFileTime_RoundTrips_ThroughSpan()
    {
        var original = new DateTimeOffset(2042, 1, 15, 8, 30, 0, TimeSpan.Zero);

        Span<byte> bytes = stackalloc byte[8];
        FileTimeHelper.WriteFileTime(original, bytes);

        var decoded = FileTimeHelper.FromFileTime(bytes);
        await Assert.That(decoded).IsEqualTo(original);
    }

    [Test]
    public async Task ToFileTimeWords_RecomposeRoundTrip()
    {
        var original = new DateTimeOffset(2026, 5, 21, 12, 34, 56, TimeSpan.Zero);
        var (low, high) = FileTimeHelper.ToFileTimeWords(original);
        var decoded = FileTimeHelper.FromFileTime(low, high);
        await Assert.That(decoded).IsEqualTo(original);
    }

    [Test]
    public async Task TryFromFileTime_Zero_IsEpoch()
    {
        bool ok = FileTimeHelper.TryFromFileTime(0L, out DateTimeOffset value);
        await Assert.That(ok).IsTrue();
        await Assert.That(value).IsEqualTo(FileTimeHelper.Epoch);
    }

    [Test]
    public async Task TryFromFileTime_OneSecond_AdvancesByOneSecond()
    {
        bool ok = FileTimeHelper.TryFromFileTime(FileTimeHelper.TicksPerSecond, out DateTimeOffset value);
        await Assert.That(ok).IsTrue();
        await Assert.That(value).IsEqualTo(FileTimeHelper.Epoch.AddSeconds(1));
    }

    [Test]
    [Arguments(long.MinValue)]
    [Arguments(-1L)]
    [Arguments(long.MaxValue)]
    public async Task TryFromFileTime_OutOfRange_ReturnsFalse(long fileTime)
    {
        bool ok = FileTimeHelper.TryFromFileTime(fileTime, out DateTimeOffset value);
        await Assert.That(ok).IsFalse();
        await Assert.That(value).IsEqualTo(default(DateTimeOffset));
    }

    [Test]
    public async Task TryFromFileTime_MaxRepresentable_IsAccepted()
    {
        long maxRaw = DateTimeOffset.MaxValue.UtcTicks - FileTimeHelper.Epoch.UtcTicks;
        bool ok = FileTimeHelper.TryFromFileTime(maxRaw, out DateTimeOffset value);
        await Assert.That(ok).IsTrue();
        await Assert.That(value.Year).IsEqualTo(9999);
    }

    [Test]
    public async Task TryFromFileTime_OneTickOverMax_ReturnsFalse()
    {
        long oneTickOver = DateTimeOffset.MaxValue.UtcTicks - FileTimeHelper.Epoch.UtcTicks + 1;
        bool ok = FileTimeHelper.TryFromFileTime(oneTickOver, out _);
        await Assert.That(ok).IsFalse();
    }
}
