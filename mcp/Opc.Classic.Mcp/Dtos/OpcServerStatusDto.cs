//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>JSON-friendly OPC server runtime status.</summary>
public sealed record OpcServerStatusDto(
    string Spec,
    DateTimeOffset StartTime,
    DateTimeOffset CurrentTime,
    DateTimeOffset LastUpdateTime,
    string State,
    string ServerVersion,
    string VendorInfo,
    int GroupCount,
    uint BandWidth,
    int MaxReturnValues,
    bool IsOperational);
