// SPDX-License-Identifier: MIT

using System.IO;

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
}
