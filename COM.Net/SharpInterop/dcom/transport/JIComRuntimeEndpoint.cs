//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//


namespace org.jinterop.dcom.transport {
    using org.jinterop.dcom.common;
    using rpc;
    using rpc.core;
    using rpc.pdu;
    using Serilog;
    using SharpCifs.Dcerpc.Ndr;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    /// <summary>
    /// Endpoint
    /// </summary>
    public sealed class JIComRuntimeEndpoint : ConnectionOrientedEndpoint {

        /// <summary>
        /// Create endpoint
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="syntax"></param>
        internal JIComRuntimeEndpoint(ITransport transport, PresentationSyntax syntax) :
            base(transport, syntax) {
        }

        /// <inheritdoc/>
        public override void Call(Semantics semantics, UUID @object, int opnum, NdrOp ndrobj) =>
            throw new JIRuntimeException((int)JIErrorCodes.JI_ILLEGAL_CALL);

        /// <summary>
        /// Process requests on endpoint
        /// </summary>
        /// <param name="workerObject"></param>
        /// <param name="baseIID"></param>
        /// <param name="listOfSupportedInterfaces"></param>
        /// <param name="cancellationToken"></param>
        public void ProcessRequests(IJICOMRuntimeWorker workerObject, string baseIID,
            List<object> listOfSupportedInterfaces, CancellationToken cancellationToken) {

            Log.Logger.Information("processRequests: [JIComRuntimeEndPoint] started new thread " +
                Thread.CurrentThread.Name);
            //this iid is the component IID just in case.
            if (baseIID != null) {
                Transport.Properties.SetProperty("IID2", baseIID);
            }
            Transport.Properties.SetProperty("LISTOFSUPPORTEDINTERFACES", listOfSupportedInterfaces);
            Bind(); // will bind to the server and perform the initial bind\bind ack.

            while (!cancellationToken.IsCancellationRequested) {
                // first recieve and then answer
                ConnectionOrientedPdu response = null;
                var request = Receive();
                Log.Logger.Information("processRequests: [JIComRuntimeEndPoint] request : " +
                    Thread.CurrentThread.Name + " , " + request + " workerObject is resolver: " +
                    workerObject.Resolver);
                NdrBuffer buffer = null;
                var ndr = new NdrCodec();
                workerObject.CurrentIID = CurrentIID;
                if (request is RequestCoPdu) {
                    buffer = new NdrBuffer(((RequestCoPdu)request).Stub, 0);
                    if (buffer.Buf != null) {
                        var byteArrayOutputStream = Utils.HexString(buffer.Buf, 0, buffer.Buf.Length);
                        Log.Logger.Verbose("\n" + byteArrayOutputStream.ToString());
                    }
                    ndr.Format = ((RequestCoPdu)request).Format;
                    workerObject.Opnum = ((RequestCoPdu)request).Opnum;
                    //sets the current object, this is used to identify the JILocalCoClass to work on.
                    //for most cases this will be null , till there is an actual COM interface request.
                    workerObject.CurrentObjectID = ((RequestCoPdu)request).Object;

                    try {
                        ((NdrOp)workerObject).Decode(ndr, buffer);
                        var responseCoPdu = new ResponseCoPdu {
                            ContextId = ((RequestCoPdu)request).ContextId,
                            Format = ((RequestCoPdu)request).Format,
                            CallId = ((RequestCoPdu)request).CallId
                        };
                        ((NdrOp)workerObject).Encode(ndr, null);
                        var length = ndr.Buffer.Length > ndr.Buffer.Index ? ndr.Buffer.Length : ndr.Buffer.Index;
                        //					  length = length + 4;
                        responseCoPdu.AllocationHint = length + 4;
                        var responsebytes = new byte[length + 4];
                        Array.Copy(ndr.Buffer.Buf, 0, responsebytes, 0, responsebytes.Length - 4);
                        responseCoPdu.Stub = responsebytes;
                        // responseCoPdu.setStub(ndr.getBuffer().getBuffer());
                        response = responseCoPdu;
                    }
                    catch (JIRuntimeException e) {
                        Log.Logger.Error(e, "JIComRuntimeEndpoint", "processRequests", e);
                        //create a fault PDU
                        response = new FaultCoPdu {
                            CallId = ((RequestCoPdu)request).CallId
                        };
                        ((FaultCoPdu)response).Status = (FaultCode)e.HResult;
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
                        if (Context is JIComRuntimeNTLMConnectionContext) {
                            ((JIComRuntimeNTLMConnectionContext)Context).UpdateListOfInterfacesSupported(
                                workerObject.QIedIIDs);
                        }
                        switch (request.Type) {
                            case BindPdu.BIND_TYPE:
                                CurrentIID = ((BindPdu)request).ContextList[0].AbstractSyntax.Uuid.ToString();
                                break;
                            case AlterContextPdu.ALTER_CONTEXT_TYPE:
                                //we need to record the iid now if this is successful and subsequent calls will now be for this iid.
                                CurrentIID = ((AlterContextPdu)request).ContextList[0].AbstractSyntax.Uuid.ToString();
                                break;
                            default:
                                //nothing
                                break;
                        }
                    }

                    response = Context.Accept(request);
                    if (!workerObject.Resolver) {
                        PresentationResult[] result = null;
                        PresentationContext context = null;
                        var successful = false;
                        if (response is BindAcknowledgePdu) {
                            result = ((BindAcknowledgePdu)response).ResultList;
                            successful = result[0].Result == PresentationResultCode.ACCEPTANCE;
                            context = ((BindPdu)request).ContextList[0]; //am expecting only one
                        }
                        else {
                            result = ((AlterContextResponsePdu)response).ResultList;
                            successful = result[0].Result == PresentationResultCode.ACCEPTANCE;
                            context = ((AlterContextPdu)request).ContextList[0]; //am expecting only one
                        }
                    }
                }
                else if (request is FaultCoPdu fault) {
                    // TODO to throw or not to throw ...that is the question :)...i think it should be logged , but not thrown
                    // otherwise this thread will be terminated and further access will be blocked for the com server.
                    // TODO write logging code here and comment this code.
                    throw new FaultException("Received fault.", fault.Status, fault.Stub);
                }
                else if (request is ShutdownPdu) {
                    throw new RpcException("Received shutdown request from server.");
                }
                else if (request is Auth3Pdu) {
                    continue; //don't do anything here, the server will send another request
                }
                Log.Logger.Information("processRequests: [JIComRuntimeEndPoint] response : " +
                    Thread.CurrentThread.Name + ", " + response);
                //now send the response.
                Send(response);
                if (workerObject.WorkerOver()) {
                    Log.Logger.Information("processRequests: [JIComRuntimeEndPoint] Worker is over," +
                        " all IPID references have been released. Thread " +
                        Thread.CurrentThread.Name + " will now exit.");
                    break;
                }
            }
        }
    }
}