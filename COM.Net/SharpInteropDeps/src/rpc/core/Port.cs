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
    using SharpCifs.Dcerpc.Ndr;
    using System.Collections.Generic;

    /// <summary>
    /// Port
    /// </summary>
    public class Port : NdrOp {

        /// <summary>
        /// Port specification
        /// </summary>
        public string PortSpec { get; set; }

        /// <summary>
        /// Create
        /// </summary>
        public Port() : 
            this(null) {
        }

        /// <summary>
        /// Create port
        /// </summary>
        /// <param name="portSpec"></param>
        public Port(string portSpec) {
            PortSpec = portSpec;
        }

        /// <override/>
        public override void Read(NdrCodec ndr) {
            var length = ndr.ReadUnsignedShort();
            if (length > 0) {
                var buf = ndr.Buffer;
                var portSpec = new char[length - 1];
                ndr.ReadCharacterArray(portSpec, 0, portSpec.Length);
                ndr.ReadUnsignedSmall(); // null terminator
                PortSpec = new string(portSpec);
            }
            else {
                PortSpec = null;
            }
        }

        /// <override/>
        public override void Write(NdrCodec ndr) {
            char[] spec;
            if (PortSpec != null) {
                spec = new char[PortSpec.Length + 1];
                PortSpec.CopyTo(0, spec, 0, PortSpec.Length - 0);
            }
            else {
                spec = new char[0];
            }
            ndr.WriteUnsignedShort(spec.Length);
            if (spec.Length > 0) {
                ndr.WriteCharacterArray(spec, 0, spec.Length);
            }
        }

        /// <override/>
        public override bool Equals(object obj) {
            if (!(obj is Port other)) {
                return false;
            }
            return (PortSpec != null) ?
                PortSpec.Equals(other.PortSpec) : other.PortSpec == null;
        }

        /// <override/>
        public override int GetHashCode() {
            return EqualityComparer<string>.Default.GetHashCode(PortSpec);
        }
    }
}