//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Kerberos.NET.Crypto;

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Established Kerberos per-message keying material.
/// </summary>
/// <param name="Key">The session or sub-session key bytes.</param>
/// <param name="EncryptionType">The Kerberos encryption type associated with <paramref name="Key" />.</param>
/// <param name="UsesAcceptorSubkey">Whether the key came from the acceptor AP-REP subkey.</param>
public sealed record KerberosSessionKey(
    ReadOnlyMemory<byte> Key,
    EncryptionType EncryptionType,
    bool UsesAcceptorSubkey);
