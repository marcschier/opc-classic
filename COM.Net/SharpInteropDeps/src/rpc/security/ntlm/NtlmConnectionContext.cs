using System;

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



namespace rpc.security.ntlm
{


	using PresentationContext = core.PresentationContext;
	using PresentationResult = core.PresentationResult;
	using AlterContextPdu = pdu.AlterContextPdu;
	using AlterContextResponsePdu = pdu.AlterContextResponsePdu;
	using Auth3Pdu = pdu.Auth3Pdu;
	using BindAcknowledgePdu = pdu.BindAcknowledgePdu;
	using BindNoAcknowledgePdu = pdu.BindNoAcknowledgePdu;
	using BindPdu = pdu.BindPdu;
	using FaultCoPdu = pdu.FaultCoPdu;
	using ShutdownPdu = pdu.ShutdownPdu;

	public class NtlmConnectionContext : ConnectionContext
	{

		private int maxTransmitFragment = ConnectionContext_Fields.DEFAULT_MAX_TRANSMIT_FRAGMENT;

		private int maxReceiveFragment = ConnectionContext_Fields.DEFAULT_MAX_RECEIVE_FRAGMENT;

		private NtlmConnection connection;

		private bool established;

		private int transmitLength;

		private int receiveLength;

		private int assocGroupId;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.ConnectionOrientedPdu init2(rpc.core.PresentationContext context, java.util.Properties properties) throws java.io.IOException
		public virtual ConnectionOrientedPdu init2(PresentationContext context, Properties properties)
		{
			established = false;
			if (properties != null)
			{
				string maxTransmit = properties.getProperty(ConnectionContext_Fields.MAX_TRANSMIT_FRAGMENT);
				if (maxTransmit != null)
				{
					maxTransmitFragment = int.Parse(maxTransmit);
				}
				string maxReceive = properties.getProperty(ConnectionContext_Fields.MAX_RECEIVE_FRAGMENT);
				if (maxReceive != null)
				{
					maxReceiveFragment = int.Parse(maxReceive);
				}
			}
            var pdu = new BindPdu {
                ContextList = new PresentationContext[] { context },
                MaxTransmitFragment = maxTransmitFragment,
                MaxReceiveFragment = maxReceiveFragment
            };
            connection = new NtlmConnection(properties);
			assocGroupId = 0;
			return pdu;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.ConnectionOrientedPdu init(rpc.core.PresentationContext context, java.util.Properties properties) throws java.io.IOException
		public virtual ConnectionOrientedPdu init(PresentationContext context, Properties properties)
		{

			var pdu = (BindPdu)init2(context, properties);
			pdu.resetCallIdCounter();
			return pdu;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.ConnectionOrientedPdu alter(rpc.core.PresentationContext context) throws java.io.IOException
		public virtual ConnectionOrientedPdu alter(PresentationContext context)
		{
			established = false;
            var pdu = new AlterContextPdu {
                ContextList = new PresentationContext[] { context },
                AssociationGroupId = assocGroupId
            };
            return pdu;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.ConnectionOrientedPdu accept(rpc.ConnectionOrientedPdu pdu) throws java.io.IOException
		public virtual ConnectionOrientedPdu accept(ConnectionOrientedPdu pdu)
		{
			PresentationResult[] results = null;
			switch (pdu.Type)
			{
			case BindAcknowledgePdu.BIND_ACKNOWLEDGE_TYPE:
				var bindAck = (BindAcknowledgePdu) pdu;
				results = bindAck.ResultList;
				if (results == null)
				{
					throw new BindException("No presentation context results.");
				}
				for (var i = results.Length - 1; i >= 0; i--)
				{
					if (results[i].result != PresentationResult.ACCEPTANCE)
					{
						throw new PresentationException("Context rejected.", results[i]);
					}
				}
				transmitLength = bindAck.MaxReceiveFragment;
				receiveLength = bindAck.MaxTransmitFragment;
				established = true;
				connection.TransmitLength = transmitLength;
				connection.ReceiveLength = receiveLength;
				assocGroupId = bindAck.AssociationGroupId;
				return new Auth3Pdu();
			case AlterContextResponsePdu.ALTER_CONTEXT_RESPONSE_TYPE:
				var alterContextResponse = (AlterContextResponsePdu) pdu;
				results = alterContextResponse.ResultList;
				if (results == null)
				{
					throw new BindException("No presentation context results.");
				}
				for (var i = results.Length - 1; i >= 0; i--)
				{
					if (results[i].result != PresentationResult.ACCEPTANCE)
					{
						throw new PresentationException("Context rejected.", results[i]);
					}
				}
				established = true;
				//return new Auth3Pdu();
				return null;
			case BindNoAcknowledgePdu.BIND_NO_ACKNOWLEDGE_TYPE:
				throw new BindException("Unable to bind.", ((BindNoAcknowledgePdu) pdu).RejectReason);
			case FaultCoPdu.FAULT_TYPE:
				throw new FaultException("Fault occurred.", ((FaultCoPdu) pdu).Status);
			case ShutdownPdu.SHUTDOWN_TYPE:
				throw new RpcException("Server shutdown connection.");
			case BindPdu.BIND_TYPE:
				established = false;
				//CHECK PRESENTATION CONTEXT
				//CHALLENGE
				throw new Exception();
			case AlterContextPdu.ALTER_CONTEXT_TYPE:
				established = false;
				//CHECK PRESENTATION CONTEXT
				//CHALLENGE
				throw new Exception();
			case Auth3Pdu.AUTH3_TYPE:
				//AUTHENTICATE
				//TWEAK CONNECTION
				established = true;
				return null;
			default:
				throw new RpcException("Unknown/unacceptable PDU type.");
			}
		}

        public virtual Connection Connection => connection;

        public virtual bool Established => established;

    }

}