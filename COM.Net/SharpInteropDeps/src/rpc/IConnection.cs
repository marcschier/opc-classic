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

namespace rpc {
    using System.IO;

    /// <summary>
    /// Connection interface
    /// </summary>
    public interface IConnection {

        /// <summary>
        /// Transmit
        /// </summary>
        /// <typeparam name="TPdu"></typeparam>
        /// <param name="pdu"></param>
        /// <param name="transport"></param>
        /// <exception cref="IOException"></exception>
        void Transmit<TPdu>(TPdu pdu, ITransport transport) 
            where TPdu : ConnectionOrientedPdu;

        /// <summary>
        /// Receive
        /// </summary>
        /// <typeparam name="TPdu"></typeparam>
        /// <param name="transport"></param>
        /// <returns></returns>
        /// <exception cref="IOException"></exception>
        TPdu Receive<TPdu>(ITransport transport)
            where TPdu : ConnectionOrientedPdu;
    }
}