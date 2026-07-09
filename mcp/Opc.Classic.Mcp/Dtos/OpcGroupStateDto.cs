// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC DA group state.
/// </summary>
public sealed record OpcGroupStateDto(
    int ServerGroupHandle,
    int ClientHandle,
    string? Name,
    bool Active,
    int UpdateRateMs,
    int RevisedUpdateRateMs,
    int TimeBiasMinutes,
    float DeadbandPercent,
    int LocaleId,
    int KeepAliveMs,
    int ItemCount);
