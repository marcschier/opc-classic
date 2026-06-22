// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Security;

/// <summary>
/// RFC 5056 / RFC 2744 channel bindings structure used for EPA hardening
/// of NTLMv2 (MsvAvChannelBindings AV-pair) and Kerberos
/// (KRB_AP_CHKSUM_TYPE_GSS authenticator checksum).
/// </summary>
public sealed record ChannelBindings(
    int InitiatorAddrType,
    System.ReadOnlyMemory<byte> InitiatorAddress,
    int AcceptorAddrType,
    System.ReadOnlyMemory<byte> AcceptorAddress,
    System.ReadOnlyMemory<byte> ApplicationData);
