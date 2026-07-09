// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic;

/// <summary>
/// Identifies which OPC specification produced an <see cref="OpcServerStatus"/>.
/// </summary>
public enum OpcStatusSpec
{
    /// <summary>
    /// Source is unknown (default / uninitialized).
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// OPC Data Access — <c>OPCSERVERSTATUS</c>.
    /// </summary>
    Da,
    /// <summary>
    /// OPC Alarms &amp; Events — <c>OPCEVENTSERVERSTATUS</c>.
    /// </summary>
    Ae,
    /// <summary>
    /// OPC Historical Data Access — <c>OPCHDA_SERVERSTATUS</c>.
    /// </summary>
    Hda,
    /// <summary>
    /// OPC Data eXchange.
    /// </summary>
    Dx,
}
