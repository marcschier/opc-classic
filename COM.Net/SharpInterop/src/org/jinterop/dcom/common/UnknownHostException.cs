// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 
using System;
using System.Runtime.Serialization;

namespace org.jinterop.dcom.common {
    [Serializable]
    internal class UnknownHostException : Exception {
        public UnknownHostException() {
        }

        public UnknownHostException(string message) : base(message) {
        }

        public UnknownHostException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnknownHostException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}