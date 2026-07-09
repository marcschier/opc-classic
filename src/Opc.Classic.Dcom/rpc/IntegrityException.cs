// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc;

/// <inheritdoc/>
public class IntegrityException : RpcException
{
    /// <inheritdoc/>
    public IntegrityException()
    {
    }

    /// <inheritdoc/>
    public IntegrityException(string message) : base(message)
    {
    }

    public IntegrityException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public IntegrityException(string? message, int hresult) : base(message, hresult)
    {
    }
}
