using System;

// 
// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
// 
// j-Interop (Pure Java implementation of DCOM protocol)
// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 



namespace rpc.ncacn_np
{


	using Config = jcifs.Config;
	using NbtAddress = jcifs.netbios.NbtAddress;
	using SmbNamedPipe = jcifs.smb.SmbNamedPipe;
	using NdrBuffer = ndr.NdrBuffer;
	using PresentationSyntax = core.PresentationSyntax;

	public class RpcTransport : Transport
	{

		public const string PROTOCOL = "ncacn_np";

		private static readonly string LOCALHOST;


		private string address;

		private readonly Properties properties;

		private SmbNamedPipe pipe;
		internal System.IO.Stream @out;
		internal System.IO.Stream @in;
		internal System.IO.Stream in2;
		private readonly int writeSize;
		private readonly int readSize;
		private bool attached;
		private bool first;

		static RpcTransport()
		{
			string localhost = null;
			try
			{
				localhost = NbtAddress.LocalHost.HostName;
			}
			catch (UnknownHostException)
			{
			}
			LOCALHOST = localhost;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public RpcTransport(String address, java.util.Properties properties) throws rpc.ProviderException
		public RpcTransport(string address, Properties properties)
		{
			this.properties = properties;
			parse(address);
		}

        public virtual string Protocol => PROTOCOL;

        public virtual Properties Properties => properties;

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public rpc.Endpoint attach(rpc.core.PresentationSyntax syntax) throws java.io.IOException
        public virtual Endpoint attach(PresentationSyntax syntax)
		{
			if (attached)
			{
				throw new RpcException("Transport already attached.");
			}

			//with the first flag an access denied exception occurs
			//with the second one file not found. so changing code here.
			/*pipe = new SmbNamedPipe(address, (0x2019f << 16) |
					SmbNamedPipe.PIPE_TYPE_RDWR | SmbNamedPipe.PIPE_TYPE_DCE_TRANSACT);
			 * */
			pipe = new SmbNamedPipe(address, SmbNamedPipe.PIPE_TYPE_DCE_TRANSACT);
			in2 = pipe.InputStream;
			@out = pipe.NamedPipeOutputStream;
			@in = pipe.NamedPipeInputStream;
			attached = true;
			return new ConnectionOrientedEndpoint(this, syntax);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void close() throws java.io.IOException
		public virtual void close()
		{
			try
			{
				if (pipe != null)
				{
					@in.Close();
					@out.Close();
					in2.Close();
				}
			}
			finally
			{
				attached = false;
				pipe = null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void send(ndr.NdrBuffer buffer) throws java.io.IOException
		public virtual void send(NdrBuffer buffer)
		{
			if (!attached)
			{
				throw new RpcException("Transport not attached.");
			}
			@out.Write(buffer.Buffer, 0, buffer.Length);
			first = true;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void receive(ndr.NdrBuffer buffer) throws java.io.IOException
		public virtual void receive(NdrBuffer buffer)
		{
			var buf = buffer.Buffer;
			int off = 0, bytes_to_read , n ;

			if (!attached)
			{
				throw new RpcException("Transport not attached.");
			}

			if (first)
			{
				n = @in.Read(buf, 0, 1024); // TransactNamedPipe
				first = false;
			}
			else
			{ // Plain read
				n = in2.Read(buf, off, buf.Length);
			}

			buffer.Index = ConnectionOrientedPdu.FRAG_LENGTH_OFFSET;
			bytes_to_read = buffer.dec_ndr_short();

			off += n;
			bytes_to_read -= n;

			while (bytes_to_read > 0)
			{
				n = in2.Read(buf, off, bytes_to_read);
				off += n;
				bytes_to_read -= n;
			}
			buffer.length = off;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void parse(String address) throws rpc.ProviderException
		protected internal virtual void parse(string address)
		{
			if (address == null)
			{
				throw new ProviderException("Null address.");
			}
			if (!address.StartsWith("ncacn_np:", StringComparison.Ordinal))
			{
				throw new ProviderException("Not an ncacn_np address.");
			}
			address = address.Substring(9);
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
			while (address.StartsWith("\\", StringComparison.Ordinal))
			{
				address = address.Substring(1);
			}
			if (!address.regionMatches(true, 0, "PIPE", 0, 4))
			{
				throw new ProviderException("Not a named pipe address.");
			}
			address = address.Substring(4);
			while (address.StartsWith("\\", StringComparison.Ordinal))
			{
				address = address.Substring(1);
			}
			if ("".Equals(address))
			{
				throw new ProviderException("Empty port.");
			}
			while (server.StartsWith("\\", StringComparison.Ordinal))
			{
				server = server.Substring(1);
			}
			if ("".Equals(server))
			{
				server = LOCALHOST;
			}
			var properties = Properties;
			if (properties != null)
			{
				string userInfo = properties.getProperty("rpc.ncacn_np.username");
				if (userInfo == null)
				{
					userInfo = Config.getProperty("jcifs.smb.client.username");
				}
				if (userInfo != null)
				{
					string domain = properties.getProperty("rpc.ncacn_np.domain");
					if (domain == null)
					{
						domain = Config.getProperty("jcifs.smb.client.domain");
					}
					if (domain != null)
					{
						userInfo = domain + ';' + userInfo;
					}
					string password = properties.getProperty("rpc.ncacn_np.password");
					if (password == null)
					{
						password = Config.getProperty("jcifs.smb.client.password");
					}
					if (password != null)
					{
						userInfo += ':' + password;
					}
				}
				if (userInfo != null)
				{
					server = userInfo + '@' + server;
				}
			}
			this.address = "smb://" + server + "/IPC$/" + address;
		}

	}

}