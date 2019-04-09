// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.transport {
    using rpc;
    using rpc.core;
    using Serilog;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;
    using System;
    using System.IO;
    using System.Net;

    /// <summary>
    /// Borrowed all from ncacn_ip_tcp.RpcTransport from jarapac.
    /// </summary>
    internal sealed class JIComTransport : ITransport {

        /// <inheritdoc/>
        public string Protocol => PROTOCOL;

        /// <inheritdoc/>
        public Properties Properties { get; }

        // Use this as means of indicating to the reader thread that data is ready
        // to be read...
        // (alternatively could use a CyclicBarrier - but have to reset broken
        // barrier on a
        // timeout which causes spurious BrokenBarrierExceptions anyway (is this
        // http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=6253848 ?)).
        private readonly SynchronousQueue<object> readReadyHandoff = new SynchronousQueue<object>();

        private readonly long readReadyHandoffTimeoutSecs = kDEFAULT_READ_READY_HANDOFF_TIMEOUT_SECS;

        /// <summary>
        /// Initialize class
        /// </summary>
        static JIComTransport() {
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
        /// <exception cref="rpc.ProviderException"></exception>
        /// <param name="address"></param>
        /// <param name="selectorManager"></param>
        /// <param name="properties"></param>
        public JIComTransport(string address, SelectorManager selectorManager, Properties properties) {
            this._selectorManager = selectorManager;
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
            if ("".Equals(server)) {
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
            if (_attached) {
                throw new RpcException("Transport already attached.");
            }
            try {
                Log.Logger.Verbose("Opening socket on " + new InetSocketAddress(InetAddress.getByName(_host), _port));

                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final java.nio.channels.SocketChannel channel = java.nio.channels.SocketChannel.open();
                SocketChannel channel = SocketChannel.open();

                // Connects without a timeout. If a timeout is needed then someone
                // should write a blockingConnect() method similar to the
                // blockingRead() method.
                channel.connect(new InetSocketAddress(InetAddress.getByName(_host), _port));

                _channelWrapper = ChannelWrapperFactory.createChannelWrapper(_selectorManager, channel, new ChannelListenerAnonymousInnerClassHelper(this));

                // Configure the channel to be non-blocking, we will handle
                // simulating blocking mode using selectors. Using a blocking
                // connect above is fine as that does not cause the NIO code to
                // generate temporary pipe on Linux/Unix.
                channel.configureBlocking(false);

                _attached = true;

                // backup for not providing a timeout...
                channel.socket().KeepAlive = true;

                return new JIComEndpoint(this, syntax);
            }
            catch (IOException ex) {
                try {
                    Close();
                }
                catch (Exception) { // ignored
                }
                throw ex;
            }
        }

        private class ChannelListenerAnonymousInnerClassHelper : ChannelListener {
            private readonly JIComTransport _outerInstance;

            public ChannelListenerAnonymousInnerClassHelper(JIComTransport outerInstance) => this._outerInstance = outerInstance;

            public virtual void readReady() {
                try {
                    if (!_outerInstance.readReadyHandoff.offer(kHANDOFF, _outerInstance.readReadyHandoffTimeoutSecs, TimeUnit.SECONDS)) {
                        // Maybe the reader thread has died between
                        // adding read interest and waiting for the
                        // handoff
                        Log.Logger.Debug("Timeout while awaiting read ready handoff to " + _outerInstance);
                    }
                }
                catch (InterruptedException) {
                    // Re-set interrupt flag
                    Thread.CurrentThread.Interrupt();
                }
            }
        }

        /// <inheritdoc/>
        public void Close() {
            try {
                if (_channelWrapper != null) {
                    Log.Logger.Verbose("Closing " + _channelWrapper);
                    _channelWrapper.close();
                }
            }
            finally {
                _attached = false;
                _channelWrapper = null;
            }
        }

        /// <inheritdoc/>
        public void Send(NdrBuffer buffer) {
            if (!_attached) {
                throw new RpcException("Transport not attached.");
            }

            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final ByteBuffer byteBuffer = ByteBuffer.wrap(buffer.getBuffer(), 0, buffer.getLength());
            ByteBuffer byteBuffer = ByteBuffer.wrap(buffer.Buf, 0, buffer.Length);

            _channelWrapper.writeAll(byteBuffer);
        }

        /// <inheritdoc/>
        public void Receive(NdrBuffer buffer) {
            if (!_attached) {
                throw new RpcException("Transport not attached.");
            }

            //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
            //ORIGINAL LINE: final int timeoutMillis = getCurentTimeoutMillis();
            var timeoutMillis = CurentTimeoutMillis;

            // Register for read and wait for the read to occur
            _channelWrapper.registerForRead();

            try {
                object handoffResult;
                if (timeoutMillis == 0) {
                    handoffResult = readReadyHandoff.take();
                }
                else {
                    handoffResult = readReadyHandoff.poll(timeoutMillis, TimeUnit.MILLISECONDS);
                }

                if (handoffResult == null) {
                    throw new TimeoutException();
                }

                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final ByteBuffer wrapped = ByteBuffer.wrap(buffer.getBuffer());
                ByteBuffer wrapped = ByteBuffer.wrap(buffer.Buf);

                buffer.length = _channelWrapper.read(wrapped);
            }
            catch (InterruptedException) {
                // Re-set interrupted flag
                Thread.CurrentThread.Interrupt();

                throw new IOException("Interrupted while reading");
            }
        }

        /// <inheritdoc/>
        public override string ToString() => "Transport to " + _host + ":" + _port;

        /// <summary>
        /// Returns the current socket timeout.
        /// </summary>
        private int CurentTimeoutMillis {
            get {
                var timeout = 0;
                try {
                    timeout = int.Parse((string)Properties.GetProperty("rpc.socketTimeout", "0"));
                }
                catch (System.FormatException) { // ignored
                }

                return timeout;
            }
        }

        public const string PROTOCOL = "ncacn_ip_tcp";

        private static readonly string kLOCALHOST;
        private const long kDEFAULT_READ_READY_HANDOFF_TIMEOUT_SECS = 30;
        private static readonly object kHANDOFF = new object();
        private string _host;
        private int _port;
        private bool _attached;
        private ChannelWrapper _channelWrapper;
        private readonly SelectorManager _selectorManager;
    }
}