//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic.Da;

/// <summary>
/// Managed projection of the multi-output state returned by <c>IOPCGroupStateMgt::GetState</c>.
/// </summary>
/// <param name="ClientHandle">Client-supplied group handle echoed by the server.</param>
/// <param name="ServerHandle">Server-assigned group handle.</param>
/// <param name="Name">Current group name.</param>
/// <param name="Active">Whether the group is active.</param>
/// <param name="UpdateRate">Current update rate in milliseconds.</param>
/// <param name="TimeBias">Time bias, in minutes from UTC.</param>
/// <param name="PercentDeadband">Deadband percentage applied to analog values.</param>
/// <param name="LocaleId">LCID used for server-supplied text.</param>
public sealed record OpcGroupState(
    int ClientHandle,
    int ServerHandle,
    string? Name,
    bool Active,
    int UpdateRate,
    int TimeBias,
    float PercentDeadband,
    int LocaleId);
