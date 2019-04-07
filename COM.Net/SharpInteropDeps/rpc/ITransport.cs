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
    using rpc.core;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;
    using System.IO;

    /// <summary>
    /// Transport interface
    /// </summary>
    public interface ITransport {

        /// <summary>
        /// Protocol name
        /// </summary>
        string Protocol { get; }

        /// <summary>
        /// Configuration
        /// </summary>
        Properties Properties { get; }

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
}