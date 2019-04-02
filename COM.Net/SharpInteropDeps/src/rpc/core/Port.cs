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
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	public class Port : NdrObject
	{

		public string portSpec;

		public Port() : this(null)
		{
		}

		public Port(string portSpec)
		{
			this.portSpec = portSpec;
		}

		public override void read(NetworkDataRepresentation ndr)
		{
			var length = ndr.readUnsignedShort();
			if (length > 0)
			{
				var buf = ndr.Buffer;
				var portSpec = new char[length - 1];
				ndr.readCharacterArray(portSpec, 0, portSpec.Length);
				ndr.readUnsignedSmall(); // null terminator
				this.portSpec = new string(portSpec);
			}
			else
			{
				portSpec = null;
			}
		}

		public override void write(NetworkDataRepresentation ndr)
		{
			char[] spec;
			if (portSpec != null)
			{
				spec = new char[portSpec.Length + 1];
				portSpec.CopyTo(0, spec, 0, portSpec.Length - 0);
			}
			else
			{
				spec = new char[0];
			}
			ndr.writeUnsignedShort(spec.Length);
			if (spec.Length > 0)
			{
				ndr.writeCharacterArray(spec, 0, spec.Length);
			}
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Port))
			{
				return false;
			}
			return (portSpec != null) ? portSpec.Equals(((Port) obj).portSpec) : ((Port) obj).portSpec == null;
		}

	}

}