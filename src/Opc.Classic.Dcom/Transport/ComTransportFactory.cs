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
    using SharpInterop.Common;
    using SharpInterop.Rpc;
    using SharpCifs.Util.Sharpen;
    using System.IO;

    /// <summary>
    /// Factory for <seealso cref="ComTransport"/>
    /// </summary>
    public sealed class ComTransportFactory : TransportFactory {

        /// <summary>
        /// private constructor
        /// </summary>
        private ComTransportFactory() {
        }

        /// <inheritdoc/>
        public override ITransport CreateTransport(string address, PropertyBag properties) =>
            new ComTransport(address, properties);

        /// <summary>
        /// Singleton
        /// </summary>
        public static ComTransportFactory Instance {
            get {
                lock (typeof(ComTransportFactory)) {
                    if (_instance == null) {
                        try {
                            _instance = new ComTransportFactory();
                        }
                        catch (IOException e) {
                            throw new InteropException(-1, e);
                        }
                    }
                    return _instance;
                }
            }
        }

        private static ComTransportFactory _instance;
    }
}