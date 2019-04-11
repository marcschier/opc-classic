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
    /// Implements the <i>TYPEKIND</i> structure of COM Automation
    /// 
    /// @since 2.0 (formerly TYPEKIND)
    /// 
    /// </summary>
    public interface TypeKind {

        /// <summary>
        /// A set of enumerators.
        /// </summary>
        /// <summary>
        /// A structure with no methods.
        /// </summary>
        /// <summary>
        /// A module that can only have static functions and data (for example, a DLL).
        /// </summary>
        /// <summary>
        /// A type that has virtual and pure functions.
        /// </summary>
        /// <summary>
        /// A set of methods and properties that are accessible through IDispatch::Invoke.
        /// By default, dual interfaces return TKIND_DISPATCH.
        /// </summary>
        /// <summary>
        /// A set of implemented component object interfaces.
        /// </summary>
        /// <summary>
        /// A type that is an alias for another type.
        /// </summary>
        /// <summary>
        /// A union, all of whose members have an offset of zero.
        /// </summary>
        /// <summary>
        /// End of ENUM marker.
        /// </summary>

    }

    public static class TypeKind_Fields {
        public static readonly int? TKIND_ENUM = new int?(0);
        public static readonly int? TKIND_RECORD = new int?(1);
        public static readonly int? TKIND_MODULE = new int?(2);
        public static readonly int? TKIND_INTERFACE = new int?(3);
        public static readonly int? TKIND_DISPATCH = new int?(4);
        public static readonly int? TKIND_COCLASS = new int?(5);
        public static readonly int? TKIND_ALIAS = new int?(6);
        public static readonly int? TKIND_UNION = new int?(7);
        public static readonly int? TKIND_MAX = new int?(8);
    }

}