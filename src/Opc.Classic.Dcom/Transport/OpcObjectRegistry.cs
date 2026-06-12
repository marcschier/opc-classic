//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Concurrent;
using Opc.Classic.Hosting;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Server-side registry mapping per-call <c>RequestCoPdu.Object</c> UUIDs
/// (interface pointer identifiers, "IPIDs") to the
/// <see cref="IOpcServerDispatcher" /> set that backs the corresponding
/// managed object.
/// </summary>
/// <remarks>
/// <para>
/// In DCOM, every interface pointer carries an IPID; calls on that
/// pointer set <c>PFC_OBJECT_UUID</c> and include the IPID in
/// <c>RequestCoPdu.Object</c>. The server uses the IPID + interface-id
/// pair to route the call to the right managed object's
/// per-interface dispatcher.
/// </para>
/// <para>
/// The root server object (which exposes <c>IOPCServer</c>,
/// <c>IOPCCommon</c>, etc. without requiring an Object UUID) lives in
/// <see cref="RpcServerConnectionProcessor"/>'s root dispatcher map; the
/// registry is consulted only when an inbound request carries an Object
/// UUID. Hosts add per-object entries (groups, items, enumerators)
/// when their managed implementations create those objects.
/// </para>
/// <para>
/// Thread-safe: <see cref="ConcurrentDictionary{TKey, TValue}"/>-backed.
/// </para>
/// </remarks>
public sealed class OpcObjectRegistry
{
    private readonly ConcurrentDictionary<Guid, IReadOnlyDictionary<Guid, IOpcServerDispatcher>> _objects = new();

    /// <summary>
    /// Gets the number of currently registered objects.
    /// </summary>
    public int Count => _objects.Count;

    /// <summary>
    /// Registers a new object exposing the supplied interface set and
    /// returns a freshly-allocated IPID.
    /// </summary>
    /// <param name="interfaceDispatchers">
    /// The interfaces the new object supports, keyed by interface IID,
    /// each mapped to the source-generated dispatcher wrapping the
    /// managed implementation.
    /// </param>
    public Guid Register(IReadOnlyDictionary<Guid, IOpcServerDispatcher> interfaceDispatchers)
    {
        ArgumentNullException.ThrowIfNull(interfaceDispatchers);
        Guid ipid = Guid.NewGuid();
        if (!_objects.TryAdd(ipid, interfaceDispatchers))
        {
            // Collision is astronomically unlikely for a freshly-allocated
            // v4 GUID; retry once and then surface the failure.
            ipid = Guid.NewGuid();
            if (!_objects.TryAdd(ipid, interfaceDispatchers))
            {
                throw new InvalidOperationException("OpcObjectRegistry could not allocate a fresh IPID.");
            }
        }
        return ipid;
    }

    /// <summary>
    /// Registers an object under a caller-supplied stable IPID.
    /// Returns <see langword="false"/> if the IPID is already in use.
    /// </summary>
    public bool RegisterWithIpid(Guid ipid, IReadOnlyDictionary<Guid, IOpcServerDispatcher> interfaceDispatchers)
    {
        ArgumentNullException.ThrowIfNull(interfaceDispatchers);
        return _objects.TryAdd(ipid, interfaceDispatchers);
    }

    /// <summary>
    /// Removes an object's registration (e.g., after RemoveGroup or
    /// the managed lifetime expires).
    /// </summary>
    /// <returns><see langword="true"/> if the IPID was present.</returns>
    public bool Unregister(Guid ipid) => _objects.TryRemove(ipid, out _);

    /// <summary>
    /// Attempts to resolve a dispatcher for a specific (IPID, interface)
    /// pair.
    /// </summary>
    public bool TryGetDispatcher(Guid ipid, Guid interfaceId, out IOpcServerDispatcher dispatcher)
    {
        if (_objects.TryGetValue(ipid, out IReadOnlyDictionary<Guid, IOpcServerDispatcher>? interfaceMap)
            && interfaceMap.TryGetValue(interfaceId, out IOpcServerDispatcher? found))
        {
            dispatcher = found;
            return true;
        }

        dispatcher = null!;
        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> when any registered object exposes
    /// the supplied interface identifier.
    /// </summary>
    public bool ContainsInterface(Guid interfaceId)
    {
        foreach (IReadOnlyDictionary<Guid, IOpcServerDispatcher> interfaceMap in _objects.Values)
        {
            if (interfaceMap.ContainsKey(interfaceId))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns <see langword="true"/> when the given IPID is currently
    /// registered (regardless of which interface).
    /// </summary>
    public bool Contains(Guid ipid) => _objects.ContainsKey(ipid);
}
