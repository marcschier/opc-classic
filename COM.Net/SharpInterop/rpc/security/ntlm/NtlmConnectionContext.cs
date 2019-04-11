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

namespace rpc.security.ntlm {
    using System;
    using System.IO;
    using rpc.core;
    using rpc.pdu;
    using SharpCifs.Util.Sharpen;

    /// <summary>
    /// Connection context
    /// </summary>
    public class NtlmConnectionContext : IConnectionContext {

        /// <summary>
        /// Connection
        /// </summary>
        public IConnection Connection { get; private set; }

        /// <summary>
        /// Established
        /// </summary>
        public virtual bool Established { get; private set; }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="context"></param>
        /// <param name="properties"></param>
        /// <exception cref="IOException"></exception>
        /// <returns></returns>
        public virtual ConnectionOrientedPdu Init2(PresentationContext context, Properties properties) {
            Established = false;
            if (properties != null) {
                var maxTransmit = (string)properties.GetProperty(rpc.Connection.MAX_TRANSMIT_FRAGMENT);
                if (maxTransmit != null) {
                    _maxTransmitFragment = int.Parse(maxTransmit);
                }
                var maxReceive = (string)properties.GetProperty(rpc.Connection.MAX_RECEIVE_FRAGMENT);
                if (maxReceive != null) {
                    _maxReceiveFragment = int.Parse(maxReceive);
                }
            }
            var pdu = new BindPdu {
                ContextList = new PresentationContext[] { context },
                MaxTransmitFragment = _maxTransmitFragment,
                MaxReceiveFragment = _maxReceiveFragment
            };
            Connection = new NtlmConnection(properties);
            _assocGroupId = 0;
            return pdu;
        }

        /// <inheritdoc/>
        public virtual ConnectionOrientedPdu Init(PresentationContext context, Properties properties) {

            var pdu = (BindPdu)Init2(context, properties);
            pdu.ResetCallIdCounter();
            return pdu;
        }

        /// <inheritdoc/>
        public virtual ConnectionOrientedPdu Alter(PresentationContext context) {
            Established = false;
            var pdu = new AlterContextPdu {
                ContextList = new PresentationContext[] { context },
                AssociationGroupId = _assocGroupId
            };
            return pdu;
        }

        /// <inheritdoc/>
        public virtual ConnectionOrientedPdu Accept(ConnectionOrientedPdu pdu) {
            PresentationResult[] results;
            switch (pdu.Type) {
                case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
                    var bindAck = (BindAcknowledgePdu)pdu;
                    results = bindAck.ResultList;
                    if (results == null) {
                        throw new BindException("No presentation context results.");
                    }
                    for (var i = results.Length - 1; i >= 0; i--) {
                        if (results[i].Result != PresentationResultCode.ACCEPTANCE) {
                            throw new PresentationException("Context rejected.", results[i]);
                        }
                    }
                    _transmitLength = bindAck.MaxReceiveFragment;
                    _receiveLength = bindAck.MaxTransmitFragment;
                    Established = true;
                    ((NtlmConnection)Connection).TransmitLength = _transmitLength;
                    ((NtlmConnection)Connection).ReceiveLength = _receiveLength;
                    _assocGroupId = bindAck.AssociationGroupId;
                    return new Auth3Pdu();
                case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
                    var alterContextResponse = (AlterContextResponsePdu)pdu;
                    results = alterContextResponse.ResultList;
                    if (results == null) {
                        throw new BindException("No presentation context results.");
                    }
                    for (var i = results.Length - 1; i >= 0; i--) {
                        if (results[i].Result != PresentationResultCode.ACCEPTANCE) {
                            throw new PresentationException("Context rejected.", results[i]);
                        }
                    }
                    Established = true;
                    // return new Auth3Pdu();
                    return null;
                case BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE:
                    throw new BindException("Unable to bind.", ((BindNoAcknowledgePdu)pdu).RejectReason);
                case FaultCoPdu.FAULT_TYPE:
                    throw new FaultException("Fault occurred.", ((FaultCoPdu)pdu).Status);
                case ShutdownPdu.SHUTDOWN_TYPE:
                    throw new RpcException("Server shutdown connection.");
                case BindPdu.BIND_TYPE:
                    Established = false;
                    // CHECK PRESENTATION CONTEXT
                    // CHALLENGE
                    throw new Exception();
                case AlterContextPdu.ALTER_CONTEXT_TYPE:
                    Established = false;
                    // CHECK PRESENTATION CONTEXT
                    // CHALLENGE
                    throw new Exception();
                case Auth3Pdu.AUTH3_TYPE:
                    // AUTHENTICATE
                    // TWEAK CONNECTION
                    Established = true;
                    return null;
                default:
                    throw new RpcException("Unknown/unacceptable PDU type.");
            }
        }

        private int _maxTransmitFragment = rpc.Connection.DEFAULT_MAX_TRANSMIT_FRAGMENT;
        private int _maxReceiveFragment = rpc.Connection.DEFAULT_MAX_RECEIVE_FRAGMENT;
        private int _transmitLength;
        private int _receiveLength;
        private int _assocGroupId;
    }

}