//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Caller-supplied parameters to <see cref="ICaptureSource.StartAsync"/>.
/// </summary>
/// <param name="InterfaceName">
/// Name (or description) of the network interface for live sources;
/// null for sources that don't bind to a NIC.
/// </param>
/// <param name="BpfFilter">
/// Optional Berkeley Packet Filter expression. When null/empty the
/// source applies its default filter (typically the OPC Classic DCOM
/// port set for <see cref="PcapCaptureSource"/>).
/// </param>
/// <param name="Promiscuous">
/// True to open the interface in promiscuous mode (sees frames not
/// addressed to this host). Default true; falls back to non-promiscuous
/// on first open if the OS refuses elevation.
/// </param>
/// <param name="MaxBytes">
/// Stop the capture when the total captured bytes reach this value.
/// Null = engine default (50 MB).
/// </param>
/// <param name="MaxPackets">
/// Stop the capture when the total packet count reaches this value.
/// Null = no per-packet cap.
/// </param>
/// <param name="MaxDurationSeconds">
/// Stop the capture after this many wall-clock seconds since
/// <see cref="ICaptureSource.StartAsync"/>. Null = engine default
/// (1800 = 30 min).
/// </param>
/// <param name="ReplaySourceDirectory">
/// For <c>OpcWireCaptureSource</c>: directory of <c>.hex</c> files to
/// replay as if they were live frames. Ignored by live NIC sources.
/// </param>
public sealed record class CaptureStartRequest(
    string? InterfaceName = null,
    string? BpfFilter = null,
    bool Promiscuous = true,
    long? MaxBytes = null,
    long? MaxPackets = null,
    int? MaxDurationSeconds = null,
    string? ReplaySourceDirectory = null);
