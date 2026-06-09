//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Da;

/// <summary>
/// Configuration of an OPC DA subscription (server-side group). Passed to
/// <c>IDaServer.CreateSubscriptionAsync</c> and to <c>IDaSubscription.SetState</c>.
/// </summary>
/// <remarks>
/// Mirrors the parameters that <c>IOPCServer::AddGroup</c> /
/// <c>IOPCGroupStateMgt::SetState</c> take, but as a managed init-only record.
/// Nullable fields signal "leave unchanged" semantics for SetState.
/// </remarks>
public sealed class SubscriptionState {
    /// <summary>
    /// Subscription / group name. Optional — servers will assign one if blank.
    /// Group names are unique within a server connection.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Opaque client handle for the subscription. Echoed back in callbacks.
    /// </summary>
    public int ClientHandle { get; init; }

    /// <summary>
    /// Update rate, in milliseconds. The server may negotiate a different
    /// (slower) rate and report it back via <c>OPC_S_UNSUPPORTEDRATE</c>.
    /// </summary>
    public int UpdateRateMs { get; init; }

    /// <summary>
    /// Whether the subscription is active (server pushes OnDataChange callbacks
    /// when true; silent when false).
    /// </summary>
    public bool Active { get; init; } = true;

    /// <summary>
    /// Deadband as a percentage (0..100) of EU range. Updates within the
    /// deadband are suppressed.
    /// </summary>
    public float DeadbandPercent { get; init; }

    /// <summary>
    /// Time bias (offset from UTC) in minutes — used to translate server-side
    /// timestamps for clients in different time zones.
    /// </summary>
    public int TimeBiasMinutes { get; init; }

    /// <summary>
    /// LCID — locale ID for any server-supplied messages. Use
    /// <see cref="System.Globalization.CultureInfo.LCID"/> to obtain.
    /// </summary>
    public int LocaleId { get; init; }

    /// <summary>
    /// DA 3.0 keep-alive interval, in milliseconds. If no data changes occur,
    /// the server sends an empty OnDataChange every <c>KeepAliveMs</c> to
    /// reassure the client the connection is healthy. Zero disables keepalive.
    /// </summary>
    public int KeepAliveMs { get; init; }

    /// <summary>Convenience: create a subscription that polls every <paramref name="updateRate"/>.</summary>
    public static SubscriptionState At(TimeSpan updateRate, bool active = true) {
        if (updateRate <= TimeSpan.Zero) {
            throw new ArgumentOutOfRangeException(nameof(updateRate), updateRate,
                "Update rate must be positive.");
        }
        return new SubscriptionState {
            UpdateRateMs = (int)updateRate.TotalMilliseconds,
            Active = active,
        };
    }
}
