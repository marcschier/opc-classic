//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Transport {
    using SharpInterop.Common;
    using SharpInterop.Rpc;
    using SharpInterop.Rpc.Core;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;
    using System.IO;
    using System.Net.Sockets;

    /// <summary>
    /// Transport
    /// </summary>
    internal sealed class ComRuntimeTransport : ITransport {

        /// <summary>
        /// Create transport
        /// </summary>
        /// <exception cref="ProviderException"></exception>
        /// <param name="address"></param>
        /// <param name="properties"></param>
        public ComRuntimeTransport(string address, Properties properties) {
            // address is ignored but should not be null
            System.Diagnostics.Debug.Assert(address != null);
            Properties = properties;
        }

        /// <inheritdoc/>
        public string Protocol => "ncacn_ip_tcp";

        /// <inheritdoc/>
        public Properties Properties { get; }

        /// <inheritdoc/>
        public IEndpoint Attach(PresentationSyntax syntax) {
            if (_attached) {
                throw new RpcException("Transport already attached.");
            }

            IEndpoint endPoint = null;
            try {
                _socket = Interop.Internal_getSocket();
                _stream = new System.Net.Sockets.NetworkStream(_socket);
                _attached = true;
                endPoint = new ComRuntimeEndpoint(this, syntax);
            }
            catch {
                try {
                    Close();
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch {
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
                }
            }
            return endPoint;
        }

        /// <inheritdoc/>
        public void Close() {
            try {
                _socket?.Close();
            }
            finally {
                _attached = false;
                _socket = null;
                _stream?.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Send(NdrBuffer buffer) {
            if (!_attached) {
                throw new RpcException("Transport not attached.");
            }
            _stream.Write(buffer.Buf, 0, buffer.Length);
            _stream.Flush();
        }

        /// <inheritdoc/>
        public void Receive(NdrBuffer buffer) {
            if (!_attached) {
                throw new RpcException("Transport not attached.");
            }
            buffer.Length = _stream.Read(buffer.Buf, 0, buffer.GetCapacity());
        }

        private Socket _socket;
        private Stream _stream;
        private bool _attached;
    }
}