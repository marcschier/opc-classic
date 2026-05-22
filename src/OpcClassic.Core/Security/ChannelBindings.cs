//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic.Security;

/// <summary>
/// RFC 5056 / RFC 2744 channel bindings structure used for EPA hardening
/// of NTLMv2 (MsvAvChannelBindings AV-pair) and Kerberos
/// (KERB_AD_RESTRICTION_ENTRY).
/// </summary>
public sealed record ChannelBindings(
    int InitiatorAddrType,
    System.ReadOnlyMemory<byte> InitiatorAddress,
    int AcceptorAddrType,
    System.ReadOnlyMemory<byte> AcceptorAddress,
    System.ReadOnlyMemory<byte> ApplicationData);
