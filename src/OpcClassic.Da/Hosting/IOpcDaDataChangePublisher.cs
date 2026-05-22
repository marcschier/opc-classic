//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System.Threading;
using System.Threading.Tasks;

namespace OpcClassic.Da.Hosting;

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
