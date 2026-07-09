// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Implements the <i>CALLCONV</i> data type of COM Automation.
/// Identifies the calling convention used by a member function.
/// </summary>
public enum CallConv
{
    /// <summary>
    /// Fast call
    /// </summary>
    CC_FASTCALL = 0,

    /// <summary>
    /// Indicates that the Cdecl calling convention is used for a method.
    /// </summary>
    CC_CDECL = 1,

    /// <summary>
    /// Indicates that the Mscpascal calling convention is used for a method.
    /// </summary>
    CC_MSCPASCAL,

    /// <summary>
    /// Indicates that the Pascal calling convention is used for a method.
    /// </summary>
    CC_PASCAL = CC_MSCPASCAL,

    /// <summary>
    /// Indicates that the Macpascal calling convention is used for a method.
    /// </summary>
    CC_MACPASCAL,

    /// <summary>
    /// Indicates that the Stdcall calling convention is used for a method.
    /// </summary>
    CC_STDCALL,

    /// <summary>
    /// FP fast call
    /// </summary>
    CC_FPFASTCALL,

    /// <summary>
    /// Indicates that the Syscall calling convention is used for a method.
    /// </summary>
    CC_SYSCALL,

    /// <summary>
    /// Indicates that the Mpwcdecl calling convention is used for a method.
    /// </summary>
    CC_MPWCDECL,

    /// <summary>
    /// Indicates that the Mpwpascal calling convention is used for a method.
    /// </summary>
    CC_MPWPASCAL,

    /// <summary>
    /// Indicates the end of the CALLCONV enumeration.
    /// </summary>
    CC_MAX
}
