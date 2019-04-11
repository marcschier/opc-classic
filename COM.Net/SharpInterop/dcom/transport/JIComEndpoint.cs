//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.transport {
    using rpc;
    using rpc.core;
    using System.IO;

    /// <summary>
    /// Endpoint
    /// </summary>
    public sealed class JIComEndpoint : ConnectionOrientedEndpoint {

        /// <summary>
        /// Create endpoint
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="syntax"></param>
        internal JIComEndpoint(ITransport transport, PresentationSyntax syntax) :
            base(transport, syntax) {
        }

        /// <summary>
        /// Rebind
        /// </summary>
        /// <exception cref="IOException"></exception>
        public void RebindEndPoint() => Rebind();
    }
}