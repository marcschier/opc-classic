// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

#pragma warning disable CA1707 // OPC HRESULT symbolic name preserves spec casing.

namespace Opc.Classic.Batch;

/// <summary>
/// Spec-defined HRESULT constants for OPC Batch 2.00 (<c>OpcBatchError.h</c>).
/// </summary>
public static class OpcBatchErrors
{
    /// <summary>
    /// <c>OPCB_E_NOT_MEANINGFUL</c> (0xC0040300) — data is not meaningful at the present time.
    /// </summary>
    public const int OPCB_E_NOT_MEANINGFUL = unchecked((int)0xC0040300u);
}
