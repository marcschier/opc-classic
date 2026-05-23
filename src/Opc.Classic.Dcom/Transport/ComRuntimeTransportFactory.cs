//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Transport {
    using Opc.Classic.Dcom.Internal;
    using SharpInterop.Rpc;

    /// <summary>
    /// Transport factory
    /// </summary>
    public sealed class ComRuntimeTransportFactory : TransportFactory {

        /// <summary>
        /// Private constructor
        /// </summary>
        private ComRuntimeTransportFactory() {
        }

        /// <inheritdoc/>
        public override ITransport CreateTransport(string address,
            PropertyBag properties) =>
            new ComRuntimeTransport(address, properties);

        /// <summary>
        /// Singleton
        /// </summary>
        public static ComRuntimeTransportFactory Instance {
            get {
                if (_factory == null) {
                    lock (typeof(ComTransportFactory)) {
                        if (_factory == null) {
                            _factory = new ComRuntimeTransportFactory();
                        }
                    }
                }
                return _factory;
            }
        }

        private static ComRuntimeTransportFactory _factory;
    }
}