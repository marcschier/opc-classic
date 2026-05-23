//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc.Core {
    /// <summary>
    /// Presentation result reason
    /// </summary>
    public enum PresentationResultReason {

        /// <summary>
        /// No reason
        /// </summary>
        REASON_NOT_SPECIFIED = 0,

        /// <summary>
        /// Not supported
        /// </summary>
        ABSTRACT_SYNTAX_NOT_SUPPORTED = 1,

        /// <summary>
        /// Transfer syntax not supported
        /// </summary>
        PROPOSED_TRANSFER_SYNTAXES_NOT_SUPPORTED = 2,

        /// <summary>
        /// Local limit exceeded
        /// </summary>
        LOCAL_LIMIT_EXCEEDED = 3,
    }
}