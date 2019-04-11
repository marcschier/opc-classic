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
    using NdrException = ndr.NdrException;
    using NdrObject = ndr.NdrObject;
    using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

    public class ProtocolVersion : NdrObject {

        internal int MajorVersion_Renamed, MinorVersion_Renamed;

        public virtual int GetMajorVersion() {
            return MajorVersion_Renamed;
        }

        public virtual void SetMajorVersion(short majorVersion) {
            this.MajorVersion_Renamed = majorVersion;
        }

        public virtual int GetMinorVersion() {
            return MinorVersion_Renamed;
        }

        public virtual void SetMinorVersion(short minorVersion) {
            this.MinorVersion_Renamed = minorVersion;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void encode(ndr.NetworkDataRepresentation ndr, ndr.NdrBuffer dst) throws ndr.NdrException
        public override void Encode(NetworkDataRepresentation ndr, NdrBuffer dst) {
            dst.Enc_ndr_small(MajorVersion_Renamed);
            dst.Enc_ndr_small(MinorVersion_Renamed);
        }
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void decode(ndr.NetworkDataRepresentation ndr, ndr.NdrBuffer src) throws ndr.NdrException
        public override void Decode(NetworkDataRepresentation ndr, NdrBuffer src) {
            MajorVersion_Renamed = src.Dec_ndr_small();
            MinorVersion_Renamed = src.Dec_ndr_small();
        }
    }

}