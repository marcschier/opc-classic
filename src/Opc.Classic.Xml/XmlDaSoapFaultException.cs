//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Xml;

/// <summary>
/// Exception thrown when an OPC XML-DA SOAP response contains a SOAP Fault.
/// </summary>
public sealed class XmlDaSoapFaultException : Exception
{
    /// <summary>Creates an empty SOAP fault exception.</summary>
    public XmlDaSoapFaultException()
    {
    }

    /// <summary>Creates a SOAP fault exception with a custom message.</summary>
    public XmlDaSoapFaultException(string? message)
        : base(message)
    {
    }

    /// <summary>Creates a SOAP fault exception with a custom message and inner exception.</summary>
    public XmlDaSoapFaultException(string? message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Creates a SOAP fault exception from the decoded SOAP fault fields.</summary>
    public XmlDaSoapFaultException(string? faultCode, string? faultString, XmlDaErrorCode errorCode)
        : base(BuildMessage(faultCode, faultString, errorCode))
    {
        FaultCode = faultCode ?? string.Empty;
        FaultString = faultString ?? string.Empty;
        ErrorCode = errorCode;
    }

    /// <summary>The raw SOAP <c>faultcode</c> text.</summary>
    public string FaultCode { get; } = string.Empty;

    /// <summary>The raw SOAP <c>faultstring</c> text.</summary>
    public string FaultString { get; } = string.Empty;

    /// <summary>The typed XML-DA error code parsed from <see cref="FaultCode"/>.</summary>
    public XmlDaErrorCode ErrorCode { get; } = XmlDaErrorCode.Unknown;

    private static string BuildMessage(string? faultCode, string? faultString, XmlDaErrorCode errorCode)
    {
        string codeText = string.IsNullOrWhiteSpace(faultCode) ? "unknown" : faultCode;
        string messageText = string.IsNullOrWhiteSpace(faultString) ? "SOAP Fault" : faultString;
        return $"SOAP Fault {codeText} ({errorCode}): {messageText}";
    }
}
