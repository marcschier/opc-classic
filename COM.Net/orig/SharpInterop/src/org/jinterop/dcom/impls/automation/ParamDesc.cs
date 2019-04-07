using System;

/// <summary>
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
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.impls.automation {

	using JIPointer = org.jinterop.dcom.core.JIPointer;
	using JIStruct = org.jinterop.dcom.core.JIStruct;

	/// <summary>
	/// Implements the <i>PARAMDESC</i> structure of COM Automation. Contains
	/// information needed for transferring a structure element, parameter,
	/// or function return value between processes.
	/// 
	/// @since 1.0
	/// 
	/// </summary>
	[Serializable]
	public sealed class ParamDesc {

		private const long SerialVersionUID = 7181403713923608809L;
		public const short PARAMFLAG_NONE = 0x00;
		public const short PARAMFLAG_FIN = 0x01;
		public const short PARAMFLAG_FOUT = 0x02;
		public const short PARAMFLAG_FLCID = 0x04;
		public const short PARAMFLAG_FRETVAL = 0x08;
		public const short PARAMFLAG_FOPT = 0x10;
		public const short PARAMFLAG_FHASDEFAULT = 0x20;
		public const short PARAMFLAG_FHASCUSTDATA = 0x40;


		public readonly JIPointer LpVarValue;

		/// <summary>
		/// IN, OUT, etc
		/// </summary>
		public readonly short WPARAMFlags;

		public ParamDesc(JIStruct values) {
			if (values == null) {
				LpVarValue = null;
				WPARAMFlags = -1;
				return;
			}

			LpVarValue = (JIPointer)values.GetMember(0);
			//lpVarValue = (JIVariant)values.getMember(0);
			WPARAMFlags = (short)((short?)values.GetMember(1));
		}

	}

}