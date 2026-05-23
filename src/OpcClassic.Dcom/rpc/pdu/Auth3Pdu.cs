//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc.pdu {
    using OpcClassic.Dcom.Internal.LegacyNdr;

    /// <summary>
    /// Auth pdu
    /// </summary>
    public class Auth3Pdu : ConnectionOrientedPdu {

        /// <summary> Type info - TODO - move to PduTypes.cs </summary>
        public const int AUTH3_TYPE = 0x10;

        /// <inheritdoc/>
        public override int Type => AUTH3_TYPE;

        /// <summary>
        /// Create pdu
        /// </summary>
        public Auth3Pdu() =>
            // Really useless value
            CallId = 0;

        /// <inheritdoc/>
        protected internal override void WriteBody(NdrCodec ndr) =>
            ndr.WriteUnsignedLong(0);
    }
}