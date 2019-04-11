//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.core {
    using org.jinterop.dcom.common;
    using System;

    /// <summary>
    /// Scode type
    /// </summary>
    [Serializable]
    public sealed class Scode {

        /// <summary>
        /// Null value
        /// </summary>
        public static Scode Ok { get; } = new Scode(0);

        /// <summary>
        /// Error code
        /// </summary>
        public int ErrorCode { get; }

        /// <summary>
        /// Create error code
        /// </summary>
        /// <param name="errorCode"></param>
        public Scode(int errorCode) => ErrorCode = errorCode;

        /// <summary>
        /// Create error code
        /// </summary>
        /// <param name="errorCode"></param>
        public Scode(JIErrorCodes errorCode) :
            this((int)errorCode) {
        }
    }
}