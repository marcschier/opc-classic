// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic;

/// <summary>
/// The OPC quality top-level category (bits 0-1 of the DA quality WORD).
/// </summary>
public enum OpcQualityKind
{
    /// <summary>
    /// Bad: the value is not useful (sensor failure, comms loss, ...).
    /// </summary>
    Bad = 0,
    /// <summary>
    /// Uncertain: the value is not known to be correct.
    /// </summary>
    Uncertain = 1,
    /// <summary>
    /// Reserved by the OPC DA spec — should never appear.
    /// </summary>
    Reserved = 2,
    /// <summary>
    /// Good: the value is current and reliable.
    /// </summary>
    Good = 3,
}
