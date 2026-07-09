// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Internal;

namespace Opc.Classic.Dcom.Rpc.Ncacn_Np;

/// <summary>
/// Transport factory
/// </summary>
public class TransportFactory : Opc.Classic.Dcom.Rpc.TransportFactory
{
    /// <summary>
    /// Create transport
    /// </summary>
    /// <param name="address">Network address or binding address for the remote endpoint.</param>
    /// <param name="properties">Property values used to initialize the COM descriptor.</param>
    /// <exception cref="ProviderException">Thrown when the provider cannot complete the requested RPC transport operation.</exception>
    /// <returns>A new <see cref="ITransport"/> instance built from <paramref name="address"/>.</returns>
    public override ITransport CreateTransport(string address,
        PropertyBag properties) => new RpcTransport(address, properties);
}
