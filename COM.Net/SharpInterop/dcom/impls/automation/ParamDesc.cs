// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.impls.automation {
    using org.jinterop.dcom.common;
    using org.jinterop.dcom.core;
    using rpc.core;
    using System;

    /// <summary>
    /// Implements the <i>PARAMDESC</i> structure of COM Automation. Contains
    /// information needed for transferring a structure element, parameter,
    /// or function return value between processes.
    /// </summary>
    [Serializable]
	public sealed class ParamDesc
	{
		public const short PARAMFLAG_NONE = 0x00;
		public const short PARAMFLAG_FIN = 0x01;
		public const short PARAMFLAG_FOUT = 0x02;
		public const short PARAMFLAG_FLCID = 0x04;
		public const short PARAMFLAG_FRETVAL = 0x08;
		public const short PARAMFLAG_FOPT = 0x10;
		public const short PARAMFLAG_FHASDEFAULT = 0x20;
		public const short PARAMFLAG_FHASCUSTDATA = 0x40;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
        public readonly JIPointer lpVarValue;
		public readonly short wPARAMFlags;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Create param description
        /// </summary>
        /// <param name="values"></param>
		internal ParamDesc(JIStruct values)
		{
			if (values == null)
			{
				lpVarValue = null;
				wPARAMFlags = -1;
				return;
			}

			lpVarValue = (JIPointer)values.GetMember(0);
			wPARAMFlags = (short)(short?)values.GetMember(1);
		}
	}
}