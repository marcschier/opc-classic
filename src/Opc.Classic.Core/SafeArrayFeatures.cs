// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic;

/// <summary>
/// Feature flags carried by the OLE Automation <c>SAFEARRAY</c> descriptor's
/// <c>fFeatures</c> field as defined by [MS-OAUT] §2.2.30.6.
/// </summary>
/// <remarks>
/// The flags describe how the descriptor and elements are owned or typed. OPC
/// payloads most commonly use <see cref="HaveVartype"/> and, for heterogeneous
/// arrays, <see cref="Variant"/>. Ownership flags such as <see cref="Auto"/>,
/// <see cref="Static"/>, <see cref="Embedded"/>, and <see cref="FixedSize"/>
/// are preserved for round-tripping but do not change managed ownership.
/// </remarks>
[Flags]
public enum SafeArrayFeatures : ushort
{
    /// <summary>
    /// No SAFEARRAY feature flags are set.
    /// </summary>
    None = 0x0000,

    /// <summary>
    /// FADF_AUTO: array memory is stack-owned by the producer.
    /// </summary>
    Auto = 0x0001,

    /// <summary>
    /// FADF_STATIC: array memory is statically allocated by the producer.
    /// </summary>
    Static = 0x0002,

    /// <summary>
    /// FADF_EMBEDDED: array is embedded in another structure.
    /// </summary>
    Embedded = 0x0004,

    /// <summary>
    /// FADF_FIXEDSIZE: array cannot be resized.
    /// </summary>
    FixedSize = 0x0010,

    /// <summary>
    /// FADF_RECORD: elements are VT_RECORD user-defined records.
    /// </summary>
    Record = 0x0020,

    /// <summary>
    /// FADF_HAVEIID: descriptor carries an IID for interface-pointer elements.
    /// </summary>
    HaveIID = 0x0040,

    /// <summary>
    /// FADF_HAVEVARTYPE: descriptor carries a VARTYPE for all elements.
    /// </summary>
    HaveVartype = 0x0080,

    /// <summary>
    /// FADF_BSTR: elements are BSTR values.
    /// </summary>
    Bstr = 0x0100,

    /// <summary>
    /// FADF_UNKNOWN: elements are IUnknown pointers.
    /// </summary>
    Unknown = 0x0200,

    /// <summary>
    /// FADF_DISPATCH: elements are IDispatch pointers.
    /// </summary>
    Dispatch = 0x0400,

    /// <summary>
    /// FADF_VARIANT: elements are VARIANT values.
    /// </summary>
    Variant = 0x0800,
}
