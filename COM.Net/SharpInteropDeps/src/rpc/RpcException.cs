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

	public class RpcException : IOException
	{

		/// 
		private const long serialVersionUID = -6529915206074406077L;

		public RpcException() : base()
		{
		}

		public RpcException(string message) : base(message)
		{
		}

	}

}