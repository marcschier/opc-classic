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
    using SharpCifs.Util.Sharpen;

    /// <summary>
    /// Transport factory
    /// </summary>
    public sealed class JIComRuntimeTransportFactory : TransportFactory {

        /// <summary>
        /// Private constructor
        /// </summary>
        private JIComRuntimeTransportFactory() {
        }

        /// <inheritdoc/>
        public override ITransport CreateTransport(string address,
            Properties properties) =>
            new JIComRuntimeTransport(address, properties);

        /// <summary>
        /// Singleton
        /// </summary>
        public static JIComRuntimeTransportFactory Instance {
            get {
                if (_factory == null) {
                    lock (typeof(JIComTransportFactory)) {
                        if (_factory == null) {
                            _factory = new JIComRuntimeTransportFactory();
                        }
                    }
                }
                return _factory;
            }
        }

        private static JIComRuntimeTransportFactory _factory;
    }
}