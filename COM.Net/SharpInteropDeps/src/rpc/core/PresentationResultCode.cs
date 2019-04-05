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



namespace rpc.core {
    /// <summary>
    /// Presentation result code
    /// </summary>
    public enum PresentationResultCode {

        /// <summary>
        /// Accept
        /// </summary>
        ACCEPTANCE = 0,

        /// <summary>
        /// User rejected
        /// </summary>
        USER_REJECTION = 1,

        /// <summary>
        /// Rejected
        /// </summary>
        PROVIDER_REJECTION = 2,
    }
}