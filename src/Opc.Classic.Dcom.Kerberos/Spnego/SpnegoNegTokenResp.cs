//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Dcom.Kerberos.Spnego;

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
