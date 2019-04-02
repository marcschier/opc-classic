using System;

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

	public class AuthenticationVerifier : NdrObject
	{

		public int authenticationService;

		public int protectionLevel;

		public int contextId;

		public sbyte[] body;

		public AuthenticationVerifier() : this(Security_Fields.AUTHENTICATION_SERVICE_NONE, Security_Fields.PROTECTION_LEVEL_NONE, 0, null)
		{
		}

		public AuthenticationVerifier(int authenticatorLength) : this(Security_Fields.AUTHENTICATION_SERVICE_NONE, Security_Fields.PROTECTION_LEVEL_NONE, 0, authenticatorLength)
		{
		}

		public AuthenticationVerifier(int authenticationService, int protectionLevel, int contextId, int authenticatorLength) : this(authenticationService, protectionLevel, contextId, new sbyte[authenticatorLength])
		{
		}

		public AuthenticationVerifier(int authenticationService, int protectionLevel, int contextId, sbyte[] body)
		{
			this.authenticationService = authenticationService;
			this.protectionLevel = protectionLevel;
			this.contextId = contextId;
			this.body = body;
		}

		public override void decode(NetworkDataRepresentation ndr, NdrBuffer src)
		{
			src.align(4);
			authenticationService = src.dec_ndr_small();
			protectionLevel = src.dec_ndr_small();
			src.dec_ndr_small(); // padding count
			contextId = src.dec_ndr_long();
			Array.Copy(src.Buffer, src.Index, body, 0, body.Length);
			src.index += body.Length;
		}

		public override void encode(NetworkDataRepresentation ndr, NdrBuffer dst)
		{
			var padding = dst.align(4, (sbyte) 0);
			dst.enc_ndr_small(authenticationService);
			dst.enc_ndr_small(protectionLevel);
			dst.enc_ndr_small(padding);
			dst.enc_ndr_small(0); //Reserved
			dst.enc_ndr_long(contextId);
			Array.Copy(body, 0, dst.Buffer, dst.Index, body.Length);
			//dst.index += body.length;
			dst.advance(body.Length);
		}

		public override bool Equals(object obj)
		{
			if (!(obj is AuthenticationVerifier))
			{
				return false;
			}
			var other = (AuthenticationVerifier) obj;
			return authenticationService == other.authenticationService && protectionLevel == other.protectionLevel && contextId == other.contextId && Arrays.Equals(body, other.body);
		}

		public override int GetHashCode()
		{
			return authenticationService ^ protectionLevel ^ contextId;
		}

	}

}