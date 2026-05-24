//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Opc.Classic.Generators;

namespace SharpInterop.Core;

/// <summary>Source-generated proxy/dispatcher surface for IRemoteSCMActivator.</summary>
[OpcInterface(SharpInterop.Interfaces.IID_IRemoteSCMActivator)]
[GenerateOpcProxy]
[OpcGenerateServerDispatch]
public partial interface IRemoteSCMActivator {
    /// <summary>IRemoteSCMActivator::RemoteGetClassObject (opnum 3).</summary>
    [OpcMethod(3)]
    Task<int> RemoteGetClassObjectAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default);

    /// <summary>IRemoteSCMActivator::RemoteCreateInstance (opnum 4).</summary>
    [OpcMethod(4)]
    Task<int> RemoteCreateInstanceAsync(Guid clsid, Guid requestedIid, CancellationToken cancellationToken = default);
}

/// <summary>Server-side contract for IRemoteSCMActivator activation handling.</summary>
public interface IRemoteSCMActivatorServer : IRemoteSCMActivator {
    /// <summary>Handles a decoded RemoteCreateInstance request.</summary>
    Task<RemoteCreateInstanceResponse> RemoteCreateInstanceAsync(
        RemoteCreateInstanceRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Handles a decoded RemoteGetClassObject request.</summary>
    Task<RemoteGetClassObjectResponse> RemoteGetClassObjectAsync(
        RemoteGetClassObjectRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>Decoded server-side IRemoteSCMActivator::RemoteCreateInstance request fields.</summary>
public sealed record RemoteCreateInstanceRequest(
    Guid Clsid,
    Guid RequestedIid,
    IReadOnlyList<int> ProtocolSequences) {
    /// <summary>Decoded activation properties supplied by the client.</summary>
    public ActivationProperties ActivationProperties { get; init; } = ActivationProperties.Empty;

    /// <summary>Raw activation property payload, when the caller has not decoded it yet.</summary>
    public byte[] RawActivationProperties { get; init; } = Array.Empty<byte>();
}

/// <summary>Server-side IRemoteSCMActivator::RemoteCreateInstance response.</summary>
public sealed record RemoteCreateInstanceResponse(int Hresult, Guid Oxid, Guid Ipid, byte[] ObjRef) {
    /// <summary>Object identifier allocated for the exported object.</summary>
    public Guid Oid { get; init; }

    /// <summary>Activation properties returned to the client.</summary>
    public ActivationProperties ActivationProperties { get; init; } = ActivationProperties.Empty;

    /// <summary>Encoded activation properties returned to the client.</summary>
    public byte[] EncodedActivationProperties { get; init; } = Array.Empty<byte>();
}

/// <summary>Decoded server-side IRemoteSCMActivator::RemoteGetClassObject request fields.</summary>
public sealed record RemoteGetClassObjectRequest(
    Guid Clsid,
    Guid RequestedIid,
    IReadOnlyList<int> ProtocolSequences) {
    /// <summary>Decoded activation properties supplied by the client.</summary>
    public ActivationProperties ActivationProperties { get; init; } = ActivationProperties.Empty;

    /// <summary>Raw activation property payload, when the caller has not decoded it yet.</summary>
    public byte[] RawActivationProperties { get; init; } = Array.Empty<byte>();
}

/// <summary>Server-side IRemoteSCMActivator::RemoteGetClassObject response.</summary>
public sealed record RemoteGetClassObjectResponse(int Hresult, Guid Oxid, Guid Ipid, byte[] ObjRef) {
    /// <summary>Object identifier allocated for the exported class factory.</summary>
    public Guid Oid { get; init; }

    /// <summary>Activation properties returned to the client.</summary>
    public ActivationProperties ActivationProperties { get; init; } = ActivationProperties.Empty;

    /// <summary>Encoded activation properties returned to the client.</summary>
    public byte[] EncodedActivationProperties { get; init; } = Array.Empty<byte>();
}
