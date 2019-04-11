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

    /// <summary>
    /// Idl flag
    /// </summary>
    [Flags]
    public enum IdlFlag : short {

        /// <summary>
        /// None
        /// </summary>
        IDLFLAG_NONE = 0x0,

        /// <summary>
        /// Find
        /// </summary>
        IDLFLAG_FIN = 0x1,

        /// <summary>
        /// Fout
        /// </summary>
        IDLFLAG_FOUT = 0x2,

        /// <summary>
        /// locale
        /// </summary>
        IDLFLAG_FLCID = 0x4,

        /// <summary>
        /// Retval
        /// </summary>
        IDLFLAG_FRETVAL = 0x8
    }
}