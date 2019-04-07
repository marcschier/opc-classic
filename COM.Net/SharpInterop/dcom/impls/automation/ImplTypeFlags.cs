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
	/// Implements the <i>IMPLTYPEFLAGS</i> structure of COM Automation.
	/// 
	/// @since 2.0 (formerly IMPLTYPEFLAGS)
	/// </summary>

	public interface ImplTypeFlags
	{

		/// <summary>
		/// The interface or dispinterface represents the default for the source or sink.
		/// </summary>
		/// <summary>
		/// This member of a coclass is called rather than implemented.
		/// </summary>
		/// <summary>
		/// The member should not be displayed or programmable by users.
		/// </summary>
		/// <summary>
		/// Sinks receive events through the VTBL.
		/// </summary>

	}

	public static class ImplTypeFlags_Fields
	{
		public const int IMPLTYPEFLAG_FDEFAULT = 0x1;
		public const int IMPLTYPEFLAG_FSOURCE = 0x2;
		public const int IMPLTYPEFLAG_FRESTRICTED = 0x4;
		public const int IMPLTYPEFLAG_FDEFAULTVTABLE = 0x800;
	}

}