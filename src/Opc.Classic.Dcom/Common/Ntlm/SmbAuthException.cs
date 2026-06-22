// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Common.Ntlm;

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
