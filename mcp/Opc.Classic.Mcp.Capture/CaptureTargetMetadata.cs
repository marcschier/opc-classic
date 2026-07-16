// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Mcp.Capture;

/// <summary>
/// Target resolution metadata collected after a live capture is already running.
/// </summary>
public sealed record class CaptureTargetMetadata
{
    public string? Host { get; init; }
    public string? ProgId { get; init; }
    public Guid? Clsid { get; init; }
    public string? ConnectionString { get; init; }
    public string Status { get; init; } = "not_requested";
    public IReadOnlyList<string> Bindings { get; init; } = [];
    public IReadOnlyList<int> Ports { get; init; } = [];
    public Guid? Oxid { get; init; }
    public Guid? IpidRemUnknown { get; init; }
    public uint? AuthenticationHint { get; init; }
    public string? ServerVersion { get; init; }
    public string? Error { get; init; }
}
