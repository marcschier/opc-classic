// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Implements the <i>TYPEKIND</i> structure of COM Automation
/// </summary>
public enum TypeKind
{
    /// <summary>
    /// A set of enumerators.
    /// </summary>
    TKIND_ENUM = 0,

    /// <summary>
    /// A structure with no methods.
    /// </summary>
    TKIND_RECORD,

    /// <summary>
    /// A module that can only have static functions and data (for example, a DLL).
    /// </summary>
    TKIND_MODULE,

    /// <summary>
    /// A type that has virtual and pure functions.
    /// </summary>
    TKIND_INTERFACE,

    /// <summary>
    /// A set of methods and properties that are accessible through IDispatch::Invoke.
    /// By default, dual interfaces return TKIND_DISPATCH.
    /// </summary>
    TKIND_DISPATCH,

    /// <summary>
    /// A set of implemented component object interfaces.
    /// </summary>
    TKIND_COCLASS,

    /// <summary>
    /// A type that is an alias for another type.
    /// </summary>
    TKIND_ALIAS,

    /// <summary>
    /// A union, all of whose members have an offset of zero.
    /// </summary>
    TKIND_UNION,

    /// <summary>
    /// End of ENUM marker.
    /// </summary>
    TKIND_MAX
}
