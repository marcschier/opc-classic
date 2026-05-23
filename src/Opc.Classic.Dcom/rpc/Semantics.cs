//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc {
    using System;

    /// <summary>
    /// Endpoint type
    /// </summary>
    [Flags]
    public enum Semantics {

        /// <summary>
        /// Maybe
        /// </summary>
        MAYBE = 0x01,

        /// <summary>
        /// Idempotent
        /// </summary>
        IDEMPOTENT = 0x02,

        /// <summary>
        /// Broadcast
        /// </summary>
        BROADCAST = 0x04,
    }
}