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

	public interface Connection
	{

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void transmit(ConnectionOrientedPdu pdu, Transport transport) throws java.io.IOException;
		void transmit(ConnectionOrientedPdu pdu, Transport transport);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ConnectionOrientedPdu receive(Transport transport) throws java.io.IOException;
		ConnectionOrientedPdu receive(Transport transport);

	}

}