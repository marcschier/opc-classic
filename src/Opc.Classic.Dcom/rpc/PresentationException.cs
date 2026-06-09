// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Rpc.Core;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Presentation exception
/// </summary>
public class PresentationException : BindException {

    /// <summary>
    /// Create default
    /// </summary>
    public PresentationException() {
    }

    /// <summary>
    /// Create
    /// </summary>
    /// <param name="message"></param>
    public PresentationException(string message) :
        base(message) {
    }

    /// <summary>
    /// Create presentation exception
    /// </summary>
    /// <param name="message"></param>
    /// <param name="result"></param>
    public PresentationException(string message, PresentationResult result) :
        base(ToString(message, result)) {
    }

    /// <summary>
    /// Create message
    /// </summary>
    /// <param name="message"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    private static string ToString(string message, PresentationResult result) {
        if (result == null) {
            return message;
        }
        return !string.IsNullOrEmpty(message) ? message +
            " (" + result + ")" : result.ToString();
    }
}
