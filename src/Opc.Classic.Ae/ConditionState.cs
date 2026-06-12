//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Ae;

/// <summary>
/// State flags of an OPC AE condition.
/// </summary>
[Flags]
public enum ConditionState
{
    /// <summary>No state bits set.</summary>
    None = 0,

    /// <summary>The condition is currently in alarm.</summary>
    Active = 0x0001,

    /// <summary>The condition has been acknowledged by an operator.</summary>
    Acknowledged = 0x0002,

    /// <summary>The condition is enabled (server is monitoring it).</summary>
    Enabled = 0x0004,
}
