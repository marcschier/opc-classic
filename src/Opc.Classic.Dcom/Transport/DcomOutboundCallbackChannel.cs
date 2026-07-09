// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Opens object-routed DCOM channels to client-hosted callback OBJREFs.
/// </summary>
public static class DcomOutboundCallbackChannel
{
    /// <summary>
    /// Resolves a callback OBJREF's bindings and opens an authenticated channel routed to its IPID.
    /// </summary>
    public static async Task<DcomCallChannel> ConnectAsync(
        IOpcInterfaceRef sinkRef,
        DcomCallChannelFactory channelFactory,
        Func<IAuthContext> authContextFactory,
        string fallbackHost,
        Guid callbackInterfaceId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sinkRef);
        ArgumentNullException.ThrowIfNull(channelFactory);
        ArgumentNullException.ThrowIfNull(authContextFactory);
        ArgumentException.ThrowIfNullOrWhiteSpace(fallbackHost);

        EndPoint endpoint = DualStringArrayResolver.ResolveFirstTransport(fallbackHost, sinkRef.ResolverBindings)
            ?? throw new InvalidOperationException("The callback OBJREF did not contain a supported binding.");
        ICallChannel channel = await channelFactory.ConnectActivatedAsync(
            endpoint,
            authContextFactory(),
            sinkRef.Ipid,
            [callbackInterfaceId],
            cancellationToken).ConfigureAwait(false);
        return channel as DcomCallChannel
            ?? throw new InvalidOperationException("The DCOM callback channel factory returned a non-DCOM channel.");
    }
}
