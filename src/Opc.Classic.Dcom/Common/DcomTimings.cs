//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Common;

/// <summary>
/// DCOM OXID ping timing defaults and runtime overrides.
/// </summary>
public static class DcomTimings
{
    /// <summary>
    /// MS-DCOM §3.1.4.1 PingPeriod constant.
    /// </summary>
    public static TimeSpan SpecMandatedPingPeriod { get; } = TimeSpan.FromSeconds(80);

    /// <summary>
    /// Maximum tolerated ping period per MS-DCOM §3.1.4.1.
    /// </summary>
    public static TimeSpan MaximumAllowedPingPeriod { get; } = TimeSpan.FromSeconds(120);

    /// <summary>
    /// Default server-side object expiry period: two PingPeriod intervals.
    /// </summary>
    public static TimeSpan DefaultObjectExpiryPeriod { get; } = TimeSpan.FromSeconds(160);

    /// <summary>
    /// Client-side OXID ping period.
    /// </summary>
    public static TimeSpan PingPeriod
    {
        get => _pingPeriod;
        set
        {
            ValidatePingPeriod(value);
            if (_objectExpiryPeriod < Double(value))
            {
                throw new ArgumentException("ObjectExpiryPeriod must be at least twice PingPeriod.", nameof(value));
            }

            _pingPeriod = value;
        }
    }

    /// <summary>
    /// Server-side object expiry period for references that stop receiving OXID pings.
    /// </summary>
    public static TimeSpan ObjectExpiryPeriod
    {
        get => _objectExpiryPeriod;
        set
        {
            if (value < Double(_pingPeriod))
            {
                throw new ArgumentException("ObjectExpiryPeriod must be at least twice PingPeriod.", nameof(value));
            }

            _objectExpiryPeriod = value;
        }
    }

    internal static DateTimeOffset UtcNow => TimeProvider.GetUtcNow();

    internal static TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    internal static void ResetForTesting()
    {
        _pingPeriod = SpecMandatedPingPeriod;
        _objectExpiryPeriod = DefaultObjectExpiryPeriod;
        TimeProvider = TimeProvider.System;
    }

    private static void ValidatePingPeriod(TimeSpan value)
    {
        if (value < MinimumPingPeriod || value > MaximumAllowedPingPeriod)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"PingPeriod must be between {MinimumPingPeriod} and {MaximumAllowedPingPeriod}.");
        }
    }

    private static TimeSpan Double(TimeSpan value) => TimeSpan.FromTicks(value.Ticks * 2);

    private static readonly TimeSpan MinimumPingPeriod = TimeSpan.FromSeconds(1);
    private static TimeSpan _pingPeriod = SpecMandatedPingPeriod;
    private static TimeSpan _objectExpiryPeriod = DefaultObjectExpiryPeriod;
}
