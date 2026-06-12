//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Security;

/// <summary>
/// OPC Security impersonation levels used by <c>IOPCSecurityNT::QueryMinImpersonationLevel</c>.
/// </summary>
public enum OpcImpersonationLevel
{
    /// <summary>
    /// Use the COM default impersonation level.
    /// </summary>
    Default = 0,

    /// <summary>
    /// The client is anonymous to the server.
    /// </summary>
    Anonymous = 1,

    /// <summary>
    /// The server can identify the client but cannot impersonate it.
    /// </summary>
    Identify = 2,

    /// <summary>
    /// The server can impersonate the client on the local system.
    /// </summary>
    Impersonate = 3,

    /// <summary>
    /// The server can impersonate the client across remote systems.
    /// </summary>
    Delegate = 4,
}
