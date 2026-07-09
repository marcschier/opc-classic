// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Optional status exposed by transport sinks so groups can drop unreachable callbacks.
/// </summary>
public interface IOpcDataCallbackSinkStatus
{
    /// <summary>
    /// Gets whether the sink has observed a permanent transport failure.
    /// </summary>
    bool IsUnreachable { get; }
}
