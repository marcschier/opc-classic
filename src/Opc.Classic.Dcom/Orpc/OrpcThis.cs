//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using Opc.Classic.Ndr;

namespace Opc.Classic.Dcom.Orpc;

/// <summary>
/// ORPC_THIS request envelope defined by [MS-DCOM] section 2.2.19.
/// </summary>
public sealed class OrpcThis {
    /// <summary>Wire size when the extensions pointer is null.</summary>
    public const int NullExtensionsWireSize = 32;

    /// <summary>Gets or initializes the COM version advertised in the call envelope.</summary>
    public OrpcComVersion Version { get; init; } = OrpcComVersion.Default;

    /// <summary>Gets or initializes ORPC flags. Zero is the normal value for remote calls.</summary>
    public uint Flags { get; init; }

    /// <summary>Gets or initializes the causality identifier for this logical call chain.</summary>
    public Guid CausalityId { get; init; } = Guid.NewGuid();

    /// <summary>Gets or initializes the optional ORPC extension array. Null encodes a null pointer.</summary>
    public IReadOnlyList<OrpcExtent>? Extensions { get; init; }

    /// <summary>Writes this envelope using NDR encoding.</summary>
    public void Write(ref NdrWriter writer) {
        writer.WriteUInt16(Version.Major);
        writer.WriteUInt16(Version.Minor);
        writer.WriteUInt32(Flags);
        writer.WriteUInt32(0u);
        writer.WriteGuid(CausalityId);
        OrpcExtentArrayCodec.Write(ref writer, Extensions);
    }

    /// <summary>Reads an ORPC_THIS envelope using NDR encoding.</summary>
    public static OrpcThis Read(ref NdrReader reader) {
        var version = new OrpcComVersion(reader.ReadUInt16(), reader.ReadUInt16());
        uint flags = reader.ReadUInt32();
        uint reserved1 = reader.ReadUInt32();
        if (reserved1 != 0u) {
            throw new InvalidOperationException($"ORPC_THIS reserved1 must be zero but was {reserved1}.");
        }

        Guid causalityId = reader.ReadGuid();
        IReadOnlyList<OrpcExtent>? extensions = OrpcExtentArrayCodec.Read(ref reader);
        return new OrpcThis {
            Version = version,
            Flags = flags,
            CausalityId = causalityId,
            Extensions = extensions,
        };
    }
}

/// <summary>
/// COMVERSION value carried by ORPC_THIS.
/// </summary>
public readonly record struct OrpcComVersion(ushort Major, ushort Minor) {
    /// <summary>Default DCOM COMVERSION for ORPC calls.</summary>
    public static OrpcComVersion Default { get; } = new(5, 7);
}

/// <summary>
/// ORPC extension payload entry.
/// </summary>
public sealed class OrpcExtent {
    private readonly byte[] _data;

    /// <summary>Initializes a new ORPC extension entry.</summary>
    public OrpcExtent(Guid id, ReadOnlyMemory<byte> data) {
        Id = id;
        _data = data.ToArray();
    }

    private OrpcExtent(Guid id, byte[] data) {
        Id = id;
        _data = data;
    }

    /// <summary>Gets the extension identifier.</summary>
    public Guid Id { get; }

    /// <summary>Gets the extension data.</summary>
    public ReadOnlyMemory<byte> Data => _data;

    internal static OrpcExtent FromOwnedData(Guid id, byte[] data) => new(id, data);
}
