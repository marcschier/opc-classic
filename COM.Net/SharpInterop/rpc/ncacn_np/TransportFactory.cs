//
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
//
// j-Interop (Pure Java implementation of DCOM protocol)
//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace rpc.ncacn_np {
    using SharpCifs.Util.Sharpen;

    /// <summary>
    /// Transport factory
    /// </summary>
    public class TransportFactory : rpc.TransportFactory {

        /// <summary>
        /// Create transport
        /// </summary>
        /// <param name="address"></param>
        /// <param name="properties"></param>
        /// <exception cref="ProviderException"></exception>
        /// <returns></returns>
        public override ITransport CreateTransport(string address,
            Properties properties) => new RpcTransport(address, properties);
    }
}