//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic.Da;

/// <summary>
/// OPC DA's <c>OPCITEMSTATE</c> — the current value+quality+timestamp for
/// one item, paired with the client-side handle. Returned by
/// <c>IOPCSyncIO::Read</c> et al.
/// </summary>
/// <param name="ClientHandle">The client-supplied handle echoed by the server.</param>
/// <param name="Timestamp">UTC timestamp the server attached to the value.</param>
/// <param name="Quality">DA quality word (low 16 bits — kind/sub/limit).</param>
/// <param name="Value">The current value.</param>
public sealed record OpcItemState(
    int ClientHandle,
    DateTimeOffset Timestamp,
    OpcQuality Quality,
    OpcVariant Value);
