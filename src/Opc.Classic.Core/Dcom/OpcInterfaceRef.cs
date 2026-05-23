//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;

namespace Opc.Classic.Dcom;

/// <summary>
/// Immutable managed representation of a decoded DCOM OBJREF_STANDARD interface pointer.
/// </summary>
public sealed class OpcInterfaceRef : IOpcInterfaceRef
{
    private readonly ushort[] _resolverBindings;

    /// <summary>Creates a new managed interface-reference handle.</summary>
    public OpcInterfaceRef(
        Guid iid,
        uint flags,
        uint publicRefs,
        ulong oxid,
        ulong oid,
        Guid ipid,
        ushort securityOffset,
        IReadOnlyList<ushort> resolverBindings)
    {
        ArgumentNullException.ThrowIfNull(resolverBindings);

        Iid = iid;
        Flags = flags;
        PublicRefs = publicRefs;
        Oxid = oxid;
        Oid = oid;
        Ipid = ipid;
        SecurityOffset = securityOffset;
        _resolverBindings = new ushort[resolverBindings.Count];
        for (int i = 0; i < resolverBindings.Count; i++)
        {
            _resolverBindings[i] = resolverBindings[i];
        }
    }

    /// <inheritdoc />
    public Guid Iid { get; }

    /// <inheritdoc />
    public uint Flags { get; }

    /// <inheritdoc />
    public uint PublicRefs { get; }

    /// <inheritdoc />
    public ulong Oxid { get; }

    /// <inheritdoc />
    public ulong Oid { get; }

    /// <inheritdoc />
    public Guid Ipid { get; }

    /// <inheritdoc />
    public ushort SecurityOffset { get; }

    /// <inheritdoc />
    public IReadOnlyList<ushort> ResolverBindings => _resolverBindings;
}
