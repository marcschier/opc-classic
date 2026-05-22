//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

#pragma warning disable MA0025 // This Phase 6D scaffold is intentionally not implemented yet.
#pragma warning disable VSTHRD200 // Public DA callback terminology uses DataChanges without an Async suffix.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpcClassic.Da;

/// <summary>
/// Placeholder for the LocalCoClass-backed OPC DA subscription sink implementation.
/// </summary>
public sealed class OpcDaSubscription : IOpcDaSubscription
{
    /// <inheritdoc />
    public IAsyncEnumerable<OpcDaDataChange> DataChanges(CancellationToken ct = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public ValueTask DisposeAsync() => throw new NotImplementedException();
}
