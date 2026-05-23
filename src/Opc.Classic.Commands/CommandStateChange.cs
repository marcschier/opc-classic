//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Commands;

/// <summary>
/// Server-to-client state-change notification for an OPC Commands invocation.
/// </summary>
public sealed record CommandStateChange(
    Guid InvocationId,
    CommandState NewState,
    int Hresult,
    DateTimeOffset Timestamp);
