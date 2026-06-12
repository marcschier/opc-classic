//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable MA0025 // Scaffold is intentionally not implemented yet.
#pragma warning disable VSTHRD200 // Public DA callback terminology uses DataChanges without an Async suffix.

namespace Opc.Classic.Da;

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
