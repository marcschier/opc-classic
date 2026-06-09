//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

namespace Opc.Classic.Ae;

/// <summary>
/// OPC Alarms &amp; Events event-type bitmask (<c>OPCEVENT*</c>). An event
/// may belong to one or more categories — the values here are bit flags.
/// </summary>
[Flags]
public enum EventType {
    /// <summary>No event type selected.</summary>
    None = 0,
    /// <summary>A simple event (a notification without a condition).</summary>
    Simple = 0x0001,
    /// <summary>A tracking event (an operator action).</summary>
    Tracking = 0x0002,
    /// <summary>A condition-related event (alarm).</summary>
    Condition = 0x0004,
    /// <summary>Any/all of the above.</summary>
    All = Simple | Tracking | Condition,
}
