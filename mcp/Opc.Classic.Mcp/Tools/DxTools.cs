// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Opc.Classic.Dx;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;

namespace Opc.Classic.Mcp.Tools;

/// <summary>
/// Creates DX client state for a session.
/// </summary>
public interface IOpcDxConnectionFactory
{
    /// <summary>
    /// Connects to a DX server and returns a client state object.
    /// </summary>
    Task<DxClientState> ConnectAsync(DxConnectionRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Connection request used by DX tools.
/// </summary>
public sealed record DxConnectionRequest(
    string Host,
    string? ProgId,
    string? Clsid,
    string? Username,
    string? Password,
    bool UseKerberos,
    string? ConnectionString,
    string? AuthLevel = null);

/// <summary>
/// Registers in-memory DX clients for MCP tests and loopback scenarios.
/// </summary>
public static class InMemoryDxConnectionRegistry
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, Func<IOpcDxClient>> Clients = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers an in-memory DX client by name.
    /// </summary>
    public static IDisposable Register(string name, IOpcDxClient client)
    {
        ArgumentNullException.ThrowIfNull(client);
        return Register(name, () => client);
    }

    /// <summary>
    /// Registers a factory that creates an in-memory DX client lease per connection.
    /// </summary>
    public static IDisposable Register(string name, Func<IOpcDxClient> clientFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(clientFactory);

        Clients[name] = clientFactory;
        return new Registration(name);
    }

    internal static bool TryGet(string name, out IOpcDxClient client)
    {
        if (Clients.TryGetValue(name, out Func<IOpcDxClient>? factory))
        {
            client = factory();
            return true;
        }

        client = null!;
        return false;
    }

    private sealed class Registration : IDisposable
    {
        private readonly string _name;
        private bool _disposed;

        public Registration(string name) => _name = name;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Clients.TryRemove(_name, out _);
        }
    }
}

/// <summary>
/// MCP tools for OPC DX configuration operations.
/// </summary>
public sealed class DxTools
{
    private readonly IOpcSessionManager _sessionManager;
    private readonly IOpcDxConnectionFactory _connectionFactory;

