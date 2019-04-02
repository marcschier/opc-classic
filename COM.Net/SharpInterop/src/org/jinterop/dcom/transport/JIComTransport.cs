// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.transport {


    using NdrBuffer = ndr.NdrBuffer;

    using JISystem = common.JISystem;
    using ChannelListener = niosupport.ChannelListener;
    using ChannelWrapper = niosupport.ChannelWrapper;
    using ChannelWrapperFactory = niosupport.ChannelWrapperFactory;
    using SelectorManager = niosupport.SelectorManager;

    using Endpoint = rpc.Endpoint;
    using ProviderException = rpc.ProviderException;
    using RpcException = rpc.RpcException;
    using Transport = rpc.Transport;
    using PresentationSyntax = rpc.core.PresentationSyntax;
    using Serilog;

    /// <summary>
    /// Borrowed all from ncacn_ip_tcp.RpcTransport from jarapac.
    /// 
    /// @exclude
    /// @since 1.0
    /// </summary>
    internal sealed class JIComTransport : Transport
	{
		public const string PROTOCOL = "ncacn_ip_tcp";

		private static readonly string LOCALHOST;

		private const long DEFAULT_READ_READY_HANDOFF_TIMEOUT_SECS = 30;

		private static readonly object HANDOFF = new object();
        private string host;

		private int port;

		private bool attached;

		private ChannelWrapper channelWrapper;

		private readonly SelectorManager selectorManager;

		// Use this as means of indicating to the reader thread that data is ready
		// to be read...
		// (alternatively could use a CyclicBarrier - but have to reset broken
		// barrier on a
		// timeout which causes spurious BrokenBarrierExceptions anyway (is this
		// http://bugs.sun.com/bugdatabase/view_bug.do?bug_id=6253848 ?)).
		private readonly SynchronousQueue<object> readReadyHandoff = new SynchronousQueue<object>();

		private long readReadyHandoffTimeoutSecs = DEFAULT_READ_READY_HANDOFF_TIMEOUT_SECS;

		static JIComTransport()
		{
			string localhost = null;
			try
			{
				localhost = InetAddress.LocalHost.HostName;
			}
			catch (UnknownHostException)
			{ // ignored
			}
			LOCALHOST = localhost;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIComTransport(String address, org.jinterop.dcom.transport.niosupport.SelectorManager selectorManager, java.util.Properties properties) throws rpc.ProviderException
		public JIComTransport(string address, SelectorManager selectorManager, Properties properties)
		{
			this.selectorManager = selectorManager;
			Properties = properties;

			parse(address);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void parse(String address) throws rpc.ProviderException
		private void parse(string address)
		{
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
			if ("".Equals(server))
			{
				server = LOCALHOST;
			}
			try
			{
				port = int.Parse(address);
			}
			catch (Exception)
			{
				throw new ProviderException("Invalid port specifier.");
			}
			host = server;
		}

        /// <seealso cref= rpc.Transport#getProtocol() </seealso>
        public string Protocol => PROTOCOL;

        /// <seealso cref= rpc.Transport#getProperties() </seealso>
        public Properties Properties { get; }

        /// <seealso cref= rpc.Transport#attach(rpc.core.PresentationSyntax) </seealso>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public rpc.Endpoint attach(rpc.core.PresentationSyntax syntax) throws java.io.IOException
        public Endpoint attach(PresentationSyntax syntax)
		{
			if (attached)
			{
				throw new RpcException("Transport already attached.");
			}

			try
			{
                Log.Logger.Verbose("Opening socket on " + new InetSocketAddress(InetAddress.getByName(host), port));

                //JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
                //ORIGINAL LINE: final java.nio.channels.SocketChannel channel = java.nio.channels.SocketChannel.open();
                SocketChannel channel = SocketChannel.open();

				// Connects without a timeout. If a timeout is needed then someone
				// should write a blockingConnect() method similar to the
				// blockingRead() method.
				channel.connect(new InetSocketAddress(InetAddress.getByName(host), port));

				channelWrapper = ChannelWrapperFactory.createChannelWrapper(selectorManager, channel, new ChannelListenerAnonymousInnerClassHelper(this));

				// Configure the channel to be non-blocking, we will handle
				// simulating blocking mode using selectors. Using a blocking
				// connect above is fine as that does not cause the NIO code to
				// generate temporary pipe on Linux/Unix.
				channel.configureBlocking(false);

				attached = true;

				// backup for not providing a timeout...
				channel.socket().KeepAlive = true;

				return new JIComEndpoint(this, syntax);
			}
			catch (IOException ex)
			{
				try
				{
					close();
				}
				catch (Exception)
				{ // ignored
				}
				throw ex;
			}
		}

		private class ChannelListenerAnonymousInnerClassHelper : ChannelListener
		{
			private readonly JIComTransport outerInstance;

			public ChannelListenerAnonymousInnerClassHelper(JIComTransport outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void readReady()
			{
				try
				{
					if (!outerInstance.readReadyHandoff.offer(HANDOFF, outerInstance.readReadyHandoffTimeoutSecs, TimeUnit.SECONDS))
					{
                        // Maybe the reader thread has died between
                        // adding read interest and waiting for the
                        // handoff
                        Log.Logger.Debug("Timeout while awaiting read ready handoff to " + outerInstance);
                    }
                }
				catch (InterruptedException)
				{
					// Re-set interrupt flag
					Thread.CurrentThread.Interrupt();
				}
			}
		}

		/// <seealso cref= rpc.Transport#close() </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
		public void close()
		{
			try
			{
				if (channelWrapper != null)
				{
                    Log.Logger.Verbose("Closing " + channelWrapper);
                    channelWrapper.close();
				}
			}
			finally
			{
				attached = false;
				channelWrapper = null;
			}
		}

		/// <seealso cref= rpc.Transport#send(ndr.NdrBuffer) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void send(ndr.NdrBuffer buffer) throws java.io.IOException
		public void send(NdrBuffer buffer)
		{
			if (!attached)
			{
				throw new RpcException("Transport not attached.");
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer byteBuffer = ByteBuffer.wrap(buffer.getBuffer(), 0, buffer.getLength());
			ByteBuffer byteBuffer = ByteBuffer.wrap(buffer.Buffer, 0, buffer.Length);

			channelWrapper.writeAll(byteBuffer);
		}

		/// <seealso cref= rpc.Transport#receive(ndr.NdrBuffer) </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void receive(ndr.NdrBuffer buffer) throws java.io.IOException
		public void receive(NdrBuffer buffer)
		{
			if (!attached)
			{
				throw new RpcException("Transport not attached.");
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int timeoutMillis = getCurentTimeoutMillis();
			var timeoutMillis = CurentTimeoutMillis;

			// Register for read and wait for the read to occur
			channelWrapper.registerForRead();

			try
			{
				object handoffResult;
				if (timeoutMillis == 0)
				{
					handoffResult = readReadyHandoff.take();
				}
				else
				{
					handoffResult = readReadyHandoff.poll(timeoutMillis, TimeUnit.MILLISECONDS);
				}

				if (handoffResult == null)
				{
					throw new SocketTimeoutException();
				}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ByteBuffer wrapped = ByteBuffer.wrap(buffer.getBuffer());
				ByteBuffer wrapped = ByteBuffer.wrap(buffer.Buffer);

				buffer.length = channelWrapper.read(wrapped);
			}
			catch (InterruptedException)
			{
				// Re-set interrupted flag
				Thread.CurrentThread.Interrupt();

				throw new IOException("Interrupted while reading");
			}
		}

		/// <summary>
		/// Returns the current socket timeout.
		/// </summary>
		private int CurentTimeoutMillis
		{
			get
			{
				var timeout = 0;
				try
				{
					timeout = int.Parse(Properties.getProperty("rpc.socketTimeout", "0"));
				}
				catch (System.FormatException)
				{ // ignored
				}
    
				return timeout;
			}
		}

		/// <seealso cref= java.lang.Object#toString() </seealso>

		public override string ToString()
		{
			return "Transport to " + host + ":" + port;
		}
	}

}