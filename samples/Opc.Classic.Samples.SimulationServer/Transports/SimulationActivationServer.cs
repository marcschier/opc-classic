// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;
using Microsoft.Extensions.Logging;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Ae.Hosting;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Discovery.Dcom;
using Opc.Classic.Hda.Dcom;
using Opc.Classic.Hda.Hosting;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;
using Opc.Classic.Samples.SimulationServer.Ae;
using ActivationProperties = Opc.Classic.Dcom.Core.ActivationProperties;
using ActivationInfoCodec = Opc.Classic.Dcom.Core.ActivationInfoCodec;
using IRemoteSCMActivatorServer = Opc.Classic.Dcom.Core.IRemoteSCMActivatorServer;
using RemoteCreateInstanceRequest = Opc.Classic.Dcom.Core.RemoteCreateInstanceRequest;
using RemoteCreateInstanceResponse = Opc.Classic.Dcom.Core.RemoteCreateInstanceResponse;
using RemoteGetClassObjectRequest = Opc.Classic.Dcom.Core.RemoteGetClassObjectRequest;
using RemoteGetClassObjectResponse = Opc.Classic.Dcom.Core.RemoteGetClassObjectResponse;
using ScmReplyInfo = Opc.Classic.Dcom.Core.ScmReplyInfo;

namespace Opc.Classic.Samples.SimulationServer.Transports;

/// <summary>
/// Modern (NativeAOT, generated-dispatcher) server-side <c>IActivation</c> implementation that
/// cold-activates the simulation's DA server. On <c>RemoteActivation</c> for the DA CLSID it
/// registers the DA generated dispatchers in the shared <see cref="OpcObjectRegistry" /> (which
/// returns the IPID the activated object is reachable at) and returns an activation response
/// carrying that IPID. A client then routes subsequent ORPC calls to that IPID via the same
/// listener — the path the modern <c>dcom://</c> client (and, ultimately, a native explorer) uses.
/// </summary>
/// <remarks>
/// This deliberately does NOT use the legacy <c>RemoteSCMActivatorServer</c>/<c>ComOxidRuntime</c>
/// export path, which is reflection-based and serves objects from its own legacy socket runtime —
/// incompatible with the AOT generated dispatchers. The activation response carries a real
/// <c>OBJREF_STANDARD</c> (MS-DCOM §2.2.18.1) in <c>InterfaceResults[0]</c> whose STDOBJREF IPID is
/// the registry IPID, so the client locates the activated interface exactly as it does for a native
/// server (per MS-DCOM §3.2.4.1.2). The activation response carries the listener's
/// OXID-resolver string bindings so native clients can resolve the object's ORPC data port.
/// </remarks>
public sealed class SimulationActivationServer : IActivationServer, IRemoteSCMActivatorServer
{
    private const uint AuthnHintPacketIntegrity = 5;
    private static readonly (ushort Major, ushort Minor) ServerComVersion = (5, 1);
    private const int RegdbEClassNotReg = unchecked((int)0x80040154u);
    private static readonly int ENoInterface = global::Opc.Classic.OpcResultId.NoInterface.Code;
    private static readonly Guid IidIUnknown = Guid.Parse(Interfaces.IID_IUnknown);

    private readonly Guid _daClsid;
    private readonly Guid _aeClsid;
    private readonly Guid _hdaClsid;
    private readonly IClsidRegistry _clsidRegistry;
    private readonly SimDaHostServer _daServer;
    private readonly IOpcAeServer _aeServer;
    private readonly SimHdaHostServer _hdaServer;
    private readonly OpcObjectRegistry _objectRegistry;
    private readonly Func<IOpcInterfaceRef, IOPCEventSink>? _aeEventSinkFactory;
    private readonly Func<IPEndPoint?> _endpointProvider;
    private readonly Guid _remUnknownIpid;
    private readonly ILogger? _logger;

