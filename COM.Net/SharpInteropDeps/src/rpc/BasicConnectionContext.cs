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
namespace rpc
{


	using PresentationContext = core.PresentationContext;
	using PresentationResult = core.PresentationResult;
	using AlterContextPdu = pdu.AlterContextPdu;
	using AlterContextResponsePdu = pdu.AlterContextResponsePdu;
	using BindAcknowledgePdu = pdu.BindAcknowledgePdu;
	using BindNoAcknowledgePdu = pdu.BindNoAcknowledgePdu;
	using BindPdu = pdu.BindPdu;
	using FaultCoPdu = pdu.FaultCoPdu;
	using ShutdownPdu = pdu.ShutdownPdu;

	public class BasicConnectionContext : ConnectionContext
	{

		private int maxTransmitFragment = ConnectionContext_Fields.DEFAULT_MAX_TRANSMIT_FRAGMENT;

		private int maxReceiveFragment = ConnectionContext_Fields.DEFAULT_MAX_RECEIVE_FRAGMENT;

		private Connection connection;

		private bool established;

		private int transmitLength;

		private int receiveLength;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ConnectionOrientedPdu init(rpc.core.PresentationContext context, java.util.Properties properties) throws java.io.IOException
		public virtual ConnectionOrientedPdu init(PresentationContext context, Properties properties)
		{
			established = false;
			connection = new DefaultConnection();
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
            return pdu;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ConnectionOrientedPdu alter(rpc.core.PresentationContext context) throws java.io.IOException
		public virtual ConnectionOrientedPdu alter(PresentationContext context)
		{
			established = false;
            var pdu = new AlterContextPdu {
                ContextList = new PresentationContext[] { context }
            };
            return pdu;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ConnectionOrientedPdu accept(ConnectionOrientedPdu pdu) throws java.io.IOException
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
				connection = new DefaultConnection(transmitLength, receiveLength);
				return null;
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

        public virtual Connection Connection => connection;

        public virtual bool Established => established;

    }

}