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
    /// Implements the <i>ARRAYDESC</i> structure of COM Automation.
    /// Definition from MSDN: <i> Contained within the TYPEDESC, which describes the
    /// type of the array's elements, and information about the array's dimensions.
    /// </i>
    /// </summary>
    [Serializable]
    public sealed class ArrayDesc {

        /// <summary>
        /// Element Type.
        /// </summary>
        public readonly TypeDesc typeDesc;
        /// <summary>
        /// Dimension Count.
        /// </summary>
        public readonly short cDims;
        /// <summary>
        /// Variable length array containing one element for each dimension.
        /// </summary>
        public readonly SafeArrayBounds[] safeArrayBounds;

        internal ArrayDesc(JIStruct values) {
            if (values == null) {
                typeDesc = null;
                cDims = -1;
                safeArrayBounds = null;
                return;
            }

            typeDesc = new TypeDesc((JIStruct)values.GetMember(0));
            cDims = (short)(short?)values.GetMember(1);
            var arry = (JIArray)values.GetMember(2);
            var arry2 = (object[])arry.ArrayInstance;

            if (arry2 != null) {
                safeArrayBounds = new SafeArrayBounds[arry2.Length];
                for (var i = 0; i < arry2.Length; i++) {
                    safeArrayBounds[i] = new SafeArrayBounds((JIStruct)arry2[i]);
                }
            }
            else {
                safeArrayBounds = null;
            }
        }

        internal ArrayDesc(JIPointer values) : this(values.Null ? null : (JIStruct)values.Referent) {
        }
    }

}