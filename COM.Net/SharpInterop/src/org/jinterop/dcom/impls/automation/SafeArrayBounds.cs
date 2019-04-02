// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.impls.automation {

    using JIStruct = core.JIStruct;

    /// <summary>
    /// Implements the <i>SAFEARRAYBOUNDS</i> structure of COM Automation.
    /// 
    /// @since 1.0
    /// 
    /// </summary>
    [Serializable]
	public sealed class SafeArrayBounds
	{

		private const long serialVersionUID = -3110688445129575984L;
		public readonly int cElements;
		public readonly int lLbound;

		internal SafeArrayBounds(JIStruct values)
		{
			if (values == null)
			{
				cElements = -1;
				lLbound = -1;
				return;
			}
			cElements = (int)(int?)values.getMember(0);
			lLbound = (int)(int?)values.getMember(0);
		}
	}

}