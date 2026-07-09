// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Common;

namespace Opc.Classic.Dcom.Core;

/// <summary>
/// Scode type
/// </summary>
[Serializable]
public sealed class Scode
{
    /// <summary>
    /// Null value
    /// </summary>
    public static Scode Ok { get; } = new Scode(0);

    /// <summary>
    /// Error code
    /// </summary>
    public int ErrorCode { get; }

#pragma warning disable RECS0154 // Parameter is never used
    /// <summary>
    /// Create error code
    /// </summary>
    /// <param name="errorCode">Protocol or HRESULT error code reported by the operation.</param>
    public Scode(int errorCode) => ErrorCode = errorCode;
#pragma warning restore RECS0154 // Parameter is never used

    /// <summary>
    /// Create error code
    /// </summary>
    /// <param name="errorCode">Protocol or HRESULT error code reported by the operation.</param>
    public Scode(ErrorCode errorCode) :
        this((int)errorCode)
    {
    }
}
