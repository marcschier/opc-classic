//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0048 // OPC DX status records are grouped by specification section.

namespace Opc.Classic.Dx;

/// <summary>OPC DX <c>OPCError</c> value carried by status records.</summary>
public sealed record DxError(OpcResultId Id, string? Text = null);

/// <summary>OPC DX <c>DXQuality</c> complex type.</summary>
public sealed record DxQuality(
    DxQualityStatus Quality = DxQualityStatus.Unknown,
    DxLimitStatus LimitBits = DxLimitStatus.None,
    ulong VendorBits = 0);

/// <summary>OPC DX <c>ServerStatus</c> complex type from §4.2.</summary>
public sealed record DxServerStatus(
    DxServerState ServerState,
    string? ConfigurationVersion,
    uint DxConnectionCount,
    uint MaxDxConnections,
    bool DirtyFlag,
    OpcResultId ErrorId,
    string? ErrorDiagnostic,
    string[] SourceServerTypes,
    uint MaxQueueSize);

/// <summary>OPC DX <c>DXConnectionStatus</c> complex type from §4.3.2.19.</summary>
public sealed record DxConnectionStatus(
    DxConnectionState DxConnectionState,
    OpcVariant WriteValue,
    DateTimeOffset WriteTimestamp,
    DxQuality WriteQuality,
    OpcResultId WriteErrorId,
    string? WriteErrorDiagnostic,
    OpcVariant SourceValue,
    DateTimeOffset SourceTimestamp,
    DxQuality SourceQuality,
    OpcResultId SourceErrorId,
    string? SourceErrorDiagnostic,
    uint ActualUpdateRate,
    uint QueueHighWaterMark,
    uint QueueFlushCount,
    bool SourceItemConnected,
    bool TargetItemConnected,
    bool Overridden,
    OpcVariant OverrideValue);

/// <summary>OPC DX <c>DXSourceServerStatus</c> complex type from §4.4.1.6.</summary>
public sealed record DxSourceServerStatus(
    DxConnectStatus ConnectStatus,
    OpcResultId ErrorId,
    string? ErrorDiagnostic,
    DateTimeOffset LastConnectTimestamp,
    DateTimeOffset LastConnectFailTimestamp,
    uint ConnectFailCount,
    uint PingTime,
    DateTimeOffset LastDataChangeTimestamp,
    bool SourceServerConnected);

/// <summary>Response shape for <c>QueryDXConnections</c>.</summary>
public sealed record DxConnectionQueryResult(int[] Errors, DxConnection[] Connections);

/// <summary>Response shape for DX operations that return per-mask errors plus a general response.</summary>
public sealed record DxUpdateConnectionsResult(int[] Errors, DxGeneralResponse Response);
