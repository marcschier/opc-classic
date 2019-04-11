/// <summary>
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
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.core {

    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    //Users can implement this object to provide for custom handling of there objects
    /// <summary>
    /// Users can implement this class to provide for custom handling of there objects
    /// 
    /// @since 2.0 (formerly JIUserCallObject)
    /// </summary>
    public abstract class JIUserCallBuilder : JICallBuilder {

        public abstract void WriteObject(NetworkDataRepresentation ndr);
        public abstract void ReadObject(NetworkDataRepresentation ndr);

        public JIUserCallBuilder(bool dispatchNotSupported) : base(dispatchNotSupported) {
        }

        public JIUserCallBuilder() : base() {
        }

        public override void Write(NetworkDataRepresentation ndr) {
            WriteObject(ndr);
        }

        public override void Read(NetworkDataRepresentation ndr) {
            ReadObject(ndr);
        }

    }

}