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

namespace ndr {

    public abstract class NdrObject {

        internal const int Opnum_Renamed = -1;

        public object Value;

        public virtual int Opnum {
            get {
                return Opnum_Renamed;
            }
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void encode(NetworkDataRepresentation ndr, NdrBuffer dst) throws NdrException
        public virtual void Encode(NetworkDataRepresentation ndr, NdrBuffer dst) {
            ndr.Buf = dst;
            Write(ndr); // just for compatibility with jarapac < 0.2
        }
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void decode(NetworkDataRepresentation ndr, NdrBuffer src) throws NdrException
        public virtual void Decode(NetworkDataRepresentation ndr, NdrBuffer src) {
            ndr.Buf = src;
            Read(ndr);
        }
        public virtual void Write(NetworkDataRepresentation ndr) {
        }
        public virtual void Read(NetworkDataRepresentation ndr) {
        }
    }


}