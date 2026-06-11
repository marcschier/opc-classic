//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;

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
