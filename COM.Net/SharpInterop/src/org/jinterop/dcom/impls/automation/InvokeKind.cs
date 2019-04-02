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
	/// Implements the <i>INVOKEKIND</i> structure of COM Automation.
	/// 
	/// @since 2.0 (formerly INVOKEKIND)
	/// </summary>
	public interface InvokeKind
	{

		/// <summary>
		///  The member is called using a normal function invocation syntax
		/// </summary>

		/// <summary>
		/// The function is invoked using a normal property-access syntax.
		/// </summary>

		/// <summary>
		/// The function is invoked using a property value assignment syntax.
		/// Syntactically, a typical programming language might represent
		/// changing a property in the same way as assignment.
		/// For example:object.property : = value.
		/// </summary>

		/// <summary>
		///  The function is invoked using a property reference assignment syntax.
		/// </summary>
	}

	public static class InvokeKind_Fields
	{
		public static readonly int? INVOKE_FUNC = 1;
		public static readonly int? INVOKE_PROPERTYGET = 2;
		public static readonly int? INVOKE_PROPERTYPUT = 4;
		public static readonly int? INVOKE_PROPERTYPUTREF = 8;
	}

}