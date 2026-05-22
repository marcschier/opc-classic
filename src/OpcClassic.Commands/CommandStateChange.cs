//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic.Commands;

/// <summary>
/// Server-to-client state-change notification for an OPC Commands invocation.
/// </summary>
public sealed record CommandStateChange(
    Guid InvocationId,
    CommandState NewState,
    int Hresult,
    DateTimeOffset Timestamp);
