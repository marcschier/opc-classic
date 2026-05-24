//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//


using Opc.Classic.Dcom.Internal;
using System.Collections.Generic;
using SharpInterop.Rpc.pdu;
using SharpInterop.Rpc.Auth.ntlm;
using SharpInterop.Rpc.Core;
using SharpCifs.Util.Sharpen;
using SharpInterop.Rpc;

namespace SharpInterop.Transport; 
/// <summary>
/// Connection context
/// </summary>
public sealed class ComRuntimeNtlmConnectionContext : NtlmConnectionContext {

    /// <inheritdoc/>
    public override bool Established => base.Established | _established;

    /// <inheritdoc/>
    public override ConnectionOrientedPdu Init(PresentationContext context, PropertyBag properties) {
        base.Init2(context, properties);
        _properties = properties;
        lock (_listOfInterfacesSupported) { // TODO - find another way...
            _listOfInterfacesSupported.Add(((string)properties.GetProperty(kIID)).ToUpper());
            _listOfInterfacesSupported.Add(((string)properties.GetProperty(kIID2)).ToUpper() + ":0.0");
        }
        UpdateListOfInterfacesSupported2(
            (List<string>)properties.GetProperty("LISTOFSUPPORTEDINTERFACES")); // TODO - find another way...
        return null;
    }

    /// <inheritdoc/>
    public override ConnectionOrientedPdu Accept(ConnectionOrientedPdu pdu) {
        ConnectionOrientedPdu reply = null;
        switch (pdu.Type) {
            case BindPdu.BIND_TYPE:
                _established = true;
                var presentationContexts = ((BindPdu)pdu).ContextList;
                reply = new BindAcknowledgePdu();
                var result = new PresentationResult[1];
                for (var i = 0; i < presentationContexts.Length; i++) {
                    var presentationContext = presentationContexts[i];

                    var contains = false;
                    lock (_listOfInterfacesSupported) {
                        contains = _listOfInterfacesSupported.Contains(presentationContext.AbstractSyntax.ToString().ToUpper());
                    }
                    if (!contains) {
                        // create a fault PDU stating the syntax is not supported.
                        result[0] = new PresentationResult(PresentationResultCode.PROVIDER_REJECTION,
                            PresentationResultReason.ABSTRACT_SYNTAX_NOT_SUPPORTED,
                            new PresentationSyntax(UUID.NIL_UUID + ":0.0"));
                        ((BindAcknowledgePdu)reply).ResultList = result;
                        break;
                    }
                }

                // all okay
                if (((BindAcknowledgePdu)reply).ResultList == null) {
                    result[0] = new PresentationResult(); // this will be acceptance.
                    ((BindAcknowledgePdu)reply).AssociationGroupId =
                        new object().GetHashCode(); // TODO should I save this ?
                    ((BindAcknowledgePdu)reply).ResultList = result;
                }
                ((BindAcknowledgePdu)reply).CallId = pdu.CallId;
                // issue a challenge against the request info
                break;
            case AlterContextPdu.ALTER_CONTEXT_TYPE:
                _established = true;

                presentationContexts = ((AlterContextPdu)pdu).ContextList;
                reply = new AlterContextResponsePdu();
                result = new PresentationResult[1];
                for (var i = 0; i < presentationContexts.Length; i++) {
                    var presentationContext = presentationContexts[i];
                    var contains = false;
                    lock (_listOfInterfacesSupported) {
                        contains = _listOfInterfacesSupported.Contains(presentationContext.AbstractSyntax.ToString().ToUpper());
                    }
                    if (!contains) {
                        // create a fault PDU stating the syntax is not supported.
                        result[0] = new PresentationResult(PresentationResultCode.PROVIDER_REJECTION,
                            PresentationResultReason.ABSTRACT_SYNTAX_NOT_SUPPORTED,
                            new PresentationSyntax(UUID.NIL_UUID + ":0.0"));
                        ((AlterContextResponsePdu)reply).ResultList = result;
                        break;
                    }
                }

                // all okay
                if (((AlterContextResponsePdu)reply).ResultList == null) {
                    result[0] = new PresentationResult(); // this will be acceptance.
                    ((AlterContextResponsePdu)reply).AssociationGroupId =
                        new object().GetHashCode(); // TODO should I save this ?
                    ((AlterContextResponsePdu)reply).ResultList = result;
                }

                ((AlterContextResponsePdu)reply).CallId = pdu.CallId;
                // issue a challenge against the request info
                break;
            default:
                reply = base.Accept(reply);
                break;
        }
        return reply;
    }

    /// <summary>
    /// Update interfaces
    /// </summary>
    /// <param name="newList"></param>
    internal void UpdateListOfInterfacesSupported(List<string> newList) {
        lock (_listOfInterfacesSupported) {
            _listOfInterfacesSupported.AddRange(newList);
        }
    }

    internal void UpdateListOfInterfacesSupported2(List<string> newList) {
        lock (_listOfInterfacesSupported) {
            for (var i = 0; i < newList.Count; i++) {
                _listOfInterfacesSupported.Add(newList[i] + ":0.0");
            }
        }
    }

    private const string kIID = "IID";
    private const string kIID2 = "IID2";
    private bool _established;
#pragma warning disable IDE0052 // Remove unread private members
    private PropertyBag _properties;
#pragma warning restore IDE0052 // Remove unread private members
    private readonly List<string> _listOfInterfacesSupported = new List<string>();
}
