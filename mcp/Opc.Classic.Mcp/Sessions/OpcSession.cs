//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Concurrent;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dx;
using Opc.Classic.Xml;

namespace Opc.Classic.Mcp.Sessions;

/// <summary>Holds MCP session state and per-OPC-spec client state.</summary>
public sealed class OpcSession : IAsyncDisposable {
    private bool _disposed;

    /// <summary>Creates a new session with the requested idle expiry.</summary>
    public OpcSession(string sessionId, TimeSpan idleExpiry) {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        if (idleExpiry <= TimeSpan.Zero) {
            throw new ArgumentOutOfRangeException(nameof(idleExpiry), idleExpiry, "Idle expiry must be positive.");
        }

        SessionId = sessionId;
        IdleExpiry = idleExpiry;
        CreatedAt = DateTimeOffset.UtcNow;
        LastUsedAt = CreatedAt;
    }

    /// <summary>Opaque session identifier passed to subsequent tools.</summary>
    public string SessionId { get; }

    /// <summary>UTC time at which the session was created.</summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>UTC time at which the session was last used.</summary>
    public DateTimeOffset LastUsedAt { get; private set; }

    /// <summary>Idle expiry duration after the last tool use.</summary>
    public TimeSpan IdleExpiry { get; }

    /// <summary>Per-session OPC DA client state.</summary>
    public DaClientState? DaClient { get; set; }

    /// <summary>Per-session OPC AE client state.</summary>
    public AeClientState? AeClient { get; set; }

    /// <summary>Per-session OPC HDA client state.</summary>
    public HdaClientState? HdaClient { get; set; }

    /// <summary>Per-session OPC Batch client state.</summary>
    public BatchClientState? BatchClient { get; set; }

    /// <summary>Per-session OPC Commands client state.</summary>
    public CommandsClientState? CommandsClient { get; set; }

    /// <summary>Per-session OPC Complex Data client state.</summary>
    public CpxClientState? CpxClient { get; set; }

    /// <summary>Per-session OPC DX client state.</summary>
    public DxClientState? DxClient { get; set; }

    /// <summary>Per-session OPC Security client state.</summary>
    public SecurityClientState? SecurityClient { get; set; }

    /// <summary>Per-session OPC XML-DA client state.</summary>
    public XmlDaClientState? XmlDaClient { get; set; }

    /// <summary>Returns true when the session has exceeded its idle expiry.</summary>
    public bool IsExpired(DateTimeOffset now) => now - LastUsedAt >= IdleExpiry;

    /// <summary>Updates the last-used timestamp.</summary>
    public void Touch() => LastUsedAt = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        if (_disposed) {
            return;
        }

        _disposed = true;
        DaClientState? daClient = DaClient;
        AeClientState? aeClient = AeClient;
        HdaClientState? hdaClient = HdaClient;
        BatchClientState? batchClient = BatchClient;
        CommandsClientState? commandsClient = CommandsClient;
        CpxClientState? cpxClient = CpxClient;
        DxClientState? dxClient = DxClient;
        SecurityClientState? securityClient = SecurityClient;
        XmlDaClientState? xmlDaClient = XmlDaClient;
        DaClient = null;
        AeClient = null;
        HdaClient = null;
        BatchClient = null;
        CommandsClient = null;
        CpxClient = null;
        DxClient = null;
        SecurityClient = null;
        XmlDaClient = null;
        if (cpxClient is not null) {
            await cpxClient.DisposeAsync().ConfigureAwait(false);
        }

        if (daClient is not null) {
            await daClient.DisposeAsync().ConfigureAwait(false);
        }

        if (aeClient is not null) {
            await aeClient.DisposeAsync().ConfigureAwait(false);
        }

        if (hdaClient is not null) {
            await hdaClient.DisposeAsync().ConfigureAwait(false);
        }

        if (batchClient is not null) {
            await batchClient.DisposeAsync().ConfigureAwait(false);
        }

        if (commandsClient is not null) {
            await commandsClient.DisposeAsync().ConfigureAwait(false);
        }

        if (dxClient is not null) {
            await dxClient.DisposeAsync().ConfigureAwait(false);
        }

        if (securityClient is not null) {
            await securityClient.DisposeAsync().ConfigureAwait(false);
        }

        if (xmlDaClient is not null) {
            await xmlDaClient.DisposeAsync().ConfigureAwait(false);
        }
    }
}

