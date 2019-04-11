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
    /// Implements the <i>PARAMDESC</i> structure of COM Automation. Contains
    /// information needed for transferring a structure element, parameter,
    /// or function return value between processes.
    /// </summary>
    [Serializable]
    public sealed class ParamDesc
    {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable IDE1006 // Naming Styles
        public readonly JIPointer lpVarValue;
        public readonly ParamFlag wPARAMFlags;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Create param description
        /// </summary>
        /// <param name="values"></param>
        internal ParamDesc(JIStruct values)
        {
            if (values == null)
            {
                lpVarValue = null;
                wPARAMFlags = (ParamFlag)(-1);
                return;
            }

            lpVarValue = (JIPointer)values.GetMember(0);
            wPARAMFlags = (ParamFlag)values.GetMember(1);
        }
    }
}