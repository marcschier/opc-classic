//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// SPNEGO negotiation result state values from RFC 4178.
/// </summary>
public enum SpnegoNegState
{
    /// <summary>
    /// Authentication completed successfully.
    /// </summary>
    AcceptCompleted = 0,

    /// <summary>
    /// Authentication requires another exchange.
    /// </summary>
    AcceptIncomplete = 1,

    /// <summary>
    /// Authentication was rejected.
    /// </summary>
    Reject = 2,

    /// <summary>
    /// A mechanism-list MIC is required.
    /// </summary>
    RequestMic = 3,
}
