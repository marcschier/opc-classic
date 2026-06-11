// SPDX-License-Identifier: MIT

using Opc.Classic.Dcom.Common;
using Opc.Classic.Dcom.Rpc;
using Opc.Classic.Dcom.Rpc.Core;
using Opc.Classic.Dcom.Rpc.pdu;
using Opc.Classic.Dcom.Internal;
using Opc.Classic.Dcom.Internal.LegacyNdr;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Opc.Classic.Dcom.Transport;

/// <summary>
/// Endpoint
/// </summary>
public sealed class ComRuntimeEndpoint : ConnectionOrientedEndpoint
{

    /// <summary>
    /// Create endpoint
    /// </summary>
    /// <param name="transport">Underlying RPC transport handle, such as a TCP socket or SMB named pipe.</param>
    /// <param name="syntax">Presentation syntax negotiated for the RPC context.</param>
    internal ComRuntimeEndpoint(ITransport transport, PresentationSyntax syntax) :
        base(transport, syntax)
    {
    }

    /// <inheritdoc/>
    public override void Call(Semantics semantics, UUID objectId, int opnum, NdrOp ndrobj) =>
        throw new InteropRuntimeException((int)ErrorCode.INTEROP_ILLEGAL_CALL);

    /// <summary>
    /// Process requests on endpoint
    /// </summary>
    /// <param name="workerObject">COM runtime worker that dispatches incoming RPC requests.</param>
    /// <param name="baseIID">Base interface IID used to derive the requested COM interface metadata.</param>
    /// <param name="listOfSupportedInterfaces">Interfaces supported by the COM object or runtime endpoint.</param>
    /// <param name="cancellationToken">Token used to cancel the asynchronous operation.</param>
#pragma warning disable MA0051 // Legacy request loop is deliberately kept as one state machine.
    public void ProcessRequests(IComRuntimeWorker workerObject, string baseIID,
        IReadOnlyList<string> listOfSupportedInterfaces, CancellationToken cancellationToken)
    {

        Log.Logger.Information("processRequests: [ComRuntimeEndPoint] started new thread " +
            Thread.CurrentThread.Name);

        // this iid is the component IID just in case.
        if (baseIID != null)
        {
            Transport.Properties.SetProperty("IID2", baseIID); // TODO - find another way...
        }

        // TODO - find another way...
        Transport.Properties.SetProperty("LISTOFSUPPORTEDINTERFACES", listOfSupportedInterfaces);
        Bind(); // will bind to the server and perform the initial bind\bind ack.

        while (!cancellationToken.IsCancellationRequested)
        {
            // first recieve and then answer
            ConnectionOrientedPdu response = null;
            var request = Receive();
            Log.Logger.Information("processRequests: [ComRuntimeEndPoint] request : " +
                Thread.CurrentThread.Name + ", " + request + " workerObject is resolver: " +
                workerObject.Resolver);
            var ndr = new NdrCodec();
            workerObject.CurrentIID = CurrentIID;
            if (request is RequestCoPdu requestPdu)
            {
                var buffer = new NdrBuffer(requestPdu.Stub, 0);
                if (buffer.Buf != null)
                {
                    var byteArrayOutputStream = Utils.HexString(buffer.Buf, 0, buffer.Buf.Length);
                    Log.Logger.Verbose("\n" + byteArrayOutputStream.ToString());
                }
                ndr.Format = requestPdu.Format;
                workerObject.Opnum = requestPdu.Opnum;
                // sets the current object, this is used to identify the <see cref="LocalCoClass"/> to work on.
                // for most cases this will be null, till there is an actual COM interface request.
                workerObject.CurrentObjectID = ((RequestCoPdu)request).Object;

                try
                {
                    ((NdrOp)workerObject).Decode(ndr, buffer);
                    var responseCoPdu = new ResponseCoPdu
                    {
                        ContextId = ((RequestCoPdu)request).ContextId,
                        Format = ((RequestCoPdu)request).Format,
                        CallId = ((RequestCoPdu)request).CallId
                    };
                    ((NdrOp)workerObject).Encode(ndr, null);
                    var length = ndr.Buffer.Length > ndr.Buffer.Index ? ndr.Buffer.Length : ndr.Buffer.Index;
                    //                      length = length + 4;
                    responseCoPdu.AllocationHint = length + 4;
                    var responsebytes = new byte[length + 4];
                    Array.Copy(ndr.Buffer.Buf, 0, responsebytes, 0, responsebytes.Length - 4);
                    responseCoPdu.Stub = responsebytes;
                    // responseCoPdu.setStub(ndr.getBuffer().getBuffer());
                    response = responseCoPdu;
                }
                catch (InteropRuntimeException e)
                {
                    Log.Logger.Error(e, "ComRuntimeEndpoint processRequests");
                    // create a fault PDU
                    response = new FaultCoPdu
                    {
                        CallId = ((RequestCoPdu)request).CallId
                    };
                    ((FaultCoPdu)response).Status = (FaultCode)e.HResult;
                }
            }
            else if (request is BindPdu || request is AlterContextPdu)
            {
                if (!workerObject.Resolver)
                {
                    // this list will be clear after this call.
                    /* Basically the cycle expected is like this...first a bind call comes, then a RemQI, that populates the
                     * list internally (Remunknownobject), then an alter context comes for the QIed interface, this clears the set
                     * object (if any), then a normal request comes through.
                     *
                     */
                    // this call is only valid when the workerObject is RemUnknownObject.
                    // so the context us NTLMConnectionContext
                    if (Context is ComRuntimeNtlmConnectionContext ntlmContext)
                    {
                        ntlmContext.UpdateListOfInterfacesSupported(
                            workerObject.QIedIIDs);
                    }
                    switch (request.Type)
                    {
                        case BindPdu.BIND_TYPE:
                            CurrentIID = ((BindPdu)request).ContextList[0].AbstractSyntax.Uuid.ToString();
                            break;
                        case AlterContextPdu.ALTER_CONTEXT_TYPE:
                            // we need to record the iid now if this is successful and subsequent calls will now be for this iid.
                            CurrentIID = ((AlterContextPdu)request).ContextList[0].AbstractSyntax.Uuid.ToString();
                            break;
                        default:
                            // nothing
                            break;
                    }
                }

                response = Context.Accept(request);
                if (!workerObject.Resolver)
                {
                    PresentationResult[] result;

                    PresentationContext context;

                    bool successful;
                    if (response is BindAcknowledgePdu bindAck)
                    {
                        result = bindAck.ResultList;
                        successful = result[0].Result == PresentationResultCode.ACCEPTANCE;
                        context = ((BindPdu)request).ContextList[0]; // am expecting only one
                    }
                    else
                    {
                        result = ((AlterContextResponsePdu)response).ResultList;
                        successful = result[0].Result == PresentationResultCode.ACCEPTANCE;
                        context = ((AlterContextPdu)request).ContextList[0]; // am expecting only one
                    }
                }
            }
            else if (request is FaultCoPdu fault)
            {
                // TODO to throw or not to throw ...that is the question :)...i think it should be logged, but not thrown
                // otherwise this thread will be terminated and further access will be blocked for the com server.
                // TODO write logging code here and comment this code.
                throw new FaultException("Received fault.", fault.Status, fault.Stub);
            }
            else if (request is ShutdownPdu)
            {
                throw new RpcException("Received shutdown request from server.");
            }
            else if (request is Auth3Pdu)
            {
                continue; // don't do anything here, the server will send another request
            }
            Log.Logger.Information("processRequests: [ComRuntimeEndPoint] response : " +
                Thread.CurrentThread.Name + ", " + response);
            // now send the response.
            Send(response);
            if (workerObject.WorkerOver())
            {
                Log.Logger.Information("processRequests: [ComRuntimeEndPoint] Worker is over," +
                    " all IPID references have been released. Thread " +
                    Thread.CurrentThread.Name + " will now exit.");
                break;
            }
        }
    }
#pragma warning restore MA0051
}
