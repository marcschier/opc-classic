// Copyright (c) 2026 marcschier. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Base rpc related exception
/// </summary>
public class RpcException : IOException
{
    /// <inheritdoc/>
    public RpcException()
    {
    }

    /// <inheritdoc/>
    public RpcException(string message) : base(message)
    {
    }

    public RpcException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public RpcException(string? message, int hresult) : base(message, hresult)
    {
    }
}
