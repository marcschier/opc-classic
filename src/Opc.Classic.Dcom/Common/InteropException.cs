// SPDX-License-Identifier: MIT

using System;

namespace Opc.Classic.Dcom.Common;

/// <summary>
/// Exception class for the framework. Developers are expected to catch
/// or re-throw these exceptions and not create one themselves.
/// </summary>
public class InteropException : Exception {

    /// <summary>
    /// Create exception
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="message"></param>
    public InteropException(int errorCode, string message) :
        this(errorCode, message, null) {
    }

    /// <summary>
    /// Create exception
    /// </summary>
    /// <param name="errorCode"></param>
    /// <param name="message"></param>
    public InteropException(ErrorCode errorCode, string message) :
        this(errorCode, message, null) {
    }

    /// <summary>
    /// Create exception
    /// </summary>
    /// <param name="errorCode"></param>
    public InteropException(int errorCode) :
        this(errorCode, (Exception)null) {
    }

    /// <summary>
    /// Create exception
    /// </summary>
    /// <param name="errorCode"></param>
    public InteropException(ErrorCode errorCode) :
        this(errorCode, (Exception)null) {
    }

    /// <summary>
    /// Create exception
    /// </summary>
    public InteropException(int errorCode, Exception cause) :
        this(errorCode, null, cause) {
    }

    /// <summary>
    /// Create exception
    /// </summary>
    public InteropException(ErrorCode errorCode, Exception cause) :
        this(errorCode, null, cause) {
    }

    /// <summary>
    /// Create exception
    /// </summary>
    public InteropException(InteropRuntimeException exception) :
        this(exception.HResult, null, exception) {
    }

    /// <summary>
    /// Create exception
    /// </summary>
    public InteropException(int errorCode, string message, Exception cause) :
        base(message, cause) {
        ErrorCode = (ErrorCode)errorCode;
        _message = message;
    }

    /// <summary>
    /// Create exception
    /// </summary>
    public InteropException(ErrorCode errorCode, string message, Exception cause) :
        base(message, cause) {
        ErrorCode = errorCode;
        _message = message;
    }

    /// <summary>
    /// Returns the localized error messages.
    /// </summary>
    public override string Message =>
        _message ?? (_message = Interop.GetLocalizedMessage(ErrorCode));

    /// <summary>
    /// Returns the error code associated with this exception. 
    /// </summary>
    public ErrorCode ErrorCode { get; } = ErrorCode.UNDEFINED;

    private string _message;
}
