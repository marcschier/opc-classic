//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace org.jinterop.winreg {
    using System;

    /// <summary>
    /// Key access
    /// </summary>
    [Flags]
    public enum RegKeyAccess {

        /// <summary>
        /// Query
        /// </summary>
        KEY_QUERY_VALUE = 0x00000001,

        /// <summary>
        /// Set value
        /// </summary>
        KEY_SET_VALUE = 0x00000002,

        /// <summary>
        /// Create subkey
        /// </summary>
        KEY_CREATE_SUB_KEY = 0x00000004,

        /// <summary>
        /// Enumerate subkey
        /// </summary>
        KEY_ENUMERATE_SUB_KEYS = 0x00000008,

        /// <summary>
        /// Notify
        /// </summary>
        KEY_NOTIFY = 0x00000010,

        /// <summary>
        /// Create link
        /// </summary>
        KEY_CREATE_LINK = 0x00000020,

        /// <summary>
        /// All access
        /// </summary>
        KEY_ALL_ACCESS = 0x000f003f,

        /// <summary>
        /// Execute
        /// </summary>
        KEY_EXECUTE = 0x00020019,

        /// <summary>
        /// Read
        /// </summary>
        KEY_READ = 0x00020019,

        /// <summary>
        /// Write
        /// </summary>
        KEY_WRITE = 0x00020006,

    }

}