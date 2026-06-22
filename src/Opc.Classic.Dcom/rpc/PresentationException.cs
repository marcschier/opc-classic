// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Opc.Classic.Dcom.Rpc.Core;

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Presentation exception
/// </summary>
public class PresentationException : BindException
{
    /// <summary>
    /// Create default
    /// </summary>
    public PresentationException()
    {
    }

    /// <summary>
    /// Create
    /// </summary>
    /// <param name="message">Human-readable description of the failure condition.</param>
    public PresentationException(string? message) :
        base(message)
    {
    }

    /// <summary>
    /// Create presentation exception
    /// </summary>
    /// <param name="message">Human-readable description of the failure condition.</param>
    /// <param name="result">Result value returned by the RPC or COM operation.</param>
    public PresentationException(string message, PresentationResult result) :
        base(ToString(message, result))
    {
    }

    public PresentationException(string message, pdu.BindNoAcknowledgeReason rejectReason) : base(message, rejectReason)
    {
    }

    /// <summary>
    /// Create with inner exception
    /// </summary>
    public PresentationException(string? message, Exception? innerException) :
        base(message, innerException)
    {
    }

    /// <summary>
    /// Create with HRESULT
    /// </summary>
    public PresentationException(string? message, int hresult) :
        base(message, hresult)
    {
    }

    /// <summary>
    /// Create message
    /// </summary>
    /// <param name="message">Human-readable description of the failure condition.</param>
    /// <param name="result">Result value returned by the RPC or COM operation.</param>
    /// <returns>Returns a human-readable representation suitable for diagnostic logging.</returns>
    private static string ToString(string message, PresentationResult result)
    {
        if (result == null)
        {
            return message;
        }
        return !string.IsNullOrEmpty(message) ? message +
            " (" + result + ")" : result.ToString();
    }
}
