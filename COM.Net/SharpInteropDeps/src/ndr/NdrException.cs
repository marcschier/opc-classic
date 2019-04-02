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
// Contributors:
// Vikram Roopchand  - Moving to EPL from LGPL v1.
// 

namespace ndr {

    /// <summary>
    /// Exception
    /// </summary>
    public class NdrException : System.IO.IOException {
        /// <summary>
        /// Message 
        /// </summary>
		public const string NO_NULL_REF = "ref pointer cannot be null";

        /// <summary>
        /// Message
        /// </summary>
		public const string INVALID_CONFORMANCE = "invalid array conformance";

        /// <summary>
        /// Create exception
        /// </summary>
        /// <param name="msg"></param>
		public NdrException(string msg) : base(msg) {
        }
    }
}