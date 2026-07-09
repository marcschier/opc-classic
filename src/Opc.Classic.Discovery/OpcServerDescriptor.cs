// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Discovery;

/// <summary>
/// OPC Classic server metadata returned by OPCEnum / OPC.ServerList.1.
/// </summary>
public sealed record OpcServerDescriptor(
    Guid ClassId,
    string ProgId,
    string UserType,
    string? VerIndProgId,
    IReadOnlyList<Guid> Categories);
