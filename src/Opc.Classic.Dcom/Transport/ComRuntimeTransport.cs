// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using System.Net.Sockets;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Transport
/// </summary>
internal sealed class ComRuntimeTransport : ITransport, IDisposable
{

    /// <summary>
    /// Create transport
    /// </summary>
    /// <exception cref="ProviderException">Thrown when the provider cannot complete the requested RPC transport operation.</exception>
    /// <param name="address">Network address or binding address for the remote endpoint.</param>
    /// <param name="properties">Property values used to initialize the COM descriptor.</param>
    public ComRuntimeTransport(string address, PropertyBag properties)
    {
        // address is ignored but should not be null
        System.Diagnostics.Debug.Assert(address != null);
        Properties = properties;
    }

    /// <inheritdoc/>
    public string Protocol => "ncacn_ip_tcp";

    /// <inheritdoc/>
    public PropertyBag Properties { get; }

    /// <inheritdoc/>
    public IEndpoint Attach(PresentationSyntax syntax)
    {
        if (_attached)
        {
            throw new RpcException("Transport already attached.");
        }

        IEndpoint endPoint = null;
        try
        {
            _socket = Interop.Internal_getSocket();
            _stream = new System.Net.Sockets.NetworkStream(_socket);
            _attached = true;
            endPoint = new ComRuntimeEndpoint(this, syntax);
        }
        catch
        {
            try
            {
                Close();
            }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch
            {
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }
        }
        return endPoint;
    }

    /// <inheritdoc/>
    public void Close()
    {
        try
        {
            _socket?.Close();
        }
        finally
        {
            _attached = false;
            _socket = null;
            _stream?.Dispose();
        }
    }

    /// <summary>Releases the accepted runtime socket resources.</summary>
    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public void Send(NdrBuffer buffer)
    {
        if (!_attached)
        {
            throw new RpcException("Transport not attached.");
        }
        _stream.Write(buffer.Buf, 0, buffer.Length);
        _stream.Flush();
    }

    /// <inheritdoc/>
    public void Receive(NdrBuffer buffer)
    {
        if (!_attached)
        {
            throw new RpcException("Transport not attached.");
        }
        buffer.Length = _stream.Read(buffer.Buf, 0, buffer.GetCapacity());
    }

    private Socket _socket;
    private Stream _stream;
    private bool _attached;
}
