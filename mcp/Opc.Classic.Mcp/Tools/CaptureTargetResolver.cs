// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.Net;
using System.Diagnostics.CodeAnalysis;
using Opc.Classic.Dcom;
using Opc.Classic.Dcom.Activation;
using Opc.Classic.Dcom.Remoting;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Transport;
using Opc.Classic.Discovery;
using Opc.Classic.Mcp.Capture;
using Opc.Classic.Ndr;
using Opc.Classic.Transport;

namespace Opc.Classic.Mcp.Tools;

internal interface ICaptureTargetResolver
{
    Task<CaptureTargetMetadata> ResolveAsync(
        string? targetHost,
        string? progId,
        string? clsid,
        string? connectionString,
        CancellationToken cancellationToken);
}

internal sealed class CaptureTargetResolver : ICaptureTargetResolver
{
    private readonly ICaptureTargetActivationSessionFactory _activationSessions;

    public CaptureTargetResolver()
        : this(new CaptureTargetActivationSessionFactory())
    {
    }

    internal CaptureTargetResolver(ICaptureTargetActivationSessionFactory activationSessions)
    {
        _activationSessions = activationSessions ?? throw new ArgumentNullException(nameof(activationSessions));
    }

    public async Task<CaptureTargetMetadata> ResolveAsync(
        string? targetHost,
        string? progId,
        string? clsid,
        string? connectionString,
        CancellationToken cancellationToken)
    {
        string scheme = GetOpcScheme(connectionString);
        OpcMcpDcomConnectionRequest normalized = OpcMcpDcomConnectionHelper.NormalizeRequest(
            targetHost ?? "localhost",
            progId,
            clsid,
            username: null,
            password: null,
            useKerberos: false,
            connectionString,
            authLevel: null,
            scheme);

        if (OpcMcpDcomConnectionHelper.TryGetTcpEndpoint(
            normalized.ConnectionString,
            out string tcpHost,
            out int tcpPort))
        {
            return new CaptureTargetMetadata
            {
                Host = tcpHost,
                ProgId = normalized.ProgId,
                Clsid = Guid.TryParse(normalized.Clsid, out Guid directClsid) ? directClsid : null,
                ConnectionString = normalized.ConnectionString,
                Status = "resolved",
                Bindings = [$"ncacn_ip_tcp:{tcpHost}[{tcpPort}]"],
                Ports = [135, tcpPort],
            };
        }

        if (OpcMcpDcomConnectionHelper.TryGetInMemoryKey(normalized.ConnectionString) is not null)
        {
            return new CaptureTargetMetadata
            {
                Host = normalized.Host,
                ProgId = normalized.ProgId,
                Clsid = Guid.TryParse(normalized.Clsid, out Guid inMemoryClsid) ? inMemoryClsid : null,
                ConnectionString = normalized.ConnectionString,
                Status = "not_networked",
                Error = "In-memory targets have no network bindings to capture.",
            };
        }

        if (string.IsNullOrWhiteSpace(normalized.ProgId)
            && string.IsNullOrWhiteSpace(normalized.Clsid))
        {
            return new CaptureTargetMetadata
            {
                Host = normalized.Host,
                ConnectionString = normalized.ConnectionString,
                Status = "host_only",
                Bindings = [$"ncacn_ip_tcp:{normalized.Host}[135]"],
                Ports = [135],
            };
        }

        try
        {
            Guid resolvedClsid = await OpcMcpDcomConnectionHelper.ResolveClsidAsync(
                normalized,
                OpcEnumClient.DefaultCategoryIds.ToArray(),
                scheme,
                cancellationToken).ConfigureAwait(false);
            ICaptureTargetActivationSession activationSession =
                await _activationSessions.CreateAsync(
                    normalized,
                    resolvedClsid,
                    scheme,
                    cancellationToken).ConfigureAwait(false);
            RemoteActivationResponse? activation = null;
            Exception? releaseError = null;
            try
            {
                activation = await activationSession.ActivateAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    await activationSession.ReleaseAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    releaseError = ex;
                }
            }
            finally
            {
                await activationSession.DisposeAsync().ConfigureAwait(false);
            }

            if (activation is null)
            {
                throw new InvalidOperationException("IActivation::RemoteActivation returned no response.");
            }
            if (activation.Hresult != 0)
            {
                throw new InvalidOperationException(
                    $"IActivation::RemoteActivation returned HRESULT 0x{unchecked((uint)activation.Hresult):X8}.");
            }

            var bindings = new List<string>();
            var ports = new SortedSet<int> { 135 };
            DnsEndPoint? tcp = DualStringArrayResolver.ResolveFirstTcp(
                normalized.Host,
                activation.OxidBindings.Span);
            if (tcp is not null)
            {
                bindings.Add($"ncacn_ip_tcp:{tcp.Host}[{tcp.Port}]");
                ports.Add(tcp.Port);
            }
            NcacnNpEndPoint? pipe = DualStringArrayResolver.ResolveFirstNamedPipe(
                normalized.Host,
                activation.OxidBindings.Span);
            if (pipe is not null)
            {
                bindings.Add($"ncacn_np:{pipe.Host}[{pipe.PipeName}]");
            }

            return new CaptureTargetMetadata
            {
                Host = normalized.Host,
                ProgId = normalized.ProgId,
                Clsid = resolvedClsid,
                ConnectionString = normalized.ConnectionString,
                Status = releaseError is null ? "activated" : "activated_release_failed",
                Bindings = bindings,
                Ports = ports.ToArray(),
                Oxid = activation.Oxid,
                IpidRemUnknown = activation.IpidRemUnknown,
                AuthenticationHint = activation.AuthnHint,
                ServerVersion = $"{activation.ServerVersion.Major}.{activation.ServerVersion.Minor}",
                Error = releaseError?.Message,
            };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return new CaptureTargetMetadata
            {
                Host = normalized.Host,
                ProgId = normalized.ProgId,
                Clsid = Guid.TryParse(normalized.Clsid, out Guid parsedClsid) ? parsedClsid : null,
                ConnectionString = normalized.ConnectionString,
                Status = "failed",
                Ports = [135],
                Error = ex.Message,
            };
        }
    }

    private static string GetOpcScheme(string? connectionString)
    {
        if (Uri.TryCreate(connectionString, UriKind.Absolute, out Uri? uri)
            && uri.Scheme is "opcda" or "opcae" or "opchda")
        {
            return uri.Scheme;
        }
        return "opcda";
    }
}

