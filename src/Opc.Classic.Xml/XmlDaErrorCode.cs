//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Xml;

/// <summary>
/// Type-safe OPC XML-DA standard result code discriminator.
/// </summary>
public enum XmlDaErrorCode
{
    /// <summary>Unknown, vendor-specific, or malformed result code.</summary>
    Unknown = 0,

    /// <summary><c>S_OK</c> or an omitted per-item <c>ResultID</c>.</summary>
    Ok = 1,

    /// <summary><c>S_CLAMP</c>.</summary>
    Clamp = 2,

    /// <summary><c>S_DATAQUEUEOVERFLOW</c>.</summary>
    DataQueueOverflow = 3,

    /// <summary><c>S_UNSUPPORTEDRATE</c>.</summary>
    UnsupportedRate = 4,

    /// <summary><c>E_ACCESS_DENIED</c>.</summary>
    AccessDenied = 100,

    /// <summary><c>E_BUSY</c>.</summary>
    Busy = 101,

    /// <summary><c>E_FAIL</c>.</summary>
    Fail = 102,

    /// <summary><c>E_INVALIDCONTINUATIONPOINT</c>.</summary>
    InvalidContinuationPoint = 103,

    /// <summary><c>E_INVALIDFILTER</c>.</summary>
    InvalidFilter = 104,

    /// <summary><c>E_INVALIDHOLDTIME</c>.</summary>
    InvalidHoldTime = 105,

    /// <summary><c>E_INVALIDITEMID</c>.</summary>
    InvalidItemId = 106,

    /// <summary><c>E_INVALIDITEMNAME</c>.</summary>
    InvalidItemName = 107,

    /// <summary><c>E_INVALIDITEMPATH</c>.</summary>
    InvalidItemPath = 108,

    /// <summary><c>E_INVALIDPID</c>.</summary>
    InvalidPid = 109,

    /// <summary><c>E_NOSUBSCRIPTION</c>.</summary>
    NoSubscription = 110,

    /// <summary><c>E_NOTSUPPORTED</c>.</summary>
    NotSupported = 111,

    /// <summary><c>E_OUTOFMEMORY</c>.</summary>
    OutOfMemory = 112,

    /// <summary><c>E_RANGE</c>.</summary>
    Range = 113,

    /// <summary><c>E_BADTYPE</c>.</summary>
    BadType = 114,

    /// <summary><c>E_READONLY</c>.</summary>
    ReadOnly = 115,

    /// <summary><c>E_SERVERSTATE</c>.</summary>
    ServerState = 116,

    /// <summary><c>E_TIMEDOUT</c>.</summary>
    TimedOut = 117,

    /// <summary><c>E_UNKNOWNITEMID</c>.</summary>
    UnknownItemId = 118,

    /// <summary><c>E_UNKNOWNITEMNAME</c>.</summary>
    UnknownItemName = 119,

    /// <summary><c>E_UNKNOWNITEMPATH</c>.</summary>
    UnknownItemPath = 120,

    /// <summary><c>E_WRITEONLY</c>.</summary>
    WriteOnly = 121,

    /// <summary><c>E_BADRIGHTS</c>, accepted for compatibility with legacy servers.</summary>
    BadRights = 122,
}
