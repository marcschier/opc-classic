// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Dtos;

/// <summary>
/// JSON-friendly OPC Security state and capability information.
/// </summary>
public sealed record OpcSecurityInfoDto(
    bool SupportsWindowsAuthentication,
    bool SupportsPrivateAuthentication,
    bool IsAuthenticated,
    string CurrentIdentity,
    string Message);
