//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Dcom.Smb.Rpc;

/// <summary>Connects the SMB2 state machine to an underlying transport.</summary>
public delegate Task<ISmb2Transport> Smb2TransportConnector(
    string host,
    int port,
    int maxSmb2MessageSize,
    CancellationToken cancellationToken);

/// <summary>
/// Sync-over-async adapter that exposes an SMB2 named pipe as a single-shot
/// request/response (transceive) endpoint suitable for the legacy
/// <c>Opc.Classic.Dcom.Rpc.ITransport</c> contract used by <c>ncacn_np</c>.
/// </summary>
/// <remarks>
/// <para>
/// The DCE/RPC over SMB transport (per [MS-RPCE] §2.1.1.2) sends each PDU as
/// a named-pipe write and receives each response as a named-pipe read; the
/// recommended optimization for synchronous calls is to combine the last
/// request fragment write and the first response fragment read into a single
/// transact (SMB2 IOCTL with <c>FSCTL_PIPE_TRANSCEIVE</c>). This adapter
/// implements that transact pattern via <see cref="Smb2NamedPipe.TransceiveAsync" />.
/// </para>
/// <para>
/// The async-to-sync bridging via <c>.GetAwaiter().GetResult()</c> matches the
/// pattern used by the existing legacy transport (which itself blocks on
/// synchronous streams). For new code prefer the async API on
/// <see cref="Smb2NamedPipe" /> directly.
/// </para>
/// </remarks>
public sealed class Smb2RpcTransportAdapter : IDisposable, IAsyncDisposable
{
    private readonly Smb2Connection _connection;
    private readonly Smb2NamedPipe _pipe;
    private bool _disposed;

    /// <summary>Initializes a new adapter over an established named-pipe handle.</summary>
    public Smb2RpcTransportAdapter(Smb2Connection connection, Smb2NamedPipe pipe)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _pipe = pipe ?? throw new ArgumentNullException(nameof(pipe));
    }

    /// <summary>Sends data with an SMB2 WRITE request.</summary>
    public async Task WriteAsync(ReadOnlyMemory<byte> request, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        await _pipe.WriteAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Reads data with an SMB2 READ request.</summary>
    public async Task<ReadOnlyMemory<byte>> ReadAsync(int maxLength, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return await _pipe.ReadAsync(maxLength, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronous round-trip: send <paramref name="request" /> and return the
    /// server's response in one SMB2 IOCTL transact.
    /// </summary>
    public async Task<ReadOnlyMemory<byte>> TransceiveAsync(
        ReadOnlyMemory<byte> request,
        int maxOutputResponse = 64 * 1024,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return await _pipe.TransceiveAsync(request, maxOutputResponse, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Synchronous write wrapper for legacy callers.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage", "VSTHRD002:Avoid problematic synchronous waits",
        Justification = "Intentional sync-over-async bridge to the legacy Opc.Classic.Dcom.Rpc.ITransport contract (which is itself synchronous).")]
    public void Write(ReadOnlyMemory<byte> request, CancellationToken cancellationToken = default) =>
        WriteAsync(request, cancellationToken).GetAwaiter().GetResult();

    /// <summary>Synchronous read wrapper for legacy callers.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage", "VSTHRD002:Avoid problematic synchronous waits",
        Justification = "Intentional sync-over-async bridge to the legacy Opc.Classic.Dcom.Rpc.ITransport contract (which is itself synchronous).")]
    public ReadOnlyMemory<byte> Read(int maxLength, CancellationToken cancellationToken = default) =>
        ReadAsync(maxLength, cancellationToken).GetAwaiter().GetResult();

    /// <summary>
    /// Synchronous round-trip: send <paramref name="request" /> and return the
    /// server's response in one SMB2 IOCTL transact.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage", "VSTHRD002:Avoid problematic synchronous waits",
        Justification = "Intentional sync-over-async bridge to the legacy Opc.Classic.Dcom.Rpc.ITransport contract (which is itself synchronous). For new code prefer the async API on Smb2NamedPipe directly.")]
    public ReadOnlyMemory<byte> Transceive(
        ReadOnlyMemory<byte> request,
        int maxOutputResponse = 64 * 1024,
        CancellationToken cancellationToken = default) =>
        TransceiveAsync(request, maxOutputResponse, cancellationToken).GetAwaiter().GetResult();

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage", "VSTHRD002:Avoid problematic synchronous waits",
        Justification = "IDisposable.Dispose is intrinsically synchronous; we delegate to the async DisposeAsync. Callers wanting deterministic async tear-down can call DisposeAsync directly.")]
    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        await _pipe.DisposeAsync().ConfigureAwait(false);
        await _connection.DisposeAsync().ConfigureAwait(false);
    }
}