/// <summary>Holds OPC DA wire proxies and server-side group/subscription handles.</summary>
public sealed class DaClientState : IAsyncDisposable {
    private readonly ICallChannel _channel;
    private readonly bool _ownsChannel;
    private bool _disposed;

    /// <summary>Creates DA client state over an existing call channel.</summary>
    public DaClientState(string host, string? progId, Guid? clsid, ICallChannel channel, bool ownsChannel) {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(channel);

        Host = host;
        ProgId = progId;
        Clsid = clsid;
        _channel = channel;
        _ownsChannel = ownsChannel;
        Server = new IOPCServerClientProxy(channel);
        Browse = new IOPCBrowseClientProxy(channel);
        ItemProperties = new IOPCItemPropertiesClientProxy(channel);
        ItemIo = new IOPCItemIOClientProxy(channel);
        ItemMgt = new IOPCItemMgtClientProxy(channel);
        SyncIo = new IOPCSyncIOClientProxy(channel);
        AsyncIo2 = new IOPCAsyncIO2ClientProxy(channel);
        GroupState = new IOPCGroupStateMgtClientProxy(channel);
        GroupState2 = new IOPCGroupStateMgt2ClientProxy(channel);
        ConnectionPoint = new IConnectionPointClientProxy(channel);
    }

    /// <summary>Underlying DCOM call channel used by optional companion interfaces.</summary>
    internal ICallChannel CallChannel => _channel;

    /// <summary>Target host.</summary>
    public string Host { get; }

    /// <summary>Connected DA server ProgID, if known.</summary>
    public string? ProgId { get; }

    /// <summary>Connected DA server CLSID, if known.</summary>
    public Guid? Clsid { get; }

    /// <summary>Top-level DA server proxy.</summary>
    public IOPCServerClientProxy Server { get; }

    /// <summary>DA 3.0 browse proxy.</summary>
    public IOPCBrowseClientProxy Browse { get; }

    /// <summary>DA 2.x item properties proxy.</summary>
    public IOPCItemPropertiesClientProxy ItemProperties { get; }

    /// <summary>DA 3.0 stateless item I/O proxy.</summary>
    public IOPCItemIOClientProxy ItemIo { get; }

    /// <summary>Group item-management proxy.</summary>
    public IOPCItemMgtClientProxy ItemMgt { get; }

    /// <summary>Synchronous DA I/O proxy.</summary>
    public IOPCSyncIOClientProxy SyncIo { get; }

    /// <summary>Asynchronous DA I/O proxy.</summary>
    public IOPCAsyncIO2ClientProxy AsyncIo2 { get; }

    /// <summary>Group state proxy.</summary>
    public IOPCGroupStateMgtClientProxy GroupState { get; }

    /// <summary>DA 3.0 group state proxy.</summary>
    public IOPCGroupStateMgt2ClientProxy GroupState2 { get; }

    /// <summary>Connection point proxy for callback setup.</summary>
    public IConnectionPointClientProxy ConnectionPoint { get; }

    /// <summary>Known groups by server handle.</summary>
    public ConcurrentDictionary<int, DaGroupContext> Groups { get; } = new();

    /// <summary>Known poll-based subscriptions by identifier.</summary>
    public ConcurrentDictionary<string, DaSubscriptionContext> Subscriptions { get; } = new(StringComparer.Ordinal);

    /// <summary>
    /// Loopback inbound <c>IOPCDataCallback</c> endpoint (Track AU scaffold).
    /// Lazily created on first use via <see cref="GetOrCreateCallbackEndpointAsync"/>.
    /// Disposed by <see cref="DisposeAsync"/>. Not auto-started by Subscribe;
    /// production callback bring-up against real OPC servers needs
    /// <c>IObjectExporter</c> OXID-resolution support which is tracked under
    /// AP1/AP2/AP4 — see <c>docs/interop/da-callbacks.md</c>.
    /// </summary>
    public Tools.DaCallbackEndpoint? CallbackEndpoint { get; private set; }

    /// <summary>
    /// Lazily creates and starts a loopback <see cref="Tools.DaCallbackEndpoint"/>
    /// for this client. Subsequent calls return the same instance. Concurrent
    /// callers race-safely; only one endpoint is ever created per client.
    /// </summary>
    public async Task<Tools.DaCallbackEndpoint> GetOrCreateCallbackEndpointAsync(System.Threading.CancellationToken cancellationToken = default) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        Tools.DaCallbackEndpoint? existing = CallbackEndpoint;
        if (existing is not null) {
            return existing;
        }

