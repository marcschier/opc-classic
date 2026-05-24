//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using Opc.Classic.Dcom.Internal;
using SharpInterop.Rpc.Core;
using SharpCifs.Util.Sharpen;
using System.IO;

namespace SharpInterop.Rpc; 
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
