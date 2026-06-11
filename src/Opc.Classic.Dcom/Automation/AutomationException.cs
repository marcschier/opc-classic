// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Class for signifying Automation related exceptions.
/// </summary>
public sealed class AutomationException : InteropException
{

    /// <summary>
    /// Create exception
    /// </summary>
    /// <param name="e"></param>
    public AutomationException(InteropException e) :
        base(e.ErrorCode, e.Message, e.InnerException)
    {
    }

    public AutomationException(int errorCode, string message) : base(errorCode, message)
    {
    }

    public AutomationException(ErrorCode errorCode, string message) : base(errorCode, message)
    {
    }

    public AutomationException(int errorCode) : base(errorCode)
    {
    }

    public AutomationException(ErrorCode errorCode) : base(errorCode)
    {
    }

    public AutomationException(int errorCode, Exception cause) : base(errorCode, cause)
    {
    }

    public AutomationException(ErrorCode errorCode, Exception cause) : base(errorCode, cause)
    {
    }

    public AutomationException(InteropRuntimeException exception) : base(exception)
    {
    }

    public AutomationException(int errorCode, string message, Exception cause) : base(errorCode, message, cause)
    {
    }

    public AutomationException(ErrorCode errorCode, string message, Exception cause) : base(errorCode, message, cause)
    {
    }

    public AutomationException() : base()
    {
    }

    public AutomationException(string? message) : base(message)
    {
    }

    public AutomationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// Exception information
    /// </summary>
    internal ExcepInfo ExcepInfo
    {
        set
        {
            _excepInfo.ErrorCode = value.ErrorCode;
            _excepInfo.ExcepDesc = value.ExcepDesc;
            _excepInfo.HelpFilePath = value.HelpFilePath;
            _excepInfo.ExcepSource = value.ExcepSource;
        }
        get => _excepInfo;
    }

    private readonly ExcepInfo _excepInfo = new ExcepInfo();
}
