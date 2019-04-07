// 
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
// 
// j-Interop (Pure Java implementation of DCOM protocol)
// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace rpc.pdu {

    /// <summary>
    /// Shutdown
    /// </summary>
    public class ShutdownPdu : ConnectionOrientedPdu {

        /// <summary> Type info - TODO - move to PduTypes.cs </summary>
		public const int SHUTDOWN_TYPE = 0x11;

        /// <inheritdoc/>
        public override int Type => SHUTDOWN_TYPE;
    }
}