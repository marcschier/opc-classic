// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using SharpCifs.Util.Sharpen;
using System.IO;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Transport interface
/// </summary>
public interface ITransport
{

    /// <summary>
    /// Protocol name
    /// </summary>
    string Protocol { get; }

    /// <summary>
    /// Configuration
    /// </summary>
    PropertyBag Properties { get; }

    /// <summary>
    /// Attach
    /// </summary>
    /// <param name="syntax"></param>
    /// <exception cref="IOException"></exception>
    /// <returns></returns>
    IEndpoint Attach(PresentationSyntax syntax);

    /// <summary>
    /// Send
    /// </summary>
    /// <param name="buffer"></param>
    /// <exception cref="IOException"></exception>
    void Send(NdrBuffer buffer);

    /// <summary>
    /// Receive
    /// </summary>
    /// <param name="buffer"></param>
    /// <exception cref="IOException"></exception>
    void Receive(NdrBuffer buffer);

    /// <summary>
    /// Close
    /// </summary>
    /// <exception cref="IOException"></exception>
    void Close();
}
