// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom.Rpc.pdu;

namespace Opc.Classic.Dcom.Rpc;

/// <inheritdoc/>
public class BindException : RpcException
{
    /// <inheritdoc/>
    public BindException()
    {
    }

    /// <inheritdoc/>
    public BindException(string message) :
        base(message)
    {
    }

    /// <inheritdoc/>
    public BindException(string message, BindNoAcknowledgeReason rejectReason) :
        base(ToString(message, rejectReason))
    {
        RejectReason = rejectReason;
    }

    /// <summary>
    /// Gets the DCE/RPC <c>bind_nak</c> reject reason, when the bind was rejected with a
    /// BIND_NAK PDU. <see langword="null" /> for other bind failures.
    /// </summary>
    public BindNoAcknowledgeReason? RejectReason { get; }

    public BindException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public BindException(string? message, int hresult) : base(message, hresult)
    {
    }

    /// <summary>
    /// Combine to string
    /// </summary>
    /// <param name="message">Human-readable description of the failure condition.</param>
    /// <param name="reason">Diagnostic reason associated with the RPC or COM failure.</param>
    /// <returns>Returns a human-readable representation suitable for diagnostic logging.</returns>
    private static string ToString(string message,
        BindNoAcknowledgeReason reason) => !string.IsNullOrEmpty(message) ?
            message + " (" + ToString(reason) + ")" : ToString(reason);

    /// <summary>
    /// TODO Move to extension
    /// </summary>
    /// <param name="reason">Diagnostic reason associated with the RPC or COM failure.</param>
    /// <returns>Returns a human-readable representation suitable for diagnostic logging.</returns>
    private static string ToString(BindNoAcknowledgeReason reason)
    {
        switch (reason)
        {
            case BindNoAcknowledgeReason.REASON_NOT_SPECIFIED:
                return "REASON_NOT_SPECIFIED";
            case BindNoAcknowledgeReason.TEMPORARY_CONGESTION:
                return "TEMPORARY_CONGESTION";
            case BindNoAcknowledgeReason.LOCAL_LIMIT_EXCEEDED:
                return "LOCAL_LIMIT_EXCEEDED";
            case BindNoAcknowledgeReason.CALLED_PADDR_UNKNOWN:
                return "CALLED_PADDR_UNKNOWN";
            case BindNoAcknowledgeReason.PROTOCOL_VERSION_NOT_SUPPORTED:
                return "PROTOCOL_VERSION_NOT_SUPPORTED";
            case BindNoAcknowledgeReason.DEFAULT_CONTEXT_NOT_SUPPORTED:
                return "DEFAULT_CONTEXT_NOT_SUPPORTED";
            case BindNoAcknowledgeReason.USER_DATA_NOT_READABLE:
                return "USER_DATA_NOT_READABLE";
            case BindNoAcknowledgeReason.NO_PSAP_AVAILABLE:
                return "NO_PSAP_AVAILABLE";
            default:
                return "unknown";
        }
    }
}
