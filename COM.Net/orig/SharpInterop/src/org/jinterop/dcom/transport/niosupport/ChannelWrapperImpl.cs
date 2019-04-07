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

namespace org.jinterop.dcom.transport.niosupport {


	using JISystem = org.jinterop.dcom.common.JISystem;

	/// <summary>
	/// Wrapper for a <seealso cref="SelectableChannel"/> so that it can straightforwardly be
	/// used with a <seealso cref="SelectorManager"/>.
	/// <para>
	/// Allows non-blocking reads, but writes are blocking.
	/// </para>
	/// </summary>
	public sealed class ChannelWrapperImpl : ChannelWrapper {
		private readonly SelectorManager SelectorManager;

		private readonly SelectableChannel SelectableChannel;

		private readonly ChannelListener ChannelListener_Renamed;

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
		public ChannelWrapperImpl(SelectorManager selectorManager, SelectableChannel selectableChannel, ChannelListener channelListener) {
			this.SelectorManager = selectorManager;
			this.SelectableChannel = selectableChannel;
			this.ChannelListener_Renamed = channelListener;

			selectorManager.RegisterChannel(selectableChannel, channelListener);
		}

		private ChannelListener ChannelListener {
			get {
				return ChannelListener_Renamed;
			}
		}

		/// <seealso cref= org.jinterop.dcom.transport.niosupport.ChannelWrapper#isConnected() </seealso>

		public bool Connected {
			get {
				return ((SocketChannel) SelectableChannel).Connected;
			}
		}

		/// <seealso cref= org.jinterop.dcom.transport.niosupport.ChannelWrapper#isOpen() </seealso>

		public bool Open {
			get {
				return SelectableChannel.Open;
			}
		}

		/// <seealso cref= org.jinterop.dcom.transport.niosupport.ChannelWrapper#getRemoteSocketAddress() </seealso>

		public SocketAddress RemoteSocketAddress {
			get {
				return ((SocketChannel) SelectableChannel).socket().RemoteSocketAddress;
			}
		}

		/// <seealso cref= org.jinterop.dcom.transport.niosupport.ChannelWrapper#read(java.nio.ByteBuffer) </seealso>

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public int read(final ByteBuffer buffer) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		public int Read(ByteBuffer buffer) {
			return ((ReadableByteChannel) SelectableChannel).read(buffer);
		}

		/// <seealso cref= org.jinterop.dcom.transport.niosupport.ChannelWrapper#registerForRead() </seealso>

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void registerForRead() throws java.io.IOException
		public void RegisterForRead() {
			SelectorManager.ReadInterest = SelectableChannel;
		}

		/// <seealso cref= org.jinterop.dcom.transport.niosupport.ChannelWrapper#unregisterForRead() </seealso>

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void unregisterForRead() throws java.io.IOException
		public void UnregisterForRead() {
			SelectorManager.RemoveReadInterest(SelectableChannel);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private int write(final ByteBuffer buffer) throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
		private int Write(ByteBuffer buffer) {
			return ((WritableByteChannel) SelectableChannel).write(buffer);
		}

		/// <seealso cref= org.jinterop.dcom.transport.niosupport.ChannelWrapper#writeAll(java.nio.ByteBuffer) </seealso>

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void writeAll(ByteBuffer buffer) throws java.io.IOException
		public void WriteAll(ByteBuffer buffer) {
			while (buffer.hasRemaining()) {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int bytesWritten = write(buffer);
				int bytesWritten = Write(buffer);

				if (JISystem.Logger.isLoggable(Level.FINE)) {
					JISystem.Logger.fine(this + " bytes written " + bytesWritten);
				}
			}
		}

		/// <seealso cref= org.jinterop.dcom.transport.niosupport.ChannelWrapper#close() </seealso>

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
		public void Close() {
			SelectableChannel.close();
		}

		/// <seealso cref= java.lang.Object#toString() </seealso>

		public override string ToString() {
			return "Channel to " + RemoteSocketAddress;
		}
	}

}