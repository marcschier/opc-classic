// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

#pragma warning disable MA0048 // Endpoint contracts and their value records form one API surface.

namespace Opc.Classic.Dx;

/// <summary>
/// Connectivity state reported by a DX data endpoint.
/// </summary>
public enum DxEndpointHealthState
{
    /// <summary>The endpoint has not supplied health information.</summary>
    Unknown,
    /// <summary>The endpoint is available for transfers.</summary>
    Healthy,
    /// <summary>The endpoint is available but degraded.</summary>
    Degraded,
    /// <summary>The endpoint is disconnected.</summary>
    Disconnected,
    /// <summary>The endpoint has failed.</summary>
    Failed,
}

/// <summary>
/// A point-in-time endpoint health report.
/// </summary>
public sealed record DxEndpointHealth(
    DxEndpointHealthState State,
    DateTimeOffset Timestamp,
    OpcResultId ErrorId,
    string? Diagnostic = null)
{
    /// <summary>
    /// Whether the endpoint is currently usable for transfers.
    /// </summary>
    public bool IsAvailable =>
        State is DxEndpointHealthState.Healthy or DxEndpointHealthState.Degraded;
}

/// <summary>
/// Identifies a DA item without depending on a concrete DA client.
/// </summary>
public sealed record DxDataItem
{
    /// <summary>
    /// Creates an item identifier.
    /// </summary>
    public DxDataItem(string itemName, string? itemPath = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemName);
        ItemName = itemName;
        ItemPath = itemPath;
    }

    /// <summary>
    /// DA item name.
    /// </summary>
    public string ItemName { get; }

    /// <summary>
    /// Optional DA item path.
    /// </summary>
    public string? ItemPath { get; }
}

/// <summary>
/// A source value with OPC quality and per-item error information.
/// </summary>
public sealed record DxDataValue(
    OpcVariant Value,
    DateTimeOffset Timestamp,
    DxQuality Quality,
    OpcResultId ErrorId,
    string? ErrorDiagnostic = null)
{
    /// <summary>
    /// Whether the source operation succeeded.
    /// </summary>
    public bool IsSuccess => ErrorId.IsSuccess;
}

/// <summary>
/// A source read result correlated to its requested item.
/// </summary>
public sealed record DxReadResult(DxDataItem Item, DxDataValue Data);

/// <summary>
/// A value-quality-timestamp request for a target item.
/// </summary>
public sealed record DxWriteRequest(
    DxDataItem Item,
    OpcVariant Value,
    DxQuality Quality,
    DateTimeOffset Timestamp);

/// <summary>
/// A target write result with per-item error information.
/// </summary>
public sealed record DxWriteResult(
    DxDataItem Item,
    OpcResultId ErrorId,
    string? ErrorDiagnostic = null)
{
    /// <summary>
    /// Whether the target write succeeded.
    /// </summary>
    public bool IsSuccess => ErrorId.IsSuccess;
}

/// <summary>
/// Common lifecycle and health operations for a DX data endpoint.
/// </summary>
public interface IDxDataEndpoint
{
    /// <summary>
    /// Stable endpoint name used in diagnostics.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets current connectivity and failure information.
    /// </summary>
    ValueTask<DxEndpointHealth> GetHealthAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-establishes the underlying connection.
    /// </summary>
    ValueTask ReconnectAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// DA-shaped source and target operations consumed by the DX reference engine.
/// </summary>
public interface IDxDaAdapter : IDxDataEndpoint
{
    /// <summary>
    /// Reads source items in request order.
    /// </summary>
    ValueTask<IReadOnlyList<DxReadResult>> ReadAsync(
        IReadOnlyList<DxDataItem> items,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Writes target items in request order.
    /// </summary>
    ValueTask<IReadOnlyList<DxWriteResult>> WriteAsync(
        IReadOnlyList<DxWriteRequest> requests,
        CancellationToken cancellationToken = default);
}
