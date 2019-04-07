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

namespace rpc {

	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	public interface Security {

		int VerifierLength { get; }

		int AuthenticationService { get; }

		int ProtectionLevel { get; }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void processIncoming(ndr.NetworkDataRepresentation ndr, int index, int length, int verifierIndex, boolean isFragmented) throws java.io.IOException;
		void ProcessIncoming(NetworkDataRepresentation ndr, int index, int length, int verifierIndex, bool isFragmented);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void processOutgoing(ndr.NetworkDataRepresentation ndr, int index, int length, int verifierIndex, boolean isFragmented) throws java.io.IOException;
		void ProcessOutgoing(NetworkDataRepresentation ndr, int index, int length, int verifierIndex, bool isFragmented);

	}

	public static class Security_Fields {
		public const string USERNAME = "rpc.security.username";
		public const string PASSWORD = "rpc.security.password";
		public const int AUTHENTICATION_SERVICE_NONE = 0;
		public const int PROTECTION_LEVEL_NONE = 1;
		public const int PROTECTION_LEVEL_CONNECT = 2;
		public const int PROTECTION_LEVEL_CALL = 3;
		public const int PROTECTION_LEVEL_PACKET = 4;
		public const int PROTECTION_LEVEL_INTEGRITY = 5;
		public const int PROTECTION_LEVEL_PRIVACY = 6;
	}

}