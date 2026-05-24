//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using System;

namespace SharpInterop.Automation; 
/// <summary>
/// Implements the <i>IMPLTYPEFLAGS</i> structure of COM Automation.
/// </summary>
[Flags]
public enum ImplTypeFlags {

    /// <summary>
    /// The interface or dispinterface represents the default for the source or sink.
    /// </summary>
    IMPLTYPEFLAG_FDEFAULT = 0x1,

    /// <summary>
    /// This member of a coclass is called rather than implemented.
    /// </summary>
    IMPLTYPEFLAG_FSOURCE = 0x2,

    /// <summary>
    /// The member should not be displayed or programmable by users.
    /// </summary>
    IMPLTYPEFLAG_FRESTRICTED = 0x4,

    /// <summary>
    /// Sinks receive events through the VTBL.
    /// </summary>
    IMPLTYPEFLAG_FDEFAULTVTABLE = 0x800,
}
