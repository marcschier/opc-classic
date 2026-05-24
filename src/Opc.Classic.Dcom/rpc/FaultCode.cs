// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Rpc; 
/// <summary>
/// Fault codes
/// </summary>
public enum FaultCode {

    /// <summary>
    /// Unknown fault
    /// </summary>
    UNKNOWN = -1,

    /// <summary>
    /// Fault status indicating the server does not support the RPC protocol
    /// version specified in the request.
    /// </summary>
    RPC_VERSION_MISMATCH = 0x1c000008,

    /// <summary>
    /// Fault status indicating the server is rejecting the request for
    /// unspecified reasons.
    /// </summary>
    UNSPECIFIED_REJECTION = 0x1c000009,

    /// <summary>
    /// Connectionless fault status indicating the server has no state
    /// corresponding to the specified activity identifier.
    /// </summary>
    BAD_ACTIVITY_ID = 0x1c00000a,

    /// <summary>
    /// Connectionless fault status indicating the conversation manager
    /// callback failed.
    /// </summary>
    WHO_ARE_YOU_FAILED = 0x1c00000b,

    /// <summary>
    /// Fault status indicating the server manager routine has not been
    /// entered and executed.
    /// </summary>
    MANAGER_NOT_ENTERED = 0x1c00000c,

    /// <summary>
    /// Fault status indicating the requested operation number is out of
    /// range.
    /// </summary>
    OPERATION_RANGE_ERROR = 0x1c010002,

    /// <summary>
    /// Fault status indicating the server does not export the interface
    /// requested by the client.
    /// </summary>
    UNKNOWN_INTERFACE = 0x1c010003,

    /// <summary>
    /// Connectionless fault status indicating the specified boot time does
    /// not match the actual server boot time.
    /// </summary>
    WRONG_BOOT_TIME = 0x1c010006,

    /// <summary>
    /// Connectionless fault status indicating a restarted server called
    /// back a client.
    /// </summary>
    YOU_CRASHED = 0x1c010009,

    /// <summary>
    /// Fault status indicating a protocol violation.
    /// </summary>
    PROTOCOL_ERROR = 0x1c01000b,

    /// <summary>
    /// Fault status indicating the operation's output parameters are larger
    /// than their declared maximum size.
    /// </summary>
    OUTPUT_ARGUMENTS_TOO_BIG = 0x1c010013,

    /// <summary>
    /// Fault status indicating the server is currently too busy to service
    /// the request.
    /// </summary>
    SERVER_TOO_BUSY = 0x1c010014,

    /// <summary>
    /// Fault status indicating the server does not implement the requested
    /// operation for the requested object's type.
    /// </summary>
    UNSUPPORTED_TYPE = 0x1c010017,

    /// <summary>
    /// Connection-oriented fault status indicating the requested presentation
    /// context ID is invalid.
    /// </summary>
    INVALID_PRESENTATION_CONTEXT_ID = 0x1c00001c,

    /// <summary>
    /// Fault status indicating the server does not support the authentication
    /// level requested.
    /// </summary>
    UNSUPPORTED_AUTHENTICATION_LEVEL = 0x1c00001d,

    /// <summary>
    /// Fault status indicating an invalid checksum.
    /// </summary>
    INVALID_CHECKSUM = 0x1c00001f,

    /// <summary>
    /// Fault status indicating an invalid CRC.
    /// </summary>
    INVALID_CRC = 0x1c000020,
}
