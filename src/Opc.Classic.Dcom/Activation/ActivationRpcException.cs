// Copyright (c) 2026 Opc.Classic Contributors. Licensed under the MIT License.

namespace Opc.Classic.Dcom.Activation;

/// <summary>
/// A modern activation call failed at the RPC layer.
/// </summary>
public sealed class ActivationRpcException : InvalidOperationException
{
    public ActivationRpcException()
    {
    }

    public ActivationRpcException(string message)
        : base(message)
    {
    }

    public ActivationRpcException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public ActivationRpcException(string operation, int hresult)
        : base($"{operation} RPC fault 0x{unchecked((uint)hresult):X8}.")
    {
        Hresult = hresult;
    }

    public int Hresult { get; }
}
