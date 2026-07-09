// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Da;

/// <summary>
/// OPC DA's <c>OPCITEMVQT</c> — a value paired with explicit quality and
/// timestamp flags. Used by <c>IOPCSyncIO2::WriteVQT</c> and
/// <c>IOPCAsyncIO3::WriteVQT</c> to write items with caller-supplied
/// quality and/or timestamp (rather than letting the server default).
/// </summary>
/// <param name="Value">The value to write.</param>
/// <param name="Quality">Caller-supplied quality, or null to use the server's default.</param>
/// <param name="Timestamp">Caller-supplied UTC timestamp, or null to use the server's "now".</param>
/// <remarks>
/// Wire form (<c>tagOPCITEMVQT</c> from opcda.h):
/// <code>
///     VARIANT  vDataValue;
///     BOOL     bQualitySpecified;   // 4-byte Win32 BOOL (-1 / 0)
///     WORD     wQuality;
///     WORD     wReserved;
///     BOOL     bTimeStampSpecified; // 4-byte Win32 BOOL
///     DWORD    dwReserved;
///     FILETIME ftTimeStamp;
/// </code>
/// The NDR codec lives in <see cref="Opc.Classic.Da.Ndr.NdrOpcItemVqtCodec"/>.
/// </remarks>
public sealed record OpcItemVqt(
    OpcVariant Value,
    OpcQuality? Quality = null,
    DateTimeOffset? Timestamp = null);
