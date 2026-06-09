//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Xml;

/// <summary>
/// Server state codes from the OPC XML-DA 1.0 spec (<c>serverState</c>
/// element in <c>GetStatusResponse</c>).
/// </summary>
public enum XmlDaServerState {
    /// <summary><c>running</c> — server is operating normally.</summary>
    Running = 0,

    /// <summary><c>failed</c> — abnormal error has rendered the server inoperative.</summary>
    Failed = 1,

    /// <summary><c>noConfig</c> — server is running but has no configuration to act on.</summary>
    NoConfig = 2,

    /// <summary><c>suspended</c> — server has halted polling, callbacks suspended.</summary>
    Suspended = 3,

    /// <summary><c>test</c> — server is running in test mode; values may be simulated.</summary>
    Test = 4,

    /// <summary><c>commFault</c> — the underlying data source is unreachable.</summary>
    CommFault = 5,
}
