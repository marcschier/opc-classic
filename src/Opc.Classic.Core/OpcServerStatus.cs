// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic;

/// <summary>
/// Unified managed representation of an OPC server's runtime status, applicable
/// to DA (<c>OPCSERVERSTATUS</c>), AE (<c>OPCEVENTSERVERSTATUS</c>), and
/// HDA (<c>OPCHDA_SERVERSTATUS</c>).
/// </summary>
/// <remarks>
/// Fields that are spec-specific carry sensible defaults (typically zero) when
/// inapplicable to the originating server. Use <see cref="Spec"/> to disambiguate.
/// </remarks>
public sealed class OpcServerStatus
{
    /// <summary>
    /// The OPC specification that produced this status.
    /// </summary>
    public OpcStatusSpec Spec { get; init; }

    /// <summary>
    /// Server start time (UTC).
    /// </summary>
    public DateTimeOffset StartTime { get; init; }

    /// <summary>
    /// Server current time at the moment the status was sampled (UTC).
    /// </summary>
    public DateTimeOffset CurrentTime { get; init; }

    /// <summary>
    /// Last update time — meaning is spec-dependent:
    /// for DA, the most recent cache update; for AE, the most recent event
    /// delivered; for HDA, the most recent historical sample stored.
    /// </summary>
    public DateTimeOffset LastUpdateTime { get; init; }

    /// <summary>
    /// Runtime state.
    /// </summary>
    public OpcServerState State { get; init; }

    /// <summary>
    /// Server version (major.minor.build).
    /// </summary>
    public Version ServerVersion { get; init; } = new Version(0, 0, 0);

    /// <summary>
    /// Server-supplied vendor / product description string.
    /// </summary>
    public string VendorInfo { get; init; } = string.Empty;

    /// <summary>
    /// DA-only: number of groups currently created on the server (0 for non-DA).
    /// </summary>
    public int GroupCount { get; init; }

    /// <summary>
    /// DA-only: server-reported bandwidth utilization, 0..10000 representing
    /// 0%..100% in hundredths of a percent (0xFFFFFFFF if not supported / non-DA).
    /// </summary>
    public uint BandWidth { get; init; }

    /// <summary>
    /// HDA-only: server-reported maximum number of values returnable in a single
    /// read call (0 for non-HDA).
    /// </summary>
    public int MaxReturnValues { get; init; }

    /// <summary>
    /// The status reading is "good": <see cref="State"/> is <see cref="OpcServerState.Running"/>.
    /// </summary>
    public bool IsOperational => State is OpcServerState.Running;

    public override string ToString()
        => $"{Spec} server v{ServerVersion} '{VendorInfo}' state={State} current={CurrentTime:O}";
}
