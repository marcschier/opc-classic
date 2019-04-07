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

	using NdrException = ndr.NdrException;
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	public class PresentationContext : NdrObject {

		public int ContextId;

		public PresentationSyntax AbstractSyntax;

		public PresentationSyntax[] TransferSyntaxes;

		public PresentationContext() : this(0, new PresentationSyntax(), new PresentationSyntax[] { new PresentationSyntax(NetworkDataRepresentation.NDR_SYNTAX) }) {
		}

		public PresentationContext(int contextId, PresentationSyntax abstractSyntax) : this(contextId, abstractSyntax, new PresentationSyntax[] { new PresentationSyntax(NetworkDataRepresentation.NDR_SYNTAX) }) {
		}

		public PresentationContext(int contextId, PresentationSyntax abstractSyntax, PresentationSyntax[] transferSyntaxes) {
			this.ContextId = contextId;
			this.AbstractSyntax = abstractSyntax;
			this.TransferSyntaxes = transferSyntaxes;
		}

		public override void Read(NetworkDataRepresentation ndr) {
			ndr.Buffer.Align(4);
			ContextId = ndr.ReadUnsignedShort();
			int count = ndr.ReadUnsignedSmall();

			try {
				AbstractSyntax.Decode(ndr, ndr.Buffer);
				   TransferSyntaxes = new PresentationSyntax[count];
				for (int i = 0; i < count; i++) {
					TransferSyntaxes[i] = new PresentationSyntax();
					TransferSyntaxes[i].Decode(ndr, ndr.Buffer);
				}
			}
			catch (NdrException) {
			}
		}

		public override void Write(NetworkDataRepresentation ndr) {
			ndr.Buffer.Align(4, unchecked((sbyte)0xcc));
			ndr.WriteUnsignedShort(ContextId);
			ndr.WriteUnsignedShort((short) TransferSyntaxes.Length);

			try {
				AbstractSyntax.Encode(ndr, ndr.Buffer);
				for (int i = 0; i < TransferSyntaxes.Length; i++) {
					TransferSyntaxes[i].Encode(ndr, ndr.Buffer);
				}
			}
			catch (NdrException) {
			}
		}

	}

}