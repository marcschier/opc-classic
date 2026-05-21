//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

using System;

namespace OpcClassic.Ae;

/// <summary>
/// A reference to a single OPC AE condition. Used as the key when
/// enabling / disabling / acknowledging conditions.
/// </summary>
public readonly record struct ConditionRef(string Source, string ConditionName)
{
    /// <inheritdoc />
    public override string ToString() => $"{Source}::{ConditionName}";
}
