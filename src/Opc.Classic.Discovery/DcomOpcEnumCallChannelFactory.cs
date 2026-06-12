//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Globalization;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Rpc.Auth.ntlm;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Transport;

namespace Opc.Classic.Discovery;

/// <summary>
/// Default OPCEnum channel factory for DCOM over ncacn_ip_tcp.
/// </summary>
public sealed class DcomOpcEnumCallChannelFactory : IOpcEnumCallChannelFactory
{
    private const int EndpointMapperPort = 135;
    private const ushort TcpTowerId = 0x07;

    private readonly DcomCallChannelFactory _channelFactory;
    private readonly Func<IAuthContext> _authContextFactory;

    /// <summary>
    /// Creates a default unauthenticated DCOM channel factory.
    /// </summary>
    public DcomOpcEnumCallChannelFactory()
        : this(
            new DcomCallChannelFactory(new TcpSocketTransportFactory()),
            static () => NoOpAuthContext.Instance,
            OpcProtectionLevel.Integrity)
    {
    }

    /// <summary>
    /// Creates a DCOM channel factory from OPC connection authentication settings.
    /// </summary>
    public DcomOpcEnumCallChannelFactory(OpcConnectData connectData)
        : this(
            new DcomCallChannelFactory(new TcpSocketTransportFactory()),
            CreateAuthContextFactory(connectData),
            NormalizeActivationProtection(connectData.ProtectionLevel))
    {
    }

    /// <summary>
    /// Creates a DCOM channel factory with injectable transport and authentication.
    /// </summary>
    public DcomOpcEnumCallChannelFactory(
        DcomCallChannelFactory channelFactory,
        Func<IAuthContext> authContextFactory)
        : this(channelFactory, authContextFactory, OpcProtectionLevel.Integrity)
    {
    }

    /// <summary>
    /// Creates a DCOM channel factory with injectable transport, authentication, and activation protection.
    /// </summary>
    public DcomOpcEnumCallChannelFactory(
        DcomCallChannelFactory channelFactory,
        Func<IAuthContext> authContextFactory,
        OpcProtectionLevel activationProtectionLevel)
    {
        ArgumentNullException.ThrowIfNull(channelFactory);
        ArgumentNullException.ThrowIfNull(authContextFactory);

        _channelFactory = channelFactory;
        _authContextFactory = authContextFactory;
        ActivationProtectionLevel = NormalizeActivationProtection(activationProtectionLevel);
    }

    /// <inheritdoc />
    public OpcProtectionLevel ActivationProtectionLevel { get; }

    /// <inheritdoc />
    public ValueTask<ICallChannel> CreateActivationChannelAsync(
        string host,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);

