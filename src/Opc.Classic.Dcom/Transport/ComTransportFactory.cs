// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Common.Ntlm;
using System.IO;
using System.Threading;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Factory for <seealso cref="ComTransport"/>
/// </summary>
public sealed class ComTransportFactory : TransportFactory
{

    /// <summary>
    /// private constructor
    /// </summary>
    private ComTransportFactory()
    {
    }

    /// <inheritdoc/>
    public override ITransport CreateTransport(string address, PropertyBag properties) =>
        new ComTransport(address, properties);

    /// <summary>
    /// Singleton
    /// </summary>
    public static ComTransportFactory Instance
    {
        get
        {
            lock (s_factoryLock)
            {
                if (_instance == null)
                {
                    try
                    {
                        _instance = new ComTransportFactory();
                    }
                    catch (IOException e)
                    {
                        throw new InteropException(-1, e);
                    }
                }
                return _instance;
            }
        }
    }

    private static readonly Lock s_factoryLock = new();
    private static ComTransportFactory _instance;
}