/// <summary>
/// Address parser for the legacy <c>smb://user@server/IPC$/pipename</c> form
/// used by <c>Opc.Classic.Dcom.Rpc.Ncacn_Np.RpcTransport</c>.
/// </summary>
public static class SmbRpcAddress
{
    /// <summary>Parsed components of an SMB pipe URL.</summary>
    public sealed record Parsed(
        string Host,
        string ShareName,
        string PipeName,
        string? UserName,
        string? Domain,
        string? Password);

    /// <summary>
    /// Parses an <c>smb://[user[:password]@][domain;]server/IPC$/pipename</c> address.
    /// </summary>
    public static Parsed Parse(string smbUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(smbUrl);
        const string Scheme = "smb://";
        if (!smbUrl.StartsWith(Scheme, StringComparison.OrdinalIgnoreCase))
        {
            throw new FormatException($"SMB URL must start with '{Scheme}'.");
        }

        string body = smbUrl[Scheme.Length..];
        string? userInfo = null;
        int at = body.IndexOf('@', StringComparison.Ordinal);
        if (at > 0)
        {
            userInfo = body[..at];
            body = body[(at + 1)..];
        }

        int firstSlash = body.IndexOf('/', StringComparison.Ordinal);
        if (firstSlash <= 0)
        {
            throw new FormatException("SMB URL is missing a share name.");
        }

        string host = body[..firstSlash];
        string rest = body[(firstSlash + 1)..];
        int secondSlash = rest.IndexOf('/', StringComparison.Ordinal);
        if (secondSlash <= 0)
        {
            throw new FormatException("SMB URL is missing a pipe name.");
        }

        string share = rest[..secondSlash];
        string pipe = rest[(secondSlash + 1)..];

        string? userName = null;
        string? domain = null;
        string? password = null;
        if (userInfo is not null)
        {
            int colon = userInfo.IndexOf(':', StringComparison.Ordinal);
            if (colon >= 0)
            {
                password = Uri.UnescapeDataString(userInfo[(colon + 1)..]);
                userInfo = userInfo[..colon];
            }
            int semi = userInfo.IndexOf(';', StringComparison.Ordinal);
            if (semi >= 0)
            {
                domain = userInfo[..semi];
                userName = userInfo[(semi + 1)..];
            }
            else
            {
                userName = userInfo;
            }
        }

        return new Parsed(host, share, pipe, userName, domain, password);
    }

    /// <summary>
    /// Formats a parsed SMB URL back to its <c>smb://[user[:password]@][domain;]server/IPC$/pipe</c> string form.
    /// </summary>
    public static string Format(Parsed parsed)
    {
        ArgumentNullException.ThrowIfNull(parsed);
        var sb = new System.Text.StringBuilder("smb://", capacity: 64);
        if (!string.IsNullOrEmpty(parsed.UserName))
        {
            if (!string.IsNullOrEmpty(parsed.Domain))
            {
                sb.Append(parsed.Domain).Append(';');
            }
            sb.Append(parsed.UserName);
            if (!string.IsNullOrEmpty(parsed.Password))
            {
                sb.Append(':').Append(Uri.EscapeDataString(parsed.Password));
            }
            sb.Append('@');
        }
        sb.Append(parsed.Host);
        sb.Append('/').Append(parsed.ShareName);
        sb.Append('/').Append(parsed.PipeName);
        return sb.ToString();
    }
}

/// <summary>
/// Builder that opens an SMB2 connection, performs NTLMSSP session setup
/// (via a caller-supplied blob provider), connects to the IPC$ tree, opens
/// the named pipe, and returns an adapter ready for synchronous RPC transacts.
/// </summary>
public sealed class Smb2RpcTransportBuilder
{
    private readonly SmbRpcAddress.Parsed _address;
    private readonly NtlmsspBlobProvider _blobProvider;
    private readonly Smb2SessionKeyProvider? _sessionKeyProvider;
    private Smb2TransportConnector _transportConnector = ConnectTcpTransportAsync;
    private int _port = 445;
    private int _maxSmb2MessageSize = Smb2Constants.MaxNetBiosFrameSize;

