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


    using PresentationContext = rpc.core.PresentationContext;
    using PresentationResult = rpc.core.PresentationResult;
    using AlterContextPdu = rpc.pdu.AlterContextPdu;
    using AlterContextResponsePdu = rpc.pdu.AlterContextResponsePdu;
    using BindAcknowledgePdu = rpc.pdu.BindAcknowledgePdu;
    using BindNoAcknowledgePdu = rpc.pdu.BindNoAcknowledgePdu;
    using BindPdu = rpc.pdu.BindPdu;
    using FaultCoPdu = rpc.pdu.FaultCoPdu;
    using ShutdownPdu = rpc.pdu.ShutdownPdu;

    public class BasicConnectionContext : ConnectionContext {

        private int MaxTransmitFragment = ConnectionContext_Fields.DEFAULT_MAX_TRANSMIT_FRAGMENT;

        private int MaxReceiveFragment = ConnectionContext_Fields.DEFAULT_MAX_RECEIVE_FRAGMENT;

        private Connection Connection_Renamed;

        private bool Established_Renamed;

        private int TransmitLength;

        private int ReceiveLength;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ConnectionOrientedPdu init(rpc.core.PresentationContext context, java.util.Properties properties) throws java.io.IOException
        public virtual ConnectionOrientedPdu Init(PresentationContext context, Properties properties) {
            Established_Renamed = false;
            Connection_Renamed = new DefaultConnection();
            if (properties != null) {
                string maxTransmit = properties.getProperty(ConnectionContext_Fields.MAX_TRANSMIT_FRAGMENT);
                if (maxTransmit != null) {
                    MaxTransmitFragment = int.Parse(maxTransmit);
                }
                string maxReceive = properties.getProperty(ConnectionContext_Fields.MAX_RECEIVE_FRAGMENT);
                if (maxReceive != null) {
                    MaxReceiveFragment = int.Parse(maxReceive);
                }
            }
            BindPdu pdu = new BindPdu();
            pdu.ContextList = new PresentationContext[] { context };
            pdu.MaxTransmitFragment = MaxTransmitFragment;
            pdu.MaxReceiveFragment = MaxReceiveFragment;
            return pdu;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ConnectionOrientedPdu alter(rpc.core.PresentationContext context) throws java.io.IOException
        public virtual ConnectionOrientedPdu Alter(PresentationContext context) {
            Established_Renamed = false;
            AlterContextPdu pdu = new AlterContextPdu();
            pdu.ContextList = new PresentationContext[] { context };
            return pdu;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ConnectionOrientedPdu accept(ConnectionOrientedPdu pdu) throws java.io.IOException
        public virtual ConnectionOrientedPdu Accept(ConnectionOrientedPdu pdu) {
            PresentationResult[] results = null;
            switch (pdu.Type) {
            case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
                BindAcknowledgePdu bindAck = (BindAcknowledgePdu) pdu;
                results = bindAck.ResultList;
                if (results == null) {
                    throw new BindException("No presentation context results.");
                }
                for (int i = results.Length - 1; i >= 0; i--) {
                    if (results[i].Result != PresentationResult.ACCEPTANCE) {
                        throw new PresentationException("Context rejected.", results[i]);
                    }
                }
                TransmitLength = bindAck.MaxReceiveFragment;
                ReceiveLength = bindAck.MaxTransmitFragment;
                Established_Renamed = true;
                Connection_Renamed = new DefaultConnection(TransmitLength, ReceiveLength);
                return null;
            case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
                AlterContextResponsePdu alterContextResponse = (AlterContextResponsePdu) pdu;
                results = alterContextResponse.ResultList;
                if (results == null) {
                    throw new BindException("No presentation context results.");
                }
                for (int i = results.Length - 1; i >= 0; i--) {
                    if (results[i].Result != PresentationResult.ACCEPTANCE) {
                        throw new PresentationException("Context rejected.", results[i]);
                    }
                }
                Established_Renamed = true;
                return null;
            case BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE:
                throw new BindException("Unable to bind.", ((BindNoAcknowledgePdu) pdu).RejectReason);
            case FaultCoPdu.FAULT_TYPE:
                throw new FaultException("Fault occurred.", ((FaultCoPdu) pdu).Status);
            case ShutdownPdu.SHUTDOWN_TYPE:
                throw new RpcException("Server shutdown connection.");
            case BindPdu.BIND_TYPE:
            case AlterContextPdu.ALTER_CONTEXT_TYPE:
                throw new RpcException("Server-side currently unsupported.");
            default:
                throw new RpcException("Unknown/unacceptable PDU type.");
            }
        }

        public virtual Connection Connection {
            get {
                return Connection_Renamed;
            }
        }

        public virtual bool Established {
            get {
                return Established_Renamed;
            }
        }

    }

}