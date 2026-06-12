//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

namespace Opc.Classic.Dcom.Orpc;

/// <summary>
/// ORPC extension payload entry.
/// </summary>
public sealed class OrpcExtent
{
    private readonly byte[] _data;

    /// <summary>
    /// Initializes a new ORPC extension entry.
    /// </summary>
    public OrpcExtent(Guid id, ReadOnlyMemory<byte> data)
    {
        Id = id;
        _data = data.ToArray();
    }

    private OrpcExtent(Guid id, byte[] data)
    {
        Id = id;
        _data = data;
    }

    /// <summary>
    /// Gets the extension identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the extension data.
    /// </summary>
    public ReadOnlyMemory<byte> Data => _data;

    internal static OrpcExtent FromOwnedData(Guid id, byte[] data) => new(id, data);
}
