// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

using System.IO.Pipelines;
using System.IO.Pipes;
using System.Net;
using System.Runtime.Versioning;
using System.Security.Principal;
using Opc.Classic.Transport;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Pipelines-backed <see cref="IAsyncTransport"/> over a connected local
/// Windows named pipe (<c>ncacn_np</c> against
/// <c>\\.\pipe\&lt;pipeName&gt;</c>).
/// </summary>
/// <remarks>
/// <para>
/// This transport is the local-only counterpart of <see cref="NcacnNpTransport"/>
/// (which tunnels over SMB2 for cross-machine named-pipe access). Local pipes
/// reach the kernel directly via <see cref="NamedPipeClientStream"/>, bypassing
/// SMB and the Windows Server service entirely. Authentication is handled by
/// the pipe's intrinsic impersonation: the connecting user's primary token is
/// implicitly conveyed to the server side, so DCE/RPC bind PDUs flow without
/// an SSPI auth-trailer block (pair the transport with
/// <see cref="NoOpAuthContext.Instance"/> when constructing a
/// <see cref="DcomCallChannel"/>).
/// </para>
/// <para>
/// Used by the in-repo activation client for local OPC servers that bind
/// LRPC rather than ncacn_ip_tcp (e.g. the OPC Foundation native
/// TestServer). The static convenience
/// <see cref="ConnectAsync(string,TokenImpersonationLevel,CancellationToken)"/>
/// opens <c>\\.\pipe\&lt;pipeName&gt;</c> with asynchronous I/O and
/// impersonation enabled by default; for full-control scenarios construct a
/// <see cref="NamedPipeClientStream"/> directly and wrap it via the
/// public constructor.
/// </para>
/// </remarks>
[SupportedOSPlatform("windows")]
public sealed class LocalNamedPipeTransport : IAsyncTransport
{
    private const string LocalHost = ".";

    private readonly NamedPipeClientStream _stream;
    private readonly string _pipeName;
    private int _disposed;

    /// <summary>
    /// Wraps a pre-connected <see cref="NamedPipeClientStream"/> as an
    /// <see cref="IAsyncTransport"/>. The transport takes ownership of the
    /// stream; do not access it directly after construction.
    /// </summary>
    public LocalNamedPipeTransport(NamedPipeClientStream stream, string pipeName)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(pipeName);
        if (!stream.IsConnected)
        {
            throw new ArgumentException(
                "NamedPipeClientStream must be connected before constructing the transport.",
                nameof(stream));
        }

        _stream = stream;
        _pipeName = pipeName;
        RemoteEndpoint = new NcacnNpEndPoint(LocalHost, pipeName);
        Input = PipeReader.Create(_stream);
        Output = PipeWriter.Create(_stream);
    }

    /// <inheritdoc />
    public EndPoint RemoteEndpoint { get; }

    /// <inheritdoc />
    public PipeReader Input { get; }

    /// <inheritdoc />
    public PipeWriter Output { get; }

    /// <summary>
    /// Gets the local pipe name (without the <c>\\.\pipe\</c> prefix).
    /// </summary>
    public string PipeName => _pipeName;

    /// <inheritdoc />
    public async ValueTask FlushAsync(CancellationToken cancellationToken = default) =>
        await Output.FlushAsync(cancellationToken).ConfigureAwait(false);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        await Input.CompleteAsync().ConfigureAwait(false);
        await Output.CompleteAsync().ConfigureAwait(false);
        await _stream.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Opens <c>\\.\pipe\&lt;pipeName&gt;</c> on the local machine using
    /// asynchronous I/O and impersonation, and returns the wrapped
    /// transport. The caller is responsible for disposing the returned
    /// transport (or the <see cref="DcomCallChannel"/> built from it).
    /// </summary>
    /// <param name="pipeName">Pipe name relative to <c>\\.\pipe\</c>.</param>
    /// <param name="impersonationLevel">Token impersonation level conveyed
    /// to the server. Defaults to <see cref="TokenImpersonationLevel.Impersonation"/>
    /// which matches the COM/DCOM <c>RPC_C_IMP_LEVEL_IMPERSONATE</c> default
    /// for local activations.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task<LocalNamedPipeTransport> ConnectAsync(
        string pipeName,
        TokenImpersonationLevel impersonationLevel = TokenImpersonationLevel.Impersonation,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pipeName);

        string normalizedPipeName = NormalizePipeName(pipeName);
        var stream = new NamedPipeClientStream(
            LocalHost,
            normalizedPipeName,
            PipeDirection.InOut,
            System.IO.Pipes.PipeOptions.Asynchronous,
            impersonationLevel);
        try
        {
            await stream.ConnectAsync(cancellationToken).ConfigureAwait(false);
            return new LocalNamedPipeTransport(stream, normalizedPipeName);
        }
        catch
        {
            await stream.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Normalizes pipe-name strings as returned by the DCOM activation
    /// resolver. Strips any leading <c>\\?\</c> / <c>\\.\</c> server prefix
    /// and any <c>pipe\</c> / <c>PIPE\</c> prefix so the result is the bare
    /// pipe name (which is what <see cref="NamedPipeClientStream"/> expects).
    /// </summary>
    public static string NormalizePipeName(string pipeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(pipeName);
        string value = pipeName.Trim().Replace('/', '\\');

        // Trim the optional [ ] wrapping that DCE/RPC string-binding form uses.
        if (value.Length >= 2 && value[0] == '[' && value[^1] == ']')
        {
            value = value[1..^1];
        }

        // Trim a leading \\.\pipe\ (or \\?\pipe\) UNC-style prefix.
        if (value.StartsWith("\\\\.\\pipe\\", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("\\\\?\\pipe\\", StringComparison.OrdinalIgnoreCase))
        {
            value = value[9..];
        }

        // Trim any leading backslashes.
        int index = 0;
        while (index < value.Length && value[index] == '\\')
        {
            index++;
        }
        if (index > 0)
        {
            value = value[index..];
        }

        // Trim a leading PIPE\ prefix (case-insensitive).
        if (value.StartsWith("PIPE\\", StringComparison.OrdinalIgnoreCase))
        {
            value = value[5..];
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new FormatException("Named-pipe endpoint is missing the pipe name.");
        }

        return value;
    }
}
