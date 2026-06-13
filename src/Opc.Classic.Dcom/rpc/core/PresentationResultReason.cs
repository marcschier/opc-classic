// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Rpc.Core;

/// <summary>
/// Presentation result reason
/// </summary>
public enum PresentationResultReason
{
    /// <summary>
    /// No reason
    /// </summary>
    REASON_NOT_SPECIFIED = 0,

    /// <summary>
    /// Not supported
    /// </summary>
    ABSTRACT_SYNTAX_NOT_SUPPORTED = 1,

    /// <summary>
    /// Transfer syntax not supported
    /// </summary>
    PROPOSED_TRANSFER_SYNTAXES_NOT_SUPPORTED = 2,

    /// <summary>
    /// Local limit exceeded
    /// </summary>
    LOCAL_LIMIT_EXCEEDED = 3,
}
