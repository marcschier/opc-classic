// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Function flags
/// </summary>
[Flags]
public enum FuncFlag
{
    /// <summary>
    /// Restricted
    /// </summary>
    FUNCFLAG_FRESTRICTED = 0x1,

    /// <summary>
    /// Source
    /// </summary>
    FUNCFLAG_FSOURCE = 0x2,

    /// <summary>
    /// Bindable
    /// </summary>
    FUNCFLAG_FBINDABLE = 0x4,

    /// <summary>
    /// Edit
    /// </summary>
    FUNCFLAG_FREQUESTEDIT = 0x8,

    /// <summary>
    /// Display bind
    /// </summary>
    FUNCFLAG_FDISPLAYBIND = 0x10,

    /// <summary>
    /// Default bind
    /// </summary>
    FUNCFLAG_FDEFAULTBIND = 0x20,

    /// <summary>
    /// Hidden
    /// </summary>
    FUNCFLAG_FHIDDEN = 0x40,

    /// <summary>
    /// Get last error
    /// </summary>
    FUNCFLAG_FUSESGETLASTERROR = 0x80,

    /// <summary>
    /// Default collection element
    /// </summary>
    FUNCFLAG_FDEFAULTCOLLELEM = 0x100,

    /// <summary>
    /// Uid default
    /// </summary>
    FUNCFLAG_FUIDEFAULT = 0x200,

    /// <summary>
    /// Non browesable
    /// </summary>
    FUNCFLAG_FNONBROWSABLE = 0x400,

    /// <summary>
    /// Replaceable
    /// </summary>
    FUNCFLAG_FREPLACEABLE = 0x800,

    /// <summary>
    /// Immediate bind
    /// </summary>
    FUNCFLAG_FIMMEDIATEBIND = 0x1000,
}
