using System;
using System.Collections;

/// <summary>
/// Donated by Jarapac (http://jarapac.sourceforge.net/) and released under EPL.
/// 
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
/// Vikram Roopchand  - Moving to EPL from LGPL v1.
/// 
/// </summary>


namespace rpc {


	using NdrBuffer = ndr.NdrBuffer;
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;
	using PresentationContext = rpc.core.PresentationContext;
	using PresentationResult = rpc.core.PresentationResult;
	using PresentationSyntax = rpc.core.PresentationSyntax;
	using UUID = rpc.core.UUID;
	using AlterContextPdu = rpc.pdu.AlterContextPdu;
	using AlterContextResponsePdu = rpc.pdu.AlterContextResponsePdu;
	using BindAcknowledgePdu = rpc.pdu.BindAcknowledgePdu;
	using BindPdu = rpc.pdu.BindPdu;
	using FaultCoPdu = rpc.pdu.FaultCoPdu;
	using RequestCoPdu = rpc.pdu.RequestCoPdu;
	using ResponseCoPdu = rpc.pdu.ResponseCoPdu;
	using ShutdownPdu = rpc.pdu.ShutdownPdu;

	public class ConnectionOrientedEndpoint : Endpoint {
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields() {
			ContextIdToUse = ContextIdCounter;
		}


		public const string CONNECTION_CONTEXT = "rpc.connectionContext";

		protected internal ConnectionContext Context;

		private Transport Transport_Renamed;

		private PresentationSyntax Syntax_Renamed;

		private bool Bound;

		private int CallId;

		private int ContextIdCounter = 0;

		private int ContextIdToUse;

		private static readonly Logger Logger = Logger.getLogger("org.jinterop");

		//This is so as to reuse the contextids for already exported contexts.
		private IDictionary UuidsVsContextIds = new Hashtable();

