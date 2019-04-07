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

	/// <summary>
	///Implements the <i>CALLCONV</i> data type of COM Automation.
	/// <para>
	/// Definition from MSDN: <i> Identifies the calling convention used by a member function. </i>
	/// 
	/// 
	/// @since 2.0 (formerly CALLCONV)
	/// </para>
	/// </summary>
	public interface CallConv {
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

	public static class CallConv_Fields {
		public static readonly int? CC_FASTCALL = new int?(0);
		public static readonly int? CC_CDECL = new int?(1);
		public static readonly int? CC_MSCPASCAL = new int?((int)CC_CDECL + (1));
		public static readonly int? CC_PASCAL = CC_MSCPASCAL;
		public static readonly int? CC_MACPASCAL = new int?((int)CC_PASCAL + 1);
		public static readonly int? CC_STDCALL = new int?((int)CC_MACPASCAL + 1);
		public static readonly int? CC_FPFASTCALL = new int?((int)CC_STDCALL + 1);
		public static readonly int? CC_SYSCALL = new int?((int)CC_FPFASTCALL + 1);
		public static readonly int? CC_MPWCDECL = new int?((int)CC_SYSCALL + 1);
		public static readonly int? CC_MPWPASCAL = new int?((int)CC_MPWCDECL + 1);
		public static readonly int? CC_MAX = new int?((int)CC_MPWPASCAL + 1);
	}

}