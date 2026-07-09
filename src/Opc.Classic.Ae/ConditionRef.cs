// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Ae;

/// <summary>
/// A reference to a single OPC AE condition. Used as the key when
/// enabling / disabling / acknowledging conditions.
/// </summary>
public readonly record struct ConditionRef(string Source, string ConditionName)
{
    /// <inheritdoc />
    public override string ToString() => $"{Source}::{ConditionName}";
}
