// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Buffers.Binary;

namespace Opc.Classic;

/// <summary>
/// Bidirectional conversions between Windows <c>FILETIME</c> (100-nanosecond
/// ticks since 1601-01-01 00:00:00 UTC) and <see cref="DateTimeOffset"/>.
/// AOT-clean: <see cref="Span{T}"/>-based with no allocations on the hot path.
/// </summary>
/// <remarks>
/// OPC Classic uses Windows <c>FILETIME</c> ubiquitously in its struct types
/// (<c>OPCSERVERSTATUS</c>, <c>OPCITEMSTATE</c>, <c>ONEVENTSTRUCT</c>, HDA
/// time-range bounds, A&amp;E event timestamps, …). The on-the-wire encoding
/// is two little-endian unsigned 32-bit integers (<c>dwLowDateTime</c> then
/// <c>dwHighDateTime</c>) which together form a 64-bit count of 100-nanosecond
/// intervals since the Windows epoch.
/// </remarks>
public static class FileTimeHelper
{
    /// <summary>
    /// Number of 100-nanosecond ticks per second.
    /// </summary>
    public const long TicksPerSecond = 10_000_000L;

    /// <summary>
    /// The Windows <c>FILETIME</c> epoch: 1601-01-01 00:00:00 UTC.
    /// </summary>
    public static readonly DateTimeOffset Epoch =
        new(year: 1601, month: 1, day: 1, hour: 0, minute: 0, second: 0, offset: TimeSpan.Zero);

    /// <summary>
    /// Convert a 64-bit Windows <c>FILETIME</c> (ticks since 1601-01-01 UTC) to
    /// <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <param name="fileTime">Ticks since the Windows epoch (100ns units).</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="fileTime"/> is negative or would overflow
    /// <see cref="DateTimeOffset.MaxValue"/>.
    /// </exception>
    public static DateTimeOffset FromFileTime(long fileTime)
    {
        if (fileTime < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fileTime), fileTime, "FILETIME ticks cannot be negative.");
        }
        // DateTimeOffset interprets its tick origin as 0001-01-01; offset = Epoch.Ticks (504,911,232,000,000,000).
        try
        {
            return new DateTimeOffset(fileTime + Epoch.Ticks, TimeSpan.Zero);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fileTime),
                $"FILETIME ticks ({fileTime}) exceed DateTimeOffset.MaxValue. ({ex.Message})");
        }
    }

    /// <summary>
    /// Attempts to convert a 64-bit Windows <c>FILETIME</c> to
    /// <see cref="DateTimeOffset"/>. Returns false (without throwing) when the
    /// value is negative or would exceed <see cref="DateTimeOffset.MaxValue"/>.
    /// </summary>
    /// <remarks>
    /// Use this overload at decode boundaries where the wire input is
    /// untrusted — e.g. an OPC server returning a sentinel
    /// <c>FILETIME</c> for "not yet known" timestamps. Callers can substitute
    /// a default value or raise a structured decode failure with surrounding
    /// wire context.
    /// </remarks>
    public static bool TryFromFileTime(long fileTime, out DateTimeOffset value)
    {
        if (fileTime < 0)
        {
            value = default;
            return false;
        }

        try
        {
            long ticks = checked(fileTime + Epoch.Ticks);
            if (ticks < 0L || ticks > DateTimeOffset.MaxValue.UtcTicks)
            {
                value = default;
                return false;
            }

            value = new DateTimeOffset(ticks, TimeSpan.Zero);
            return true;
        }
        catch (OverflowException)
        {
            value = default;
            return false;
        }
        catch (ArgumentOutOfRangeException)
        {
            value = default;
            return false;
        }
    }

    /// <summary>
    /// Convert the (low, high) word pair as transmitted on the wire to <see cref="DateTimeOffset"/>.
    /// </summary>
    public static DateTimeOffset FromFileTime(uint dwLowDateTime, uint dwHighDateTime)
        => FromFileTime(((long)dwHighDateTime << 32) | dwLowDateTime);

    /// <summary>
    /// Read an 8-byte little-endian FILETIME from <paramref name="source"/> and
    /// convert to <see cref="DateTimeOffset"/>. The source must be at least
    /// 8 bytes; extra bytes are ignored.
    /// </summary>
    public static DateTimeOffset FromFileTime(ReadOnlySpan<byte> source)
    {
        if (source.Length < 8)
        {
            throw new ArgumentException(
                "FILETIME requires at least 8 bytes.", nameof(source));
        }
        var ticks = BinaryPrimitives.ReadInt64LittleEndian(source[..8]);
        return FromFileTime(ticks);
    }

    /// <summary>
    /// Convert a <see cref="DateTimeOffset"/> to a 64-bit Windows <c>FILETIME</c>.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="value"/> is earlier than the Windows
    /// epoch (1601-01-01 00:00:00 UTC).
    /// </exception>
    public static long ToFileTime(DateTimeOffset value)
    {
        var utc = value.ToUniversalTime();
        if (utc < Epoch)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value), value, "Value is earlier than the FILETIME epoch (1601-01-01 UTC).");
        }
        return utc.Ticks - Epoch.Ticks;
    }

    /// <summary>
    /// Convert a <see cref="DateTimeOffset"/> to the (low, high) word pair as
    /// transmitted on the wire.
    /// </summary>
    public static (uint Low, uint High) ToFileTimeWords(DateTimeOffset value)
    {
        var ticks = ToFileTime(value);
        return ((uint)(ticks & 0xFFFFFFFFL), (uint)((ticks >> 32) & 0xFFFFFFFFL));
    }

    /// <summary>
    /// Write a <see cref="DateTimeOffset"/> as 8 bytes of little-endian FILETIME
    /// into <paramref name="destination"/>. Destination must be at least 8 bytes.
    /// </summary>
    public static void WriteFileTime(DateTimeOffset value, Span<byte> destination)
    {
        if (destination.Length < 8)
        {
            throw new ArgumentException(
                "Destination must be at least 8 bytes.", nameof(destination));
        }
        BinaryPrimitives.WriteInt64LittleEndian(destination[..8], ToFileTime(value));
    }
}
