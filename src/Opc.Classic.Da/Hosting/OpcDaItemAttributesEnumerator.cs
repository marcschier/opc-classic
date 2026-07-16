// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// Stateful per-cursor implementation of
/// <see cref="IEnumOPCItemAttributes"/>. Snapshots the group's items at
/// construction time so Next/Skip/Reset/Clone are deterministic regardless
/// of subsequent item additions/removals.
/// </summary>
/// <remarks>
/// <para>
/// Per-cursor: each <c>CreateEnumerator</c> / <c>Clone</c> allocates a
/// fresh instance with its own cursor and snapshot. The host registers
/// each instance in the <see cref="OpcObjectRegistry"/> so the assigned
/// IPID routes inbound requests to the right cursor.
/// </para>
/// <para>
/// <b>Snapshot semantics (OPC DA 2.05a §4.4.7.2).</b> The OPC DA spec
/// states "the enumerator reflects the state of the group at the time of
/// creation." This implementation captures the item array at
/// construction; subsequent <c>AddItems</c> / <c>RemoveItems</c> calls
/// on the originating group do NOT affect previously-issued enumerators.
/// Clients that want fresh data must call <c>CreateEnumerator</c> again.
/// </para>
/// </remarks>
public sealed class OpcDaItemAttributesEnumerator : IEnumOPCItemAttributes
{
    private readonly OpcItemAttributes[] _snapshot;
    private readonly OpcObjectRegistry? _registry;
    private int _cursor;

    /// <summary>
    /// Initializes a new enumerator over the supplied snapshot.
    /// </summary>
    /// <param name="snapshot">The full item attributes array; iteration starts at index 0.</param>
    /// <param name="registry">Optional registry used to register clones (<see cref="CloneAsync"/>); when null, clones return a synthetic ref.</param>
    public OpcDaItemAttributesEnumerator(OpcItemAttributes[] snapshot, OpcObjectRegistry? registry = null)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _snapshot = snapshot;
        _registry = registry;
        _cursor = 0;
    }

    /// <summary>
    /// Returns the total number of items in the snapshot (test diagnostic).
    /// </summary>
    public int Length => _snapshot.Length;

    /// <summary>
    /// Returns the current cursor position (test diagnostic).
    /// </summary>
    public int Position => _cursor;

    /// <inheritdoc />
    public Task NextAsync(
        int count,
        out OpcItemAttributes[] attributes,
        out int fetchedCount,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (count <= 0)
        {
            attributes = Array.Empty<OpcItemAttributes>();
            fetchedCount = 0;
            return Task.CompletedTask;
        }

        int available = Math.Max(0, _snapshot.Length - _cursor);
        int take = Math.Min(count, available);
        if (take == 0)
        {
            attributes = Array.Empty<OpcItemAttributes>();
            fetchedCount = 0;
            return Task.CompletedTask;
        }

        var batch = new OpcItemAttributes[take];
        Array.Copy(_snapshot, _cursor, batch, 0, take);
        _cursor += take;
        attributes = batch;
        fetchedCount = take;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SkipAsync(int count, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (count > 0)
        {
            _cursor = Math.Min(_cursor + count, _snapshot.Length);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task ResetAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _cursor = 0;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IOpcInterfaceRef> CloneAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var clone = new OpcDaItemAttributesEnumerator(_snapshot, _registry) { _cursor = _cursor };

        Guid ipid;
        if (_registry is not null)
        {
            var dispatchers = new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IEnumOPCItemAttributes.InterfaceId] = new IEnumOPCItemAttributesServerDispatcher(clone),
            };
            ipid = _registry.Register(dispatchers, publicRefs: 1);
        }
        else
        {
            ipid = Guid.CreateVersion7();
        }

        return Task.FromResult(
            _registry is null
                ? new OpcInterfaceRef(
                    iid: IEnumOPCItemAttributes.InterfaceId,
                    flags: 0,
                    publicRefs: 1,
                    oxid: 1,
                    oid: 0,
                    ipid: ipid,
                    securityOffset: 0,
                    resolverBindings: Array.Empty<ushort>())
                : CreateRegisteredRef(_registry, ipid));
    }

    private static IOpcInterfaceRef CreateRegisteredRef(OpcObjectRegistry registry, Guid ipid)
    {
        if (!registry.TryGetObjectMetadata(ipid, out OpcObjectMetadata metadata))
        {
            throw new InvalidOperationException("The registered item enumerator has no identity metadata.");
        }

        return new OpcInterfaceRef(
            iid: IEnumOPCItemAttributes.InterfaceId,
            flags: 0,
            publicRefs: 1,
            oxid: metadata.Oxid,
            oid: metadata.Oid,
            ipid: ipid,
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>());
    }
}
