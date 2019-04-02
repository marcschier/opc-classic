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


namespace rpc.pdu
{

	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	public class Auth3Pdu : ConnectionOrientedPdu
	{

		public const int AUTH3_TYPE = 0x10;

		public Auth3Pdu()
		{
			//Really useless value
			CallId = 0;
		}
        public override int Type => AUTH3_TYPE;

        protected internal override void writeBody(NetworkDataRepresentation ndr)
		{
			ndr.writeUnsignedLong(0);
		}
	}

}