internal interface ICaptureTargetActivationSessionFactory
{
    Task<ICaptureTargetActivationSession> CreateAsync(
        OpcMcpDcomConnectionRequest request,
        Guid clsid,
        string opcScheme,
        CancellationToken cancellationToken);
}

internal interface ICaptureTargetActivationSession : IAsyncDisposable
{
    Task<RemoteActivationResponse> ActivateAsync(CancellationToken cancellationToken);

    Task ReleaseAsync(CancellationToken cancellationToken);
}

internal sealed class CaptureTargetActivationSessionFactory : ICaptureTargetActivationSessionFactory
{
    public async Task<ICaptureTargetActivationSession> CreateAsync(
        OpcMcpDcomConnectionRequest request,
        Guid clsid,
        string opcScheme,
        CancellationToken cancellationToken)
    {
        IAuthContext authContext = OpcMcpDcomConnectionHelper.CreateAuthContext(request, clsid, opcScheme);
        try
        {
            ActivationClient client = await ActivationClient.ConnectTcpAsync(
                request.Host,
                authContext,
                cancellationToken).ConfigureAwait(false);
            return new CaptureTargetActivationSession(request, clsid, opcScheme, authContext, client);
        }
        catch
        {
            await DisposeAuthContextAsync(authContext).ConfigureAwait(false);
            throw;
        }
    }

    private static async ValueTask DisposeAuthContextAsync(IAuthContext authContext)
    {
        if (ReferenceEquals(authContext, NoOpAuthContext.Instance))
        {
            return;
        }
        if (authContext is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else if (authContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}

internal sealed class CaptureTargetActivationSession : ICaptureTargetActivationSession
{
    private static readonly Guid IidIUnknown = new("00000000-0000-0000-C000-000000000046");

    private readonly OpcMcpDcomConnectionRequest _request;
    private readonly Guid _clsid;
    private readonly string _opcScheme;
    private readonly IAuthContext _activationAuth;
    private readonly ActivationClient _activationClient;
    private RemoteActivationResponse? _activation;
    private bool _released;
    private bool _disposed;

    public CaptureTargetActivationSession(
        OpcMcpDcomConnectionRequest request,
        Guid clsid,
        string opcScheme,
        IAuthContext activationAuth,
        ActivationClient activationClient)
    {
        _request = request;
        _clsid = clsid;
        _opcScheme = opcScheme;
        _activationAuth = activationAuth;
        _activationClient = activationClient;
    }

    public async Task<RemoteActivationResponse> ActivateAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _activation = await _activationClient.RemoteActivationAsync(
            _clsid,
            ["ncacn_ip_tcp", "ncacn_np"],
            objectStorage: null,
            [IidIUnknown],
            cancellationToken).ConfigureAwait(false);
        return _activation;
    }

    public async Task ReleaseAsync(CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_released || _activation is null)
        {
            return;
        }
        await ReleaseActivatedReferencesAsync(_activation, cancellationToken).ConfigureAwait(false);
        _released = true;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Dispose retries best-effort RemRelease but must always dispose the activation channel and auth context.")]
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }
        try
        {
            if (!_released && _activation is not null)
            {
                try
                {
                    await ReleaseActivatedReferencesAsync(_activation, CancellationToken.None).ConfigureAwait(false);
                    _released = true;
                }
                catch
                {
                    // Explicit ReleaseAsync reports cleanup errors. Dispose remains best-effort.
                }
            }
        }
        finally
        {
            _disposed = true;
            try
            {
                await _activationClient.DisposeAsync().ConfigureAwait(false);
            }
            finally
            {
                await DisposeAuthContextAsync(_activationAuth).ConfigureAwait(false);
            }
        }
    }

