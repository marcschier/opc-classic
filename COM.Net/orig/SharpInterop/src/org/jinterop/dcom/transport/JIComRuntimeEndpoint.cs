using System;
using System.Collections;
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
	using NdrObject = ndr.NdrObject;
	using NetworkDataRepresentation = ndr.NetworkDataRepresentation;

	using IJICOMRuntimeWorker = org.jinterop.dcom.common.IJICOMRuntimeWorker;
	using JIErrorCodes = org.jinterop.dcom.common.JIErrorCodes;
	using JIRuntimeException = org.jinterop.dcom.common.JIRuntimeException;
	using JISystem = org.jinterop.dcom.common.JISystem;

	using ConnectionOrientedEndpoint = rpc.ConnectionOrientedEndpoint;
	using ConnectionOrientedPdu = rpc.ConnectionOrientedPdu;
	using FaultException = rpc.FaultException;
	using RpcException = rpc.RpcException;
	using Transport = rpc.Transport;
	using PresentationContext = rpc.core.PresentationContext;
	using PresentationResult = rpc.core.PresentationResult;
	using PresentationSyntax = rpc.core.PresentationSyntax;
	using UUID = rpc.core.UUID;
	using AlterContextPdu = rpc.pdu.AlterContextPdu;
	using AlterContextResponsePdu = rpc.pdu.AlterContextResponsePdu;
	using Auth3Pdu = rpc.pdu.Auth3Pdu;
	using BindAcknowledgePdu = rpc.pdu.BindAcknowledgePdu;
	using BindPdu = rpc.pdu.BindPdu;
	using FaultCoPdu = rpc.pdu.FaultCoPdu;
	using RequestCoPdu = rpc.pdu.RequestCoPdu;
	using ResponseCoPdu = rpc.pdu.ResponseCoPdu;
	using ShutdownPdu = rpc.pdu.ShutdownPdu;

	/// <summary>
	/// @exclude
	/// @since 1.0
	/// 
	/// </summary>
	public sealed class JIComRuntimeEndpoint : ConnectionOrientedEndpoint {

		public JIComRuntimeEndpoint(Transport transport, PresentationSyntax syntax) : base(transport,syntax) {
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void call(int semantics, rpc.core.UUID object, int opnum, ndr.NdrObject ndrobj) throws java.io.IOException
		public void Call(int semantics, UUID @object, int opnum, NdrObject ndrobj) {
			throw new JIRuntimeException(JIErrorCodes.JI_ILLEGAL_CALL);
		}

		//use this oxidObject, it is actually OxidResolverImpl extends NdrObject.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public void processRequests(org.jinterop.dcom.common.IJICOMRuntimeWorker workerObject, String baseIID, java.util.List listOfSupportedInterfaces) throws java.io.IOException
		public void ProcessRequests(IJICOMRuntimeWorker workerObject, string baseIID, IList listOfSupportedInterfaces) {

			if (JISystem.Logger.isLoggable(Level.INFO)) {
				JISystem.Logger.info("processRequests: [JIComRuntimeEndPoint] started new thread " + Thread.CurrentThread.Name);
			}
			//this iid is the component IID just in case.
			if (baseIID != null) {
				Transport.Properties.setProperty("IID2", baseIID);
			}

			Transport.Properties.put("LISTOFSUPPORTEDINTERFACES",listOfSupportedInterfaces);

			bind(); // will bind to the server and perform the initial bind\bind ack.

			while (true) {

				  // first recieve and then answer
				  ConnectionOrientedPdu response = null;
				  ConnectionOrientedPdu request = receive();

				  if (!workerObject.Resolver) {
					  int j = 0;
				  }
				  if (JISystem.Logger.isLoggable(Level.INFO)) {
					  JISystem.Logger.info("processRequests: [JIComRuntimeEndPoint] request : " + Thread.CurrentThread.Name + " , " + request + " workerObject is resolver: " + workerObject.Resolver);
				  }
				  NdrBuffer buffer = null;
				  NetworkDataRepresentation ndr = new NetworkDataRepresentation();
				  workerObject.CurrentIID = currentIID;
				  if (request is RequestCoPdu) {
					  buffer = new NdrBuffer(((RequestCoPdu) request).Stub, 0);
					  if (buffer.buf != null) {
						if (JISystem.Logger.isLoggable(Level.FINEST)) {
							ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();
							jcifs.util.Hexdump.hexdump(new PrintStream(byteArrayOutputStream), buffer.buf, 0, buffer.buf.length);
							JISystem.Logger.finest("\n" + byteArrayOutputStream.ToString());
						}
						 // System.err.println("Vikram: " + Long.toString(Thread.currentThread().getId()));
						 // jcifs.util.Hexdump.hexdump(System.err, buffer.buf, 0, buffer.buf.length);
					  }
					  ndr.Format = ((RequestCoPdu) request).Format;
					  workerObject.Opnum = ((RequestCoPdu) request).Opnum;
					  //sets the current object, this is used to identify the JILocalCoClass to work on.
					  //for most cases this will be null , till there is an actual COM interface request.
					  workerObject.CurrentObjectID = ((RequestCoPdu) request).Object;

					  try {

						  ((NdrObject)workerObject).decode(ndr, buffer);
						  ResponseCoPdu responseCoPdu = new ResponseCoPdu();
						  responseCoPdu.ContextId = ((RequestCoPdu) request).ContextId;
						  responseCoPdu.Format = ((RequestCoPdu) request).Format;
						  responseCoPdu.CallId = ((RequestCoPdu) request).CallId;
						  ((NdrObject)workerObject).encode(ndr,null);
						  int length = ndr.Buffer.length > ndr.Buffer.index ? ndr.Buffer.length : ndr.Buffer.index;
	//					  length = length + 4;
						  responseCoPdu.AllocationHint = length + 4;
						  sbyte[] responsebytes = new sbyte[length + 4];
						  Array.Copy(ndr.Buffer.Buffer, 0, responsebytes, 0, responsebytes.Length - 4);
						  responseCoPdu.Stub = responsebytes;
	//					  responseCoPdu.setStub(ndr.getBuffer().getBuffer());
						  response = responseCoPdu;



					  }
					  catch (JIRuntimeException e) {
						  JISystem.Logger.throwing("JIComRuntimeEndpoint","processRequests",e);
						  //create a fault PDU
						  response = new FaultCoPdu();
						  response.CallId = ((RequestCoPdu) request).CallId;
						  ((FaultCoPdu)response).Status = e.HResult;
					  }
				  }
				  else if (request is BindPdu || request is AlterContextPdu) {

					  if (!workerObject.Resolver) {
						  //this list will be clear after this call.
						  /* Basically the cycle expected is like this...first a bind call comes, then a RemQI, that populates the
						   * list internally (Remunknownobject), then an alter context comes for the QIed interface, this clears the set
						   * object (if any) , then a normal request comes through.
						   *
						   */
						  //this call is only valid when the workerObject is RemUnknownObject.
						  //so the context us NTLMConnectionContext
						  if (context is JIComRuntimeNTLMConnectionContext) {
							  ((JIComRuntimeNTLMConnectionContext)context).UpdateListOfInterfacesSupported(workerObject.QIedIIDs);
						  }


							switch (request.Type) {
								  case BindPdu.BIND_TYPE:
										  currentIID = ((BindPdu)request).ContextList[0].abstractSyntax.Uuid.ToString();
									  break;
								  case AlterContextPdu.ALTER_CONTEXT_TYPE:
										  //we need to record the iid now if this is successful and subsequent calls will now be for this iid.
										  currentIID = ((AlterContextPdu)request).ContextList[0].abstractSyntax.Uuid.ToString();
									  break;
								  default:
									  //nothing
							  break;
							}

					  }

					  response = context.accept(request);

					  if (!workerObject.Resolver) {
						  PresentationResult[] result = null;
						  PresentationContext context = null;
						  bool successful = false;
						  if (response is BindAcknowledgePdu) {
							  result = ((BindAcknowledgePdu)response).ResultList;
							  successful = result[0].result == PresentationResult.ACCEPTANCE;
							  context = ((BindPdu)request).ContextList[0]; //am expecting only one
						  }
						  else {
							  result = ((AlterContextResponsePdu)response).ResultList;
							  successful = result[0].result == PresentationResult.ACCEPTANCE;
							  context = ((AlterContextPdu)request).ContextList[0]; //am expecting only one
						  }

	//					  if (successful)
	//					  {
	//						  //now select the Interface from the request and set that as the object expected to come.
	//						  workerObject.setCurrentJavaInstanceFromIID(context.abstractSyntax.toString().toUpperCase());
	//						  //set the component null;
	//					  }

					  }
				  }
				  else if (request is FaultCoPdu) {
					   // TODO to throw or not to throw ...that is the question :)...i think it should be logged , but not thrown
					   // otherwise this thread will be terminated and further access will be blocked for the com server.
					   // TODO write logging code here and comment this code.
						FaultCoPdu fault = (FaultCoPdu) request;
						throw new FaultException("Received fault.", fault.Status, fault.Stub);
				  }
				  else if (request is ShutdownPdu) {
						throw new RpcException("Received shutdown request from server.");
				  }
				  else if (request is Auth3Pdu) {
	//				  try {
	//					Thread.sleep(1000);
	//				} catch (InterruptedException e) {
	//					// TODO Auto-generated catch block
	//					e.printStackTrace();
	//				}
						continue; //don't do anything here, the server will send another request
				  }
				  if (JISystem.Logger.isLoggable(Level.INFO)) {
					  JISystem.Logger.info("processRequests: [JIComRuntimeEndPoint] response : " + Thread.CurrentThread.Name + " , " + response);
				  }
				  //now send the response.
				  send(response);

				  if (workerObject.WorkerOver()) {
					  JISystem.Logger.info("processRequests: [JIComRuntimeEndPoint] Worker is over, all IPID references have been released. Thread " + Thread.CurrentThread.Name + " will now exit.");
					  break;
				  }
			}

		}
	}

}