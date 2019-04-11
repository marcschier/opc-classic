//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.transport {
    using org.jinterop.dcom.common;
    using rpc;
    using SharpCifs.Util.Sharpen;
    using System.IO;

    /// <summary>
    /// Factory for <seealso cref="JIComTransport"/>
    /// </summary>
    public sealed class JIComTransportFactory : TransportFactory {

        /// <summary>
        /// private constructor
        /// </summary>
        private JIComTransportFactory() {
        }

        /// <inheritdoc/>
        public override ITransport CreateTransport(string address, Properties properties) =>
            new JIComTransport(address, properties);

        /// <summary>
        /// Singleton
        /// </summary>
        public static JIComTransportFactory Instance {
            get {
                lock (typeof(JIComTransportFactory)) {
                    if (_instance == null) {
                        try {
                            _instance = new JIComTransportFactory();
                        }
                        catch (IOException e) {
                            throw new JIException(-1, e);
                        }
                    }
                    return _instance;
                }
            }
        }

        private static JIComTransportFactory _instance;
    }
}