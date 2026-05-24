//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using SharpInterop.Core;
using System;

namespace SharpInterop.Common; 
/// <summary>
/// Framework Internal class.
/// </summary>
/// <remarks>Internally used class from <see cref="CallBuilder"/>,
/// since the read(), write() do not throw exceptions. 
/// The <see cref="IComObject"/> call or QI or any other APIs
/// will always throw checked <see cref="InteropException"/>
/// </remarks>
public sealed class InteropRuntimeException : Exception {

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
