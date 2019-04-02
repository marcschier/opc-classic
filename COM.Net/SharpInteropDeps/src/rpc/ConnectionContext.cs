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

namespace rpc
{


	using PresentationContext = core.PresentationContext;

	public interface ConnectionContext
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ConnectionOrientedPdu init(rpc.core.PresentationContext context, java.util.Properties properties) throws java.io.IOException;
		ConnectionOrientedPdu init(PresentationContext context, Properties properties);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ConnectionOrientedPdu alter(rpc.core.PresentationContext context) throws java.io.IOException;
		ConnectionOrientedPdu alter(PresentationContext context);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ConnectionOrientedPdu accept(ConnectionOrientedPdu pdu) throws java.io.IOException;
		ConnectionOrientedPdu accept(ConnectionOrientedPdu pdu);

		Connection Connection {get;}

		bool Established {get;}

	}

	public static class ConnectionContext_Fields
	{
		public const string MAX_TRANSMIT_FRAGMENT = "rpc.connectionContext.maxTransmitFragment";
		public const string MAX_RECEIVE_FRAGMENT = "rpc.connectionContext.maxReceiveFragment";
		public const int DEFAULT_MAX_TRANSMIT_FRAGMENT = 4280;
		public const int DEFAULT_MAX_RECEIVE_FRAGMENT = 4280;
	}

}