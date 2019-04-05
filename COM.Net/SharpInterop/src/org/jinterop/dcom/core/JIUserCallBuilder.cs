// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core {
    using SharpCifs.Dcerpc.Ndr;

    /// <summary>
    /// Users can implement this class to provide for custom handling of there objects
    /// </summary>
    public abstract class JIUserCallBuilder : JICallBuilder {

        /// <summary>
        /// Write
        /// </summary>
        /// <param name="ndr"></param>
        public abstract void writeObject(NdrCodec ndr);

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="ndr"></param>
        public abstract void readObject(NdrCodec ndr);

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="dispatchNotSupported"></param>
        public JIUserCallBuilder(bool dispatchNotSupported) : 
            base(dispatchNotSupported) {
        }

        /// <summary>
        /// Create
        /// </summary>
        public JIUserCallBuilder() {
        }

        /// <inheritdoc/>
        public override void Write(NdrCodec ndr) {
            writeObject(ndr);
        }

        /// <inheritdoc/>
        public override void Read(NdrCodec ndr) {
            readObject(ndr);
        }
    }

}