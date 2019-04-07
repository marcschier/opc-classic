using System;

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
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	public class AuthenticationVerifier : NdrObject {

		public int AuthenticationService;

		public int ProtectionLevel;

		public int ContextId;

		public sbyte[] Body;

		public AuthenticationVerifier() : this(rpc.Security_Fields.AUTHENTICATION_SERVICE_NONE, rpc.Security_Fields.PROTECTION_LEVEL_NONE, 0, null) {
		}

		public AuthenticationVerifier(int authenticatorLength) : this(rpc.Security_Fields.AUTHENTICATION_SERVICE_NONE, rpc.Security_Fields.PROTECTION_LEVEL_NONE, 0, authenticatorLength) {
		}

		public AuthenticationVerifier(int authenticationService, int protectionLevel, int contextId, int authenticatorLength) : this(authenticationService, protectionLevel, contextId, new sbyte[authenticatorLength]) {
		}

		public AuthenticationVerifier(int authenticationService, int protectionLevel, int contextId, sbyte[] body) {
			this.AuthenticationService = authenticationService;
			this.ProtectionLevel = protectionLevel;
			this.ContextId = contextId;
			this.Body = body;
		}

		public override void Decode(NetworkDataRepresentation ndr, NdrBuffer src) {
			src.Align(4);
			AuthenticationService = src.Dec_ndr_small();
			ProtectionLevel = src.Dec_ndr_small();
			src.Dec_ndr_small(); // padding count
			ContextId = src.Dec_ndr_long();
			Array.Copy(src.Buffer, src.Index, Body, 0, Body.Length);
			src.Index_Renamed += Body.Length;
		}

		public override void Encode(NetworkDataRepresentation ndr, NdrBuffer dst) {
			int padding = dst.Align(4, (sbyte) 0);
			dst.Enc_ndr_small(AuthenticationService);
			dst.Enc_ndr_small(ProtectionLevel);
			dst.Enc_ndr_small(padding);
			dst.Enc_ndr_small(0); //Reserved
			dst.Enc_ndr_long(ContextId);
			Array.Copy(Body, 0, dst.Buffer, dst.Index, Body.Length);
			//dst.index += body.length;
			dst.Advance(Body.Length);
		}

		public override bool Equals(object obj) {
			if (!(obj is AuthenticationVerifier)) {
				return false;
			}
			AuthenticationVerifier other = (AuthenticationVerifier) obj;
			return (AuthenticationService == other.AuthenticationService && ProtectionLevel == other.ProtectionLevel && ContextId == other.ContextId && Arrays.Equals(Body, other.Body));
		}

		public override int GetHashCode() {
			return AuthenticationService ^ ProtectionLevel ^ ContextId;
		}

	}

}