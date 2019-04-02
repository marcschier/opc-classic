using System;
using System.Collections;

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


	public interface Fragmentable : ICloneable
	{

		IEnumerator fragment(int size);

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Fragmentable assemble(java.util.Iterator fragments) throws java.io.IOException;
		Fragmentable assemble(IEnumerator fragments);

		object clone();

	}

}