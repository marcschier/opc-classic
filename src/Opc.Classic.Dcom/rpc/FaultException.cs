// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Fault
/// </summary>
public class FaultException : RpcException
{
    /// <summary>
    /// Fault code
    /// </summary>
    public FaultCode Code { get; }

    /// <summary>
    /// Stub
    /// </summary>
    public byte[] Stub { get; }

    /// <summary>
    /// Create fault
    /// </summary>
    public FaultException() => Stub = null;

    /// <summary>
    /// Create fault
    /// </summary>
    /// <param name="message">Human-readable description of the failure condition.</param>
    public FaultException(string message) : base(message)
    {
        Stub = null;
        Code = FaultCode.UNKNOWN;
    }

    /// <summary>
    /// Create exception
    /// </summary>
    /// <param name="message">Human-readable description of the failure condition.</param>
    /// <param name="status">Status code reported by the RPC or COM operation.</param>
    public FaultException(string message, FaultCode status) :
        base(ToString(message, status))
    {
        Code = status;
        Stub = null;
    }

    /// <summary>
    /// Crate exception
    /// </summary>
    /// <param name="message">Human-readable description of the failure condition.</param>
    /// <param name="status">Status code reported by the RPC or COM operation.</param>
    /// <param name="stub">Wire-format bytes consumed or produced by the operation.</param>
    public FaultException(string message, FaultCode status, byte[] stub) :
        base(ToString(message, status))
    {
        Code = status;
        Stub = stub;
    }

    public FaultException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public FaultException(string? message, int hresult) : base(message, hresult)
    {
    }

    /// <summary>
    /// Convert to string
    /// </summary>
    /// <param name="message">Human-readable description of the failure condition.</param>
    /// <param name="status">Status code reported by the RPC or COM operation.</param>
    /// <returns>Returns a human-readable representation suitable for diagnostic logging.</returns>
    private static string ToString(string message, FaultCode status) =>
        !string.IsNullOrEmpty(message) ? message +
            " (" + ToString(status) + ")" : ToString(status);

    /// <summary>
    /// TODO: As extension
    /// </summary>
    /// <param name="status">Status code reported by the RPC or COM operation.</param>
    /// <returns>Returns a human-readable representation suitable for diagnostic logging.</returns>
    private static string ToString(FaultCode status)
    {
        switch (status)
        {
            case FaultCode.RPC_VERSION_MISMATCH:
                return "RPC_VERSION_MISMATCH";
            case FaultCode.UNSPECIFIED_REJECTION:
                return "UNSPECIFIED_REJECTION";
            case FaultCode.BAD_ACTIVITY_ID:
                return "BAD_ACTIVITY_ID";
            case FaultCode.WHO_ARE_YOU_FAILED:
                return "WHO_ARE_YOU_FAILED";
            case FaultCode.MANAGER_NOT_ENTERED:
                return "MANAGER_NOT_ENTERED";
            case FaultCode.OPERATION_RANGE_ERROR:
                return "OPERATION_RANGE_ERROR";
            case FaultCode.UNKNOWN_INTERFACE:
                return "UNKNOWN_INTERFACE";
            case FaultCode.WRONG_BOOT_TIME:
                return "WRONG_BOOT_TIME";
            case FaultCode.YOU_CRASHED:
                return "YOU_CRASHED";
            case FaultCode.PROTOCOL_ERROR:
                return "PROTOCOL_ERROR";
            case FaultCode.OUTPUT_ARGUMENTS_TOO_BIG:
                return "OUTPUT_ARGUMENTS_TOO_BIG";
            case FaultCode.SERVER_TOO_BUSY:
                return "SERVER_TOO_BUSY";
            case FaultCode.UNSUPPORTED_TYPE:
                return "UNSUPPORTED_TYPE";
            case FaultCode.INVALID_PRESENTATION_CONTEXT_ID:
                return "INVALID_PRESENTATION_CONTEXT_ID";
            case FaultCode.UNSUPPORTED_AUTHENTICATION_LEVEL:
                return "UNSUPPORTED_AUTHENTICATION_LEVEL";
            case FaultCode.INVALID_CHECKSUM:
                return "INVALID_CHECKSUM";
            case FaultCode.INVALID_CRC:
                return "INVALID_CRC";
            default:
                return "unknown";
        }
    }
}
