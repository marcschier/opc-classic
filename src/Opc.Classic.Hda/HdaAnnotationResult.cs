//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace Opc.Classic.Hda;

/// <summary>
/// Annotations returned for a single item by
/// <see cref="IHdaServer.ReadAnnotationsAsync"/>.
/// </summary>
public sealed class HdaAnnotationResult
{
    /// <summary>The item these annotations belong to.</summary>
    public string ItemId { get; init; } = string.Empty;

    /// <summary>Per-item HRESULT.</summary>
    public OpcResultId ResultId { get; init; } = OpcResultId.Ok;

    /// <summary>The annotations in chronological order.</summary>
    public IReadOnlyList<HdaAnnotation> Annotations { get; init; } = Array.Empty<HdaAnnotation>();
}
