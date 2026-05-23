//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Core {
    using OpcClassic.Dcom.Internal.LegacyNdr;

    /// <summary>
    /// Users can implement this class to provide for custom handling of there objects
    /// </summary>
    public abstract class CustomCallBuilder : CallBuilder {

        /// <summary>
        /// Write
        /// </summary>
        /// <param name="ndr"></param>
        public abstract void WriteObject(NdrCodec ndr);

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="ndr"></param>
        public abstract void ReadObject(NdrCodec ndr);

        /// <summary>
        /// Create
        /// </summary>
        /// <param name="dispatchNotSupported"></param>
        protected CustomCallBuilder(bool dispatchNotSupported) :
            base(dispatchNotSupported) {
        }

        /// <summary>
        /// Create
        /// </summary>
        protected CustomCallBuilder() {
        }

        /// <inheritdoc/>
        public override void Write(NdrCodec ndr) => WriteObject(ndr);

        /// <inheritdoc/>
        public override void Read(NdrCodec ndr) => ReadObject(ndr);
    }
}