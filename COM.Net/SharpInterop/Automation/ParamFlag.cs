//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Automation {
    using System;

    /// <summary>
    /// Param flags
    /// </summary>
    [Flags]
    public enum ParamFlag : short {

        /// <summary>
        /// None
        /// </summary>
        PARAMFLAG_NONE = 0x00,

        /// <summary>
        /// In
        /// </summary>
        PARAMFLAG_FIN = 0x01,

        /// <summary>
        /// Out
        /// </summary>
        PARAMFLAG_FOUT = 0x02,

        /// <summary>
        /// Lcid
        /// </summary>
        PARAMFLAG_FLCID = 0x04,

        /// <summary>
        /// Return
        /// </summary>
        PARAMFLAG_FRETVAL = 0x08,

        /// <summary>
        /// Optional
        /// </summary>
        PARAMFLAG_FOPT = 0x10,

        /// <summary>
        /// Default
        /// </summary>
        PARAMFLAG_FHASDEFAULT = 0x20,

        /// <summary>
        /// Custom data
        /// </summary>
        PARAMFLAG_FHASCUSTDATA = 0x40
    }
}