// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Discovery.Dcom;

/// <summary>
/// Managed OPCEnum server-list object backed by the cross-platform CLSID registry.
/// </summary>
public sealed class OpcEnumServer
{
    private static readonly OpcResultId ClassNotRegistered = new(unchecked((int)0x80040154u), "REGDB_E_CLASSNOTREG");
    private static readonly OpcResultId ClassStringInvalid = new(unchecked((int)0x800401F3u), "CO_E_CLASSSTRING");

    private readonly IClsidRegistry _clsidRegistry;
    private readonly OpcObjectRegistry _objectRegistry;
    private readonly Func<ReadOnlyMemory<byte>> _oxidBindingsProvider;

    /// <summary>
    /// Initializes a managed OPCEnum object.
    /// </summary>
    public OpcEnumServer(
        IClsidRegistry clsidRegistry,
        OpcObjectRegistry objectRegistry,
        Func<ReadOnlyMemory<byte>>? oxidBindingsProvider = null)
    {
        _clsidRegistry = clsidRegistry ?? throw new ArgumentNullException(nameof(clsidRegistry));
        _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
        _oxidBindingsProvider = oxidBindingsProvider ?? (() => ReadOnlyMemory<byte>.Empty);
    }

    /// <summary>
    /// Resolves a ProgID to its registered CLSID.
    /// </summary>
    public Guid ClsidFromProgId(string progId)
    {
        if (string.IsNullOrWhiteSpace(progId) || !_clsidRegistry.TryResolveProgId(progId, out OpcClsidRegistration registration))
        {
            throw new OpcException(ClassStringInvalid);
        }

        return registration.Clsid;
    }

    /// <summary>
    /// Returns ProgID, friendly name, and version-independent ProgID for a CLSID.
    /// </summary>
    public OpcEnumClassDetails GetClassDetails(Guid clsid)
    {
        if (!_clsidRegistry.TryResolve(clsid, out OpcClsidRegistration registration))
        {
            throw new OpcException(ClassNotRegistered);
        }

        return new OpcEnumClassDetails(
            registration.ProgId,
            string.IsNullOrWhiteSpace(registration.FriendlyName) ? registration.ProgId : registration.FriendlyName,
            DeriveVersionIndependentProgId(registration.ProgId));
    }

    /// <summary>
    /// Creates and registers a routable GUID enumerator for classes matching the category filters.
    /// </summary>
    public IOpcInterfaceRef EnumClassesOfCategories(Guid[] implementedCategories, Guid[] requiredCategories, Guid enumeratorIid)
    {
        Guid[] classIds = EnumerateClassIds(implementedCategories, requiredCategories);
        var enumerator = new OpcEnumGuidServer(classIds, enumeratorIid, CloneEnumerator);
        var dispatcher = new IOPCEnumGUIDServerDispatcher(enumerator);
        var dispatchers = new Dictionary<Guid, IOpcServerDispatcher>
        {
            [OpcGuids.IID_IOPCEnumGUID] = dispatcher,
            [OpcGuids.IID_IEnumGUID] = dispatcher,
        };
        Guid ipid = _objectRegistry.Register(dispatchers, publicRefs: 1);
        return CreateInterfaceRef(enumeratorIid, ipid);
    }

    private Guid[] EnumerateClassIds(Guid[] implementedCategories, Guid[] requiredCategories)
    {
        Guid[] implemented = implementedCategories ?? Array.Empty<Guid>();
        Guid[] required = requiredCategories ?? Array.Empty<Guid>();
        var results = new List<OpcClsidRegistration>();

        foreach (OpcClsidRegistration registration in _clsidRegistry.Enumerate())
        {
            IReadOnlyList<Guid> categories = registration.ImplementedCategories ?? Array.Empty<Guid>();
            if (categories.Count == 0)
            {
                continue;
            }

            if (ContainsAll(categories, implemented) && ContainsAll(categories, required))
            {
                results.Add(registration);
            }
        }

        results.Sort(static (left, right) => string.Compare(left.ProgId, right.ProgId, StringComparison.OrdinalIgnoreCase));
        var classIds = new Guid[results.Count];
        for (int i = 0; i < results.Count; i++)
        {
            classIds[i] = results[i].Clsid;
        }

        return classIds;
    }

    private IOpcInterfaceRef CloneEnumerator(IReadOnlyList<Guid> classIds, int index, Guid interfaceId)
    {
        var clone = new OpcEnumGuidServer(classIds, interfaceId, CloneEnumerator, index);
        var dispatcher = new IOPCEnumGUIDServerDispatcher(clone);
        Guid ipid = _objectRegistry.Register(new Dictionary<Guid, IOpcServerDispatcher>
        {
            [OpcGuids.IID_IOPCEnumGUID] = dispatcher,
            [OpcGuids.IID_IEnumGUID] = dispatcher,
        }, publicRefs: 1);
        return CreateInterfaceRef(interfaceId, ipid);
    }

    private IOpcInterfaceRef CreateInterfaceRef(Guid iid, Guid ipid)
    {
        (ushort securityOffset, ushort[] bindings) = DecodeDualStringArray(_oxidBindingsProvider().Span);
        return new OpcInterfaceRef(
            iid,
            flags: 0,
            publicRefs: 1,
            oxid: 1,
            oid: 1,
            ipid: ipid,
            securityOffset: securityOffset,
            resolverBindings: bindings);
    }

    private static bool ContainsAll(IReadOnlyList<Guid> categories, ReadOnlySpan<Guid> required)
    {
        for (int i = 0; i < required.Length; i++)
        {
            bool found = false;
            for (int j = 0; j < categories.Count; j++)
            {
                if (categories[j] == required[i])
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                return false;
            }
        }

        return true;
    }

    private static string DeriveVersionIndependentProgId(string progId)
    {
        int lastDot = progId.LastIndexOf('.');
        if (lastDot <= 0 || lastDot == progId.Length - 1)
        {
            return progId;
        }

        ReadOnlySpan<char> suffix = progId.AsSpan(lastDot + 1);
        for (int i = 0; i < suffix.Length; i++)
        {
            if (!char.IsAsciiDigit(suffix[i]))
            {
                return progId;
            }
        }

        return progId[..lastDot];
    }

    private static (ushort SecurityOffset, ushort[] Bindings) DecodeDualStringArray(ReadOnlySpan<byte> dualStringArray)
    {
        if (dualStringArray.Length < 4)
        {
            return (0, Array.Empty<ushort>());
        }

        var reader = new NdrReader(dualStringArray);
        ushort entryCount = reader.ReadUInt16();
        ushort securityOffset = reader.ReadUInt16();
        var bindings = new ushort[entryCount];
        for (int i = 0; i < bindings.Length; i++)
        {
            bindings[i] = reader.ReadUInt16();
        }

        return (securityOffset, bindings);
    }
}
