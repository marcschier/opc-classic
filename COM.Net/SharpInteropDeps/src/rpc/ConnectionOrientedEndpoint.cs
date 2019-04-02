using System;
using System.Collections;

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


namespace rpc
{


	using NdrBuffer = ndr.NdrBuffer;
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;
	using PresentationContext = core.PresentationContext;
	using PresentationResult = core.PresentationResult;
	using PresentationSyntax = core.PresentationSyntax;
	using UUID = core.UUID;
	using AlterContextPdu = pdu.AlterContextPdu;
	using AlterContextResponsePdu = pdu.AlterContextResponsePdu;
	using BindAcknowledgePdu = pdu.BindAcknowledgePdu;
	using BindPdu = pdu.BindPdu;
	using FaultCoPdu = pdu.FaultCoPdu;
	using RequestCoPdu = pdu.RequestCoPdu;
	using ResponseCoPdu = pdu.ResponseCoPdu;
	using ShutdownPdu = pdu.ShutdownPdu;

	public class ConnectionOrientedEndpoint : Endpoint
	{
		private readonly bool InstanceFieldsInitialized;

		private void InitializeInstanceFields()
		{
			contextIdToUse = contextIdCounter;
		}


		public const string CONNECTION_CONTEXT = "rpc.connectionContext";

		protected internal ConnectionContext context;

		private readonly Transport transport;

		private readonly PresentationSyntax syntax;

		private bool bound;

		private readonly int callId;

		private int contextIdCounter;

		private int contextIdToUse;

		private static readonly Logger logger = Logger.getLogger("org.jinterop");

		//This is so as to reuse the contextids for already exported contexts.
		private readonly IDictionary uuidsVsContextIds = new Hashtable();

		public ConnectionOrientedEndpoint(Transport transport, PresentationSyntax syntax)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			this.transport = transport;
			this.syntax = syntax;
		}

        public virtual Transport Transport => transport;

        public virtual PresentationSyntax Syntax => syntax;