    /// <summary>
    /// Creates the DX tool set.
    /// </summary>
    public DxTools(IOpcSessionManager sessionManager, IEnumerable<IOpcDxConnectionFactory> connectionFactories)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        ArgumentNullException.ThrowIfNull(connectionFactories);
        _connectionFactory = connectionFactories.FirstOrDefault() ?? new DefaultOpcDxConnectionFactory();
    }

    /// <summary>
    /// Connects a session to an OPC DX server.
    /// </summary>
    [McpServerTool(Name = "opcclassic.dx.connect", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Connects an existing MCP session to an OPC DX server. Use connectionString=inmemory://name for registered test clients.")]
    public async Task<OpcSessionDto> Connect(
        [Description("The sessionId returned by opcclassic.session.create.")]
        string sessionId,
        [Description("OPC DX server host name or IP address. Ignored when connectionString uses inmemory://.")]
        string host = "localhost",
        [Description("OPC DX server ProgID. Optional when clsid or connectionString is supplied.")]
        string? progId = null,
        [Description("OPC DX server CLSID as a GUID string. Optional when progId or connectionString is supplied.")]
        string? clsid = null,
        [Description("Optional user name for DCOM authentication.")]
        string? username = null,
        [Description("Optional password for DCOM authentication.")]
        string? password = null,
        [Description("True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied.")]
        bool useKerberos = false,
        [Description("Optional connection string. Use inmemory://name for a registered in-memory DX client.")]
        string? connectionString = null,
        [Description(OpcMcpAuthLevel.Description)]
        string? authLevel = null,
        CancellationToken cancellationToken = default)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        DxClientState client = await _connectionFactory.ConnectAsync(
            new DxConnectionRequest(host, progId, clsid, username, password, useKerberos, connectionString, authLevel),
            cancellationToken).ConfigureAwait(false);

        DxClientState? existing = session.DxClient;
        session.DxClient = client;
        if (existing is not null)
        {
            await existing.DisposeAsync().ConfigureAwait(false);
        }

        _ = await client.Client.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        session.Touch();
        return ToSessionDto(session);
    }

    /// <summary>
    /// Gets OPC DX server status for a connected session.
    /// </summary>
    [McpServerTool(Name = "opcclassic.dx.get_status", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Gets the OPC DX server status exposed by the connected server.")]
    public async Task<OpcServerStatusDto> GetStatus(
        [Description("The sessionId returned by opcclassic.session.create and connected with opcclassic.dx.connect.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        DxClientState client = GetDxClient(sessionId);
        OpcServerStatus status = await client.Client.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        return ToDto(status);
    }

    /// <summary>
    /// Queries DX connection names.
    /// </summary>
    [McpServerTool(Name = "opcclassic.dx.query_connections", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Queries OPC DX connection names using QueryDXConnectionNames semantics.")]
    public async Task<IReadOnlyList<string>> QueryConnections(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("DX browse path to query. Use an empty string for the root.")]
        string browsePath = "",
        [Description("Optional connection-name masks. Omit or pass empty to return all names.")]
        string[]? connectionMasks = null,
        [Description("True to include descendant browse paths.")]
        bool recursive = false,
        CancellationToken cancellationToken = default)
    {
        DxClientState client = GetDxClient(sessionId);
        return await client.Client.QueryConnectionNamesAsync(browsePath ?? string.Empty, connectionMasks ?? [], recursive, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Queries configured DX source servers.
    /// </summary>
    [McpServerTool(Name = "opcclassic.dx.query_source_servers", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Lists the source servers configured in the connected OPC DX server.")]
    public async Task<IReadOnlyList<OpcDxSourceServerDto>> QuerySourceServers(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        DxClientState client = GetDxClient(sessionId);
        IReadOnlyList<DxSourceServer> sources = await client.Client.QuerySourceServersAsync(cancellationToken).ConfigureAwait(false);
        return sources.Select(ToDto).ToArray();
    }

    /// <summary>
    /// Adds a DX connection.
    /// </summary>
    [McpServerTool(Name = "opcclassic.dx.add_connection", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Adds an OPC DX connection definition.")]
    public async Task<OpcResultDto> AddConnection(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("DX connection definition to add.")]
        OpcDxConnectionDto connection,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        DxClientState client = GetDxClient(sessionId);
        OpcResultId result = await client.Client.AddConnectionAsync(ToConnection(connection), cancellationToken).ConfigureAwait(false);
        return ToResult(result, connection.Name, "DX connection added.");
    }

    /// <summary>
    /// Modifies a DX connection.
    /// </summary>
    [McpServerTool(Name = "opcclassic.dx.modify_connection", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Modifies an existing OPC DX connection definition.")]
    public async Task<OpcResultDto> ModifyConnection(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("DX connection definition to modify.")]
        OpcDxConnectionDto connection,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connection);
        DxClientState client = GetDxClient(sessionId);
        OpcResultId result = await client.Client.ModifyConnectionAsync(ToConnection(connection), cancellationToken).ConfigureAwait(false);
        return ToResult(result, connection.Name, "DX connection modified.");
    }

    /// <summary>
    /// Updates matching DX connections.
    /// </summary>
    [McpServerTool(Name = "opcclassic.dx.update_connection", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Updates OPC DX connections matching a connection name and browse path.")]
    public async Task<OpcResultDto> UpdateConnection(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Connection name or mask to update.")]
        string connectionName,
        [Description("Updated DX connection fields.")]
        OpcDxConnectionDto connection,
        [Description("DX browse path to search. Use an empty string for the root.")]
        string browsePath = "",
        [Description("True to include descendant browse paths.")]
        bool recursive = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
        ArgumentNullException.ThrowIfNull(connection);
        DxClientState client = GetDxClient(sessionId);
        OpcResultId result = await client.Client.UpdateConnectionAsync(browsePath ?? string.Empty, connectionName, recursive, ToConnection(connection), cancellationToken).ConfigureAwait(false);
        return ToResult(result, connectionName, "DX connection updated.");
    }

    /// <summary>
    /// Deletes a DX connection.
    /// </summary>
    [McpServerTool(Name = "opcclassic.dx.delete_connection", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Deletes an OPC DX connection by name.")]
    public async Task<OpcResultDto> DeleteConnection(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("DX connection name to delete.")]
        string connectionName,
        [Description("DX browse path to search. Use an empty string for the root.")]
        string browsePath = "",
        [Description("True to include descendant browse paths.")]
        bool recursive = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionName);
        DxClientState client = GetDxClient(sessionId);
        OpcResultId result = await client.Client.DeleteConnectionAsync(browsePath ?? string.Empty, connectionName, recursive, cancellationToken).ConfigureAwait(false);
        return ToResult(result, connectionName, "DX connection deleted.");
    }

    /// <summary>
    /// Adds a DX source server.
    /// </summary>
    [McpServerTool(Name = "opcclassic.dx.add_source_server", ReadOnly = false, Idempotent = false, Destructive = false, OpenWorld = true)]
    [Description("Adds an OPC DX source-server definition.")]
    public async Task<OpcResultDto> AddSourceServer(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("DX source-server definition to add.")]
        OpcDxSourceServerDto sourceServer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceServer);
        DxClientState client = GetDxClient(sessionId);
        OpcResultId result = await client.Client.AddSourceServerAsync(ToSourceServer(sourceServer), cancellationToken).ConfigureAwait(false);
        return ToResult(result, sourceServer.Name, "DX source server added.");
    }

    /// <summary>
    /// Modifies a DX source server.
    /// </summary>
    [McpServerTool(Name = "opcclassic.dx.modify_source_server", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Modifies an existing OPC DX source-server definition.")]
    public async Task<OpcResultDto> ModifySourceServer(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("DX source-server definition to modify.")]
        OpcDxSourceServerDto sourceServer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sourceServer);
        DxClientState client = GetDxClient(sessionId);
        OpcResultId result = await client.Client.ModifySourceServerAsync(ToSourceServer(sourceServer), cancellationToken).ConfigureAwait(false);
        return ToResult(result, sourceServer.Name, "DX source server modified.");
    }

    /// <summary>
    /// Resets DX configuration.
    /// </summary>
    [McpServerTool(Name = "opcclassic.dx.reset_configuration", ReadOnly = false, Idempotent = false, Destructive = true, OpenWorld = true)]
    [Description("Resets all configured OPC DX connections and source servers.")]
    public async Task<OpcResultDto> ResetConfiguration(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Optional current configuration version supplied to the server.")]
        string configurationVersion = "",
        CancellationToken cancellationToken = default)
    {
        DxClientState client = GetDxClient(sessionId);
        string newVersion = await client.Client.ResetConfigurationAsync(configurationVersion ?? string.Empty, cancellationToken).ConfigureAwait(false);
        return new OpcResultDto(0, string.IsNullOrEmpty(newVersion) ? "DX configuration reset." : $"DX configuration reset. New version: {newVersion}", Succeeded: true, ValueType: newVersion);
    }

    /// <summary>
    /// Disconnects DX client state from the session.
    /// </summary>
    [McpServerTool(Name = "opcclassic.dx.disconnect", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Disconnects the session from its OPC DX server and releases DX client state.")]
    public async Task<OpcResultDto> Disconnect(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        DxClientState? client = session.DxClient;
        session.DxClient = null;
        if (client is not null)
        {
            await client.DisposeAsync().ConfigureAwait(false);
            return new OpcResultDto(0, "DX client disconnected.", Succeeded: true);
        }

        return new OpcResultDto(1, "DX client was not connected.", Succeeded: false);
    }

    private DxClientState GetDxClient(string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        return session.DxClient ?? throw new McpException($"Session '{sessionId}' is not connected to an OPC DX server. Call opcclassic.dx.connect first.");
    }

    private static OpcDxConnectionDto ToDto(DxConnection connection) => new(
        connection.Name,
        connection.Description,
        connection.ItemPath,
        connection.ItemName,
        connection.Version,
        connection.BrowsePaths,
        connection.Keyword,
        connection.DefaultSourceItemConnected,
        connection.DefaultTargetItemConnected,
        connection.DefaultOverridden,
        NormalizeVariant(connection.DefaultOverrideValue),
        NormalizeVariant(connection.SubstituteValue),
        connection.EnableSubstituteValue,
        connection.TargetItemPath,
        connection.TargetItemName,
        connection.SourceServerName,
        connection.SourceItemPath,
        connection.SourceItemName,
        connection.SourceItemQueueSize,
        connection.UpdateRateMilliseconds,
        connection.DeadbandPercent,
        connection.VendorData,
        connection.Mask);

    private static OpcDxSourceServerDto ToDto(DxSourceServer source) => new(
        source.Name,
        source.ServerUrl,
        source.Description,
        source.ServerType,
        source.ItemPath,
        source.ItemName,
        source.Version,
        source.DefaultConnected,
        source.Mask,
        source.Reserved);

    private static DxConnection ToConnection(OpcDxConnectionDto dto) => new(
        dto.Name,
        dto.Description,
        dto.ItemPath,
        dto.ItemName,
        dto.Version,
        dto.BrowsePaths,
        dto.Keyword,
        dto.DefaultSourceItemConnected,
        dto.DefaultTargetItemConnected,
        dto.DefaultOverridden,
        ToVariantOrNull(dto.DefaultOverrideValue),
        ToVariantOrNull(dto.SubstituteValue),
        dto.EnableSubstituteValue,
        dto.TargetItemPath,
        dto.TargetItemName,
        dto.SourceServerName,
        dto.SourceItemPath,
        dto.SourceItemName,
        dto.SourceItemQueueSize,
        dto.UpdateRateMilliseconds,
        dto.DeadbandPercent,
        dto.VendorData,
        dto.Mask);

    private static DxSourceServer ToSourceServer(OpcDxSourceServerDto dto) => new(
        dto.Name,
        dto.ServerUrl,
        dto.Description,
        dto.ServerType,
        dto.ItemPath,
        dto.ItemName,
        dto.Version,
        dto.DefaultConnected,
        dto.Mask,
        dto.Reserved);

    private static OpcVariant? ToVariantOrNull(object? value) => value is null ? null : ToVariant(value);

    private static OpcVariant ToVariant(object? value) => value switch
    {
        null => OpcVariant.Null,
        OpcVariant variant => variant,
        JsonElement element => ToVariant(element),
        bool boolean => OpcVariant.FromBoolean(boolean),
        int int32 => OpcVariant.FromInt32(int32),
        long int64 => OpcVariant.FromInt64(int64),
        float single => OpcVariant.FromSingle(single),
        double dbl => OpcVariant.FromDouble(dbl),
        decimal dec => OpcVariant.FromDouble((double)dec),
        DateTime dateTime => OpcVariant.FromDate(dateTime),
        DateTimeOffset dateTimeOffset => OpcVariant.FromDate(dateTimeOffset.UtcDateTime),
        Guid guid => OpcVariant.FromClsid(guid),
        string text => StringToVariant(text),
        _ => OpcVariant.FromString(value.ToString() ?? string.Empty),
    };

    private static OpcVariant ToVariant(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null or JsonValueKind.Undefined => OpcVariant.Null,
        JsonValueKind.True => OpcVariant.FromBoolean(true),
        JsonValueKind.False => OpcVariant.FromBoolean(false),
        JsonValueKind.Number when value.TryGetInt32(out int int32) => OpcVariant.FromInt32(int32),
        JsonValueKind.Number when value.TryGetInt64(out long int64) => OpcVariant.FromInt64(int64),
        JsonValueKind.Number when value.TryGetDouble(out double dbl) => OpcVariant.FromDouble(dbl),
        JsonValueKind.String => StringToVariant(value.GetString()),
        _ => OpcVariant.FromString(value.GetRawText()),
    };

    private static OpcVariant StringToVariant(string? value)
    {
        if (value is null)
        {
            return OpcVariant.Null;
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime dateTime))
        {
            return OpcVariant.FromDate(dateTime);
        }

        if (Guid.TryParse(value, out Guid guid))
        {
            return OpcVariant.FromClsid(guid);
        }

        return OpcVariant.FromString(value);
    }

    private static object? NormalizeVariant(OpcVariant? variant) => variant.HasValue ? NormalizeValue(OpcVariantConverter.ToObject(variant.Value)) : null;

    private static object? NormalizeValue(object? value) => value switch
    {
        DateTime dateTime => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
        OpcSafeArray safeArray => safeArray.ToString(),
        _ => value,
    };

    private static OpcResultDto ToResult(OpcResultId result, string? name, string successMessage) =>
        new(result.Code, result.IsSuccess ? successMessage : result.ToString(), result.IsSuccess, ItemName: name);

    private static OpcServerStatusDto ToDto(OpcServerStatus status) => new(
        status.Spec.ToString(),
        status.StartTime,
        status.CurrentTime,
        status.LastUpdateTime,
        status.State.ToString(),
        status.ServerVersion.ToString(),
        status.VendorInfo,
        status.GroupCount,
        status.BandWidth,
        status.MaxReturnValues,
        status.IsOperational);

    private static OpcSessionDto ToSessionDto(OpcSession session)
    {
        DxClientState? dx = session.DxClient;
        return new OpcSessionDto(
            session.SessionId,
            session.CreatedAt,
            session.LastUsedAt,
            session.LastUsedAt.Add(session.IdleExpiry),
            checked((int)Math.Ceiling(session.IdleExpiry.TotalSeconds)),
            dx is not null,
            dx?.Host,
            dx?.ProgId,
            dx?.Clsid);
    }

    private sealed class DefaultOpcDxConnectionFactory : IOpcDxConnectionFactory
    {
        public Task<DxClientState> ConnectAsync(DxConnectionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
            string? key = TryGetInMemoryKey(request.ConnectionString);
            if (key is not null)
            {
                if (!InMemoryDxConnectionRegistry.TryGet(key, out IOpcDxClient client))
                {
                    throw new McpException($"No in-memory DX client is registered for '{key}'.");
                }

                return Task.FromResult(new DxClientState("inmemory", request.ProgId ?? key, Guid.Empty, client));
            }

            throw new McpException("No OPC DX connection factory is registered for DCOM connections. Register IOpcDxConnectionFactory or use connectionString=inmemory://name.");
        }

        private static string? TryGetInMemoryKey(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return null;
            }

            if (Uri.TryCreate(connectionString, UriKind.Absolute, out Uri? uri)
                && uri.Scheme.Equals("inmemory", StringComparison.OrdinalIgnoreCase))
            {
                string key = uri.Host + uri.AbsolutePath.Trim('/');
                return string.IsNullOrWhiteSpace(key) ? null : key;
            }

            const string prefix = "inmemory:";
            return connectionString.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? connectionString[prefix.Length..].Trim('/')
                : null;
        }
    }
}