    /// <summary>Initializes a new builder from a parsed SMB URL.</summary>
    /// <param name="address">Parsed SMB endpoint.</param>
    /// <param name="blobProvider">Callback that emits NTLMSSP/Kerberos SESSION_SETUP blobs.</param>
    /// <param name="sessionKeyProvider">Optional callback that exposes the SessionKey for SMB signing; see [MS-SMB2] §3.1.5.1.</param>
    public Smb2RpcTransportBuilder(
        SmbRpcAddress.Parsed address,
        NtlmsspBlobProvider blobProvider,
        Smb2SessionKeyProvider? sessionKeyProvider = null)
    {
        _address = address ?? throw new ArgumentNullException(nameof(address));
        _blobProvider = blobProvider ?? throw new ArgumentNullException(nameof(blobProvider));
        _sessionKeyProvider = sessionKeyProvider;
    }

    /// <summary>Sets the TCP port for the SMB2 transport (default 445).</summary>
    public Smb2RpcTransportBuilder UsePort(int port)
    {
        if (port is <= 0 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), port, "Port must be 1..65535.");
        }
        _port = port;
        return this;
    }

    /// <summary>Sets the maximum SMB2 message size for inbound and outbound frames.</summary>
    public Smb2RpcTransportBuilder UseMaxSmb2MessageSize(int maxSmb2MessageSize)
    {
        _ = new Smb2ConnectionOptions(_address.Host, _port) { MaxSmb2MessageSize = maxSmb2MessageSize };
        _maxSmb2MessageSize = maxSmb2MessageSize;
        return this;
    }

    /// <summary>Overrides the SMB2 byte transport connector, primarily for tests.</summary>
    public Smb2RpcTransportBuilder UseTransportConnector(Smb2TransportConnector transportConnector)
    {
        _transportConnector = transportConnector ?? throw new ArgumentNullException(nameof(transportConnector));
        return this;
    }

    /// <summary>
    /// Opens the SMB2 connection, completes NTLMSSP session setup, connects to IPC$,
    /// opens the named pipe, and returns the adapter.
    /// </summary>
    public async Task<Smb2RpcTransportAdapter> BuildAsync(CancellationToken cancellationToken = default)
    {
        ISmb2Transport tcp = await _transportConnector(_address.Host, _port, _maxSmb2MessageSize, cancellationToken).ConfigureAwait(false);
        var conn = new Smb2Connection(new Smb2ConnectionOptions(_address.Host, _port) { MaxSmb2MessageSize = _maxSmb2MessageSize }, tcp);
        try
        {
            _ = await conn.NegotiateAsync(cancellationToken).ConfigureAwait(false);
            await conn.SessionSetupAsync(_blobProvider, _sessionKeyProvider, cancellationToken).ConfigureAwait(false);
            _ = await conn.TreeConnectIpcAsync(cancellationToken).ConfigureAwait(false);
            var pipe = await conn.OpenNamedPipeAsync(_address.PipeName, cancellationToken).ConfigureAwait(false);
            return new Smb2RpcTransportAdapter(conn, pipe);
        }
        catch
        {
            await conn.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    private static async Task<ISmb2Transport> ConnectTcpTransportAsync(
        string host,
        int port,
        int maxSmb2MessageSize,
        CancellationToken cancellationToken) =>
        await TcpSmb2Transport.ConnectAsync(host, port, maxSmb2MessageSize, cancellationToken).ConfigureAwait(false);

    /// <summary>Convenience wrapper that runs <see cref="BuildAsync" /> synchronously.</summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Usage", "VSTHRD002:Avoid problematic synchronous waits",
        Justification = "Convenience sync wrapper for the legacy ncacn_np transport callers. Prefer BuildAsync from new code.")]
    public Smb2RpcTransportAdapter Build(CancellationToken cancellationToken = default) =>
        BuildAsync(cancellationToken).GetAwaiter().GetResult();
}