    /// <summary>Initializes a new instance of the <see cref="SimulationActivationServer" /> class.</summary>
    /// <param name="daClsid">CLSID this activator instantiates.</param>
    /// <param name="daServer">The managed DA server to expose on activation.</param>
    /// <param name="objectRegistry">Shared per-IPID object registry served by the host listener.</param>
    /// <param name="endpointProvider">Returns the listener's current TCP endpoint.</param>
    /// <param name="remUnknownIpid">IPID of the listener's registered <c>IRemUnknown</c>.</param>
    /// <param name="logger">Optional logger.</param>
    public SimulationActivationServer(
        Guid daClsid,
        SimDaHostServer daServer,
        OpcObjectRegistry objectRegistry,
        Guid? aeClsid = null,
        IOpcAeServer? aeServer = null,
        Guid? hdaClsid = null,
        SimHdaHostServer? hdaServer = null,
        IClsidRegistry? clsidRegistry = null,
        Func<IOpcInterfaceRef, IOPCEventSink>? aeEventSinkFactory = null,
        Func<IPEndPoint?>? endpointProvider = null,
        Guid? remUnknownIpid = null,
        ILogger? logger = null)
    {
        _daClsid = daClsid;
        _aeClsid = aeClsid ?? new Guid("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0002");
        _hdaClsid = hdaClsid ?? new Guid("D9A0B0C1-5E21-49C7-9C0E-2D7B6A1F0003");
        _clsidRegistry = clsidRegistry ?? CreateDefaultClsidRegistry(daClsid, _aeClsid, _hdaClsid);
        _daServer = daServer ?? throw new ArgumentNullException(nameof(daServer));
        _aeServer = aeServer ?? new SimAeHostServer(new SimulatedPlantModel());
        _hdaServer = hdaServer ?? new SimHdaHostServer(new SimulatedPlantModel());
        _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
        _aeEventSinkFactory = aeEventSinkFactory;
        _endpointProvider = endpointProvider ?? (() => null);
        _remUnknownIpid = EnsureRemUnknownRegistered(objectRegistry, remUnknownIpid ?? Guid.NewGuid());
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> RemoteActivationAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default)
    {
        Opc.Classic.Dcom.Activation.RemoteActivationResponse response = await RemoteActivationAsync(
            new Opc.Classic.Dcom.Activation.RemoteActivationRequest(
                Clsid: clsid,
                RequestedIids: new[] { requestedIid == Guid.Empty ? IidIUnknown : requestedIid },
                ClientImpLevel: 2,
                Mode: 0,
                RequestedProtocolSequences: new ushort[] { 0x07 }),
            cancellationToken).ConfigureAwait(false);
        return response.Hresult;
    }

    /// <inheritdoc />
    public Task<Opc.Classic.Dcom.Activation.RemoteActivationResponse> RemoteActivationAsync(
        Opc.Classic.Dcom.Activation.RemoteActivationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsKnownClsid(request.Clsid))
        {
            // MS-DCOM §3.1.2.5.2.3.1: on failure, pResults MUST contain one zeroed entry per
            // requested IID (not an empty array), so the client's per-IID decode succeeds and
            // surfaces the overall HRESULT.
            var failed = new RemoteActivationInterfaceResult[request.RequestedIids.Count];
            for (int i = 0; i < failed.Length; i++)
            {
                failed[i] = new RemoteActivationInterfaceResult(0, Array.Empty<byte>());
            }

            return Task.FromResult(new Opc.Classic.Dcom.Activation.RemoteActivationResponse(
                Hresult: RegdbEClassNotReg,
                Oxid: Guid.Empty,
                IpidRemUnknown: _remUnknownIpid,
                AuthnHint: AuthnHintPacketIntegrity,
                ServerVersion: ServerComVersion,
                InterfaceResults: failed)
            {
                OxidBindings = IObjectExporterDispatcher.EncodeDualStringArrayForListener(_endpointProvider()),
            });
        }

