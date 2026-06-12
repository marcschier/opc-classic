//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC DX connection definition.
/// </summary>
public sealed record OpcDxConnectionDto(
    string? Name = null,
    string? Description = null,
    string? ItemPath = null,
    string? ItemName = null,
    string? Version = null,
    string[]? BrowsePaths = null,
    string? Keyword = null,
    bool? DefaultSourceItemConnected = null,
    bool? DefaultTargetItemConnected = null,
    bool? DefaultOverridden = null,
    object? DefaultOverrideValue = null,
    object? SubstituteValue = null,
    bool? EnableSubstituteValue = null,
    string? TargetItemPath = null,
    string? TargetItemName = null,
    string? SourceServerName = null,
    string? SourceItemPath = null,
    string? SourceItemName = null,
    int? SourceItemQueueSize = null,
    int? UpdateRateMilliseconds = null,
    float? DeadbandPercent = null,
    string? VendorData = null,
    int Mask = 0);
