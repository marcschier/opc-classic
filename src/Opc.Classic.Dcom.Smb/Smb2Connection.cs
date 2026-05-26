//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Opc.Classic.Dcom.Smb;

/// <summary>
/// User-supplied callback that produces NTLMSSP security blobs for SMB2 SESSION_SETUP.
/// The first call is invoked with an empty input; subsequent calls receive the server's
/// previous SESSION_SETUP response blob and must return the next Type-3/Type-1 message.
/// </summary>
/// <remarks>
/// The callback returns <see langword="null" /> to signal the negotiation is complete
/// (no further security blob to send).
/// </remarks>
public delegate ReadOnlyMemory<byte>? NtlmsspBlobProvider(ReadOnlyMemory<byte> serverBlob);

/// <summary>
/// Configuration for an outbound SMB2 connection.
/// </summary>
public sealed record Smb2ConnectionOptions(
    string Host,
    int Port = 445,
    int ReceiveTimeoutMs = 30_000,
    int SendTimeoutMs = 30_000);

/// <summary>
/// Top-level facade for an SMB2 client connection that opens an IPC$ tree
/// and exposes named-pipe <see cref="Smb2NamedPipe" /> handles for DCE/RPC
/// tunneling per [MS-RPCE] §2.1.1.2.
/// </summary>
/// <remarks>
/// <para>
/// Phase 1 (the current phase) delivers the wire-format primitives and the
/// connection state-machine skeleton. The transport-level I/O loop is left as
/// a separate concern: callers wishing to issue SMB2 traffic against a real
/// server should construct an <see cref="Smb2Connection" /> with an
/// <see cref="ISmb2Transport" /> implementation. Phase 2 wires the production
/// TCP+NetBIOS transport into <c>Opc.Classic.Dcom.Rpc.Ncacn_Np.RpcTransport</c>.
/// </para>
/// <para>
/// The state machine progresses: <c>Disconnected → Negotiating → Authenticated
/// → TreeConnected → PipeOpen → Closed</c>. Each transition is driven by a
/// single SMB2 round-trip plus (for SESSION_SETUP) one or more NTLMSSP iterations.
/// </para>
/// </remarks>
public sealed class Smb2Connection : IAsyncDisposable
{
    private readonly Smb2ConnectionOptions _options;
    private readonly ISmb2Transport _transport;
    private readonly Smb2MessageCounter _counter = new();
    private ulong _sessionId;
    private uint _treeId;
    private Smb2Dialect _negotiatedDialect;
    private bool _disposed;

    /// <summary>Initializes a new SMB2 connection over the supplied transport.</summary>
    public Smb2Connection(Smb2ConnectionOptions options, ISmb2Transport transport)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    /// <summary>Gets the negotiated SMB2 dialect (valid after <see cref="NegotiateAsync" />).</summary>
    public Smb2Dialect NegotiatedDialect => _negotiatedDialect;

    /// <summary>Gets the session identifier (valid after authentication).</summary>
    public ulong SessionId => _sessionId;

    /// <summary>Gets the tree identifier for the current tree connect (0 if none).</summary>
    public uint TreeId => _treeId;

    /// <summary>
    /// Performs the SMB2 NEGOTIATE exchange. The client advertises support for
    /// SMB 2.0.2 through SMB 3.1.1 and accepts the highest dialect the server selects.
    /// </summary>
    public async Task<Smb2NegotiateResponse> NegotiateAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        IReadOnlyList<Smb2Dialect> dialects =
        [
            Smb2Dialect.Smb202,
            Smb2Dialect.Smb210,
            Smb2Dialect.Smb300,
            Smb2Dialect.Smb302,
            Smb2Dialect.Smb311,
        ];

        var request = new Smb2NegotiateRequest(
            SecurityMode: 0x0001, // SMB2_NEGOTIATE_SIGNING_ENABLED
            Capabilities: 0,
            ClientGuid: Guid.CreateVersion7(),
            Dialects: dialects);

        var responseBytes = await ExchangeAsync(
            command: Smb2Command.Negotiate,
            sessionId: 0,
            treeId: 0,
            writeBody: (Span<byte> body) => request.WriteTo(body),
            maxBodySize: 256,
            cancellationToken).ConfigureAwait(false);