        //JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
        //ORIGINAL LINE: public void call(int semantics, rpc.core.UUID object, int opnum, ndr.NdrObject ndrobj) throws java.io.IOException
        public virtual void call(int semantics, UUID @object, int opnum, NdrObject ndrobj)
		{
			bind();
            var request = new RequestCoPdu {
                ContextId = contextIdToUse
            };

            var b = new sbyte[1024];
			var buffer = new NdrBuffer(b, 0);
			var ndr = new NetworkDataRepresentation();
			ndrobj.encode(ndr, buffer);
			var stub = new sbyte[buffer.Length]; // yuk
			Array.Copy(buffer.buf, 0, stub, 0, stub.Length);

			if (logger.isLoggable(Level.FINEST))
			{
				//jcifs.util.Hexdump.hexdump(System.err, stub, 0, stub.length);
				   var byteArrayOutputStream = new ByteArrayOutputStream();
				   jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), stub, 0, stub.Length);
				   logger.finest("\n" + byteArrayOutputStream.ToString());
			}



			request.Stub = stub;
			request.AllocationHint = buffer.Length;
			request.Opnum = opnum;
			request.Object = @object;
			if ((semantics & Endpoint_Fields.MAYBE) != 0)
			{
				request.setFlag(ConnectionOrientedPdu.PFC_MAYBE, true);
			}
			send(request);

	//        if (semantics == 100)
	//        try{
	//        	Thread.sleep(100);
	//        }catch(Exception e)
	//        {
	//
	//        }

			if (request.getFlag(ConnectionOrientedPdu.PFC_MAYBE))
			{
				return;
			}
			var reply = receive();
			if (reply is ResponseCoPdu)
			{
				ndr.Format = reply.Format;

				buffer = new NdrBuffer(((ResponseCoPdu) reply).Stub, 0);

				if (logger.isLoggable(Level.FINEST))
				{
					//jcifs.util.Hexdump.hexdump(System.err, buffer.buf, 0, buffer.buf.length);
					   var byteArrayOutputStream = new ByteArrayOutputStream();
					   jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), buffer.buf, 0, buffer.buf.Length);
					   logger.finest("\n" + byteArrayOutputStream.ToString());
				}

				ndrobj.decode(ndr, buffer);

			}
			else if (reply is FaultCoPdu)
			{
				var fault = (FaultCoPdu) reply;
				throw new FaultException("Received fault.", fault.Status, fault.Stub);
			}
			else if (reply is ShutdownPdu)
			{
				throw new RpcException("Received shutdown request from server.");
			}
			else
			{
				throw new RpcException("Received unexpected PDU from server.");
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void rebind() throws java.io.IOException
		protected internal virtual void rebind()
		{
			bound = false;
			bind();
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void bind() throws java.io.IOException
		protected internal virtual void bind()
		{
			if (bound)
			{
				return;
			}
			if (context != null)
			{
				bound = true;
				try
				{
					var cid = (int?)uuidsVsContextIds[Syntax.ToString().ToUpper()];
					var pdu = context.alter(new PresentationContext(cid == null ?++contextIdCounter : (int)cid, Syntax));
					var sendAlter = false;
					if (cid == null)
					{
						uuidsVsContextIds[Syntax.ToString().ToUpper()] = contextIdCounter;
						contextIdToUse = contextIdCounter;
						sendAlter = true;
					}
					else
					{
						contextIdToUse = (int)cid;
					}

					if (sendAlter)
					{
						if (pdu != null)
						{
							send(pdu);
						}
						while (!context.Established)
						{
							var recieved = receive();
							if ((pdu = context.accept(recieved)) != null)
							{
								switch (pdu.Type)
								{
									case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
										if (((BindAcknowledgePdu)pdu).ResultList[0].reason != PresentationResult.PROVIDER_REJECTION)
										{
											currentIID = ((BindPdu)recieved).ContextList[0].abstractSyntax.Uuid.ToString();
										}
										break;
									case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
										//we need to record the iid now if this is successful and subsequent calls will now be for this iid.
										if (((AlterContextResponsePdu)pdu).ResultList[0].reason != PresentationResult.PROVIDER_REJECTION)
										{
											currentIID = ((AlterContextPdu)recieved).ContextList[0].abstractSyntax.Uuid.ToString();
										}
										break;
									default:
										//nothing
								break;
								}
								send(pdu);
							}
						}
					}
				}
				catch (IOException ex)
				{
					bound = false;
					throw ex;
				}
				catch (Exception ex)
				{
					bound = false;
					throw ex;
				}
				catch (Exception ex)
				{
					bound = false;
					throw new IOException(ex.Message);
				}
			}
			else
			{
				connect();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void send(ConnectionOrientedPdu request) throws java.io.IOException
		protected internal virtual void send(ConnectionOrientedPdu request)
		{
			bind();
			context.Connection.transmit(request, Transport);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected ConnectionOrientedPdu receive() throws java.io.IOException
		protected internal virtual ConnectionOrientedPdu receive()
		{
			return context.Connection.receive(Transport);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void detach() throws java.io.IOException
		public virtual void detach()
		{
			bound = false;
			context = null;
			Transport.close();
		}

		protected internal string currentIID;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void connect() throws java.io.IOException
		private void connect()
		{
			bound = true;
			contextIdCounter = 0;
			currentIID = null;
			try
			{
				uuidsVsContextIds[Syntax.ToString().ToUpper()] = contextIdCounter;
				context = createContext();
				var pdu = context.init(new PresentationContext(contextIdCounter, Syntax), Transport.Properties);
				contextIdToUse = contextIdCounter;
				if (pdu != null)
				{
					send(pdu);
				}
				while (!context.Established)
				{
					var recieved = receive();
					if ((pdu = context.accept(recieved)) != null)
					{
						switch (pdu.Type)
						{
							case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
								if (((BindAcknowledgePdu)pdu).ResultList[0].reason != PresentationResult.PROVIDER_REJECTION)
								{
									currentIID = ((BindPdu)recieved).ContextList[0].abstractSyntax.Uuid.ToString();
								}
								break;
							case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
								//we need to record the iid now if this is successful and subsequent calls will now be for this iid.
								if (((AlterContextResponsePdu)pdu).ResultList[0].reason != PresentationResult.PROVIDER_REJECTION)
								{
									currentIID = ((AlterContextPdu)recieved).ContextList[0].abstractSyntax.Uuid.ToString();
								}
								break;
							default:
								//nothing
						break;
						}
						send(pdu);
					}
				}
			}
			catch (IOException ex)
			{
				try
				{
					detach();
				}
				catch (IOException)
				{
				}
				throw ex;
			}
			catch (Exception ex)
			{
				try
				{
					detach();
				}
				catch (IOException)
				{
				}
				throw ex;
			}
			catch (Exception ex)
			{
				try
				{
					detach();
				}
				catch (IOException)
				{
				}
				throw new IOException(ex.Message);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected ConnectionContext createContext() throws ProviderException
		protected internal virtual ConnectionContext createContext()
		{
			var properties = Transport.Properties;
			if (properties == null)
			{
				return new BasicConnectionContext();
			}
			string context = properties.getProperty(CONNECTION_CONTEXT);
			if (context == null)
			{
				return new BasicConnectionContext();
			}
			try
			{
				return (ConnectionContext) Type.GetType(context).newInstance();
			}
			catch (Exception ex)
			{
				throw new ProviderException(ex.Message);
			}
		}

	}

}