//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable VSTHRD200 // Public DA callback terminology uses DataChanges without an Async suffix.

namespace Opc.Classic.Da;

/// <summary>
/// Client-side OPC DA subscription contract exposing pushed data changes as an async stream.
/// </summary>
public interface IOpcDaSubscription : IAsyncDisposable
{
    /// <summary>Streams incoming <c>IOPCDataCallback::OnDataChange</c> deliveries.</summary>
    IAsyncEnumerable<OpcDaDataChange> DataChanges(CancellationToken ct = default);
}
