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
    /// Implements the <i>VARDESC</i> structure of COM Automation
    /// </summary>
    [Serializable]
    public sealed class VarDesc {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
        public readonly int memberId;
        public readonly JIPointer lpstrSchema;
        public readonly JIUnion u;
        public readonly ElemDesc elemdescVar;
        public readonly short wVarFlags;
        public readonly int varkind;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary> Per instance discriminant </summary>
        public const int VAR_PERINSTANCE = 0;
        /// <summary> Static discriminant </summary>
        public const int VAR_STATIC = 1;
        /// <summary> Const discriminant </summary>
        public const int VAR_CONST = 2;
        /// <summary> Dispatch discriminant </summary>
        public const int VAR_DISPATCH = 3;

        /// <summary>
        /// Create description
        /// </summary>
        /// <param name="values"></param>
        internal VarDesc(JIPointer values) :
            this(values.IsNull ? null : (JIStruct)values.Referent) {
        }

        /// <summary>
        /// Create description
        /// </summary>
        /// <param name="filledStruct"></param>
        internal VarDesc(JIStruct filledStruct) {
            if (filledStruct == null) {
                memberId = -1;
                lpstrSchema = null;
                u = null;
                elemdescVar = null;
                wVarFlags = -1;
                varkind = -1;
                return;
            }
            memberId = (int)filledStruct.GetMember(0);
            lpstrSchema = (JIPointer)filledStruct.GetMember(1);
            u = (JIUnion)filledStruct.GetMember(2);
            elemdescVar = new ElemDesc((JIStruct)filledStruct.GetMember(3));
            wVarFlags = (short)filledStruct.GetMember(4);
            varkind = (int)filledStruct.GetMember(5);
        }
    }
}