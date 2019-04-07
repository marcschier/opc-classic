// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 



namespace org.jinterop.dcom.impls.automation {
    using System;
    using JIPointer = core.JIPointer;
    using JIStruct = core.JIStruct;

    /// <summary>
    ///Implements the <i>ELEMDESC</i> structure of COM Automation.
    /// <para>
    /// Definition from MSDN: <i> Includes the type description and process-transfer
    /// information for a variable a function, or a function parameter.
    /// </i>
    /// 
    /// 
    /// @since 1.0
    /// 
    /// </para>
    /// </summary>
    [Serializable]
	public sealed class ElemDesc
	{

		private const long serialVersionUID = 3022259075461969376L;
		/// <summary>
		/// Type of the element.
		/// </summary>
		public readonly TypeDesc typeDesc;
		/// <summary>
		/// Information about the parameter.
		/// </summary>
		public readonly ParamDesc paramDesc;

		public ElemDesc(JIStruct values)
		{
			if (values == null)
			{
				typeDesc = null;
				paramDesc = null;
				return;
			}
			typeDesc = new TypeDesc((JIStruct)values.GetMember(0));
			paramDesc = new ParamDesc((JIStruct)values.GetMember(1));
		}

		internal ElemDesc(JIPointer ptrValues) : this(ptrValues.Null ? null : (JIStruct)ptrValues.Referent)
		{
		}

	}

}