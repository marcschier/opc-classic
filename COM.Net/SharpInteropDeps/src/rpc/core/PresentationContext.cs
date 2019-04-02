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

	using NdrException = ndr.NdrException;
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	public class PresentationContext : NdrObject
	{

		public int contextId;

		public PresentationSyntax abstractSyntax;

		public PresentationSyntax[] transferSyntaxes;

		public PresentationContext() : this(0, new PresentationSyntax(), new PresentationSyntax[] {new PresentationSyntax(NetworkDataRepresentation.NDR_SYNTAX)})
		{
		}

		public PresentationContext(int contextId, PresentationSyntax abstractSyntax) : this(contextId, abstractSyntax, new PresentationSyntax[] {new PresentationSyntax(NetworkDataRepresentation.NDR_SYNTAX)})
		{
		}

		public PresentationContext(int contextId, PresentationSyntax abstractSyntax, PresentationSyntax[] transferSyntaxes)
		{
			this.contextId = contextId;
			this.abstractSyntax = abstractSyntax;
			this.transferSyntaxes = transferSyntaxes;
		}

		public override void read(NetworkDataRepresentation ndr)
		{
			ndr.Buffer.align(4);
			contextId = ndr.readUnsignedShort();
			var count = ndr.readUnsignedSmall();

			try
			{
				abstractSyntax.decode(ndr, ndr.Buffer);
				   transferSyntaxes = new PresentationSyntax[count];
				for (var i = 0; i < count; i++)
				{
					transferSyntaxes[i] = new PresentationSyntax();
					transferSyntaxes[i].decode(ndr, ndr.Buffer);
				}
			}
			catch (NdrException)
			{
			}
		}

		public override void write(NetworkDataRepresentation ndr)
		{
			ndr.Buffer.align(4, unchecked((sbyte)0xcc));
			ndr.writeUnsignedShort(contextId);
			ndr.writeUnsignedShort((short) transferSyntaxes.Length);

			try
			{
				abstractSyntax.encode(ndr, ndr.Buffer);
				for (var i = 0; i < transferSyntaxes.Length; i++)
				{
					transferSyntaxes[i].encode(ndr, ndr.Buffer);
				}
			}
			catch (NdrException)
			{
			}
		}

	}

}