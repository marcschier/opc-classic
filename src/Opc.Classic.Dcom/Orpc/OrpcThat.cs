//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Orpc;

/// <summary>
/// ORPC_THAT response envelope defined by [MS-DCOM] section 2.2.20.
/// </summary>
public sealed class OrpcThat
{
    /// <summary>Wire size when the extensions pointer is null.</summary>
    public const int NullExtensionsWireSize = 8;

    /// <summary>Gets or initializes ORPC response flags.</summary>
    public uint Flags { get; init; }

    /// <summary>Gets or initializes the optional ORPC extension array. Null encodes a null pointer.</summary>
    public IReadOnlyList<OrpcExtent>? Extensions { get; init; }

    /// <summary>Writes this envelope using NDR encoding.</summary>
    public void Write(ref NdrWriter writer)
    {
        writer.WriteUInt32(Flags);
        OrpcExtentArrayCodec.Write(ref writer, Extensions);
    }

    /// <summary>Reads an ORPC_THAT envelope using NDR encoding.</summary>
    public static OrpcThat Read(ref NdrReader reader)
    {
        uint flags = reader.ReadUInt32();
        ValidateFlags(flags);
        IReadOnlyList<OrpcExtent>? extensions = OrpcExtentArrayCodec.Read(ref reader);
        return new OrpcThat
        {
            Flags = flags,
            Extensions = extensions,
        };
    }

    private static void ValidateFlags(uint flags)
    {
        const uint knownFlags = 0x0000001Fu;
        if ((flags & ~knownFlags) != 0u)
        {
            throw new InvalidOperationException($"ORPC_THAT flags contain reserved bits: 0x{flags:X8}.");
        }
    }
}
