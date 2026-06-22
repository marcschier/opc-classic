// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Common.Ntlm;

public class SmbException : IOException
{
    public SmbException()
    {
    }

    public SmbException(string message)
        : base(message)
    {
    }

    public SmbException(string message, System.Exception innerException)
        : base(message, innerException)
    {
    }

    public SmbException(string? message, int hresult) : base(message, hresult)
    {
    }

    public virtual int GetNtStatus() => HResult;
}
