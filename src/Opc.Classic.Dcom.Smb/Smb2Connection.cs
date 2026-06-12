//
// SPDX-License-Identifier: MIT
// Copyright (c) 2026 Opc.Classic .NET Contributors
//

using System.Security.Cryptography;

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
/// Callback that exposes the NTLMSSP/Kerberos SessionKey used to derive SMB2 signing keys; see [MS-SMB2] §3.1.5.1.
/// </summary>
/// <returns>The established session key, or <see langword="null" /> if authentication did not produce one.</returns>
public delegate ReadOnlyMemory<byte>? Smb2SessionKeyProvider();

/// <summary>
/// Configuration for an outbound SMB2 connection.
/// </summary>
public sealed record Smb2ConnectionOptions(
    string Host,
    int Port = 445,
    int ReceiveTimeoutMs = 30_000,
    int SendTimeoutMs = 30_000)
{
    public int MaxSmb2MessageSize { get; init; } = Smb2Constants.MaxNetBiosFrameSize;
}

/// <summary>
/// Top-level facade for an SMB2 client connection that opens an IPC$ tree
/// and exposes named-pipe <see cref="Smb2NamedPipe" /> handles for DCE/RPC
/// tunneling per [MS-RPCE] §2.1.1.2.
/// </summary>
/// <remarks>
/// <para>
/// Delivers the wire-format primitives and the
/// connection state-machine skeleton. The transport-level I/O loop is left as
/// a separate concern: callers wishing to issue SMB2 traffic against a real
/// server should construct an <see cref="Smb2Connection" /> with an
/// <see cref="ISmb2Transport" /> implementation. The production
/// TCP+NetBIOS transport is wired into <c>Opc.Classic.Dcom.Rpc.Ncacn_Np.RpcTransport</c>.
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
    private Smb2EncryptionAlgorithm _encryptionAlgorithm = Smb2EncryptionAlgorithm.AesCcm;
    private Smb2Signer? _signer;
    private Smb2Crypter? _encrypter;
    private Smb2Crypter? _decrypter;
    private byte[]? _preauthIntegrityHash;
    private byte[]? _lastNegotiateRequest;
    private byte[]? _lastNegotiateResponse;
    private bool _signingEnabled;
    private bool _signingRequired;
    private bool _supportsEncryption;
    private bool _sessionEncryptData;
    private bool _treeEncryptData;
    private bool _disposed;

    /// <summary>Initializes a new SMB2 connection over the supplied transport.</summary>
    public Smb2Connection(Smb2ConnectionOptions options, ISmb2Transport transport)
    {
        _options = ValidateOptions(options ?? throw new ArgumentNullException(nameof(options)));
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
    }

    /// <summary>Gets the negotiated SMB2 dialect (valid after <see cref="NegotiateAsync" />).</summary>
    public Smb2Dialect NegotiatedDialect => _negotiatedDialect;

    /// <summary>Gets the session identifier (valid after authentication).</summary>
    public ulong SessionId => _sessionId;

    /// <summary>Gets the tree identifier for the current tree connect (0 if none).</summary>
    public uint TreeId => _treeId;

    /// <summary>
    /// Configures SMB2 signing and SMB3 encryption from the NTLMSSP/Kerberos SessionKey,
    /// deriving keys per [MS-SMB2] §3.1.5.1 and §3.1.4.2.
    /// </summary>
    /// <param name="sessionKey">The established authentication SessionKey.</param>
    public void SetSessionKey(ReadOnlySpan<byte> sessionKey)
    {
        ThrowIfDisposed();
        if (_negotiatedDialect == default)
        {
            throw new InvalidOperationException("Call NegotiateAsync before setting the SMB2 session key.");
        }

        ReadOnlySpan<byte> preauthContext = _negotiatedDialect == Smb2Dialect.Smb311
            ? GetPreauthIntegrityHash()
            : default;
        _signer = Smb2Signer.CreateForDialect(_negotiatedDialect, sessionKey, preauthContext);

        if (_supportsEncryption)
        {
            Smb2Crypter.ValidateDialectAlgorithm(_negotiatedDialect, _encryptionAlgorithm);
            byte[] encryptionKey = Smb2Crypter.DeriveSmb3ClientEncryptionKey(_negotiatedDialect, sessionKey, preauthContext);
            byte[] decryptionKey = Smb2Crypter.DeriveSmb3ClientDecryptionKey(_negotiatedDialect, sessionKey, preauthContext);
            _encrypter = new Smb2Crypter(encryptionKey, _encryptionAlgorithm);
            _decrypter = new Smb2Crypter(decryptionKey, _encryptionAlgorithm);
            CryptographicOperations.ZeroMemory(encryptionKey);
            CryptographicOperations.ZeroMemory(decryptionKey);
        }
    }

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
            SecurityMode: Smb2Constants.SecurityModeSigningEnabled,
            Capabilities: Smb2Constants.GlobalCapEncryption,
            ClientGuid: Guid.CreateVersion7(),
            Dialects: dialects,
            IncludeSmb311NegotiateContexts: true);

        var responseBytes = await ExchangeAsync(
            command: Smb2Command.Negotiate,
            sessionId: 0,
            treeId: 0,
            writeBody: (Span<byte> body) => request.WriteTo(body),
            maxBodySize: 256,
            cancellationToken).ConfigureAwait(false);

        var response = Smb2NegotiateResponse.Read(responseBytes.Span);
        _negotiatedDialect = response.Dialect;
        _signingRequired = (response.SecurityMode & Smb2Constants.SecurityModeSigningRequired) != 0;
        _signingEnabled = _signingRequired ||
            (response.SecurityMode & Smb2Constants.SecurityModeSigningEnabled) != 0;
        _supportsEncryption = IsSmb3Dialect(response.Dialect) &&
            ((response.Capabilities & Smb2Constants.GlobalCapEncryption) != 0 || response.EncryptionAlgorithm.HasValue);
        if (_supportsEncryption)
        {
            _encryptionAlgorithm = response.EncryptionAlgorithm ?? Smb2Crypter.GetDefaultAlgorithmForDialect(response.Dialect);
        }
        InitializePreauthIntegrityHashIfNeeded();
        return response;
    }

    /// <summary>
    /// Performs the SMB2 SESSION_SETUP exchange, iterating NTLMSSP type-1/2/3
    /// blobs through the supplied <paramref name="blobProvider" />.
    /// </summary>
    public Task SessionSetupAsync(
        NtlmsspBlobProvider blobProvider,
        CancellationToken cancellationToken = default) =>
        SessionSetupAsync(blobProvider, sessionKeyProvider: null, cancellationToken);

    /// <summary>
    /// Performs SMB2 SESSION_SETUP and configures SMB signing from the established SessionKey,
    /// deriving dialect-specific signing material per [MS-SMB2] §3.1.5.1.
    /// </summary>
    /// <param name="blobProvider">Callback that emits NTLMSSP/Kerberos security blobs.</param>
    /// <param name="sessionKeyProvider">Callback that returns the NTLMSSP/Kerberos SessionKey after authentication succeeds.</param>
    /// <param name="cancellationToken">Cancellation token for the SMB round trips.</param>
    public async Task SessionSetupAsync(
        NtlmsspBlobProvider blobProvider,
        Smb2SessionKeyProvider? sessionKeyProvider,
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
                CaptureSessionEncryptionFlag(response);
                ConfigureSessionSecurityAfterSessionSetup(sessionKeyProvider);
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
        var response = Smb2TreeConnectResponse.Read(responseBytes.Span);
        _treeEncryptData = _supportsEncryption &&
            (response.ShareFlags & Smb2Constants.ShareFlagEncryptData) != 0;
        return response;
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

    /// <summary>Issues an SMB2 WRITE against the opened named pipe.</summary>
    public async Task PipeWriteAsync(
        Smb2NamedPipe pipe,
        ReadOnlyMemory<byte> data,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipe);
        ThrowIfDisposed();

        var request = new Smb2WriteRequest(
            Offset: 0,
            FileIdPersistent: pipe.FileIdPersistent,
            FileIdVolatile: pipe.FileIdVolatile,
            Data: data);

        var responseBytes = await ExchangeAsync(
            command: Smb2Command.Write,
            sessionId: _sessionId,
            treeId: _treeId,
            writeBody: (Span<byte> body) => request.WriteTo(body),
            maxBodySize: 48 + data.Length,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        var response = Smb2WriteResponse.Read(responseBytes.Span);
        if (response.Count != data.Length)
        {
            throw new Smb2ProtocolException($"SMB2 WRITE acknowledged {response.Count} of {data.Length} bytes.");
        }
    }

    /// <summary>Issues an SMB2 READ against the opened named pipe.</summary>
    public async Task<ReadOnlyMemory<byte>> PipeReadAsync(
        Smb2NamedPipe pipe,
        int maxLength,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipe);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);
        ThrowIfDisposed();

        var request = new Smb2ReadRequest(
            Length: checked((uint)maxLength),
            Offset: 0,
            FileIdPersistent: pipe.FileIdPersistent,
            FileIdVolatile: pipe.FileIdVolatile,
            MinimumCount: 1);

        var responseBytes = await ExchangeAsync(
            command: Smb2Command.Read,
            sessionId: _sessionId,
            treeId: _treeId,
            writeBody: (Span<byte> body) => request.WriteTo(body),
            maxBodySize: 49,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        return Smb2ReadResponse.Read(responseBytes.Span).Data;
    }

    /// <summary>Issues a raw SMB2 IOCTL with <c>FSCTL_PIPE_TRANSCEIVE</c>.</summary>
    public async Task<ReadOnlyMemory<byte>> PipeTransceiveAsync(
        Smb2NamedPipe pipe,
        ReadOnlyMemory<byte> data,
        int maxOutputResponse = 64 * 1024,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pipe);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxOutputResponse);
        ThrowIfDisposed();

        var request = new Smb2IoctlRequest(
            CtlCode: FsctlCode.PipeTransceive,
            FileIdPersistent: pipe.FileIdPersistent,
            FileIdVolatile: pipe.FileIdVolatile,
            Input: data,
            MaxOutputResponse: checked((uint)maxOutputResponse),
            IsFsctl: true);

        var responseBytes = await ExchangeAsync(
            command: Smb2Command.Ioctl,
            sessionId: _sessionId,
            treeId: _treeId,
            writeBody: (Span<byte> body) => request.WriteTo(body),
            maxBodySize: 56 + data.Length,
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

    private static Smb2ConnectionOptions ValidateOptions(Smb2ConnectionOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(options.Host);
        if (options.Port is <= 0 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(options), options.Port, "Port must be 1..65535.");
        }
        if (options.MaxSmb2MessageSize is <= Smb2Constants.PacketHeaderSize or > Smb2Constants.MaxNetBiosFrameSize)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.MaxSmb2MessageSize,
                $"SMB2 message quota must be {Smb2Constants.PacketHeaderSize + 1}..{Smb2Constants.MaxNetBiosFrameSize} bytes.");
        }
        return options;
    }

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
        byte[] packetBuffer = CreateRequestPacket(command, sessionId, treeId, maxBodySize);
        int bodySize = writeBody(packetBuffer.AsSpan(Smb2Constants.PacketHeaderSize));
        if (bodySize < 0 || bodySize > maxBodySize)
        {
            throw new Smb2ProtocolException($"SMB2 {command} writer produced invalid body size {bodySize}.");
        }
        int totalSize = Smb2Constants.PacketHeaderSize + bodySize;
        ReadOnlyMemory<byte> requestPacket = ProtectRequest(
            command,
            sessionId,
            treeId,
            packetBuffer,
            totalSize,
            out bool requestWasEncrypted);

        if (command == Smb2Command.Negotiate)
        {
            _lastNegotiateRequest = packetBuffer.AsSpan(0, totalSize).ToArray();
        }

        await _transport.SendAsync(requestPacket, cancellationToken).ConfigureAwait(false);

        var rawResponseBuffer = await _transport.ReceiveAsync(cancellationToken).ConfigureAwait(false);
        var responseBuffer = DecodeResponse(
            rawResponseBuffer.Span,
            sessionId,
            requestWasEncrypted,
            out bool responseWasEncrypted);

        CaptureSigningState(command, packetBuffer.AsSpan(0, totalSize), responseBuffer.Span);

        var responseHeader = Smb2PacketHeader.Read(responseBuffer.Span);
        if (!responseWasEncrypted)
        {
            VerifyResponseSignatureIfNeeded(command, responseHeader, responseBuffer.Span);
        }
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

    private byte[] CreateRequestPacket(Smb2Command command, ulong sessionId, uint treeId, int maxBodySize)
    {
        if (maxBodySize < 0 || maxBodySize > _options.MaxSmb2MessageSize - Smb2Constants.PacketHeaderSize)
        {
            throw new Smb2ProtocolException(
                $"SMB2 {command} request body quota {maxBodySize} exceeds the configured message quota of {_options.MaxSmb2MessageSize} bytes.");
        }

        byte[] packetBuffer = new byte[Smb2Constants.PacketHeaderSize + maxBodySize];
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
        return packetBuffer;
    }

    private void CaptureSigningState(
        Smb2Command command,
        ReadOnlySpan<byte> requestPacket,
        ReadOnlySpan<byte> responsePacket)
    {
        if (command == Smb2Command.Negotiate)
        {
            _lastNegotiateResponse = responsePacket.ToArray();
        }
        else if (command == Smb2Command.SessionSetup)
        {
            UpdatePreauthIntegrityHashForSessionSetup(requestPacket, responsePacket);
        }
    }

    private void CaptureSessionEncryptionFlag(Smb2SessionSetupResponse response)
    {
        _sessionEncryptData = _supportsEncryption &&
            (response.SessionFlags & Smb2Constants.SessionFlagEncryptData) != 0;
        if (_sessionEncryptData)
        {
            _signingRequired = false;
        }
    }

    private void ConfigureSessionSecurityAfterSessionSetup(Smb2SessionKeyProvider? sessionKeyProvider)
    {
        if (!_signingEnabled && !_signingRequired && !_supportsEncryption)
        {
            return;
        }

        ReadOnlyMemory<byte>? sessionKey = sessionKeyProvider?.Invoke();
        if (!sessionKey.HasValue || sessionKey.Value.IsEmpty)
        {
            throw new InvalidOperationException(
                "SMB2 signing or encryption was negotiated, but no NTLMSSP/Kerberos SessionKey was provided.");
        }

        SetSessionKey(sessionKey.Value.Span);
    }

    private ReadOnlyMemory<byte> ProtectRequest(
        Smb2Command command,
        ulong sessionId,
        uint treeId,
        byte[] packetBuffer,
        int totalSize,
        out bool requestWasEncrypted)
    {
        requestWasEncrypted = ShouldEncrypt(command, sessionId, treeId);
        if (requestWasEncrypted)
        {
            return EncryptRequest(packetBuffer.AsSpan(0, totalSize), sessionId);
        }

        SignRequestIfNeeded(command, sessionId, treeId, packetBuffer.AsSpan(0, totalSize));
        return packetBuffer.AsMemory(0, totalSize);
    }

    private ReadOnlyMemory<byte> DecodeResponse(
        ReadOnlySpan<byte> rawResponsePacket,
        ulong sessionId,
        bool requestWasEncrypted,
        out bool responseWasEncrypted)
    {
        if (rawResponsePacket.Length > _options.MaxSmb2MessageSize)
        {
            throw new Smb2ProtocolException(
                $"SMB2 response length {rawResponsePacket.Length} exceeds the configured message quota of {_options.MaxSmb2MessageSize} bytes.");
        }

        var responseBuffer = DecryptResponseIfNeeded(rawResponsePacket, sessionId, out responseWasEncrypted);
        if (responseBuffer.Length > _options.MaxSmb2MessageSize)
        {
            throw new Smb2ProtocolException(
                $"SMB2 decoded response length {responseBuffer.Length} exceeds the configured message quota of {_options.MaxSmb2MessageSize} bytes.");
        }
        if (!responseWasEncrypted && requestWasEncrypted)
        {
            throw new Smb2ProtocolException("SMB2 response was not encrypted after encryption was required.");
        }
        if (responseBuffer.Length < Smb2Constants.PacketHeaderSize)
        {
            throw new Smb2ProtocolException("SMB2 response too short for a header.");
        }

        return responseBuffer;
    }

    private void SignRequestIfNeeded(Smb2Command command, ulong sessionId, uint treeId, Span<byte> packet)
    {
        if (!ShouldSign(command, sessionId, treeId))
        {
            return;
        }

        if (_signer is null)
        {
            throw new InvalidOperationException(
                "SMB2 signing was negotiated, but no NTLMSSP/Kerberos SessionKey was provided.");
        }

        packet[16] |= (byte)Smb2Constants.FlagsSigned;
        _signer.Sign(packet);
    }

    private void VerifyResponseSignatureIfNeeded(
        Smb2Command command,
        Smb2PacketHeader responseHeader,
        ReadOnlySpan<byte> responsePacket)
    {
        if (!ShouldSign(command, responseHeader.SessionId, responseHeader.TreeId))
        {
            return;
        }

        if ((responseHeader.Flags & Smb2Constants.FlagsSigned) == 0)
        {
            throw new Smb2ProtocolException("SMB2 response was not signed after signing was negotiated.");
        }

        if (_signer is null || !_signer.VerifySignature(responsePacket))
        {
            throw new Smb2ProtocolException("SMB2 response signature verification failed.");
        }
    }

    private bool ShouldSign(Smb2Command command, ulong sessionId, uint treeId) =>
        (_signingEnabled || _signingRequired) &&
        sessionId != 0 &&
        command != Smb2Command.Negotiate &&
        command != Smb2Command.SessionSetup &&
        !ShouldEncrypt(command, sessionId, treeId);

    private bool ShouldEncrypt(Smb2Command command, ulong sessionId, uint treeId) =>
        _supportsEncryption &&
        sessionId != 0 &&
        command != Smb2Command.Negotiate &&
        command != Smb2Command.SessionSetup &&
        (_sessionEncryptData || (treeId != 0 && _treeEncryptData));

    private ReadOnlyMemory<byte> EncryptRequest(ReadOnlySpan<byte> packet, ulong sessionId)
    {
        if (_encrypter is null)
        {
            throw new InvalidOperationException(
                "SMB3 encryption was negotiated, but no NTLMSSP/Kerberos SessionKey was provided.");
        }

        byte[] nonce = new byte[_encrypter.NonceLength];
        RandomNumberGenerator.Fill(nonce);
        try
        {
            return _encrypter.EncryptMessage(packet, nonce, sessionId);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(nonce);
        }
    }

    private ReadOnlyMemory<byte> DecryptResponseIfNeeded(
        ReadOnlySpan<byte> responsePacket,
        ulong expectedSessionId,
        out bool responseWasEncrypted)
    {
        responseWasEncrypted = Smb2TransformHeader.HasTransformProtocolId(responsePacket);
        if (!responseWasEncrypted)
        {
            return responsePacket.ToArray();
        }

        if (!_supportsEncryption)
        {
            throw new Smb2ProtocolException("Received an SMB2 TRANSFORM_HEADER before encryption was negotiated.");
        }
        if (_decrypter is null)
        {
            throw new InvalidOperationException(
                "SMB3 encryption was negotiated, but no NTLMSSP/Kerberos SessionKey was provided.");
        }

        ulong sessionId = expectedSessionId != 0 ? expectedSessionId : _sessionId;
        return _decrypter.DecryptMessage(responsePacket, sessionId);
    }

    private static bool IsSmb3Dialect(Smb2Dialect dialect) =>
        dialect is Smb2Dialect.Smb300 or Smb2Dialect.Smb302 or Smb2Dialect.Smb311;

    private void InitializePreauthIntegrityHashIfNeeded()
    {
        if (_negotiatedDialect != Smb2Dialect.Smb311)
        {
            _preauthIntegrityHash = null;
            _lastNegotiateRequest = null;
            _lastNegotiateResponse = null;
            return;
        }

        if (_lastNegotiateRequest is null || _lastNegotiateResponse is null)
        {
            throw new Smb2ProtocolException("SMB 3.1.1 preauth hash cannot be initialized without NEGOTIATE messages.");
        }

        _preauthIntegrityHash = new byte[SHA512.HashSizeInBytes];
        UpdatePreauthIntegrityHash(_lastNegotiateRequest);
        UpdatePreauthIntegrityHash(_lastNegotiateResponse);
        _lastNegotiateRequest = null;
        _lastNegotiateResponse = null;
    }

    private void UpdatePreauthIntegrityHashForSessionSetup(
        ReadOnlySpan<byte> requestPacket,
        ReadOnlySpan<byte> responsePacket)
    {
        if (_negotiatedDialect != Smb2Dialect.Smb311 || _preauthIntegrityHash is null)
        {
            return;
        }

        UpdatePreauthIntegrityHash(requestPacket);
        UpdatePreauthIntegrityHash(responsePacket);
    }

    private void UpdatePreauthIntegrityHash(ReadOnlySpan<byte> packet)
    {
        byte[] previous = GetPreauthIntegrityHash().ToArray();
        byte[] input = new byte[previous.Length + packet.Length];
        previous.CopyTo(input, 0);
        packet.CopyTo(input.AsSpan(previous.Length));
        _preauthIntegrityHash = SHA512.HashData(input);
        CryptographicOperations.ZeroMemory(previous);
    }

    private ReadOnlySpan<byte> GetPreauthIntegrityHash() =>
        _preauthIntegrityHash ?? throw new InvalidOperationException(
            "SMB 3.1.1 signing requires the PreauthIntegrityHashValue before deriving the signing key.");

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

    /// <summary>Sends data with an SMB2 WRITE request.</summary>
    public Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default) =>
        _connection.PipeWriteAsync(this, data, cancellationToken);

    /// <summary>Reads data with an SMB2 READ request.</summary>
    public Task<ReadOnlyMemory<byte>> ReadAsync(int maxLength, CancellationToken cancellationToken = default) =>
        _connection.PipeReadAsync(this, maxLength, cancellationToken);

    /// <summary>Sends data and waits for a response in one round-trip via FSCTL_PIPE_TRANSCEIVE.</summary>
    public Task<ReadOnlyMemory<byte>> TransceiveAsync(
        ReadOnlyMemory<byte> data,
        int maxOutputResponse = 64 * 1024,
        CancellationToken cancellationToken = default) =>
        _connection.PipeTransceiveAsync(this, data, maxOutputResponse, cancellationToken);

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
