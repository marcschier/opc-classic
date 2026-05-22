//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

#pragma warning disable VSTHRD200 // Public DA callback terminology uses DataChanges without an Async suffix.

using System;
using System.Collections.Generic;
using System.Threading;

namespace OpcClassic.Da;

/// <summary>
/// Client-side OPC DA subscription contract exposing pushed data changes as an async stream.
/// </summary>
public interface IOpcDaSubscription : IAsyncDisposable
{
    /// <summary>Streams incoming <c>IOPCDataCallback::OnDataChange</c> deliveries.</summary>
    IAsyncEnumerable<OpcDaDataChange> DataChanges(CancellationToken ct = default);
}
