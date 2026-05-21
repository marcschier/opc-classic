//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic.Dx;

/// <summary>
/// A DX connection — a configured data flow from a source-server item to a
/// target-server item. Mirrors <c>DXConnection</c> from OPC DX 1.0 §4.3.
/// </summary>
/// <remarks>
/// One DX server hosts many connections. Each connection is independently
/// state-tracked (<see cref="State"/>) and has its own QoS settings
/// (<see cref="DeadbandPercent"/>, <see cref="UpdateRateMs"/>).
/// </remarks>
public sealed class DxConnection
{
    /// <summary>Server-assigned identifier for this connection.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>The friendly name of the <see cref="SourceServer"/> this connection draws from.</summary>
    public string SourceServerName { get; init; } = string.Empty;

    /// <summary>The source-server item ID (what's being read at the source).</summary>
    public string SourceItemId { get; init; } = string.Empty;

    /// <summary>Optional source-server access path.</summary>
    public string? SourceItemPath { get; init; }

    /// <summary>The local-server item ID (where the value is written / mirrored).</summary>
    public string TargetItemId { get; init; } = string.Empty;

    /// <summary>Optional target-server access path.</summary>
    public string? TargetItemPath { get; init; }

    /// <summary>
    /// Update rate (milliseconds) at which the source is subscribed. Subject
    /// to source-server clamping (just like a DA group's update rate).
    /// </summary>
    public int UpdateRateMs { get; init; }

    /// <summary>Deadband percentage applied to source updates.</summary>
    public float DeadbandPercent { get; init; }

    /// <summary>
    /// Whether the configured <see cref="OverrideValue"/> is currently being
    /// substituted for live source data.
    /// </summary>
    public OverrideState OverrideState { get; init; } = OverrideState.Disabled;

    /// <summary>Value to substitute when <see cref="OverrideState"/> is Enabled.</summary>
    public object? OverrideValue { get; init; }

    /// <summary>Current connection state (server-reported).</summary>
    public ConnectionState State { get; init; } = ConnectionState.Initial;

    /// <summary>When non-null, the time the connection most recently reached its current state.</summary>
    public DateTimeOffset? LastStateChange { get; init; }
}
