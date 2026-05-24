//
// Copyright (c) 2013 Vikram Roopchand
//
// All rights reserved. This program and the accompanying materials
// are made available under the terms of the Eclipse Public License v1.0
// which accompanies this distribution, and is available at
// http://www.eclipse.org/legal/epl-v10.html
//

using SharpInterop.Rpc.pdu;

namespace SharpInterop.Rpc; 
/// <inheritdoc/>
public class BindException : RpcException {

    /// <inheritdoc/>
    public BindException() {
    }

    /// <inheritdoc/>
    public BindException(string message) :
        base(message) {
    }

    /// <inheritdoc/>
    public BindException(string message, BindNoAcknowledgeReason rejectReason) :
        base(ToString(message, rejectReason)) {
    }

    /// <summary>
    /// Combine to string
    /// </summary>
    /// <param name="message"></param>
    /// <param name="reason"></param>
    /// <returns></returns>
    private static string ToString(string message,
        BindNoAcknowledgeReason reason) => !string.IsNullOrEmpty(message) ?
            message + " (" + ToString(reason) + ")" : ToString(reason);

    /// <summary>
    /// TODO Move to extension
    /// </summary>
    /// <param name="reason"></param>
    /// <returns></returns>
    private static string ToString(BindNoAcknowledgeReason reason) {
        switch (reason) {
            case BindNoAcknowledgeReason.REASON_NOT_SPECIFIED:
                return "REASON_NOT_SPECIFIED";
            case BindNoAcknowledgeReason.TEMPORARY_CONGESTION:
                return "TEMPORARY_CONGESTION";
            case BindNoAcknowledgeReason.LOCAL_LIMIT_EXCEEDED:
                return "LOCAL_LIMIT_EXCEEDED";
            case BindNoAcknowledgeReason.CALLED_PADDR_UNKNOWN:
                return "CALLED_PADDR_UNKNOWN";
            case BindNoAcknowledgeReason.PROTOCOL_VERSION_NOT_SUPPORTED:
                return "PROTOCOL_VERSION_NOT_SUPPORTED";
            case BindNoAcknowledgeReason.DEFAULT_CONTEXT_NOT_SUPPORTED:
                return "DEFAULT_CONTEXT_NOT_SUPPORTED";
            case BindNoAcknowledgeReason.USER_DATA_NOT_READABLE:
                return "USER_DATA_NOT_READABLE";
            case BindNoAcknowledgeReason.NO_PSAP_AVAILABLE:
                return "NO_PSAP_AVAILABLE";
            default:
                return "unknown";
        }
    }
}
