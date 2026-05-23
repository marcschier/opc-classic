//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

#pragma warning disable CA1720 // Identifier contains type name — OPCBinary spec names

namespace Opc.Classic.Cpx;

/// <summary>
/// The categories of leaf type defined by OPCBinary (the OPC Complex Data
/// type-dictionary schema). Composite types (Struct, Array) are represented
/// in the managed model by <see cref="StructType"/> declarations and
/// <see cref="StructField"/>.Repeats &gt; 1, not by separate <see cref="TypeKind"/> values.
/// </summary>
public enum TypeKind
{
    /// <summary>Not a recognized type.</summary>
    Unknown = 0,
    /// <summary>Boolean.</summary>
    Boolean = 1,
    /// <summary>Signed 8-bit integer.</summary>
    Int8 = 2,
    /// <summary>Signed 16-bit integer.</summary>
    Int16 = 3,
    /// <summary>Signed 32-bit integer.</summary>
    Int32 = 4,
    /// <summary>Signed 64-bit integer.</summary>
    Int64 = 5,
    /// <summary>Unsigned 8-bit integer.</summary>
    UInt8 = 6,
    /// <summary>Unsigned 16-bit integer.</summary>
    UInt16 = 7,
    /// <summary>Unsigned 32-bit integer.</summary>
    UInt32 = 8,
    /// <summary>Unsigned 64-bit integer.</summary>
    UInt64 = 9,
    /// <summary>IEEE 32-bit float.</summary>
    Single = 10,
    /// <summary>IEEE 64-bit float.</summary>
    Double = 11,
    /// <summary>Length-prefixed bytes (CharWidth / StringEncoding determined by field options).</summary>
    String = 12,
    /// <summary>Windows FILETIME (8 bytes).</summary>
    FileTime = 13,
    /// <summary>16-byte GUID.</summary>
    Guid = 14,
    /// <summary>Length-prefixed byte blob.</summary>
    Blob = 15,
    /// <summary>Reference to a struct defined in the same dictionary.</summary>
    StructReference = 16,
}
