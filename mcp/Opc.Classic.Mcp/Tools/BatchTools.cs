//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Buffers.Binary;
using System.ComponentModel;
using System.Globalization;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Opc.Classic.Batch;
using Opc.Classic.Batch.Dcom;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Core;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Discovery;
using Opc.Classic.Mcp.Dtos;
using Opc.Classic.Mcp.Sessions;
using Opc.Classic.Ndr;
using Opc.Classic.Transport;

namespace Opc.Classic.Mcp.Tools;

/// <summary>Creates Batch client state for a session.</summary>
public interface IOpcBatchConnectionFactory
{
    /// <summary>Connects to a Batch server and returns a client state object.</summary>
    Task<BatchClientState> ConnectAsync(BatchConnectionRequest request, CancellationToken cancellationToken = default);
}

/// <summary>Connection request used by Batch tools.</summary>
public sealed record BatchConnectionRequest(
    string Host,
    string? ProgId,
    string? Clsid,
    string? Username,
    string? Password,
    bool UseKerberos,
    string? ConnectionString,
    string? AuthLevel = null);

/// <summary>Registers in-memory Batch call channels for MCP tests and loopback scenarios.</summary>
public static class InMemoryBatchConnectionRegistry
{
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, ICallChannel> Channels = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Registers an in-memory Batch call channel by name.</summary>
    public static IDisposable Register(string name, ICallChannel channel)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(channel);

        Channels[name] = channel;
        return new Registration(name);
    }

    internal static bool TryGet(string name, out ICallChannel channel) => Channels.TryGetValue(name, out channel!);

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
            Channels.TryRemove(_name, out _);
        }
    }
}

/// <summary>MCP tools for OPC Batch client operations.</summary>
public sealed class BatchTools
{
    private readonly IOpcSessionManager _sessionManager;
    private readonly IOpcBatchConnectionFactory _connectionFactory;

    /// <summary>Creates the Batch tool set.</summary>
    public BatchTools(IOpcSessionManager sessionManager, IEnumerable<IOpcBatchConnectionFactory> connectionFactories)
    {
        _sessionManager = sessionManager ?? throw new ArgumentNullException(nameof(sessionManager));
        ArgumentNullException.ThrowIfNull(connectionFactories);
        _connectionFactory = connectionFactories.FirstOrDefault() ?? new DefaultOpcBatchConnectionFactory();
    }

