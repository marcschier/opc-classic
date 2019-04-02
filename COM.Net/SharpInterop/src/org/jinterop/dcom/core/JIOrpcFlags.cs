// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.core {

    internal static class JIOrpcFlags {

        internal const long ORPCF_NULL = 0; // no additional info in packet
        internal const long ORPCF_LOCAL = 1; // call is local to this machine
        internal const long ORPCF_RESERVED1 = 2; // reserved for local use
        internal const long ORPCF_RESERVED2 = 4; // reserved for local use
        internal const long ORPCF_RESERVED3 = 8; // reserved for local use
        internal const long ORPCF_RESERVED4 = 16; // reserved for local use
    }
}