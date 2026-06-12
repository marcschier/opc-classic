//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Hda;

/// <summary>
/// The historical values returned for a single item by a read-raw,
/// read-processed, or read-at-time call.
/// </summary>
public sealed class HdaReadResult
{
    /// <summary>
    /// The item these values belong to.
    /// </summary>
    public string ItemId { get; init; } = string.Empty;

    /// <summary>
    /// Per-item HRESULT.
    /// </summary>
    public OpcResultId ResultId { get; init; } = OpcResultId.Ok;

    /// <summary>
    /// The historical values in chronological order.
    /// </summary>
    public IReadOnlyList<HdaItemValue> Values { get; init; } = Array.Empty<HdaItemValue>();

    /// <summary>
    /// For paged reads: server-supplied continuation handle. Pass to a
    /// follow-up <c>ReadRawAsync</c> to retrieve more values, or
    /// <see langword="null"/> when the read is complete.
    /// </summary>
    public int? ContinuationHandle { get; init; }
}
