using System;

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



namespace rpc.security.ntlm {


    using PresentationContext = rpc.core.PresentationContext;
    using PresentationResult = rpc.core.PresentationResult;
    using AlterContextPdu = rpc.pdu.AlterContextPdu;
    using AlterContextResponsePdu = rpc.pdu.AlterContextResponsePdu;
    using Auth3Pdu = rpc.pdu.Auth3Pdu;
    using BindAcknowledgePdu = rpc.pdu.BindAcknowledgePdu;
    using BindNoAcknowledgePdu = rpc.pdu.BindNoAcknowledgePdu;
    using BindPdu = rpc.pdu.BindPdu;
    using FaultCoPdu = rpc.pdu.FaultCoPdu;
    using ShutdownPdu = rpc.pdu.ShutdownPdu;

    public class NtlmConnectionContext : ConnectionContext {

        private int MaxTransmitFragment = rpc.ConnectionContext_Fields.DEFAULT_MAX_TRANSMIT_FRAGMENT;

        private int MaxReceiveFragment = rpc.ConnectionContext_Fields.DEFAULT_MAX_RECEIVE_FRAGMENT;

        private NtlmConnection Connection_Renamed;

        private bool Established_Renamed;

        private int TransmitLength;

        private int ReceiveLength;

        private int AssocGroupId = 0;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.ConnectionOrientedPdu init2(rpc.core.PresentationContext context, java.util.Properties properties) throws java.io.IOException
        public virtual ConnectionOrientedPdu Init2(PresentationContext context, Properties properties) {
            Established_Renamed = false;
            if (properties != null) {
                string maxTransmit = properties.getProperty(rpc.ConnectionContext_Fields.MAX_TRANSMIT_FRAGMENT);
                if (maxTransmit != null) {
                    MaxTransmitFragment = int.Parse(maxTransmit);
                }
                string maxReceive = properties.getProperty(rpc.ConnectionContext_Fields.MAX_RECEIVE_FRAGMENT);
                if (maxReceive != null) {
                    MaxReceiveFragment = int.Parse(maxReceive);
                }
            }
            BindPdu pdu = new BindPdu();
            pdu.ContextList = new PresentationContext[] { context };
            pdu.MaxTransmitFragment = MaxTransmitFragment;
            pdu.MaxReceiveFragment = MaxReceiveFragment;
            Connection_Renamed = new NtlmConnection(properties);
            AssocGroupId = 0;
            return pdu;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.ConnectionOrientedPdu init(rpc.core.PresentationContext context, java.util.Properties properties) throws java.io.IOException
        public virtual ConnectionOrientedPdu Init(PresentationContext context, Properties properties) {

            BindPdu pdu = (BindPdu)Init2(context, properties);
            pdu.ResetCallIdCounter();
            return pdu;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.ConnectionOrientedPdu alter(rpc.core.PresentationContext context) throws java.io.IOException
        public virtual ConnectionOrientedPdu Alter(PresentationContext context) {
            Established_Renamed = false;
            AlterContextPdu pdu = new AlterContextPdu();
            pdu.ContextList = new PresentationContext[] { context };
            pdu.AssociationGroupId = AssocGroupId;
            return pdu;
        }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.ConnectionOrientedPdu accept(rpc.ConnectionOrientedPdu pdu) throws java.io.IOException
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
                Connection_Renamed.TransmitLength = TransmitLength;
                Connection_Renamed.ReceiveLength = ReceiveLength;
                AssocGroupId = bindAck.AssociationGroupId;
                return new Auth3Pdu();
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
                //return new Auth3Pdu();
                return null;
            case BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE:
                throw new BindException("Unable to bind.", ((BindNoAcknowledgePdu) pdu).RejectReason);
            case FaultCoPdu.FAULT_TYPE:
                throw new FaultException("Fault occurred.", ((FaultCoPdu) pdu).Status);
            case ShutdownPdu.SHUTDOWN_TYPE:
                throw new RpcException("Server shutdown connection.");
            case BindPdu.BIND_TYPE:
                Established_Renamed = false;
                //CHECK PRESENTATION CONTEXT
                //CHALLENGE
                throw new Exception();
            case AlterContextPdu.ALTER_CONTEXT_TYPE:
                Established_Renamed = false;
                //CHECK PRESENTATION CONTEXT
                //CHALLENGE
                throw new Exception();
            case Auth3Pdu.AUTH3_TYPE:
                //AUTHENTICATE
                //TWEAK CONNECTION
                Established_Renamed = true;
                return null;
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