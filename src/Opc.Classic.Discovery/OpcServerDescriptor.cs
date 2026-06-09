//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;

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
