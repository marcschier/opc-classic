// Copyright (c) 2026 marcschier. Licensed under the MIT License.

using Microsoft.Extensions.Logging;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Da.Hosting;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

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
/// server (per MS-DCOM §3.2.4.1.2). The OXID-resolver string bindings are left empty for now
/// (loopback clients reuse the activation endpoint); emitting a byte-correct DUALSTRINGARRAY
/// data-port for unmodified native clients is a follow-up.
/// </remarks>
public sealed class SimulationActivationServer : IActivationServer
{
    private const uint AuthnHintPacketIntegrity = 5;
    private static readonly (ushort Major, ushort Minor) ServerComVersion = (5, 1);
    private const int RegdbEClassNotReg = unchecked((int)0x80040154u);
    private static readonly int ENoInterface = global::Opc.Classic.OpcResultId.NoInterface.Code;
    private static readonly Guid IidIUnknown = Guid.Parse(Interfaces.IID_IUnknown);

    private readonly Guid _daClsid;
    private readonly SimDaHostServer _daServer;
    private readonly OpcObjectRegistry _objectRegistry;
    private readonly ILogger? _logger;

    /// <summary>Initializes a new instance of the <see cref="SimulationActivationServer" /> class.</summary>
    /// <param name="daClsid">CLSID this activator instantiates.</param>
    /// <param name="daServer">The managed DA server to expose on activation.</param>
    /// <param name="objectRegistry">Shared per-IPID object registry served by the host listener.</param>
    /// <param name="logger">Optional logger.</param>
    public SimulationActivationServer(
        Guid daClsid,
        SimDaHostServer daServer,
        OpcObjectRegistry objectRegistry,
        ILogger? logger = null)
    {
        _daClsid = daClsid;
        _daServer = daServer ?? throw new ArgumentNullException(nameof(daServer));
        _objectRegistry = objectRegistry ?? throw new ArgumentNullException(nameof(objectRegistry));
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<int> RemoteActivationAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default)
    {
        RemoteActivationResponse response = await RemoteActivationAsync(
            new RemoteActivationRequest(
                Clsid: clsid,
                RequestedIids: new[] { requestedIid == Guid.Empty ? IidIUnknown : requestedIid },
                ClientImpLevel: 2,
                Mode: 0,
                RequestedProtocolSequences: new ushort[] { 0x07 }),
            cancellationToken).ConfigureAwait(false);
        return response.Hresult;
    }

    /// <inheritdoc />
    public Task<RemoteActivationResponse> RemoteActivationAsync(
        RemoteActivationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (request.Clsid != _daClsid)
        {
            // MS-DCOM §3.1.2.5.2.3.1: on failure, pResults MUST contain one zeroed entry per
            // requested IID (not an empty array), so the client's per-IID decode succeeds and
            // surfaces the overall HRESULT.
            var failed = new RemoteActivationInterfaceResult[request.RequestedIids.Count];
            for (int i = 0; i < failed.Length; i++)
            {
                failed[i] = new RemoteActivationInterfaceResult(0, Array.Empty<byte>());
            }

            return Task.FromResult(new RemoteActivationResponse(
                Hresult: RegdbEClassNotReg,
                Oxid: Guid.Empty,
                IpidRemUnknown: Guid.Empty,
                AuthnHint: AuthnHintPacketIntegrity,
                ServerVersion: ServerComVersion,
                InterfaceResults: failed));
        }

        var dispatchers = new Dictionary<Guid, IOpcServerDispatcher>
        {
            [IOPCServer.InterfaceId] = new IOPCServerServerDispatcher(_daServer),
        };
        Guid ipid = _objectRegistry.Register(dispatchers);

        // The simulation only exposes IOPCServer. The primary requested IID gets a real
        // OBJREF_STANDARD carrying the registry IPID; any additional IIDs are reported as
        // E_NOINTERFACE (mirrors LegacyActivationServer.TranslateCreateInstanceResponse).
        Guid primaryIid = request.RequestedIids.Count > 0 ? request.RequestedIids[0] : IOPCServer.InterfaceId;
        byte[] objRef = EncodeStandardObjRef(primaryIid, ipid);

        int requested = Math.Max(1, request.RequestedIids.Count);
        var results = new RemoteActivationInterfaceResult[requested];
        results[0] = new RemoteActivationInterfaceResult(0, objRef);
        for (int i = 1; i < requested; i++)
        {
            results[i] = new RemoteActivationInterfaceResult(ENoInterface, Array.Empty<byte>());
        }

        return Task.FromResult(new RemoteActivationResponse(
            Hresult: 0,
            Oxid: Guid.NewGuid(),
            IpidRemUnknown: ipid,
            AuthnHint: AuthnHintPacketIntegrity,
            ServerVersion: ServerComVersion,
            InterfaceResults: results));
    }

    // Builds an OBJREF_STANDARD (MEOW + STDOBJREF + DUALSTRINGARRAY) whose STDOBJREF IPID is the
    // registry IPID, encoded exactly as a native server would so the modern client decodes it via
    // OpcInterfaceRefCodec.Read and routes ORPC to that IPID. Resolver bindings are empty for the
    // loopback case (the client reuses the activation endpoint).
    private static byte[] EncodeStandardObjRef(Guid iid, Guid ipid)
    {
        var interfaceRef = new OpcInterfaceRef(
            iid,
            flags: 0,
            publicRefs: 1,
            oxid: BitConverter.ToUInt64(Guid.NewGuid().ToByteArray(), 0),
            oid: BitConverter.ToUInt64(Guid.NewGuid().ToByteArray(), 0),
            ipid: ipid,
            securityOffset: 0,
            resolverBindings: Array.Empty<ushort>());

        var buffer = new byte[128];
        var writer = new NdrWriter(buffer);
        OpcInterfaceRefCodec.Write(ref writer, interfaceRef);
        return buffer.AsSpan(0, writer.Position).ToArray();
    }
}