        // Activation channel binds the RPC endpoint mapper (port 135) which only
        // exposes IActivation / IRemoteSCMActivator. Do NOT pre-bind Discovery
        // interface IIDs here — port 135 will reject them with PROVIDER_REJECTION.
        return new ValueTask<ICallChannel>(ConnectAsync(new DnsEndPoint(host, EndpointMapperPort), OpcGuids.CLSID_OpcEnum, preBindIids: null, cancellationToken));
    }

    /// <inheritdoc />
    public ValueTask<ICallChannel> CreateObjectChannelAsync(
        string host,
        IOpcInterfaceRef interfaceRef,
        Guid interfaceId,
        CancellationToken cancellationToken = default) =>
        CreateObjectChannelAsync(host, interfaceRef, interfaceId, oxidBindings: default, cancellationToken);

    /// <inheritdoc />
    public ValueTask<ICallChannel> CreateObjectChannelAsync(
        string host,
        IOpcInterfaceRef interfaceRef,
        Guid interfaceId,
        ReadOnlyMemory<byte> oxidBindings,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(interfaceRef);

        DnsEndPoint endpoint = ResolveDataPortEndpoint(host, interfaceRef, oxidBindings);
        // Data-port channel binds the activated OPCEnum endpoint which exposes the
        // Discovery interface family. Pre-declare the full Discovery IID set so the
        // bind PDU's presentation-context list covers IOPCServerList(2),
        // IOPCEnumGUID, IEnumGUID, and IRemUnknown(2) in one round-trip and the
        // server preloads all stub marshalers. Route every RequestCoPdu to the
        // activated IPID — without that, OPCEnum cannot identify which object the
        // call targets and returns RPC_E_DISCONNECTED (0x80010108).
        return ConnectActivatedAsync(endpoint, interfaceRef.Ipid, OpcDiscoverySpecCatalog.Discovery, cancellationToken);
    }

    private async ValueTask<ICallChannel> ConnectActivatedAsync(EndPoint endpoint, Guid objectIpid, IReadOnlyList<Guid> preBindIids, CancellationToken cancellationToken) =>
        await _channelFactory.ConnectActivatedAsync(endpoint, _authContextFactory(), objectIpid, preBindIids, cancellationToken).ConfigureAwait(false);

    private static DnsEndPoint ResolveDataPortEndpoint(string fallbackHost, IOpcInterfaceRef interfaceRef, ReadOnlyMemory<byte> oxidBindings)
    {
        // The OBJREF's own ResolverBindings carry only the OXID-resolver address
        // (typically port 135 / the RPC endpoint mapper) and lack the data-port
        // suffix. Prefer the OXID-level DUALSTRINGARRAY returned by the activation
        // call when available — it includes the per-interface data port (e.g.
        // ncacn_ip_tcp:HOST[57539]) and lets us bind the activated object directly.
        if (!oxidBindings.IsEmpty
            && TryResolveDataPortFromOxidBindings(fallbackHost, oxidBindings.Span, out DnsEndPoint? dataPortEndpoint))
        {
            return dataPortEndpoint!;
        }

        return ResolveObjectEndpoint(fallbackHost, interfaceRef);
    }

    private static bool TryResolveDataPortFromOxidBindings(string fallbackHost, ReadOnlySpan<byte> bindings, out DnsEndPoint? endpoint)
    {
        endpoint = null;
        if (bindings.Length < 4)
        {
            return false;
        }

        ushort secOffset = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bindings.Slice(2));
        int idx = 4;
        int entriesConsumed = 2;
        while (idx + 2 <= bindings.Length && entriesConsumed < secOffset)
        {
            ushort tower = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bindings.Slice(idx));
            idx += 2;
            entriesConsumed++;
            if (tower == 0)
            {
                return false;
            }

            string address = ReadNullTerminatedWideString(bindings, ref idx, ref entriesConsumed, secOffset);
            if (tower != TcpTowerId)
            {
                continue;
            }

            if (TryParseHostPort(address, fallbackHost, out string host, out int port))
            {
                endpoint = new DnsEndPoint(host, port);
                return true;
            }
        }

        return false;
    }

    private static string ReadNullTerminatedWideString(ReadOnlySpan<byte> bindings, ref int idx, ref int entriesConsumed, ushort secOffset)
    {
        var sb = new System.Text.StringBuilder();
        while (idx + 2 <= bindings.Length && entriesConsumed < secOffset)
        {
            ushort ch = System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bindings.Slice(idx));
            idx += 2;
            entriesConsumed++;
            if (ch == 0)
            {
                break;
            }

            sb.Append((char)ch);
        }

        return sb.ToString();
    }

    private static bool TryParseHostPort(string address, string fallbackHost, out string host, out int port)
    {
        host = string.Empty;
        port = 0;
        int bracket = address.LastIndexOf('[');
        if (bracket < 0 || !address.EndsWith(']'))
        {
            return false;
        }

        string portText = address.Substring(bracket + 1, address.Length - bracket - 2);
        if (!int.TryParse(portText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out port))
        {
            return false;
        }

        host = address.Substring(0, bracket);
        if (string.IsNullOrWhiteSpace(host))
        {
            host = fallbackHost;
        }

        return true;
    }

    private Task<ICallChannel> ConnectAsync(EndPoint endpoint, Guid clsidToActivate, IReadOnlyList<Guid>? preBindIids, CancellationToken cancellationToken) =>
        _channelFactory.ConnectAsync(endpoint, clsidToActivate, _authContextFactory(), preBindIids, cancellationToken);

    private static Func<IAuthContext> CreateAuthContextFactory(OpcConnectData connectData)
    {
        ArgumentNullException.ThrowIfNull(connectData);
        OpcConnectData activationConnectData = NormalizeConnectData(connectData);
        return () => NtlmAuthentication.CreateAuthContext(activationConnectData);
    }

    private static OpcConnectData NormalizeConnectData(OpcConnectData connectData)
    {
        OpcProtectionLevel activationProtection = NormalizeActivationProtection(connectData.ProtectionLevel);
        return activationProtection == connectData.ProtectionLevel
            ? connectData
            : new OpcConnectData(
                connectData.Url,
                connectData.Credentials,
                connectData.AuthMode,
                activationProtection,
                connectData.OperationTimeout,
                connectData.ChannelBindings);
    }

    private static OpcProtectionLevel NormalizeActivationProtection(OpcProtectionLevel protectionLevel) =>
        protectionLevel == OpcProtectionLevel.Privacy ? OpcProtectionLevel.Privacy : OpcProtectionLevel.Integrity;

    private static DnsEndPoint ResolveObjectEndpoint(string fallbackHost, IOpcInterfaceRef interfaceRef)
    {
        if (TryFindTcpBinding(interfaceRef.ResolverBindings, out string? host, out int port))
        {
            return new DnsEndPoint(string.IsNullOrWhiteSpace(host) ? fallbackHost : host, port);
        }

        return new DnsEndPoint(fallbackHost, EndpointMapperPort);
    }

    private static bool TryFindTcpBinding(
        System.Collections.Generic.IReadOnlyList<ushort> entries,
        out string? host,
        out int port)
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

    private static string ReadNullTerminatedString(System.Collections.Generic.IReadOnlyList<ushort> entries, ref int index)
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
        if (bracketStart < 0 || !networkAddress.EndsWith(']'))
        {
            return;
        }

        string portText = networkAddress.Substring(bracketStart + 1, networkAddress.Length - bracketStart - 2);
        if (int.TryParse(portText, NumberStyles.None, CultureInfo.InvariantCulture, out int parsedPort)
            && parsedPort is > 0 and <= 65535)
        {
            port = parsedPort;
            host = networkAddress[..bracketStart];
        }
    }

    private sealed class TcpSocketTransportFactory : IAsyncTransportFactory
    {
        public async ValueTask<IAsyncTransport> ConnectAsync(
            EndPoint endpoint,
            CancellationToken cancellationToken = default)
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
}
