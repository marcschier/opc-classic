// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Kerberos.NET.Crypto;

namespace Opc.Classic.Dcom.Kerberos;

/// <summary>
/// Established Kerberos per-message keying material.
/// </summary>
/// <param name="Key">The session or sub-session key bytes.</param>
/// <param name="EncryptionType">The Kerberos encryption type associated with <paramref name="Key" />.</param>
/// <param name="UsesAcceptorSubkey">Whether the key came from the acceptor AP-REP subkey.</param>
/// <param name="SendSequenceNumber">The negotiated initial sequence number for outbound tokens.</param>
/// <param name="ReceiveSequenceNumber">The negotiated initial sequence number for inbound tokens.</param>
public sealed record KerberosSessionKey(
    ReadOnlyMemory<byte> Key,
    EncryptionType EncryptionType,
    bool UsesAcceptorSubkey,
    long SendSequenceNumber = 0,
    long ReceiveSequenceNumber = 0);
