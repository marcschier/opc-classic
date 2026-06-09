// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Rpc.Core;
using SharpCifs.Util.Sharpen;
using System.IO;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Connection context
/// </summary>
public interface IConnectionContext {

    /// <summary>
    /// Connectrion
    /// </summary>
    IConnection Connection { get; }

    /// <summary>
    /// Whether it is established
    /// </summary>
    bool Established { get; }

    /// <summary>
    /// Initialize
    /// </summary>
    /// <param name="context"></param>
    /// <param name="properties"></param>
    /// <exception cref="IOException"></exception>
    /// <returns></returns>
    ConnectionOrientedPdu Init(PresentationContext context,
        PropertyBag properties);

    /// <summary>
    /// Alter
    /// </summary>
    /// <param name="context"></param>
    /// <exception cref="IOException"></exception>
    /// <returns></returns>
    ConnectionOrientedPdu Alter(PresentationContext context);

    /// <summary>
    /// Accept
    /// </summary>
    /// <param name="pdu"></param>
    /// <exception cref="IOException"></exception>
    /// <returns></returns>
    ConnectionOrientedPdu Accept(ConnectionOrientedPdu pdu);
}
