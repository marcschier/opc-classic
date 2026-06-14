//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// In-process OBJREF stand-in used by managed host tests to bind an IOPCShutdown sink.
/// </summary>
public sealed class OpcDaShutdownSinkRef : IOpcInterfaceRef
{
    /// <summary>
    /// Initializes a new instance of the <see cref="OpcDaShutdownSinkRef" /> class.
    /// </summary>
    public OpcDaShutdownSinkRef(IOPCShutdown sink)
    {
        Sink = sink ?? throw new ArgumentNullException(nameof(sink));
        Ipid = Guid.CreateVersion7();
    }

    /// <summary>
    /// Gets the in-process shutdown sink.
    /// </summary>
    public IOPCShutdown Sink { get; }

    /// <inheritdoc />
    public Guid Iid => IOPCShutdown.InterfaceId;

    /// <inheritdoc />
    public uint Flags => 0;

    /// <inheritdoc />
    public uint PublicRefs => 1;

    /// <inheritdoc />
    public ulong Oxid => 1;

    /// <inheritdoc />
    public ulong Oid => 1;

    /// <inheritdoc />
    public Guid Ipid { get; }

    /// <inheritdoc />
    public ushort SecurityOffset => 0;

    /// <inheritdoc />
    public IReadOnlyList<ushort> ResolverBindings => Array.Empty<ushort>();
}
