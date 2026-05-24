// SPDX-License-Identifier: MIT

using System;

namespace Opc.Classic.Dcom.Automation; 
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
