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
    /// Implements the <i>TYPEDESC</i> structure of COM Automation and
    /// describes the type of a variable, the return type of a function,
    /// or the type of a function parameter.
    /// </summary>
    [Serializable]
    public sealed class TypeDesc {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
        public readonly JIPointer typeDesc;
        public readonly JIPointer arrayDesc;
        public readonly int hrefType;
        public readonly short vt;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


        /// <summary> pointer </summary>
        public static readonly short VT_PTR = 0x1a;
        /// <summary> safe array </summary>
        public static readonly short VT_SAFEARRAY = 0x1b;
        /// <summary> c-style array </summary>
        public static readonly short VT_CARRAY = 0x1c;
        /// <summary> user </summary>
        public static readonly short VT_USERDEFINED = 0x1d;

        /// <summary>
        /// Create description
        /// </summary>
        /// <param name="values"></param>
        internal TypeDesc(JIPointer values) :
            this(values.IsNull ? null : (JIStruct)values.GetReferent()) {
        }

        /// <summary>
        /// Create type description
        /// </summary>
        /// <param name="values"></param>
        internal TypeDesc(JIStruct values) {
            if (values == null) {
                typeDesc = null;
                arrayDesc = null;
                hrefType = -1;
                vt = -1;
                return;
            }

            vt = (short)values.GetMember(1);
            var union = (JIUnion)values.GetMember(0);
            if (vt.Equals(VT_PTR) || vt.Equals(VT_SAFEARRAY)) {
                var pointer = (JIPointer)union.Members[VT_PTR];
                if (pointer == null) {
                    pointer = (JIPointer)union.Members[VT_SAFEARRAY];
                }
                typeDesc = new JIPointer(new TypeDesc(pointer), false);
                arrayDesc = null;
                hrefType = -1;
            }
            else if (vt.Equals(VT_CARRAY)) {
                hrefType = -1;
                typeDesc = null;
                var pointer = (JIPointer)union.Members[VT_CARRAY];
                arrayDesc = new JIPointer(new ArrayDesc(pointer));
            }
            else if (vt.Equals(VT_USERDEFINED)) {
                typeDesc = null;
                arrayDesc = null;
                hrefType = (int)union.Members[VT_USERDEFINED];
            }
            else {
                typeDesc = null;
                arrayDesc = null;
                hrefType = -1;
            }
        }
    }
}