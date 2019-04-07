// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.impls.automation {

    using JIPointer = core.JIPointer;
    using JIStruct = core.JIStruct;
    using JIUnion = core.JIUnion;

    /// <summary>
    /// Implements the <i>VARDESC</i> structure of COM Automation
    /// 
    /// @since 1.0
    /// 
    /// </summary>
    [Serializable]
	public sealed class VarDesc
	{

		private const long serialVersionUID = -3874889610447398180L;
		public const int VAR_PERINSTANCE = 0;
		public const int VAR_STATIC = 1;
		public const int VAR_CONST = 2;
		public const int VAR_DISPATCH = 3;

		public readonly int memberId;
		public readonly JIPointer lpstrSchema;
		public readonly JIUnion u;
		/// <summary>
		///  Contains the variable type.
		/// </summary>
		public readonly ElemDesc elemdescVar;
		/// <summary>
		/// Definition of flags follows
		/// </summary>
		public readonly short wVarFlags;
		public readonly int varkind;

		internal VarDesc(JIPointer values) : this(values.Null ? null : (JIStruct)values.Referent)
		{
		}

		internal VarDesc(JIStruct filledStruct)
		{
			if (filledStruct == null)
			{
				memberId = -1;
				lpstrSchema = null;
				u = null;
				elemdescVar = null;
				wVarFlags = -1;
				varkind = -1;
				return;
			}

			memberId = (int)(int?)filledStruct.GetMember(0);
			lpstrSchema = (JIPointer)filledStruct.GetMember(1);
			u = (JIUnion)filledStruct.GetMember(2);
			elemdescVar = new ElemDesc((JIStruct)filledStruct.GetMember(3));
			wVarFlags = (short)(short?)filledStruct.GetMember(4);
			varkind = (int)(int?)filledStruct.GetMember(5);
		}


	}

}