    /// <summary>Connects a session to an OPC Batch server.</summary>
    [McpServerTool(Name = "opcclassic.batch.connect", ReadOnly = false, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Connects an existing MCP session to an OPC Batch server using DCOM or an in-memory test channel.")]
    public async Task<OpcResultDto> Connect(
        [Description("The sessionId returned by opcclassic.session.create.")]
        string sessionId,
        [Description("OPC Batch server host name or IP address. Ignored when connectionString uses inmemory://.")]
        string host = "localhost",
        [Description("OPC Batch server ProgID. Optional when clsid or connectionString is supplied.")]
        string? progId = null,
        [Description("OPC Batch server CLSID as a GUID string. Optional when progId or connectionString is supplied.")]
        string? clsid = null,
        [Description("Optional user name for NTLMv2 or Kerberos authentication. Use DOMAIN\\user when a Windows domain is required.")]
        string? username = null,
        [Description("Optional password for NTLMv2 or Kerberos authentication. Omit only for anonymous or in-memory connections.")]
        string? password = null,
        [Description("True to request Kerberos/SPNEGO authentication instead of NTLMv2 when credentials are supplied.")]
        bool useKerberos = false,
        [Description("Optional connection string. Use inmemory://name for a registered InMemoryBatchConnectionRegistry channel, or dcom://host/ProgID for DCOM.")]
        string? connectionString = null,
        [Description(OpcMcpAuthLevel.Description)]
        string? authLevel = null,
        CancellationToken cancellationToken = default)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        BatchClientState client = await _connectionFactory.ConnectAsync(
            new BatchConnectionRequest(host, progId, clsid, username, password, useKerberos, connectionString, authLevel),
            cancellationToken).ConfigureAwait(false);

        BatchClientState? existing = session.BatchClient;
        session.BatchClient = client;
        if (existing is not null)
        {
            await existing.DisposeAsync().ConfigureAwait(false);
        }

        session.Touch();
        return new OpcResultDto(0, "Batch client connected.", Succeeded: true, ValueType: "Batch");
    }

    /// <summary>Gets OPC Batch connection status for a connected session.</summary>
    [McpServerTool(Name = "opcclassic.batch.get_status", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Gets Batch connection status and verifies the server delimiter when supported.")]
    public async Task<OpcResultDto> GetStatus(
        [Description("The sessionId returned by opcclassic.session.create and connected with opcclassic.batch.connect.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        BatchClientState client = GetBatchClient(sessionId);
        string delimiter = await client.BatchServer.GetDelimiterAsync(cancellationToken).ConfigureAwait(false);
        return new OpcResultDto(0, $"Batch client connected to {client.Host}; delimiter='{delimiter}'.", Succeeded: true, ValueType: "Batch");
    }

    /// <summary>Disconnects a session from its OPC Batch server.</summary>
    [McpServerTool(Name = "opcclassic.batch.disconnect", ReadOnly = false, Idempotent = true, Destructive = true, OpenWorld = true)]
    [Description("Disconnects the session from its OPC Batch server and releases the Batch channel.")]
    public async Task<OpcResultDto> Disconnect(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        BatchClientState? client = session.BatchClient;
        session.BatchClient = null;
        if (client is not null)
        {
            await client.DisposeAsync().ConfigureAwait(false);
            return new OpcResultDto(0, "Batch client disconnected.", Succeeded: true, ValueType: "Batch");
        }

        return new OpcResultDto(1, "Batch client was not connected.", Succeeded: false, ValueType: "Batch");
    }

    /// <summary>Queries OPC Batch summary records using Batch 2.0 filters.</summary>
    [McpServerTool(Name = "opcclassic.batch.query_batch_summaries", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Queries OPC Batch summaries with optional Batch 2.0 filter fields and returns JSON-friendly summary DTOs.")]
    public async Task<IReadOnlyList<OpcBatchSummaryDto>> QueryBatchSummaries(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Optional batch identifier substring filter.")]
        string? id = null,
        [Description("Optional batch description substring filter.")]
        string? description = null,
        [Description("Optional OPC item identifier substring filter.")]
        string? opcItemId = null,
        [Description("Optional master recipe identifier substring filter.")]
        string? masterRecipeId = null,
        [Description("Optional minimum batch size. Omit to leave unbounded.")]
        float? minBatchSize = null,
        [Description("Optional maximum batch size. Omit to leave unbounded.")]
        float? maxBatchSize = null,
        [Description("Optional engineering-units substring filter.")]
        string? engineeringUnits = null,
        [Description("Optional execution-state substring filter, such as RUNNING or COMPLETE.")]
        string? executionState = null,
        [Description("Optional execution-mode substring filter, such as AUTOMATIC or MANUAL.")]
        string? executionMode = null,
        [Description("Optional minimum actual start time, as an ISO-8601 timestamp.")]
        DateTimeOffset? minStartTime = null,
        [Description("Optional maximum actual start time, as an ISO-8601 timestamp.")]
        DateTimeOffset? maxStartTime = null,
        [Description("Optional minimum actual end time, as an ISO-8601 timestamp.")]
        DateTimeOffset? minEndTime = null,
        [Description("Optional maximum actual end time, as an ISO-8601 timestamp.")]
        DateTimeOffset? maxEndTime = null,
        [Description("Batch model string passed to Batch 2.0 filtered enumeration. Defaults to OPCBBatchModel.")]
        string model = "OPCBBatchModel",
        [Description("Maximum summaries to return. Use 0 to request up to 1000 summaries.")]
        int maxResults = 100,
        CancellationToken cancellationToken = default)
    {
        BatchClientState client = GetBatchClient(sessionId);
        // Unbounded-min defaults use FileTimeHelper.Epoch (1601-01-01) because
        // DateTimeOffset.MinValue (year 0001) would encode as a negative FILETIME
        // which is invalid per the Windows FILETIME spec and rejected by the
        // strict decode path. Unbounded-max stays at DateTimeOffset.MaxValue
        // (year 9999) which is the upper bound of the valid FILETIME range.
        OpcBatchSummaryFilter filter = new(
            Normalize(id),
            Normalize(description),
            Normalize(opcItemId),
            Normalize(masterRecipeId),
            minBatchSize ?? float.MinValue,
            maxBatchSize ?? float.MaxValue,
            Normalize(engineeringUnits),
            Normalize(executionState),
            Normalize(executionMode),
            minStartTime ?? FileTimeHelper.Epoch,
            maxStartTime ?? DateTimeOffset.MaxValue,
            minEndTime ?? FileTimeHelper.Epoch,
            maxEndTime ?? DateTimeOffset.MaxValue);

        IEnumOPCBatchSummary enumerator = await CreateSummaryEnumeratorAsync(client, filter, model, cancellationToken).ConfigureAwait(false);
        int remaining = maxResults <= 0 ? 1000 : maxResults;
        var summaries = new List<OpcBatchSummaryDto>();
        while (remaining > 0)
        {
            int requestCount = Math.Min(remaining, 100);
            OpcBatchSummary[] batch = await enumerator.NextAsync(requestCount, cancellationToken).ConfigureAwait(false);
            if (batch.Length == 0)
            {
                break;
            }

            summaries.AddRange(batch.Select(ToDto));
            remaining -= batch.Length;
            if (batch.Length < requestCount)
            {
                break;
            }
        }

        return summaries;
    }

    /// <summary>Queries the Batch enumeration-set catalog.</summary>
    [McpServerTool(Name = "opcclassic.batch.query_enumeration_sets", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Queries the OPC Batch enumeration-set IDs and names exposed by IOPCEnumerationSets.")]
    public async Task<IReadOnlyList<OpcBatchEnumerationSetDto>> QueryEnumerationSets(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        BatchClientState client = GetBatchClient(sessionId);
        await client.EnumerationSets.QueryEnumerationSetsAsync(out int[] ids, out string[] names, cancellationToken).ConfigureAwait(false);
        int count = Math.Min(ids.Length, names.Length);
        var results = new List<OpcBatchEnumerationSetDto>(count);
        for (int i = 0; i < count; i++)
        {
            results.Add(new OpcBatchEnumerationSetDto(ids[i], names[i]));
        }

        return results;
    }

    /// <summary>Queries a Batch enumeration value name.</summary>
    [McpServerTool(Name = "opcclassic.batch.query_enumeration", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Queries the display name for a single OPC Batch enumeration value.")]
    public async Task<OpcBatchEnumerationDto> QueryEnumeration(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Enumeration set ID returned by opcclassic.batch.query_enumeration_sets.")]
        int enumerationSetId,
        [Description("Numeric enumeration value to resolve.")]
        int enumerationValue,
        CancellationToken cancellationToken = default)
    {
        BatchClientState client = GetBatchClient(sessionId);
        string name = await client.EnumerationSets.QueryEnumerationAsync(enumerationSetId, enumerationValue, cancellationToken).ConfigureAwait(false);
        return new OpcBatchEnumerationDto(enumerationSetId, enumerationValue, name);
    }

    /// <summary>Queries all Batch enumeration values in a set.</summary>
    [McpServerTool(Name = "opcclassic.batch.query_enumeration_list", ReadOnly = true, Idempotent = true, Destructive = false, OpenWorld = true)]
    [Description("Queries the complete OPC Batch enumeration-value list for an enumeration set.")]
    public async Task<IReadOnlyList<OpcBatchEnumerationDto>> QueryEnumerationList(
        [Description("The connected OPC Classic sessionId.")]
        string sessionId,
        [Description("Enumeration set ID returned by opcclassic.batch.query_enumeration_sets.")]
        int enumerationSetId,
        CancellationToken cancellationToken = default)
    {
        BatchClientState client = GetBatchClient(sessionId);
        await client.EnumerationSets.QueryEnumerationListAsync(enumerationSetId, out int[] values, out string[] names, cancellationToken).ConfigureAwait(false);
        int count = Math.Min(values.Length, names.Length);
        var results = new List<OpcBatchEnumerationDto>(count);
        for (int i = 0; i < count; i++)
        {
            results.Add(new OpcBatchEnumerationDto(enumerationSetId, values[i], names[i]));
        }

        return results;
    }

    private BatchClientState GetBatchClient(string sessionId)
    {
        OpcSession session = _sessionManager.GetSession(sessionId);
        return session.BatchClient ?? throw new McpException($"Session '{sessionId}' is not connected to an OPC Batch server. Call opcclassic.batch.connect first.");
    }

    private static async Task<IEnumOPCBatchSummary> CreateSummaryEnumeratorAsync(BatchClientState client, OpcBatchSummaryFilter filter, string model, CancellationToken cancellationToken)
    {
        try
        {
            _ = await client.BatchServer2.CreateFilteredEnumeratorAsync(
                IEnumOPCBatchSummary.InterfaceId,
                filter,
                string.IsNullOrWhiteSpace(model) ? "OPCBBatchModel" : model,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OpcException ex) when (ex.ResultId.Code == OpcResultId.NotImplemented.Code)
        {
            _ = await client.BatchServer.CreateEnumeratorAsync(IEnumOPCBatchSummary.InterfaceId, cancellationToken).ConfigureAwait(false);
        }

        return new IEnumOPCBatchSummaryClientProxy(client.Channel);
    }

    private static OpcBatchSummaryDto ToDto(OpcBatchSummary summary) =>
        new(
            summary.Id,
            summary.Description,
            summary.OpcItemId,
            summary.MasterRecipeId,
            summary.BatchSize,
            summary.EngineeringUnits,
            summary.ExecutionState,
            summary.ExecutionMode,
            summary.ActualStartTime,
            summary.ActualEndTime);

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed class DefaultOpcBatchConnectionFactory : IOpcBatchConnectionFactory
    {
        public Task<BatchClientState> ConnectAsync(BatchConnectionRequest request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            return OpcClassicDcomConnectionFactory.ConnectAsync(
                new OpcClassicConnectionRequest(request.Host, request.ProgId, request.Clsid, request.Username, request.Password, request.UseKerberos, request.ConnectionString, request.AuthLevel),
                IOPCBatchServer.InterfaceId,
                OpcGuids.BatchCategoryIds,
                static (host, progId, clsid, channel, ownsChannel) => new BatchClientState(host, progId, clsid, channel, ownsChannel),
                InMemoryBatchConnectionRegistry.TryGet,
                "Batch",
                cancellationToken);
        }
    }
}

internal sealed record OpcClassicConnectionRequest(
    string Host,
    string? ProgId,
    string? Clsid,
    string? Username,
    string? Password,
    bool UseKerberos,
    string? ConnectionString,
    string? AuthLevel = null);

internal delegate bool TryGetOpcClassicInMemoryChannel(string name, out ICallChannel channel);

internal static class OpcClassicDcomConnectionFactory
{
    private const int EndpointMapperPort = 135;
    private const int RemoteCreateInstanceOpnum = 4;
    private const int ClassContext = 0x14;
    private const int RpcProtocolSequenceTcp = 7;
    private const int DefaultPayloadSize = 4096;
    private const int MaximumPayloadSize = 65536;
    private const uint ObjRefSignature = 0x574F454D;
    private const ushort TcpTowerId = 0x07;
    private static readonly Guid RemoteScmActivatorInterfaceId = new("000001A0-0000-0000-C000-000000000046");

    public static async Task<TClient> ConnectAsync<TClient>(
        OpcClassicConnectionRequest request,
        Guid requestedInterfaceId,
        IReadOnlyList<Guid> categoryIds,
        Func<string, string?, Guid?, ICallChannel, bool, TClient> createClient,
        TryGetOpcClassicInMemoryChannel tryGetInMemoryChannel,
        string specName,
        CancellationToken cancellationToken = default)
        where TClient : class
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(categoryIds);
        ArgumentNullException.ThrowIfNull(createClient);
        ArgumentNullException.ThrowIfNull(tryGetInMemoryChannel);

        OpcClassicConnectionRequest normalized = NormalizeRequest(request);
        if (TryCreateInMemoryClient(normalized, createClient, tryGetInMemoryChannel, specName, out TClient? inMemoryClient))
        {
            return inMemoryClient ?? throw new InvalidOperationException("In-memory connection factory returned no client.");
        }

        Guid clsid = await ResolveClsidAsync(normalized, categoryIds, specName, cancellationToken).ConfigureAwait(false);
        var channelFactory = new DcomCallChannelFactory(new TcpSocketTransportFactory());
        ICallChannel? activationChannel = null;
        try
        {
            IAuthContext activationAuth = CreateAuthContext(normalized, clsid);
            activationChannel = await channelFactory.ConnectAsync(
                new DnsEndPoint(normalized.Host, EndpointMapperPort),
                clsid,
                activationAuth,
                cancellationToken).ConfigureAwait(false);
            byte[] payload = EncodeRemoteCreateInstanceRequest(normalized.Host, clsid, requestedInterfaceId, activationAuth.ProtectionLevel);
            NdrCallResult activationResult = await activationChannel.InvokeAsync(
                RemoteScmActivatorInterfaceId,
                RemoteCreateInstanceOpnum,
                payload,
                cancellationToken).ConfigureAwait(false);
            IOpcInterfaceRef serverRef = DecodeRemoteCreateInstanceResponse(activationResult);
            EndPoint endpoint = ResolveObjectEndpoint(normalized.Host, serverRef);
            ICallChannel serverChannel = await channelFactory.ConnectAsync(
                endpoint,
                Guid.Empty,
                CreateAuthContext(normalized, clsid),
                cancellationToken).ConfigureAwait(false);
            return createClient(normalized.Host, normalized.ProgId, clsid, serverChannel, true);
        }
        finally
        {
            await DisposeChannelAsync(activationChannel).ConfigureAwait(false);
        }
    }

    private static OpcClassicConnectionRequest NormalizeRequest(OpcClassicConnectionRequest request)
    {
        string host = string.IsNullOrWhiteSpace(request.Host) ? "localhost" : request.Host.Trim();
        string? progId = NormalizeText(request.ProgId);
        string? clsid = NormalizeText(request.Clsid);
        string? connectionString = NormalizeText(request.ConnectionString);
        if (connectionString is not null && Uri.TryCreate(connectionString, UriKind.Absolute, out Uri? uri))
        {
            if (uri.Scheme.Equals("inmemory", StringComparison.OrdinalIgnoreCase))
            {
                return request with { Host = host, ConnectionString = connectionString };
            }

            host = string.IsNullOrWhiteSpace(uri.Host) ? host : uri.Host;
            string pathValue = uri.AbsolutePath.Trim('/');
            if (!string.IsNullOrWhiteSpace(pathValue))
            {
                if (Guid.TryParse(pathValue, out _))
                {
                    clsid = pathValue;
                }
                else
                {
                    progId = pathValue;
                }
            }
        }

        return request with { Host = host, ProgId = progId, Clsid = clsid, ConnectionString = connectionString };
    }

    private static bool TryCreateInMemoryClient<TClient>(
        OpcClassicConnectionRequest request,
        Func<string, string?, Guid?, ICallChannel, bool, TClient> createClient,
        TryGetOpcClassicInMemoryChannel tryGetInMemoryChannel,
        string specName,
        out TClient? client)
        where TClient : class
    {
        string? key = TryGetInMemoryKey(request.ConnectionString);
        if (key is null)
        {
            client = null;
            return false;
        }

        if (!tryGetInMemoryChannel(key, out ICallChannel channel))
        {
            throw new McpException($"No in-memory {specName} channel is registered for '{key}'.");
        }

        client = createClient("inmemory", request.ProgId ?? key, Guid.Empty, channel, false);
        return true;
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

    private static async Task<Guid> ResolveClsidAsync(OpcClassicConnectionRequest request, IReadOnlyList<Guid> categoryIds, string specName, CancellationToken cancellationToken)
    {
        if (Guid.TryParse(request.Clsid, out Guid clsid))
        {
            return clsid;
        }

        if (Guid.TryParse(request.ProgId, out clsid))
        {
            return clsid;
        }

        if (string.IsNullOrWhiteSpace(request.ProgId))
        {
            throw new McpException($"Provide an OPC {specName} server ProgID, CLSID, or connectionString.");
        }

        OpcConnectData? discoveryConnectData = CreateDiscoveryConnectData(request);
        OpcServerDescriptor[] servers = discoveryConnectData is null
            ? await OpcDiscovery.EnumerateAsync(
                request.Host,
                categoryIds,
                cancellationToken).ConfigureAwait(false)
            : await OpcDiscovery.EnumerateAsync(
                request.Host,
                discoveryConnectData,
                categoryIds,
                cancellationToken).ConfigureAwait(false);
        OpcServerDescriptor? match = servers.FirstOrDefault(server =>
            string.Equals(server.ProgId, request.ProgId, StringComparison.OrdinalIgnoreCase)
            || string.Equals(server.VerIndProgId, request.ProgId, StringComparison.OrdinalIgnoreCase));
        return match?.ClassId ?? throw new McpException($"OPC {specName} ProgID '{request.ProgId}' was not found on host '{request.Host}'.");
    }

    private static IAuthContext CreateAuthContext(OpcClassicConnectionRequest request, Guid clsid)
    {
        NetworkCredential? credentials = CreateCredential(request.Username, request.Password);
        OpcUrl url = OpcUrl.Parse($"opcda://{request.Host}/{(request.ProgId ?? clsid.ToString("D"))}");
        OpcProtectionLevel protectionLevel = OpcMcpAuthLevel.ParseOrDefault(request.AuthLevel);
        OpcConnectData connectData = credentials is null
            ? new OpcConnectData(url, credentials: null, authMode: OpcAuthMode.Anonymous, protectionLevel: protectionLevel)
            : request.UseKerberos
                ? OpcConnectData.WithKerberos(url, credentials, protectionLevel)
                : OpcConnectData.WithNtlmV2(url, credentials, protectionLevel);
        return NtlmAuthentication.CreateAuthContext(connectData);
    }

    private static OpcConnectData? CreateDiscoveryConnectData(OpcClassicConnectionRequest request)
    {
        NetworkCredential? credentials = CreateCredential(request.Username, request.Password);
        OpcProtectionLevel protectionLevel = OpcMcpAuthLevel.ParseOrDefault(request.AuthLevel);
        if (credentials is null)
        {
            return OpcMcpAuthLevel.IsSpecified(request.AuthLevel)
                ? new OpcConnectData(OpcUrl.Parse($"opcda://{request.Host}/OPC.ServerList.1"), credentials: null, authMode: OpcAuthMode.Anonymous, protectionLevel: protectionLevel)
                : null;
        }

        OpcUrl url = OpcUrl.Parse($"opcda://{request.Host}/OPC.ServerList.1");
        return request.UseKerberos
            ? OpcConnectData.WithKerberos(url, credentials, protectionLevel)
            : OpcConnectData.WithNtlmV2(url, credentials, protectionLevel);
    }

    private static NetworkCredential? CreateCredential(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return null;
        }

        string user = username.Trim();
        string domain = string.Empty;
        int slash = user.IndexOf('\\', StringComparison.Ordinal);
        if (slash > 0 && slash < user.Length - 1)
        {
            domain = user[..slash];
            user = user[(slash + 1)..];
        }

        return new NetworkCredential(user, password ?? string.Empty, domain);
    }

    private static byte[] EncodeRemoteCreateInstanceRequest(
        string host,
        Guid clsid,
        Guid requestedIid,
        OpcProtectionLevel activationProtectionLevel)
    {
        var activationProperties = new ActivationProperties(
            new SpecialPropertiesData(ActivationComVersion.V5_6, Mode: 0, ClassContext, requestedIid, Array.Empty<int>()),
            new InstanceInfo(clsid, requestedIid, ClassContext, Mode: 0),
            new LocationInfo(host, Environment.ProcessId, new[] { RpcProtocolSequenceTcp }),
            null,
            new SecurityInfo(ToActivationAuthenticationLevel(activationProtectionLevel), ImpersonationLevel: 3, Capabilities: 0));
        byte[] encodedProperties = ActivationInfoCodec.Encode(activationProperties);

        return WritePayload((ref NdrWriter writer) =>
        {
            writer.WriteGuid(clsid);
            writer.WriteGuid(requestedIid);
            writer.WriteUInt32(1);
            writer.WriteInt32(RpcProtocolSequenceTcp);
            writer.WriteUInt32((uint)encodedProperties.Length);
            writer.WriteRawBytes(encodedProperties);
        });
    }

    private static OpcProtectionLevel NormalizeActivationProtection(OpcProtectionLevel protectionLevel) =>
        protectionLevel == OpcProtectionLevel.Privacy ? OpcProtectionLevel.Privacy : OpcProtectionLevel.Integrity;

    private static int ToActivationAuthenticationLevel(OpcProtectionLevel protectionLevel) =>
        (int)NormalizeActivationProtection(protectionLevel);

    private static IOpcInterfaceRef DecodeRemoteCreateInstanceResponse(NdrCallResult result)
    {
        OpcException.ThrowIfFailed(new OpcResultId(result.Hresult, null), "IRemoteSCMActivator::RemoteCreateInstance");
        if (result.ResponsePayload.IsEmpty)
        {
            throw new InvalidOperationException("RemoteCreateInstance did not return an OPC OBJREF.");
        }

        ReadOnlySpan<byte> response = result.ResponsePayload.Span;
        if (TryDecodeObjRef(response, out IOpcInterfaceRef? directObjRef))
        {
            return directObjRef!;
        }

        if (TryDecodeActivationProperties(response, out IOpcInterfaceRef? activationObjRef))
        {
            return activationObjRef!;
        }

        return DecodeLengthPrefixedObjRef(response);
    }

    private static IOpcInterfaceRef DecodeLengthPrefixedObjRef(ReadOnlySpan<byte> response)
    {
        var reader = new NdrReader(response);
        int innerHresult = reader.ReadInt32();
        OpcException.ThrowIfFailed(new OpcResultId(innerHresult, null), "IRemoteSCMActivator::RemoteCreateInstance");
        uint objRefLength = reader.ReadUInt32();
        if (objRefLength > reader.RemainingBytes)
        {
            throw new InvalidOperationException("RemoteCreateInstance OBJREF length exceeds the remaining response payload.");
        }

        byte[] objRefBytes = reader.ReadRawBytes((int)objRefLength).ToArray();
        if (TryDecodeObjRef(objRefBytes, out IOpcInterfaceRef? objRef))
        {
            return objRef!;
        }

        throw new InvalidOperationException("RemoteCreateInstance returned an invalid OPC OBJREF.");
    }

    private static bool TryDecodeActivationProperties(ReadOnlySpan<byte> response, out IOpcInterfaceRef? objRef)
    {
        objRef = null;
        if (!ActivationInfoCodec.TryDecode(response, out ActivationProperties properties)
            || properties.ScmReplyInfo?.ObjRef is not { Length: > 0 } objRefBytes)
        {
            return false;
        }

        return TryDecodeObjRef(objRefBytes, out objRef);
    }

    private static bool TryDecodeObjRef(ReadOnlySpan<byte> payload, out IOpcInterfaceRef? objRef)
    {
        objRef = null;
        if (payload.Length < sizeof(uint) || BinaryPrimitives.ReadUInt32LittleEndian(payload) != ObjRefSignature)
        {
            return false;
        }

        try
        {
            var reader = new NdrReader(payload);
            objRef = OpcInterfaceRefCodec.Read(ref reader);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static EndPoint ResolveObjectEndpoint(string fallbackHost, IOpcInterfaceRef interfaceRef)
    {
        if (TryFindTcpBinding(interfaceRef.ResolverBindings, out string? host, out int port))
        {
            return new DnsEndPoint(string.IsNullOrWhiteSpace(host) ? fallbackHost : host, port);
        }

        return new DnsEndPoint(fallbackHost, EndpointMapperPort);
    }

    private static bool TryFindTcpBinding(IReadOnlyList<ushort> entries, out string? host, out int port)
    {
        host = null;
        port = EndpointMapperPort;
        for (int index = 0; index < entries.Count;)
        {
            ushort towerId = entries[index++];
            if (towerId == 0)
            {
                return false;
            }

            string networkAddress = ReadNullTerminatedString(entries, ref index);
            if (towerId != TcpTowerId)
            {
                continue;
            }

            ParseNetworkAddress(networkAddress, out host, out port);
            return true;
        }

        return false;
    }

    private static string ReadNullTerminatedString(IReadOnlyList<ushort> entries, ref int index)
    {
        var chars = new char[Math.Max(0, entries.Count - index)];
        int length = 0;
        while (index < entries.Count)
        {
            ushort value = entries[index++];
            if (value == 0)
            {
                break;
            }

            chars[length++] = (char)value;
        }

        return new string(chars, 0, length);
    }

    private static void ParseNetworkAddress(string networkAddress, out string? host, out int port)
    {
        host = networkAddress;
        port = EndpointMapperPort;
        int bracketStart = networkAddress.LastIndexOf('[');
        if (bracketStart < 0 || !networkAddress.EndsWith("]", StringComparison.Ordinal))
        {
            return;
        }

        string portText = networkAddress[(bracketStart + 1)..^1];
        if (int.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out int parsedPort))
        {
            port = parsedPort;
            host = networkAddress[..bracketStart];
        }
    }

    private static byte[] WritePayload(NdrWriteAction action)
    {
        for (int size = DefaultPayloadSize; size <= MaximumPayloadSize; size *= 2)
        {
            var buffer = new byte[size];
            var writer = new NdrWriter(buffer);
            try
            {
                action(ref writer);
                return buffer[..writer.Position];
            }
            catch (InvalidOperationException) when (size < MaximumPayloadSize)
            {
            }
        }

        throw new InvalidOperationException("Unable to encode the DCOM activation payload.");
    }

    private delegate void NdrWriteAction(ref NdrWriter writer);

    private sealed class TcpSocketTransportFactory : IAsyncTransportFactory
    {
        public async ValueTask<IAsyncTransport> ConnectAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(endpoint);
            var client = new TcpClient();
            try
            {
                switch (endpoint)
                {
                    case DnsEndPoint dns:
                        await client.ConnectAsync(dns.Host, dns.Port, cancellationToken).ConfigureAwait(false);
                        break;
                    case IPEndPoint ip:
                        await client.ConnectAsync(ip.Address, ip.Port, cancellationToken).ConfigureAwait(false);
                        break;
                    default:
                        throw new NotSupportedException($"Endpoint type '{endpoint.GetType().FullName}' is not supported.");
                }

                return new TcpSocketTransport(client);
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }
    }

    private sealed class TcpSocketTransport : IAsyncTransport
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        public TcpSocketTransport(TcpClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _stream = client.GetStream();
            Input = PipeReader.Create(_stream);
            Output = PipeWriter.Create(_stream);
            RemoteEndpoint = client.Client.RemoteEndPoint ?? new IPEndPoint(IPAddress.None, 0);
        }

        public EndPoint RemoteEndpoint { get; }
        public PipeReader Input { get; }
        public PipeWriter Output { get; }

        public async ValueTask FlushAsync(CancellationToken cancellationToken = default) =>
            await Output.FlushAsync(cancellationToken).ConfigureAwait(false);

        public async ValueTask DisposeAsync()
        {
            await Input.CompleteAsync().ConfigureAwait(false);
            await Output.CompleteAsync().ConfigureAwait(false);
            await _stream.DisposeAsync().ConfigureAwait(false);
            _client.Dispose();
        }
    }

    private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static async ValueTask DisposeChannelAsync(ICallChannel? channel)
    {
        if (channel is null)
        {
            return;
        }

        switch (channel)
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
