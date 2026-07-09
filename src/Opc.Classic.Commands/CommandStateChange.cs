// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Commands;

/// <summary>
/// Server-to-client state-change notification for an OPC Commands invocation.
/// </summary>
public sealed record CommandStateChange(
    Guid InvocationId,
    CommandState NewState,
    int Hresult,
    DateTimeOffset Timestamp);
