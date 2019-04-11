//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.dcom.impls.automation {
    using org.jinterop.dcom.core;
    using System;

    /// <summary>
    /// Implements the <i>FUNCDESC</i> structure of COM Automation.
    /// <i> Describes a function.</i>
    /// See http://msdn.microsoft.com/en-us/library/ms221425(VS.85).aspx .
    /// </summary>
    /// <remarks>
    ///  MEMBERID memid;           // Function member ID.
    ///  /* [size_is] */ SCODE __RPC_FAR *lprgscode;
    ///  /* [size_is] */ ELEMDESC __RPC_FAR *lprgelemdescParam;
    ///  FuncKind funckind;        // Specifies whether the function is virtual, static, or dispatch-only.
    ///  InvokeKind invkind;       // Invocation kind. Indicates if this is a property function, and if so, what kind.
    ///  CallConv callconv;        // Specifies the function's calling
    ///                            // convention.
    ///  short cParams;            // Count of total number of parameters.
    ///  short cParamsOpt;         // Count of optional parameters (detailed
    ///                            // description follows).
    ///  short oVft;               // For FUNC_VIRTUAL, specifies the offset in the VTBL.
    ///  short cScodes;            // Count of permitted return values.
    ///  ELEMDESC elemdescFunc;    // Contains the return type of the function.
    ///  WORD wFuncFlags;          // Definition of flags follows.
    /// </remarks>
    [Serializable]
    public sealed class FuncDesc {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
        public readonly int memberId;
        public readonly JIPointer lprgscode;
        public readonly JIPointer lprgelemdescParam;
        public readonly int funcKind;
        public readonly int invokeKind;
        public readonly int callConv;
        public readonly short cParams;
        public readonly short cParamsOpt;
        public readonly short oVft;
        public readonly short cScodes;
        public readonly ElemDesc elemdescFunc;
        public readonly short wFuncFlags;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Create description
        /// </summary>
        /// <param name="values"></param>
        internal FuncDesc(JIPointer values) :
            this(values.IsNull ? null : (JIStruct)values.Referent) {
        }

        /// <summary>
        /// Create description
        /// </summary>
        /// <param name="filledStruct"></param>
        internal FuncDesc(JIStruct filledStruct) {
            if (filledStruct == null) {
                _values = null;
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
            _values = filledStruct;
            memberId = (int)_values.GetMember(0);
            lprgscode = (JIPointer)_values.GetMember(1);
            var ptr = (JIPointer)_values.GetMember(2);
            JIArray arrayOfElemDesc = null;
            if (!ptr.IsNull) {
                var arry = (JIArray)ptr.Referent;
                var obj = (object[])arry.ArrayInstance;
                arrayOfElemDesc = new JIArray(obj);
            }
            lprgelemdescParam = new JIPointer(arrayOfElemDesc);
            funcKind = (int)_values.GetMember(3);
            invokeKind = (int)_values.GetMember(4);
            callConv = (int)_values.GetMember(5);
            cParams = (short)_values.GetMember(6);
            cParamsOpt = (short)_values.GetMember(7);
            oVft = (short)_values.GetMember(8);
            cScodes = (short)_values.GetMember(9);
            elemdescFunc = new ElemDesc((JIStruct)_values.GetMember(10));
            wFuncFlags = (short)_values.GetMember(11);
        }

        private readonly JIStruct _values;
    }
}