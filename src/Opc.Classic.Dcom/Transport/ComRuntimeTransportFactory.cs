// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Rpc;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Transport factory
/// </summary>
public sealed class ComRuntimeTransportFactory : TransportFactory
{
    /// <summary>
    /// Private constructor
    /// </summary>
    private ComRuntimeTransportFactory()
    {
    }

    /// <inheritdoc/>
    public override ITransport CreateTransport(string address,
        PropertyBag properties) =>
        new ComRuntimeTransport(address, properties);

    /// <summary>
    /// Singleton
    /// </summary>
    public static ComRuntimeTransportFactory Instance
    {
        get
        {
            if (_factory == null)
            {
                lock (s_factoryLock)
                {
                    if (_factory == null)
                    {
                        _factory = new ComRuntimeTransportFactory();
                    }
                }
            }
            return _factory;
        }
    }

    private static readonly Lock s_factoryLock = new();
    private static ComRuntimeTransportFactory _factory;
}
