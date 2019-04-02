// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core {
    using ndr;

    /// <summary>
    /// Users can implement this class to provide for custom handling of there objects
    /// </summary>
    public abstract class JIUserCallBuilder : JICallBuilder {

        /// <summary>
        /// Write
        /// </summary>
        /// <param name="ndr"></param>
        public abstract void writeObject(NetworkDataRepresentation ndr);

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="ndr"></param>
        public abstract void readObject(NetworkDataRepresentation ndr);

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
        public override void write(NetworkDataRepresentation ndr) {
            writeObject(ndr);
        }

        /// <inheritdoc/>
        public override void read(NetworkDataRepresentation ndr) {
            readObject(ndr);
        }
    }

}