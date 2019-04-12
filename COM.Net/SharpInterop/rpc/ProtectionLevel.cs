//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc {
    /// <summary>
    /// Protection level
    /// </summary>
    public enum ProtectionLevel {

        /// <summary>
        /// None
        /// </summary>
        PROTECTION_LEVEL_NONE = 1,

        /// <summary>
        /// Connect
        /// </summary>
        PROTECTION_LEVEL_CONNECT = 2,

        /// <summary>
        /// Call
        /// </summary>
        PROTECTION_LEVEL_CALL = 3,

        /// <summary>
        /// Packet
        /// </summary>
        PROTECTION_LEVEL_PACKET = 4,

        /// <summary>
        /// Integrity
        /// </summary>
        PROTECTION_LEVEL_INTEGRITY = 5,

        /// <summary>
        /// Privacy
        /// </summary>
        PROTECTION_LEVEL_PRIVACY = 6,
    }

}