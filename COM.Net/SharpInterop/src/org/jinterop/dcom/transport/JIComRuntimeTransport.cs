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

    using Endpoint = rpc.Endpoint;
    using RpcException = rpc.RpcException;
    using Transport = rpc.Transport;
    using PresentationSyntax = rpc.core.PresentationSyntax;
    /// <summary>
    /// @exclude
    /// @since 1.0
    /// 
    /// </summary>
    internal sealed class JIComRuntimeTransport : Transport
	{


		public const string PROTOCOL = "ncacn_ip_tcp";
        private Socket socket;

		private System.IO.Stream output;

		private System.IO.Stream input;

		private bool attached;


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public JIComRuntimeTransport(String address, java.util.Properties properties) throws rpc.ProviderException
		public JIComRuntimeTransport(string address, Properties properties)
		{
			Properties = properties;
			//address is ignored
		}

        public string Protocol => PROTOCOL;

        public Properties Properties { get; }

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public rpc.Endpoint attach(rpc.core.PresentationSyntax syntax) throws java.io.IOException
        public Endpoint attach(PresentationSyntax syntax)
		{
			if (attached)
			{
				throw new RpcException("Transport already attached.");
			}

			Endpoint endPoint = null;
			try
			{
				socket = (Socket)JISystem.internal_getSocket();
				output = null;
				input = null;
				attached = true;
				endPoint = new JIComRuntimeEndpoint(this, syntax);
			}
			catch (Exception)
			{
				try
				{
					close();
				}
				catch (Exception)
				{
				}
			}
			return endPoint;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
		public void close()
		{
			try
			{
				if (socket != null)
				{
					socket.close();
				}
			}
			finally
			{
				attached = false;
				socket = null;
				output = null;
				input = null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void send(ndr.NdrBuffer buffer) throws java.io.IOException
		public void send(NdrBuffer buffer)
		{
			if (!attached)
			{
				throw new RpcException("Transport not attached.");
			}
			if (output == null)
			{
				output = socket.OutputStream;
			}
			output.Write(buffer.Buffer, 0, buffer.Length);
			output.Flush();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void receive(ndr.NdrBuffer buffer) throws java.io.IOException
		public void receive(NdrBuffer buffer)
		{
			if (!attached)
			{
				throw new RpcException("Transport not attached.");
			}
			if (input == null)
			{
				input = socket.InputStream;
			}
			buffer.length = input.Read(buffer.Buffer, 0, buffer.Capacity);
		}


	}

}