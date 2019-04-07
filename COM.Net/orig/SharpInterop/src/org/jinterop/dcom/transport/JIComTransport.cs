using System;
using System.Threading;

/// <summary>
/// j-Interop (Pure Java implementation of DCOM protocol)
/// 
/// Copyright (c) 2013 Vikram Roopchand
/// 
/// All rights reserved. This program and the accompanying materials
/// are made available under the terms of the Eclipse Public License v1.0
/// which accompanies this distribution, and is available at
/// http://www.eclipse.org/legal/epl-v10.html
/// 
/// Contributors:
/// Vikram Roopchand  - Moving to EPL from LGPL v3.
/// 
/// </summary>

namespace org.jinterop.dcom.transport {


	using NdrBuffer = ndr.NdrBuffer;

	using JISystem = org.jinterop.dcom.common.JISystem;
	using ChannelListener = org.jinterop.dcom.transport.niosupport.ChannelListener;
	using ChannelWrapper = org.jinterop.dcom.transport.niosupport.ChannelWrapper;
	using ChannelWrapperFactory = org.jinterop.dcom.transport.niosupport.ChannelWrapperFactory;
	using SelectorManager = org.jinterop.dcom.transport.niosupport.SelectorManager;

	using Endpoint = rpc.Endpoint;
	using ProviderException = rpc.ProviderException;
	using RpcException = rpc.RpcException;
	using Transport = rpc.Transport;
	using PresentationSyntax = rpc.core.PresentationSyntax;

	/// <summary>
	/// Borrowed all from ncacn_ip_tcp.RpcTransport from jarapac.
	/// 
	/// @exclude
	/// @since 1.0
	/// </summary>
	internal sealed class JIComTransport : Transport {
		public const string PROTOCOL = "ncacn_ip_tcp";

		private static readonly string LOCALHOST;

		private const long DEFAULT_READ_READY_HANDOFF_TIMEOUT_SECS = 30;

		private static object HANDOFF = new object();

		private Properties Properties_Renamed;

		private string Host;

		private int Port;

		private bool Attached;

		private ChannelWrapper ChannelWrapper;

		private readonly SelectorManager SelectorManager;

		// Use this as means of indicating to the reader thread that data is ready
		// to be read...
		// (alternatively could use a CyclicBarrier - but have to reset broken
		// barrier on a
		// timeout which causes spurious BrokenBarrierExceptions anyway (is this
		// http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=6253848 ?)).
		private readonly SynchronousQueue<object> ReadReadyHandoff = new SynchronousQueue<object>();

		private long ReadReadyHandoffTimeoutSecs = DEFAULT_READ_READY_HANDOFF_TIMEOUT_SECS;

