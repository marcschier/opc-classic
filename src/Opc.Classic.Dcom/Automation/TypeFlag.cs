// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Type flags
/// </summary>
[Flags]
public enum TypeFlag : short
{

    /// <summary>
    /// App object
    /// </summary>
    TYPEFLAG_FAPPOBJECT = 0x01,

    /// <summary>
    /// Create
    /// </summary>
    TYPEFLAG_FCANCREATE = 0x02,

    /// <summary>
    /// Licensed
    /// </summary>
    TYPEFLAG_FLICENSED = 0x04,

    /// <summary>
    /// predeclared
    /// </summary>
    TYPEFLAG_FPREDECLID = 0x08,

    /// <summary>
    /// Hidden
    /// </summary>
    TYPEFLAG_FHIDDEN = 0x10,

    /// <summary>
    /// Control
    /// </summary>
    TYPEFLAG_FCONTROL = 0x20,

    /// <summary>
    /// Dual
    /// </summary>
    TYPEFLAG_FDUAL = 0x40,

    /// <summary>
    /// Not extensible
    /// </summary>
    TYPEFLAG_FNONEXTENSIBLE = 0x80,

    /// <summary>
    /// Automation
    /// </summary>
    TYPEFLAG_FOLEAUTOMATION = 0x100,

    /// <summary>
    /// Restricted
    /// </summary>
    TYPEFLAG_FRESTRICTED = 0x200,

    /// <summary>
    /// Aggregatable
    /// </summary>
    TYPEFLAG_FAGGREGATABLE = 0x400,

    /// <summary>
    /// Replaceable
    /// </summary>
    TYPEFLAG_FREPLACEABLE = 0x800,

    /// <summary>
    /// Dispatch
    /// </summary>
    TYPEFLAG_FDISPATCHABLE = 0x1000,

    /// <summary>
    /// Reverse bind
    /// </summary>
    TYPEFLAG_FREVERSEBIND = 0x2000,
}
