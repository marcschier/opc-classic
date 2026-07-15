// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Opc.Classic.Da.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Hosting;
using Opc.Classic.Ndr;

namespace Opc.Classic.Da.Hosting;

/// <summary>
/// DA dispatcher adapter that delegates to source-generated OPC DA dispatchers.
/// </summary>
public sealed class OpcDaServerDispatcher : IOpcDaServerDispatcher, IOPCServer, IOPCCommon, IConnectionPointContainer, IConnectionPoint
{
    private static readonly Action<ILogger, string, Exception?> ClientNameSet = LoggerMessage.Define<string>(
        LogLevel.Debug,
        new EventId(1, nameof(ClientNameSet)),
        "OPC DA client name set: {ClientName}");

    private readonly IOpcDaServer _server;
    private readonly IOpcServerDispatcher _serverDispatcher;
    private readonly IOPCCommonServerDispatcher _commonDispatcher;
    private readonly IConnectionPointContainerServerDispatcher _connectionPointContainerDispatcher;
    private readonly IConnectionPointServerDispatcher _connectionPointDispatcher;
    private readonly OpcObjectRegistry _objectRegistry;
    private readonly ConcurrentDictionary<int, IOPCShutdown> _shutdownSinks = new();
    private int _nextShutdownCookie;
    private readonly ConnectionDiagnostics _connectionContext = new();
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="OpcDaServerDispatcher" /> class.
    /// </summary>
    public OpcDaServerDispatcher(IOpcDaServer server, ILogger? logger)
        : this(server, logger, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpcDaServerDispatcher" /> class.
    /// </summary>
    public OpcDaServerDispatcher(
        IOpcDaServer server,
        ILogger? logger = null,
        OpcObjectRegistry? objectRegistry = null)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _logger = logger ?? NullLogger.Instance;
        _objectRegistry = objectRegistry ?? new OpcObjectRegistry();
        _serverDispatcher = new GroupEnumeratorDispatcher(
            this,
            new IOPCServerServerDispatcher(this));
        _commonDispatcher = new IOPCCommonServerDispatcher(this);
        _connectionPointContainerDispatcher = new IConnectionPointContainerServerDispatcher(this);
        _connectionPointDispatcher = new IConnectionPointServerDispatcher(this);
        if (_server is IDaServer daServer)
        {
            daServer.ServerShutdown += OnServerShutdown;
        }
    }

    /// <summary>
    /// Gets the latest client name supplied through <c>IOPCCommon::SetClientName</c>.
    /// </summary>
    public string ClientName => _connectionContext.ClientName;
    public IOpcServerDispatcher ServerDispatcher => _serverDispatcher;
    public IOpcServerDispatcher CommonDispatcher => _commonDispatcher;
    public IOpcServerDispatcher ConnectionPointContainerDispatcher => _connectionPointContainerDispatcher;
    public IOpcServerDispatcher ConnectionPointDispatcher => _connectionPointDispatcher;

    Task IOPCServer.AddGroupAsync(
        string name,
        bool active,
        int requestedUpdateRate,
        int clientGroupHandle,
        int timeBias,
        float percentDeadband,
        int localeId,
        Guid requestedInterfaceId,
        out int serverGroupHandle,
        out int revisedUpdateRate,
        out IOpcInterfaceRef group,
        CancellationToken cancellationToken) =>
        ((IOPCServer)_server).AddGroupAsync(
            name,
            active,
            requestedUpdateRate,
            clientGroupHandle,
            timeBias,
            percentDeadband,
            localeId,
            requestedInterfaceId,
            out serverGroupHandle,
            out revisedUpdateRate,
            out group,
            cancellationToken);

    Task<IOpcInterfaceRef> IOPCServer.GetGroupByNameAsync(
        string name,
        Guid requestedInterfaceId,
        CancellationToken cancellationToken) =>
        ((IOPCServer)_server).GetGroupByNameAsync(name, requestedInterfaceId, cancellationToken);

    Task<OpcServerStatus> IOPCServer.GetStatusAsync(CancellationToken cancellationToken) =>
        _server.GetStatusAsync(cancellationToken);

    Task<string> IOPCServer.GetErrorStringAsync(
        int errorCode,
        int localeId,
        CancellationToken cancellationToken) =>
        _server.GetErrorStringAsync(errorCode, localeId, cancellationToken);

    Task IOPCServer.RemoveGroupAsync(
        int serverGroupHandle,
        bool force,
        CancellationToken cancellationToken) =>
        _server.RemoveGroupAsync(serverGroupHandle, force, cancellationToken);

