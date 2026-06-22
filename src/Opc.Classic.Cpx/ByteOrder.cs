// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Cpx;

/// <summary>
/// Byte order for OPCBinary numeric fields.
/// </summary>
public enum ByteOrder
{
    /// <summary>
    /// Little-endian (default in OPCBinary, native on x86/x64).
    /// </summary>
    LittleEndian = 0,

    /// <summary>
    /// Big-endian.
    /// </summary>
    BigEndian = 1,
}
