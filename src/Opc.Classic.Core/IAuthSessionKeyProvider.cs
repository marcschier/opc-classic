//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic;

/// <summary>
/// Optional authentication context capability for transports that need the established session key.
/// </summary>
public interface IAuthSessionKeyProvider
{
    /// <summary>Gets the negotiated NTLMSSP/Kerberos session key, if one is established.</summary>
    ReadOnlyMemory<byte>? GetSessionKey();
}