    async Task<IOpcInterfaceRef> IOPCServer.CreateGroupEnumeratorAsync(
        int scope,
        Guid requestedInterfaceId,
        CancellationToken cancellationToken) =>
        (await CreateGroupEnumeratorCoreAsync(
            scope,
            requestedInterfaceId,
            cancellationToken).ConfigureAwait(false)).InterfaceRef;

    private async Task<GroupEnumeratorResult> CreateGroupEnumeratorCoreAsync(
        int scope,
        Guid requestedInterfaceId,
        CancellationToken cancellationToken)
    {
        OpcDaGroupEnumerationScope enumerationScope =
            OpcDaGroupEnumerationScopeExtensions.FromWireValue(scope);
        Guid expectedIid = enumerationScope.IsConnectionScope()
            ? IEnumUnknown.InterfaceId
            : IEnumString.InterfaceId;
        if (requestedInterfaceId != expectedIid)
        {
            throw new OpcException(new OpcResultId(unchecked((int)0x80004002), "E_NOINTERFACE"));
        }

        OpcDaGroupEnumerationSnapshot snapshot = await _server
            .CreateGroupEnumerationSnapshotAsync(enumerationScope, cancellationToken)
            .ConfigureAwait(false);
        IOpcInterfaceRef interfaceRef = snapshot.EnumeratesConnections
            ? OpcDaGroupEnumeratorFactory.CreateUnknown(snapshot.Groups, _objectRegistry)
            : OpcDaGroupEnumeratorFactory.CreateString(snapshot.Names, _objectRegistry);
        return new GroupEnumeratorResult(interfaceRef, snapshot.Groups.Count == 0);
    }

    /// <inheritdoc />
    public async Task<NdrCallResult> DispatchAsync(
        Guid interfaceId,
        int opnum,
        ReadOnlyMemory<byte> requestPayload,
        CancellationToken cancellationToken)
    {
        if (interfaceId == IOPCServer.InterfaceId)
        {
            return (await _serverDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
                .ToNdrCallResult();
        }

        if (interfaceId == IOPCCommon.InterfaceId)
        {
            return (await _commonDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
                .ToNdrCallResult();
        }

        if (interfaceId == IConnectionPointContainer.InterfaceId)
        {
            return (await _connectionPointContainerDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
                .ToNdrCallResult();
        }

        if (interfaceId == IConnectionPoint.InterfaceId)
        {
            return (await _connectionPointDispatcher.DispatchAsync(opnum, requestPayload, cancellationToken).ConfigureAwait(false))
                .ToNdrCallResult();
        }

        return new NdrCallResult(OpcResultId.NotImplemented.Code, ReadOnlyMemory<byte>.Empty);
    }

    /// <inheritdoc />
    public Task SetLocaleIdAsync(int localeId, CancellationToken cancellationToken = default) =>
        _server is IOPCCommon common
            ? common.SetLocaleIdAsync(localeId, cancellationToken)
            : NotImplementedAsync();

    /// <inheritdoc />
    public Task<int> GetLocaleIdAsync(CancellationToken cancellationToken = default) =>
        _server is IOPCCommon common
            ? common.GetLocaleIdAsync(cancellationToken)
            : NotImplementedAsync<int>();

    /// <inheritdoc />
    public Task<int[]> QueryAvailableLocaleIdsAsync(CancellationToken cancellationToken = default) =>
        _server is IOPCCommon common
            ? common.QueryAvailableLocaleIdsAsync(cancellationToken)
            : NotImplementedAsync<int[]>();

    /// <inheritdoc />
    public Task<string> GetErrorStringAsync(int errorCode, CancellationToken cancellationToken = default) =>
        _server is IOPCCommon common
            ? common.GetErrorStringAsync(errorCode, cancellationToken)
            : _server.GetErrorStringAsync(errorCode, localeId: 0, cancellationToken);

    /// <inheritdoc />
    public Task SetClientNameAsync(string name, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(name);
        cancellationToken.ThrowIfCancellationRequested();
        _connectionContext.ClientName = name;
        ClientNameSet(_logger, name, null);

        return _server is IDaServer daServer
            ? daServer.SetClientNameAsync(name, cancellationToken)
            : Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IOpcInterfaceRef> EnumConnectionPointsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IOpcInterfaceRef>(CreateSyntheticInterfaceRef(OpcGuids.IID_IEnumConnectionPoints));
    }

    /// <inheritdoc />
    public Task<IOpcInterfaceRef> FindConnectionPointAsync(Guid iid, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (iid != IOPCShutdown.InterfaceId)
        {
            throw new OpcException(new OpcResultId(unchecked((int)0x80040200), "CONNECT_E_NOCONNECTION"));
        }

        return Task.FromResult<IOpcInterfaceRef>(CreateSyntheticInterfaceRef(IConnectionPoint.InterfaceId));
    }

    /// <inheritdoc />
    public Task<Guid> GetConnectionInterfaceAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(IOPCShutdown.InterfaceId);
    }

    /// <inheritdoc />
    public Task<int> AdviseAsync(IOpcInterfaceRef sink, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sink);
        cancellationToken.ThrowIfCancellationRequested();
        if (sink.Iid != IOPCShutdown.InterfaceId || sink is not OpcDaShutdownSinkRef directSink)
        {
            throw new OpcException(new OpcResultId(unchecked((int)0x80040200), "CONNECT_E_NOCONNECTION"));
        }

        int cookie = Interlocked.Increment(ref _nextShutdownCookie);
        _shutdownSinks[cookie] = directSink.Sink;
        return Task.FromResult(cookie);
    }

    /// <inheritdoc />
    public Task UnadviseAsync(int cookie, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!_shutdownSinks.TryRemove(cookie, out _))
        {
            throw new OpcException(new OpcResultId(unchecked((int)0x80040200), "CONNECT_E_NOCONNECTION"));
        }
        return Task.CompletedTask;
    }

