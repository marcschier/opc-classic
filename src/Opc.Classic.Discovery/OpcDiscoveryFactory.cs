// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Opc.Classic.Discovery;

/// <summary>
/// Composite IOpcDiscovery that fans out across multiple discovery strategies
/// and de-duplicates the results by CLSID.
/// </summary>
public sealed class OpcDiscoveryFactory : IOpcDiscovery
{
    private readonly IReadOnlyList<IOpcDiscovery> _strategies;

    public OpcDiscoveryFactory(params IOpcDiscovery[] strategies)
    {
        ArgumentNullException.ThrowIfNull(strategies);
        if (strategies.Length == 0)
        {
            throw new ArgumentException("At least one discovery strategy is required.", nameof(strategies));
        }

        _strategies = strategies;
    }

    public async IAsyncEnumerable<OpcServerEntry> DiscoverAsync(
        string? host = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var seenClsids = new HashSet<Guid>();
        foreach (IOpcDiscovery strategy in _strategies)
        {
            IReadOnlyList<OpcServerEntry> entries = await ReadStrategyEntriesAsync(strategy, host, cancellationToken)
                .ConfigureAwait(false);
            foreach (OpcServerEntry entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (seenClsids.Add(entry.Clsid))
                {
                    yield return entry;
                }
            }
        }
    }

    private static async Task<IReadOnlyList<OpcServerEntry>> ReadStrategyEntriesAsync(
        IOpcDiscovery strategy,
        string? host,
        CancellationToken cancellationToken)
    {
        try
        {
            var entries = new List<OpcServerEntry>();
            await foreach (OpcServerEntry entry in strategy.DiscoverAsync(host, cancellationToken)
                .WithCancellation(cancellationToken)
                .ConfigureAwait(false))
            {
                entries.Add(entry);
            }

            return entries;
        }
        catch (OpcException)
        {
            return Array.Empty<OpcServerEntry>();
        }
        catch (IOException)
        {
            return Array.Empty<OpcServerEntry>();
        }
        catch (SocketException)
        {
            return Array.Empty<OpcServerEntry>();
        }
        catch (TimeoutException)
        {
            return Array.Empty<OpcServerEntry>();
        }
        catch (UnauthorizedAccessException)
        {
            return Array.Empty<OpcServerEntry>();
        }
    }
}
