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

namespace SharpCifs.Dcerpc.Ndr {
    using System;

    // TODO: Use SharpCifs.Dcerpc.DcerpcMessage instead

    /// <summary>
    /// Ndr object
    /// </summary>
    public abstract class NdrOp {

        /// <summary>
        /// Value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Op num of object
        /// </summary>
        public virtual int Opnum { get; set; } = -1;

        /// <summary>
        /// Decode
        /// </summary>
        /// <exception cref="NdrException"></exception>
        /// <param name="ndr"></param>
        /// <param name="dst"></param>
        public virtual void Encode(NdrCodec ndr, NdrBuffer dst) {
            ndr.Buffer = dst;
            Write(ndr); // just for compatibility with jarapac < 0.2
        }

        /// <summary>
        /// Encode
        /// </summary>
        /// <exception cref="NdrException"></exception>
        /// <param name="ndr"></param>
        /// <param name="src"></param>
        public virtual void Decode(NdrCodec ndr, NdrBuffer src) {
            ndr.Buffer = src;
            Read(ndr);
        }

        /// <summary>
        /// Write
        /// </summary>
        /// <param name="ndr"></param>
        public virtual void Write(NdrCodec ndr) {}

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="ndr"></param>
        public virtual void Read(NdrCodec ndr) {}
    }
}