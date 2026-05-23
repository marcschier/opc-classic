//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

namespace SharpInterop.Transport {
    using Opc.Classic.Dcom.Internal;
    using SharpInterop.Rpc;
    using SharpInterop.Rpc.Core;
    using SharpInterop.Rpc.pdu;
    using System;

    /// <summary>
    /// Connection context
    /// </summary>
    public sealed class ComRuntimeConnectionContext : BasicConnectionContext {

        /// <inheritdoc/>
        public override bool Established => base.Established | _established;

        /// <inheritdoc/>
        public override ConnectionOrientedPdu Init(PresentationContext context,
            PropertyBag properties) {
            base.Init(context, properties);
            _properties = properties;
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
                        if (!presentationContext.AbstractSyntax.ToString().ToUpper().Equals(
                            (string)_properties.GetProperty(kIID), StringComparison.CurrentCultureIgnoreCase)) {
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
                    break;
                case AlterContextPdu.ALTER_CONTEXT_TYPE:
                    _established = true;

                    presentationContexts = ((AlterContextPdu)pdu).ContextList;
                    reply = new AlterContextResponsePdu();
                    result = new PresentationResult[1];
                    for (var i = 0; i < presentationContexts.Length; i++) {
                        var presentationContext = presentationContexts[i];
                        if (!presentationContext.AbstractSyntax.ToString().ToUpper().Equals(
                            (string)_properties.GetProperty(kIID), StringComparison.CurrentCultureIgnoreCase)) {
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
                    break;
                default:
                    reply = base.Accept(reply);
                    break;
            }
            return reply;
        }

        private const string kIID = "IID";
        private bool _established;
        private PropertyBag _properties;
    }
}