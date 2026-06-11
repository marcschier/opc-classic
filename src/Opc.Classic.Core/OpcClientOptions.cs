//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic;

/// <summary>
/// Client-side configuration for the Opc.Classic stack: operation timeout
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

    /// <summary>Maximum decoded NDR payload size in bytes. Defaults to 16 MiB.</summary>
    public int MaxNdrPayloadSize { get; init; } = 16 * 1024 * 1024;

    /// <summary>Maximum NTLMSSP message size in bytes. Defaults to the 64 KiB security-buffer ceiling.</summary>
    public int MaxNtlmMessageSize { get; init; } = 64 * 1024 - 1;

    /// <summary>Maximum SMB2 message size in bytes. Defaults to the repository's NetBIOS frame ceiling.</summary>
    public int MaxSmb2MessageSize { get; init; } = 0x1FFFF;
}
