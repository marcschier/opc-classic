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

    using JIArray = org.jinterop.dcom.core.JIArray;
    using JIPointer = org.jinterop.dcom.core.JIPointer;
    using JIStruct = org.jinterop.dcom.core.JIStruct;

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
    public sealed class FuncDesc {

      private const long SerialVersionUID = -1361861233072624432L;
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
    //    FuncKind funckind;           // Specifies whether the function is virtual, static, or dispatch-only.
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

        private JIStruct Values = null;
        /// <summary>
        /// Function member ID.
        /// </summary>
        public readonly int MemberId;
        public readonly JIPointer Lprgscode;
        public readonly JIPointer LprgelemdescParam;

        /// <summary>
        /// Specifies whether the function is virtual, static, or dispatch-only.
        /// </summary>
        public readonly int FuncKind;

        /// <summary>
        /// Invocation kind. Indicates if this is a property function, and if so, what kind.
        /// </summary>
        public readonly int InvokeKind;

        /// <summary>
        /// Specifies the function's calling convention.
        /// </summary>
        public readonly int CallConv;

        /// <summary>
        ///  Count of total number of parameters.
        /// </summary>
        public readonly short CParams;

        /// <summary>
        /// Count of optional parameters (detailed description follows).
        /// </summary>
        public readonly short CParamsOpt;
        /// <summary>
        /// For FUNC_VIRTUAL, specifies the offset in the VTBL.
        /// </summary>
        public readonly short OVft;
        /// <summary>
        /// Count of permitted return values.
        /// </summary>
        public readonly short CScodes;
        /// <summary>
        /// Contains the return type of the function.
        /// </summary>
        public readonly ElemDesc ElemdescFunc;
        /// <summary>
        /// Definition of flags follows.
        /// </summary>
        public readonly short WFuncFlags;


        public FuncDesc(JIPointer values) : this(values.Null ? null : (JIStruct)values.GetReferent()) {
        }
        public FuncDesc(JIStruct filledStruct) {
            if (filledStruct == null) {
                Values = null;
                MemberId = -1;
                Lprgscode = null;
                LprgelemdescParam = null;
                FuncKind = -1;
                InvokeKind = -1;
                CallConv = -1;
                CParams = -1;
                CParamsOpt = -1;
                OVft = -1;
                CScodes = -1;
                ElemdescFunc = null;
                WFuncFlags = -1;
                return;
            }
            Values = filledStruct;
            MemberId = (int)((int?)Values.GetMember(0));
            Lprgscode = (JIPointer)Values.GetMember(1);
            JIPointer ptr = (JIPointer)Values.GetMember(2);
            JIArray arrayOfElemDesc = null;
            if (!ptr.Null) {
                JIArray arry = (JIArray)ptr.GetReferent();
                object[] obj = (object[])arry.ArrayInstance;
    //            ElemDesc[] arry2 = new ElemDesc[obj.length];
    //            for (int i = 0; i < obj.length; i++)
    //            {
    //                arry2[i] = new ElemDesc((JIStruct)obj[i]);
    //            }

    //            arrayOfElemDesc = new JIArray(arry2);
                arrayOfElemDesc = new JIArray(obj);
            }

            LprgelemdescParam = new JIPointer(arrayOfElemDesc);
            FuncKind = (int)((int?)Values.GetMember(3));
            InvokeKind = (int)((int?)Values.GetMember(4));
            CallConv = (int)((int?)Values.GetMember(5));
            CParams = (short)((short?)Values.GetMember(6));
            CParamsOpt = (short)((short?)Values.GetMember(7));
            OVft = (short)((short?)Values.GetMember(8));
            CScodes = (short)((short?)Values.GetMember(9));
            ElemdescFunc = new ElemDesc(((JIStruct)Values.GetMember(10)));
            WFuncFlags = (short)((short?)Values.GetMember(11));
        }

    }

}