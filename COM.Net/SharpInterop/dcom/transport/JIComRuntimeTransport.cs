// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.transport {
    using org.jinterop.dcom.common;
    using rpc;
    using rpc.core;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.IO;

    /// <summary>
    /// Transport
    /// </summary>
    internal sealed class JIComRuntimeTransport : ITransport {

        /// <summary>
        /// Create transport
        /// </summary>
        /// <exception cref="rpc.ProviderException"></exception>
        /// <param name="address"></param>
        /// <param name="properties"></param>
        public JIComRuntimeTransport(string address, Properties properties) => 
            Properties = properties; //address is ignored

        /// <inheritdoc/>
        public string Protocol => PROTOCOL;

        /// <inheritdoc/>
        public Properties Properties { get; }

        /// <inheritdoc/>
        public IEndpoint Attach(PresentationSyntax syntax) {
            if (_attached) {
                throw new RpcException("Transport already attached.");
            }

            IEndpoint endPoint = null;
            try {
                _socket = (SocketEx)JISystem.Internal_getSocket();
                _output = null;
                _input = null;
                _attached = true;
                endPoint = new JIComRuntimeEndpoint(this, syntax);
            }
            catch (Exception) {
                try {
                    Close();
                }
                catch (Exception) {
                }
            }
            return endPoint;
        }

        /// <inheritdoc/>
        public void Close() {
            try {
                if (_socket != null) {
                    _socket.Close();
                }
            }
            finally {
                _attached = false;
                _socket = null;
                _output = null;
                _input = null;
            }
        }

        /// <inheritdoc/>
        public void Send(NdrBuffer buffer) {
            if (!_attached) {
                throw new RpcException("Transport not attached.");
            }
            if (_output == null) {
                _output = _socket.GetOutputStream();
            }
            _output.Write(buffer.Buf, 0, buffer.Length);
            _output.Flush();
        }

        /// <inheritdoc/>
        public void Receive(NdrBuffer buffer) {
            if (!_attached) {
                throw new RpcException("Transport not attached.");
            }
            if (_input == null) {
                _input = _socket.GetInputStream();
            }
            buffer.Length = _input.Read(buffer.Buf, 0, buffer.GetCapacity());
        }

        public const string PROTOCOL = "ncacn_ip_tcp";
        private SocketEx _socket;
        private Stream _output;
        private Stream _input;
        private bool _attached;
    }

}