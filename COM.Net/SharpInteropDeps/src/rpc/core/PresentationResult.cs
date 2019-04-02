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

	public class PresentationResult : NdrObject
	{

		public const int ACCEPTANCE = 0;

		public const int USER_REJECTION = 1;

		public const int PROVIDER_REJECTION = 2;

		public const int REASON_NOT_SPECIFIED = 0;

		public const int ABSTRACT_SYNTAX_NOT_SUPPORTED = 1;

		public const int PROPOSED_TRANSFER_SYNTAXES_NOT_SUPPORTED = 2;

		public const int LOCAL_LIMIT_EXCEEDED = 3;

		public int result;

		public int reason;

		public PresentationSyntax transferSyntax;

		public PresentationResult() : this(ACCEPTANCE, REASON_NOT_SPECIFIED, new PresentationSyntax(NetworkDataRepresentation.NDR_SYNTAX))
		{
		}

		public PresentationResult(PresentationSyntax transferSyntax) : this(ACCEPTANCE, REASON_NOT_SPECIFIED, transferSyntax)
		{
		}

		public PresentationResult(int result, int reason) : this(result, reason, null)
		{
		}

		public PresentationResult(int result, int reason, PresentationSyntax transferSyntax)
		{
			this.result = result;
			this.reason = reason;
			this.transferSyntax = transferSyntax;
		}

		public override void read(NetworkDataRepresentation ndr)
		{
			ndr.Buffer.align(4);
			result = ndr.readUnsignedShort();
			reason = ndr.readUnsignedShort();
			//if (result == ACCEPTANCE) //commenting this since the entire packet should be decoded VRC
			{
				transferSyntax = new PresentationSyntax();
				try
				{
					transferSyntax.decode(ndr, ndr.Buffer);
				}
				catch (NdrException)
				{
				}
			}
		}

		public override void write(NetworkDataRepresentation ndr)
		{
			ndr.Buffer.align(4, (sbyte) 0);
			ndr.writeUnsignedShort(result);
			ndr.writeUnsignedShort(reason);
			//if (result == ACCEPTANCE && transferSyntax != null)
			if (transferSyntax != null) //commenting this since the entire packet should be written VRC
			{
				try
				{
					transferSyntax.encode(ndr, ndr.Buffer);
				}
				catch (NdrException)
				{
				}
			}
		}

	}

}