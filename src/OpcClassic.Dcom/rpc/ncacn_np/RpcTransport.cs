//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Rpc.Ncacn_Np {
    using OpcClassic.Dcom.Internal;
    using SharpInterop.Rpc.Core;
    using SharpCifs;
    using OpcClassic.Dcom.Internal.LegacyNdr;
    using SharpCifs.Netbios;
    using SharpCifs.Smb;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.IO;

    /// <summary>
    /// Rpc transport
    /// </summary>
    public class RpcTransport : ITransport {

        /// <inheritdoc/>
        public string Protocol => "ncacn_np";

        /// <inheritdoc/>
        public PropertyBag Properties { get; }

        /// <summary>
        /// Initialize transport
        /// </summary>
        static RpcTransport() {
            string localhost = null;
            try {
                localhost = NbtAddress.GetLocalHost().GetHostName();
            }
            catch (UnknownHostException) {
            }
            kLOCALHOST = localhost;
        }

        /// <summary>
        /// Create transport
        /// </summary>
        /// <param name="address"></param>
        /// <param name="properties"></param>
        /// <exception cref="ProviderException"></exception>
        public RpcTransport(string address, PropertyBag properties) {
            Properties = properties;
            Parse(address);
        }

        /// <inheritdoc/>
        public IEndpoint Attach(PresentationSyntax syntax) {
            if (_attached) {
                throw new RpcException("Transport already attached.");
            }

            // with the first flag an access denied exception occurs
            // with the second one file not found. so changing code here.
            /*pipe = new SmbNamedPipe(address, (0x2019f << 16) |
                    SmbNamedPipe.PIPE_TYPE_RDWR | SmbNamedPipe.PIPE_TYPE_DCE_TRANSACT);
             * */
            _pipe = new SmbNamedPipe(_address, SmbNamedPipe.PipeTypeDceTransact);
            _in2 = _pipe.GetInputStream();
            _out = _pipe.GetNamedPipeOutputStream();
            _in = _pipe.GetNamedPipeInputStream();
            _attached = true;
            return new ConnectionOrientedEndpoint(this, syntax);
        }

        /// <inheritdoc/>
        public void Close() {
            try {
                if (_pipe != null) {
                    _in.Close();
                    _out.Close();
                    _in2.Close();
                }
            }
            finally {
                _attached = false;
                _pipe = null;
            }
        }

        /// <inheritdoc/>
        public void Send(NdrBuffer buffer) {
            if (!_attached) {
                throw new RpcException("Transport not attached.");
            }
            _out.Write(buffer.Buf, 0, buffer.Length);
            _first = true;
        }

        /// <inheritdoc/>
        public void Receive(NdrBuffer buffer) {
            var buf = buffer.GetBuffer();
            int off = 0, bytes_to_read, n;

            if (!_attached) {
                throw new RpcException("Transport not attached.");
            }

            if (_first) {
                n = _in.Read(buf, 0, 1024); // TransactNamedPipe
                _first = false;
            }
            else { // Plain read
                n = _in2.Read(buf, off, buf.Length);
            }

            buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
            bytes_to_read = buffer.Dec_ndr_short();

            off += n;
            bytes_to_read -= n;

            while (bytes_to_read > 0) {
                n = _in2.Read(buf, off, bytes_to_read);
                off += n;
                bytes_to_read -= n;
            }
            buffer.SetLength(off);
        }

        /// <summary>
        /// Parse
        /// </summary>
        /// <param name="address"></param>
        protected internal void Parse(string address) {
            if (address == null) {
                throw new ProviderException("Null address.");
            }
            if (!address.StartsWith("ncacn_np:", StringComparison.Ordinal)) {
                throw new ProviderException("Not an ncacn_np address.");
            }
            address = address.Substring(9);
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
            while (address.StartsWith("\\", StringComparison.Ordinal)) {
                address = address.Substring(1);
            }
            if (!address.RegionMatches(true, 0, "PIPE", 0, 4)) {
                throw new ProviderException("Not a named pipe address.");
            }
            address = address.Substring(4);
            while (address.StartsWith("\\", StringComparison.Ordinal)) {
                address = address.Substring(1);
            }
            if ("".Equals(address)) {
                throw new ProviderException("Empty port.");
            }
            while (server.StartsWith("\\", StringComparison.Ordinal)) {
                server = server.Substring(1);
            }
            if ("".Equals(server)) {
                server = kLOCALHOST;
            }
            var properties = Properties;
            if (properties != null) {
                var userInfo = (string)properties.GetProperty("rpc.ncacn_np.username");
                if (userInfo == null) {
                    userInfo = Config.GetProperty("SharpCifs.smb.client.username");
                }
                if (userInfo != null) {
                    var domain = (string)properties.GetProperty("rpc.ncacn_np.domain");
                    if (domain == null) {
                        domain = Config.GetProperty("SharpCifs.smb.client.domain");
                    }
                    if (domain != null) {
                        userInfo = domain + ';' + userInfo;
                    }
                    var password = (string)properties.GetProperty("rpc.ncacn_np.password");
                    if (password == null) {
                        password = Config.GetProperty("SharpCifs.smb.client.password");
                    }
                    if (password != null) {
                        userInfo += ':' + password;
                    }
                }
                if (userInfo != null) {
                    server = userInfo + '@' + server;
                }
            }
            _address = "smb://" + server + "/IPC$/" + address;
        }

        private static readonly string kLOCALHOST;
        private string _address;
        private SmbNamedPipe _pipe;
        internal Stream _out;
        internal Stream _in;
        internal Stream _in2;
        // private readonly int _writeSize;
        // private readonly int _readSize;
        private bool _attached;
        private bool _first;
    }
}