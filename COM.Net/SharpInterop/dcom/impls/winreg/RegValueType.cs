//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.winreg {

    /// <summary>
    /// Registry value types
    /// </summary>
    public enum RegValueType {

        /// <summary>
        /// No type (the stored value, if any)
        /// </summary>
        REG_NONE = 0,

        /// <summary>
        /// A string value, normally stored and exposed in UTF-16LE
        /// </summary>
        REG_SZ,

        /// <summary>
        /// An "expandable" string value that can contain environment variables
        /// </summary>
        REG_EXPAND_SZ,

        /// <summary>
        /// Binary data in any form.
        /// </summary>
        REG_BINARY,

        /// <summary>
        /// A 32-bit number.
        /// </summary>
        REG_DWORD,

        /// <summary>
        /// A 32-bit number in LE.
        /// </summary>
        REG_DWORD_LITTLE_ENDIAN = REG_DWORD,

        /// <summary>
        /// A 32-bit number in BE.
        /// </summary>
        REG_DWORD_BIG_ENDIAN,

        /// <summary>
        /// A symbolic link (UNICODE) to another registry key
        /// </summary>
        REG_LINK,

        /// <summary>
        /// A multi-string value, which is an ordered list of non-empty
        /// strings, normally stored and exposed in UTF-16LE
        /// </summary>
        REG_MULTI_SZ = 7,

        /// <summary>
        /// A QWORD value, a 64-bit integer
        /// </summary>
        REG_QWORD = 11,

        /// <summary>
        /// A QWORD value, a 64-bit integer in LE
        /// </summary>
        REG_QWORD_LITTLE_ENDIAN = REG_QWORD,
    }

}