    private void OnServerShutdown(object? sender, ServerShutdownEventArgs e)
    {
        foreach (IOPCShutdown sink in _shutdownSinks.Values)
        {
#pragma warning disable VSTHRD002
            sink.ShutdownRequestAsync(e.Reason ?? string.Empty, CancellationToken.None).GetAwaiter().GetResult();
#pragma warning restore VSTHRD002
        }
    }

    private static IOpcInterfaceRef CreateSyntheticInterfaceRef(Guid iid) =>
        new OpcInterfaceRef(iid, 0, 1, 1, 1, Guid.CreateVersion7(), 0, Array.Empty<ushort>());

    private static Task NotImplementedAsync() =>
        Task.FromException(new OpcException(OpcResultId.NotImplemented));

    private static Task<T> NotImplementedAsync<T>() =>
        Task.FromException<T>(new OpcException(OpcResultId.NotImplemented));

    private sealed class ConnectionDiagnostics
    {
        public string ClientName { get; set; } = string.Empty;
    }

    private readonly record struct GroupEnumeratorResult(
        IOpcInterfaceRef InterfaceRef,
        bool IsEmpty);

    private sealed class GroupEnumeratorDispatcher : IOpcServerDispatcher
    {
        private const int EInvalidArg = unchecked((int)0x80070057);
        private readonly OpcDaServerDispatcher _owner;
        private readonly IOpcServerDispatcher _inner;

        public GroupEnumeratorDispatcher(
            OpcDaServerDispatcher owner,
            IOpcServerDispatcher inner)
        {
            _owner = owner;
            _inner = inner;
        }

        public async ValueTask<DispatchResult> DispatchAsync(
            int opnum,
            ReadOnlyMemory<byte> requestPayload,
            CancellationToken cancellationToken = default)
        {
            if (opnum != IOPCServer.Opnums.CreateGroupEnumeratorAsync)
            {
                return await _inner
                    .DispatchAsync(opnum, requestPayload, cancellationToken)
                    .ConfigureAwait(false);
            }

            try
            {
                var reader = new NdrReader(requestPayload.Span);
                int scope = reader.ReadInt32();
                Guid requestedInterfaceId = reader.ReadGuid();
                GroupEnumeratorResult result = await _owner
                    .CreateGroupEnumeratorCoreAsync(scope, requestedInterfaceId, cancellationToken)
                    .ConfigureAwait(false);
                var buffer = new byte[1024];
                var writer = new NdrWriter(buffer);
                OpcMInterfacePointerCodec.Write(ref writer, result.InterfaceRef);
                return DispatchResult.Success(
                    buffer.AsSpan(0, writer.Position).ToArray(),
                    result.IsEmpty ? OpcResultId.False.Code : OpcResultId.Ok.Code);
            }
            catch (OpcException exception)
            {
                return DispatchResult.Fault(exception.ResultId.Code);
            }
            catch (ArgumentException)
            {
                return DispatchResult.Fault(EInvalidArg);
            }
            catch (InvalidOperationException)
            {
                return DispatchResult.Fault(EInvalidArg);
            }
        }
    }
}
