//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Collections.Concurrent;
using System.Threading.Channels;
using Opc.Classic.Ae;
using Opc.Classic.Ae.Dcom;
using Opc.Classic.Hda;
using Opc.Classic.Hda.Dcom;

namespace Opc.Classic.Mcp.Sessions;

/// <summary>Holds OPC AE wire proxies and poll-based subscription queues.</summary>
public sealed class AeClientState : IAsyncDisposable {
    private readonly ICallChannel _channel;
    private readonly bool _ownsChannel;
    private bool _disposed;

    /// <summary>Creates AE client state over an existing call channel.</summary>
    public AeClientState(string host, string? progId, Guid? clsid, ICallChannel channel, bool ownsChannel, IAeServer? managedServer = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(channel);

        Host = host;
        ProgId = progId;
        Clsid = clsid;
        _channel = channel;
        _ownsChannel = ownsChannel;
        ManagedServer = managedServer;
        EventServer = new IOPCEventServerClientProxy(channel);
        EventServer2 = new IOPCEventServer2ClientProxy(channel);
        SubscriptionMgt = new IOPCEventSubscriptionMgtClientProxy(channel);
        SubscriptionMgt2 = new IOPCEventSubscriptionMgt2ClientProxy(channel);
        AreaBrowser = new IOPCEventAreaBrowserClientProxy(channel);
    }

    /// <summary>Target host.</summary>
    public string Host { get; }

    /// <summary>Connected AE server ProgID, if known.</summary>
    public string? ProgId { get; }

    /// <summary>Connected AE server CLSID, if known.</summary>
    public Guid? Clsid { get; }

    /// <summary>Optional managed AE client used by in-process loopback connections.</summary>
    public IAeServer? ManagedServer { get; }

    /// <summary>Top-level AE event server proxy.</summary>
    public IOPCEventServerClientProxy EventServer { get; }

    /// <summary>AE 1.10 event server proxy.</summary>
    public IOPCEventServer2ClientProxy EventServer2 { get; }

    /// <summary>AE subscription management proxy.</summary>
    public IOPCEventSubscriptionMgtClientProxy SubscriptionMgt { get; }

    /// <summary>AE subscription keep-alive proxy.</summary>
    public IOPCEventSubscriptionMgt2ClientProxy SubscriptionMgt2 { get; }

    /// <summary>AE area browser proxy.</summary>
    public IOPCEventAreaBrowserClientProxy AreaBrowser { get; }

    /// <summary>Known poll-based AE subscriptions by identifier.</summary>
    public ConcurrentDictionary<string, AeSubscriptionContext> Subscriptions { get; } = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        if (_disposed) {
            return;
        }

        _disposed = true;
        foreach (AeSubscriptionContext subscription in Subscriptions.Values) {
            try {
                await subscription.DisposeAsync().ConfigureAwait(false);
            }
            catch (ObjectDisposedException) {
                // Underlying socket was already closed (e.g. by a server-side
                // disconnect after a sign/seal-protected call); subscription
                // teardown can't write a graceful unsubscribe. Treat as
                // already-disposed.
            }
            catch (System.IO.IOException) {
                // Same as above — the connection went away under us mid-call.
            }
        }

        Subscriptions.Clear();
        if (_ownsChannel) {
            try {
                switch (_channel) {
                    case IAsyncDisposable asyncDisposable:
                        await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                        break;
                    case IDisposable disposable:
                        disposable.Dispose();
                        break;
                }
            }
            catch (ObjectDisposedException) {
                // The DCOM peer (e.g. opcae_ps.dll on the samples-ae native-CCW
                // path) frequently sends a TCP RST after the response is
                // dispatched. By the time the MCP layer calls disconnect, the
                // socket is already disposed and any graceful-close write
                // would throw. The session-level state is already cleaned up;
                // swallow and return.
            }
            catch (System.IO.IOException) {
                // Same rationale — connection went away under us.
            }
        }
    }
}

/// <summary>Tracks an AE subscription and its MCP poll queue.</summary>
public sealed class AeSubscriptionContext : IAsyncDisposable {
    private bool _disposed;