        await _callbackEndpointLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (CallbackEndpoint is null) {
                var endpoint = new Tools.DaCallbackEndpoint();
                await endpoint.StartAsync(cancellationToken).ConfigureAwait(false);
                CallbackEndpoint = endpoint;
            }

            return CallbackEndpoint;
        }
        finally {
            _callbackEndpointLock.Release();
        }
    }

    private readonly System.Threading.SemaphoreSlim _callbackEndpointLock = new(1, 1);

    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        if (_disposed) {
            return;
        }

        // Track BI: tear down each Advise cookie + sink registration before
        // disposing the underlying sinks. Best-effort: the channel may be
        // already half-closed (connection dropped, server crashed) so we
        // swallow OpcException / InvalidOperationException and proceed to
        // local cleanup. The sink Dispose() loop below still runs even if
        // remote Unadvise fails.
        foreach (DaSubscriptionContext subscription in Subscriptions.Values) {
            if (subscription.AdviseCookie is int cookie) {
                try {
                    await ConnectionPoint.UnadviseAsync(cookie).ConfigureAwait(false);
                }
                catch (OpcException) {
                }
                catch (InvalidOperationException) {
                }

                subscription.AdviseCookie = null;
            }

            if (subscription.SinkIpid != Guid.Empty) {
                CallbackEndpoint?.UnregisterSink(subscription.SinkIpid);
                subscription.SinkIpid = Guid.Empty;
            }

            subscription.Sink.Dispose();
        }

        Subscriptions.Clear();

        // Serialize disposal with concurrent GetOrCreateCallbackEndpointAsync —
        // otherwise a creator inside the lock could publish a freshly-started
        // endpoint into a disposed DaClientState. We deliberately do NOT
        // dispose the semaphore: outstanding waiters may still need to
        // Release on it after their startup raced with disposal.
        await _callbackEndpointLock.WaitAsync().ConfigureAwait(false);
        Tools.DaCallbackEndpoint? callbackEndpoint;
        try {
            _disposed = true;
            callbackEndpoint = CallbackEndpoint;
            CallbackEndpoint = null;
        }
        finally {
            _callbackEndpointLock.Release();
        }

        if (callbackEndpoint is not null) {
            await callbackEndpoint.DisposeAsync().ConfigureAwait(false);
        }

        if (_ownsChannel) {
            switch (_channel) {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }
}

/// <summary>Tracks a DA group created in a session.</summary>
public sealed class DaGroupContext {
    /// <summary>Creates a group context.</summary>
    public DaGroupContext(
        int serverGroupHandle,
        string? name,
        int clientHandle,
        bool active,
        int updateRateMs,
        int revisedUpdateRateMs,
        int timeBiasMinutes,
        float deadbandPercent,
        int localeId,
        int keepAliveMs,
        IReadOnlyDictionary<Guid, Guid>? interfaceIpids = null) {
        ServerGroupHandle = serverGroupHandle;
        Name = name;
        ClientHandle = clientHandle;
        Active = active;
        UpdateRateMs = updateRateMs;
        RevisedUpdateRateMs = revisedUpdateRateMs;
        TimeBiasMinutes = timeBiasMinutes;
        DeadbandPercent = deadbandPercent;
        LocaleId = localeId;
        KeepAliveMs = keepAliveMs;
        InterfaceIpids = interfaceIpids is null
            ? new Dictionary<Guid, Guid>()
            : new Dictionary<Guid, Guid>(interfaceIpids);
    }

    /// <summary>Server-assigned group handle.</summary>
    public int ServerGroupHandle { get; }

    /// <summary>Group name.</summary>
    public string? Name { get; }

    /// <summary>Client group handle.</summary>
    public int ClientHandle { get; }

    /// <summary>Whether the group is active.</summary>
    public bool Active { get; }

    /// <summary>Requested update rate.</summary>
    public int UpdateRateMs { get; }

    /// <summary>Server-revised update rate.</summary>
    public int RevisedUpdateRateMs { get; }

    /// <summary>Time bias in minutes.</summary>
    public int TimeBiasMinutes { get; }

    /// <summary>Deadband percentage.</summary>
    public float DeadbandPercent { get; }

