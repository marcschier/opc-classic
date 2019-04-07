//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {

    internal enum JIOrpcFlags {

        /// <summary>
        /// no additional info in packet
        /// </summary>
        ORPCF_NULL = 0,

        /// <summary>
        /// call is local to this machine
        /// </summary>
        ORPCF_LOCAL = 1,

        /// <summary>
        /// reserved for local use
        /// </summary>
        ORPCF_RESERVED1 = 2,

        /// <summary>
        /// reserved for local use
        /// </summary>
        ORPCF_RESERVED2 = 4,

        /// <summary>
        /// reserved for local use
        /// </summary>
        ORPCF_RESERVED3 = 8,

        /// <summary>
        /// reserved for local use
        /// </summary>
        ORPCF_RESERVED4 = 16;
    }
}