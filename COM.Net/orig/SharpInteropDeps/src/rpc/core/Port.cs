/// <summary>
/// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
/// 
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v1.
/// 
/// </summary>



namespace rpc.core {

    using NdrBuffer = ndr.NdrBuffer;
    using NdrObject = ndr.NdrObject;
    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    public class Port : NdrObject {

        public string PortSpec;

        public Port() : this(null) {
        }

        public Port(string portSpec) {
            this.PortSpec = portSpec;
        }

        public override void Read(NetworkDataRepresentation ndr) {
            int length = ndr.ReadUnsignedShort();
            if (length > 0) {
                NdrBuffer buf = ndr.Buffer;
                char[] portSpec = new char[length - 1];
                ndr.ReadCharacterArray(portSpec, 0, portSpec.Length);
                ndr.ReadUnsignedSmall(); // null terminator
                this.PortSpec = new string(portSpec);
            }
            else {
                this.PortSpec = null;
            }
        }

        public override void Write(NetworkDataRepresentation ndr) {
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

        public override bool Equals(object obj) {
            if (!(obj is Port)) {
                return false;
            }
            return (PortSpec != null) ? PortSpec.Equals(((Port) obj).PortSpec) : ((Port) obj).PortSpec == null;
        }

    }

}