//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using Opc.Classic.Dcom.Internal;
using SharpInterop.Rpc.Core;
using SharpInterop.Rpc.pdu;

namespace SharpInterop.Rpc; 
/// <summary>
/// Basic connection context
/// </summary>
public class BasicConnectionContext : IConnectionContext {

    /// <inheritdoc/>
    public IConnection Connection { get; private set; }

    /// <inheritdoc/>
    public virtual bool Established { get; private set; }

    /// <inheritdoc/>
    public virtual ConnectionOrientedPdu Init(PresentationContext context, PropertyBag properties) {
        Established = false;
        Connection = new DefaultConnection();
        if (properties != null) {
            var maxTransmit = (string)properties.GetProperty(SharpInterop.Rpc.Connection.MAX_TRANSMIT_FRAGMENT);
            if (maxTransmit != null) {
                _maxTransmitFragment = int.Parse(maxTransmit);
            }
            var maxReceive = (string)properties.GetProperty(SharpInterop.Rpc.Connection.MAX_RECEIVE_FRAGMENT);
            if (maxReceive != null) {
                _maxReceiveFragment = int.Parse(maxReceive);
            }
        }
        var pdu = new BindPdu {
            ContextList = new PresentationContext[] { context },
            MaxTransmitFragment = _maxTransmitFragment,
            MaxReceiveFragment = _maxReceiveFragment
        };
        return pdu;
    }

    /// <inheritdoc/>
    public virtual ConnectionOrientedPdu Alter(PresentationContext context) {
        Established = false;
        var pdu = new AlterContextPdu {
            ContextList = new PresentationContext[] { context }
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
                Connection = new DefaultConnection(_transmitLength, _receiveLength);
                return null;
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
                return null;
            case BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE:
                throw new BindException("Unable to bind.", ((BindNoAcknowledgePdu)pdu).RejectReason);
            case FaultCoPdu.FAULT_TYPE:
                throw new FaultException("Fault occurred.", ((FaultCoPdu)pdu).Status);
            case ShutdownPdu.SHUTDOWN_TYPE:
                throw new RpcException("Server shutdown connection.");
            case BindPdu.BIND_TYPE:
            case AlterContextPdu.ALTER_CONTEXT_TYPE:
                throw new RpcException("Server-side currently unsupported.");
            default:
                throw new RpcException("Unknown/unacceptable PDU type.");
        }
    }

    private int _maxTransmitFragment = SharpInterop.Rpc.Connection.DEFAULT_MAX_TRANSMIT_FRAGMENT;
    private int _maxReceiveFragment = SharpInterop.Rpc.Connection.DEFAULT_MAX_RECEIVE_FRAGMENT;
    private int _transmitLength;
    private int _receiveLength;
}
