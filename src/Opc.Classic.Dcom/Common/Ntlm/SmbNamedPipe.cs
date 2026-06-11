// SPDX-License-Identifier: MIT

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SharpCifs.Smb;

public sealed class SmbNamedPipe : IDisposable
{
    public const int PipeTypeDceTransact = 0x0200;
    public const int PIPE_TYPE_RDWR = 0x0003;
    public const int PIPE_TYPE_DCE_TRANSACT = PipeTypeDceTransact;

    private readonly MemoryStream _input = new();
    private readonly MemoryStream _output = new();

    [SuppressMessage(
        "Design", "CA1054:URI-like parameters should not be strings",
        Justification = "SMB pipe URLs use the Windows UNC form (\\\\server\\IPC$\\pipe\\xyz) which is not a registered System.Uri scheme.")]
    public SmbNamedPipe(string url, int pipeType)
    {
        Url = url;
        PipeType = pipeType;
    }

    [SuppressMessage(
        "Design", "CA1056:URI-like properties should not be strings",
        Justification = "SMB pipe URLs use the Windows UNC form (\\\\server\\IPC$\\pipe\\xyz) which is not a registered System.Uri scheme.")]
    public string Url { get; }

    public int PipeType { get; }

    public Stream GetInputStream() => _input;

    public Stream GetNamedPipeInputStream() => _input;

    public Stream GetNamedPipeOutputStream() => _output;

    public void Dispose()
    {
        _input.Dispose();
        _output.Dispose();
        GC.SuppressFinalize(this);
    }
}
