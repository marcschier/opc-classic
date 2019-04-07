// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.impls.automation
{

	/// <summary>
	/// Implements the <i>CALLCONV</i> data type of COM Automation.
	/// Definition from MSDN: <i> Identifies the calling convention used by a member function. </i>
	/// </summary>
	public interface CallConv
	{
		/// <summary>
		/// Indicates that the Cdecl calling convention is used for a method.
		/// </summary>
		/// <summary>
		/// Indicates that the Mscpascal calling convention is used for a method.
		/// </summary>
		/// <summary>
		/// Indicates that the Pascal calling convention is used for a method.
		/// </summary>
		/// <summary>
		/// Indicates that the Macpascal calling convention is used for a method.
		/// </summary>
		/// <summary>
		/// Indicates that the Stdcall calling convention is used for a method.
		/// </summary>
		/// <summary>
		/// Indicates that the Syscall calling convention is used for a method.
		/// </summary>
		/// <summary>
		/// Indicates that the Mpwcdecl calling convention is used for a method.
		/// </summary>
		/// <summary>
		/// Indicates that the Mpwpascal calling convention is used for a method.
		/// </summary>
		/// <summary>
		/// Indicates the end of the CALLCONV enumeration.
		/// </summary>

	}

	public static class CallConv_Fields
	{
		public static readonly int? CC_FASTCALL = 0;
		public static readonly int? CC_CDECL = 1;
		public static readonly int? CC_MSCPASCAL = (int)CC_CDECL + 1;
		public static readonly int? CC_PASCAL = CC_MSCPASCAL;
		public static readonly int? CC_MACPASCAL = (int)CC_PASCAL + 1;
		public static readonly int? CC_STDCALL = (int)CC_MACPASCAL + 1;
		public static readonly int? CC_FPFASTCALL = (int)CC_STDCALL + 1;
		public static readonly int? CC_SYSCALL = (int)CC_FPFASTCALL + 1;
		public static readonly int? CC_MPWCDECL = (int)CC_SYSCALL + 1;
		public static readonly int? CC_MPWPASCAL = (int)CC_MPWCDECL + 1;
		public static readonly int? CC_MAX = (int)CC_MPWPASCAL + 1;
	}

}