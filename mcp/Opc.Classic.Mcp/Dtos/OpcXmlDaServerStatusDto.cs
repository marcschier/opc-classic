//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Mcp.Dtos;

/// <summary>JSON-friendly OPC XML-DA server status.</summary>
public sealed record OpcXmlDaServerStatusDto(
    DateTimeOffset StartTime,
    string ProductVersion,
    string VendorInfo,
    IReadOnlyList<string> SupportedLocaleIds,
    IReadOnlyList<string> SupportedInterfaceVersions,
    string ServerState,
    string? StatusInfo);
