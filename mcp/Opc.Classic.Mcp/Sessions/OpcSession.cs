//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Concurrent;
using Opc.Classic.Da.Dcom;

namespace Opc.Classic.Mcp.Sessions;

/// <summary>Holds MCP session state and per-OPC-spec client state.</summary>
public sealed class OpcSession : IAsyncDisposable
{
    private bool _disposed;

    /// <summary>Creates a new session with the requested idle expiry.</summary>
    public OpcSession(string sessionId, TimeSpan idleExpiry)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        if (idleExpiry <= TimeSpan.Zero)
        {
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

    /// <summary>Returns true when the session has exceeded its idle expiry.</summary>
    public bool IsExpired(DateTimeOffset now) => now - LastUsedAt >= IdleExpiry;

    /// <summary>Updates the last-used timestamp.</summary>
    public void Touch() => LastUsedAt = DateTimeOffset.UtcNow;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        DaClientState? daClient = DaClient;
        AeClientState? aeClient = AeClient;
        HdaClientState? hdaClient = HdaClient;
        DaClient = null;
        AeClient = null;
        HdaClient = null;
        if (daClient is not null)
        {
            await daClient.DisposeAsync().ConfigureAwait(false);
        }

        if (aeClient is not null)
        {
            await aeClient.DisposeAsync().ConfigureAwait(false);
        }

        if (hdaClient is not null)
        {
            await hdaClient.DisposeAsync().ConfigureAwait(false);
        }
    }
}

/// <summary>Holds OPC DA wire proxies and server-side group/subscription handles.</summary>
public sealed class DaClientState : IAsyncDisposable
{
    private readonly ICallChannel _channel;
    private readonly bool _ownsChannel;
    private bool _disposed;

    /// <summary>Creates DA client state over an existing call channel.</summary>
    public DaClientState(string host, string? progId, Guid? clsid, ICallChannel channel, bool ownsChannel)
    {
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

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_ownsChannel)
        {
            switch (_channel)
            {
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
public sealed class DaGroupContext
{
    /// <summary>Creates a group context.</summary>
    public DaGroupContext(int serverGroupHandle, string? name, int clientHandle, bool active, int updateRateMs, int revisedUpdateRateMs, int timeBiasMinutes, float deadbandPercent, int localeId, int keepAliveMs)
    {
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

    /// <summary>Known items in the group by server handle.</summary>
    public ConcurrentDictionary<int, DaItemBindingContext> Items { get; } = new();
}

/// <summary>Tracks a DA item binding returned by AddItems.</summary>
public sealed record DaItemBindingContext(string ItemName, string? ItemPath, int ClientHandle, int ServerHandle);

/// <summary>Tracks a poll-based DA subscription.</summary>
public sealed record DaSubscriptionContext(string SubscriptionId, int GroupHandle, bool FromCache, int TransactionId, int? CancelId);

