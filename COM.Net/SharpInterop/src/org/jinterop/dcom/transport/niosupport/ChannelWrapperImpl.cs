// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.transport.niosupport
{
    using Serilog;
    using JISystem = common.JISystem;

	/// <summary>
	/// Wrapper for a <seealso cref="SelectableChannel"/> so that it can straightforwardly be
	/// used with a <seealso cref="SelectorManager"/>.
	/// <para>
	/// Allows non-blocking reads, but writes are blocking.
	/// </para>
	/// </summary>
	public sealed class ChannelWrapperImpl : ChannelWrapper
	{
		private readonly SelectorManager selectorManager;

		private readonly SelectableChannel selectableChannel;

        /// <summary>
        /// Constructor for ChannelWrapperImpl.
        /// </summary>
        /// <param name="selectorManager"> </param>
        /// <param name="selectableChannel"> </param>
        /// <param name="channelListener"> </param>
        /// <exception cref="IOException"> </exception>
        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: ChannelWrapperImpl(final SelectorManager selectorManager, final java.nio.channels.SelectableChannel selectableChannel, final ChannelListener channelListener) throws java.io.IOException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        internal ChannelWrapperImpl(SelectorManager selectorManager, SelectableChannel selectableChannel, ChannelListener channelListener)
		{
			this.selectorManager = selectorManager;
			this.selectableChannel = selectableChannel;
			ChannelListener = channelListener;

			selectorManager.registerChannel(selectableChannel, channelListener);
		}

        private ChannelListener ChannelListener { get; }

        /// <seealso cref= niosupport.ChannelWrapper#isConnected() </seealso>

        public bool Connected => ((SocketChannel)selectableChannel).Connected;

        /// <seealso cref= niosupport.ChannelWrapper#isOpen() </seealso>

        public bool Open => selectableChannel.Open;

        /// <seealso cref= niosupport.ChannelWrapper#getRemoteSocketAddress() </seealso>

        public SocketAddress RemoteSocketAddress => ((SocketChannel)selectableChannel).socket().RemoteSocketAddress;

        /// <seealso cref= niosupport.ChannelWrapper#read(java.nio.ByteBuffer) </seealso>

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public int read(final ByteBuffer buffer) throws java.io.IOException
        //JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
        public int read(ByteBuffer buffer)
		{
			return ((ReadableByteChannel) selectableChannel).read(buffer);
		}

        /// <seealso cref= niosupport.ChannelWrapper#registerForRead() </seealso>

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void registerForRead() throws java.io.IOException
        public void registerForRead()
		{
			selectorManager.ReadInterest = selectableChannel;
		}

        /// <seealso cref= niosupport.ChannelWrapper#unregisterForRead() </seealso>

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void unregisterForRead() throws java.io.IOException
        public void unregisterForRead()
		{
			selectorManager.removeReadInterest(selectableChannel);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int write(final ByteBuffer buffer) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		private int write(ByteBuffer buffer)
		{
			return ((WritableByteChannel) selectableChannel).write(buffer);
		}

        /// <seealso cref= niosupport.ChannelWrapper#writeAll(java.nio.ByteBuffer) </seealso>

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void writeAll(ByteBuffer buffer) throws java.io.IOException
        public void writeAll(ByteBuffer buffer)
		{
			while (buffer.hasRemaining())
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int bytesWritten = write(buffer);
				var bytesWritten = write(buffer);

                Log.Logger.Debug(this + " bytes written " + bytesWritten);
            }
        }

        /// <seealso cref= niosupport.ChannelWrapper#close() </seealso>

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void close() throws java.io.IOException
        public void close()
		{
			selectableChannel.close();
		}

		/// <seealso cref= java.lang.Object#toString() </seealso>

		public override string ToString()
		{
			return "Channel to " + RemoteSocketAddress;
		}
	}

}