		static JIComTransport() {
			string localhost = null;
			try {
				localhost = InetAddress.LocalHost.HostName;
			}
			catch (UnknownHostException)
			{ // ignored
			}
			LOCALHOST = localhost;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIComTransport(String address, org.jinterop.dcom.transport.niosupport.SelectorManager selectorManager, java.util.Properties properties) throws rpc.ProviderException
		public JIComTransport(string address, SelectorManager selectorManager, Properties properties) {
			this.SelectorManager = selectorManager;
			this.Properties_Renamed = properties;

			Parse(address);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void parse(String address) throws rpc.ProviderException
		private void Parse(string address) {
			if (address == null) {
				throw new ProviderException("Null address.");
			}
			if (!address.StartsWith("ncacn_ip_tcp:", StringComparison.Ordinal)) {
				throw new ProviderException("Not an ncacn_ip_tcp address.");
			}
			address = address.Substring(13);
			int index = address.IndexOf('[');
			if (index == -1) {
				throw new ProviderException("No port specifier present.");
			}
			string server = address.Substring(0, index);
			address = address.Substring(index + 1);
			index = address.IndexOf(']');
			if (index == -1) {
				throw new ProviderException("Port specifier not terminated.");
			}
			address = address.Substring(0, index);
			if ("".Equals(server)) {
				server = LOCALHOST;
			}
			try {
				Port = int.Parse(address);
			}
			catch (Exception) {
				throw new ProviderException("Invalid port specifier.");
			}
			Host = server;
		}

		/// <seealso cref= rpc.Transport#getProtocol() </seealso>
		public string Protocol {
			get {
				return PROTOCOL;
			}
		}

		/// <seealso cref= rpc.Transport#getProperties() </seealso>
		public Properties Properties {
			get {
				return Properties_Renamed;
			}
		}

		/// <seealso cref= rpc.Transport#attach(rpc.core.PresentationSyntax) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.Endpoint attach(rpc.core.PresentationSyntax syntax) throws java.io.IOException
		public Endpoint Attach(PresentationSyntax syntax) {
			if (Attached) {
				throw new RpcException("Transport already attached.");
			}

			try {
				if (JISystem.Logger.isLoggable(Level.FINEST)) {
					JISystem.Logger.finest("Opening socket on " + new InetSocketAddress(InetAddress.getByName(Host), Port));
				}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.nio.channels.SocketChannel channel = java.nio.channels.SocketChannel.open();
				SocketChannel channel = SocketChannel.open();

				// Connects without a timeout. If a timeout is needed then someone
				// should write a blockingConnect() method similar to the
				// blockingRead() method.
				channel.connect(new InetSocketAddress(InetAddress.getByName(Host), Port));

				ChannelWrapper = ChannelWrapperFactory.CreateChannelWrapper(SelectorManager, channel, new ChannelListenerAnonymousInnerClassHelper(this));

				// Configure the channel to be non-blocking, we will handle
				// simulating blocking mode using selectors. Using a blocking
				// connect above is fine as that does not cause the NIO code to
				// generate temporary pipe on Linux/Unix.
				channel.configureBlocking(false);

				Attached = true;

				// backup for not providing a timeout...
				channel.socket().KeepAlive = true;

				return new JIComEndpoint(this, syntax);
			}
			catch (IOException ex) {
				try {
					Close();
				}
				catch (Exception)
				{ // ignored
				}
				throw ex;
			}
		}

		private class ChannelListenerAnonymousInnerClassHelper : ChannelListener {
			private readonly JIComTransport OuterInstance;

			public ChannelListenerAnonymousInnerClassHelper(JIComTransport outerInstance) {
				this.OuterInstance = outerInstance;
			}


			public virtual void ReadReady() {
				try {
					if (!OuterInstance.ReadReadyHandoff.offer(HANDOFF, OuterInstance.ReadReadyHandoffTimeoutSecs, TimeUnit.SECONDS)) {
						// Maybe the reader thread has died between
						// adding read interest and waiting for the
						// handoff
						if (JISystem.Logger.isLoggable(Level.FINE)) {
							JISystem.Logger.fine("Timeout while awaiting read ready handoff to " + OuterInstance);
						}
					}
				}
				catch (InterruptedException) {
					// Re-set interrupt flag
					Thread.CurrentThread.Interrupt();
				}
			}
		}

		/// <seealso cref= rpc.Transport#close() </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
		public void Close() {
			try {
				if (ChannelWrapper != null) {
					if (JISystem.Logger.isLoggable(Level.FINEST)) {
						JISystem.Logger.finest("Closing " + ChannelWrapper);
					}
					ChannelWrapper.Close();
				}
			}
			finally {
				Attached = false;
				ChannelWrapper = null;
			}
		}

		/// <seealso cref= rpc.Transport#send(ndr.NdrBuffer) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void send(ndr.NdrBuffer buffer) throws java.io.IOException
		public void Send(NdrBuffer buffer) {
			if (!Attached) {
				throw new RpcException("Transport not attached.");
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer byteBuffer = ByteBuffer.wrap(buffer.getBuffer(), 0, buffer.getLength());
			ByteBuffer byteBuffer = ByteBuffer.wrap(buffer.Buffer, 0, buffer.Length);

			ChannelWrapper.WriteAll(byteBuffer);
		}

		/// <seealso cref= rpc.Transport#receive(ndr.NdrBuffer) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void receive(ndr.NdrBuffer buffer) throws java.io.IOException
		public void Receive(NdrBuffer buffer) {
			if (!Attached) {
				throw new RpcException("Transport not attached.");
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int timeoutMillis = getCurentTimeoutMillis();
			int timeoutMillis = CurentTimeoutMillis;

			// Register for read and wait for the read to occur
			ChannelWrapper.RegisterForRead();

			try {
				object handoffResult;
				if (timeoutMillis == 0) {
					handoffResult = ReadReadyHandoff.take();
				}
				else {
					handoffResult = ReadReadyHandoff.poll(timeoutMillis, TimeUnit.MILLISECONDS);
				}

				if (handoffResult == null) {
					throw new SocketTimeoutException();
				}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer wrapped = ByteBuffer.wrap(buffer.getBuffer());
				ByteBuffer wrapped = ByteBuffer.wrap(buffer.Buffer);

				buffer.length = ChannelWrapper.Read(wrapped);
			}
			catch (InterruptedException) {
				// Re-set interrupted flag
				Thread.CurrentThread.Interrupt();

				throw new IOException("Interrupted while reading");
			}
		}

		/// <summary>
		/// Returns the current socket timeout.
		/// </summary>
		private int CurentTimeoutMillis {
			get {
				int timeout = 0;
				try {
					timeout = int.Parse(this.Properties_Renamed.getProperty("rpc.socketTimeout", "0"));
				}
				catch (System.FormatException)
				{ // ignored
				}
    
				return timeout;
			}
		}

		/// <seealso cref= java.lang.Object#toString() </seealso>

		public override string ToString() {
			return "Transport to " + Host + ":" + Port;
		}
	}

}