// SPDX-License-Identifier: MIT

using System.IO;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Connection interface
/// </summary>
public interface IConnection
{

    /// <summary>
    /// Transmit
    /// </summary>
    /// <param name="pdu"></param>
    /// <param name="transport"></param>
    /// <exception cref="IOException"></exception>
    void Transmit(ConnectionOrientedPdu pdu, ITransport transport);

    /// <summary>
    /// Receive
    /// </summary>
    /// <param name="transport"></param>
    /// <returns></returns>
    /// <exception cref="IOException"></exception>
    ConnectionOrientedPdu Receive(ITransport transport);
}
