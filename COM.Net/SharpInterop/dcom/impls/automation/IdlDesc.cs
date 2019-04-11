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
    /// IDL description
    /// </summary>
    [Serializable]
    public sealed class IdlDesc {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
        public readonly JIPointer dwReserved;
        public readonly IdlFlag wIDLFlags;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Create description
        /// </summary>
        /// <param name="values"></param>
        internal IdlDesc(JIStruct values) {
            if (values == null) {
                dwReserved = null;
                wIDLFlags = (IdlFlag)(-1);
                return;
            }
            dwReserved = (JIPointer)values.GetMember(0);
            wIDLFlags = (IdlFlag)values.GetMember(1);
        }
    }
}