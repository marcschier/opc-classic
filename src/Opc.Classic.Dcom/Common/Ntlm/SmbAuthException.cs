// SPDX-License-Identifier: MIT

namespace SharpCifs.Smb;

public sealed class SmbAuthException : SmbException
{
    public SmbAuthException()
    {
    }

    public SmbAuthException(string message)
        : base(message)
    {
    }

    public SmbAuthException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public SmbAuthException(string? message, int hresult) : base(message, hresult)
    {
    }
}