    /// <summary>Locale identifier.</summary>
    public int LocaleId { get; }

    /// <summary>Keep-alive interval.</summary>
    public int KeepAliveMs { get; }

    /// <summary>Per-interface IPIDs returned for this group object.</summary>
    internal IReadOnlyDictionary<Guid, Guid> InterfaceIpids { get; }

    /// <summary>Known items in the group by server handle.</summary>
    public ConcurrentDictionary<int, DaItemBindingContext> Items { get; } = new();
}

/// <summary>Tracks a DA item binding returned by AddItems.</summary>
public sealed record DaItemBindingContext(string ItemName, string? ItemPath, int ClientHandle, int ServerHandle);

/// <summary>Tracks a poll-based DA subscription.</summary>
/// <remarks>
/// The <see cref="Sink"/> is constructed eagerly even though the production
/// callback bind path (Track AP1/AP2) is not yet wired. This makes the
/// queue-drain contract used by <c>opcclassic.da.poll_subscription</c>
/// testable in isolation and a no-op for live polling until a sink producer
/// is plumbed.
/// </remarks>
public sealed class DaSubscriptionContext {
    /// <summary>Creates a new subscription context with a fresh callback sink.</summary>
    public DaSubscriptionContext(
        string subscriptionId,
        int groupHandle,
        bool fromCache,
        int transactionId,
        int? cancelId) {
        ArgumentException.ThrowIfNullOrEmpty(subscriptionId);
        SubscriptionId = subscriptionId;
        GroupHandle = groupHandle;
        FromCache = fromCache;
        TransactionId = transactionId;
        CancelId = cancelId;
        Sink = new Tools.DaDataCallbackSink();
    }

    /// <summary>Opaque subscription identifier returned to the caller.</summary>
    public string SubscriptionId { get; }

    /// <summary>Server-assigned group handle this subscription targets.</summary>
    public int GroupHandle { get; }

    /// <summary>True when the subscription polls from the server cache; false for device reads.</summary>
    public bool FromCache { get; }

    /// <summary>Transaction identifier used for refresh callbacks.</summary>
    public int TransactionId { get; }

    /// <summary>Cancel identifier returned by <c>IOPCAsyncIO2::Refresh2</c>, if any.</summary>
    public int? CancelId { get; }

    /// <summary>Sink that receives <c>IOPCDataCallback</c> push notifications for this subscription.</summary>
    public Tools.DaDataCallbackSink Sink { get; }

    /// <summary>
    /// IPID under which <see cref="Sink"/> is registered with the
    /// <c>DaCallbackEndpoint</c> object registry. <see cref="Guid.Empty"/>
    /// when the subscription has not yet completed
    /// <c>IConnectionPoint::Advise</c> (loopback-only test mode or
    /// pre-Track-BI fallback).
    /// </summary>
    public Guid SinkIpid { get; set; }

    /// <summary>
    /// Cookie returned by <c>IConnectionPoint::Advise</c> when the
    /// subscription's <see cref="Sink"/> was registered with the OPC
    /// server. Null when no successful Advise call has been made
    /// (sink not yet advised, or pre-Track-BI poll-only path). Used by
    /// <c>RemoveGroup</c> / <c>DisposeAsync</c> to call
    /// <c>IConnectionPoint::Unadvise(cookie)</c>.
    /// </summary>
    public int? AdviseCookie { get; set; }
}

/// <summary>Operations required by MCP DX tools.</summary>
public interface IOpcDxClient : IAsyncDisposable {
    /// <summary>Gets DX server status.</summary>
    Task<OpcServerStatus> GetStatusAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists DX connection names.</summary>
    Task<IReadOnlyList<string>> QueryConnectionNamesAsync(string browsePath, IReadOnlyList<string> connectionMasks, bool recursive, CancellationToken cancellationToken = default);

    /// <summary>Lists configured source servers.</summary>
    Task<IReadOnlyList<DxSourceServer>> QuerySourceServersAsync(CancellationToken cancellationToken = default);

    /// <summary>Adds a DX connection.</summary>
    Task<OpcResultId> AddConnectionAsync(DxConnection connection, CancellationToken cancellationToken = default);

    /// <summary>Modifies a DX connection.</summary>
    Task<OpcResultId> ModifyConnectionAsync(DxConnection connection, CancellationToken cancellationToken = default);

