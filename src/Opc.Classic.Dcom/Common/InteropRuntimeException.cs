// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Core;
using System;

namespace Opc.Classic.Dcom.Common;

/// <summary>
/// Framework Internal class.
/// </summary>
/// <remarks>Internally used class from <see cref="CallBuilder"/>,
/// since the read(), write() do not throw exceptions. 
/// The <see cref="IComObject"/> call or QI or any other APIs
/// will always throw checked <see cref="InteropException"/>
/// </remarks>
public sealed class InteropRuntimeException : Exception
{

    /// <summary>
    /// Create exception
    /// </summary>
    /// <param name="hresult"></param>
    public InteropRuntimeException(int hresult) =>
        HResult = hresult;

    /// <summary>
    /// Create exception
    /// </summary>
    /// <param name="hresult"></param>
    public InteropRuntimeException(ErrorCode hresult) =>
        HResult = (int)hresult;

    /// <summary>
    /// Create exception
    /// </summary>
    /// <param name="hresult"></param>
    /// <param name="parameters"></param>
    public InteropRuntimeException(int hresult, params object[] parameters) :
        this(hresult) => Parameters = parameters;

    /// <summary>
    /// Params
    /// </summary>
    public object[] Parameters { get; }

    /// <summary>
    /// Get message
    /// </summary>
    public override string Message =>
        Interop.GetLocalizedMessage((ErrorCode)HResult);
}
