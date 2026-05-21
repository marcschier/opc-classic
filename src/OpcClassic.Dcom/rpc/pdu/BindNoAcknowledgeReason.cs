//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc.pdu {
    /// <summary>
    /// Reason code
    /// </summary>
    public enum BindNoAcknowledgeReason {

        /// <summary>
        /// reason not specified
        /// </summary>
        REASON_NOT_SPECIFIED = 0,

        /// <summary>
        /// Temporary congested
        /// </summary>
        TEMPORARY_CONGESTION = 1,

        /// <summary>
        /// Limits exceeded
        /// </summary>
        LOCAL_LIMIT_EXCEEDED = 2,

        /// <summary>
        /// Called paddr unknown
        /// </summary>
        CALLED_PADDR_UNKNOWN = 3, // not used

        /// <summary>
        /// Version not supported
        /// </summary>
        PROTOCOL_VERSION_NOT_SUPPORTED = 4,

        /// <summary>
        /// Context not supported
        /// </summary>
        DEFAULT_CONTEXT_NOT_SUPPORTED = 5, // not used

        /// <summary>
        /// User data not readable
        /// </summary>
        USER_DATA_NOT_READABLE = 6, // not used

        /// <summary>
        /// No psap
        /// </summary>
        NO_PSAP_AVAILABLE = 7, // not used
    }
}