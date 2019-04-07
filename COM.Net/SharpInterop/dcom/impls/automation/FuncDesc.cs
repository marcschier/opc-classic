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
    using JIArray = core.JIArray;
    using JIPointer = core.JIPointer;
    using JIStruct = core.JIStruct;

    /// <summary>
    /// Implements the <i>FUNCDESC</i> structure of COM Automation.
    /// <para>
    /// Definition from MSDN: <i> Describes a function.</i>
    /// 
    /// More information can be obtained here http://msdn.microsoft.com/en-us/library/ms221425(VS.85).aspx .
    /// 
    /// @since 1.0
    /// </para>
    /// </summary>
    [Serializable]
	public sealed class FuncDesc
	{

	  private const long serialVersionUID = -1361861233072624432L;
	  public const int FUNCFLAG_FRESTRICTED = 0x1;
	  public const int FUNCFLAG_FSOURCE = 0x2;
	  public const int FUNCFLAG_FBINDABLE = 0x4;
	  public const int FUNCFLAG_FREQUESTEDIT = 0x8;
	  public const int FUNCFLAG_FDISPLAYBIND = 0x10;
	  public const int FUNCFLAG_FDEFAULTBIND = 0x20;
	  public const int FUNCFLAG_FHIDDEN = 0x40;
	  public const int FUNCFLAG_FUSESGETLASTERROR = 0x80;
	  public const int FUNCFLAG_FDEFAULTCOLLELEM = 0x100;
	  public const int FUNCFLAG_FUIDEFAULT = 0x200;
	  public const int FUNCFLAG_FNONBROWSABLE = 0x400;
	  public const int FUNCFLAG_FREPLACEABLE = 0x800;
	  public const int FUNCFLAG_FIMMEDIATEBIND = 0x1000;


	//    MEMBERID memid;                        // Function member ID.
	///* [size_is] */ SCODE __RPC_FAR *lprgscode;
	///* [size_is] */ ELEMDESC __RPC_FAR *lprgelemdescParam;
	//	FuncKind funckind;           // Specifies whether the function is virtual, static, or dispatch-only.
	//    InvokeKind invkind;        // Invocation kind. Indicates if this is a property function, and if so, what kind.
	//    CallConv callconv;        // Specifies the function's calling
	//                            // convention.
	//    short cParams;            // Count of total number of parameters.
	//    short cParamsOpt;        // Count of optional parameters (detailed
	//                            // description follows).
	//    short oVft;                // For FUNC_VIRTUAL, specifies the offset in the VTBL.
	//    short cScodes;    // Count of permitted return values.
	//    ELEMDESC elemdescFunc;    // Contains the return type of the function.
	//    WORD wFuncFlags;     // Definition of flags follows.

		private JIStruct values;
		/// <summary>
		/// Function member ID.
		/// </summary>
		public readonly int memberId;
		public readonly JIPointer lprgscode;
		public readonly JIPointer lprgelemdescParam;

		/// <summary>
		/// Specifies whether the function is virtual, static, or dispatch-only.
		/// </summary>
		public readonly int funcKind;

		/// <summary>
		/// Invocation kind. Indicates if this is a property function, and if so, what kind.
		/// </summary>
		public readonly int invokeKind;

		/// <summary>
		/// Specifies the function's calling convention.
		/// </summary>
		public readonly int callConv;

		/// <summary>
		///  Count of total number of parameters.
		/// </summary>
		public readonly short cParams;

		/// <summary>
		/// Count of optional parameters (detailed description follows).
		/// </summary>
		public readonly short cParamsOpt;
		/// <summary>
		/// For FUNC_VIRTUAL, specifies the offset in the VTBL.
		/// </summary>
		public readonly short oVft;
		/// <summary>
		/// Count of permitted return values.
		/// </summary>
		public readonly short cScodes;
		/// <summary>
		/// Contains the return type of the function.
		/// </summary>
		public readonly ElemDesc elemdescFunc;
		/// <summary>
		/// Definition of flags follows.
		/// </summary>
		public readonly short wFuncFlags;


		internal FuncDesc(JIPointer values) : this(values.Null ? null : (JIStruct)values.Referent)
		{
		}
		internal FuncDesc(JIStruct filledStruct)
		{
			if (filledStruct == null)
			{
				values = null;
				memberId = -1;
				lprgscode = null;
				lprgelemdescParam = null;
				funcKind = -1;
				invokeKind = -1;
				callConv = -1;
				cParams = -1;
				cParamsOpt = -1;
				oVft = -1;
				cScodes = -1;
				elemdescFunc = null;
				wFuncFlags = -1;
				return;
			}
			values = filledStruct;
			memberId = (int)(int?)values.GetMember(0);
			lprgscode = (JIPointer)values.GetMember(1);
			var ptr = (JIPointer)values.GetMember(2);
			JIArray arrayOfElemDesc = null;
			if (!ptr.Null)
			{
				var arry = (JIArray)ptr.Referent;
				var obj = (object[])arry.ArrayInstance;
	//			ElemDesc[] arry2 = new ElemDesc[obj.length];
	//			for (int i = 0; i < obj.length; i++)
	//			{
	//				arry2[i] = new ElemDesc((JIStruct)obj[i]);
	//			}

	//			arrayOfElemDesc = new JIArray(arry2);
				arrayOfElemDesc = new JIArray(obj);
			}

			lprgelemdescParam = new JIPointer(arrayOfElemDesc);
			funcKind = (int)(int?)values.GetMember(3);
			invokeKind = (int)(int?)values.GetMember(4);
			callConv = (int)(int?)values.GetMember(5);
			cParams = (short)(short?)values.GetMember(6);
			cParamsOpt = (short)(short?)values.GetMember(7);
			oVft = (short)(short?)values.GetMember(8);
			cScodes = (short)(short?)values.GetMember(9);
			elemdescFunc = new ElemDesc((JIStruct)values.GetMember(10));
			wFuncFlags = (short)(short?)values.GetMember(11);
		}

	}

}