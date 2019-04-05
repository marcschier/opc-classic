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

namespace rpc {
    using SharpCifs.Dcerpc.Ndr;

    /// <summary>
    /// Defines a PDU
    /// </summary>
    public interface IProtocolDataUnit {

        /// <summary>
        /// Major version
        /// </summary>
        int MajorVersion { get; }

        /// <summary>
        /// Type
        /// </summary>
        int Type { get; }

        /// <summary>
        /// Format to use
        /// </summary>
        NdrFormat Format { get; set; }
    }
}