// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;

namespace Opc.Classic.Dcom.Automation;

/// <summary>
/// Class for signifying Automation related exceptions.
/// </summary>
public sealed class AutomationException : InteropException {

    /// <summary>
    /// Create exception
    /// </summary>
    /// <param name="e"></param>
    public AutomationException(InteropException e) :
        base(e.ErrorCode, e.Message, e.InnerException) {
    }

    /// <summary>
    /// Exception information
    /// </summary>
    internal ExcepInfo ExcepInfo {
        set {
            _excepInfo.ErrorCode = value.ErrorCode;
            _excepInfo.ExcepDesc = value.ExcepDesc;
            _excepInfo.HelpFilePath = value.HelpFilePath;
            _excepInfo.ExcepSource = value.ExcepSource;
        }
        get => _excepInfo;
    }

    private readonly ExcepInfo _excepInfo = new ExcepInfo();
}
