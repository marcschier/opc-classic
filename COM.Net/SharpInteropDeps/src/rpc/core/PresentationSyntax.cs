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

	public class PresentationSyntax : NdrObject
	{

		private const int UUID_INDEX = 0;

		private const int VERSION_INDEX = 1;

		internal UUID uuid;
		internal int version;

		public PresentationSyntax()
		{
		}

		public PresentationSyntax(string syntax) : this()
		{
			parse(syntax);
		}

		public PresentationSyntax(UUID uuid, int majorVersion, int minorVersion) : this()
		{
			Uuid = uuid;
			setVersion(majorVersion, minorVersion);
		}

		public virtual UUID Uuid {
            get => uuid;
            set => uuid = value;
        }


        public virtual int Version {
            get => version;
            set => version = value;
        }


        public virtual int MajorVersion => version & 0xffff;

        public virtual int MinorVersion => (version >> 16) & 0xffff;

        public virtual void setVersion(int majorVersion, int minorVersion)
		{
			Version = (majorVersion & 0xffff) | (minorVersion << 16);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void encode(ndr.NetworkDataRepresentation ndr, ndr.NdrBuffer dst) throws ndr.NdrException
		public override void encode(NetworkDataRepresentation ndr, NdrBuffer dst)
		{
			uuid.encode(ndr, dst);
			dst.enc_ndr_long(version);
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void decode(ndr.NetworkDataRepresentation ndr, ndr.NdrBuffer src) throws ndr.NdrException
		public override void decode(NetworkDataRepresentation ndr, NdrBuffer src)
		{
			uuid = new UUID();
			uuid.decode(ndr, src);
			version = src.dec_ndr_long();
		}

		public override string ToString()
		{
			return Uuid.ToString() + ":" + MajorVersion + "." + MinorVersion;
		}

		public virtual void parse(string syntax)
		{
			var tokenizer = new StringTokenizer(syntax, ":.");
			uuid = new UUID();
			uuid.parse(tokenizer.nextToken());
			setVersion(int.Parse(tokenizer.nextToken()), int.Parse(tokenizer.nextToken()));
		}

	}

}