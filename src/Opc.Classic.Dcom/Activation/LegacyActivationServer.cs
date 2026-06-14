//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using Opc.Classic.Dcom.Core;

namespace Opc.Classic.Dcom.Activation;

/// <summary>
/// Bridges the legacy <c>IActivation::RemoteActivation</c> wire signature onto
/// the modern <see cref="RemoteSCMActivatorServer" /> activation logic. The
/// legacy call is decomposed into a per-IID sequence of class-object lookups
/// + activation results, all routed through the shared
/// <see cref="ClassFactoryRegistry" />.
/// </summary>
public sealed class LegacyActivationServer : IActivationServer
{
    private const uint ModeGetClassObject = 1;
    private static readonly int E_NOINTERFACE = global::Opc.Classic.OpcResultId.NoInterface.Code;
    private const uint AuthnHintPacketIntegrity = 5;
    private static readonly (ushort Major, ushort Minor) ServerComVersion = (5, 1);
    private readonly RemoteSCMActivatorServer _modernActivator;

    /// <summary>
    /// Initializes a new legacy activation server backed by the modern activator.
    /// </summary>
    public LegacyActivationServer(RemoteSCMActivatorServer modernActivator)
    {
        _modernActivator = modernActivator ?? throw new ArgumentNullException(nameof(modernActivator));
    }

    /// <inheritdoc />
    public async Task<int> RemoteActivationAsync(
        Guid clsid,
        Guid requestedIid,
        CancellationToken cancellationToken = default)
    {
        var response = await RemoteActivationAsync(
            new RemoteActivationRequest(
                Clsid: clsid,
                RequestedIids: new[] { requestedIid == Guid.Empty ? Guid.Parse(Opc.Classic.Dcom.Interfaces.IID_IUnknown) : requestedIid },
                ClientImpLevel: 2, // RPC_C_IMP_LEVEL_IDENTIFY
                Mode: 0,
                RequestedProtocolSequences: new ushort[] { 0x07 }), // ncacn_ip_tcp
            cancellationToken).ConfigureAwait(false);
        return response.Hresult;
    }

    /// <inheritdoc />
    public async Task<RemoteActivationResponse> RemoteActivationAsync(
        RemoteActivationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (request.RequestedIids.Count == 0)
        {
            return new RemoteActivationResponse(
                Hresult: E_NOINTERFACE,
                Oxid: Guid.Empty,
                IpidRemUnknown: Guid.Empty,
                AuthnHint: AuthnHintPacketIntegrity,
                ServerVersion: ServerComVersion,
                InterfaceResults: Array.Empty<RemoteActivationInterfaceResult>());
        }

        Guid primaryIid = request.RequestedIids[0];

        if (request.Mode == ModeGetClassObject)
        {
            var modernRequest = new RemoteGetClassObjectRequest(
                request.Clsid,
                primaryIid,
                request.RequestedProtocolSequences.Select(p => (int)p).ToArray());
            RemoteGetClassObjectResponse modernResponse = await _modernActivator
                .RemoteGetClassObjectAsync(modernRequest, cancellationToken).ConfigureAwait(false);
            return TranslateGetClassObjectResponse(modernResponse, request.RequestedIids);
        }
        else
        {
            var modernRequest = new RemoteCreateInstanceRequest(
                request.Clsid,
                primaryIid,
                request.RequestedProtocolSequences.Select(p => (int)p).ToArray());
            RemoteCreateInstanceResponse modernResponse = await _modernActivator
                .RemoteCreateInstanceAsync(modernRequest, cancellationToken).ConfigureAwait(false);
            return TranslateCreateInstanceResponse(modernResponse, request.RequestedIids);
        }
    }

    private static RemoteActivationResponse TranslateCreateInstanceResponse(
        RemoteCreateInstanceResponse modern,
        IReadOnlyList<Guid> requestedIids)
    {
        var perIid = new List<RemoteActivationInterfaceResult>(requestedIids.Count)
        {
            new(modern.Hresult, modern.ObjRef ?? Array.Empty<byte>()),
        };
        for (int i = 1; i < requestedIids.Count; i++)
        {
            perIid.Add(new RemoteActivationInterfaceResult(E_NOINTERFACE, ReadOnlyMemory<byte>.Empty));
        }

        return new RemoteActivationResponse(
            Hresult: modern.Hresult,
            Oxid: modern.Oxid,
            IpidRemUnknown: modern.Ipid,
            AuthnHint: AuthnHintPacketIntegrity,
            ServerVersion: ServerComVersion,
            InterfaceResults: perIid)
        {
            OxidBindings = modern.OxidBindings,
        };
    }

    private static RemoteActivationResponse TranslateGetClassObjectResponse(
        RemoteGetClassObjectResponse modern,
        IReadOnlyList<Guid> requestedIids)
    {
        var perIid = new List<RemoteActivationInterfaceResult>(requestedIids.Count)
        {
            new(modern.Hresult, modern.ObjRef ?? Array.Empty<byte>()),
        };
        for (int i = 1; i < requestedIids.Count; i++)
        {
            perIid.Add(new RemoteActivationInterfaceResult(E_NOINTERFACE, ReadOnlyMemory<byte>.Empty));
        }

        return new RemoteActivationResponse(
            Hresult: modern.Hresult,
            Oxid: modern.Oxid,
            IpidRemUnknown: modern.Ipid,
            AuthnHint: AuthnHintPacketIntegrity,
            ServerVersion: ServerComVersion,
            InterfaceResults: perIid)
        {
            OxidBindings = modern.OxidBindings,
        };
    }
}
