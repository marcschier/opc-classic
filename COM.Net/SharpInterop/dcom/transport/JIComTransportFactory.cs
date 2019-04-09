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
    using System.IO;
    using org.jinterop.dcom.common;

    /// <summary>
    /// Factory for <seealso cref="JIComTransport"/>
    /// </summary>
    public sealed class JIComTransportFactory : rpc.TransportFactory {

        /// <summary>
        /// Constructor for JIComTransportFactory.
        /// </summary>
        /// <exception cref="IOException"></exception>
        private JIComTransportFactory() {
            _selectorManager = new SelectorManager();
        }

        /// <inheritdoc/>
        public override ITransport CreateTransport(string address, Properties properties) => 
            new JIComTransport(address, _selectorManager, properties);

        /// <summary>
        /// Singleton
        /// </summary>
        public static JIComTransportFactory Singleton {
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
        private readonly SelectorManager _selectorManager;
    }
}