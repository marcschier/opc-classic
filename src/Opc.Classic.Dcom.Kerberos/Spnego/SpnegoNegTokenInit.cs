//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace Opc.Classic.Dcom.Kerberos.Spnego;

/// <summary>
/// RFC 4178 NegTokenInit fields carried in the initial SPNEGO token.
/// </summary>
/// <param name="MechTypes">Mechanism object identifiers in initiator preference order.</param>
/// <param name="MechToken">Optional optimistic mechanism token.</param>
/// <param name="MechListMic">Optional MIC over the mechanism list.</param>
public sealed record SpnegoNegTokenInit(
    IReadOnlyList<string> MechTypes,
    ReadOnlyMemory<byte> MechToken,
    ReadOnlyMemory<byte>? MechListMic);
