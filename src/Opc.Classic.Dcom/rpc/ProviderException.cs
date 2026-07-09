// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Rpc;

/// <summary>
/// Provider exception
/// </summary>
public class ProviderException : RpcException
{
    /// <inheritdoc/>
    public ProviderException()
    {
    }

    /// <inheritdoc/>
    public ProviderException(string message) :
        base(message)
    {
    }

    public ProviderException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    public ProviderException(string? message, int hresult) : base(message, hresult)
    {
    }
}
