// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

#pragma warning disable CA1707 // OPC HRESULT constants intentionally preserve opcerror.h names.
#pragma warning disable IDE1006

namespace Opc.Classic.Cpx;

/// <summary>
/// OPC Complex Data HRESULT constants defined by CPX 1.00 §9 and OPC <c>opcerror.h</c>.
/// </summary>
public static class OpcComplexDataResult
{
    /// <summary>
    /// XML-DA namespace for CPX-specific errors.
    /// </summary>
    public const string XmlDaNamespace = "http://opcfoundation.org/ComplexData/1.0/";

    /// <summary>
    /// The dictionary and/or type description for the item has changed.
    /// </summary>
    public const int OPCCPX_E_TYPE_CHANGED = unchecked((int)0xC0040407);

    /// <summary>
    /// A data filter item with the specified name already exists.
    /// </summary>
    public const int OPCCPX_E_FILTER_DUPLICATE = unchecked((int)0xC0040408);

    /// <summary>
    /// The data filter value does not conform to the server's syntax.
    /// </summary>
    public const int OPCCPX_E_FILTER_INVALID = unchecked((int)0xC0040409);

    /// <summary>
    /// An error occurred when the filter value was applied to the source data.
    /// </summary>
    public const int OPCCPX_E_FILTER_ERROR = unchecked((int)0xC004040A);

    /// <summary>
    /// The item value is empty because the data filter has excluded all fields.
    /// </summary>
    public const int OPCCPX_S_FILTER_NO_DATA = 0x0004040B;

    /// <summary>
    /// Alias for <see cref="OPCCPX_E_TYPE_CHANGED"/>.
    /// </summary>
    public const int E_TYPE_CHANGED = OPCCPX_E_TYPE_CHANGED;

    /// <summary>
    /// Alias for <see cref="OPCCPX_E_FILTER_DUPLICATE"/>.
    /// </summary>
    public const int E_FILTER_DUPLICATE = OPCCPX_E_FILTER_DUPLICATE;

    /// <summary>
    /// Alias for <see cref="OPCCPX_E_FILTER_INVALID"/>.
    /// </summary>
    public const int E_FILTER_INVALID = OPCCPX_E_FILTER_INVALID;

    /// <summary>
    /// Alias for <see cref="OPCCPX_E_FILTER_ERROR"/>.
    /// </summary>
    public const int E_FILTER_ERROR = OPCCPX_E_FILTER_ERROR;

    /// <summary>
    /// Alias for <see cref="OPCCPX_S_FILTER_NO_DATA"/>.
    /// </summary>
    public const int S_FILTER_NO_DATA = OPCCPX_S_FILTER_NO_DATA;
}
