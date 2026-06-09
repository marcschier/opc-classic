//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable CA1707 // OPC Security HRESULT names preserve OpcErrSec.h identifiers.

namespace Opc.Classic;

/// <summary>
/// Spec-defined HRESULT constants for OPC Security 1.00 (<c>OpcErrSec.h</c>).
/// </summary>
public static class OpcSecurityErrors {
    /// <summary><c>OPC_E_PRIVATE_ACTIVE</c> (0xC0040301) — private OPC credentials are already active.</summary>
    public const int OPC_E_PRIVATE_ACTIVE = unchecked((int)0xC0040301u);

    /// <summary><c>OPC_E_LOW_IMPERS_LEVEL</c> (0xC0040302) — server requires a higher impersonation level.</summary>
    public const int OPC_E_LOW_IMPERS_LEVEL = unchecked((int)0xC0040302u);

    /// <summary><c>OPC_S_LOW_AUTHN_LEVEL</c> (0x00040303) — server expected higher packet privacy.</summary>
    public const int OPC_S_LOW_AUTHN_LEVEL = 0x00040303;
}
