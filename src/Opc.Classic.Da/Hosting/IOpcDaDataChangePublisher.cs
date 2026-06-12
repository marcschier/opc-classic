//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Server-side data-change publisher. The user's IOpcDaServer implementation
/// produces OpcDaDataChange instances; the publisher fan-outs them to all
/// advised IOPCDataCallback subscribers registered via IConnectionPoint.
/// </summary>
public interface IOpcDaDataChangePublisher
{
    /// <summary>Publishes a data-change batch to advised callback subscribers.</summary>
    ValueTask PublishAsync(
        OpcDaDataChange change,
        CancellationToken cancellationToken = default);
}
