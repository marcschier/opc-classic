//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic.Hda;

/// <summary>
/// OPC HDA built-in aggregate functions per OPC HDA 1.x §3.5.
/// Values are the IDs the server expects on the wire.
/// </summary>
public enum HdaAggregate
{
    /// <summary>No aggregate — return raw values.</summary>
    None = 0,
    /// <summary>Interpolated values across the requested time range.</summary>
    Interpolative = 1,
    /// <summary>Sum of values (rectangular integration). Engineering-unit-time.</summary>
    Total = 2,
    /// <summary>Time-weighted average.</summary>
    Average = 3,
    /// <summary>Time-weighted standard deviation.</summary>
    TimeAverage = 4,
    /// <summary>Count of raw values.</summary>
    Count = 5,
    /// <summary>Standard deviation of raw values.</summary>
    StandardDeviation = 6,
    /// <summary>Minimum actual value over the range.</summary>
    Minimum = 7,
    /// <summary>Time the minimum occurred.</summary>
    MinimumActualTime = 8,
    /// <summary>Maximum actual value over the range.</summary>
    Maximum = 9,
    /// <summary>Time the maximum occurred.</summary>
    MaximumActualTime = 10,
    /// <summary>Start value of the interval.</summary>
    Start = 11,
    /// <summary>End value of the interval.</summary>
    End = 12,
    /// <summary>End value minus start value.</summary>
    Delta = 13,
    /// <summary>Slope of the linear regression line.</summary>
    RegSlope = 14,
    /// <summary>Y-intercept of the linear regression line.</summary>
    RegConst = 15,
    /// <summary>Coefficient of determination (r²).</summary>
    RegDev = 16,
    /// <summary>Variance over the interval.</summary>
    Variance = 17,
    /// <summary>End value minus start value, ignoring quality gaps.</summary>
    Range = 18,
    /// <summary>Difference between minimum and maximum.</summary>
    Duration = 19,
}
