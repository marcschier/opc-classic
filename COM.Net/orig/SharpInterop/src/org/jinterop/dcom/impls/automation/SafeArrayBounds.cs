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

    using JIStruct = org.jinterop.dcom.core.JIStruct;

    /// <summary>
    /// Implements the <i>SAFEARRAYBOUNDS</i> structure of COM Automation.
    /// 
    /// @since 1.0
    /// 
    /// </summary>
    [Serializable]
    public sealed class SafeArrayBounds {

        private const long SerialVersionUID = -3110688445129575984L;
        public readonly int CElements;
        public readonly int LLbound;

        public SafeArrayBounds(JIStruct values) {
            if (values == null) {
                CElements = -1;
                LLbound = -1;
                return;
            }
            CElements = (int)((int?)values.GetMember(0));
            LLbound = (int)((int?)values.GetMember(0));
        }
    }

}