// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.transport
{

	using ConnectionOrientedEndpoint = rpc.ConnectionOrientedEndpoint;
	using ITransport = rpc.ITransport;
	using PresentationSyntax = rpc.core.PresentationSyntax;

	/// <summary>
	/// @exclude
	/// @since 1.0
	/// 
	/// </summary>
	public sealed class JIComEndpoint : ConnectionOrientedEndpoint
	{

	  internal JIComEndpoint(ITransport transport, PresentationSyntax syntax) : base(transport,syntax)
	  {
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void rebindEndPoint() throws java.io.IOException
	  public void rebindEndPoint()
	  {
		  Rebind();
	  }
	}

}