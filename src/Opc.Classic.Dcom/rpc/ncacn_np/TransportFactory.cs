// SPDX-License-Identifier: MIT

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
    /// <param name="address"></param>
    /// <param name="properties"></param>
    /// <exception cref="ProviderException"></exception>
    /// <returns></returns>
    public override ITransport CreateTransport(string address,
        PropertyBag properties) => new RpcTransport(address, properties);
}
