//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Transport {
    using SharpInterop.Rpc;
    using SharpInterop.Rpc.Core;
    using OpcClassic.Dcom.Internal;
    using OpcClassic.Dcom.Internal.LegacyNdr;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;

    /// <summary>
    /// Transport
    /// </summary>
    internal sealed class ComTransport : ITransport {

        /// <inheritdoc/>
        public string Protocol => "ncacn_ip_tcp";

        /// <inheritdoc/>
        public PropertyBag Properties { get; }

        /// <summary>
        /// Initialize class
        /// </summary>
        static ComTransport() {
            string localhost = null;
            try {
                localhost = Dns.GetHostName();
            }
            catch (UnknownHostException) { // ignored
            }
            kLOCALHOST = localhost;
        }

        /// <summary>
        /// Create transport
        /// </summary>
        /// <exception cref="ProviderException"></exception>
        /// <param name="address"></param>
        /// <param name="properties"></param>
        public ComTransport(string address, PropertyBag properties) {
            Properties = properties;

            if (address == null) {
                throw new ProviderException("Null address.");
            }
            if (!address.StartsWith("ncacn_ip_tcp:", StringComparison.Ordinal)) {
                throw new ProviderException("Not an ncacn_ip_tcp address.");
            }
            address = address.Substring(13);
            var index = address.IndexOf('[');
            if (index == -1) {
                throw new ProviderException("No port specifier present.");
            }
            var server = address.Substring(0, index);
            address = address.Substring(index + 1);
            index = address.IndexOf(']');
            if (index == -1) {
                throw new ProviderException("Port specifier not terminated.");
            }
            address = address.Substring(0, index);
            if (string.IsNullOrEmpty(server)) {
                server = kLOCALHOST;
            }
            try {
                _port = int.Parse(address);
            }
            catch (Exception) {
                throw new ProviderException("Invalid port specifier.");
            }
            _host = server;
        }

        /// <inheritdoc/>
        public IEndpoint Attach(PresentationSyntax syntax) {
            if (_client != null) {
                throw new RpcException("Transport already attached.");
            }
            try {
                Log.Logger.Verbose("Connecting to " + _host + ":" + _port);
                _client = new TcpClient();
                var timeout = int.Parse((string)Properties.GetProperty("rpc.socketTimeout", "0"));
                if (timeout != 0) {
                    _client.ReceiveTimeout = timeout;
                }
                // Connects without a timeout. If a timeout is needed then someone
                // should write a blockingConnect() method similar to the
                _client.Connect(_host, _port);
                _stream = _client.GetStream();
                return new ComEndpoint(this, syntax);
            }
            catch (IOException ex) {
                try {
                    Close();
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch (Exception) { // ignored
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                }
                throw ex;
            }
        }

        /// <inheritdoc/>
        public void Close() {
            try {
                if (_client != null) {
                    Log.Logger.Verbose("Closing client to " + _host + ":" + _port);
                    _client.Close();
                }
            }
            finally {
                _client?.Dispose();
                _client = null;
                _stream?.Dispose();
                _stream = null;
            }
        }

        /// <inheritdoc/>
        public void Send(NdrBuffer buffer) {
            if (_client == null) {
                throw new RpcException("Transport not attached.");
            }
            _stream.Write(buffer.Buf, 0, buffer.Length);
        }

        /// <inheritdoc/>
        public void Receive(NdrBuffer buffer) {
            if (_client == null) {
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
}