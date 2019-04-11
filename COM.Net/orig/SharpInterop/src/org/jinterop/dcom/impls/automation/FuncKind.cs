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

    /// <summary>
    /// Implements the <i>FUNCKIND</i> structure of COM Automation.
    /// 
    /// @since 2.0 (formerly FUNCKIND)
    /// 
    /// </summary>
    public interface FuncKind {

        /// <summary>
        /// The function is accessed the same as PUREVIRTUAL, except the function has an implementation.
        /// </summary>
        /// <summary>
        /// The function is accessed through the virtual function table (VTBL), and takes an implicit <i>this</i> pointer.
        /// </summary>
        /// <summary>
        /// The function is accessed by static address and takes an implicit <i>this</i> pointer.
        /// </summary>
        /// <summary>
        /// The function is accessed by static address and does not take an implicit <i>this</i> pointer.
        /// </summary>
        /// <summary>
        /// The function can be accessed only through IDispatch.
        /// </summary>

    }

    public static class FuncKind_Fields {
        public static readonly int? FUNC_VIRTUAL = new int?(0);
        public static readonly int? FUNC_PUREVIRTUAL = new int?(1);
        public static readonly int? FUNC_NONVIRTUAL = new int?(2);
        public static readonly int? FUNC_STATIC = new int?(3);
        public static readonly int? FUNC_DISPATCH = new int?(4);
    }

}