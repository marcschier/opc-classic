// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Transport
/// </summary>
internal sealed class ComTransport : ITransport, IDisposable
{

    /// <inheritdoc/>
    public string Protocol => "ncacn_ip_tcp";

    /// <inheritdoc/>
    public PropertyBag Properties { get; }

    /// <summary>
    /// Initialize class
    /// </summary>
    static ComTransport()
    {
        string localhost = null;
        try
        {
            localhost = Dns.GetHostName();
        }
        catch (System.Net.Sockets.SocketException)
        { // ignored
        }
        kLOCALHOST = localhost;
    }

    /// <summary>
    /// Create transport
    /// </summary>
    /// <exception cref="ProviderException">Thrown when the provider cannot complete the requested RPC transport operation.</exception>
    /// <param name="address">Network address or binding address for the remote endpoint.</param>
    /// <param name="properties">Property values used to initialize the COM descriptor.</param>
    public ComTransport(string address, PropertyBag properties)
    {
        Properties = properties;

        if (address == null)
        {
            throw new ProviderException("Null address.");
        }
        if (!address.StartsWith("ncacn_ip_tcp:", StringComparison.Ordinal))
        {
            throw new ProviderException("Not an ncacn_ip_tcp address.");
        }
        address = address.Substring(13);
        var index = address.IndexOf('[');
        if (index == -1)
        {
            throw new ProviderException("No port specifier present.");
        }
        var server = address.Substring(0, index);
        address = address.Substring(index + 1);
        index = address.IndexOf(']');
        if (index == -1)
        {
            throw new ProviderException("Port specifier not terminated.");
        }
        address = address.Substring(0, index);
        if (string.IsNullOrEmpty(server))
        {
            server = kLOCALHOST;
        }
        if (!int.TryParse(address, NumberStyles.Integer, CultureInfo.InvariantCulture, out int port))
        {
            throw new ProviderException("Invalid port specifier.");
        }
        _port = port;
        _host = server;
    }

    /// <inheritdoc/>
    public IEndpoint Attach(PresentationSyntax syntax)
    {
        if (_client != null)
        {
            throw new RpcException("Transport already attached.");
        }
        try
        {
            Log.Logger.Verbose("Connecting to " + _host + ":" + _port);
            _client = new TcpClient();
            var timeout = int.Parse((string)Properties.GetProperty("rpc.socketTimeout", "0"), CultureInfo.InvariantCulture);
            if (timeout != 0)
            {
                _client.ReceiveTimeout = timeout;
            }
            // Connects without a timeout. If a timeout is needed then someone
            // should write a blockingConnect() method similar to the
            _client.Connect(_host, _port);
            _stream = _client.GetStream();
            return new ComEndpoint(this, syntax);
        }
        catch (IOException)
        {
            try
            {
                Close();
            }
#pragma warning disable RCS1075 // Best-effort close after a failed I/O operation; the original IOException is re-thrown unchanged
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
            catch (Exception)
            { // ignored
            }
#pragma warning restore RECS0022
#pragma warning restore RCS1075
            throw;
        }
    }

    /// <inheritdoc/>
    public void Close()
    {
        try
        {
            if (_client != null)
            {
                Log.Logger.Verbose("Closing client to " + _host + ":" + _port);
                _client.Close();
            }
        }
        finally
        {
            _client?.Dispose();
            _client = null;
            _stream?.Dispose();
            _stream = null;
        }
    }

    /// <summary>Releases the underlying TCP resources.</summary>
    public void Dispose()
    {
        Close();
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc/>
    public void Send(NdrBuffer buffer)
    {
        if (_client == null)
        {
            throw new RpcException("Transport not attached.");
        }
        _stream.Write(buffer.Buf, 0, buffer.Length);
    }

    /// <inheritdoc/>
    public void Receive(NdrBuffer buffer)
    {
        if (_client == null)
        {
            throw new RpcException("Transport not attached.");
        }
        buffer.Length = _stream.Read(buffer.Buf, 0, buffer.GetCapacity());
    }

    /// <inheritdoc/>
    public override string ToString() => "Transport to " + _host + ":" + _port;

    private static readonly string kLOCALHOST;
    private readonly string _host;
    private readonly int _port;
    private Stream _stream;
    private TcpClient _client;
}
