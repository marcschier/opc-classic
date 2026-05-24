//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc; 
/// <summary>
/// Constants
/// </summary>
public static class Connection {

    /// <summary>
    /// Key to read max fragments
    /// </summary>
    public const string MAX_TRANSMIT_FRAGMENT = "rpc.connectionContext.maxTransmitFragment";

    /// <summary>
    /// Key to read max fragments
    /// </summary>
    public const string MAX_RECEIVE_FRAGMENT = "rpc.connectionContext.maxReceiveFragment";

    /// <summary>
    /// Default
    /// </summary>
    public const int DEFAULT_MAX_TRANSMIT_FRAGMENT = 4280;

    /// <summary>
    /// Default
    /// </summary>
    public const int DEFAULT_MAX_RECEIVE_FRAGMENT = 4280;
}
