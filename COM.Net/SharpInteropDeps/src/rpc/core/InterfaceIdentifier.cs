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

	using NdrObject = ndr.NdrObject;

	public class InterfaceIdentifier : NdrObject
	{

		internal UUID uuid;
		internal int majorVersion, minorVersion;

		public InterfaceIdentifier(string syntax)
		{
			parse(syntax);
		}

		public InterfaceIdentifier(UUID uuid, int majorVersion, int minorVersion)
		{
			Uuid = uuid;
			MajorVersion = majorVersion;
			MinorVersion = minorVersion;
		}

		public virtual UUID Uuid {
            get => uuid;
            set => uuid = value;
        }


        public virtual int MajorVersion {
            get => majorVersion;
            set => majorVersion = value;
        }


        public virtual int MinorVersion {
            get => minorVersion;
            set => minorVersion = value;
        }


        public override string ToString()
		{
			return Uuid.ToString() + ":" + MajorVersion + "." + MinorVersion;
		}

		public virtual void parse(string syntax)
		{
			var tokenizer = new StringTokenizer(syntax, ":.");
			Uuid.parse(tokenizer.nextToken());
			MajorVersion = int.Parse(tokenizer.nextToken());
			MinorVersion = int.Parse(tokenizer.nextToken());
		}

	}

}