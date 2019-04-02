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
    using System;

    /// <summary>
    /// Ndr object
    /// </summary>
    public abstract class NdrObject {

        /// <summary>
        /// Value
        /// </summary>
        public object value { get; set; }

        /// <summary>
        /// Op num of object
        /// </summary>
        public virtual int Opnum { get => -1; set => throw new NotSupportedException(); }

        /// <summary>
        /// Decode
        /// </summary>
        /// <exception cref="NdrException"></exception>
        /// <param name="ndr"></param>
        /// <param name="dst"></param>
        public virtual void encode(NetworkDataRepresentation ndr, NdrBuffer dst) {
            ndr.buf = dst;
            write(ndr); // just for compatibility with jarapac < 0.2
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <exception cref="NdrException"></exception>
        /// <param name="ndr"></param>
        /// <param name="src"></param>
        public virtual void decode(NetworkDataRepresentation ndr, NdrBuffer src) {
            ndr.buf = src;
            read(ndr);
        }

        /// <summary>
        /// Write
        /// </summary>
        /// <param name="ndr"></param>
        public virtual void write(NetworkDataRepresentation ndr) {}

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="ndr"></param>
        public virtual void read(NetworkDataRepresentation ndr) {}
    }
}