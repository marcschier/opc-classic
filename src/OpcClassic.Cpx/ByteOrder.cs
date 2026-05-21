//
// SPDX-License-Identifier: EPL-1.0
// Copyright (c) 2026 OPC Classic .NET Contributors
//

namespace OpcClassic.Cpx;

/// <summary>Byte order for OPCBinary numeric fields.</summary>
public enum ByteOrder
{
    /// <summary>Little-endian (default in OPCBinary, native on x86/x64).</summary>
    LittleEndian = 0,
    /// <summary>Big-endian.</summary>
    BigEndian = 1,
}
