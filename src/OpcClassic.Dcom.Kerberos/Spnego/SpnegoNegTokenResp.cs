//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic.Dcom.Kerberos.Spnego;

/// <summary>
/// RFC 4178 NegTokenResp fields returned by the acceptor.
/// </summary>
/// <param name="NegState">Optional negotiation state.</param>
/// <param name="SupportedMech">Optional selected mechanism object identifier.</param>
/// <param name="ResponseToken">Optional mechanism response token.</param>
/// <param name="MechListMic">Optional MIC over the mechanism list.</param>
public sealed record SpnegoNegTokenResp(
    SpnegoNegState? NegState,
    string? SupportedMech,
    ReadOnlyMemory<byte>? ResponseToken,
    ReadOnlyMemory<byte>? MechListMic);
