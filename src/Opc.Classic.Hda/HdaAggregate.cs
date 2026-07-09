// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Hda;

/// <summary>
/// OPC HDA built-in aggregate functions per OPC HDA 1.20 §5.3.3.
/// Values are the IDs the server expects on the wire.
/// </summary>
/// <remarks>
/// Each member's XML documentation includes the spec's canonical
/// <c>OPCHDA_*</c> identifier and value. Callers that need to pass raw
/// integer IDs (for vendor-specific aggregates outside the standard
/// catalogue) can cast directly to <c>int</c> — wire compatibility is
/// preserved.
/// </remarks>
public enum HdaAggregate
{
    /// <summary>
    /// <c>OPCHDA_NOAGGREGATE</c> (0): do not retrieve an aggregate.
    /// </summary>
    None = 0,

    /// <summary>
    /// <c>OPCHDA_INTERPOLATIVE</c> (1): interpolated values across the requested time range.
    /// </summary>
    Interpolative = 1,

    /// <summary>
    /// <c>OPCHDA_TOTAL</c> (2): totalized value (time integral) over the resample interval.
    /// </summary>
    Total = 2,

    /// <summary>
    /// <c>OPCHDA_AVERAGE</c> (3): average data over the resample interval.
    /// </summary>
    Average = 3,

    /// <summary>
    /// <c>OPCHDA_TIMEAVERAGE</c> (4): time-weighted average data over the resample interval.
    /// </summary>
    TimeAverage = 4,

    /// <summary>
    /// <c>OPCHDA_COUNT</c> (5): number of raw values over the resample interval.
    /// </summary>
    Count = 5,

    /// <summary>
    /// <c>OPCHDA_STDEV</c> (6): standard deviation over the resample interval.
    /// </summary>
    StandardDeviation = 6,

    /// <summary>
    /// <c>OPCHDA_MINIMUMACTUALTIME</c> (7): minimum value and the timestamp of the minimum value.
    /// </summary>
    /// <remarks>
    /// Note: the OPC HDA 1.20 spec uses ID 7 for <c>MINIMUMACTUALTIME</c>
    /// (minimum with timestamp) and ID 8 for <c>MINIMUM</c> (value only).
    /// Legacy code may have referred to ID 7 as plain <c>Minimum</c> — that
    /// name is preserved as an alias on the obsolete member below.
    /// </remarks>
    MinimumActualTime = 7,

    /// <summary>
    /// <c>OPCHDA_MINIMUM</c> (8): minimum value in the resample interval.
    /// </summary>
    Minimum = 8,

    /// <summary>
    /// <c>OPCHDA_MAXIMUMACTUALTIME</c> (9): maximum value and the timestamp of the maximum value.
    /// </summary>
    MaximumActualTime = 9,

    /// <summary>
    /// <c>OPCHDA_MAXIMUM</c> (10): maximum value in the resample interval.
    /// </summary>
    Maximum = 10,

    /// <summary>
    /// <c>OPCHDA_START</c> (11): value at the beginning of the resample interval.
    /// </summary>
    Start = 11,

    /// <summary>
    /// <c>OPCHDA_END</c> (12): value at the end of the resample interval.
    /// </summary>
    End = 12,

    /// <summary>
    /// <c>OPCHDA_DELTA</c> (13): difference between the first and last value in the resample interval.
    /// </summary>
    Delta = 13,

    /// <summary>
    /// <c>OPCHDA_REGSLOPE</c> (14): slope of the regression line over the resample interval.
    /// </summary>
    RegSlope = 14,

    /// <summary>
    /// <c>OPCHDA_REGCONST</c> (15): intercept of the regression line over the resample interval.
    /// </summary>
    RegConst = 15,

    /// <summary>
    /// <c>OPCHDA_REGDEV</c> (16): standard deviation of the regression line over the resample interval.
    /// </summary>
    RegDev = 16,

    /// <summary>
    /// <c>OPCHDA_VARIANCE</c> (17): variance over the sample interval.
    /// </summary>
    Variance = 17,

    /// <summary>
    /// <c>OPCHDA_RANGE</c> (18): difference between the minimum and maximum value over the sample interval.
    /// </summary>
    Range = 18,

    /// <summary>
    /// <c>OPCHDA_DURATIONGOOD</c> (19): duration in seconds during which the data is good.
    /// </summary>
    DurationGood = 19,

    /// <summary>
    /// <c>OPCHDA_DURATIONBAD</c> (20): duration in seconds during which the data is bad.
    /// </summary>
    DurationBad = 20,

    /// <summary>
    /// <c>OPCHDA_PERCENTGOOD</c> (21): percent of data in the interval with good quality.
    /// </summary>
    PercentGood = 21,

    /// <summary>
    /// <c>OPCHDA_PERCENTBAD</c> (22): percent of data in the interval with bad quality.
    /// </summary>
    PercentBad = 22,

    /// <summary>
    /// <c>OPCHDA_WORSTQUALITY</c> (23): worst quality of data in the interval.
    /// </summary>
    WorstQuality = 23,

    /// <summary>
    /// <c>OPCHDA_ANNOTATIONS</c> (24): number of annotations in the interval.
    /// </summary>
    Annotations = 24,
}
