// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using System.IO;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Endpoint
/// </summary>
public sealed class ComEndpoint : ConnectionOrientedEndpoint
{

    /// <summary>
    /// Create endpoint
    /// </summary>
    /// <param name="transport"></param>
    /// <param name="syntax"></param>
    internal ComEndpoint(ITransport transport, PresentationSyntax syntax) :
        base(transport, syntax)
    {
    }

    /// <summary>
    /// Rebind
    /// </summary>
    /// <exception cref="IOException"></exception>
    public void RebindEndPoint() => Rebind();
}
