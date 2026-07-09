// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Discovery;

/// <summary>
/// Convenience entry points for OPC Classic server discovery.
/// </summary>
public static class OpcDiscovery
{
    /// <summary>
    /// Enumerates OPC servers on <paramref name="host" /> through OPCEnum / OPC.ServerList.1.
    /// </summary>
    public static Task<OpcServerDescriptor[]> EnumerateAsync(
        string host,
        IEnumerable<Guid>? categories = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);

        var client = new OpcEnumClient(host, new DcomOpcEnumCallChannelFactory(), categories);
        return client.EnumerateAsync(host, categories, cancellationToken);
    }

    /// <summary>
    /// Enumerates OPC servers on <paramref name="host" /> through OPCEnum using the supplied DCOM authentication settings.
    /// </summary>
    public static Task<OpcServerDescriptor[]> EnumerateAsync(
        string host,
        OpcConnectData connectData,
        IEnumerable<Guid>? categories = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(connectData);

        var client = new OpcEnumClient(host, new DcomOpcEnumCallChannelFactory(connectData), categories);
        return client.EnumerateAsync(host, categories, cancellationToken);
    }
}
