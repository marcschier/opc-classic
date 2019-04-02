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
    ///Implements the <i>TYPEDESC</i> structure of COM Automation and
    /// describes the type of a variable, the return type of a function,
    /// or the type of a function parameter.
    /// 
    /// @since 1.0
    /// </summary>
    [Serializable]
	public sealed class TypeDesc
	{

		private const long serialVersionUID = 6276233095707726579L;
		public static readonly short? VT_PTR = (short)0x1a;
		public static readonly short? VT_SAFEARRAY = (short)0x1b;
		public static readonly short? VT_CARRAY = (short)0x1c;
		public static readonly short? VT_USERDEFINED = (short)0x1d;

		public readonly JIPointer typeDesc;
		public readonly JIPointer arrayDesc;
		public readonly int hrefType;
		public readonly short vt;

		internal TypeDesc(JIStruct values)
		{
			if (values == null)
			{
				typeDesc = null;
				arrayDesc = null;
				hrefType = -1;
				vt = -1;
				return;
			}

			vt = (short)(short?)values.getMember(1);
			var union = (JIUnion)values.getMember(0);

			if (vt.Equals(VT_PTR) || vt.Equals(VT_SAFEARRAY))
			{
				JIPointer pointer = (pointer = (JIPointer)union.Members[VT_PTR]) == null ? (JIPointer)union.Members[VT_SAFEARRAY] : pointer;
				typeDesc = new JIPointer(new TypeDesc(pointer),false);
				arrayDesc = null;
				hrefType = -1;
			}
			else if (vt.Equals(VT_CARRAY))
			{
				hrefType = -1;
				typeDesc = null;
				arrayDesc = new JIPointer(new ArrayDesc((JIPointer)union.Members[VT_CARRAY]));
			}
			else if (vt.Equals(VT_USERDEFINED))
			{
				typeDesc = null;
				arrayDesc = null;
				hrefType = (int)(int?)union.Members[VT_USERDEFINED];
			}
			else
			{
				typeDesc = null;
				arrayDesc = null;
				hrefType = -1;
			}

		}

		internal TypeDesc(JIPointer values) : this(values.Null ? null : (JIStruct)values.Referent)
		{
		}

	}

}