// 
// Copyright (c) 2013 Vikram Roopchand
// 
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
// 


namespace org.jinterop.dcom.transport {


    using ConnectionOrientedPdu = rpc.ConnectionOrientedPdu;
    using PresentationContext = rpc.core.PresentationContext;
    using PresentationResult = rpc.core.PresentationResult;
    using PresentationSyntax = rpc.core.PresentationSyntax;
    using UUID = rpc.core.UUID;
    using AlterContextPdu = rpc.pdu.AlterContextPdu;
    using AlterContextResponsePdu = rpc.pdu.AlterContextResponsePdu;
    using BindAcknowledgePdu = rpc.pdu.BindAcknowledgePdu;
    using BindPdu = rpc.pdu.BindPdu;
    using NtlmConnectionContext = rpc.security.ntlm.NtlmConnectionContext;

    /// <summary>
    /// @exclude
    /// @since 1.0
    /// 
    /// </summary>
    public sealed class JIComRuntimeNTLMConnectionContext : NtlmConnectionContext
	{

		  private const string IID = "IID";
		  private const string IID2 = "IID2";

		  private bool established;
		  private SharpCifs.Util.Sharpen.Properties properties;
		  private IList listOfInterfacesSupported = Collections.synchronizedList(new ArrayList());

		  // this returns null, so that a recieve is performed first.
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.ConnectionOrientedPdu init(rpc.core.PresentationContext context, java.util.SharpCifs.Util.Sharpen.Properties properties) throws java.io.IOException
		  public ConnectionOrientedPdu init(PresentationContext context, SharpCifs.Util.Sharpen.Properties properties)
		  {
			  base.Init2(context,properties);
			  this.properties = properties;
			  listOfInterfacesSupported.Add(((string)properties.getProperty(IID)).ToUpper());
			  listOfInterfacesSupported.Add(((string)properties.getProperty(IID2)).ToUpper() + ":0.0");
			  updateListOfInterfacesSupported2((IList)properties.get("LISTOFSUPPORTEDINTERFACES"));
			  return null;
		  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public rpc.ConnectionOrientedPdu accept(rpc.ConnectionOrientedPdu pdu) throws java.io.IOException
		  public ConnectionOrientedPdu accept(ConnectionOrientedPdu pdu)
		  {
			  ConnectionOrientedPdu reply = null;
			  switch (pdu.Type)
			  {
				  case BindPdu.BIND_TYPE:
					  established = true;
					  var presentationContexts = ((BindPdu)pdu).ContextList;
					  reply = new BindAcknowledgePdu();
					  var result = new PresentationResult[1];
					  for (var i = 0; i < presentationContexts.Length;i++)
					  {
						  var presentationContext = presentationContexts[i];

						  var contains = false;
						  lock (listOfInterfacesSupported)
						  {
							  contains = listOfInterfacesSupported.Contains(presentationContext.AbstractSyntax.ToString().ToUpper());
						  }
						  if (!contains)
						  {
							  //create a fault PDU stating the syntax is not supported.
							  result[0] = new PresentationResult(PresentationResult.PROVIDER_REJECTION,PresentationResult.ABSTRACT_SYNTAX_NOT_SUPPORTED,new PresentationSyntax(UUID.NIL_UUID + ":0.0"));
							  ((BindAcknowledgePdu)reply).ResultList = result;
							  break;
						  }
					  }

					  //all okay
					  if (((BindAcknowledgePdu)reply).ResultList == null)
					  {
						  result[0] = new PresentationResult(); //this will be acceptance.
						  ((BindAcknowledgePdu)reply).AssociationGroupId = new object().GetHashCode(); //TODO should I save this ?
						  ((BindAcknowledgePdu)reply).ResultList = result;
					  }((BindAcknowledgePdu)reply).setCallId(pdu.getCallId());


					  //issue a challenge against the request info


					  break;
				  case AlterContextPdu.ALTER_CONTEXT_TYPE:
					  established = true;

					  presentationContexts = ((AlterContextPdu)pdu).ContextList;
					  reply = new AlterContextResponsePdu();
					  result = new PresentationResult[1];
					  for (var i = 0; i < presentationContexts.Length;i++)
					  {
						  var presentationContext = presentationContexts[i];
						  var contains = false;
						  lock (listOfInterfacesSupported)
						  {
							  contains = listOfInterfacesSupported.Contains(presentationContext.AbstractSyntax.ToString().ToUpper());
						  }
						  if (!contains)
						  {
							  //create a fault PDU stating the syntax is not supported.
							  result[0] = new PresentationResult(PresentationResult.PROVIDER_REJECTION,PresentationResult.ABSTRACT_SYNTAX_NOT_SUPPORTED,new PresentationSyntax(UUID.NIL_UUID + ":0.0"));
							  ((AlterContextResponsePdu)reply).ResultList = result;
							  break;
						  }
					  }

					  //all okay
					  if (((AlterContextResponsePdu)reply).ResultList == null)
					  {
						  result[0] = new PresentationResult(); //this will be acceptance.
						  ((AlterContextResponsePdu)reply).AssociationGroupId = new object().GetHashCode(); //TODO should I save this ?
						  ((AlterContextResponsePdu)reply).ResultList = result;
					  }

					  ((AlterContextResponsePdu)reply).CallId = pdu.CallId;

					  //issue a challenge against the request info


				  break;
				  default:
					  reply = base.Accept(reply);
				  break;
			  }

			  return reply;
		  }

        public bool Established => base.Established | established;

        internal void updateListOfInterfacesSupported(IList newList)
		  {
			  lock (listOfInterfacesSupported)
			  {
				listOfInterfacesSupported.AddRange(newList);
			  }
		  }

		  internal void updateListOfInterfacesSupported2(IList newList)
		  {
			for (var i = 0;i < newList.Count;i++)
			{
				listOfInterfacesSupported.Add(newList[i] + ":0.0");
			}
		  }

	}

}