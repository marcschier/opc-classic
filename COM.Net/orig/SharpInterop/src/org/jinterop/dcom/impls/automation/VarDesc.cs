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

    using JIPointer = org.jinterop.dcom.core.JIPointer;
    using JIStruct = org.jinterop.dcom.core.JIStruct;
    using JIUnion = org.jinterop.dcom.core.JIUnion;

    /// <summary>
    /// Implements the <i>VARDESC</i> structure of COM Automation
    /// 
    /// @since 1.0
    /// 
    /// </summary>
    [Serializable]
    public sealed class VarDesc {

        private const long SerialVersionUID = -3874889610447398180L;
        public const int VAR_PERINSTANCE = 0;
        public const int VAR_STATIC = 1;
        public const int VAR_CONST = 2;
        public const int VAR_DISPATCH = 3;

        public readonly int MemberId;
        public readonly JIPointer LpstrSchema;
        public readonly JIUnion u;
        /// <summary>
        ///  Contains the variable type.
        /// </summary>
        public readonly ElemDesc ElemdescVar;
        /// <summary>
        /// Definition of flags follows
        /// </summary>
        public readonly short WVarFlags;
        public readonly int Varkind;

        public VarDesc(JIPointer values) : this(values.Null ? null : (JIStruct)values.GetReferent()) {
        }

        public VarDesc(JIStruct filledStruct) {
            if (filledStruct == null) {
                MemberId = -1;
                LpstrSchema = null;
                u = null;
                ElemdescVar = null;
                WVarFlags = -1;
                Varkind = -1;
                return;
            }

            MemberId = (int)((int?)filledStruct.GetMember(0));
            LpstrSchema = (JIPointer)filledStruct.GetMember(1);
            u = (JIUnion)filledStruct.GetMember(2);
            ElemdescVar = new ElemDesc((JIStruct)filledStruct.GetMember(3));
            WVarFlags = (short)((short?)filledStruct.GetMember(4));
            Varkind = (int)((int?)filledStruct.GetMember(5));
        }


    }

}