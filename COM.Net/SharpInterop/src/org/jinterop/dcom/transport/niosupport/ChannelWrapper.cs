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


	/// <summary>
	/// A wrapper for a selectable channel and a selection manager
	/// </summary>
	public interface ChannelWrapper
	{
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void close() throws java.io.IOException;
		void close();

		/// <summary>
		/// Follows the contract of SocketChannel.read() for non-blocking operations.
		/// </summary>
		/// <param name="buffer"> </param>
		/// <returns> bytes read </returns>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: int read(ByteBuffer buffer) throws java.io.IOException;
		int read(ByteBuffer buffer);

		/// <summary>
		/// This method may result in a read attempt from the socket.
		/// </summary>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void registerForRead() throws java.io.IOException;
		void registerForRead();

		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void unregisterForRead() throws java.io.IOException;
		void unregisterForRead();

		/// <summary>
		/// Writes the remaining contents of the buffer. May block.
		/// </summary>
		/// <param name="buffer"> </param>
		/// <exception cref="IOException"> </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void writeAll(ByteBuffer buffer) throws java.io.IOException;
		void writeAll(ByteBuffer buffer);

		/// <returns> whether the underlying channel is connected. </returns>
		bool Connected {get;}

		/// <returns> whether the underlying channel is open. </returns>
		bool Open {get;}

		/// <summary>
		/// Gets the remote socket address
		/// </summary>
		/// <returns> the remote socket address </returns>
		/// <seealso cref= java.net.Socket#getRemoteSocketAddress() </seealso>
		SocketAddress RemoteSocketAddress {get;}
	}

}