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

namespace rpc {
    using System;
    using SharpCifs.Dcerpc.Ndr;
    using SharpCifs.Util.Sharpen;
    using rpc.core;
    using rpc.pdu;
    using Serilog;
    using System.IO;

    /// <summary>
    /// Connection oriented endpoint
    /// </summary>
    public class ConnectionOrientedEndpoint : IEndpoint {

        /// <inheritdoc/>
        public ITransport Transport { get; }

        /// <inheritdoc/>
        public PresentationSyntax Syntax { get; }

        /// <summary>
        /// Current iid
        /// </summary>
        protected string CurrentIID { get; set; }

        /// <summary>
        /// Current connection context
        /// </summary>
        protected IConnectionContext Context { get; set; }

        /// <summary>
        /// Create endpoint
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="syntax"></param>
        public ConnectionOrientedEndpoint(ITransport transport, PresentationSyntax syntax) {
            _contextIdToUse = _contextIdCounter;
            Transport = transport;
            Syntax = syntax;
        }

        /// <inheritdoc/>
        public virtual void Call(Semantics semantics, UUID objectId,
            int opnum, NdrOp ndrobj) {
            Bind();
            var request = new RequestCoPdu {
                ContextId = _contextIdToUse
            };

            var b = new byte[1024];
            var buffer = new NdrBuffer(b, 0);
            var ndr = new NdrCodec();
            ndrobj.Encode(ndr, buffer);
            var stub = new byte[buffer.Length]; // yuk
            Array.Copy(buffer.Buf, 0, stub, 0, stub.Length);

            Log.Logger.Verbose("\n" + Utils.HexString(stub, 0, stub.Length));
            request.Stub = stub;
            request.AllocationHint = buffer.Length;
            request.Opnum = opnum;
            request.Object = objectId;
            if ((semantics & Semantics.MAYBE) != 0) {
                request.SetFlag(ConnectionOrientedPdu.PFC_MAYBE, true);
            }
            Send(request);

            if (request.GetFlag(ConnectionOrientedPdu.PFC_MAYBE)) {
                return;
            }
            var reply = Receive();
            if (reply is ResponseCoPdu) {
                ndr.Format = reply.Format;

                buffer = new NdrBuffer(((ResponseCoPdu)reply).Stub, 0);
                Log.Logger.Verbose("\n" + Utils.HexString(buffer.Buf, 0, buffer.Buf.Length));
                ndrobj.Decode(ndr, buffer);
            }
            else if (reply is FaultCoPdu fault) {
                throw new FaultException("Received fault.", fault.Status, fault.Stub);
            }
            else if (reply is ShutdownPdu) {
                throw new RpcException("Received shutdown request from server.");
            }
            else {
                throw new RpcException("Received unexpected PDU from server.");
            }
        }

        /// <inheritdoc/>
        public void Detach() {
            _bound = false;
            Context = null;
            Transport.Close();
        }

        /// <summary>
        /// Rebind
        /// </summary>
        /// <exception cref="IOException"></exception>
        protected void Rebind() {
            _bound = false;
            Bind();
        }

        /// <summary>
        /// Bind
        /// </summary>
        /// <exception cref="IOException"></exception>
        protected void Bind() {
            if (_bound) {
                return;
            }
            if (Context != null) {
                _bound = true;
                try {
                    var cid = (int?)_uuidsVsContextIds[Syntax.ToString().ToUpper()];
                    var pdu = Context.Alter(new PresentationContext(cid == null ? ++_contextIdCounter : (int)cid, Syntax));
                    var sendAlter = false;
                    if (cid == null) {
                        _uuidsVsContextIds[Syntax.ToString().ToUpper()] = _contextIdCounter;
                        _contextIdToUse = _contextIdCounter;
                        sendAlter = true;
                    }
                    else {
                        _contextIdToUse = (int)cid;
                    }

                    if (sendAlter) {
                        if (pdu != null) {
                            Send(pdu);
                        }
                        while (!Context.Established) {
                            var recieved = Receive();
                            if ((pdu = Context.Accept(recieved)) != null) {
                                switch (pdu.Type) {
                                    case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
                                        if (((BindAcknowledgePdu)pdu).ResultList[0].Result /*was: Reason*/ !=
                                            PresentationResultCode.PROVIDER_REJECTION) {
                                            CurrentIID = ((BindPdu)recieved).ContextList[0]
                                                .AbstractSyntax.Uuid.ToString();
                                        }
                                        break;
                                    case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
                                        // we need to record the iid now if this is successful and subsequent
                                        // calls will now be for this iid.
                                        if (((AlterContextResponsePdu)pdu).ResultList[0].Result /*was: Reason*/ !=
                                            PresentationResultCode.PROVIDER_REJECTION) {
                                            CurrentIID = ((AlterContextPdu)recieved).ContextList[0]
                                                .AbstractSyntax.Uuid.ToString();
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
                    _bound = false;
                    throw ex;
                }
                catch (Exception ex) {
                    _bound = false;
                    throw ex;
                }
            }
            else {
                Connect();
            }
        }

        /// <summary>
        /// Send
        /// </summary>
        /// <param name="request"></param>
        /// <exception cref="IOException"></exception>
        protected void Send(ConnectionOrientedPdu request) {
            Bind();
            Context.Connection.Transmit(request, Transport);
        }

        /// <summary>
        /// Receive
        /// </summary>
        /// <exception cref="IOException"></exception>
        protected ConnectionOrientedPdu Receive() {
            return Context.Connection.Receive(Transport);
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <exception cref="IOException"></exception>
        private void Connect() {
            _bound = true;
            _contextIdCounter = 0;
            CurrentIID = null;
            try {
                _uuidsVsContextIds[Syntax.ToString().ToUpper()] = _contextIdCounter;
                Context = CreateContext();
                var pdu = Context.Init(new PresentationContext(_contextIdCounter, Syntax), Transport.Properties);
                _contextIdToUse = _contextIdCounter;
                if (pdu != null) {
                    Send(pdu);
                }
                while (!Context.Established) {
                    var recieved = Receive();
                    if ((pdu = Context.Accept(recieved)) != null) {
                        switch (pdu.Type) {
                            case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
                                if (((BindAcknowledgePdu)pdu).ResultList[0].Result /*was: Reason*/ !=
                                    PresentationResultCode.PROVIDER_REJECTION) {
                                    CurrentIID = ((BindPdu)recieved).ContextList[0].AbstractSyntax.Uuid.ToString();
                                }
                                break;
                            case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
                                // we need to record the iid now if this is successful and subsequent calls
                                // will now be for this iid.
                                if (((AlterContextResponsePdu)pdu).ResultList[0].Result /*was: Reason*/ !=
                                    PresentationResultCode.PROVIDER_REJECTION) {
                                    CurrentIID = ((AlterContextPdu)recieved).ContextList[0].AbstractSyntax.Uuid.ToString();
                                }
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
        }

        /// <summary>
        /// Create context
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ProviderException"></exception>
        private IConnectionContext CreateContext() {
            var properties = Transport.Properties;
            if (properties == null) {
                return new BasicConnectionContext();
            }
            var context = (string)properties.GetProperty("rpc.connectionContext");
            if (context == null) {
                return new BasicConnectionContext();
            }
            try {
                return (IConnectionContext)Type.GetType(context)
                    .GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
            }
            catch (Exception ex) {
                throw new ProviderException(ex.Message);
            }
        }

        private bool _bound;
        private int _contextIdCounter;
        private int _contextIdToUse;
        private readonly Hashtable _uuidsVsContextIds = new Hashtable();
    }
}