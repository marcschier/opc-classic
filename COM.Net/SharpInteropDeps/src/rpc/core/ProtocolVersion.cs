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


namespace rpc.core
{

	using NdrBuffer = ndr.NdrBuffer;
	using NdrException = ndr.NdrException;
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	public class ProtocolVersion : NdrObject
	{

		internal int majorVersion, minorVersion;

		public virtual int getMajorVersion()
		{
			return majorVersion;
		}

		public virtual void setMajorVersion(short majorVersion)
		{
			this.majorVersion = majorVersion;
		}

		public virtual int getMinorVersion()
		{
			return minorVersion;
		}

		public virtual void setMinorVersion(short minorVersion)
		{
			this.minorVersion = minorVersion;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void encode(ndr.NetworkDataRepresentation ndr, ndr.NdrBuffer dst) throws ndr.NdrException
		public override void encode(NetworkDataRepresentation ndr, NdrBuffer dst)
		{
			dst.enc_ndr_small(majorVersion);
			dst.enc_ndr_small(minorVersion);
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void decode(ndr.NetworkDataRepresentation ndr, ndr.NdrBuffer src) throws ndr.NdrException
		public override void decode(NetworkDataRepresentation ndr, NdrBuffer src)
		{
			majorVersion = src.dec_ndr_small();
			minorVersion = src.dec_ndr_small();
		}
	}

}