    /// <summary>Creates an AE subscription context.</summary>
    public AeSubscriptionContext(string subscriptionId, int clientSubscription, bool active, int bufferTimeMs, int maxBufferSize, int revisedBufferTimeMs, int revisedMaxBufferSize, IAeSubscription? subscription) {
        ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);
        SubscriptionId = subscriptionId;
        ClientSubscription = clientSubscription;
        Active = active;
        BufferTimeMs = bufferTimeMs;
        MaxBufferSize = maxBufferSize;
        RevisedBufferTimeMs = revisedBufferTimeMs;
        RevisedMaxBufferSize = revisedMaxBufferSize;
        Subscription = subscription;
    }

    /// <summary>Subscription identifier returned to MCP callers.</summary>
    public string SubscriptionId { get; }

    /// <summary>Client subscription handle supplied to the AE server.</summary>
    public int ClientSubscription { get; }

    /// <summary>Whether the subscription is active.</summary>
    public bool Active { get; set; }

    /// <summary>Requested buffer time in milliseconds.</summary>
    public int BufferTimeMs { get; }

    /// <summary>Requested maximum buffer size.</summary>
    public int MaxBufferSize { get; }

    /// <summary>Server-revised buffer time in milliseconds.</summary>
    public int RevisedBufferTimeMs { get; }

    /// <summary>Server-revised maximum buffer size.</summary>
    public int RevisedMaxBufferSize { get; }

    /// <summary>Managed subscription, when available.</summary>
    public IAeSubscription? Subscription { get; }

    /// <summary>Current filter tracked for JSON results.</summary>
    public SubscriptionFilter Filter { get; set; } = new();

    /// <summary>Queued notifications for MCP polling.</summary>
    public Channel<OpcEventNotification> Events { get; } = Channel.CreateUnbounded<OpcEventNotification>();

    /// <summary>Cancellation source for the background event pump.</summary>
    public CancellationTokenSource Cancellation { get; } = new();

    /// <summary>Background event pump task.</summary>
    public Task? PumpTask { get; set; }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        if (_disposed) {
            return;
        }

        _disposed = true;
        Cancellation.Cancel();
        Events.Writer.TryComplete();
        if (Subscription is not null) {
            await Subscription.DisposeAsync().ConfigureAwait(false);
        }

        if (PumpTask is not null) {
            try {
                await PumpTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException) {
            }
        }

        Cancellation.Dispose();
    }
}

/// <summary>Holds OPC HDA wire proxies and item handles.</summary>
public sealed class HdaClientState : IAsyncDisposable {
    private readonly ICallChannel _channel;
    private readonly bool _ownsChannel;
    private bool _disposed;

    /// <summary>Creates HDA client state over an existing call channel.</summary>
    public HdaClientState(string host, string? progId, Guid? clsid, ICallChannel channel, bool ownsChannel, IHdaServer? managedServer = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(channel);

        Host = host;
        ProgId = progId;
        Clsid = clsid;
        _channel = channel;
        _ownsChannel = ownsChannel;
        ManagedServer = managedServer;
        Server = new IOPCHDA_ServerClientProxy(channel);
        Browser = new IOPCHDA_BrowserClientProxy(channel);
        SyncRead = new IOPCHDA_SyncReadClientProxy(channel);
        SyncUpdate = new IOPCHDA_SyncUpdateClientProxy(channel);
        SyncAnnotations = new IOPCHDA_SyncAnnotationsClientProxy(channel);
    }

    /// <summary>Target host.</summary>
    public string Host { get; }

    /// <summary>Connected HDA server ProgID, if known.</summary>
    public string? ProgId { get; }

    /// <summary>Connected HDA server CLSID, if known.</summary>
    public Guid? Clsid { get; }

    /// <summary>Optional managed HDA client used by in-process loopback connections.</summary>
    public IHdaServer? ManagedServer { get; }

    /// <summary>Top-level HDA server proxy.</summary>
    public IOPCHDA_ServerClientProxy Server { get; }

    /// <summary>HDA browser proxy.</summary>
    public IOPCHDA_BrowserClientProxy Browser { get; }

    /// <summary>HDA synchronous read proxy.</summary>
    public IOPCHDA_SyncReadClientProxy SyncRead { get; }

    /// <summary>HDA synchronous update proxy.</summary>
    public IOPCHDA_SyncUpdateClientProxy SyncUpdate { get; }

    /// <summary>HDA synchronous annotations proxy.</summary>
    public IOPCHDA_SyncAnnotationsClientProxy SyncAnnotations { get; }

    /// <summary>Known HDA item handles by server handle.</summary>
    public ConcurrentDictionary<int, HdaItemHandleContext> ItemHandles { get; } = new();

    /// <inheritdoc />
    public async ValueTask DisposeAsync() {
        if (_disposed) {
            return;
        }

        _disposed = true;
        ItemHandles.Clear();
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

/// <summary>Tracks an HDA item handle returned by GetItemHandles.</summary>
public sealed record HdaItemHandleContext(string ItemId, int ClientHandle, int ServerHandle);