		public ConnectionOrientedEndpoint(Transport transport, PresentationSyntax syntax) {
			if (!InstanceFieldsInitialized) {
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			this.Transport_Renamed = transport;
			this.Syntax_Renamed = syntax;
		}

		public virtual Transport Transport {
			get {
				return Transport_Renamed;
			}
		}

		public virtual PresentationSyntax Syntax {
			get {
				return Syntax_Renamed;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void call(int semantics, rpc.core.UUID object, int opnum, ndr.NdrObject ndrobj) throws java.io.IOException
		public virtual void Call(int semantics, UUID @object, int opnum, NdrObject ndrobj) {
			Bind();
			RequestCoPdu request = new RequestCoPdu();
			request.ContextId = ContextIdToUse;

			sbyte[] b = new sbyte[1024];
			NdrBuffer buffer = new NdrBuffer(b, 0);
			NetworkDataRepresentation ndr = new NetworkDataRepresentation();
			ndrobj.Encode(ndr, buffer);
			sbyte[] stub = new sbyte[buffer.Length]; // yuk
			Array.Copy(buffer.Buf, 0, stub, 0, stub.Length);

			if (Logger.isLoggable(Level.FINEST)) {
				//jcifs.util.Hexdump.hexdump(System.err, stub, 0, stub.length);
				   ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
				   jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), stub, 0, stub.Length);
				   Logger.finest("\n" + byteArrayOutputStream.ToString());
			}



			request.Stub = stub;
			request.AllocationHint = buffer.Length;
			request.Opnum = opnum;
			request.Object = @object;
			if ((semantics & Endpoint_Fields.MAYBE) != 0) {
				request.SetFlag(ConnectionOrientedPdu.PFC_MAYBE, true);
			}
			Send(request);

	//        if (semantics == 100)
	//        try{
	//        	Thread.sleep(100);
	//        }catch(Exception e)
	//        {
	//
	//        }

			if (request.GetFlag(ConnectionOrientedPdu.PFC_MAYBE)) {
				return;
			}
			ConnectionOrientedPdu reply = Receive();
			if (reply is ResponseCoPdu) {
				ndr.Format = reply.Format;

				buffer = new NdrBuffer(((ResponseCoPdu) reply).Stub, 0);

				if (Logger.isLoggable(Level.FINEST)) {
					//jcifs.util.Hexdump.hexdump(System.err, buffer.buf, 0, buffer.buf.length);
					   ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
					   jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), buffer.Buf, 0, buffer.Buf.Length);
					   Logger.finest("\n" + byteArrayOutputStream.ToString());
				}

				ndrobj.Decode(ndr, buffer);

			}
			else if (reply is FaultCoPdu) {
				FaultCoPdu fault = (FaultCoPdu) reply;
				throw new FaultException("Received fault.", fault.Status, fault.Stub);
			}
			else if (reply is ShutdownPdu) {
				throw new RpcException("Received shutdown request from server.");
			}
			else {
				throw new RpcException("Received unexpected PDU from server.");
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void rebind() throws java.io.IOException
		public virtual void Rebind() {
			Bound = false;
			Bind();
		}


//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void bind() throws java.io.IOException
		public virtual void Bind() {
			if (Bound) {
				return;
			}
			if (Context != null) {
				Bound = true;
				try {
					int? cid = (int?)UuidsVsContextIds.GetValueOrNull(Syntax.ToString().ToUpper());
					ConnectionOrientedPdu pdu = Context.Alter(new PresentationContext(cid == null ?++ContextIdCounter : (int)cid, Syntax));
					bool sendAlter = false;
					if (cid == null) {
						UuidsVsContextIds[Syntax.ToString().ToUpper()] = new int?(ContextIdCounter);
						ContextIdToUse = ContextIdCounter;
						sendAlter = true;
					}
					else {
						ContextIdToUse = (int)cid;
					}

					if (sendAlter) {
						if (pdu != null) {
							Send(pdu);
						}
						while (!Context.Established) {
							ConnectionOrientedPdu recieved = Receive();
							if ((pdu = Context.Accept(recieved)) != null) {
								switch (pdu.Type) {
									case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
										if (((BindAcknowledgePdu)pdu).ResultList[0].reason != PresentationResult.PROVIDER_REJECTION) {
											CurrentIID = ((BindPdu)recieved).ContextList[0].abstractSyntax.Uuid.ToString();
										}
										break;
									case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
										//we need to record the iid now if this is successful and subsequent calls will now be for this iid.
										if (((AlterContextResponsePdu)pdu).ResultList[0].reason != PresentationResult.PROVIDER_REJECTION) {
											CurrentIID = ((AlterContextPdu)recieved).ContextList[0].abstractSyntax.Uuid.ToString();
										}
										break;
									default:
										//nothing
								break;
								}
								Send(pdu);
							}
						}
					}
				}
				catch (IOException ex) {
					Bound = false;
					throw ex;
				}
				catch (Exception ex) {
					Bound = false;
					throw ex;
				}
				catch (Exception ex) {
					Bound = false;
					throw new IOException(ex.Message);
				}
			}
			else {
				Connect();
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void send(ConnectionOrientedPdu request) throws java.io.IOException
		public virtual void Send(ConnectionOrientedPdu request) {
			Bind();
			Context.Connection.Transmit(request, Transport);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected ConnectionOrientedPdu receive() throws java.io.IOException
		public virtual ConnectionOrientedPdu Receive() {
			return Context.Connection.Receive(Transport);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void detach() throws java.io.IOException
		public virtual void Detach() {
			Bound = false;
			Context = null;
			Transport.Close();
		}

		protected internal string CurrentIID = null;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void connect() throws java.io.IOException
		private void Connect() {
			Bound = true;
			ContextIdCounter = 0;
			CurrentIID = null;
			try {
				UuidsVsContextIds[Syntax.ToString().ToUpper()] = new int?(ContextIdCounter);
				Context = CreateContext();
				ConnectionOrientedPdu pdu = Context.Init(new PresentationContext(ContextIdCounter, Syntax), Transport.Properties);
				ContextIdToUse = ContextIdCounter;
				if (pdu != null) {
					Send(pdu);
				}
				while (!Context.Established) {
					ConnectionOrientedPdu recieved = Receive();
					if ((pdu = Context.Accept(recieved)) != null) {
						switch (pdu.Type) {
							case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
								if (((BindAcknowledgePdu)pdu).ResultList[0].reason != PresentationResult.PROVIDER_REJECTION) {
									CurrentIID = ((BindPdu)recieved).ContextList[0].abstractSyntax.Uuid.ToString();
								}
								break;
							case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
								//we need to record the iid now if this is successful and subsequent calls will now be for this iid.
								if (((AlterContextResponsePdu)pdu).ResultList[0].reason != PresentationResult.PROVIDER_REJECTION) {
									CurrentIID = ((AlterContextPdu)recieved).ContextList[0].abstractSyntax.Uuid.ToString();
								}
								break;
							default:
								//nothing
						break;
						}
						Send(pdu);
					}
				}
			}
			catch (IOException ex) {
				try {
					Detach();
				}
				catch (IOException) {
				}
				throw ex;
			}
			catch (Exception ex) {
				try {
					Detach();
				}
				catch (IOException) {
				}
				throw ex;
			}
			catch (Exception ex) {
				try {
					Detach();
				}
				catch (IOException) {
				}
				throw new IOException(ex.Message);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected ConnectionContext createContext() throws ProviderException
		public virtual ConnectionContext CreateContext() {
			Properties properties = Transport.Properties;
			if (properties == null) {
				return new BasicConnectionContext();
			}
			string context = properties.getProperty(CONNECTION_CONTEXT);
			if (context == null) {
				return new BasicConnectionContext();
			}
			try {
				return (ConnectionContext) Type.GetType(context).newInstance();
			}
			catch (Exception ex) {
				throw new ProviderException(ex.Message);
			}
		}

	}

}