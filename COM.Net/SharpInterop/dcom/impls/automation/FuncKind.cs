// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 

namespace org.jinterop.dcom.impls.automation {

    /// <summary>
    /// Implements the <i>FUNCKIND</i> structure of COM Automation.
    /// </summary>
    public enum FuncKind {

        /// <summary>
        /// The function is accessed the same as PUREVIRTUAL, 
        /// except the function has an implementation.
        /// </summary>
        FUNC_VIRTUAL = 0,

        /// <summary>
        /// The function is accessed through the virtual function 
        /// table (VTBL), and takes an implicit <i>this</i> pointer.
        /// </summary>
        FUNC_PUREVIRTUAL,

        /// <summary>
        /// The function is accessed by static address and takes 
        /// an implicit <i>this</i> pointer.
        /// </summary>
        FUNC_NONVIRTUAL,

        /// <summary>
        /// The function is accessed by static address and does 
        /// not take an implicit <i>this</i> pointer.
        /// </summary>
   		FUNC_STATIC,

        /// <summary>
        /// The function can be accessed only through IDispatch.
        /// </summary>
		FUNC_DISPATCH,
    }

}