        ActivationExport export = Activate(request.Clsid, primaryIid: request.RequestedIids.Count > 0 ? request.RequestedIids[0] : IOPCServer.InterfaceId);

        // Every root interface backed by the activated object is registered under the
        // same object IPID. Return a native-style OBJREF for each supported requested IID so
        // activation and subsequent IRemUnknown::RemQueryInterface agree on reachability.
        int requested = Math.Max(1, request.RequestedIids.Count);
        var results = new RemoteActivationInterfaceResult[requested];
        for (int i = 0; i < requested; i++)
        {
            Guid iid = request.RequestedIids.Count == 0 ? IOPCServer.InterfaceId : request.RequestedIids[i];
            results[i] = export.SupportedIids.Contains(iid)
                ? new RemoteActivationInterfaceResult(0, EncodeStandardObjRef(iid, export.Oxid, export.Oid, export.Ipid, export.OxidBindings))
                : new RemoteActivationInterfaceResult(ENoInterface, Array.Empty<byte>());
        }

        return Task.FromResult(new Opc.Classic.Dcom.Activation.RemoteActivationResponse(
            Hresult: 0,
            Oxid: export.Oxid,
            IpidRemUnknown: _remUnknownIpid,
            AuthnHint: AuthnHintPacketIntegrity,
            ServerVersion: ServerComVersion,
            InterfaceResults: results)
        {
            OxidBindings = export.OxidBindings,
        });
    }

    /// <inheritdoc />
    public async Task<int> RemoteCreateInstanceAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default)
    {
        RemoteCreateInstanceResponse response = await RemoteCreateInstanceAsync(
            new RemoteCreateInstanceRequest(clsid, requestedIid, Array.Empty<int>()),
            cancellationToken).ConfigureAwait(false);
        return response.Hresult;
    }

    /// <inheritdoc />
    public async Task<int> RemoteGetClassObjectAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default)
    {
        RemoteGetClassObjectResponse response = await RemoteGetClassObjectAsync(
            new RemoteGetClassObjectRequest(clsid, requestedIid, Array.Empty<int>()),
            cancellationToken).ConfigureAwait(false);
        return response.Hresult;
    }

    /// <inheritdoc />
    public Task<RemoteCreateInstanceResponse> RemoteCreateInstanceAsync(
        RemoteCreateInstanceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsKnownClsid(request.Clsid))
        {
            return Task.FromResult(new RemoteCreateInstanceResponse(RegdbEClassNotReg, Guid.Empty, Guid.Empty, Array.Empty<byte>())
            {
                IpidRemUnknown = _remUnknownIpid,
                OxidBindings = IObjectExporterDispatcher.EncodeDualStringArrayForListener(_endpointProvider()),
                ActivationProperties = ResolveActivationProperties(request.ActivationProperties, request.RawActivationProperties),
            });
        }

        ActivationProperties activationProperties = ResolveActivationProperties(request.ActivationProperties, request.RawActivationProperties);
        Guid requestedIid = activationProperties.GetRequestedIidOr(request.RequestedIid == Guid.Empty ? IidIUnknown : request.RequestedIid);
        ActivationExport export = Activate(request.Clsid, requestedIid);
        ScmReplyInfo reply = new(0, export.Oxid, export.Oid, export.Ipid, export.ObjRef, copy: true);
        ActivationProperties responseProperties = activationProperties.WithScmReplyInfo(reply);
        return Task.FromResult(new RemoteCreateInstanceResponse(0, export.Oxid, export.Ipid, export.ObjRef)
        {
            Oid = export.Oid,
            IpidRemUnknown = _remUnknownIpid,
            ActivationProperties = responseProperties,
            EncodedActivationProperties = ActivationInfoCodec.Encode(responseProperties),
            OxidBindings = export.OxidBindings,
        });
    }

    /// <inheritdoc />
    public Task<RemoteGetClassObjectResponse> RemoteGetClassObjectAsync(
        RemoteGetClassObjectRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!IsKnownClsid(request.Clsid))
        {
            return Task.FromResult(new RemoteGetClassObjectResponse(RegdbEClassNotReg, Guid.Empty, Guid.Empty, Array.Empty<byte>())
            {
                IpidRemUnknown = _remUnknownIpid,
                OxidBindings = IObjectExporterDispatcher.EncodeDualStringArrayForListener(_endpointProvider()),
                ActivationProperties = ResolveActivationProperties(request.ActivationProperties, request.RawActivationProperties),
            });
        }

        ActivationProperties activationProperties = ResolveActivationProperties(request.ActivationProperties, request.RawActivationProperties);
        Guid requestedIid = request.RequestedIid == Guid.Empty ? IidIUnknown : request.RequestedIid;
        ActivationExport export = Activate(request.Clsid, requestedIid);
        ScmReplyInfo reply = new(0, export.Oxid, export.Oid, export.Ipid, export.ObjRef, copy: true);
        ActivationProperties responseProperties = activationProperties.WithScmReplyInfo(reply);
        return Task.FromResult(new RemoteGetClassObjectResponse(0, export.Oxid, export.Ipid, export.ObjRef)
        {
            Oid = export.Oid,
            IpidRemUnknown = _remUnknownIpid,
            ActivationProperties = responseProperties,
            EncodedActivationProperties = ActivationInfoCodec.Encode(responseProperties),
            OxidBindings = export.OxidBindings,
        });
    }

    // Builds an OBJREF_STANDARD (MEOW + STDOBJREF + DUALSTRINGARRAY) whose STDOBJREF IPID is the
    // registry IPID, encoded exactly as a native server would so the modern client decodes it via
    // OpcInterfaceRefCodec.Read and routes ORPC to that IPID.
    private ActivationExport Activate(Guid clsid, Guid primaryIid)
    {
        IReadOnlyDictionary<Guid, IOpcServerDispatcher> dispatchers = BuildDispatchers(clsid);
        Guid ipid = _objectRegistry.Register(dispatchers, publicRefs: 1);
        Guid oxid = Guid.NewGuid();
        Guid oid = Guid.NewGuid();
        byte[] oxidBindings = IObjectExporterDispatcher.EncodeDualStringArrayForListener(_endpointProvider());
        byte[] objRef = EncodeStandardObjRef(primaryIid, oxid, oid, ipid, oxidBindings);
        return new ActivationExport(oxid, oid, ipid, oxidBindings, objRef, dispatchers.Keys.ToArray());
    }

    private IReadOnlyDictionary<Guid, IOpcServerDispatcher> BuildDispatchers(Guid clsid)
    {
        if (clsid == _aeClsid)
        {
            IOpcAeServer effectiveAeServer = _aeServer is IAeServer
                ? new IAeServerToOpcAeServerAdapter(_aeServer, _aeEventSinkFactory)
                : _aeServer;
            var aeDispatcher = new OpcAeServerDispatcher(effectiveAeServer, _aeEventSinkFactory);
            IOpcServerDispatcher eventServerDispatcher = aeDispatcher.EventServerDispatcher;
            if (_aeServer is IAeServer aeServerForInterceptor)
            {
                eventServerDispatcher = new AeEventServerDispatcherInterceptor(
                    eventServerDispatcher,
                    aeServerForInterceptor,
                    _objectRegistry,
                    _logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance,
                    _aeEventSinkFactory);
            }
            return new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCEventServer.InterfaceId] = eventServerDispatcher,
                [OpcCommonClientProxy.InterfaceId] = aeDispatcher.CommonDispatcher,
                [Opc.Classic.Ae.Dcom.IConnectionPointContainer.InterfaceId] = new Opc.Classic.Ae.Dcom.IConnectionPointContainerServerDispatcher(aeDispatcher),
                [Opc.Classic.Ae.Dcom.IConnectionPoint.InterfaceId] = new Opc.Classic.Ae.Dcom.IConnectionPointServerDispatcher(aeDispatcher),
            };
        }

        if (clsid == _hdaClsid)
        {
            var hdaDispatcher = new OpcHdaServerDispatcher(_hdaServer);
            var dispatchers = new Dictionary<Guid, IOpcServerDispatcher>
            {
                [IOPCHDA_Server.InterfaceId] = hdaDispatcher.ServerDispatcher,
                [OpcCommonClientProxy.InterfaceId] = hdaDispatcher.CommonDispatcher,
            };
            if (_hdaServer is IOPCHDA_SyncRead syncRead)
            {
                dispatchers[IOPCHDA_SyncRead.InterfaceId] = new IOPCHDA_SyncReadServerDispatcher(syncRead);
            }
            if (_hdaServer is IOPCHDA_AsyncRead asyncRead)
            {
                dispatchers[IOPCHDA_AsyncRead.InterfaceId] = new IOPCHDA_AsyncReadServerDispatcher(asyncRead);
            }
            if (_hdaServer is Opc.Classic.Da.Dcom.IConnectionPointContainer connectionPointContainer)
            {
                dispatchers[Opc.Classic.Da.Dcom.IConnectionPointContainer.InterfaceId] = new Opc.Classic.Da.Dcom.IConnectionPointContainerServerDispatcher(connectionPointContainer);
            }
            if (_hdaServer is Opc.Classic.Da.Dcom.IConnectionPoint connectionPoint)
            {
                dispatchers[Opc.Classic.Da.Dcom.IConnectionPoint.InterfaceId] = new Opc.Classic.Da.Dcom.IConnectionPointServerDispatcher(connectionPoint);
            }

            return dispatchers;
        }

        if (clsid == OpcGuids.CLSID_OpcEnum)
        {
            var opcEnum = new OpcEnumServer(
                _clsidRegistry,
                _objectRegistry,
                () => IObjectExporterDispatcher.EncodeDualStringArrayForListener(_endpointProvider()));
            return new Dictionary<Guid, IOpcServerDispatcher>
            {
                [OpcGuids.IID_IOPCServerList] = new Opc.Classic.Discovery.Dcom.IOPCServerListServerDispatcher(opcEnum),
                [OpcGuids.IID_IOPCServerList2] = new Opc.Classic.Discovery.Dcom.IOPCServerList2ServerDispatcher(opcEnum),
            };
        }

        var daDispatcher = new OpcDaServerDispatcher(_daServer, _logger);
        IOpcAddressSpace addressSpace = _daServer.BuildAddressSpace();
        return new Dictionary<Guid, IOpcServerDispatcher>
        {
            [IOPCServer.InterfaceId] = daDispatcher.ServerDispatcher,
            [IOPCCommon.InterfaceId] = daDispatcher.CommonDispatcher,
            [IOPCBrowseServerAddressSpace.InterfaceId] = new IOPCBrowseServerAddressSpaceServerDispatcher(new DefaultBrowseServerAddressSpace(addressSpace)),
            [IOPCBrowse.InterfaceId] = new IOPCBrowseServerDispatcher(new DefaultBrowse(addressSpace)),
            [IOPCItemProperties.InterfaceId] = new IOPCItemPropertiesServerDispatcher(new DefaultItemProperties(NullItemPropertyProvider.Instance)),
            [IOPCItemIO.InterfaceId] = new IOPCItemIOServerDispatcher(new DefaultItemIO(_daServer)),
            [Opc.Classic.Da.Dcom.IConnectionPointContainer.InterfaceId] = daDispatcher.ConnectionPointContainerDispatcher,
            [Opc.Classic.Da.Dcom.IConnectionPoint.InterfaceId] = daDispatcher.ConnectionPointDispatcher,
        };
    }

    private bool IsKnownClsid(Guid clsid) => clsid == _daClsid || clsid == _aeClsid || clsid == _hdaClsid || clsid == OpcGuids.CLSID_OpcEnum;

    private static IClsidRegistry CreateDefaultClsidRegistry(Guid daClsid, Guid aeClsid, Guid hdaClsid)
    {
        var registry = new InMemoryClsidRegistry();
        registry.Register(new OpcClsidRegistration(
            daClsid,
            "Opc.Classic.Simulation.DA.1",
            "Opc.Classic.Samples.SimulationServer",
            typeof(SimDaHostServer).FullName!,
            "Opc.Classic Full-Feature Simulation Server (DA)",
            [OpcGuids.CATID_OPCDAServer20, OpcGuids.CATID_OPCDAServer30]));
        registry.Register(new OpcClsidRegistration(
            aeClsid,
            "Opc.Classic.Simulation.AE.1",
            "Opc.Classic.Samples.SimulationServer",
            typeof(SimAeHostServer).FullName!,
            "Opc.Classic Full-Feature Simulation Server (AE)",
            [OpcGuids.CATID_OPCAEServer10]));
        registry.Register(new OpcClsidRegistration(
            hdaClsid,
            "Opc.Classic.Simulation.HDA.1",
            "Opc.Classic.Samples.SimulationServer",
            typeof(SimHdaHostServer).FullName!,
            "Opc.Classic Full-Feature Simulation Server (HDA)",
            [OpcGuids.CATID_OPCHDAServer10]));
        return registry;
    }

    private static Guid EnsureRemUnknownRegistered(OpcObjectRegistry objectRegistry, Guid remUnknownIpid)
    {
        if (objectRegistry.Contains(remUnknownIpid))
        {
            return remUnknownIpid;
        }

        var remUnknown = new RemUnknownServerDispatcher(objectRegistry);
        var dispatchers = new Dictionary<Guid, IOpcServerDispatcher>
        {
            [RemUnknownServerDispatcher.InterfaceId] = remUnknown,
            [RemUnknownServerDispatcher.InterfaceId2] = remUnknown,
        };
        if (!objectRegistry.RegisterWithIpid(remUnknownIpid, dispatchers, publicRefs: 1))
        {
            throw new InvalidOperationException("The IRemUnknown IPID is already registered.");
        }

        return remUnknownIpid;
    }

    private static ActivationProperties ResolveActivationProperties(
        ActivationProperties activationProperties,
        byte[] rawActivationProperties)
    {
        if (rawActivationProperties.Length == 0)
        {
            return activationProperties ?? ActivationProperties.Empty;
        }

        return ActivationInfoCodec.TryDecode(rawActivationProperties, out ActivationProperties decoded)
            ? decoded
            : activationProperties ?? ActivationProperties.Empty;
    }

    private static byte[] EncodeStandardObjRef(Guid iid, Guid oxid, Guid oid, Guid ipid, ReadOnlyMemory<byte> oxidBindings)
    {
        (ushort securityOffset, ushort[] resolverBindings) = DecodeDualStringArray(oxidBindings.Span);
        var interfaceRef = new OpcInterfaceRef(
            iid,
            flags: 0,
            publicRefs: 1,
            oxid: UInt64FromGuid(oxid),
            oid: UInt64FromGuid(oid),
            ipid: ipid,
            securityOffset: securityOffset,
            resolverBindings: resolverBindings);

        var buffer = new byte[256 + resolverBindings.Length * sizeof(ushort)];
        var writer = new NdrWriter(buffer);
        OpcInterfaceRefCodec.Write(ref writer, interfaceRef);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static ulong UInt64FromGuid(Guid value)
    {
        Span<byte> bytes = stackalloc byte[16];
        bool ok = value.TryWriteBytes(bytes);
        if (!ok)
        {
            throw new InvalidOperationException("Guid.TryWriteBytes failed unexpectedly.");
        }

        return BitConverter.ToUInt64(bytes);
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

    private sealed record ActivationExport(Guid Oxid, Guid Oid, Guid Ipid, byte[] OxidBindings, byte[] ObjRef, IReadOnlyCollection<Guid> SupportedIids);
}
