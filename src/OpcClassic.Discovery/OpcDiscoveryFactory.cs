//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic.Discovery;

/// <summary>
/// Composite IOpcDiscovery that fans out across multiple discovery strategies
/// and de-duplicates the results by CLSID.
/// </summary>
public sealed class OpcDiscoveryFactory : IOpcDiscovery
{
    private readonly System.Collections.Generic.IReadOnlyList<IOpcDiscovery> _strategies;

    public OpcDiscoveryFactory(params IOpcDiscovery[] strategies)
    {
        System.ArgumentNullException.ThrowIfNull(strategies);
        if (strategies.Length == 0)
        {
            throw new System.ArgumentException("At least one discovery strategy is required.", nameof(strategies));
        }

        _strategies = strategies;
    }

    public async System.Collections.Generic.IAsyncEnumerable<OpcServerEntry> DiscoverAsync(
        string? host = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] System.Threading.CancellationToken cancellationToken = default)
    {
        var seenClsids = new System.Collections.Generic.HashSet<System.Guid>();
        foreach (var strategy in _strategies)
        {
            System.Collections.Generic.IAsyncEnumerable<OpcServerEntry> stream;
            try
            {
                stream = strategy.DiscoverAsync(host, cancellationToken);
            }
            catch (System.NotImplementedException)
            {
                // Strategy is a scaffold (Phase 10A-followup or 10B-followup); skip silently.
                continue;
            }

            await foreach (var entry in stream.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                if (seenClsids.Add(entry.Clsid))
                {
                    yield return entry;
                }
            }
        }
    }
}
