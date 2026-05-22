//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic;

/// <summary>
/// Client-side configuration for the OpcClassic stack: operation timeout
/// + future fields (retry policy, circuit-breaker config).
/// </summary>
public sealed record OpcClientOptions
{
    /// <summary>
    /// Default per-operation timeout. Applied to every InvokeAsync that
    /// doesn't pass its own CancellationToken with a deadline. Defaults
    /// to <c>00:00:30</c>.
    /// </summary>
    public System.TimeSpan OperationTimeout { get; init; } = System.TimeSpan.FromSeconds(30);

    /// <summary>
    /// Whether to enable the Polly circuit-breaker around all DCOM calls.
    /// Off by default; opt-in via DI configuration.
    /// </summary>
    public bool EnableCircuitBreaker { get; init; }
}
