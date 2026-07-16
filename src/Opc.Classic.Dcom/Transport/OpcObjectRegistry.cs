// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

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
    private readonly ConcurrentDictionary<Guid, ObjectEntry> _objects = new();
    private readonly Dictionary<Guid, uint> _publicRefs = new();
    private readonly Dictionary<Guid, Action> _finalReleaseCallbacks = new();
    private readonly Lock _lifetimeGate = new();
    private readonly ulong _oxid = CreateNonZeroId();
    private ulong _nextOid;

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
    public Guid Register(
        IReadOnlyDictionary<Guid, IOpcServerDispatcher> interfaceDispatchers,
        uint publicRefs = 0) =>
        Register(interfaceDispatchers, publicRefs, finalRelease: null);

    /// <summary>
    /// Registers a new object and invokes <paramref name="finalRelease"/> when
    /// its last public reference is released or it is explicitly unregistered.
    /// </summary>
    public Guid Register(
        IReadOnlyDictionary<Guid, IOpcServerDispatcher> interfaceDispatchers,
        uint publicRefs,
        Action? finalRelease)
    {
        ArgumentNullException.ThrowIfNull(interfaceDispatchers);
        lock (_lifetimeGate)
        {
            Guid ipid = Guid.NewGuid();
            var entry = new ObjectEntry(interfaceDispatchers, AllocateMetadataUnderLock());
            if (!_objects.TryAdd(ipid, entry))
            {
                // Collision is astronomically unlikely for a freshly-allocated
                // v4 GUID; retry once and then surface the failure.
                ipid = Guid.NewGuid();
                if (!_objects.TryAdd(ipid, entry))
                {
                    throw new InvalidOperationException("OpcObjectRegistry could not allocate a fresh IPID.");
                }
            }
            SeedLifetimeUnderLock(ipid, publicRefs, finalRelease);
            return ipid;
        }
    }

    /// <summary>
    /// Registers an object under a caller-supplied stable IPID.
    /// Returns <see langword="false"/> if the IPID is already in use.
    /// </summary>
    public bool RegisterWithIpid(
        Guid ipid,
        IReadOnlyDictionary<Guid, IOpcServerDispatcher> interfaceDispatchers,
        uint publicRefs = 0) =>
        RegisterWithIpid(ipid, interfaceDispatchers, publicRefs, finalRelease: null);

    /// <summary>
    /// Registers an object under a caller-supplied IPID and invokes
    /// <paramref name="finalRelease"/> when its lifetime ends.
    /// </summary>
    public bool RegisterWithIpid(
        Guid ipid,
        IReadOnlyDictionary<Guid, IOpcServerDispatcher> interfaceDispatchers,
        uint publicRefs,
        Action? finalRelease)
    {
        ArgumentNullException.ThrowIfNull(interfaceDispatchers);
        lock (_lifetimeGate)
        {
            if (_objects.ContainsKey(ipid))
            {
                return false;
            }

            var entry = new ObjectEntry(interfaceDispatchers, AllocateMetadataUnderLock());
            if (!_objects.TryAdd(ipid, entry))
            {
                return false;
            }
            SeedLifetimeUnderLock(ipid, publicRefs, finalRelease);
            return true;
        }
    }

    /// <summary>
    /// Removes an object's registration (e.g., after RemoveGroup or
    /// the managed lifetime expires).
    /// </summary>
    /// <returns><see langword="true"/> if the IPID was present.</returns>
    public bool Unregister(Guid ipid)
    {
        Action? callback = null;
        bool removed;
        lock (_lifetimeGate)
        {
            _publicRefs.Remove(ipid);
            _finalReleaseCallbacks.Remove(ipid, out callback);
            removed = _objects.TryRemove(ipid, out _);
        }
        callback?.Invoke();
        return removed;
    }

    public bool AddPublicRefs(Guid ipid, uint publicRefs)
    {
        if (publicRefs == 0)
        {
            return Contains(ipid);
        }

        lock (_lifetimeGate)
        {
            if (!_objects.ContainsKey(ipid))
            {
                return false;
            }

            _publicRefs.TryGetValue(ipid, out uint current);
            _publicRefs[ipid] = unchecked(current + publicRefs);
            return true;
        }
    }

    public bool ReleasePublicRefs(Guid ipid, uint publicRefs)
    {
        if (publicRefs == 0)
        {
            return false;
        }

        Action? callback = null;
        bool removed = false;
        lock (_lifetimeGate)
        {
            if (!_objects.ContainsKey(ipid) || !_publicRefs.TryGetValue(ipid, out uint current) || publicRefs > current)
            {
                return false;
            }

            uint remaining = current - publicRefs;
            if (remaining != 0)
            {
                _publicRefs[ipid] = remaining;
                return false;
            }

            _publicRefs.Remove(ipid);
            _objects.TryRemove(ipid, out _);
            _finalReleaseCallbacks.Remove(ipid, out callback);
            removed = true;
        }
        callback?.Invoke();
        return removed;
    }

    private void SeedLifetimeUnderLock(Guid ipid, uint publicRefs, Action? finalRelease)
    {
        if (publicRefs != 0)
        {
            _publicRefs[ipid] = publicRefs;
        }
        if (finalRelease is not null)
        {
            _finalReleaseCallbacks[ipid] = finalRelease;
        }
    }

    /// <summary>
    /// Attempts to resolve the full interface-dispatcher map for an IPID.
    /// </summary>
    public bool TryGetInterfaceDispatchers(Guid ipid, out IReadOnlyDictionary<Guid, IOpcServerDispatcher> interfaceDispatchers)
    {
        if (TryGetObject(ipid, out interfaceDispatchers, out _))
        {
            return true;
        }

        interfaceDispatchers = null!;
        return false;
    }

    /// <summary>
    /// Attempts to resolve the stable exporter and object identifiers assigned
    /// when the IPID was registered.
    /// </summary>
    public bool TryGetObjectMetadata(Guid ipid, out OpcObjectMetadata metadata)
    {
        if (TryGetObject(ipid, out _, out metadata))
        {
            return true;
        }

        metadata = default;
        return false;
    }

    /// <summary>
    /// Attempts to resolve the dispatcher set and stable identity metadata from
    /// the same registered object entry.
    /// </summary>
    public bool TryGetObject(
        Guid ipid,
        out IReadOnlyDictionary<Guid, IOpcServerDispatcher> interfaceDispatchers,
        out OpcObjectMetadata metadata)
    {
        if (_objects.TryGetValue(ipid, out ObjectEntry? entry))
        {
            interfaceDispatchers = entry.InterfaceDispatchers;
            metadata = entry.Metadata;
            return true;
        }

        interfaceDispatchers = null!;
        metadata = default;
        return false;
    }

    /// <summary>
    /// Attempts to resolve a dispatcher for a specific (IPID, interface)
    /// pair.
    /// </summary>
    public bool TryGetDispatcher(Guid ipid, Guid interfaceId, out IOpcServerDispatcher dispatcher)
    {
        if (_objects.TryGetValue(ipid, out ObjectEntry? entry)
            && entry.InterfaceDispatchers.TryGetValue(interfaceId, out IOpcServerDispatcher? found))
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
        foreach (ObjectEntry entry in _objects.Values)
        {
            if (entry.InterfaceDispatchers.ContainsKey(interfaceId))
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

    private OpcObjectMetadata AllocateMetadataUnderLock()
    {
        if (_nextOid == ulong.MaxValue)
        {
            throw new InvalidOperationException("OpcObjectRegistry exhausted its OID space.");
        }

        return new OpcObjectMetadata(_oxid, ++_nextOid);
    }

    private static ulong CreateNonZeroId()
    {
        Span<byte> bytes = stackalloc byte[16];
        ulong value;
        do
        {
            Guid.NewGuid().TryWriteBytes(bytes);
            value = BitConverter.ToUInt64(bytes);
        }
        while (value == 0);

        return value;
    }

    private sealed record ObjectEntry(
        IReadOnlyDictionary<Guid, IOpcServerDispatcher> InterfaceDispatchers,
        OpcObjectMetadata Metadata);
}
