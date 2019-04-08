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
    /// Implements the <i>INVOKEKIND</i> structure of COM Automation.
    /// </summary>
    public enum InvokeKind {

        /// <summary>
        ///  The member is called using a normal function invocation syntax
        /// </summary>
        INVOKE_FUNC = 1,

        /// <summary>
        /// The function is invoked using a normal property-access syntax.
        /// </summary>
        INVOKE_PROPERTYGET = 2,

        /// <summary>
        /// The function is invoked using a property value assignment syntax.
        /// Syntactically, a typical programming language might represent
        /// changing a property in the same way as assignment.
        /// For example:object.property : = value.
        /// </summary>
        INVOKE_PROPERTYPUT = 4,

        /// <summary>
        ///  The function is invoked using a property reference assignment syntax.
        /// </summary>
        INVOKE_PROPERTYPUTREF = 8,
    }
}