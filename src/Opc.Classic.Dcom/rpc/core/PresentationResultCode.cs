// SPDX-License-Identifier: MIT

namespace Opc.Classic.Dcom.Rpc.Core;

/// <summary>
/// Presentation result code
/// </summary>
public enum PresentationResultCode
{

    /// <summary>
    /// Accept
    /// </summary>
    ACCEPTANCE = 0,

    /// <summary>
    /// User rejected
    /// </summary>
    USER_REJECTION = 1,

    /// <summary>
    /// Rejected
    /// </summary>
    PROVIDER_REJECTION = 2,
}