    /// <summary>Updates DX connections matching a mask.</summary>
    Task<OpcResultId> UpdateConnectionAsync(string browsePath, string connectionName, bool recursive, DxConnection connectionDefinition, CancellationToken cancellationToken = default);

    /// <summary>Deletes a DX connection.</summary>
    Task<OpcResultId> DeleteConnectionAsync(string browsePath, string connectionName, bool recursive, CancellationToken cancellationToken = default);

    /// <summary>Adds a source server.</summary>
    Task<OpcResultId> AddSourceServerAsync(DxSourceServer sourceServer, CancellationToken cancellationToken = default);

    /// <summary>Modifies a source server.</summary>
    Task<OpcResultId> ModifySourceServerAsync(DxSourceServer sourceServer, CancellationToken cancellationToken = default);

    /// <summary>Resets the DX configuration and returns the new version.</summary>
    Task<string> ResetConfigurationAsync(string configurationVersion, CancellationToken cancellationToken = default);
}

/// <summary>Holds OPC DX client state for an MCP session.</summary>
public sealed class DxClientState : IAsyncDisposable {
    /// <summary>Creates DX client state.</summary>
    public DxClientState(string host, string? progId, Guid? clsid, IOpcDxClient client) {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(client);

        Host = host;
        ProgId = progId;
        Clsid = clsid;
        Client = client;
    }

    /// <summary>Target host or connection scheme.</summary>
    public string Host { get; }

    /// <summary>Connected DX server ProgID, if known.</summary>
    public string? ProgId { get; }

    /// <summary>Connected DX server CLSID, if known.</summary>
    public Guid? Clsid { get; }

    /// <summary>DX client implementation.</summary>
    public IOpcDxClient Client { get; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await Client.DisposeAsync().ConfigureAwait(false);
}

/// <summary>Operations required by MCP OPC Security tools.</summary>
public interface IOpcSecurityClient : IAsyncDisposable {
    /// <summary>True when authenticated.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Current identity, or empty when anonymous/default.</summary>
    string CurrentIdentity { get; }

    /// <summary>Checks whether Windows-integrated OPC Security is available.</summary>
    Task<bool> IsAvailableNtAsync(CancellationToken cancellationToken = default);

    /// <summary>Checks whether private username/password OPC Security is available.</summary>
    Task<bool> IsAvailablePrivateAsync(CancellationToken cancellationToken = default);

    /// <summary>Logs on with server-private credentials.</summary>
    Task<bool> LogonPrivateAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>Logs off and returns to the connection default identity.</summary>
    Task LogoffAsync(CancellationToken cancellationToken = default);
}

/// <summary>Holds OPC Security client state for an MCP session.</summary>
public sealed class SecurityClientState : IAsyncDisposable {
    /// <summary>Creates OPC Security client state.</summary>
    public SecurityClientState(IOpcSecurityClient client) {
        ArgumentNullException.ThrowIfNull(client);
        Client = client;
    }

    /// <summary>Security client implementation.</summary>
    public IOpcSecurityClient Client { get; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await Client.DisposeAsync().ConfigureAwait(false);
}

/// <summary>Holds OPC XML-DA client state for an MCP session.</summary>
public sealed class XmlDaClientState : IAsyncDisposable {
    private readonly IDisposable? _ownedDisposable;
    private readonly IAsyncDisposable? _ownedAsyncDisposable;
    private bool _disposed;

    /// <summary>Creates XML-DA client state.</summary>
    public XmlDaClientState(string endpointUrl, IXmlDaClient client, IDisposable? ownedDisposable = null, IAsyncDisposable? ownedAsyncDisposable = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpointUrl);
        ArgumentNullException.ThrowIfNull(client);

        EndpointUrl = endpointUrl;
        Client = client;
        _ownedDisposable = ownedDisposable;
        _ownedAsyncDisposable = ownedAsyncDisposable;
    }

    /// <summary>HTTP/SOAP endpoint URL.</summary>
    public string EndpointUrl { get; }

    /// <summary>XML-DA client implementation.</summary>
    public IXmlDaClient Client { get; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        if (_disposed) {
            return;
        }

        _disposed = true;
        if (_ownedAsyncDisposable is not null) {
            await _ownedAsyncDisposable.DisposeAsync().ConfigureAwait(false);
        }

        _ownedDisposable?.Dispose();
    }
}
