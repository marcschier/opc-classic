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
	/// Implements the <i>TYPEKIND</i> structure of COM Automation
	/// </summary>
	public enum TypeKind : int
	{
        /// <summary>
        /// A set of enumerators.
        /// </summary>
        TKIND_ENUM = 0,

        /// <summary>
        /// A structure with no methods.
        /// </summary>
   		TKIND_RECORD,

        /// <summary>
        /// A module that can only have static functions and data (for example, a DLL).
        /// </summary>
        TKIND_MODULE,

        /// <summary>
        /// A type that has virtual and pure functions.
        /// </summary>
        TKIND_INTERFACE,

        /// <summary>
        /// A set of methods and properties that are accessible through IDispatch::Invoke.
        /// By default, dual interfaces return TKIND_DISPATCH.
        /// </summary>
        TKIND_DISPATCH,

        /// <summary>
        /// A set of implemented component object interfaces.
        /// </summary>
        TKIND_COCLASS,

        /// <summary>
        /// A type that is an alias for another type.
        /// </summary>
        TKIND_ALIAS,

        /// <summary>
        /// A union, all of whose members have an offset of zero.
        /// </summary>
   		TKIND_UNION,

        /// <summary>
        /// End of ENUM marker.
        /// </summary>
		TKIND_MAX
    }
}