        var response = Smb2NegotiateResponse.Read(responseBytes.Span);
        _negotiatedDialect = response.Dialect;
        return response;
    }

    /// <summary>
    /// Performs the SMB2 SESSION_SETUP exchange, iterating NTLMSSP type-1/2/3
    /// blobs through the supplied <paramref name="blobProvider" />.
    /// </summary>
    public async Task SessionSetupAsync(
        NtlmsspBlobProvider blobProvider,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(blobProvider);
        ThrowIfDisposed();
        if (_negotiatedDialect == default)
        {
            throw new InvalidOperationException("Call NegotiateAsync before SessionSetupAsync.");
        }

        ReadOnlyMemory<byte> serverBlob = ReadOnlyMemory<byte>.Empty;
        ReadOnlyMemory<byte>? clientBlob = blobProvider(serverBlob);
        if (clientBlob is null)
        {
            throw new InvalidOperationException("NtlmsspBlobProvider returned null on first call.");
        }

        while (clientBlob is not null)
        {
            var setupRequest = new Smb2SessionSetupRequest(
                Flags: 0,
                SecurityMode: 0x01,
                Capabilities: 0,
                Channel: 0,
                PreviousSessionId: 0,
                SecurityBlob: clientBlob.Value);

            var responseBytes = await ExchangeAsync(
                command: Smb2Command.SessionSetup,
                sessionId: _sessionId,
                treeId: 0,
                writeBody: (Span<byte> body) => setupRequest.WriteTo(body),
                maxBodySize: 64 + clientBlob.Value.Length,
                cancellationToken: cancellationToken,
                expectedStatus: status => status == NtStatus.Success || status == NtStatus.MoreProcessingRequired,
                captureSessionId: id => { if (_sessionId == 0) { _sessionId = id; } }).ConfigureAwait(false);

            var response = Smb2SessionSetupResponse.Read(responseBytes.Span);

            // If the server replied STATUS_SUCCESS, the negotiation is done.
            // The provider may still want one final processing pass (e.g. to
            // extract the NTLM session key) — we call it one more time and
            // discard any non-null result.
            if (_lastStatus == NtStatus.Success)
            {
                _ = blobProvider(response.SecurityBlob);
                return;
            }

            // MoreProcessingRequired: feed the server blob back to the provider.
            serverBlob = response.SecurityBlob;
            clientBlob = blobProvider(serverBlob);
        }

        if (_lastStatus != NtStatus.Success)
        {
            throw new Smb2StatusException(
                _lastStatus,
                $"SESSION_SETUP did not complete; last NTSTATUS=0x{_lastStatus:X8}.");
        }
    }

    /// <summary>Connects to the <c>IPC$</c> share on the server.</summary>
    public async Task<Smb2TreeConnectResponse> TreeConnectIpcAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (_sessionId == 0)
        {
            throw new InvalidOperationException("Call SessionSetupAsync before TreeConnectIpcAsync.");
        }

        var request = new Smb2TreeConnectRequest($"\\\\{_options.Host}\\IPC$");
        var responseBytes = await ExchangeAsync(
            command: Smb2Command.TreeConnect,
            sessionId: _sessionId,
            treeId: 0,
            writeBody: (Span<byte> body) => request.WriteTo(body),
            maxBodySize: 256,
            captureTreeId: id => _treeId = id,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        return Smb2TreeConnectResponse.Read(responseBytes.Span);
    }

    /// <summary>Opens a named pipe on the IPC$ share (e.g. <c>"winreg"</c>).</summary>
    public async Task<Smb2NamedPipe> OpenNamedPipeAsync(
        string pipeName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(pipeName);
        ThrowIfDisposed();
        if (_treeId == 0)
        {
            throw new InvalidOperationException("Call TreeConnectIpcAsync before OpenNamedPipeAsync.");
        }

        var request = new Smb2CreateRequest(
            DesiredAccess: FileAccessMask.PipeReadWrite,
            FileAttributes: 0,
            ShareAccessMask: ShareAccess.ReadWrite,
            Disposition: (uint)CreateDisposition.Open,
            CreateOptionsMask: CreateOptions.NonDirectoryFile,
            Name: pipeName);

        var responseBytes = await ExchangeAsync(
            command: Smb2Command.Create,
            sessionId: _sessionId,
            treeId: _treeId,
            writeBody: (Span<byte> body) => request.WriteTo(body),
            maxBodySize: 256 + (pipeName.Length * 2),
            cancellationToken: cancellationToken).ConfigureAwait(false);

        var response = Smb2CreateResponse.Read(responseBytes.Span);
        return new Smb2NamedPipe(this, response.FileIdPersistent, response.FileIdVolatile);
    }

    /// <summary>Issues a raw SMB2 IOCTL with <c>FSCTL_PIPE_TRANSCEIVE</c>.</summary>
    public async Task<ReadOnlyMemory<byte>> PipeTransceiveAsync(
        Smb2NamedPipe pipe,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipe);
        ThrowIfDisposed();

        var request = new Smb2IoctlRequest(
            CtlCode: FsctlCode.PipeTransceive,
            FileIdPersistent: pipe.FileIdPersistent,
            FileIdVolatile: pipe.FileIdVolatile,
            Input: data,
            MaxOutputResponse: 4096,
            IsFsctl: true);

        var responseBytes = await ExchangeAsync(
            command: Smb2Command.Ioctl,
            sessionId: _sessionId,
            treeId: _treeId,
            writeBody: (Span<byte> body) => request.WriteTo(body),
            maxBodySize: 128 + data.Length,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        return Smb2IoctlResponse.Read(responseBytes.Span).Output;
    }

    /// <summary>Closes a previously-opened named pipe handle.</summary>
    public async Task ClosePipeAsync(Smb2NamedPipe pipe, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipe);
        ThrowIfDisposed();
        var request = new Smb2CloseRequest(pipe.FileIdPersistent, pipe.FileIdVolatile);
        _ = await ExchangeAsync(
            command: Smb2Command.Close,
            sessionId: _sessionId,
            treeId: _treeId,
            writeBody: (Span<byte> body) => request.WriteTo(body),
            maxBodySize: 64,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;

        try
        {
            if (_treeId != 0)
            {
                _ = await ExchangeAsync(
                    command: Smb2Command.TreeDisconnect,
                    sessionId: _sessionId,
                    treeId: _treeId,
                    writeBody: (Span<byte> body) => Smb2TreeDisconnect.Write(body),
                    maxBodySize: 16,
                    cancellationToken: default).ConfigureAwait(false);
            }

            if (_sessionId != 0)
            {
                _ = await ExchangeAsync(
                    command: Smb2Command.Logoff,
                    sessionId: _sessionId,
                    treeId: 0,
                    writeBody: (Span<byte> body) => Smb2Logoff.Write(body),
                    maxBodySize: 16,
                    cancellationToken: default).ConfigureAwait(false);
            }
        }
#pragma warning disable CA1031 // Tear-down: any failure is non-fatal because we close the transport unconditionally below.
        catch
        {
            // Ignore teardown errors; the transport is being closed regardless.
        }
#pragma warning restore CA1031

        await _transport.DisposeAsync().ConfigureAwait(false);
    }

    private uint _lastStatus;

    private delegate int WriteBodyDelegate(Span<byte> body);

    private async Task<ReadOnlyMemory<byte>> ExchangeAsync(
        Smb2Command command,
        ulong sessionId,
        uint treeId,
        WriteBodyDelegate writeBody,
        int maxBodySize,
        CancellationToken cancellationToken,
        Func<uint, bool>? expectedStatus = null,
        Action<ulong>? captureSessionId = null,
        Action<uint>? captureTreeId = null)
    {
        int packetSize = Smb2Constants.PacketHeaderSize + maxBodySize;
        var packetBuffer = new byte[packetSize];

        var header = new Smb2PacketHeader(
            CreditCharge: 1,
            Status: 0,
            Command: command,
            CreditRequestResponse: 1,
            Flags: 0,
            NextCommand: 0,
            MessageId: _counter.Next(),
            ProcessId: 0,
            TreeId: treeId,
            SessionId: sessionId,
            Signature: ReadOnlyMemory<byte>.Empty);
        header.Write(packetBuffer);

        int bodySize = writeBody(packetBuffer.AsSpan(Smb2Constants.PacketHeaderSize));
        int totalSize = Smb2Constants.PacketHeaderSize + bodySize;

        await _transport.SendAsync(packetBuffer.AsMemory(0, totalSize), cancellationToken).ConfigureAwait(false);

        var responseBuffer = await _transport.ReceiveAsync(cancellationToken).ConfigureAwait(false);
        if (responseBuffer.Length < Smb2Constants.PacketHeaderSize)
        {
            throw new Smb2ProtocolException("SMB2 response too short for a header.");
        }

        var responseHeader = Smb2PacketHeader.Read(responseBuffer.Span);
        _lastStatus = responseHeader.Status;
        captureSessionId?.Invoke(responseHeader.SessionId);
        captureTreeId?.Invoke(responseHeader.TreeId);

        bool ok = expectedStatus is null
            ? responseHeader.Status == NtStatus.Success
            : expectedStatus(responseHeader.Status);
        if (!ok)
        {
            throw new Smb2StatusException(
                responseHeader.Status,
                $"SMB2 {command} returned NTSTATUS=0x{responseHeader.Status:X8}.");
        }

        return responseBuffer[Smb2Constants.PacketHeaderSize..];
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}

/// <summary>Allocates monotonic SMB2 MessageIds (per [MS-SMB2] §3.2.4.1.4).</summary>
internal sealed class Smb2MessageCounter
{
    private ulong _next;

    public ulong Next() => Interlocked.Increment(ref _next);
}

/// <summary>Handle to an opened SMB2 named pipe (FileId on the wire).</summary>
public sealed class Smb2NamedPipe : IAsyncDisposable
{
    private readonly Smb2Connection _connection;
    private bool _closed;

    internal Smb2NamedPipe(Smb2Connection connection, ulong fileIdPersistent, ulong fileIdVolatile)
    {
        _connection = connection;
        FileIdPersistent = fileIdPersistent;
        FileIdVolatile = fileIdVolatile;
    }

    internal ulong FileIdPersistent { get; }

    internal ulong FileIdVolatile { get; }

    /// <summary>Sends data and waits for a response in one round-trip via FSCTL_PIPE_TRANSCEIVE.</summary>
    public Task<ReadOnlyMemory<byte>> TransceiveAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default) =>
        _connection.PipeTransceiveAsync(this, data, cancellationToken);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_closed)
        {
            return;
        }
        _closed = true;
        await _connection.ClosePipeAsync(this).ConfigureAwait(false);
    }
}
