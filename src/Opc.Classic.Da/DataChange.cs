//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da;

/// <summary>
/// A single <c>IOPCDataCallback::OnDataChange</c> delivery — the batched
/// payload a subscription pushes to its client when items change (or, in
/// keep-alive mode, periodically to confirm liveness).
/// </summary>
/// <remarks>
/// The OPC DA spec batches per-item updates into a single callback. This
/// type preserves that grouping so a consumer can correlate (e.g., latency
/// metrics, transactional updates) at the callback granularity rather than
/// the per-item one.
/// </remarks>
public sealed class DataChange
{
    /// <summary>
    /// Transaction ID set by the server when responding to an explicit
    /// asynchronous request (Refresh). Zero for spontaneous updates.
    /// </summary>
    public int TransactionId { get; init; }

    /// <summary>
    /// Overall HRESULT for the delivery. Individual items carry their own
    /// <see cref="ItemValueResult.ResultId"/>; this is the server's verdict
    /// on the batch as a whole. Typically <see cref="OpcResultId.Ok"/>.
    /// </summary>
    public OpcResultId MasterResult { get; init; } = OpcResultId.Ok;

    /// <summary>
    /// Per-item updates carried by this callback. Empty list means the
    /// callback is a keep-alive heartbeat (DA 3.0 only) — no item values
    /// changed since the last delivery, but the server is alive.
    /// </summary>
    public IReadOnlyList<ItemValueResult> Items { get; init; } = Array.Empty<ItemValueResult>();

    /// <summary>True if this is a keep-alive heartbeat with no item updates.</summary>
    public bool IsKeepAlive => Items.Count == 0;
}
