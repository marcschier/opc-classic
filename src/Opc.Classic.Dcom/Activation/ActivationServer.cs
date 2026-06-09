//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;

namespace Opc.Classic.Dcom.Activation;

/// <summary>Server-side dispatcher for legacy <c>IActivation::RemoteActivation</c>.</summary>
public sealed class ActivationServer : IRpcRequestContextDispatcher {
    private const int RemoteActivationOpnum = 0;
    private const int E_ACCESSDENIED = unchecked((int)0x80070005u);
    private const int E_INVALIDARG = unchecked((int)0x80070057u);
    private const OpcProtectionLevel RequiredActivationProtectionLevel = OpcProtectionLevel.Integrity;

    private static readonly Action<ILogger, OpcProtectionLevel, Exception?> AuthenticationRejected =
        LoggerMessage.Define<OpcProtectionLevel>(
            LogLevel.Warning,
            new EventId(1, nameof(AuthenticationRejected)),
            "IActivation::RemoteActivation rejected because RPC authentication is missing or level {ProtectionLevel} is below packet integrity.");

    private static readonly Action<ILogger, Exception?> MalformedRequest =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(2, nameof(MalformedRequest)),
            "IActivation::RemoteActivation request body was malformed.");

    private readonly IActivationServer _activationServer;
    private readonly ILogger _logger;

    /// <summary>Initializes a dispatcher backed by a decoded legacy activation server.</summary>
    public ActivationServer(IActivationServer activationServer, ILogger? logger = null) {
        _activationServer = activationServer ?? throw new ArgumentNullException(nameof(activationServer));
        _logger = logger ?? NullLogger.Instance;
    }

    /// <summary>Initializes a dispatcher backed by the modern activation implementation.</summary>
    public ActivationServer(RemoteSCMActivatorServer modernActivator, ILogger? logger = null)
        : this(new LegacyActivationServer(modernActivator ?? throw new ArgumentNullException(nameof(modernActivator))), logger) {
    }

    /// <summary>Initializes a dispatcher backed by managed class factories.</summary>
    public ActivationServer(ClassFactoryRegistry classFactories, ILogger? logger = null)
        : this(new RemoteSCMActivatorServer(classFactories ?? throw new ArgumentNullException(nameof(classFactories))), logger) {
    }

    /// <summary>Gets the legacy activation interface IID.</summary>
    public static Guid InterfaceId { get; } = Guid.Parse(Interfaces.IID_IActivation);

    /// <summary>Adds this dispatcher to an endpoint dispatcher registry.</summary>
    public static void Register(
        IDictionary<Guid, IOpcServerDispatcher> dispatchers,
        IActivationServer activationServer,
        ILogger? logger = null) {
        ArgumentNullException.ThrowIfNull(dispatchers);
        dispatchers[InterfaceId] = new ActivationServer(activationServer, logger);
    }

    /// <summary>Adds this dispatcher to an endpoint dispatcher registry.</summary>
    public static void Register(
        IDictionary<Guid, IOpcServerDispatcher> dispatchers,
        RemoteSCMActivatorServer modernActivator,
        ILogger? logger = null) {
        ArgumentNullException.ThrowIfNull(dispatchers);
        dispatchers[InterfaceId] = new ActivationServer(modernActivator, logger);
    }

    /// <summary>Dispatches <c>IActivation::RemoteActivation</c> from an already-decoded RPC request body.</summary>
    public static ValueTask<DispatchResult> DispatchRemoteActivationAsync(
        IActivationServer activationServer,
        ReadOnlyMemory<byte> requestPayload,
        OpcProtectionLevel protectionLevel,
        ILogger? logger = null,
        CancellationToken cancellationToken = default) =>
        DispatchRemoteActivationAsync(
            activationServer,
            requestPayload,
            protectionLevel >= RequiredActivationProtectionLevel,
            protectionLevel,
            logger,
            cancellationToken);

    /// <summary>Dispatches <c>IActivation::RemoteActivation</c> from an already-decoded RPC request body.</summary>
    public static async ValueTask<DispatchResult> DispatchRemoteActivationAsync(
        IActivationServer activationServer,
        ReadOnlyMemory<byte> requestPayload,
        bool isAuthenticated,
        OpcProtectionLevel protectionLevel,
        ILogger? logger = null,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(activationServer);
        logger ??= NullLogger.Instance;
        cancellationToken.ThrowIfCancellationRequested();

        // Legacy IActivation has a history of remote-code-execution CVEs (for example MS03-026).
        // Enforce the threat-model minimum for privileged activation before invoking class factories:
        // docs\security\THREAT_MODEL.md §1.5 and §3.1 require authenticated, integrity-protected activation.
        if (!isAuthenticated || protectionLevel < RequiredActivationProtectionLevel) {
            AuthenticationRejected(logger, protectionLevel, null);
            return DispatchResult.Fault(E_ACCESSDENIED);
        }

        RemoteActivationRequest request;
        try {
            request = IActivationCodec.DecodeRemoteActivationRequest(requestPayload.Span);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or OverflowException) {
            MalformedRequest(logger, ex);
            return DispatchResult.Fault(E_INVALIDARG);
        }

        RemoteActivationResponse response = await activationServer.RemoteActivationAsync(request, cancellationToken).ConfigureAwait(false);
        byte[] responsePayload = IActivationCodec.EncodeRemoteActivationResponse(response);
        return DispatchResult.Success(responsePayload);
    }

    /// <inheritdoc />
    public ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken = default) =>
        DispatchAsync(opnum, requestPayload, isAuthenticated: false, OpcProtectionLevel.None, cancellationToken);

    ValueTask<DispatchResult> IRpcRequestContextDispatcher.DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        RpcRequestContext requestContext,
        CancellationToken cancellationToken) =>
        DispatchAsync(
            opnum,
            requestPayload,
            requestContext.IsAuthenticated,
            requestContext.ProtectionLevel,
            cancellationToken);

    private ValueTask<DispatchResult> DispatchAsync(
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        bool isAuthenticated,
        OpcProtectionLevel protectionLevel,
        CancellationToken cancellationToken) {
        if (opnum != RemoteActivationOpnum) {
            return ValueTask.FromResult(DispatchResult.NotImplemented(opnum));
        }

        return DispatchRemoteActivationAsync(
            _activationServer,
            requestPayload,
            isAuthenticated,
            protectionLevel,
            _logger,
            cancellationToken);
    }
}