    private async Task ReleaseActivatedReferencesAsync(
        RemoteActivationResponse activation,
        CancellationToken cancellationToken)
    {
        if (activation.IpidRemUnknown == Guid.Empty)
        {
            throw new InvalidOperationException("Activation returned no IRemUnknown IPID for releasing interface references.");
        }

        var refs = new Dictionary<Guid, uint>();
        foreach (RemoteActivationInterfaceResult result in activation.InterfaceResults)
        {
            if (result.Hresult != 0 || result.ObjRef.IsEmpty)
            {
                continue;
            }
            var reader = new NdrReader(result.ObjRef.Span);
            IOpcInterfaceRef interfaceRef = OpcInterfaceRefCodec.Read(ref reader);
            if (interfaceRef.Ipid == Guid.Empty || interfaceRef.PublicRefs == 0)
            {
                continue;
            }
            refs[interfaceRef.Ipid] = refs.TryGetValue(interfaceRef.Ipid, out uint existing)
                ? checked(existing + interfaceRef.PublicRefs)
                : interfaceRef.PublicRefs;
        }
        if (refs.Count == 0)
        {
            return;
        }

        EndPoint endpoint = DualStringArrayResolver.ResolveFirstTransport(
            _request.Host,
            activation.OxidBindings.Span)
            ?? throw new InvalidOperationException("Activation returned no transport binding for IRemUnknown::RemRelease.");
        IAuthContext releaseAuth = OpcMcpDcomConnectionHelper.CreateAuthContext(_request, _clsid, _opcScheme);
        ICallChannel? releaseChannel = null;
        try
        {
            IAsyncTransportFactory transportFactory = OperatingSystem.IsWindows()
                ? TransportFactoryDispatcher.CreateWindowsLocal(new CaptureTcpTransportFactory())
                : new CaptureTcpTransportFactory();
            var channelFactory = new DcomCallChannelFactory(transportFactory);
            releaseChannel = await channelFactory.ConnectActivatedAsync(
                endpoint,
                releaseAuth,
                activation.IpidRemUnknown,
                [IRemUnknown.InterfaceId],
                cancellationToken).ConfigureAwait(false);
            NdrCallResult result = await releaseChannel.InvokeAsync(
                IRemUnknown.InterfaceId,
                opnum: 5,
                EncodeRemRelease(refs),
                cancellationToken).ConfigureAwait(false);
            if (result.IsFailure)
            {
                throw new InvalidOperationException(
                    $"IRemUnknown::RemRelease failed with HRESULT 0x{unchecked((uint)result.Hresult):X8}.");
            }
        }
        finally
        {
            try
            {
                if (releaseChannel is IAsyncDisposable asyncChannel)
                {
                    await asyncChannel.DisposeAsync().ConfigureAwait(false);
                }
                else if (releaseChannel is IDisposable channelDisposable)
                {
                    channelDisposable.Dispose();
                }
            }

            finally
            {
                await DisposeAuthContextAsync(releaseAuth).ConfigureAwait(false);
            }
        }
    }

    private static byte[] EncodeRemRelease(IReadOnlyDictionary<Guid, uint> refs)
    {
        var buffer = new byte[checked(8 + refs.Count * 24)];
        var writer = new NdrWriter(buffer);
        writer.WriteUInt16(checked((ushort)refs.Count));
        writer.WriteConformanceHeader(refs.Count);
        foreach (KeyValuePair<Guid, uint> pair in refs)
        {
            writer.WriteGuid(pair.Key);
            writer.WriteUInt32(pair.Value);
            writer.WriteUInt32(0);
        }
        return buffer.AsSpan(0, writer.Position).ToArray();
    }

    private static async ValueTask DisposeAuthContextAsync(IAuthContext authContext)
    {
        if (ReferenceEquals(authContext, NoOpAuthContext.Instance))
        {
            return;
        }
        if (authContext is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else if (authContext is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
